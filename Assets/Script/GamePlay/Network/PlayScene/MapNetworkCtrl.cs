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
        
        #region 省份操作 网络同步接口

        public void StartRecruitArmyEvent(uint provinceID, ArmyUnitData armyUnitData) {
            if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsClient) {
                return;
            }
            StartRecruitArmyServerRpc(provinceID, armyUnitData);
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartRecruitArmyServerRpc(uint provinceID, ArmyUnitData armyUnitData, ServerRpcParams rpcParams = default) {
            //Debug.Log($"服务器接收到招募军队事件, 来自省份: {provinceID}, 玩家: {rpcParams.Receive.SenderClientId}");
            if (IsSpawned) {
                StartRecruitArmyClientRpc(rpcParams.Receive.SenderClientId, provinceID, armyUnitData);
            }
        }

        [ClientRpc]
        private void StartRecruitArmyClientRpc(ulong requesterID, uint provinceID, ArmyUnitData armyUnitData, ClientRpcParams rpcParams = default) {
            if (IsSpawned) {
                if (requesterID == NetworkManager.LocalClientId) {
                    // 不需要在本地再次进行征募
                    return;
                }
                // 找到对应的省份
                Province province = mapController.GetProvinceByID(provinceID);

                // 执行该省份上的事件
                province.StartRecruitArmy(armyUnitData);

                //Debug.Log("成功在客户端执行招募事件!");
            }
        }

        // TODO: 完成 中断招募事件
        public void CancelRecruitArmyEvent(uint provinceID, uint recruitEventOrder) {

        }

        [ServerRpc(RequireOwnership = false)]
        private void CancelRecruitArmyServerRpc(ServerRpcParams rpcParams = default) {

        }

        [ClientRpc]
        private void CancelRecruitArmyClientRpc(ClientRpcParams rpcParams = default) {

        }

        /// <summary>
        /// 开启一场战役
        /// </summary>
        public void StartCombatEvent(Province province, List<Army> attacker, List<Army> defender) {
            Debug.Log($"触发了战斗事件,所在省份:{province.provinceData.provinceID},攻击者数量:{attacker.Count},防御值数量:{defender.Count}");

            if (NetworkManager.Singleton.IsServer) {
                // 联网模式，只在服务器端执行战役生成逻辑

                // 生成战役物体
                Combat combatObj = province.StartCombatInProvince(
                    attacker, defender
                );

                // spawn networkobject
                NetworkObject networkObject = combatObj.GetComponent<NetworkObject>();
                networkObject.Spawn();

                // 通知其他客户端
                StartCombatClientRpc(
                    networkObject.NetworkObjectId,
                    province.provinceData.provinceID,
                    armyNetworkCtrl.GetArmyIDList(attacker).ToArray(), 
                    armyNetworkCtrl.GetArmyIDList(defender).ToArray()
                );
            } else if (!NetworkManager.Singleton.IsClient) {
                // 单机模式
                province.StartCombatInProvince(attacker, defender);
            }
        }

        [ClientRpc]
        private void StartCombatClientRpc(ulong combatObjID, uint provinceID, ulong[] attackerArmyID, ulong[] defenderArmyID, ClientRpcParams rpcParams = default) {
            Debug.Log($"客户端接收到战斗事件:{combatObjID},所在省份:{provinceID},攻击者:{attackerArmyID.Length},防御者:{defenderArmyID.Length}");

            Province province = mapController.GetProvinceByID(provinceID);
            
            NetworkObject combatObj = GetNetworkObject(combatObjID);

            // 初始化用于联网同步的 combatnetwork
            //CombatNetwork combatNet = combatObj.GetComponent<CombatNetwork>();
            //combatNet.InitCombatNetwork(
            //    province,
            //    armyNetworkCtrl.GetArmyListByID(attackerArmyID.ToList()),
            //    armyNetworkCtrl.GetArmyListByID(defenderArmyID.ToList())
            //);

            if (!NetworkManager.Singleton.IsServer) {
                // 服务器端不需要再初始化一遍战役物体
                Combat combat = combatObj.GetComponentInChildren<Combat>();
                combat.InitCombat(
                    armyNetworkCtrl.GetArmyListByID(attackerArmyID.ToList()), 
                    armyNetworkCtrl.GetArmyListByID(defenderArmyID.ToList()), 
                    province
                );
                province.SetCurCombatObj(combat.gameObject);
            }

            // 开启网络同步的战役事件
            province.StartCombatNetwork(
                armyNetworkCtrl.GetArmyListByID(attackerArmyID.ToList()),
                armyNetworkCtrl.GetArmyListByID(defenderArmyID.ToList())
            );


        }

        /// <summary>
        /// 专门用于联网同步、控制战斗进程
        /// </summary>
        public void HandleCombat(Province province) {
            // 单机模式
            if (!NetworkManager.IsClient) {
                return;
            }

            // 只能在服务器端调用此事件
            if (!NetworkManager.IsServer) {
                return;
            }

            // TODO: 完善
            BattlePlaceNetwork[] frontAtUnits = province.currentCombat.GetFrontUnitBPList(true);
            BattlePlaceNetwork[] frontDeUnits = province.currentCombat.GetFrontUnitBPList(false);

            BattlePlaceNetwork[] rearAtUnits = province.currentCombat.GetRearUnitBPList(true);
            BattlePlaceNetwork[] rearDeUnits = province.currentCombat.GetRearUnitBPList(false);

            BattlePlaceNetwork[] withdrawAtUnits = province.currentCombat.GetWithdrawUnitBPList(true);
            BattlePlaceNetwork[] withdrawDeUnits = province.currentCombat.GetWithdrawUnitBPList(false);

            // 同步到各个客户端
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
                // 服务器端不需要被同步
                return;
            }

            Province province = mapController.GetProvinceByID(provinceID);
            
            // 调用该省份上的同步函数
            province.HandleCombat_Network(
                frontAtUnits, frontDeUnits, 
                rearAtUnits, rearDeUnits, 
                withdrawAtUnits, withdrawDeUnits
            );

            Debug.Log($"战役更新事件,攻击方前线:{frontAtUnits.Length},后方:{rearAtUnits.Length},撤退:{withdrawAtUnits.Length}");
            Debug.Log($"战役更新事件,防御方前线:{frontDeUnits.Length},后方:{rearDeUnits.Length},撤退:{withdrawDeUnits.Length}");

        }


        public void CombatOverEvent(uint combatProvinceID) {
            // 本方法仅允许 服务器调用
            if (!NetworkManager.IsServer) {
                return;
            }else if (NetworkManager.IsClient) {
                CombatOverClientRpc(combatProvinceID);
            } else {
                // 单机模式
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