//using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using WarGame_True.Infrastructure.Auth;

namespace WarGame_True.UGS.LobbyPack {
    /// <summary>
    /// 封装lobby包的接口，向外提供更完善的服务
    /// </summary>
    public class WarLobbyService : MonoBehaviour{

        public static WarLobbyService Instance { get; private set; }

        // 本机用户所在的lobby
        public LocalLobby localLobby {  get; private set; }
        // 本机用户
        public LocalLobbyPlayer localPlayer {  get; private set; }

        // 当前所在的Lobby
        public Lobby CurrentUnityLobby { get; private set; }
        public void SetCurrLobby(Lobby lobby) {
            CurrentUnityLobby = lobby;
            localLobby.ApplyRemoteData(lobby);
        }

        // 查询的最大数目
        int defaultQueryMaxCount = 16;
        // 排序设置
        List<QueryOrder> defaultQueryOrder = new List<QueryOrder>() {
            new QueryOrder(asc: false, field: QueryOrder.FieldOptions.Created)
        };

        RateLimitCooldown m_RateLimitQuery;
        RateLimitCooldown m_RateLimitJoin;
        RateLimitCooldown m_RateLimitQuickJoin;
        RateLimitCooldown m_RateLimitHost;

        // 向lobby发送信号，表示该房间依然活跃
        // heartbeat信号: 5 calls per 30 seconds.
        const float heartbeatPeriod = 8; 
        float heartbeatTimeRec = 0;
        
        // 当前是否应当更新lobby信息
        bool shouldUpdateMes = false;

        private LobbyApiService lobbyApiService;
        private WarAuthService authService;

        public void InitWarLobbyService(WarAuthService authService) {
            lobbyApiService = new LobbyApiService();
            this.authService = authService;

            // 应该在此初始化下 localPlayer
            localPlayer = new LocalLobbyPlayer();
            localLobby = new LocalLobby();

            //See https://docs.unity.com/lobby/rate-limits.html
            // 所有的lobby请求都有 速率限制
            m_RateLimitQuery = new RateLimitCooldown(1f);
            m_RateLimitJoin = new RateLimitCooldown(3f);
            m_RateLimitQuickJoin = new RateLimitCooldown(10f);
            m_RateLimitHost = new RateLimitCooldown(3f);

            Instance = this;
        }

        private async void Update() {

            // 如果是房主，则随时间发送房间信号，确保房间活跃
            if (localPlayer != null && localPlayer.IsHost) {
                DoLobbyHeartbeat(Time.deltaTime);
            }

            // 应该更新 Lobby和playerData
            if (shouldUpdateMes) {
                await UpdateLobbyDataAsync(localLobby.GetDataForUnityServices());
                await UpdatePlayerDataAsync(localPlayer.GetDataForLobbyService());

                shouldUpdateMes = false;
            }
        }


        #region Lobby事件回调

        // TODO: lobby event系统似乎不能在1.0.2中使用，建议更换版本
        // lobby event能更有效管理房间与玩家的状态，相比轮询更新效率更优

        /// <summary>
        /// 注册所有lobby相关回调，以管理房间里的player的周期（加入、更新、退出etc）
        /// </summary>
        /// /// <remarks>
        /// 在HostinState 和 ClientConnectedState 中调用，连接时发送之
        /// </remarks>
        public void RegisterPlayerWhenJoin() {

            // 不知道为什么未找到LobbyEvent类型
            // 推测是版本问题！
            // Notice: Lobby events are only available in version 1.0.0 and later.
            // var lobbyEventCallbacks = new LobbyEventCallbacks();
            // lobbyEventCallbacks.LobbyChanged += OnLobbyChanges;
            // lobbyEventCallbacks.KickedFromLobby += OnKickedFromLobby;
            // lobbyEventCallbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

            shouldUpdateMes = true;
        }

        /// <summary>
        /// 取消注册所有lobby相关回调
        /// </summary>
        public void UnregisterPlayerWhenExit() {

            if (localPlayer != null) {
                if (localPlayer.IsHost) {
                    // 是房主，则删掉房间
                    DeleteLobbyAsync();
                } else {
                    // 不是房主，则离开房间
                    string playerID = AuthenticationService.Instance.PlayerId;
                    LeaveLobbyAsync(playerID);
                }
            }

        }

        private void OnLobbyEventConnectionStateChanged() {

        }

        private void OnKickedFromLobby() {
            Debug.Log("Kicked from Lobby");
            ResetLobby();
        }

        private void OnLobbyChanges() {

        }

        /// <summary>
        /// 重置当前所在lobby和player信息
        /// </summary>
        private void ResetLobby() {
            CurrentUnityLobby = null;
            // 刷新当前用户 和 当前所在Lobby的信息
            localPlayer.ResetPlayerData();
            localLobby?.ResetLobby(localPlayer);
        }

        #endregion


        #region Lobby操作（创建、删除、加入、离开等）

