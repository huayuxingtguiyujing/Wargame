
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.NetworkPack.ChooseScene;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.NetworkPackage;
using WarGame_True.Infrastructure.SceneUtil;

namespace WarGame_True.GamePlay.GameState {
    /// <summary>
    /// 客户端的 选择势力 界面管理器
    /// </summary>
    public class ClientChooseState : GameStateBehaviour {
        public override GameState ActiveState => GameState.FactionSelect;

        [SerializeField] NetworkHook networkHook;
        [SerializeField] NetworkFactionChoose networkFactionChoose;

        [Header("连接面板UI")]
        //[SerializeField] ConnectPanel connectPanel;
        [SerializeField] ChoosePanel choosePanel;

        [Header("当前剧本")]
        [SerializeField] BookMarks currentBookMarks;

        #region MonoBehaviour

        private void Awake() {

            networkHook.OnNetworkSpawnHook += OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook += OnNetworkDespawn;

            choosePanel.InitChoosePanel(currentBookMarks, StartGame, ReturnMenu);
            choosePanel.chooseFactionCallback = ChooseFactionCall;
            // NetworkList发生改变时的回调
            networkFactionChoose.PlayerChooseStates.OnListChanged += UpdateChooseFaction;

            //Debug.Log("ClientChooseState Start方法正常");
        }

        //NOTICE: 本来上面awake的方法是写在start中的
        // 但是挂接到的networkspawn方法却执行不了，这是因为networkspawn一般和start同时执行
        // 建议参考官方文档里，networkspawn、awake、start时序的部分
        //https://docs-multiplayer.unity3d.com/netcode/current/basics/networkbehavior/

        protected override void OnDestroy() {
            networkHook.OnNetworkSpawnHook -= OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook -= OnNetworkDespawn;
        }

        public void OnNetworkSpawn() {
            if (NetworkManager.Singleton.IsClient) {
                // 向服务器发送添加玩家请求
                networkFactionChoose.PlayerRequestJoinServerRpc();
                // 刷新choosePanel的当前玩家
                List<PlayerChooseState> rec = new List<PlayerChooseState>();
                foreach (var state in networkFactionChoose.PlayerChooseStates)
                {
                    rec.Add(state);
                }
                choosePanel.ShowCurPlayers(rec);
                networkFactionChoose.OnSetPlayerFaction += OnSetPlayerFactionCallback;
            } else {
                // 不是客户端，即断网状态 或者是 纯服务器状态，禁用此脚本
                enabled = false;
            }
        }

        public void OnNetworkDespawn() {
            if (NetworkManager.Singleton.IsClient) {
                //networkCharSelect.PlayerChooseStates.OnListChanged -= OnPlayerSeatChange;
            }
        }
        #endregion

        #region 管理联网状态下的玩家选择

        public void ChooseFactionCall(string chooseFactionTag) {
            //Debug.Log(networkFactionChoose.IsSpawned);
            //if (networkFactionChoose.IsSpawned)
            //{
                ulong clientId = NetworkManager.Singleton.LocalClientId;
                //Debug.Log(clientId);
                networkFactionChoose.ChangeChooseTagServerRpc(clientId, chooseFactionTag, false);
            //}
        }

        public void UpdateChooseFaction(NetworkListEvent<PlayerChooseState> changeEvent) {
            //Debug.Log("update choose factions");

            // 根据PlayerChooseStates 更新当前的选择
            foreach (CountryItem countryItem in choosePanel.countryItems) {
                string countryTag = countryItem.ItemFaction.FactionTag;
                if (networkFactionChoose.HasPlayerChooseTag(countryTag)) {
                    PlayerChooseState playerChooseState = networkFactionChoose.GetPlayerByTag(countryTag);
                    countryItem.SetChoosen();
                    countryItem.SetChoosePlayer(playerChooseState.PlayerId, playerChooseState.PlayerColor);
                } else {
                    countryItem.SetUnchoosen();
                }
            }

            // 刷新choosePanel的当前所有玩家
            List<PlayerChooseState> rec = new List<PlayerChooseState>();
            foreach (var state in networkFactionChoose.PlayerChooseStates) {
                rec.Add(state);
            }
            choosePanel.ShowCurPlayers(rec);
        }

