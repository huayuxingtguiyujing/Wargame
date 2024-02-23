using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using WarGame_True.UGS.LobbyPack;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using NUnit.Framework;

namespace WarGame_True.Infrastructure.NetworkPackage {

    /// <summary>
    /// ������������ NGO �Կ�ʼ���ӣ���Ϊ���ӷ�ʽ�Ļ���
    /// </summary>
    public abstract class ConnectionMethodBase {

        protected ConnectionManager connectionManager;

        protected readonly string playerName;

        // dtls���ӷ�ʽ
        protected const string dtlsConnType = "dtls";

        /// <summary>
        /// ������������
        /// </summary>
        public abstract Task SetupHostConnectionAsync();

        /// <summary>
        /// ���ÿͻ�������
        /// </summary>
        public abstract Task SetupClientConnectionAsync();

        /// <summary>
        /// ���ÿͻ����� ����
        /// </summary>
        public abstract Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync();

        // ���캯��
        protected ConnectionMethodBase(ConnectionManager connectionManager, string playerName) {
            this.connectionManager = connectionManager;
            this.playerName = playerName;
        }

        /// <summary>
        /// ���� NetworkManager.NetworkConfig ��������
        /// </summary>
        protected void SetConnectionPayload(string playerId, string playerName) {
            // payload����������������
            string payload = JsonUtility.ToJson(new ConnectionPayload() {
                playerId = playerId,
                playerName = playerName,
                isDebug = Debug.isDebugBuild
            });
            // תΪ�ֽ���
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
            // NetworkManager.NetworkConfig.ConnectionData
            // �ٷ��ĵ���
            // The data to send during connection which can be used to
            // decide on if a client should get accepted
            connectionManager.networkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        /// <summary>
        /// ���playerid ����Ѿ�ͨ��auth��¼������auth��playerid�����򷵻ر��ش洢��id
        /// </summary>
        protected string GetPlayerId() {
            // ��δ���� AuthenticationService ������¼���򷵻ر��ش洢��guid
            if (Unity.Services.Core.UnityServices.State != ServicesInitializationState.Initialized) {
                return LocalClientPrefs.GetGuid();
            }

            return AuthenticationService.Instance.IsSignedIn 
                ? AuthenticationService.Instance.PlayerId 
                : LocalClientPrefs.GetGuid();
        }

    }

    /// <summary>
    /// ������IP���ӷ�ʽ��ʹ��unityTransport
    /// </summary>
    internal class ConnectionMethodIP : ConnectionMethodBase {

        string IpAddress;
        ushort PortNum;

        public override async Task SetupClientConnectionAsync() {
            SetConnectionPayload(GetPlayerId(), playerName);
            UnityTransport utp = (UnityTransport)connectionManager.networkManager.NetworkConfig.NetworkTransport;
            // NOTICE����װ����ô��㣬Ψһ��Ч��һ�δ��� ����������Ϣ
            // Դ�ĵ���If you are only setting the IP address and port number,
            // then you can use the UnityTransport.SetConnectionData method
            utp.SetConnectionData(IpAddress, PortNum);
        }

        public override async Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync() {
            return (true, true);
        }

        public override async Task SetupHostConnectionAsync() {
            await SetupClientConnectionAsync();
        }

        public ConnectionMethodIP(ConnectionManager CM, string playerName, string ipAddress, ushort portNum)
            : base(CM, playerName){
            IpAddress = ipAddress;
            PortNum = portNum;
        }
    }

    /// <summary>
    /// TODO: ʹ��unity relay connnection
    /// </summary>
    class ConnectionMethodRelay : ConnectionMethodBase {

        WarLobbyService lobbyService;
        LocalLobby localLobby;

        public ConnectionMethodRelay(WarLobbyService lobbyService, LocalLobby localLobby, ConnectionManager connectionManager, string playerName)
            : base(connectionManager, playerName) {
            this.lobbyService = lobbyService;
            this.localLobby = localLobby;
        }

        public override async Task SetupClientConnectionAsync() {
            // ����������Ϣ
            SetConnectionPayload(GetPlayerId(), playerName);

            if(lobbyService.CurrentUnityLobby == null) {
                Debug.LogError("û������Lobby�������ʹ��relay");
                return;
            }

            // ���� RelayService �ķ���������joincode�����JoinAllocation
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(
                joinCode: localLobby.RelayJoinCode    
            );

            // ˢ�µ�ǰ�������Ϣ
            await lobbyService.UpdatePlayerRelayInfoAsync(joinAllocation.AllocationId.ToString(), localLobby.RelayJoinCode);

            // ����NetworkManager�Ĵ��䷽ʽ
            var utp = (UnityTransport)connectionManager.networkManager.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(new RelayServerData(joinAllocation, dtlsConnType));
        }

        public override async Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync() {
            if (lobbyService.CurrentUnityLobby == null) {
                return (false, false);
            }

            Lobby lobby = await lobbyService.ReconnectToLobby();
            if(lobby != null) {
                return (true, false);
            } else {
                return (false, true);
            }
        }

        public override async Task SetupHostConnectionAsync() {
            // ����������Ϣ
            SetConnectionPayload(GetPlayerId(), playerName);

            // ����allocation����ȡ��joincode
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(8, region: null);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            localLobby.RelayJoinCode = joinCode;

            // ˢ�µ�ǰ��lobby��player
            await lobbyService.UpdateLobbyDataAsync(localLobby.GetDataForUnityServices());
            await lobbyService.UpdatePlayerRelayInfoAsync(hostAllocation.AllocationIdBytes.ToString(), joinCode);

            // ����NetworkManager�Ĵ��䷽ʽ
            var utp = (UnityTransport)connectionManager.networkManager.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(new RelayServerData(hostAllocation, dtlsConnType));
        }
    }

}