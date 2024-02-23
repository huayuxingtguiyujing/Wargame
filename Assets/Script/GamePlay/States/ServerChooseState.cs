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
    /// 服务器端的 选择势力 界面管理器
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
            //Debug.Log("serverchooseState正常连接");
            if (!NetworkManager.Singleton.IsServer) {
                // 如果当前不是服务器 则禁用该脚本
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
        /// TODO: 挂接给SceneManager的场景变化事件
        /// </summary>
        /// <remarks>
        /// 官方文档: Subscribe to this event to receive all SceneEventType notifications.
        /// </remarks>
        private void OnSceneEvent(SceneEvent sceneEvent) {
            if (sceneEvent.SceneEventType != SceneEventType.LoadEventCompleted) {
                return;
            }

            // 仅在进入选择国家界面时 调用seatnewplayer
            SeatNewPlayer(sceneEvent.ClientId);
        }

        private void SeatNewPlayer(ulong clientId) {
            //Debug.Log("触发了SeatNewPlayer, clentID是: " + clientId);

            string playerId = "Player" + clientId;
            SessionPlayerData sessionPlayerData = new SessionPlayerData(playerId, "noTag", true, true, clientId);
            SessionManager<SessionPlayerData>.Instance.SetConnectingPlayerSessionData(clientId, playerId, sessionPlayerData);

            // 能够找到对应的clientId信息,说明已经成功添加
            SessionPlayerData? rec = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (rec.HasValue && networkFactionChoose != null) {
                // NOTICE: 需要判断networkFactionChoose是否还未被销毁
                networkFactionChoose.NewPlayerJoinIn(clientId);
            }
        }

        public void ChangeChooseTagCallback(ulong clientId, string newFactionTag, bool isLocked) {
            //Debug.Log("当前加入的玩家数目: " + networkFactionChoose.PlayerChooseStates.Count);
            // 遍历当前的玩家数组，更新玩家选择
            for(int i = 0; i < networkFactionChoose.PlayerChooseStates.Count; i++) {
                if (networkFactionChoose.PlayerChooseStates[i].ClientId == clientId) {

                    if (!networkFactionChoose.CanPlayerChooseTag(clientId, newFactionTag)) {
                        return;
                    }
                    Debug.Log("玩家选择了新tag, clientId: " + clientId + "设置tag为" + newFactionTag + "; update it");
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