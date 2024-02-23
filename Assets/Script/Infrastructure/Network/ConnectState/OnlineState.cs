using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public abstract class OnlineState : ConnectionState {

        public override void OnUserRequestedShutdown() {
            // ����ʧ�ܣ���������ģʽ
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.offlineState);
        }

        public override void OnTransportFailure() {
            ConnectionManager.Instance.ChangeState(ConnectionManager.Instance.offlineState);
        }

    }
}