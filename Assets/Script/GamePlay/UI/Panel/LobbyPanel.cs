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

        [Header("Lobby列表")]
        [SerializeField] GameObject lobbyItemPrefab;
        [SerializeField] Transform lobbyItemParent;
        [SerializeField] TMP_Text noLobbiesText;

        [Header("加入房间的邀请码")]
        [SerializeField] TMP_InputField JoinCodeInput;

        [Header("功能按钮")]
        [SerializeField] Button closeButton;
        [SerializeField] Button JoinLobbyButton;
        [SerializeField] Button CreateLobbyButton;
        [SerializeField] Button RefreshButton;

        [Header("创建Lobby弹出框")]
        [SerializeField] GameObject CreateLobbyPop;
        [SerializeField] TMP_InputField LobbyNameInput;
        [SerializeField] Toggle PrivateToggle;
        [SerializeField] Button ConfirmCreateButton;
        [SerializeField] Button CancelCreateButton;

        [Header("加载指示器")]
        [SerializeField] GameObject loadingIndicator;

        private WarLobbyService lobbyService;

        private bool hasInitPanel = false;

        public void InitLobbyPanel(WarLobbyService lobbyService) {
            this.lobbyService = lobbyService;

            // 隐藏没有房间提示
            noLobbiesText.gameObject.SetActive(false);

            HideCreatePop();

            // 绑定按钮事件
            if (!hasInitPanel) {
                JoinLobbyButton.onClick.AddListener(JoinLobbyByCode);
                CreateLobbyButton.onClick.AddListener(InvokeCreatePop);
                RefreshButton.onClick.AddListener(RefreshCurLobby);
                // 创建房间的pop界面
                ConfirmCreateButton.onClick.AddListener(ConfirmCreateLobby);
                CancelCreateButton.onClick.AddListener(HideCreatePop);
                hasInitPanel = true;
            }
           
        }

        public override void Show() {
            base.Show();

            // 刷新当前房间列表
            UpdateCurLobbyItemList();
        }

        /// <summary>
        /// 更新当前的所有可用房间
        /// </summary>
        public async void UpdateCurLobbyItemList() {
            lobbyItemParent.ClearObjChildren();

            QueryResponse queryResponse = await lobbyService.QueryAllLobby();
            if(queryResponse == null) {
                Debug.Log("查询Lobby失败!");
                return;
            }

            Debug.Log($"房间总数:{queryResponse.Results.Count}");
            if (queryResponse.Results.Count == 0) {
                // 没有找到任何房间,唤醒没有房间提示
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
        /// 点击lobbyitem，会自动将邀请码加载到输入框中
        /// </summary>
        private void ClickLobbyItemEvent(string joinCode, string relayCode) {
            JoinCodeInput.text = joinCode;
        }

        #region 按钮事件
        private async void JoinLobbyByCode() {
            
            // 尝试加入Lobby
            string joinCode = JoinCodeInput.text;
            string requestPlayerID = AuthenticationService.Instance.PlayerId;
            var joinResult = await lobbyService.TryJoinLobby(requestPlayerID, null, joinCode);
            if (joinResult.success) {
                // 加入成功
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
            // TODO:通过输入的过滤条件进行搜索
            UpdateCurLobbyItemList();
        }

        private async void ConfirmCreateLobby() {
            string lobbyName = LobbyNameInput.text;
            bool isPrivate = PrivateToggle.isOn;

            if (string.IsNullOrEmpty(lobbyName)) {
                Debug.LogError("输入的lobbyName为空");
                return;
            }

            string requestPlayerID = AuthenticationService.Instance.PlayerId;
            var result = await lobbyService.TryCreateLobby(requestPlayerID, lobbyName, isPrivate);
            if(result.success) {
                lobbyService.localPlayer.IsHost = true;
                // 设置当前所在的房间
                lobbyService.SetCurrLobby(result.lobby);

                Debug.Log($"创建了Lobby ID: {lobbyService.localLobby.LobbyID} " +
                    $"and code {lobbyService.localLobby.LobbyCode}");
                ConnectionManager.Instance.StartHostLobby(lobbyService.localPlayer.DisplayName);
            }
        }

        #endregion

        /// <summary>
        /// 加入一个房间后: 设置当前Lobby，进入在线模式
        /// </summary>
        public void OnJoinLobby(Unity.Services.Lobbies.Models.Lobby remoteLobby) {
            lobbyService.SetCurrLobby(remoteLobby);
            // NOTICE: 此时是从离线模式转为其他模式
            ConnectionManager.Instance.StartClientLobby(lobbyService.localPlayer.DisplayName);
        }
    }
}