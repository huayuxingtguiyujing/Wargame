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

        [Header("����������")]
        [SerializeField] NetworkHook networkHook;

        [Header("��Ϸ��Ϣ����������")]
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
                // ����������� �ص�
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
                // ����ͬ����� �ص�
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
            // ����Ҽ�����ϣ����뵽��ǰ������
            // ���ɸ���ҵ� Faction
        }

        private void OnSynchronizeComplete(ulong clientId) {

        }

        #endregion


        public void InitGamePlayNetwork(Transform factionParent) {
            if (NetworkManager.Singleton.IsClient) {
                // ��������״̬�����ʼ������ͬ�����

                // ��ʼ���������� ������������ʼ���������PoliticLoader
                politicNetworkLoader.InitPoliticNetwork();
            }
        }
    }
}