        /// <summary>
        /// 尝试创建大厅
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="lobbyName"></param>
        /// <param name="maxPlayers"></param>
        /// <param name="IsPrivate"></param>
        /// <param name="playerDataValue"></param>
        /// <param name="lobbyDataValue"></param>
        /// <returns></returns>
        public async Task<(bool success, Lobby lobby)> TryCreateLobby(string requestId, string lobbyName, bool IsPrivate) {
            if (!m_RateLimitHost.CanCall) {
                Debug.LogWarning("Create Lobby hit the rate limit.");
                return (false, null);
            }

            try {
                bool hasAuth = await authService.EnsurePlayerIsAuthorized();
                if(!hasAuth) {
                    Debug.Log("没有通过认证!");
                    return (false, null);
                }

                //Debug.Log(lobbyApiService == null);
                //Debug.Log(localPlayer == null);
                Lobby lobby = await lobbyApiService.CreateLobby(
                   requestId: requestId, lobbyName: lobbyName, 
                   maxPlayers: 8, IsPrivate: IsPrivate, 
                   localPlayer.GetDataForLobbyService(), null
                );
                return (true, lobby);
            } catch (LobbyServiceException e) {
                Debug.Log("调用LobbyService时出错:" + e);
                m_RateLimitHost.PutOnCooldown();
            }

            return (false, null);
        }

        /// <summary>
        /// 删除Lobby
        /// </summary>
        public async void TryDeleteLobby() {
            if (localPlayer.IsHost) {
                try {
                    await lobbyApiService.DeleteLobby(localLobby.LobbyID);
                    ResetLobby();
                } catch (LobbyServiceException e) {
                    Debug.LogError(e);
                }
            } else {
                Debug.LogError("Only the host can delete a lobby.");
            }
        }

        /// <summary>
        /// 尝试加入大厅
        /// </summary>
        /// <param name="requestId">请求者的playerID，使用auth包获得</param>
        /// <param name="lobbyId">要加入的房间ID</param>
        /// <param name="lobbyCode">邀请码</param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public async Task<(bool success, Lobby lobby)> TryJoinLobby(string requestId, string lobbyId, string lobbyCode) {
            if (!m_RateLimitJoin.CanCall ||
                (lobbyId == null && lobbyCode == null)) {
                Debug.LogWarning("JoinLobbyByCode/JoinLobbyById 正在冷却中.");
                return (false, null);
            }

            try {
                bool hasAuth = await authService.EnsurePlayerIsAuthorized();
                if (!hasAuth) {
                    Debug.Log("没有通过认证!");
                    return (false, null);
                }

                if (!string.IsNullOrEmpty(lobbyCode)) {
                    Lobby lobby = await lobbyApiService.JoinLobbyByCode(requestId, lobbyCode, localPlayer.GetDataForLobbyService());
                    return (true, lobby);
                } else {
                    Lobby lobby = await lobbyApiService.JoinLobbyById(requestId, lobbyId, localPlayer.GetDataForLobbyService());
                    return (true, lobby);
                }
            } catch (LobbyServiceException e) {
                Debug.Log("加入Lobby时出错:" + e);
                m_RateLimitJoin.PutOnCooldown();
            }

            return (false, null);
        }

        public async Task<(bool Success, Lobby Lobby)> TryQuickJoinLobby(string requestId, Dictionary<string, PlayerDataObject> userData) {
            if (!m_RateLimitQuickJoin.CanCall) {
                Debug.LogWarning("Quick Join Lobby hit the rate limit.");
                return (false, null);
            }

            try {
                // 设置快速加入的配置
                var joinRequest = new QuickJoinLobbyOptions {
                    Player = new Player(id: requestId, data: userData)
                };
                Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync( joinRequest);
                return (true, lobby);
            } catch (LobbyServiceException e) {
                Debug.Log("快速加入Lobby时出错:" + e);
                m_RateLimitQuickJoin.PutOnCooldown();
            }

            return (false, null);
        }

        public async Task<Lobby> ReconnectToLobby() {
            try {
                return await lobbyApiService.ReconnectToLobby(localLobby.LobbyID);
            } catch (LobbyServiceException e) {
                Debug.LogError("重连失败!" + e);
            }

            return null;
        }

        public async void DeleteLobbyAsync() {
            if (localPlayer.IsHost) {
                try {
                    await lobbyApiService.DeleteLobby(localLobby.LobbyID);
                    ResetLobby();
                }catch(LobbyServiceException e) {
                    Debug.Log("删除房间时出现问题:" + e);
                }
            } else {
                Debug.Log("当前用户没有权限删除房间");
            }
        }

        /// <summary>
        /// 尝试离开Lobby
        /// </summary>
        public async void LeaveLobbyAsync(string playerId) {
            try {
                await lobbyApiService.LeaveLobby(playerId, localLobby.LobbyID);
                ResetLobby();
            } catch (LobbyServiceException e) {
                // Lobby 没有找到且本地用户不是host, 那么该房间已经被删除.
                if (e.Reason != LobbyExceptionReason.LobbyNotFound && !localPlayer.IsHost) {
                    return;
                }
                Debug.LogError("离开房间时出现问题:" + e.Reason +",");
            }

        }

