using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WarGame_True.Infrastructure.Auth;
using WarGame_True.Infrastructure.SceneUtil;
using WarGame_True.UGS.LobbyPack;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public class OfflineState : ConnectionState {

        public override void Enter() {
            // 关闭连接
            connectionManager.networkManager.Shutdown();
            
        }

        public override void Exit() {
            
        }

        public override void StartClientLobby(string playerName) {
            ConnectionMethodRelay connectionMethod = new ConnectionMethodRelay(
                WarLobbyService.Instance, WarLobbyService.Instance.localLobby, 
                connectionManager, playerName);
            connectionManager.clientReconnectingState.Configure(connectionMethod);
            connectionManager.clientConnectingState.Configure(connectionMethod);
            connectionManager.ChangeState(connectionManager.clientConnectingState);
        }

        public override void StartClientIP(string playerName, string ipaddress, int port) {
            ConnectionMethodIP connectionMethodIP = new ConnectionMethodIP(
                connectionManager,
                playerName, ipaddress, (ushort)port
            );
            // 设置当前的连接方式
            connectionManager.clientReconnectingState.Configure(connectionMethodIP);
            connectionManager.clientConnectingState.Configure(connectionMethodIP);
            // 进入 clientConnectingState
            connectionManager.ChangeState(connectionManager.clientConnectingState);

        }

        public override void StartHostLobby(string playerName) {
            ConnectionMethodRelay connectionMethod = new ConnectionMethodRelay(
               WarLobbyService.Instance, WarLobbyService.Instance.localLobby,
               connectionManager, playerName);
            connectionManager.startHostState.Configure(connectionMethod);
            connectionManager.ChangeState(connectionManager.startHostState);
        }

        public override void StartHostIP(string playerName, string ipaddress, int port) {
            ConnectionMethodIP connectionMethodIP = new ConnectionMethodIP(
                connectionManager,
                playerName, ipaddress, (ushort)port
            );
            // 设置当前的连接方式
            connectionManager.startHostState.Configure(connectionMethodIP);
            // 进入 startHostState
            connectionManager.ChangeState(connectionManager.startHostState);
        }
    }
}