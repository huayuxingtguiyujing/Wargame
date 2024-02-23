using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using WarGame_True.GamePlay.Application.TimeTask;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Infrastructure.NetworkPackage;

namespace WarGame_True.Infrastructure.Map.Controller {

    public class MapNetworkCtrl : NetworkBehaviour {

        public static MapNetworkCtrl Instance;

        [SerializeField] MapController mapController;
        [SerializeField] ArmyNetworkCtrl armyNetworkCtrl;

        private void Awake() {
            Instance = this;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
        }
        
        #region ʡ�ݲ��� ����ͬ���ӿ�

        public void StartRecruitArmyEvent(uint provinceID, ArmyUnitData armyUnitData) {
            if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsClient) {
                return;
            }
            StartRecruitArmyServerRpc(provinceID, armyUnitData);
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartRecruitArmyServerRpc(uint provinceID, ArmyUnitData armyUnitData, ServerRpcParams rpcParams = default) {
            //Debug.Log($"���������յ���ļ�����¼�, ����ʡ��: {provinceID}, ���: {rpcParams.Receive.SenderClientId}");
            if (IsSpawned) {
                StartRecruitArmyClientRpc(rpcParams.Receive.SenderClientId, provinceID, armyUnitData);
            }
        }

        [ClientRpc]
        private void StartRecruitArmyClientRpc(ulong requesterID, uint provinceID, ArmyUnitData armyUnitData, ClientRpcParams rpcParams = default) {
            if (IsSpawned) {
                if (requesterID == NetworkManager.LocalClientId) {
                    // ����Ҫ�ڱ����ٴν�����ļ
                    return;
                }
                // �ҵ���Ӧ��ʡ��
                Province province = mapController.GetProvinceByID(provinceID);

                // ִ�и�ʡ���ϵ��¼�
                province.StartRecruitArmy(armyUnitData);

                //Debug.Log("�ɹ��ڿͻ���ִ����ļ�¼�!");
            }
        }

        // TODO: ��� �ж���ļ�¼�
        public void CancelRecruitArmyEvent(uint provinceID, uint recruitEventOrder) {

        }

        [ServerRpc(RequireOwnership = false)]
        private void CancelRecruitArmyServerRpc(ServerRpcParams rpcParams = default) {

        }

        [ClientRpc]
        private void CancelRecruitArmyClientRpc(ClientRpcParams rpcParams = default) {

        }

        /// <summary>
        /// ����һ��ս��
        /// </summary>
        public void StartCombatEvent(Province province, List<Army> attacker, List<Army> defender) {
            Debug.Log($"������ս���¼�,����ʡ��:{province.provinceData.provinceID},����������:{attacker.Count},����ֵ����:{defender.Count}");

            if (NetworkManager.Singleton.IsServer) {
                // ����ģʽ��ֻ�ڷ�������ִ��ս�������߼�

                // ����ս������
                Combat combatObj = province.StartCombatInProvince(
                    attacker, defender
                );

                // spawn networkobject
                NetworkObject networkObject = combatObj.GetComponent<NetworkObject>();
                networkObject.Spawn();

                // ֪ͨ�����ͻ���
                StartCombatClientRpc(
                    networkObject.NetworkObjectId,
                    province.provinceData.provinceID,
                    armyNetworkCtrl.GetArmyIDList(attacker).ToArray(), 
                    armyNetworkCtrl.GetArmyIDList(defender).ToArray()
                );
            } else if (!NetworkManager.Singleton.IsClient) {
                // ����ģʽ
                province.StartCombatInProvince(attacker, defender);
            }
        }

