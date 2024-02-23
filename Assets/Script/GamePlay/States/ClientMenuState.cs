using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.SceneUtil;
using WarGame_True.UGS.LobbyPack;

namespace WarGame_True.GamePlay.GameState {
    public class ClientMenuState : GameStateBehaviour {
        public override GameState ActiveState => GameState.GameStartMenu;

        [SerializeField] MenuPanel menuPanel;
        [SerializeField] ConnectPanel connectPanel;
        [SerializeField] LobbyPanel lobbyPanel;

        protected override void Start() {
            base.Start();

            menuPanel.InitMenuPanel(SinglePlay, MultiPlay);
            connectPanel.InitConnectPanel(EnterMultiPlaySuccess, EnterMultiPlayFail, ShowLobbyPanel);
            connectPanel.Hide();
            lobbyPanel.InitLobbyPanel(WarLobbyService.Instance);
            lobbyPanel.Hide();
        }

        private void SinglePlay() {
            StartCoroutine(WarSceneManager.Instance.LoadScene("ChooseBookmark"));
        }

        private void MultiPlay() {
            connectPanel.Show();
        }

        private void ShowLobbyPanel() {
            lobbyPanel.Show();
        }

        /// <summary>
        /// 进入多人游戏成功的回调
        /// </summary>
        private void EnterMultiPlaySuccess() {
            //Debug.Log("joinIn success!");
            // 将玩家数据加入到SessionManager中
            ulong clientId = NetworkManager.Singleton.LocalClientId;

            // TODO：将玩家数据持久化到场景中
            //NetworkManager.Singleton.

            WarSceneManager.Instance.LoadScene("ChooseBookmark", useNetworkSceneManager: true);

        }

        private void EnterMultiPlayFail() {
            Debug.Log("join in fail!");
        }

    }
}