using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace WarGame_True.UGS.LobbyPack {
    /// <summary>
    /// 本地的Lobby用户数据
    /// </summary>
    public class LocalLobbyPlayer {

        // LocalUser 属性值改变时的回调
        public event Action<LocalLobbyPlayer> changed;

        #region 玩家的所有信息
        PlayerData playerData;
        public bool IsHost {
            get { return playerData.IsHost; }
            set {
                if (playerData.IsHost != value) {
                    playerData.IsHost = value;
                    LastChangedAttribute = UserMembers.IsHost;
                    // 字段发生改变时 调用changed回调
                    OnChanged();
                }
            }
        }
        public string DisplayName {
            get => playerData.DisplayName;
            set {
                if (playerData.DisplayName != value) {
                    playerData.DisplayName = value;
                    LastChangedAttribute = UserMembers.DisplayName;
                    OnChanged();
                }
            }
        }
        // Q: 这里的ID，是否等于AuthenticationService.Instance.PlayerId;
        public string ID {
            get => playerData.ID;
            set {
                if (playerData.ID != value) {
                    playerData.ID = value;
                    LastChangedAttribute = UserMembers.ID;
                    OnChanged();
                }
            }
        }
        [Flags] public enum UserMembers {
            // 附上Flags特性后，该枚举变量可以进行按位“|”“&”操作
            IsHost = 1,
            DisplayName = 2,
            ID = 4,
        }

        // 最近一次改变 的属性
        UserMembers LastChangedAttribute;

        void OnChanged() {
            changed?.Invoke(this);
        }

        #endregion

        public LocalLobbyPlayer() {
            ResetPlayerData();
        }

        /// <summary>
        /// 重置该玩家的数据
        /// </summary>
        public void ResetPlayerData() {
            playerData = new PlayerData(false, playerData.DisplayName, playerData.ID);
        }

        public void CopyDataFrom(LocalLobbyPlayer lobby) {
            playerData = lobby.playerData;
            LastChangedAttribute = (UserMembers)lobby.LastChangedAttribute;

            OnChanged();
        }

        public Dictionary<string, PlayerDataObject> GetDataForLobbyService() =>
            new Dictionary<string, PlayerDataObject>()
            {
                {"DisplayName", new PlayerDataObject(
                    PlayerDataObject.VisibilityOptions.Member, 
                    DisplayName)
                },
            };

    }

    /// <summary>
    /// 封装加入大厅后的 玩家的数据
    /// </summary>
    public struct PlayerData {
        public bool IsHost { get; set; }
        public string DisplayName { get; set; }
        public string ID { get; set; }

        public PlayerData(bool isHost, string displayName, string id) {
            IsHost = isHost;
            DisplayName = displayName;
            ID = id;
        }
    }
}