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
    /// 该类用于设置 NGO 以开始连接，作为连接方式的基类
    /// </summary>
    public abstract class ConnectionMethodBase {

        protected ConnectionManager connectionManager;

        protected readonly string playerName;

        // dtls连接方式
        protected const string dtlsConnType = "dtls";

        /// <summary>
        /// 设置主机连接
        /// </summary>
        public abstract Task SetupHostConnectionAsync();

        /// <summary>
        /// 设置客户端连接
        /// </summary>
        public abstract Task SetupClientConnectionAsync();

        /// <summary>
        /// 设置客户端以 重连
        /// </summary>
        public abstract Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync();

        // 构造函数
        protected ConnectionMethodBase(ConnectionManager connectionManager, string playerName) {
            this.connectionManager = connectionManager;
            this.playerName = playerName;
        }

        /// <summary>
        /// 设置 NetworkManager.NetworkConfig 连接配置
        /// </summary>
        protected void SetConnectionPayload(string playerId, string playerName) {
            // payload概念请自行上网查
            string payload = JsonUtility.ToJson(new ConnectionPayload() {
                playerId = playerId,
                playerName = playerName,
                isDebug = Debug.isDebugBuild
            });
            // 转为字节流
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
            // NetworkManager.NetworkConfig.ConnectionData
            // 官方文档：
            // The data to send during connection which can be used to
            // decide on if a client should get accepted
            connectionManager.networkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        /// <summary>
        /// 获得playerid 如果已经通过auth登录，返回auth的playerid，否则返回本地存储的id
        /// </summary>
        protected string GetPlayerId() {
            // 若未调用 AuthenticationService 方法登录，则返回本地存储的guid
            if (Unity.Services.Core.UnityServices.State != ServicesInitializationState.Initialized) {
                return LocalClientPrefs.GetGuid();
            }

            return AuthenticationService.Instance.IsSignedIn 
                ? AuthenticationService.Instance.PlayerId 
                : LocalClientPrefs.GetGuid();
        }

    }

    /// <summary>
    /// 基本的IP连接方式，使用unityTransport
    /// </summary>
    internal class ConnectionMethodIP : ConnectionMethodBase {

        string IpAddress;
        ushort PortNum;

        public override async Task SetupClientConnectionAsync() {
            SetConnectionPayload(GetPlayerId(), playerName);
            UnityTransport utp = (UnityTransport)connectionManager.networkManager.NetworkConfig.NetworkTransport;
            // NOTICE：封装了这么多层，唯一有效的一段代码 设置连接信息
            // 源文档：If you are only setting the IP address and port number,
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
    /// TODO: 使用unity relay connnection
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
            // 设置链接信息
            SetConnectionPayload(GetPlayerId(), playerName);

            if(lobbyService.CurrentUnityLobby == null) {
                Debug.LogError("没有设置Lobby的情况下使用relay");
                return;
            }

            // 调用 RelayService 的方法，传入joincode，获得JoinAllocation
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(
                joinCode: localLobby.RelayJoinCode    
            );

            // 刷新当前的玩家信息
            await lobbyService.UpdatePlayerRelayInfoAsync(joinAllocation.AllocationId.ToString(), localLobby.RelayJoinCode);

            // 设置NetworkManager的传输方式
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
            // 设置链接信息
            SetConnectionPayload(GetPlayerId(), playerName);

            // 创建allocation，获取其joincode
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(8, region: null);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            localLobby.RelayJoinCode = joinCode;

            // 刷新当前的lobby和player
            await lobbyService.UpdateLobbyDataAsync(localLobby.GetDataForUnityServices());
            await lobbyService.UpdatePlayerRelayInfoAsync(hostAllocation.AllocationIdBytes.ToString(), joinCode);

            // 设置NetworkManager的传输方式
            var utp = (UnityTransport)connectionManager.networkManager.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(new RelayServerData(hostAllocation, dtlsConnType));
        }
    }

}