        /// <summary>
        /// 踢人
        /// </summary>
        public async Task RemovePlayerFromLobby(string playerId, string lobbyId) {
            if (localPlayer.IsHost) {
                try {
                    await lobbyApiService.RemovePlayerLobby(playerId, lobbyId);
                } catch (LobbyServiceException e) {
                    Debug.LogError(e);
                }
            } else {
                Debug.LogError("Only the host can remove other players from the lobby.");
            }
        }

        public async Task<QueryResponse> QueryAllLobby() {
            if (!m_RateLimitQuery.CanCall) {
                Debug.LogWarning("QueryAllLobby 正在冷却中，请稍后");
                return null;
            }

            try {
                bool hasAuth = await authService.EnsurePlayerIsAuthorized();
                if (!hasAuth) {
                    Debug.Log("没有通过认证!");
                    return null;
                }

                QueryLobbiesOptions options = new QueryLobbiesOptions();
                options.Count = defaultQueryMaxCount;
                options.Order = defaultQueryOrder;

                QueryResponse queryResult = await lobbyApiService.QueryAllLobbies(options);
                return queryResult;
            } catch (LobbyServiceException e) {
                Debug.Log("查询大厅时出错:" + e);
                m_RateLimitQuery.PutOnCooldown();
                return null;
            }
        }

        #endregion

        #region Lobby状态管理（更新房间、玩家信息、HeartBreak）

        public async Task UpdatePlayerDataAsync(Dictionary<string, PlayerDataObject> data) {
            if (!m_RateLimitQuery.CanCall) {
                return;
            }

            try {
                // TODO: playerID最好从外界传入，不要直接使用Authentic服务的单例player，更易耦合
                var result = await lobbyApiService.UpdatePlayer(CurrentUnityLobby.Id, AuthenticationService.Instance.PlayerId, data, null, null);

                if (result != null) {
                    CurrentUnityLobby = result; // Store the most up-to-date lobby now since we have it, instead of waiting for the next heartbeat.
                }
            } catch (LobbyServiceException e) {
                if (e.Reason == LobbyExceptionReason.RateLimited) {
                    m_RateLimitQuery.PutOnCooldown();
                }
            }
        }

        /// <summary>
        /// 更新玩家的relay配置信息，如果是通过relay方式进入游戏的
        /// </summary>
        public async Task UpdatePlayerRelayInfoAsync(string allocationId, string connectionInfo) {
            if (!m_RateLimitQuery.CanCall) {
                Debug.LogWarning("UpdatePlayer 正在冷却中，请稍后");
                return;
            }

            try {
                await lobbyApiService.UpdatePlayer(CurrentUnityLobby.Id, AuthenticationService.Instance.PlayerId, new Dictionary<string, PlayerDataObject>(), allocationId, connectionInfo);
            } catch (LobbyServiceException e) {
                Debug.LogError("更新玩家信息失败!" + e);
                m_RateLimitQuery.PutOnCooldown();
            }
        }

        /// <summary>
        /// 更新Lobby的配置信息（底层是调用LobbyService.UpdateLobbyAsync）
        /// </summary>
        public async Task UpdateLobbyDataAsync(Dictionary<string, DataObject> data) {
            if (!m_RateLimitQuery.CanCall) {
                Debug.LogWarning("UpdateLobby 正在冷却中，请稍后");
                return;
            }

            // 获取当前连接到的lobby的data
            var dataCurr = CurrentUnityLobby.Data ?? new Dictionary<string, DataObject>();
            // 将传入的data更新到当前数据中
            foreach (var dataNew in data) {
                if (dataCurr.ContainsKey(dataNew.Key)) {
                    dataCurr[dataNew.Key] = dataNew.Value;
                } else {
                    dataCurr.Add(dataNew.Key, dataNew.Value);
                }
            }

            //we would want to lock lobbies from appearing in queries
            //if we're in relay mode and the relay isn't fully set up yet
            var shouldLock = string.IsNullOrEmpty(localLobby.RelayJoinCode);

            try {
                // 获取，注意不要调用SetCurLobby，会刷新数据
                var result = await lobbyApiService.UpdateLobby(CurrentUnityLobby.Id, dataCurr, shouldLock);
                if (result != null) {
                    CurrentUnityLobby = result;
                }
            } catch (LobbyServiceException e) {
                Debug.LogError("更新lobby信息失败!" + e);
                m_RateLimitQuery.PutOnCooldown();
            }
        }

        /// <summary>
        /// Lobby需要周期性地获取房间的反馈, 否则本房间会成为死房.
        /// </summary>
        public void DoLobbyHeartbeat(float deltaTime) {
            heartbeatTimeRec += deltaTime;
            // 检测是否到达的发送ping的时候
            if (heartbeatTimeRec > heartbeatPeriod) {
                heartbeatTimeRec -= heartbeatPeriod;
                try {
                    lobbyApiService.SendHeartbeatPing(CurrentUnityLobby.Id);
                } catch (LobbyServiceException e) {
                    //LobbyNotFound： Lobby has already been deleted.
                    if (e.Reason != LobbyExceptionReason.LobbyNotFound && !localPlayer.IsHost) {
                        Debug.LogError(e);
                    }
                }
            }
        }

        #endregion

    }

}