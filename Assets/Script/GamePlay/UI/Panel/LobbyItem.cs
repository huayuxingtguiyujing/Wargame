using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WarGame_True.UGS.LobbyPack;

namespace WarGame_True.GamePlay.UI {
    public class LobbyItem : MonoBehaviour {

        [SerializeField] Button LobbyButton;

        [Header("房间信息")]
        [SerializeField] TMP_Text LobbyName;
        [SerializeField] TMP_Text LobbyPeopleNum;
        [SerializeField] TMP_Text LobbyCreatorName;
        [SerializeField] TMP_Text LobbyIsPrivate;

        public delegate void LobbyItemDelegate(string joinCode, string relayCode);
        private LobbyItemDelegate ClickLobbyItemEvent;

        private string lobbyJoinCode;
        private string lobbyRelayJoinCode;

        public void InitLobbyItem(LocalLobby lobbyData, LobbyItemDelegate ClickLobbyItemEvent) {
            LobbyName.text = lobbyData.LobbyName;
            LobbyPeopleNum.text = $"{lobbyData.PlayerCount}/{lobbyData.MaxPlayerCount}";
            if (lobbyData.Private) {
                LobbyIsPrivate.text = "私有";
            } else {
                LobbyIsPrivate.text = "公开";
            }

            lobbyJoinCode = lobbyData.LobbyCode;
            lobbyRelayJoinCode = lobbyData.RelayJoinCode;

            this.ClickLobbyItemEvent = ClickLobbyItemEvent;
            LobbyButton.onClick.AddListener(ClickLobbyItem);
        }

        private void ClickLobbyItem() {
            ClickLobbyItemEvent.Invoke(lobbyJoinCode, lobbyRelayJoinCode);
        }

    }
}