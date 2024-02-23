using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.UI;
using WarGame_True.UGS.LobbyPack;

namespace WarGame_True.Infrastructure.NetworkPackage {
    /// <summary>
    /// 连接管理器，建议用ApplicationController管理
    /// </summary>
    public class ConnectionManager : MonoBehaviour {

        public static ConnectionManager Instance;

        [Header("网络管理器")]
        [Tooltip("处于单机状态也会开启")]
        private NetworkManager m_NetworkManager;
        public NetworkManager networkManager { 
            get {
                if(m_NetworkManager == null) {
                    m_NetworkManager = NetworkManager.Singleton;
                }
                return m_NetworkManager;
            }
            set { m_NetworkManager = value; }
        }

        private ConnectionState currentState;

        internal readonly OfflineState offlineState = new OfflineState();
        
        internal readonly HostingState hostingState = new HostingState();
        internal readonly StartHostState startHostState = new StartHostState();
        
        internal readonly ClientConnectedState clientConnectedState = new ClientConnectedState();
        internal readonly ClientConnectingState clientConnectingState = new ClientConnectingState();
        internal readonly ClientReconnectingState clientReconnectingState = new ClientReconnectingState();

        public void InitConnectManager(NetworkManager networkManager) {
            Instance = this;

            this.networkManager = networkManager;

            offlineState.SetConnectionManager(this);
            hostingState.SetConnectionManager(this);
            startHostState.SetConnectionManager(this);
            clientConnectedState.SetConnectionManager(this);
            clientConnectingState.SetConnectionManager(this);
            clientReconnectingState.SetConnectionManager(this);

            currentState = offlineState;

            // 注册回调
            networkManager.OnClientConnectedCallback += OnClientConnectedCall;
            networkManager.OnClientDisconnectCallback += OnClientDisconnectCall;
            networkManager.OnServerStarted += OnServerStart;
            networkManager.OnServerStopped += OnServerStop;
            networkManager.ConnectionApprovalCallback = OnConnectionApproval;
            networkManager.OnTransportFailure += OnTransportFailure;
        }

        private void OnDestroy() {
            // 注销回调
            // TODO: 为什么非要注销掉回调，留着不行吗？
            networkManager.OnClientConnectedCallback -= OnClientConnectedCall;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnectCall;
            networkManager.OnServerStarted -= OnServerStart;
            networkManager.OnServerStopped -= OnServerStop;
            //networkManager.ConnectionApprovalCallback -= OnConnectionApproval;
            networkManager.OnTransportFailure -= OnTransportFailure;
        }

        internal void ChangeState(ConnectionState nextState) {
            Debug.Log($"连接状态已经改变了:{currentState.GetType().Name} to {nextState.GetType().Name}");

            if (currentState != null) {
                currentState.Exit();
            }
            currentState = nextState;
            currentState.Enter();
        }

        #region 外部接口

        public void StartClientIp(string playerName, string ipaddress, int port) {
            currentState.StartClientIP(playerName, ipaddress, port);
        }

        public void StartHostIp(string playerName, string ipaddress, int port) {
            currentState.StartHostIP(playerName, ipaddress, port);
        }

        public void StartClientLobby(string playerName) {
            currentState.StartClientLobby(playerName);
        }

        public void StartHostLobby(string playerName) {
            currentState.StartHostLobby(playerName);
        }

        #endregion

        #region 网络事件回调

        private void OnClientConnectedCall(ulong clientId) {
            //DebugPanel.Instance.LogNotice("客户端连接成功，当前的state:" + currentState.GetType().ToString());
            //Debug.Log("客户端连接成功，当前的state:" + currentState.GetType().ToString());
            currentState.OnClientConnected(clientId);
        }

        private void OnClientDisconnectCall(ulong clientId) {
            //DebugPanel.Instance.LogNotice("客户端断开，当前的state:" + currentState.GetType().ToString());
            //Debug.Log("客户端断开，当前的state:" + currentState.GetType().ToString());
            currentState.OnClientDisconnect(clientId);
        }

        private void OnServerStart() {
            //DebugPanel.Instance.LogNotice("服务器启动，当前的state:" + currentState.GetType().ToString());
            //Debug.Log("服务器启动，当前的state:" + currentState.GetType().ToString());
            currentState.OnServerStarted();
        }

        /// <summary>
        /// 服务器断开回调
        /// </summary>
        /// <param name="IsStopHost">The first parameter of this event will be set to <see cref="true"/> when stopping a host instance and <see cref="false"/> when stopping a server instance.</param>
        private void OnServerStop(bool IsStopHost) {
            //DebugPanel.Instance.LogNotice("服务器断开，当前的state:" + currentState.GetType().ToString());
            //Debug.Log("服务器断开，当前的state:" + currentState.GetType().ToString());
            currentState.OnServerStopped(IsStopHost);
        }

        private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
            // ERROR: 触发不了！
            // 1.5 NOTICE: 在NetworkManager单例中 打开Approval 即可使此函数有效，但打开后默认需要经过验证才能连接，较为麻烦
            //DebugPanel.Instance.LogNotice("Approval 事件!，当前的state:" + currentState.GetType().ToString());
            //Debug.Log("Approval 事件!，当前的state:" + currentState.GetType().ToString());
            currentState.OnConnectApproval(request, response);
        }

        private void OnTransportFailure() {
            //DebugPanel.Instance.LogNotice("传输失败，当前的state:" + currentState.GetType().ToString());
            Debug.Log("传输失败，当前的state:" + currentState.GetType().ToString());
            currentState.OnTransportFailure();
        }
        #endregion
    }

    [Serializable]
    public class ConnectionPayload {
        //payload:翻译过来是有效载荷
        //通常在传输数据时，为了使数据传输更可靠，要把原始数据分批传输，
        //并且在每一批数据的头和尾都加上一定的辅助信息，比如数据量的大小、校验位等，
        //这样就相当于给已经分批的原始数据加一些外套，这些外套起标示作用，
        //使得原始数据不易丢失，一批数据加上“外套”就形成了传输通道的基本传输单元，
        //叫做数据帧或数据包，而其中的原始数据就是payload

        public string playerId;

        public string playerName;

        public bool isDebug;
    }
}