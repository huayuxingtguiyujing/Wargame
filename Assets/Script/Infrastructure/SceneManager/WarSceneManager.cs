using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.Infrastructure.SceneUtil {
    /// <summary>
    /// ר�����ڹ��������ص������ʹ��ʱ���ýӿڣ���Ҫ�ҽӵ�һ�������ϲ����г�ʼ��
    /// </summary>
    public class WarSceneManager : NetworkBehaviour{

        public static WarSceneManager Instance;

        private NetworkManager networkManager;

        private LoadingPanel loadingPanel;

        private bool IsLoadingScene = false;
        private float LoadingProgress = 0;

        bool IsNetworkSceneManageEnable {
            get {
                return networkManager != null
                    && networkManager.SceneManager != null
                    && networkManager.NetworkConfig.EnableSceneManagement;
            }
        }

        #region NetworkBehaviour
        public void InitWarSceneManager(NetworkManager networkManager) {
            Instance = this;
            this.networkManager = networkManager;

            // ע��ص�
            networkManager.OnServerStarted += OnNetworkSessionStart;
            networkManager.OnClientStarted += OnNetworkSessionStart;

            networkManager.OnServerStopped += OnNetworkSessionEnd;
            networkManager.OnClientStopped += OnNetworkSessionEnd;
        }

        public override void OnDestroy() {
            networkManager.OnServerStarted -= OnNetworkSessionStart;
            networkManager.OnClientStarted -= OnNetworkSessionStart;

            networkManager.OnServerStopped -= OnNetworkSessionEnd;
            networkManager.OnClientStopped -= OnNetworkSessionEnd;
        }

        public void OnNetworkSessionStart() {
            if (IsNetworkSceneManageEnable) {
                networkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            }
        }

        public void OnNetworkSessionEnd(bool IsStopHost) {
            if (IsNetworkSceneManageEnable) {
                networkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
            }
        }

        /// <summary>
        /// ����ת���¼��������첽���س���
        /// </summary>
        /// <param name="sceneEvent"></param>
        public void OnSceneEvent(SceneEvent sceneEvent) {
            
            switch (sceneEvent.SceneEventType) {
                case SceneEventType.Load:
                    // ���ڼ����У�ͬ������
                    //Debug.Log($"{networkManager.LocalClientId} receive load event: {sceneEvent.ClientId}");
                    if (networkManager.IsClient) {
                        ShowLoading();
                        SetLoadingProcess(sceneEvent.AsyncOperation);
                    }
                    break;
                case SceneEventType.LoadComplete:
                    // ���ؽ���
                    //Debug.Log($"{networkManager.LocalClientId} receive load complete: {sceneEvent.ClientId}");
                    if (IsServer) {
                        if (sceneEvent.ClientId == NetworkManager.LocalClientId) {
                            // Handle server side LoadComplete related tasks here
                        } else {
                            // Handle client LoadComplete **server-side** notifications here
                        }
                    } else {
                        // Clients generate thisn'tification locally
                        // Handle client side LoadComplete related tasks here
                    }

                    break;
                case SceneEventType.LoadEventCompleted:
                    // NOTICE: ��һ���ͻ�������Ϻ󣬻ᴥ�����¼�(��Ҳ��֪����ȫ��֪ͨ���ǽ�������)
                    //Debug.Log($"{networkManager.LocalClientId} receive load event complete from: {sceneEvent.ClientId}");
                    
                    // NOTICE: ��ʱ�����Ѿ����뵽�³������ˣ�����Ҫ�ȿ������ؽ���
                    
                    if (networkManager.IsClient && !networkManager.IsServer) {
                        // �ǿͻ��ˣ�ֱ�ӽ���
                        HideLoading();
                    } else {
                        // �Ƿ���������Ҫ����Ƿ�������Ҿ��Ѿ����룬���ܽ���
                        int curCompleteCount = sceneEvent.ClientsThatCompleted.Count + sceneEvent.ClientsThatTimedOut.Count;
                        //Debug.Log("��ǰ�Ѿ���ɼ��ص�����: " + curCompleteCount + ",���ӽ�����������: " + networkManager.ConnectedClientsIds.Count);
                        if (curCompleteCount < networkManager.ConnectedClientsIds.Count) {
                            return;
                        }
                        HideLoading();
                    }

                    /*// ���ڼ�� ����ͬ��
                    string completeStr = "";
                    foreach (var id in sceneEvent.ClientsThatCompleted)
                    {
                        completeStr += id.ToString();
                        completeStr += ", ";
                    }
                    Debug.Log("the clientId complete is: " + completeStr + ", ��ǰ���ӵ������: " + networkManager.ConnectedClientsIds.Count);

                    string timeOutStr = "";
                    foreach (var id in sceneEvent.ClientsThatTimedOut) {
                        timeOutStr += id.ToString();
                        timeOutStr += ", ";
                    }
                    Debug.Log("the clientId time out is: " + timeOutStr);*/

                    
                    break;
                default:
                    break;
            }
        }

        #endregion

        /// <summary>
        /// �첽���س���
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single) {
            if (useNetworkSceneManager) {
                if (IsSpawned && !networkManager.ShutdownInProgress) {
                    if (networkManager.IsServer) {
                        //networkManager.SceneManager.Load : 
                        // If is active server and NetworkManager uses scene management, load scene using NetworkManager's SceneManager
                        networkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                    }
                }
            } else {
                StartCoroutine(LoadScene(sceneName));
            }
        }

        public IEnumerator LoadScene(string sceneName) {
            
            ShowLoading();

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            //�������֮���Ƿ�������ת
            asyncOperation.allowSceneActivation = false;

            while (asyncOperation.progress < 0.9f) {
                SetLoadingProcess(asyncOperation);
                //Debug.Log("��ǰ���س���������" + Instance.LoadingProgress);
                yield return null;
            }

            HideLoading();
            asyncOperation.allowSceneActivation = true;
            yield return null;
        }

        /// <summary>
        /// ��õ�ǰ�ĳ�������
        /// </summary>
        public static string GetActiveScene() {
            return SceneManager.GetActiveScene().name;
        }


        #region չʾ���ؽ���
        public void ShowLoading() {
            if (Instance.loadingPanel == null) {
                try {
                    Instance.loadingPanel = GameObject.FindWithTag("LoadingPanel").GetComponent<LoadingPanel>();
                } catch {
                    //Debug.LogError("û���ڵ�ǰ�����ҵ��������");
                    return;
                }
            }

            if (!loadingPanel.Visible || !loadingPanel.gameObject.activeSelf) {
                // ���ؽ��治���ӣ���򿪼���ҳ��
                Instance.loadingPanel.gameObject.SetActive(true);
                Instance.loadingPanel.Show();
                Instance.IsLoadingScene = true;
            }
        }

        public void SetLoadingProcess(AsyncOperation asyncOperation) {
            Instance.LoadingProgress = asyncOperation.progress;
            Instance.loadingPanel.SetLoadingProcess(1, Instance.LoadingProgress);
        }

        public void HideLoading() {
            if (Instance.loadingPanel == null) {
                try {
                    Instance.loadingPanel = GameObject.FindWithTag("LoadingPanel").GetComponent<LoadingPanel>();
                }catch {
                    //Debug.LogError("û���ڵ�ǰ�����ҵ��������");
                    return;
                }
            }
            
            Instance.IsLoadingScene = false;
            //Instance.loadingPanel.Hide();
        }
        #endregion
    }
}