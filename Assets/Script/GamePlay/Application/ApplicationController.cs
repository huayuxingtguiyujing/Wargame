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

        [Header("���Զ������")]
        [SerializeField] NetworkManager networkManager;

        public WarAuthService AuthService {  get; private set; }
        

        [Header("�Ƿ���ת����ʼҳ��")]
        public bool jumpToMenu = false;

        // �Ƿ����ڲ���ui,��ʱ��ֹ�������һЩ��ݼ�
        public bool isHandingUI = false;


        private void Awake() {
            DontDestroyOnLoad(gameObject);

            Instance = this;

            audioManager = GetComponent<AudioManager>();
            audioManager.InitAudioManager();

            // ��ʼ��authService
            AuthService = new WarAuthService();
            InitializationOptions options = new InitializationOptions();
            AuthService.InitAuthService(options);

            // ��ʼ��lobbyService
            lobbyService.InitWarLobbyService(AuthService);

            // ��ʼ������������
            warSceneManager.InitWarSceneManager(networkManager);

            // ��ʼ���������ӹ�����
            connectionManager.InitConnectManager(networkManager);

            //�����ǰScene����mainmenuscene�����л����浽���˵�
            string theFirstSceneName = "MenuScene";
            if (WarSceneManager.GetActiveScene() != theFirstSceneName && jumpToMenu) {
                Debug.Log("��ǰ�Ľ��治�ǳ�ʼ���棡");
                StartCoroutine(WarSceneManager.Instance.LoadScene(theFirstSceneName));
            }
        }

        private void Update() {



            // ����gameplay scene �Ż���ܿ�ݼ�����
            //if () {
            //}


            if (Input.GetKeyDown(KeyCode.BackQuote)) {
                // ��ݼ�-�򿪵������
                //Debug.Log("enter debug !");
                if (DebugPanel.Instance != null) {
                    isHandingUI = !isHandingUI;
                    DebugPanel.Instance.HandlePanel();
                }
            }

            // �������ڲ���ui�ǽ��õĿ�ݼ�
            if (!isHandingUI) {
                return;
            }

            // �������п�ݼ�����
            if (Input.GetKeyDown(KeyCode.C)) {
                // ��ݼ�-ȷ��
                //Debug.Log("enter c !");
            }

            if (Input.GetKeyDown(KeyCode.Z)) {
                // ��ݼ�-ȡ��
                //Debug.Log("enter z !");
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                // ��ͣ ����ʱ��
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