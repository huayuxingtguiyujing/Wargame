using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public class ClientConnectingState : OnlineState {

        protected ConnectionMethodBase connectionMethod;

        /// <summary>
        /// 配置当前的连接方式
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


        #region 网络回调方法

        public override void OnClientConnected(ulong clientId) {
            base.OnClientConnected(clientId);
            // NOTICE: 当客户端连接成功时，才会转到connected状态
            connectionManager.ChangeState(connectionManager.clientConnectedState);
        }

        public override void OnClientDisconnect(ulong clientId) {
            base.OnClientDisconnect(clientId);

            ClientConnectFail();
        }

        private void ClientConnectFail() {
            string disconnectionReason = connectionManager.networkManager.DisconnectReason;
            if (!string.IsNullOrEmpty(disconnectionReason)) {
                Debug.Log("客户端连接失败:" + disconnectionReason);
            }
            // 作为客户端连接失败 转回离线模式
            connectionManager.ChangeState(connectionManager.offlineState);
            ConnectPanel.Instance.DebugReason("连接出现问题!");
        }
        #endregion

    }
}