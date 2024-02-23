using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {

    /// <summary>
    /// �������ӵ���ҵ���Ϣ
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

        #region ���������������
        // �û�player id - player data����
        Dictionary<string, T> ClientDataDic = new Dictionary<string, T>();

        // �û���clentId - playerid����
        Dictionary<ulong, string> ClientIDToPlayerIDDic;

        public string GetPlayerId(ulong clientId) {
            if (ClientIDToPlayerIDDic.TryGetValue(clientId, out string playerId)) {
                return playerId;
            }
            Debug.Log("������Ϣδ�洢��ClientId:" + clientId);
            return null;
        }

        public void PrintAllPlayerData() {
            Debug.Log("���ڴ�ӡ���е����");
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
                // ���ڴ洢�������ҵ�ӳ��
                return GetPlayerData(playerId);
            }
            //Debug.Log("������Ϣδ�洢��ClientId:" + clientId);
            return null;
        }

        public T? GetPlayerData(string playerId) {
            if (ClientDataDic.TryGetValue(playerId, out T playerData)) {
                return playerData;
            }
            //Debug.Log("������Ϣδ�洢��playerId:" + playerId);
            return null;
        }

        /// <summary>
        /// ������ҵ��������ݣ���������������ݣ�����ʹ��SetConnectingPlayerSessionData
        /// </summary>
        public void SetPlayerData(ulong clientId, string playerId, T sessionPlayerData) {
            if (ClientIDToPlayerIDDic.ContainsKey(clientId)) {
                ClientIDToPlayerIDDic[clientId] = playerId;
                // ����clientdatadic
                //Debug.Log("������SetPlayerData:" + playerId + ", �Ѿ��ɹ����");
                if (ClientDataDic.ContainsKey(playerId)) {
                    ClientDataDic[playerId] = sessionPlayerData;
                } else {
                    ClientDataDic.Add(playerId, sessionPlayerData);
                }
            } else {
                ClientIDToPlayerIDDic.Add(clientId, playerId);
                //Debug.Log("������SetPlayerData:" + playerId);
                // ����clientdatadic
                if (ClientDataDic.ContainsKey(playerId)) {
                    ClientDataDic[playerId] = sessionPlayerData;
                } else {
                    ClientDataDic.Add(playerId, sessionPlayerData);
                }
            }

        }

        /// <summary>
        /// ˢ�µ�ǰ������ҵ�����
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
        /// ��������жϿ����������
        /// </summary>
        private void ClearDisconnectedPlayersData() {
            List<ulong> idsToClear = new List<ulong>();
            foreach (var id in ClientIDToPlayerIDDic.Keys) {
                var data = GetPlayerData(id);
                // ���IsConnected�ֶ�Ϊfalse ��������б�
                if (data is { IsConnected: false }) {
                    idsToClear.Add(id);
                }
            }

            foreach (var id in idsToClear) {
                string playerId = ClientIDToPlayerIDDic[id];
                // ����������ӳ����ɾ��
                if (GetPlayerData(playerId)?.ClientID == id) {
                    ClientDataDic.Remove(playerId);
                }

                ClientIDToPlayerIDDic.Remove(id);
            }
        }
        #endregion

        public void SetConnectingPlayerSessionData(ulong clientId, string playerId, T sessionPlayerData) {
            
            // �ÿͻ����Ѿ��лỰ��Ϣ
            if (IsDuplicateConnection(playerId)) {
                return;
            }

            // �лỰ��Ϣ ����û�������ϣ�������
            bool isReconnecting = false;
            if (ClientDataDic.ContainsKey(playerId)) {
                if (!ClientDataDic[playerId].IsConnected) {
                    isReconnecting = true;
                }
            }

            // ������ ��ˢ�µ�ǰ����Ϣ
            if (isReconnecting) {
                sessionPlayerData = ClientDataDic[playerId];
                sessionPlayerData.ClientID = clientId;
                sessionPlayerData.IsConnected = true;
            }

            // ������Ϣ
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

            //Debug.Log("��ǰ��clientData�ļ�ֵ��:" + ClientDataDic.Count);
        }

        /// <summary>
        /// �ж�һ���ͻ����Ƿ��Ѿ��� ����˵� �Ự�а�������Ϣ,���Ѿ�������
        /// </summary>
        public bool IsDuplicateConnection(string playerId) {
            return ClientDataDic.ContainsKey(playerId) && ClientDataDic[playerId].IsConnected;
        }

        /// <summary>
        /// ���ͻ����˳�ʱ���ã������˳�����
        /// </summary>
        public void DisconnectClient(ulong clientId) {
            if (IsSessionStart) {
                // ��ǿͻ���Ϊδ���ӣ������������Թ�����
                if (ClientIDToPlayerIDDic.TryGetValue(clientId, out string playerId)) {
                    if (GetPlayerData(playerId)?.ClientID == clientId) {
                        var clientData = ClientDataDic[playerId];
                        clientData.IsConnected = false;
                        ClientDataDic[playerId] = clientData;
                    }
                }
            } else {
                // �ͻ��˸���û�������� ���ر�������
                if (ClientIDToPlayerIDDic.TryGetValue(clientId, out var playerId)) {
                    ClientIDToPlayerIDDic.Remove(clientId);
                    if (GetPlayerData(playerId)?.ClientID == clientId) {
                        ClientDataDic.Remove(playerId);
                    }
                }
            }
        }

        #region session��������
        /// <summary>
        /// �Ự��ʼʱ�ص�
        /// </summary>
        public void OnSessionStarted() {
            IsSessionStart = true;
        }

        /// <summary>
        /// �Ự������Ļص�
        /// </summary>
        public void OnSessionEnded() {
            // ��նϿ������
            ClearDisconnectedPlayersData();
            // ���³�ʼ���������
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