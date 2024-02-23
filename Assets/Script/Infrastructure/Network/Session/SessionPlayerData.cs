using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.Infrastructure.NetworkPackage {
    public struct SessionPlayerData : ISessionPlayerData {

        public string PlayerName;

        // 玩家当前选择的势力的Tag
        public string PlayerFactionTag;

        // 玩家是否已经连接上
        public bool HasPlayerSpawned;

        public bool IsConnected { get ; set; }
        public ulong ClientID { get; set; }

        public SessionPlayerData(string playerName, string playerFactionTag, bool hasPlayerSpawned, bool isConnected, ulong clientID) {
            PlayerName = playerName;
            PlayerFactionTag = playerFactionTag;
            HasPlayerSpawned = hasPlayerSpawned;
            IsConnected = isConnected;
            ClientID = clientID;
        }

        public void Reinit() {
            HasPlayerSpawned = false;
        }

        public override string ToString() {
            return $"玩家名称:{PlayerName},所选势力:{PlayerFactionTag},连接状况:{HasPlayerSpawned}";
        }
    }
}