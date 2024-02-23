using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public class HostingState : OnlineState {
        public override void Enter() {

            // ע: �ⲿ�ִ��룬�����ڽ�������״̬ʱ����������������������
            // TODO: Ҳ��Ӧ����������룿

            // ���Ŀǰ���е�clients������SessionData���־û�����֮
            Dictionary<ulong, NetworkClient> curClients = (Dictionary<ulong, NetworkClient>)connectionManager.networkManager.ConnectedClients;

            foreach (NetworkClient client in curClients.Values)
            {
                ulong clientId = client.ClientId;
                if (clientId == connectionManager.networkManager.LocalClientId) {

                    // playerId�Ǹ��ݵ�ǰ�������������õ�
                    string playerId = "player" + clientId.ToString();
                    string playerName = "placeHolder";
                    // 
                    SessionManager<SessionPlayerData>.Instance.SetConnectingPlayerSessionData(
                        clientId, playerId,
                        new SessionPlayerData(
                            playerName, "NoFaction", false, false, clientId
                    ));
                    //Debug.Log("HostingState,�ص�����! clentId : " + clientId + ", playerId : " + playerId);
                }
            }

            // ת������choosescene;
        }

        public override void Exit() {

        }
    }
}