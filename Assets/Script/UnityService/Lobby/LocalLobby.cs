using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace WarGame_True.UGS.LobbyPack {
    /// <summary>
    /// ��������(����)������
    /// </summary>
    public class LocalLobby {

        // �������Ըı�ʱ���¼��ص�
        public event Action<LocalLobby> changed;

        // ��ǰ������� �������

        Dictionary<string, LocalLobbyPlayer> lobbyUsers = new Dictionary<string, LocalLobbyPlayer>();
        public Dictionary<string, LocalLobbyPlayer> LobbyUsers => lobbyUsers;


        #region ���� �����������ֶ�
        public LobbyData LobbyData;

        public string LobbyID {
            get => LobbyData.LobbyID;
            set {
                LobbyData.LobbyID = value;
                OnChanged();
            }
        }

        public string LobbyCode {
            get => LobbyData.LobbyCode;
            set {
                LobbyData.LobbyCode = value;
                OnChanged();
            }
        }

        public string RelayJoinCode {
            get => LobbyData.RelayJoinCode;
            set {
                LobbyData.RelayJoinCode = value;
                OnChanged();
            }
        }

        public string LobbyName {
            get => LobbyData.LobbyName;
            set {
                LobbyData.LobbyName = value;
                OnChanged();
            }
        }

        public bool Private {
            get => LobbyData.Private;
            set {
                LobbyData.Private = value;
                OnChanged();
            }
        }

        public int PlayerCount => LobbyUsers.Count;

        public int MaxPlayerCount {
            get => LobbyData.MaxPlayerCount;
            set {
                LobbyData.MaxPlayerCount = value;
                OnChanged();
            }
        }

        #endregion


        #region ����������ӡ�ɾ����ң�

        /// <summary>
        /// Ϊ����������
        /// </summary>
        public void AddUser(LocalLobbyPlayer user) {
            if (!lobbyUsers.ContainsKey(user.ID)) {
                DoAddUser(user);
                OnChanged();
            }
        }

        private void DoAddUser(LocalLobbyPlayer user) {
            // ����ӳ���У�����player��changed�¼�
            lobbyUsers.Add(user.ID, user);
            user.changed += OnChangedUser;
        }

        /// <summary>
        /// ɾ�������е����
        /// </summary>
        public void RemoveUser(LocalLobbyPlayer user) {
            DoRemoveUser(user);
            OnChanged();
        }

        private void DoRemoveUser(LocalLobbyPlayer user) {
            if (!lobbyUsers.ContainsKey(user.ID)) {
                Debug.LogWarning($"Player {user.DisplayName}({user.ID}) does not exist in lobby: {LobbyID}");
                return;
            }
            // ��ӳ����ɾ����ɾȥplayer��changed�¼�
            lobbyUsers.Remove(user.ID);
            user.changed -= OnChangedUser;
        }

        private void OnChanged() {
            changed?.Invoke(this);
        }

        private void OnChangedUser(LocalLobbyPlayer user) {
            OnChanged();
        }

        #endregion

        /// <summary>
        /// ���ݲ�ѯ�Ľ�� ����LocalLobby���б�.
        /// </summary>
        public static List<LocalLobby> CreateLocalLobbies(QueryResponse response) {
            var retLst = new List<LocalLobby>();
            foreach (var lobby in response.Results) {
                retLst.Add(CreateLocalLobby(lobby));
            }
            return retLst;
        }

        public static LocalLobby CreateLocalLobby(Lobby lobby) {
            var data = new LocalLobby();
            data.ApplyRemoteData(lobby);
            return data;
        }

        public LocalLobby() {
            CopyDataFrom(new LobbyData(), new Dictionary<string, LocalLobbyPlayer>());
        }

        /// <summary>
        /// ���ݴ�����Lobby���ݸ��±����������
        /// </summary>
        /// <param name="lobby"></param>
        public void ApplyRemoteData(Lobby lobby) {
            // ����ԭ����lobby���� �������ص�Lobby����
            LobbyData info = new LobbyData();
            info.LobbyID = lobby.Id;
            info.LobbyCode = lobby.LobbyCode;
            info.Private = lobby.IsPrivate;
            info.LobbyName = lobby.Name;
            info.MaxPlayerCount = lobby.MaxPlayers;

            // RelayJoinCode��lobby��data������
            if (lobby.Data != null) {
                info.RelayJoinCode = lobby.Data.ContainsKey("RelayJoinCode") ? lobby.Data["RelayJoinCode"].Value : null; // By providing RelayCode through the lobby data with Member visibility, we ensure a client is connected to the lobby before they could attempt a relay connection, preventing timing issues between them.
            } else {
                info.RelayJoinCode = null;
            }

            // ������������ҵ�ӳ��
            var lobbyPlayers = new Dictionary<string, LocalLobbyPlayer>();
            foreach (var player in lobby.Players) {
                // �����Ѿ����ڵ� player
                if (player.Data != null) {
                    if (LobbyUsers.ContainsKey(player.Id)) {
                        lobbyPlayers.Add(player.Id, LobbyUsers[player.Id]);
                        continue;
                    }
                }

                // ���lobby���������ݵ�������
                var incomingData = new LocalLobbyPlayer {
                    IsHost = lobby.HostId.Equals(player.Id),
                    DisplayName = player.Data?.ContainsKey("DisplayName") == true 
                    ? player.Data["DisplayName"].Value 
                    : default,
                    ID = player.Id
                };
                lobbyPlayers.Add(incomingData.ID, incomingData);
            }

            CopyDataFrom(info, lobbyPlayers);
        }

        /// <summary>
        /// ���ݴ���������������ݣ��൱�ڸ��ƹ��캯��
        /// </summary>
        public void CopyDataFrom(LobbyData data, Dictionary<string, LocalLobbyPlayer> currPlayer) {
            LobbyData = data;

            if (currPlayer == null) {
                // �����ӳ��Ϊ��
                lobbyUsers = new Dictionary<string, LocalLobbyPlayer>();
            } else {
                List<LocalLobbyPlayer> toRemove = new List<LocalLobbyPlayer>();
                
                foreach (var oldUser in lobbyUsers) {
                    if (currPlayer.ContainsKey(oldUser.Key)) {
                        // ��������û������ϣ���������û�����
                        oldUser.Value.CopyDataFrom(currPlayer[oldUser.Key]);
                    } else {
                        // �����������Ƴ��б�
                        toRemove.Add(oldUser.Value);
                    }
                }

                // ��ԭ�ȵ��û�ȫ���Ƴ�
                foreach (var remove in toRemove) {
                    DoRemoveUser(remove);
                }

                // ��������û�
                foreach (var currUser in currPlayer) {
                    if (!lobbyUsers.ContainsKey(currUser.Key)) {
                        DoAddUser(currUser.Value);
                    }
                }
            }

            OnChanged();
        }

        /// <summary>
        /// ���÷��������
        /// </summary>
        public void ResetLobby(LocalLobbyPlayer localLobbyPlayer) {
            CopyDataFrom(new LobbyData(), new Dictionary<string, LocalLobbyPlayer>());
            AddUser(localLobbyPlayer);
        }

        public Dictionary<string, DataObject> GetDataForUnityServices() =>
            new Dictionary<string, DataObject>()
            {
                {"RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public,  RelayJoinCode)}
            };

    }

    /// <summary>
    /// ������������ݽṹ
    /// </summary>
    public struct LobbyData {
        // ����ID
        public string LobbyID { get; set; }
        // ����������
        public string LobbyCode { get; set; }
        // Relay���ļ�����
        public string RelayJoinCode { get; set; }
        
        // ��������
        public string LobbyName { get; set; }
        public bool Private { get; set; }
        public int MaxPlayerCount { get; set; }

        public LobbyData(LobbyData existing) {
            LobbyID = existing.LobbyID;
            LobbyCode = existing.LobbyCode;
            RelayJoinCode = existing.RelayJoinCode;
            LobbyName = existing.LobbyName;
            Private = existing.Private;
            MaxPlayerCount = existing.MaxPlayerCount;
        }

        public LobbyData(string lobbyCode) {
            LobbyID = null;
            LobbyCode = lobbyCode;
            RelayJoinCode = null;
            LobbyName = null;
            Private = false;
            MaxPlayerCount = -1;
        }
    }
}