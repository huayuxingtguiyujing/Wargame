using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {
    /// <summary>
    /// ������δ�̳� NetworkBehaviour �Ľű������Ե���OnNetworkSpawn����
    /// </summary>
    public class NetworkHook : NetworkBehaviour {
        public event Action OnNetworkSpawnHook;

        public event Action OnNetworkDespawnHook;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            OnNetworkSpawnHook?.Invoke();
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            OnNetworkDespawnHook?.Invoke();
        }
    }
}