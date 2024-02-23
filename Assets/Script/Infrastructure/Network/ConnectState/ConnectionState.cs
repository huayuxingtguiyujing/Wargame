using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.UGS.LobbyPack;

namespace WarGame_True.Infrastructure.NetworkPackage {

    /// <summary>
    /// ��������״̬�� ���࣬�����˸����������麯������connectionManager������
    /// </summary>
    public abstract class ConnectionState {
        
        protected ConnectionManager connectionManager;
        public void SetConnectionManager(ConnectionManager connectionManager) {
            this.connectionManager = connectionManager;
        }

        //public void 

        public abstract void Enter();
        public abstract void Exit();

        // �ͻ������ӡ��Ͽ�ʱ
        public virtual void OnClientConnected(ulong clientId) { }
        public virtual void OnClientDisconnect(ulong clientId) { }

        // ������������ֹͣʱ
        public virtual void OnServerStarted() { }
        public virtual void OnServerStopped(bool IsHostStop) { }

        // IP �ͻ��� - ����
        public virtual void StartClientIP(string playerName, string ipaddress, int port) { }

        // IP Host - ����
        public virtual void StartHostIP(string playerName, string ipaddress, int port) { }

        // Lobby �ͻ��� - ����
        public virtual void StartClientLobby(string playerName) { }

        // Lobby Host - ����
        public virtual void StartHostLobby(string playerName) { }

// �û�����ʱ - �ص�
        public virtual void OnUserRequestedShutdown() { }

        // ��Ϣ����ʧ��ʱ - �ص�
        public virtual void OnTransportFailure() { }

        // ���ӳɹ�ʱ - �ص�
        public virtual void OnConnectApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) { }


    }
}