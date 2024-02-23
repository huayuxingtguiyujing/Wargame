
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
    /// �ͻ��˵� ѡ������ ���������
    /// </summary>
    public class ClientChooseState : GameStateBehaviour {
        public override GameState ActiveState => GameState.FactionSelect;

        [SerializeField] NetworkHook networkHook;
        [SerializeField] NetworkFactionChoose networkFactionChoose;

        [Header("�������UI")]
        //[SerializeField] ConnectPanel connectPanel;
        [SerializeField] ChoosePanel choosePanel;

        [Header("��ǰ�籾")]
        [SerializeField] BookMarks currentBookMarks;

        #region MonoBehaviour

        private void Awake() {

            networkHook.OnNetworkSpawnHook += OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook += OnNetworkDespawn;

            choosePanel.InitChoosePanel(currentBookMarks, StartGame, ReturnMenu);
            choosePanel.chooseFactionCallback = ChooseFactionCall;
            // NetworkList�����ı�ʱ�Ļص�
            networkFactionChoose.PlayerChooseStates.OnListChanged += UpdateChooseFaction;

            //Debug.Log("ClientChooseState Start��������");
        }

        //NOTICE: ��������awake�ķ�����д��start�е�
        // ���ǹҽӵ���networkspawn����ȴִ�в��ˣ�������Ϊnetworkspawnһ���startͬʱִ��
        // ����ο��ٷ��ĵ��networkspawn��awake��startʱ��Ĳ���
        //https://docs-multiplayer.unity3d.com/netcode/current/basics/networkbehavior/

        protected override void OnDestroy() {
            networkHook.OnNetworkSpawnHook -= OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook -= OnNetworkDespawn;
        }

        public void OnNetworkSpawn() {
            if (NetworkManager.Singleton.IsClient) {
                // ���������������������
                networkFactionChoose.PlayerRequestJoinServerRpc();
                // ˢ��choosePanel�ĵ�ǰ���
                List<PlayerChooseState> rec = new List<PlayerChooseState>();
                foreach (var state in networkFactionChoose.PlayerChooseStates)
                {
                    rec.Add(state);
                }
                choosePanel.ShowCurPlayers(rec);
                networkFactionChoose.OnSetPlayerFaction += OnSetPlayerFactionCallback;
            } else {
                // ���ǿͻ��ˣ�������״̬ ������ ��������״̬�����ô˽ű�
                enabled = false;
            }
        }

        public void OnNetworkDespawn() {
            if (NetworkManager.Singleton.IsClient) {
                //networkCharSelect.PlayerChooseStates.OnListChanged -= OnPlayerSeatChange;
            }
        }
        #endregion

        #region ��������״̬�µ����ѡ��

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

            // ����PlayerChooseStates ���µ�ǰ��ѡ��
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

            // ˢ��choosePanel�ĵ�ǰ�������
            List<PlayerChooseState> rec = new List<PlayerChooseState>();
            foreach (var state in networkFactionChoose.PlayerChooseStates) {
                rec.Add(state);
            }
            choosePanel.ShowCurPlayers(rec);
        }

        #endregion

        private void StartGame() {

            // ����Լ��Ƿ�������״̬�µ��������������޷���ʼ��Ϸ
            if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient) {
                choosePanel.ShowNotice("�㲻���������޷���ʼ��Ϸ", 2);
                return;
            }

            if (choosePanel.currentChoosenFaction == null) {
                choosePanel.ShowNotice("����ѡ����ݵĹ���!", 2);
                return;
            }

            

            if (networkFactionChoose.IsSpawned) {
                // �ж��Ƿ�������Ҷ��ɹ�ѡ��������,���򵯳���ʾ 
                if (networkFactionChoose.HasAllPlayerChooseTag()) {
                    SessionManager<SessionPlayerData>.Instance.PrintAllPlayerData();
                    // ֪ͨ���пͻ��ˣ��������ǵĵ�ǰTag
                    networkFactionChoose.SetPlayerFactionClientRpc();
                } else {
                    choosePanel.ShowNotice("�������û��ѡ������!", 2);
                    return;
                }

                // ��������״̬ ʹ��NetworkManager.SceneManager
                WarSceneManager.Instance.LoadScene("GamePlayScene", useNetworkSceneManager: true);
            } else {
                OnSetPlayerFactionCallback();
                // ������״̬ ʹ��unityEngineԭ��loadScene
                StartCoroutine(WarSceneManager.Instance.LoadScene("GamePlayScene"));
            }
            
        }

        private void ReturnMenu() {

            // TODO: ���������������У��Ͽ�����
            if (networkFactionChoose.IsSpawned) {
                if (NetworkManager.Singleton.IsServer) {
                    // TODO: �Ƿ����� �������������ҶϿ�����
                    NetworkManager.Singleton.Shutdown();
                } else if (NetworkManager.Singleton.IsClient) {
                    NetworkManager.Singleton.Shutdown();
                }

            }

            StartCoroutine(WarSceneManager.Instance.LoadScene("MenuScene"));
        }

        public void OnSetPlayerFactionCallback() {
            ApplicationController.Instance.SetPlayerFaction(choosePanel.currentChoosenFaction);
            //Debug.Log("�Ѿ����ñ�����ѡtag: " + choosePanel.currentChoosenFaction.FactionTag);
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
        /// ռλ��������Ч��PlayerState
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
            return $"���clientID:{ClientId},playerID:{PlayerId},��ѡ����:{PlayerFactionTag}";
        }
    }


}