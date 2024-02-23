using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using WarGame_True.GamePlay.GameState;
using WarGame_True.GamePlay.NetworkPack.ChooseScene;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Infrastructure.NetworkPackage;

namespace WarGame_True.States {
    public class GamePlayServerState : GameStateBehaviour {
        public override GameState ActiveState => GameState.MultiWarGame;

        [Header("网络相关组件")]
        [SerializeField] NetworkHook networkHook;

        [Header("游戏信息网络管理组件")]
        [SerializeField] PoliticNetworkLoader politicNetworkLoader;

        #region Network/MonoBehaviour
        private void Awake() {
            //networkPlayerFactions.InitNetworkPlayerFaction();
            networkHook.OnNetworkSpawnHook += OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        protected override void Start() {
            base.Start();
        }

        protected override void OnDestroy() {
            networkHook.OnNetworkSpawnHook -= OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook -= OnNetworkDespawn;
        }

        public void OnNetworkSpawn() {
            if(NetworkManager.Singleton.IsServer) {
                // 场景加载完毕 回调
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
                // 场景同步完毕 回调
                NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
            }
        }

        public void OnNetworkDespawn() {
            if (NetworkManager.Singleton.IsServer) {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
                NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
            }
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
            // 有玩家加载完毕，进入到当前场景中
            // 生成该玩家的 Faction
        }

        private void OnSynchronizeComplete(ulong clientId) {

        }

        #endregion


        public void InitGamePlayNetwork(Transform factionParent) {
            if (NetworkManager.Singleton.IsClient) {
                // 处于联网状态，则初始化各个同步组件

                // 初始化政治势力 网络组件，其初始化必须后于PoliticLoader
                politicNetworkLoader.InitPoliticNetwork();
            }
        }
    }
}