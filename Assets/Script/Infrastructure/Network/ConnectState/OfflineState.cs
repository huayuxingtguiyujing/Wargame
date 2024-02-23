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
            // �ر�����
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
            // ���õ�ǰ�����ӷ�ʽ
            connectionManager.clientReconnectingState.Configure(connectionMethodIP);
            connectionManager.clientConnectingState.Configure(connectionMethodIP);
            // ���� clientConnectingState
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
            // ���õ�ǰ�����ӷ�ʽ
            connectionManager.startHostState.Configure(connectionMethodIP);
            // ���� startHostState
            connectionManager.ChangeState(connectionManager.startHostState);
        }
    }
}