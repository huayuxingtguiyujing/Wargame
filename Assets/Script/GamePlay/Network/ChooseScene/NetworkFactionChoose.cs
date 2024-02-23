using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.GameState;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.GamePlay.NetworkPack.ChooseScene {
    public class NetworkFactionChoose : NetworkBehaviour {

        // 用于记录当前选择的势力
        public FactionInfo curChooseRec;

        public void InitNetworkFactionChoose() {
            PlayerChooseStates = new NetworkList<PlayerChooseState>();
        }

        #region 当前参与联网的玩家
        // TODO: 需要适时刷新玩家列表，去除离线的玩家
        public NetworkList<PlayerChooseState> PlayerChooseStates;

        /// <summary>
        /// 是否所有的玩家都已经选择了势力
        /// </summary>
        public bool HasAllPlayerChooseTag() {
            bool ans = true;
            foreach (PlayerChooseState state in PlayerChooseStates)
            {
                //Debug.Log(state.ToString());
                ans = ans && state.HasChooseValidTag();
            }
            return ans;
        }

        public PlayerChooseState GetPlayerById(ulong clientId) {
            for (int i = 0; i < PlayerChooseStates.Count; i++) {
                if (PlayerChooseStates[i].ClientId == clientId) {
                    return PlayerChooseStates[i];
                }
            }
            return PlayerChooseState.GetDefaultPlayerChooseState();
        }

        public PlayerChooseState GetPlayerByTag(string factionTag) {
            for (int i = 0; i < PlayerChooseStates.Count; i++) {
                if (PlayerChooseStates[i].PlayerFactionTag == factionTag) {
                    return PlayerChooseStates[i];
                }
            }
            return PlayerChooseState.GetDefaultPlayerChooseState();
        }

        /// <summary>
        /// 是否已有玩家选择该tag
        /// </summary>
        public bool HasPlayerChooseTag(string factionTag) {
            for (int i = 0; i < PlayerChooseStates.Count; i++) {
                if (PlayerChooseStates[i].PlayerFactionTag == factionTag) {
                    return true;
                }
            }
            return false;
        }

        public string GetPlayerCurChooseTag(ulong clientId) {
            return GetPlayerById(clientId).PlayerFactionTag;
        }

        /// <summary>
        /// 判断一个Tag是否是闲置的，玩家是否可以选择
        /// </summary>
        public bool CanPlayerChooseTag(ulong clientId, string seatTag) {
            foreach (PlayerChooseState playerState in PlayerChooseStates) {
                if (playerState.ClientId == clientId) {
                    continue;
                }

                // 已经有玩家在该位置上
                if (playerState.PlayerFactionTag == seatTag) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 新玩家加入时调用
        /// </summary>
        public void NewPlayerJoinIn(ulong clientId) {
            // 在玩家列表中找到了,结束添加新用户
            if (GetPlayerById(clientId).ClientId == clientId) {
                return;
            }

            string PlayerId = "player" + (PlayerChooseStates.Count + 1).ToString();

            // 为玩家挑选颜色
            Color PlayerColor = Color.white;
            switch (clientId) {
                case 0:
                    PlayerColor = Color.red;
                    break;
                case 1:
                    PlayerColor = Color.blue;
                    break;
                case 2:
                    PlayerColor = Color.green;
                    break;
                case 3:
                    PlayerColor = Color.yellow;
                    break;
                case 4:
                    PlayerColor = new Color(1, 1, 0);
                    break;
                default:
                    break;
            }
            PlayerChooseStates.Add(new PlayerChooseState(
                clientId, PlayerId, "nofaction", Time.time, PlayerColor
            ));
            Debug.Log("有新的玩家加入! ClientId为:" + clientId + ",当前玩家数目:" + PlayerChooseStates.Count);
        }

        // 有玩家，该回调仅在客户端作出注册
        public event Action<ulong, string, bool> OnClientChangeChooseTag;

        public event Action OnSetPlayerFaction;

        [ServerRpc(RequireOwnership = false)]
        public void ChangeChooseTagServerRpc(ulong clientId, string factionTag, bool lockedIn) {
            //Debug.Log("serverRpc 启动: " + clientId);
            // TODO: 写个锁定功能
            OnClientChangeChooseTag.Invoke(clientId, factionTag, lockedIn);
        }


        // TODO: 需要考虑清理断线玩家,在合适的地方调用


        // NOTICE: RequireOwnership字段的意思是，该方法是否只能被这个脚本调用，如果为false，可以在外界调用
        [ServerRpc(RequireOwnership = false)]
        public void PlayerRequestJoinServerRpc() {
            // 是服务器端，整理所有加入的玩家的信息
            IReadOnlyDictionary<ulong, NetworkClient> curClientDic = NetworkManager.Singleton.ConnectedClients;
            foreach (var keyValuePair in curClientDic) {
                // NOTICE: 客户端不能修改服务器数据
                //Debug.Log("当前遍历到的玩家:" + keyValuePair.Key + "," + keyValuePair.Value.PlayerObject.name);
                NewPlayerJoinIn(keyValuePair.Key);
            }
            //Debug.Log("当前所有玩家数目:" + PlayerChooseStates.Count);
        }

        /// <summary>
        /// 设置客户端玩家所选择的faction
        /// </summary>
        [ClientRpc]
        public void SetPlayerFactionClientRpc() {
            OnSetPlayerFaction?.Invoke();
        }

        #endregion

    }

}