using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.CombatPart {

    /// <summary>
    /// ���� ����ͬ��ս��
    /// </summary>
    public class CombatNetwork : NetworkBehaviour {

        Province province;

        public void InitCombatNetwork(Province province, List<Army> attackers, List<Army> defenders) {

            this.province = province;

            if (!NetworkManager.Singleton.IsServer) {
                // �������˲���Ҫ�ٳ�ʼ��һ��ս������
                Combat combat = GetComponentInChildren<Combat>();
                combat.InitCombat(attackers, defenders, province);
                province.SetCurCombatObj(combat.gameObject);
            }

            // ��������ͬ����ս���¼�
            province.StartCombatNetwork(attackers, defenders);


            //// ս������
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
    /// ÿ�δ��ݸ�������Ҷ˵�ս����Ϣ
    /// </summary>
    public class CombatNetworkMes : INetworkSerializable {




        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            throw new System.NotImplementedException();
        }

    }
}