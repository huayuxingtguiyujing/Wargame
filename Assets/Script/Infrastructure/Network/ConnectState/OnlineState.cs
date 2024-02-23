using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public abstract class OnlineState : ConnectionState {

        public override void OnUserRequestedShutdown() {
            // 连接失败，进入离线模式
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.offlineState);
        }

        public override void OnTransportFailure() {
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.offlineState);
        }

    }
}