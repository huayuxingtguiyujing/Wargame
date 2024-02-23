using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.UGS.LobbyPack;

namespace WarGame_True.Infrastructure.NetworkPackage {

    /// <summary>
    /// 所有连接状态的 基类，包含了各个方法的虚函数，在connectionManager处调用
    /// </summary>
    public abstract class ConnectionState {
        
        protected ConnectionManager connectionManager;
        public void SetConnectionManager(ConnectionManager connectionManager) {
            this.connectionManager = connectionManager;
        }

        //public void 

        public abstract void Enter();
        public abstract void Exit();

        // 客户端连接、断开时
        public virtual void OnClientConnected(ulong clientId) { }
        public virtual void OnClientDisconnect(ulong clientId) { }

        // 服务器开启、停止时
        public virtual void OnServerStarted() { }
        public virtual void OnServerStopped(bool IsHostStop) { }

        // IP 客户端 - 启动
        public virtual void StartClientIP(string playerName, string ipaddress, int port) { }

        // IP Host - 启动
        public virtual void StartHostIP(string playerName, string ipaddress, int port) { }

        // Lobby 客户端 - 启动
        public virtual void StartClientLobby(string playerName) { }

        // Lobby Host - 启动
        public virtual void StartHostLobby(string playerName) { }

// 用户断连时 - 回调
        public virtual void OnUserRequestedShutdown() { }

        // 信息传输失败时 - 回调
        public virtual void OnTransportFailure() { }

        // 连接成功时 - 回调
        public virtual void OnConnectApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) { }


    }
}