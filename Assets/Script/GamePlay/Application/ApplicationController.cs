using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.Audio;
using WarGame_True.Infrastructure.Auth;
using WarGame_True.Infrastructure.NetworkPackage;
using WarGame_True.Infrastructure.SceneUtil;
using WarGame_True.UGS.LobbyPack;

namespace WarGame_True.GamePlay.Application {

    public class ApplicationController : MonoBehaviour {

        public static ApplicationController Instance { get; private set; }

        public static FactionInfo PlayerFaction { get; private set; }

        [SerializeField] AudioManager audioManager;
        [SerializeField] WarSceneManager warSceneManager;
        [SerializeField] ConnectionManager connectionManager;
        [SerializeField] WarLobbyService lobbyService;

        public ConnectionManager ConnectionManager => connectionManager;

        [Header("非自定义组件")]
        [SerializeField] NetworkManager networkManager;

        public WarAuthService AuthService {  get; private set; }
        

        [Header("是否跳转到初始页面")]
        public bool jumpToMenu = false;

        // 是否正在操作ui,此时禁止摄像机的一些快捷键
        public bool isHandingUI = false;


        private void Awake() {
            DontDestroyOnLoad(gameObject);

            Instance = this;

            audioManager = GetComponent<AudioManager>();
            audioManager.InitAudioManager();

            // 初始化authService
            AuthService = new WarAuthService();
            InitializationOptions options = new InitializationOptions();
            AuthService.InitAuthService(options);

            // 初始化lobbyService
            lobbyService.InitWarLobbyService(AuthService);

            // 初始化场景管理器
            warSceneManager.InitWarSceneManager(networkManager);

            // 初始化网络连接管理器
            connectionManager.InitConnectManager(networkManager);

            //如果当前Scene不是mainmenuscene，则切换界面到主菜单
            string theFirstSceneName = "MenuScene";
            if (WarSceneManager.GetActiveScene() != theFirstSceneName && jumpToMenu) {
                Debug.Log("当前的界面不是初始界面！");
                StartCoroutine(WarSceneManager.Instance.LoadScene(theFirstSceneName));
            }
        }

        private void Update() {



            // 仅在gameplay scene 才会接受快捷键输入
            //if () {
            //}


            if (Input.GetKeyDown(KeyCode.BackQuote)) {
                // 快捷键-打开调试面板
                //Debug.Log("enter debug !");
                if (DebugPanel.Instance != null) {
                    isHandingUI = !isHandingUI;
                    DebugPanel.Instance.HandlePanel();
                }
            }

            // 以下是在操作ui是禁用的快捷键
            if (!isHandingUI) {
                return;
            }

            // 接受所有快捷键输入
            if (Input.GetKeyDown(KeyCode.C)) {
                // 快捷键-确认
                //Debug.Log("enter c !");
            }

            if (Input.GetKeyDown(KeyCode.Z)) {
                // 快捷键-取消
                //Debug.Log("enter z !");
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                // 暂停 开启时间
            }

        }

        public void SetPlayerFaction(FactionInfo factionInfo) {
            ApplicationController.PlayerFaction = factionInfo;
            //PoliticLoader.Instance.SetPlayerFaction(factionInfo);
        }

        public void SetMultiPlayerFaction() {

        }

    }
}