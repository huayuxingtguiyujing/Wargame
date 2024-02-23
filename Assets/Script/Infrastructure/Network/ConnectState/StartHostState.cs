using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public class StartHostState : OnlineState {

        ConnectionMethodBase connectionMethod;

        /// <summary>
        /// 配置当前的连接方式
        /// </summary>
        public StartHostState Configure(ConnectionMethodBase baseConnectionMethod) {
            connectionMethod = baseConnectionMethod;
            return this;
        }

        public override async void Enter() {
            //Debug.Log("开始尝试连接startHostState: enter method");
            try {
                await connectionMethod.SetupHostConnectionAsync();
                // 尝试当主机 进行连接
                if (!connectionManager.networkManager.StartHost()) {
                    StartHostFailed();
                }
            } catch {
                StartHostFailed();
                throw;
            }
            //Debug.Log("连接尝试完毕startHostState: enter method over");
        }

        public override void Exit() {

        }


        private void StartHostFailed() {
            //Debug.Log("连接失败了！");
            connectionManager.ChangeState(connectionManager.offlineState);
            ConnectPanel.Instance.DebugReason("连接出现问题!");
        }

        #region 网络回调方法

        public override void OnServerStarted() {
            base.OnServerStarted();
            // NOTICE: 当服务器启动成功时，才会转到hosting状态
            connectionManager.ChangeState(connectionManager.hostingState);
        }

        public override void OnConnectApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
            // NOTICE: 貌似出了些问题，不会调用这个方法！！！，可能是少了些步骤？
            base.OnConnectApproval(request, response);

            var connectionData = request.Payload;
            ulong clientId = request.ClientNetworkId;

            //Debug.Log("Starthoststate中的回调，回调触发! clentId : " + clientId + ",connetionData : " + connectionData);

            if (clientId == connectionManager.networkManager.LocalClientId) {

                var payload = System.Text.Encoding.UTF8.GetString(connectionData);
                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
                // NOTICE: https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html

                // NOTICE: playerId是从request中的payload中获取的
                SessionManager<SessionPlayerData>.Instance.SetConnectingPlayerSessionData(
                    clientId, connectionPayload.playerId,
                    new SessionPlayerData(
                        connectionPayload.playerName,
                        "NoFaction", false, false, clientId)
                );
                
                // 这里的修改真的有用吗？很怀疑 <- 真的有用，官方文档写了
                response.Approved = true;
                response.CreatePlayerObject = true;
            }
        }

        public override void OnServerStopped(bool IsHostStop) {
            base.OnServerStopped(IsHostStop);
            StartHostFailed();
        }

        #endregion

    }
}