        [ClientRpc]
        private void StartCombatClientRpc(ulong combatObjID, uint provinceID, ulong[] attackerArmyID, ulong[] defenderArmyID, ClientRpcParams rpcParams = default) {
            Debug.Log($"�ͻ��˽��յ�ս���¼�:{combatObjID},����ʡ��:{provinceID},������:{attackerArmyID.Length},������:{defenderArmyID.Length}");

            Province province = mapController.GetProvinceByID(provinceID);
            
            NetworkObject combatObj = GetNetworkObject(combatObjID);

            // ��ʼ����������ͬ���� combatnetwork
            //CombatNetwork combatNet = combatObj.GetComponent<CombatNetwork>();
            //combatNet.InitCombatNetwork(
            //    province,
            //    armyNetworkCtrl.GetArmyListByID(attackerArmyID.ToList()),
            //    armyNetworkCtrl.GetArmyListByID(defenderArmyID.ToList())
            //);

            if (!NetworkManager.Singleton.IsServer) {
                // �������˲���Ҫ�ٳ�ʼ��һ��ս������
                Combat combat = combatObj.GetComponentInChildren<Combat>();
                combat.InitCombat(
                    armyNetworkCtrl.GetArmyListByID(attackerArmyID.ToList()), 
                    armyNetworkCtrl.GetArmyListByID(defenderArmyID.ToList()), 
                    province
                );
                province.SetCurCombatObj(combat.gameObject);
            }

            // ��������ͬ����ս���¼�
            province.StartCombatNetwork(
                armyNetworkCtrl.GetArmyListByID(attackerArmyID.ToList()),
                armyNetworkCtrl.GetArmyListByID(defenderArmyID.ToList())
            );


        }

        /// <summary>
        /// ר����������ͬ��������ս������
        /// </summary>
        public void HandleCombat(Province province) {
            // ����ģʽ
            if (!NetworkManager.IsClient) {
                return;
            }

            // ֻ���ڷ������˵��ô��¼�
            if (!NetworkManager.IsServer) {
                return;
            }

            // TODO: ����
            BattlePlaceNetwork[] frontAtUnits = province.currentCombat.GetFrontUnitBPList(true);
            BattlePlaceNetwork[] frontDeUnits = province.currentCombat.GetFrontUnitBPList(false);

            BattlePlaceNetwork[] rearAtUnits = province.currentCombat.GetRearUnitBPList(true);
            BattlePlaceNetwork[] rearDeUnits = province.currentCombat.GetRearUnitBPList(false);

            BattlePlaceNetwork[] withdrawAtUnits = province.currentCombat.GetWithdrawUnitBPList(true);
            BattlePlaceNetwork[] withdrawDeUnits = province.currentCombat.GetWithdrawUnitBPList(false);

            // ͬ���������ͻ���
            HandleCombatClientRpc(
                province.provinceData.provinceID, 
                frontAtUnits, frontDeUnits, 
                rearAtUnits, rearDeUnits, 
                withdrawAtUnits, withdrawDeUnits
            );
        }

        [ClientRpc]
        public void HandleCombatClientRpc(uint provinceID, BattlePlaceNetwork[] frontAtUnits, BattlePlaceNetwork[] frontDeUnits, BattlePlaceNetwork[] rearAtUnits, BattlePlaceNetwork[] rearDeUnits,
            BattlePlaceNetwork[] withdrawAtUnits, BattlePlaceNetwork[] withdrawDeUnits, ClientRpcParams rpcParams = default) {

            if (NetworkManager.IsServer) {
                // �������˲���Ҫ��ͬ��
                return;
            }

            Province province = mapController.GetProvinceByID(provinceID);
            
            // ���ø�ʡ���ϵ�ͬ������
            province.HandleCombat_Network(
                frontAtUnits, frontDeUnits, 
                rearAtUnits, rearDeUnits, 
                withdrawAtUnits, withdrawDeUnits
            );

            Debug.Log($"ս�۸����¼�,������ǰ��:{frontAtUnits.Length},��:{rearAtUnits.Length},����:{withdrawAtUnits.Length}");
            Debug.Log($"ս�۸����¼�,������ǰ��:{frontDeUnits.Length},��:{rearDeUnits.Length},����:{withdrawDeUnits.Length}");

        }


        public void CombatOverEvent(uint combatProvinceID) {
            // ������������ ����������
            if (!NetworkManager.IsServer) {
                return;
            }else if (NetworkManager.IsClient) {
                CombatOverClientRpc(combatProvinceID);
            } else {
                // ����ģʽ
            }
        }

        [ClientRpc]
        public void CombatOverClientRpc(uint combatProvinceID, ClientRpcParams rpcParams = default) {
            Province combatProvince = mapController.GetProvinceByID(combatProvinceID);
            combatProvince.CancelCombat();
        }

        #endregion

    }
}