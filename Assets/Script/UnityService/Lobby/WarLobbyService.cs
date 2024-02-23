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
    /// ��װlobby���Ľӿڣ������ṩ�����Ƶķ���
    /// </summary>
    public class WarLobbyService : MonoBehaviour{

        public static WarLobbyService Instance { get; private set; }

        // �����û����ڵ�lobby
        public LocalLobby localLobby {  get; private set; }
        // �����û�
        public LocalLobbyPlayer localPlayer {  get; private set; }

        // ��ǰ���ڵ�Lobby
        public Lobby CurrentUnityLobby { get; private set; }
        public void SetCurrLobby(Lobby lobby) {
            CurrentUnityLobby = lobby;
            localLobby.ApplyRemoteData(lobby);
        }

        // ��ѯ�������Ŀ
        int defaultQueryMaxCount = 16;
        // ��������
        List<QueryOrder> defaultQueryOrder = new List<QueryOrder>() {
            new QueryOrder(asc: false, field: QueryOrder.FieldOptions.Created)
        };

        RateLimitCooldown m_RateLimitQuery;
        RateLimitCooldown m_RateLimitJoin;
        RateLimitCooldown m_RateLimitQuickJoin;
        RateLimitCooldown m_RateLimitHost;

        // ��lobby�����źţ���ʾ�÷�����Ȼ��Ծ
        // heartbeat�ź�: 5 calls per 30 seconds.
        const float heartbeatPeriod = 8; 
        float heartbeatTimeRec = 0;
        
        // ��ǰ�Ƿ�Ӧ������lobby��Ϣ
        bool shouldUpdateMes = false;

        private LobbyApiService lobbyApiService;
        private WarAuthService authService;

        public void InitWarLobbyService(WarAuthService authService) {
            lobbyApiService = new LobbyApiService();
            this.authService = authService;

            // Ӧ���ڴ˳�ʼ���� localPlayer
            localPlayer = new LocalLobbyPlayer();
            localLobby = new LocalLobby();

            //See https://docs.unity.com/lobby/rate-limits.html
            // ���е�lobby������ ��������
            m_RateLimitQuery = new RateLimitCooldown(1f);
            m_RateLimitJoin = new RateLimitCooldown(3f);
            m_RateLimitQuickJoin = new RateLimitCooldown(10f);
            m_RateLimitHost = new RateLimitCooldown(3f);

            Instance = this;
        }

        private async void Update() {

            // ����Ƿ���������ʱ�䷢�ͷ����źţ�ȷ�������Ծ
            if (localPlayer != null && localPlayer.IsHost) {
                DoLobbyHeartbeat(Time.deltaTime);
            }

            // Ӧ�ø��� Lobby��playerData
            if (shouldUpdateMes) {
                await UpdateLobbyDataAsync(localLobby.GetDataForUnityServices());
                await UpdatePlayerDataAsync(localPlayer.GetDataForLobbyService());

                shouldUpdateMes = false;
            }
        }


        #region Lobby�¼��ص�

        // TODO: lobby eventϵͳ�ƺ�������1.0.2��ʹ�ã���������汾
        // lobby event�ܸ���Ч����������ҵ�״̬�������ѯ����Ч�ʸ���

        /// <summary>
        /// ע������lobby��ػص����Թ��������player�����ڣ����롢���¡��˳�etc��
        /// </summary>
        /// /// <remarks>
        /// ��HostinState �� ClientConnectedState �е��ã�����ʱ����֮
        /// </remarks>
        public void RegisterPlayerWhenJoin() {

            // ��֪��Ϊʲôδ�ҵ�LobbyEvent����
            // �Ʋ��ǰ汾���⣡
            // Notice: Lobby events are only available in version 1.0.0 and later.
            // var lobbyEventCallbacks = new LobbyEventCallbacks();
            // lobbyEventCallbacks.LobbyChanged += OnLobbyChanges;
            // lobbyEventCallbacks.KickedFromLobby += OnKickedFromLobby;
            // lobbyEventCallbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

            shouldUpdateMes = true;
        }

        /// <summary>
        /// ȡ��ע������lobby��ػص�
        /// </summary>
        public void UnregisterPlayerWhenExit() {

            if (localPlayer != null) {
                if (localPlayer.IsHost) {
                    // �Ƿ�������ɾ������
                    DeleteLobbyAsync();
                } else {
                    // ���Ƿ��������뿪����
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
        /// ���õ�ǰ����lobby��player��Ϣ
        /// </summary>
        private void ResetLobby() {
            CurrentUnityLobby = null;
            // ˢ�µ�ǰ�û� �� ��ǰ����Lobby����Ϣ
            localPlayer.ResetPlayerData();
            localLobby?.ResetLobby(localPlayer);
        }

        #endregion


        #region Lobby������������ɾ�������롢�뿪�ȣ�

        /// <summary>
        /// ���Դ�������
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
                    Debug.Log("û��ͨ����֤!");
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
                Debug.Log("����LobbyServiceʱ����:" + e);
                m_RateLimitHost.PutOnCooldown();
            }

            return (false, null);
        }

        /// <summary>
        /// ɾ��Lobby
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
        /// ���Լ������
        /// </summary>
        /// <param name="requestId">�����ߵ�playerID��ʹ��auth�����</param>
        /// <param name="lobbyId">Ҫ����ķ���ID</param>
        /// <param name="lobbyCode">������</param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public async Task<(bool success, Lobby lobby)> TryJoinLobby(string requestId, string lobbyId, string lobbyCode) {
            if (!m_RateLimitJoin.CanCall ||
                (lobbyId == null && lobbyCode == null)) {
                Debug.LogWarning("JoinLobbyByCode/JoinLobbyById ������ȴ��.");
                return (false, null);
            }

            try {
                bool hasAuth = await authService.EnsurePlayerIsAuthorized();
                if (!hasAuth) {
                    Debug.Log("û��ͨ����֤!");
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
                Debug.Log("����Lobbyʱ����:" + e);
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
                // ���ÿ��ټ��������
                var joinRequest = new QuickJoinLobbyOptions {
                    Player = new Player(id: requestId, data: userData)
                };
                Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync( joinRequest);
                return (true, lobby);
            } catch (LobbyServiceException e) {
                Debug.Log("���ټ���Lobbyʱ����:" + e);
                m_RateLimitQuickJoin.PutOnCooldown();
            }

            return (false, null);
        }

        public async Task<Lobby> ReconnectToLobby() {
            try {
                return await lobbyApiService.ReconnectToLobby(localLobby.LobbyID);
            } catch (LobbyServiceException e) {
                Debug.LogError("����ʧ��!" + e);
            }

            return null;
        }

        public async void DeleteLobbyAsync() {
            if (localPlayer.IsHost) {
                try {
                    await lobbyApiService.DeleteLobby(localLobby.LobbyID);
                    ResetLobby();
                }catch(LobbyServiceException e) {
                    Debug.Log("ɾ������ʱ��������:" + e);
                }
            } else {
                Debug.Log("��ǰ�û�û��Ȩ��ɾ������");
            }
        }

        /// <summary>
        /// �����뿪Lobby
        /// </summary>
        public async void LeaveLobbyAsync(string playerId) {
            try {
                await lobbyApiService.LeaveLobby(playerId, localLobby.LobbyID);
                ResetLobby();
            } catch (LobbyServiceException e) {
                // Lobby û���ҵ��ұ����û�����host, ��ô�÷����Ѿ���ɾ��.
                if (e.Reason != LobbyExceptionReason.LobbyNotFound && !localPlayer.IsHost) {
                    return;
                }
                Debug.LogError("�뿪����ʱ��������:" + e.Reason +",");
            }

        }

        /// <summary>
        /// ����
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
                Debug.LogWarning("QueryAllLobby ������ȴ�У����Ժ�");
                return null;
            }

            try {
                bool hasAuth = await authService.EnsurePlayerIsAuthorized();
                if (!hasAuth) {
                    Debug.Log("û��ͨ����֤!");
                    return null;
                }

                QueryLobbiesOptions options = new QueryLobbiesOptions();
                options.Count = defaultQueryMaxCount;
                options.Order = defaultQueryOrder;

                QueryResponse queryResult = await lobbyApiService.QueryAllLobbies(options);
                return queryResult;
            } catch (LobbyServiceException e) {
                Debug.Log("��ѯ����ʱ����:" + e);
                m_RateLimitQuery.PutOnCooldown();
                return null;
            }
        }

        #endregion

        #region Lobby״̬�������·��䡢�����Ϣ��HeartBreak��

        public async Task UpdatePlayerDataAsync(Dictionary<string, PlayerDataObject> data) {
            if (!m_RateLimitQuery.CanCall) {
                return;
            }

            try {
                // TODO: playerID��ô���紫�룬��Ҫֱ��ʹ��Authentic����ĵ���player���������
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
        /// ������ҵ�relay������Ϣ�������ͨ��relay��ʽ������Ϸ��
        /// </summary>
        public async Task UpdatePlayerRelayInfoAsync(string allocationId, string connectionInfo) {
            if (!m_RateLimitQuery.CanCall) {
                Debug.LogWarning("UpdatePlayer ������ȴ�У����Ժ�");
                return;
            }

            try {
                await lobbyApiService.UpdatePlayer(CurrentUnityLobby.Id, AuthenticationService.Instance.PlayerId, new Dictionary<string, PlayerDataObject>(), allocationId, connectionInfo);
            } catch (LobbyServiceException e) {
                Debug.LogError("���������Ϣʧ��!" + e);
                m_RateLimitQuery.PutOnCooldown();
            }
        }

        /// <summary>
        /// ����Lobby��������Ϣ���ײ��ǵ���LobbyService.UpdateLobbyAsync��
        /// </summary>
        public async Task UpdateLobbyDataAsync(Dictionary<string, DataObject> data) {
            if (!m_RateLimitQuery.CanCall) {
                Debug.LogWarning("UpdateLobby ������ȴ�У����Ժ�");
                return;
            }

            // ��ȡ��ǰ���ӵ���lobby��data
            var dataCurr = CurrentUnityLobby.Data ?? new Dictionary<string, DataObject>();
            // �������data���µ���ǰ������
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
                // ��ȡ��ע�ⲻҪ����SetCurLobby����ˢ������
                var result = await lobbyApiService.UpdateLobby(CurrentUnityLobby.Id, dataCurr, shouldLock);
                if (result != null) {
                    CurrentUnityLobby = result;
                }
            } catch (LobbyServiceException e) {
                Debug.LogError("����lobby��Ϣʧ��!" + e);
                m_RateLimitQuery.PutOnCooldown();
            }
        }

        /// <summary>
        /// Lobby��Ҫ�����Եػ�ȡ����ķ���, ���򱾷�����Ϊ����.
        /// </summary>
        public void DoLobbyHeartbeat(float deltaTime) {
            heartbeatTimeRec += deltaTime;
            // ����Ƿ񵽴�ķ���ping��ʱ��
            if (heartbeatTimeRec > heartbeatPeriod) {
                heartbeatTimeRec -= heartbeatPeriod;
                try {
                    lobbyApiService.SendHeartbeatPing(CurrentUnityLobby.Id);
                } catch (LobbyServiceException e) {
                    //LobbyNotFound�� Lobby has already been deleted.
                    if (e.Reason != LobbyExceptionReason.LobbyNotFound && !localPlayer.IsHost) {
                        Debug.LogError(e);
                    }
                }
            }
        }

        #endregion

    }

}