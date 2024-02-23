using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.NetworkPack.ChooseScene;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Infrastructure.NetworkPackage;

namespace WarGame_True.GamePlay.GameState {
    /// <summary>
    /// �������˵� ѡ������ ���������
    /// </summary>
    public class ServerChooseState : GameStateBehaviour {
        public override GameState ActiveState => GameState.FactionSelect;

        [SerializeField] NetworkHook networkHook;
        [SerializeField] NetworkFactionChoose networkFactionChoose;

        #region MonoBehaviour

        private void Awake() {
            networkFactionChoose.InitNetworkFactionChoose();
            networkHook.OnNetworkSpawnHook += OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            networkHook.OnNetworkSpawnHook -= OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook -= OnNetworkDespawn;

            if (NetworkManager.Singleton && NetworkManager.Singleton.IsClient) {
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
            }
        }

        public void OnNetworkSpawn() {
            //Debug.Log("serverchooseState��������");
            if (!NetworkManager.Singleton.IsServer) {
                // �����ǰ���Ƿ����� ����øýű�
                enabled = false;
            } else {
                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
                networkFactionChoose.OnClientChangeChooseTag += ChangeChooseTagCallback;
            }
        }

        public void OnNetworkDespawn() {
            if (NetworkManager.Singleton) {
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
            }
        }

        #endregion

        /// <summary>
        /// TODO: �ҽӸ�SceneManager�ĳ����仯�¼�
        /// </summary>
        /// <remarks>
        /// �ٷ��ĵ�: Subscribe to this event to receive all SceneEventType notifications.
        /// </remarks>
        private void OnSceneEvent(SceneEvent sceneEvent) {
            if (sceneEvent.SceneEventType != SceneEventType.LoadEventCompleted) {
                return;
            }

            // ���ڽ���ѡ����ҽ���ʱ ����seatnewplayer
            SeatNewPlayer(sceneEvent.ClientId);
        }

        private void SeatNewPlayer(ulong clientId) {
            //Debug.Log("������SeatNewPlayer, clentID��: " + clientId);

            string playerId = "Player" + clientId;
            SessionPlayerData sessionPlayerData = new SessionPlayerData(playerId, "noTag", true, true, clientId);
            SessionManager<SessionPlayerData>.Instance.SetConnectingPlayerSessionData(clientId, playerId, sessionPlayerData);

            // �ܹ��ҵ���Ӧ��clientId��Ϣ,˵���Ѿ��ɹ����
            SessionPlayerData? rec = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (rec.HasValue && networkFactionChoose != null) {
                // NOTICE: ��Ҫ�ж�networkFactionChoose�Ƿ�δ������
                networkFactionChoose.NewPlayerJoinIn(clientId);
            }
        }

        public void ChangeChooseTagCallback(ulong clientId, string newFactionTag, bool isLocked) {
            //Debug.Log("��ǰ����������Ŀ: " + networkFactionChoose.PlayerChooseStates.Count);
            // ������ǰ��������飬�������ѡ��
            for(int i = 0; i < networkFactionChoose.PlayerChooseStates.Count; i++) {
                if (networkFactionChoose.PlayerChooseStates[i].ClientId == clientId) {

                    if (!networkFactionChoose.CanPlayerChooseTag(clientId, newFactionTag)) {
                        return;
                    }
                    Debug.Log("���ѡ������tag, clientId: " + clientId + "����tagΪ" + newFactionTag + "; update it");
                    networkFactionChoose.PlayerChooseStates[i] = new PlayerChooseState(
                        clientId, networkFactionChoose.PlayerChooseStates[i].PlayerId,
                        newFactionTag, Time.time, networkFactionChoose.PlayerChooseStates[i].PlayerColor
                    );
                    return;
                }
            }
        }

    }
}