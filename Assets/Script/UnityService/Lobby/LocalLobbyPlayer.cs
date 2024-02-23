using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace WarGame_True.UGS.LobbyPack {
    /// <summary>
    /// ���ص�Lobby�û�����
    /// </summary>
    public class LocalLobbyPlayer {

        // LocalUser ����ֵ�ı�ʱ�Ļص�
        public event Action<LocalLobbyPlayer> changed;

        #region ��ҵ�������Ϣ
        PlayerData playerData;
        public bool IsHost {
            get { return playerData.IsHost; }
            set {
                if (playerData.IsHost != value) {
                    playerData.IsHost = value;
                    LastChangedAttribute = UserMembers.IsHost;
                    // �ֶη����ı�ʱ ����changed�ص�
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
        // Q: �����ID���Ƿ����AuthenticationService.Instance.PlayerId;
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
            // ����Flags���Ժ󣬸�ö�ٱ������Խ��а�λ��|����&������
            IsHost = 1,
            DisplayName = 2,
            ID = 4,
        }

        // ���һ�θı� ������
        UserMembers LastChangedAttribute;

        void OnChanged() {
            changed?.Invoke(this);
        }

        #endregion

        public LocalLobbyPlayer() {
            ResetPlayerData();
        }

        /// <summary>
        /// ���ø���ҵ�����
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
    /// ��װ���������� ��ҵ�����
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