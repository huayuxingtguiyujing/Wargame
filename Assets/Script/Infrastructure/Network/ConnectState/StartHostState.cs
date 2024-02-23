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
        /// ���õ�ǰ�����ӷ�ʽ
        /// </summary>
        public StartHostState Configure(ConnectionMethodBase baseConnectionMethod) {
            connectionMethod = baseConnectionMethod;
            return this;
        }

        public override async void Enter() {
            //Debug.Log("��ʼ��������startHostState: enter method");
            try {
                await connectionMethod.SetupHostConnectionAsync();
                // ���Ե����� ��������
                if (!connectionManager.networkManager.StartHost()) {
                    StartHostFailed();
                }
            } catch {
                StartHostFailed();
                throw;
            }
            //Debug.Log("���ӳ������startHostState: enter method over");
        }

        public override void Exit() {

        }


        private void StartHostFailed() {
            //Debug.Log("����ʧ���ˣ�");
            connectionManager.ChangeState(connectionManager.offlineState);
            ConnectPanel.Instance.DebugReason("���ӳ�������!");
        }

        #region ����ص�����

        public override void OnServerStarted() {
            base.OnServerStarted();
            // NOTICE: �������������ɹ�ʱ���Ż�ת��hosting״̬
            connectionManager.ChangeState(connectionManager.hostingState);
        }

        public override void OnConnectApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
            // NOTICE: ò�Ƴ���Щ���⣬��������������������������������Щ���裿
            base.OnConnectApproval(request, response);

            var connectionData = request.Payload;
            ulong clientId = request.ClientNetworkId;

            //Debug.Log("Starthoststate�еĻص����ص�����! clentId : " + clientId + ",connetionData : " + connectionData);

            if (clientId == connectionManager.networkManager.LocalClientId) {

                var payload = System.Text.Encoding.UTF8.GetString(connectionData);
                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
                // NOTICE: https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html

                // NOTICE: playerId�Ǵ�request�е�payload�л�ȡ��
                SessionManager<SessionPlayerData>.Instance.SetConnectingPlayerSessionData(
                    clientId, connectionPayload.playerId,
                    new SessionPlayerData(
                        connectionPayload.playerName,
                        "NoFaction", false, false, clientId)
                );
                
                // ������޸���������𣿺ܻ��� <- ������ã��ٷ��ĵ�д��
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