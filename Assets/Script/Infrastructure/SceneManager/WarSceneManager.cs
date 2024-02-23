using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.Infrastructure.SceneUtil {
    /// <summary>
    /// 专门用于管理场景加载的组件，使用时调用接口，需要挂接到一个物体上并进行初始化
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

            // 注册回调
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
        /// 场景转换事件，用于异步加载场景
        /// </summary>
        /// <param name="sceneEvent"></param>
        public void OnSceneEvent(SceneEvent sceneEvent) {
            
            switch (sceneEvent.SceneEventType) {
                case SceneEventType.Load:
                    // 正在加载中，同步进度
                    //Debug.Log($"{networkManager.LocalClientId} receive load event: {sceneEvent.ClientId}");
                    if (networkManager.IsClient) {
                        ShowLoading();
                        SetLoadingProcess(sceneEvent.AsyncOperation);
                    }
                    break;
                case SceneEventType.LoadComplete:
                    // 加载结束
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
                    // NOTICE: 当一个客户加载完毕后，会触发此事件(我也不知道是全部通知还是仅服务器)
                    //Debug.Log($"{networkManager.LocalClientId} receive load event complete from: {sceneEvent.ClientId}");
                    
                    // NOTICE: 此时可能已经进入到新场景里了，所以要先开启加载界面
                    
                    if (networkManager.IsClient && !networkManager.IsServer) {
                        // 是客户端，直接进入
                        HideLoading();
                    } else {
                        // 是服务器，需要检查是否所有玩家均已经加入，才能进入
                        int curCompleteCount = sceneEvent.ClientsThatCompleted.Count + sceneEvent.ClientsThatTimedOut.Count;
                        //Debug.Log("当前已经完成加载的人数: " + curCompleteCount + ",连接进入的玩家总数: " + networkManager.ConnectedClientsIds.Count);
                        if (curCompleteCount < networkManager.ConnectedClientsIds.Count) {
                            return;
                        }
                        HideLoading();
                    }

                    /*// 用于检查 场景同步
                    string completeStr = "";
                    foreach (var id in sceneEvent.ClientsThatCompleted)
                    {
                        completeStr += id.ToString();
                        completeStr += ", ";
                    }
                    Debug.Log("the clientId complete is: " + completeStr + ", 当前连接的玩家数: " + networkManager.ConnectedClientsIds.Count);

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
        /// 异步加载场景
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
            //加载完成之后是否允许跳转
            asyncOperation.allowSceneActivation = false;

            while (asyncOperation.progress < 0.9f) {
                SetLoadingProcess(asyncOperation);
                //Debug.Log("当前加载场景进度是" + Instance.LoadingProgress);
                yield return null;
            }

            HideLoading();
            asyncOperation.allowSceneActivation = true;
            yield return null;
        }

        /// <summary>
        /// 获得当前的场景名称
        /// </summary>
        public static string GetActiveScene() {
            return SceneManager.GetActiveScene().name;
        }


        #region 展示加载界面
        public void ShowLoading() {
            if (Instance.loadingPanel == null) {
                try {
                    Instance.loadingPanel = GameObject.FindWithTag("LoadingPanel").GetComponent<LoadingPanel>();
                } catch {
                    //Debug.LogError("没有在当前场景找到加载面板");
                    return;
                }
            }

            if (!loadingPanel.Visible || !loadingPanel.gameObject.activeSelf) {
                // 加载界面不可视，则打开加载页面
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
                    //Debug.LogError("没有在当前场景找到加载面板");
                    return;
                }
            }
            
            Instance.IsLoadingScene = false;
            //Instance.loadingPanel.Hide();
        }
        #endregion
    }
}