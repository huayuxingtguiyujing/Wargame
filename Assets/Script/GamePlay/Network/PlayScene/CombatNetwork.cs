using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.CombatPart {

    /// <summary>
    /// 用于 联网同步战役
    /// </summary>
    public class CombatNetwork : NetworkBehaviour {

        Province province;

        public void InitCombatNetwork(Province province, List<Army> attackers, List<Army> defenders) {

            this.province = province;

            if (!NetworkManager.Singleton.IsServer) {
                // 服务器端不需要再初始化一遍战役物体
                Combat combat = GetComponentInChildren<Combat>();
                combat.InitCombat(attackers, defenders, province);
                province.SetCurCombatObj(combat.gameObject);
            }

            // 开启网络同步的战役事件
            province.StartCombatNetwork(attackers, defenders);


            //// 战役物体
            //Combat combat = this.GetComponentInChildren<Combat>();
            //combat.InitCombat(attackers, defenders, province);

        }

        public void UpdateCombat() {

        }

        [ServerRpc]
        public void CombatServerRpc(ServerRpcParams rpcParams = default) {

        }

        [ClientRpc]
        public void CombatClientRpc(ClientRpcParams rpcParams = default) {

        }

    }


    /// <summary>
    /// 每次传递给其他玩家端的战役信息
    /// </summary>
    public class CombatNetworkMes : INetworkSerializable {




        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            throw new System.NotImplementedException();
        }

    }
}