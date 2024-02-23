using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public class ClientConnectingState : OnlineState {

        protected ConnectionMethodBase connectionMethod;

        /// <summary>
        /// ���õ�ǰ�����ӷ�ʽ
        /// </summary>
        public ClientConnectingState Configure(ConnectionMethodBase baseConnectionMethod) {
            connectionMethod = baseConnectionMethod;
            return this;
        }

        public override async void Enter() {
            //Debug.Log("clientConnectingState: enter method");
            try {
                await connectionMethod.SetupClientConnectionAsync();
                
                bool success = connectionManager.networkManager.StartClient();
                Debug.Log($"{success}: enter method");
                if (!success) {
                    ClientConnectFail();
                }
            } catch {
                ClientConnectFail();
                throw;
            }
            //Debug.Log("clientConnectingState: enter method over");
        }

        public override void Exit() {

        }


        #region ����ص�����

        public override void OnClientConnected(ulong clientId) {
            base.OnClientConnected(clientId);
            // NOTICE: ���ͻ������ӳɹ�ʱ���Ż�ת��connected״̬
            connectionManager.ChangeState(connectionManager.clientConnectedState);
        }

        public override void OnClientDisconnect(ulong clientId) {
            base.OnClientDisconnect(clientId);

            ClientConnectFail();
        }

        private void ClientConnectFail() {
            string disconnectionReason = connectionManager.networkManager.DisconnectReason;
            if (!string.IsNullOrEmpty(disconnectionReason)) {
                Debug.Log("�ͻ�������ʧ��:" + disconnectionReason);
            }
            // ��Ϊ�ͻ�������ʧ�� ת������ģʽ
            connectionManager.ChangeState(connectionManager.offlineState);
            ConnectPanel.Instance.DebugReason("���ӳ�������!");
        }
        #endregion

    }
}