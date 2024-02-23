using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {

    /// <summary>
    /// 处于连接的玩家的信息
    /// </summary>
    public interface ISessionPlayerData {
        bool IsConnected { get; set; }
        ulong ClientID { get; set; }
        void Reinit();
    }

    public class SessionManager<T> where T : struct, ISessionPlayerData {
        
        private static SessionManager<T> instance;
        public static SessionManager<T> Instance { 
            get {
                if (instance == null)
                {
                    instance = new SessionManager<T>();
                    instance.ClientDataDic = new Dictionary<string, T>();
                    instance.ClientIDToPlayerIDDic = new Dictionary<ulong, string>();
                }
                return instance;
            } 
        }


        bool IsSessionStart;

        #region 管理玩家连接数据
        // 用户player id - player data数据
        Dictionary<string, T> ClientDataDic = new Dictionary<string, T>();

        // 用户的clentId - playerid数据
        Dictionary<ulong, string> ClientIDToPlayerIDDic;

        public string GetPlayerId(ulong clientId) {
            if (ClientIDToPlayerIDDic.TryGetValue(clientId, out string playerId)) {
                return playerId;
            }
            Debug.Log("连接信息未存储该ClientId:" + clientId);
            return null;
        }

        public void PrintAllPlayerData() {
            Debug.Log("现在打印所有的玩家");
            //foreach (var pair in ClientIDToPlayerIDDic)
            //{
            //    Debug.Log("player clientID:" + pair.Key + ", player ID:" + pair.Value);
            //}
            foreach (var pair in ClientDataDic) {
                Debug.Log("player ID:" + pair.Key + ", player Data:" + pair.Value.ToString());
            }
        }

        public T? GetPlayerData(ulong clientId) {
            //Debug.Log(ClientIDToPlayerIDDic[clientId]);
            if (ClientIDToPlayerIDDic.TryGetValue(clientId, out string playerId)) {
                // 能在存储数据中找到映射
                return GetPlayerData(playerId);
            }
            //Debug.Log("连接信息未存储该ClientId:" + clientId);
            return null;
        }

        public T? GetPlayerData(string playerId) {
            if (ClientDataDic.TryGetValue(playerId, out T playerData)) {
                return playerData;
            }
            //Debug.Log("连接信息未存储该playerId:" + playerId);
            return null;
        }

        /// <summary>
        /// 更新玩家的连接数据，如果是添加玩家数据，建议使用SetConnectingPlayerSessionData
        /// </summary>
        public void SetPlayerData(ulong clientId, string playerId, T sessionPlayerData) {
            if (ClientIDToPlayerIDDic.ContainsKey(clientId)) {
                ClientIDToPlayerIDDic[clientId] = playerId;
                // 更新clientdatadic
                //Debug.Log("触发了SetPlayerData:" + playerId + ", 已经成功添加");
                if (ClientDataDic.ContainsKey(playerId)) {
                    ClientDataDic[playerId] = sessionPlayerData;
                } else {
                    ClientDataDic.Add(playerId, sessionPlayerData);
                }
            } else {
                ClientIDToPlayerIDDic.Add(clientId, playerId);
                //Debug.Log("触发了SetPlayerData:" + playerId);
                // 更新clientdatadic
                if (ClientDataDic.ContainsKey(playerId)) {
                    ClientDataDic[playerId] = sessionPlayerData;
                } else {
                    ClientDataDic.Add(playerId, sessionPlayerData);
                }
            }

        }

        /// <summary>
        /// 刷新当前所有玩家的数据
        /// </summary>
        private void ReinitPlayersData() {
            foreach (var id in ClientIDToPlayerIDDic.Keys) {
                string playerId = ClientIDToPlayerIDDic[id];
                T sessionPlayerData = ClientDataDic[playerId];
                sessionPlayerData.Reinit();
                ClientDataDic[playerId] = sessionPlayerData;
            }
        }

        /// <summary>
        /// 清除掉所有断开的玩家数据
        /// </summary>
        private void ClearDisconnectedPlayersData() {
            List<ulong> idsToClear = new List<ulong>();
            foreach (var id in ClientIDToPlayerIDDic.Keys) {
                var data = GetPlayerData(id);
                // 如果IsConnected字段为false 加入清楚列表
                if (data is { IsConnected: false }) {
                    idsToClear.Add(id);
                }
            }

            foreach (var id in idsToClear) {
                string playerId = ClientIDToPlayerIDDic[id];
                // 从两个数据映射中删除
                if (GetPlayerData(playerId)?.ClientID == id) {
                    ClientDataDic.Remove(playerId);
                }

                ClientIDToPlayerIDDic.Remove(id);
            }
        }
        #endregion

        public void SetConnectingPlayerSessionData(ulong clientId, string playerId, T sessionPlayerData) {
            
            // 该客户端已经有会话信息
            if (IsDuplicateConnection(playerId)) {
                return;
            }

            // 有会话信息 但是没有连接上，是重连
            bool isReconnecting = false;
            if (ClientDataDic.ContainsKey(playerId)) {
                if (!ClientDataDic[playerId].IsConnected) {
                    isReconnecting = true;
                }
            }

            // 是重连 则刷新当前的信息
            if (isReconnecting) {
                sessionPlayerData = ClientDataDic[playerId];
                sessionPlayerData.ClientID = clientId;
                sessionPlayerData.IsConnected = true;
            }

            // 更新信息
            if (ClientIDToPlayerIDDic.ContainsKey(clientId)) {
                ClientIDToPlayerIDDic[clientId] = playerId;
            } else {
                ClientIDToPlayerIDDic.Add(clientId, playerId);
            }

            if (ClientDataDic.ContainsKey(playerId)) {
                ClientDataDic[playerId] = sessionPlayerData;
            } else {
                ClientDataDic.Add(playerId, sessionPlayerData);
            }

            //Debug.Log("当前的clientData的键值对:" + ClientDataDic.Count);
        }

        /// <summary>
        /// 判断一个客户端是否已经在 服务端的 会话中包含有信息,且已经连接上
        /// </summary>
        public bool IsDuplicateConnection(string playerId) {
            return ClientDataDic.ContainsKey(playerId) && ClientDataDic[playerId].IsConnected;
        }

        /// <summary>
        /// 当客户端退出时调用，处理退出过程
        /// </summary>
        public void DisconnectClient(ulong clientId) {
            if (IsSessionStart) {
                // 标记客户端为未连接，保存其数据以供重连
                if (ClientIDToPlayerIDDic.TryGetValue(clientId, out string playerId)) {
                    if (GetPlayerData(playerId)?.ClientID == clientId) {
                        var clientData = ClientDataDic[playerId];
                        clientData.IsConnected = false;
                        ClientDataDic[playerId] = clientData;
                    }
                }
            } else {
                // 客户端根本没有连接上 不必保存数据
                if (ClientIDToPlayerIDDic.TryGetValue(clientId, out var playerId)) {
                    ClientIDToPlayerIDDic.Remove(clientId);
                    if (GetPlayerData(playerId)?.ClientID == clientId) {
                        ClientDataDic.Remove(playerId);
                    }
                }
            }
        }

        #region session生命周期
        /// <summary>
        /// 会话开始时回调
        /// </summary>
        public void OnSessionStarted() {
            IsSessionStart = true;
        }

        /// <summary>
        /// 会话结束后的回调
        /// </summary>
        public void OnSessionEnded() {
            // 清空断开的玩家
            ClearDisconnectedPlayersData();
            // 重新初始化玩家数据
            ReinitPlayersData();
            IsSessionStart = false;
        }

        /// <summary>
        /// Resets all our runtime state, so it is ready to be reinitialized when starting a new server
        /// </summary>
        public void OnServerEnded() {
            ClientDataDic.Clear();
            ClientIDToPlayerIDDic.Clear();
            IsSessionStart = false;
        }

        #endregion

    }
}