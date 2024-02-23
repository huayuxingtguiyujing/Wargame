using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public class ClientConnectedState : OnlineState {
        public override void Enter() {

        }

        public override void Exit() {

        }

        public override void OnClientDisconnect(ulong clientId) {
            base.OnClientDisconnect(clientId);
            string disconnectReason = connectionManager.networkManager.DisconnectReason;
            if(string.IsNullOrEmpty(disconnectReason)) {
                connectionManager.ChangeState(connectionManager.clientReconnectingState);
            } else {
                connectionManager.ChangeState(connectionManager.offlineState);
            }
        }

    }
}