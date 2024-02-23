using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public class ClientReconnectingState : ClientConnectingState {

        public override void Enter() {
            base.Enter();
            Debug.Log("we are trying to reconnect");
        }

        public override void Exit() { 
            base.Exit();
        }

        // TODO: 完成重连部分
        public override void OnClientConnected(ulong clientId) {
            base.OnClientConnected(clientId);
        }

        public override void OnClientDisconnect(ulong clientId) {
            base.OnClientDisconnect(clientId);
        }



    }
}