        #endregion

        private void StartGame() {

            // 检测自己是否是联网状态下的主机，不是则无法开始游戏
            if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient) {
                choosePanel.ShowNotice("你不是主机，无法开始游戏", 2);
                return;
            }

            if (choosePanel.currentChoosenFaction == null) {
                choosePanel.ShowNotice("请先选择扮演的国家!", 2);
                return;
            }

            

            if (networkFactionChoose.IsSpawned) {
                // 判断是否所有玩家都成功选择了势力,否则弹出提示 
                if (networkFactionChoose.HasAllPlayerChooseTag()) {
                    SessionManager<SessionPlayerData>.Instance.PrintAllPlayerData();
                    // 通知所有客户端，设置他们的当前Tag
                    networkFactionChoose.SetPlayerFactionClientRpc();
                } else {
                    choosePanel.ShowNotice("还有玩家没有选择势力!", 2);
                    return;
                }

                // 处于联网状态 使用NetworkManager.SceneManager
                WarSceneManager.Instance.LoadScene("GamePlayScene", useNetworkSceneManager: true);
            } else {
                OnSetPlayerFactionCallback();
                // 非联网状态 使用unityEngine原生loadScene
                StartCoroutine(WarSceneManager.Instance.LoadScene("GamePlayScene"));
            }
            
        }

        private void ReturnMenu() {

            // TODO: 若处于网络连接中，断开连接
            if (networkFactionChoose.IsSpawned) {
                if (NetworkManager.Singleton.IsServer) {
                    // TODO: 是服务器 则让连接你的玩家断开连接
                    NetworkManager.Singleton.Shutdown();
                } else if (NetworkManager.Singleton.IsClient) {
                    NetworkManager.Singleton.Shutdown();
                }

            }

            StartCoroutine(WarSceneManager.Instance.LoadScene("MenuScene"));
        }

        public void OnSetPlayerFactionCallback() {
            ApplicationController.Instance.SetPlayerFaction(choosePanel.currentChoosenFaction);
            //Debug.Log("已经设置本机所选tag: " + choosePanel.currentChoosenFaction.FactionTag);
        }

    }



    public struct PlayerChooseState : INetworkSerializable, IEquatable<PlayerChooseState> {
        public ulong ClientId;

        public Color PlayerColor;
        public NetworkString PlayerId;
        public NetworkString PlayerFactionTag;

        public float LastChangeTime;

        public PlayerChooseState(ulong clientId, NetworkString playerId, NetworkString playerFactionTag, float lastChangeTime, Color PlayerColor) {
            ClientId = clientId;
            this.PlayerId = playerId;
            this.PlayerFactionTag = playerFactionTag;
            LastChangeTime = lastChangeTime;
            this.PlayerColor = PlayerColor;
        }

        /// <summary>
        /// 占位符，是无效的PlayerState
        /// </summary>
        /// <returns></returns>
        public static PlayerChooseState GetDefaultPlayerChooseState() {
            return new PlayerChooseState(999, "noname", "nofaction", Time.time, Color.black);
        }

        public bool Equals(PlayerChooseState other) {
            return ClientId == other.ClientId
                && PlayerId == other.PlayerId
                && PlayerFactionTag == other.PlayerFactionTag
                && LastChangeTime == other.LastChangeTime;
        }

        public bool HasChooseValidTag() {
            return PlayerFactionTag != "nofaction" && !string.IsNullOrEmpty(PlayerFactionTag);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerId);
            serializer.SerializeValue(ref PlayerFactionTag);
            serializer.SerializeValue(ref LastChangeTime);
        }

        public override string ToString() {
            return $"玩家clientID:{ClientId},playerID:{PlayerId},所选国家:{PlayerFactionTag}";
        }
    }


}