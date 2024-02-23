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

        // ���ڼ�¼��ǰѡ�������
        public FactionInfo curChooseRec;

        public void InitNetworkFactionChoose() {
            PlayerChooseStates = new NetworkList<PlayerChooseState>();
        }

        #region ��ǰ�������������
        // TODO: ��Ҫ��ʱˢ������б�ȥ�����ߵ����
        public NetworkList<PlayerChooseState> PlayerChooseStates;

        /// <summary>
        /// �Ƿ����е���Ҷ��Ѿ�ѡ��������
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
        /// �Ƿ��������ѡ���tag
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
        /// �ж�һ��Tag�Ƿ������õģ�����Ƿ����ѡ��
        /// </summary>
        public bool CanPlayerChooseTag(ulong clientId, string seatTag) {
            foreach (PlayerChooseState playerState in PlayerChooseStates) {
                if (playerState.ClientId == clientId) {
                    continue;
                }

                // �Ѿ�������ڸ�λ����
                if (playerState.PlayerFactionTag == seatTag) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// ����Ҽ���ʱ����
        /// </summary>
        public void NewPlayerJoinIn(ulong clientId) {
            // ������б����ҵ���,����������û�
            if (GetPlayerById(clientId).ClientId == clientId) {
                return;
            }

            string PlayerId = "player" + (PlayerChooseStates.Count + 1).ToString();

            // Ϊ�����ѡ��ɫ
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
            Debug.Log("���µ���Ҽ���! ClientIdΪ:" + clientId + ",��ǰ�����Ŀ:" + PlayerChooseStates.Count);
        }

        // ����ң��ûص����ڿͻ�������ע��
        public event Action<ulong, string, bool> OnClientChangeChooseTag;

        public event Action OnSetPlayerFaction;

        [ServerRpc(RequireOwnership = false)]
        public void ChangeChooseTagServerRpc(ulong clientId, string factionTag, bool lockedIn) {
            //Debug.Log("serverRpc ����: " + clientId);
            // TODO: д����������
            OnClientChangeChooseTag.Invoke(clientId, factionTag, lockedIn);
        }


        // TODO: ��Ҫ��������������,�ں��ʵĵط�����


        // NOTICE: RequireOwnership�ֶε���˼�ǣ��÷����Ƿ�ֻ�ܱ�����ű����ã����Ϊfalse��������������
        [ServerRpc(RequireOwnership = false)]
        public void PlayerRequestJoinServerRpc() {
            // �Ƿ������ˣ��������м������ҵ���Ϣ
            IReadOnlyDictionary<ulong, NetworkClient> curClientDic = NetworkManager.Singleton.ConnectedClients;
            foreach (var keyValuePair in curClientDic) {
                // NOTICE: �ͻ��˲����޸ķ���������
                //Debug.Log("��ǰ�����������:" + keyValuePair.Key + "," + keyValuePair.Value.PlayerObject.name);
                NewPlayerJoinIn(keyValuePair.Key);
            }
            //Debug.Log("��ǰ���������Ŀ:" + PlayerChooseStates.Count);
        }

        /// <summary>
        /// ���ÿͻ��������ѡ���faction
        /// </summary>
        [ClientRpc]
        public void SetPlayerFactionClientRpc() {
            OnSetPlayerFaction?.Invoke();
        }

        #endregion

    }

}