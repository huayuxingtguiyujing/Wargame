using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using static Unity.Services.Lobbies.Models.QueryFilter;

namespace WarGame_True.UGS.LobbyPack {
    /// <summary>
    /// 对LobbyService的原生API进行封装
    /// </summary>
    public class LobbyApiService {

        #region 封装LobbyService的对外接口

        /// <summary>
        /// 创建大厅-封装LobbyService的CreateLobbyAsync
        /// </summary>
        /// <param name="lobbyName">要创建的大厅名称</param>
        /// <param name="maxPlayers">最多玩家数目</param>
        /// <param name="IsPrivate">是否公开</param>
        /// <returns></returns>
        internal async Task<Lobby> CreateLobby(string requestId, string lobbyName, int maxPlayers, bool IsPrivate, Dictionary<string, PlayerDataObject> playerData, Dictionary<string, DataObject> lobbyData) {

            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = IsPrivate;
            // 设置创建者的信息
            // AuthenticationService.Instance.PlayerId
            options.Player = new Player(
                id: requestId,
                data: playerData);

            // 设置额外选项信息
            options.Data = lobbyData;

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            return lobby;
        }

        /// <summary>
        /// 删除大厅-封装LobbyService的DeleteLobbyAsync
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <returns></returns>
        internal async Task<bool> DeleteLobby(string lobbyId) {
            try {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
                return true;
            } catch (LobbyServiceException e) {
                Debug.Log("删除大厅时出错:" + e);
                return false;
            }
        }

        /// <summary>
        /// 查询所有可用的大厅
        /// </summary>
        internal async Task<QueryResponse> QueryAllLobbies(QueryLobbiesOptions options) {
            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            return lobbies;
        }

        /// <summary>
        /// 根据条件查询可用的大厅
        /// </summary>
        internal async Task<(bool, QueryResponse)> QueryLobbies(QueryFilter.FieldOptions field, string value, OpOptions op, QueryOrder.FieldOptions sortOption, bool sortByAsc = false) {
            try {
                QueryLobbiesOptions options = new QueryLobbiesOptions();
                options.Count = 25;

                // 对结果进行排序，最新创建的优先
                //  = QueryOrder.FieldOptions.Created
                options.Order = new List<QueryOrder>() {
                    new QueryOrder( asc: sortByAsc, field: sortOption)
                };

                // 设定过滤条件
                options.Filters = new List<QueryFilter>() {
                    new QueryFilter( field: field, op: op, value: value)
                };

                QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
                return (true, lobbies);
            } catch (LobbyServiceException e) {
                Debug.Log("查询大厅时出错:" + e);
                return (false, null);
            }
        }

        /// <summary>
        /// 通过玩家Id 加入大厅
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <returns></returns>
        internal async Task<Lobby> JoinLobbyById(string requestUserId, string lobbyId, Dictionary<string, PlayerDataObject> userData) {
            // Join by ID:
            var idOptions = new JoinLobbyByIdOptions {
                Player = new Player(
                id: requestUserId, data: userData
            )
            };
            return await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, idOptions);
        }

        /// <summary>
        /// 通过邀请码 加入大厅
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <returns></returns>
        internal async Task<Lobby> JoinLobbyByCode(string requestUserId, string lobbyCode, Dictionary<string, PlayerDataObject> userData) {
            // Join by Code:
            var codeOptions = new JoinLobbyByCodeOptions {
                Player = new Player(
                id: requestUserId, data: userData
            )
            };
            return await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, codeOptions);
        }

        internal async Task LeaveLobby(string requestUserId, string lobbyId) {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, requestUserId);
        }

        internal async Task RemovePlayerLobby(string removeTargetId, string lobbyId) {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, removeTargetId);
        }

        /// <summary>
        /// 获取当前已经加入的大厅
        /// </summary>
        /// <returns></returns>
        internal async Task<List<string>> GetJoinedLobby() {
            try {
                List<string> lobbyIds = await LobbyService.Instance.GetJoinedLobbiesAsync();
                return lobbyIds;
            } catch (LobbyServiceException e) {
                Debug.Log(e);
                return null;
            }
        }

        internal async Task<Lobby> ReconnectToLobby(string lobbyId) {
            return await LobbyService.Instance.ReconnectToLobbyAsync(lobbyId);
        }

        #endregion

        // NOTICE: 似乎当前的lobby版本不支持lobby事件
        //internal async Task<ILobbyEvents> SubscribeToLobby(string lobbyId, LobbyEventCallbacks eventCallbacks) {
        //    return await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, eventCallbacks);
        //}

        /// <summary>
        /// 更新当前所在的lobby
        /// </summary>
        internal async Task<Lobby> UpdateLobby(string lobbyId, Dictionary<string, DataObject> data, bool shouldLock) {
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions { Data = data, IsLocked = shouldLock };
            return await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateOptions);
        }

        /// <summary>
        /// 更新当前的player信息
        /// </summary>
        internal async Task<Lobby> UpdatePlayer(string lobbyId, string playerId, Dictionary<string, PlayerDataObject> data, string allocationId, string connectionInfo) {
            UpdatePlayerOptions updateOptions = new UpdatePlayerOptions {
                Data = data,
                AllocationId = allocationId,
                ConnectionInfo = connectionInfo
            };
            return await LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, updateOptions);
        }

        /// <summary>
        /// 发送心跳反馈，确保自己的活动性
        /// </summary>
        /// <param name="lobbyId"></param>
        internal async void SendHeartbeatPing(string lobbyId) {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
        }

    }
}