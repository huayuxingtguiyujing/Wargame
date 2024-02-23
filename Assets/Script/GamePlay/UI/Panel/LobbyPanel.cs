using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.Application;
using WarGame_True.Infrastructure.NetworkPackage;
using WarGame_True.UGS.LobbyPack;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    public class LobbyPanel : BasePopUI {

        [Header("Lobby�б�")]
        [SerializeField] GameObject lobbyItemPrefab;
        [SerializeField] Transform lobbyItemParent;
        [SerializeField] TMP_Text noLobbiesText;

        [Header("���뷿���������")]
        [SerializeField] TMP_InputField JoinCodeInput;

        [Header("���ܰ�ť")]
        [SerializeField] Button closeButton;
        [SerializeField] Button JoinLobbyButton;
        [SerializeField] Button CreateLobbyButton;
        [SerializeField] Button RefreshButton;

        [Header("����Lobby������")]
        [SerializeField] GameObject CreateLobbyPop;
        [SerializeField] TMP_InputField LobbyNameInput;
        [SerializeField] Toggle PrivateToggle;
        [SerializeField] Button ConfirmCreateButton;
        [SerializeField] Button CancelCreateButton;

        [Header("����ָʾ��")]
        [SerializeField] GameObject loadingIndicator;

        private WarLobbyService lobbyService;

        private bool hasInitPanel = false;

        public void InitLobbyPanel(WarLobbyService lobbyService) {
            this.lobbyService = lobbyService;

            // ����û�з�����ʾ
            noLobbiesText.gameObject.SetActive(false);

            HideCreatePop();

            // �󶨰�ť�¼�
            if (!hasInitPanel) {
                JoinLobbyButton.onClick.AddListener(JoinLobbyByCode);
                CreateLobbyButton.onClick.AddListener(InvokeCreatePop);
                RefreshButton.onClick.AddListener(RefreshCurLobby);
                // ���������pop����
                ConfirmCreateButton.onClick.AddListener(ConfirmCreateLobby);
                CancelCreateButton.onClick.AddListener(HideCreatePop);
                hasInitPanel = true;
            }
           
        }

        public override void Show() {
            base.Show();

            // ˢ�µ�ǰ�����б�
            UpdateCurLobbyItemList();
        }

        /// <summary>
        /// ���µ�ǰ�����п��÷���
        /// </summary>
        public async void UpdateCurLobbyItemList() {
            lobbyItemParent.ClearObjChildren();

            QueryResponse queryResponse = await lobbyService.QueryAllLobby();
            if(queryResponse == null) {
                Debug.Log("��ѯLobbyʧ��!");
                return;
            }

            Debug.Log($"��������:{queryResponse.Results.Count}");
            if (queryResponse.Results.Count == 0) {
                // û���ҵ��κη���,����û�з�����ʾ
                noLobbiesText.gameObject.SetActive(true);
                return;
            }

            noLobbiesText.gameObject.SetActive(false);
            foreach (Lobby lobby in queryResponse.Results)
            {
                LocalLobby localLobby = LocalLobby.CreateLocalLobby(lobby);
                GameObject lobbyItemObject = Instantiate(lobbyItemPrefab, lobbyItemParent);
                LobbyItem lobbyItem = lobbyItemObject.GetComponent<LobbyItem>();
                lobbyItem.InitLobbyItem(localLobby, ClickLobbyItemEvent);
            }
        }

        /// <summary>
        /// ���lobbyitem�����Զ�����������ص��������
        /// </summary>
        private void ClickLobbyItemEvent(string joinCode, string relayCode) {
            JoinCodeInput.text = joinCode;
        }

        #region ��ť�¼�
        private async void JoinLobbyByCode() {
            
            // ���Լ���Lobby
            string joinCode = JoinCodeInput.text;
            string requestPlayerID = AuthenticationService.Instance.PlayerId;
            var joinResult = await lobbyService.TryJoinLobby(requestPlayerID, null, joinCode);
            if (joinResult.success) {
                // ����ɹ�
                OnJoinLobby(joinResult.lobby);
            }
        }

        private void InvokeCreatePop() {
            CreateLobbyPop.gameObject.SetActive(true);
        }

        private void HideCreatePop() {
            CreateLobbyPop.gameObject.SetActive(false);
        }

        private void RefreshCurLobby() {
            // TODO:ͨ������Ĺ���������������
            UpdateCurLobbyItemList();
        }

        private async void ConfirmCreateLobby() {
            string lobbyName = LobbyNameInput.text;
            bool isPrivate = PrivateToggle.isOn;

            if (string.IsNullOrEmpty(lobbyName)) {
                Debug.LogError("�����lobbyNameΪ��");
                return;
            }

            string requestPlayerID = AuthenticationService.Instance.PlayerId;
            var result = await lobbyService.TryCreateLobby(requestPlayerID, lobbyName, isPrivate);
            if(result.success) {
                lobbyService.localPlayer.IsHost = true;
                // ���õ�ǰ���ڵķ���
                lobbyService.SetCurrLobby(result.lobby);

                Debug.Log($"������Lobby ID: {lobbyService.localLobby.LobbyID} " +
                    $"and code {lobbyService.localLobby.LobbyCode}");
                ConnectionManager.Instance.StartHostLobby(lobbyService.localPlayer.DisplayName);
            }
        }

        #endregion

        /// <summary>
        /// ����һ�������: ���õ�ǰLobby����������ģʽ
        /// </summary>
        public void OnJoinLobby(Unity.Services.Lobbies.Models.Lobby remoteLobby) {
            lobbyService.SetCurrLobby(remoteLobby);
            // NOTICE: ��ʱ�Ǵ�����ģʽתΪ����ģʽ
            ConnectionManager.Instance.StartClientLobby(lobbyService.localPlayer.DisplayName);
        }
    }
}