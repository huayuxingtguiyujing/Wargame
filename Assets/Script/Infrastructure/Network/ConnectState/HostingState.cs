using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public class HostingState : OnlineState {
        public override void Enter() {

            // 注: 这部分代码，用于在进入联机状态时，让主机添加所有联机玩家
            // TODO: 也许不应该在这里加入？

            // 检测目前所有的clients，建立SessionData，持久化保存之
            Dictionary<ulong, NetworkClient> curClients = (Dictionary<ulong, NetworkClient>)connectionManager.networkManager.ConnectedClients;

            foreach (NetworkClient client in curClients.Values)
            {
                ulong clientId = client.ClientId;
                if (clientId == connectionManager.networkManager.LocalClientId) {

                    // playerId是根据当前的联机人数设置的
                    string playerId = "player" + clientId.ToString();
                    string playerName = "placeHolder";
                    // 
                    SessionManager<SessionPlayerData>.Instance.SetConnectingPlayerSessionData(
                        clientId, playerId,
                        new SessionPlayerData(
                            playerName, "NoFaction", false, false, clientId
                    ));
                    //Debug.Log("HostingState,回调触发! clentId : " + clientId + ", playerId : " + playerId);
                }
            }

            // 转场景到choosescene;
        }

        public override void Exit() {

        }
    }
}