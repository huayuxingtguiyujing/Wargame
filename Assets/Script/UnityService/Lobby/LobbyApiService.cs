using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using static Unity.Services.Lobbies.Models.QueryFilter;

namespace WarGame_True.UGS.LobbyPack {
    /// <summary>
    /// ��LobbyService��ԭ��API���з�װ
    /// </summary>
    public class LobbyApiService {

        #region ��װLobbyService�Ķ���ӿ�

        /// <summary>
        /// ��������-��װLobbyService��CreateLobbyAsync
        /// </summary>
        /// <param name="lobbyName">Ҫ�����Ĵ�������</param>
        /// <param name="maxPlayers">��������Ŀ</param>
        /// <param name="IsPrivate">�Ƿ񹫿�</param>
        /// <returns></returns>
        internal async Task<Lobby> CreateLobby(string requestId, string lobbyName, int maxPlayers, bool IsPrivate, Dictionary<string, PlayerDataObject> playerData, Dictionary<string, DataObject> lobbyData) {

            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = IsPrivate;
            // ���ô����ߵ���Ϣ
            // AuthenticationService.Instance.PlayerId
            options.Player = new Player(
                id: requestId,
                data: playerData);

            // ���ö���ѡ����Ϣ
            options.Data = lobbyData;

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            return lobby;
        }

        /// <summary>
        /// ɾ������-��װLobbyService��DeleteLobbyAsync
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <returns></returns>
        internal async Task<bool> DeleteLobby(string lobbyId) {
            try {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
                return true;
            } catch (LobbyServiceException e) {
                Debug.Log("ɾ������ʱ����:" + e);
                return false;
            }
        }

        /// <summary>
        /// ��ѯ���п��õĴ���
        /// </summary>
        internal async Task<QueryResponse> QueryAllLobbies(QueryLobbiesOptions options) {
            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            return lobbies;
        }

        /// <summary>
        /// ����������ѯ���õĴ���
        /// </summary>
        internal async Task<(bool, QueryResponse)> QueryLobbies(QueryFilter.FieldOptions field, string value, OpOptions op, QueryOrder.FieldOptions sortOption, bool sortByAsc = false) {
            try {
                QueryLobbiesOptions options = new QueryLobbiesOptions();
                options.Count = 25;

                // �Խ�������������´���������
                //  = QueryOrder.FieldOptions.Created
                options.Order = new List<QueryOrder>() {
                    new QueryOrder( asc: sortByAsc, field: sortOption)
                };

                // �趨��������
                options.Filters = new List<QueryFilter>() {
                    new QueryFilter( field: field, op: op, value: value)
                };

                QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
                return (true, lobbies);
            } catch (LobbyServiceException e) {
                Debug.Log("��ѯ����ʱ����:" + e);
                return (false, null);
            }
        }

        /// <summary>
        /// ͨ�����Id �������
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
        /// ͨ�������� �������
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
        /// ��ȡ��ǰ�Ѿ�����Ĵ���
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

        // NOTICE: �ƺ���ǰ��lobby�汾��֧��lobby�¼�
        //internal async Task<ILobbyEvents> SubscribeToLobby(string lobbyId, LobbyEventCallbacks eventCallbacks) {
        //    return await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, eventCallbacks);
        //}

        /// <summary>
        /// ���µ�ǰ���ڵ�lobby
        /// </summary>
        internal async Task<Lobby> UpdateLobby(string lobbyId, Dictionary<string, DataObject> data, bool shouldLock) {
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions { Data = data, IsLocked = shouldLock };
            return await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateOptions);
        }

        /// <summary>
        /// ���µ�ǰ��player��Ϣ
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
        /// ��������������ȷ���Լ��Ļ��
        /// </summary>
        /// <param name="lobbyId"></param>
        internal async void SendHeartbeatPing(string lobbyId) {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
        }

    }
}