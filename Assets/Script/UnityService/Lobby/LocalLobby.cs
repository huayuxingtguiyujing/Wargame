using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace WarGame_True.UGS.LobbyPack {
    /// <summary>
    /// 单个大厅(房间)的数据
    /// </summary>
    public class LocalLobby {

        // 房间属性改变时的事件回调
        public event Action<LocalLobby> changed;

        // 当前房间里的 所有玩家

        Dictionary<string, LocalLobbyPlayer> lobbyUsers = new Dictionary<string, LocalLobbyPlayer>();
        public Dictionary<string, LocalLobbyPlayer> LobbyUsers => lobbyUsers;


        #region 房间 的所有数据字段
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


        #region 房间管理（增加、删除玩家）

        /// <summary>
        /// 为大厅添加玩家
        /// </summary>
        public void AddUser(LocalLobbyPlayer user) {
            if (!lobbyUsers.ContainsKey(user.ID)) {
                DoAddUser(user);
                OnChanged();
            }
        }

        private void DoAddUser(LocalLobbyPlayer user) {
            // 加入映射中，设置player的changed事件
            lobbyUsers.Add(user.ID, user);
            user.changed += OnChangedUser;
        }

        /// <summary>
        /// 删除大厅中的玩家
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
            // 从映射中删除，删去player的changed事件
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
        /// 根据查询的结果 创建LocalLobby的列表.
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
        /// 根据传来的Lobby数据更新本房间的数据
        /// </summary>
        /// <param name="lobby"></param>
        public void ApplyRemoteData(Lobby lobby) {
            // 根据原生的lobby数据 创建本地的Lobby数据
            LobbyData info = new LobbyData();
            info.LobbyID = lobby.Id;
            info.LobbyCode = lobby.LobbyCode;
            info.Private = lobby.IsPrivate;
            info.LobbyName = lobby.Name;
            info.MaxPlayerCount = lobby.MaxPlayers;

            // RelayJoinCode在lobby的data部分中
            if (lobby.Data != null) {
                info.RelayJoinCode = lobby.Data.ContainsKey("RelayJoinCode") ? lobby.Data["RelayJoinCode"].Value : null; // By providing RelayCode through the lobby data with Member visibility, we ensure a client is connected to the lobby before they could attempt a relay connection, preventing timing issues between them.
            } else {
                info.RelayJoinCode = null;
            }

            // 创建房间内玩家的映射
            var lobbyPlayers = new Dictionary<string, LocalLobbyPlayer>();
            foreach (var player in lobby.Players) {
                // 更新已经存在的 player
                if (player.Data != null) {
                    if (LobbyUsers.ContainsKey(player.Id)) {
                        lobbyPlayers.Add(player.Id, LobbyUsers[player.Id]);
                        continue;
                    }
                }

                // 添加lobby里的玩家数据到本房间
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
        /// 根据传入的数据重置数据，相当于复制构造函数
        /// </summary>
        public void CopyDataFrom(LobbyData data, Dictionary<string, LocalLobbyPlayer> currPlayer) {
            LobbyData = data;

            if (currPlayer == null) {
                // 传入的映射为空
                lobbyUsers = new Dictionary<string, LocalLobbyPlayer>();
            } else {
                List<LocalLobbyPlayer> toRemove = new List<LocalLobbyPlayer>();
                
                foreach (var oldUser in lobbyUsers) {
                    if (currPlayer.ContainsKey(oldUser.Key)) {
                        // 如果在新用户名单上，则更新老用户数据
                        oldUser.Value.CopyDataFrom(currPlayer[oldUser.Key]);
                    } else {
                        // 否则就移入待移除列表
                        toRemove.Add(oldUser.Value);
                    }
                }

                // 将原先的用户全部移除
                foreach (var remove in toRemove) {
                    DoRemoveUser(remove);
                }

                // 添加上新用户
                foreach (var currUser in currPlayer) {
                    if (!lobbyUsers.ContainsKey(currUser.Key)) {
                        DoAddUser(currUser.Value);
                    }
                }
            }

            OnChanged();
        }

        /// <summary>
        /// 重置房间的数据
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
    /// 单个房间的数据结构
    /// </summary>
    public struct LobbyData {
        // 房间ID
        public string LobbyID { get; set; }
        // 房间邀请码
        public string LobbyCode { get; set; }
        // Relay包的加入码
        public string RelayJoinCode { get; set; }
        
        // 房间名称
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