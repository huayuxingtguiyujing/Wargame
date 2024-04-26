using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.Application.TimeTask;
using WarGame_True.Infrastructure.Map.Controller;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Infrastructure.Map.Util;
using WarGame_True.Infrastructure.NetworkPackage;

namespace WarGame_True.GamePlay.ArmyPart {
    public class ArmyNetworkCtrl : NetworkBehaviour {
        
        public static ArmyNetworkCtrl Instance;

        [SerializeField] ArmyController armyController;
        [SerializeField] MapController mapController;

        public NetworkList<NetworkObjectReference> allArmyNetworkList;

        // ���ڼ�¼һ�β�ֺ�Ľ��������������ͬ��ʱ�Ĳ��
        private List<Army> splitArmyList;

        private void Awake() {
            Instance = this;
            splitArmyList = new List<Army>();
            allArmyNetworkList = new NetworkList<NetworkObjectReference>();
        }

        #region ���Ӳ��� ����ͬ���ӿ�

        public List<ulong> GetArmyIDList(List<Army> curChooseArmy) {
            List<ulong> result = new List<ulong>();
            foreach (Army army in curChooseArmy) {
                // �ҵ���Ӧ��networkObject
                NetworkObject netObj = army.GetComponent<NetworkObject>();
                if (netObj == null) {
                    Debug.LogError("δ��ȡ��networkObject");
                    return null;
                }
                if (netObj.IsSpawned) {
                    result.Add(netObj.NetworkObjectId);
                }
            }
            return result;
        }

        public List<Army> GetArmyListByID(List<ulong> armyNetObjID) {
            List<Army> armies = new List<Army>();
            foreach (ulong netObjID in armyNetObjID) {
                // �ҵ���Ӧ��networkObject
                armies.Add(GetArmyByID(netObjID));
            }
            return armies;
        }

        public Army GetArmyByID(ulong armyNetID) {
            NetworkObject networkObject = GetNetworkObject(armyNetID);
            if (networkObject == null) {
                Debug.LogError($"û���ҵ�idΪ{armyNetID}��networkobject");
            }
            Army army = networkObject.GetComponent<Army>();
            return army;
        }

        public ulong GetArmyID(Army army) {
            // �ҵ���Ӧ��networkObject
            NetworkObject netObj = army.GetComponent<NetworkObject>();
            if (netObj == null) {
                Debug.LogError("δ��ȡ��networkObject");
                return default;
            }

            // ʹ�ô˷���ʱ Ĭ���Ѿ�������netobj
            if (netObj.IsSpawned) {
                return netObj.NetworkObjectId;
            } else {
                Debug.Log("��networkobject��Ϊ����");
                return default;
            }
            
        }

        // ��������
        public void CreateArmyEvent(ArmyUnitData armyUnitData, uint provinceID) {
            if (NetworkManager.Singleton.IsServer) {

                //Debug.Log("���������յ���������");

                // �������ӵ�networkobj����spawn
                Province province = MapController.Instance.GetProvinceByID(provinceID);
                Army army = armyController.CreateArmyObj(province);
                NetworkObject networkObject = army.GetComponent<NetworkObject>();
                networkObject.Spawn();

                // TODO�����´����ľ��Ӽ��뵽networkList��
                allArmyNetworkList.Add(networkObject);
                NetworkGuid networkGuid = System.Guid.NewGuid().ToNetworkGuid();
                // ͬ�������пͻ���
                CreateArmyClientRpc(networkGuid, networkObject, armyUnitData, provinceID);
            } else if(!NetworkManager.Singleton.IsClient) {
                // ����ģʽ ֱ���ڱ��ش�������
                Province province = MapController.Instance.GetProvinceByID(provinceID);
                Army army = armyController.CreateArmyObj(province);

                // ÿ����λ����ʱ����һ��������NetworkGuid,��������ͬ��
                NetworkGuid networkGuid = System.Guid.NewGuid().ToNetworkGuid();
                ArmyUnit armyUnit = new ArmyUnit(armyUnitData, networkGuid);
                armyController.InitArmy(armyUnit, province, army);
            }

            // �ͻ��˲���Ҫ�����κβ��� �ȴ�����������

            return;
        }

        [ClientRpc]
        private void CreateArmyClientRpc(NetworkGuid networkGuid, NetworkObjectReference armyObj, ArmyUnitData armyUnitData, uint provinceID, ClientRpcParams rpcParams = default) {
            //if (NetworkManager.Singleton.IsServer) {
            //    return;
            //}

            ArmyUnit armyUnit = new ArmyUnit(armyUnitData, networkGuid);
            Province province = MapController.Instance.GetProvinceByID(provinceID);
            armyObj.TryGet(out NetworkObject networkObject);
            Army army = networkObject.GetComponent<Army>();
            armyController.InitArmy(armyUnit, province, army);

            //Debug.Log("�ͻ�����ɴ�������: " + networkObject.NetworkObjectId);

        }

        // �Ƴ�����
        public void RemoveArmyEvent_Choosen() {
            //if (NetworkManager.Singleton.IsClient) {
            //    List<ulong> armyObjIDs = GetArmyIDList(armyController.GetArmyChoosen());
            //    RemoveArmyServerRpc(armyObjIDs.ToArray());
            //} else {
            //    // ����ģʽ ֱ���Ƴ�ѡ�еĵ�λ����
            //    armyController.RemoveArmy_Choosen();
            //}
            RemoveArmyEvent(armyController.GetArmyChoosen());
        }

        public void RemoveArmyEvent(List<Army> army) {
            if (NetworkManager.Singleton.IsClient) {
                List<ulong> armyObjIDs = GetArmyIDList(army);
                RemoveArmyServerRpc(armyObjIDs.ToArray());
            } else {
                armyController.RemoveArmies(army);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RemoveArmyServerRpc(ulong[] armyObjIDs, ServerRpcParams rpcParams = default) {
            //Debug.Log("�������յ��Ƴ���������");

            // ���ڸ����ͻ��˴��Ƴ�
            RemoveArmyClientRpc(armyObjIDs);


            // ��networklist�ӳ�ȥ
            //for(int i = ArmyNetworkList.Count; i >= 0; i--) {
            //    if (ArmyNetworkList[i] == null) {

            //    }
            //}

            // ���ȱ�ɾ�������Ƴ�ArmyNetworkList�еĿ�Ӧ�ÿ��ܻ�����⣩Despawn ������
            //foreach (ulong netObjID in armyObjIDs) {
            //    NetworkObject networkObject = GetNetworkObject(netObjID);
            //    if (networkObject != null) {
            //        if (ArmyNetworkList.Contains(networkObject)) {
            //            ArmyNetworkList.Remove(networkObject);
            //        }
            //        networkObject.Despawn();
            //        //Destroy(networkObject);
            //    } else {
            //        Debug.LogError($"û���ҵ�netidΪ{netObjID}������");
            //    }
            //}
        }

        [ClientRpc]
        private void RemoveArmyClientRpc(ulong[] armyObjIDs, ClientRpcParams rpcParams = default) {
            List<Army> removeTargets = new List<Army>();
            foreach (ulong netObjID in armyObjIDs)
            {
                // �ҵ���Ӧ��networkObject
                Army army = GetArmyByID(netObjID);
                removeTargets.Add(army);
            }

            // ����armycontroller�ķ����Ƴ����ؾ���
            armyController.RemoveArmies(removeTargets);
        }

        // �ϲ�����
        public void MergeArmyEvent_Choosen() {
            if (NetworkManager.Singleton.IsClient) {
                List<ulong> armyObjIDs = GetArmyIDList(armyController.GetArmyChoosen());
                //Debug.Log($"�����ϲ������¼�,�ϲ�������:{armyObjIDs.Count}");
                if (armyObjIDs.Count > 1) {
                    ulong mergeHostID = armyObjIDs[0];
                    MergeArmyServerRpc(mergeHostID, armyObjIDs.ToArray());

                    // �ϲ�������mergehostΪ�µ�ѡ�о���
                    // ˢ��ѡ�о���Ϊ�ϲ���ľ���
                    
                    List<Army> newCurChooseArmy = new List<Army> { GetArmyByID(mergeHostID) };
                    armyController.currentChooseArmy = newCurChooseArmy;

                    armyController.InvokeArmyPanel();
                }

            } else {
                // ����ģʽ ֱ�ӳ��Ժϲ�
                armyController.MergeArmy_Choosen();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void MergeArmyServerRpc(ulong mergeHostID, ulong[] armyObjIDs, ServerRpcParams rpcParams = default) {
            MergeArmyClientRpc(mergeHostID, armyObjIDs);
        }

        [ClientRpc]
        private void MergeArmyClientRpc(ulong mergeHostID, ulong[] armyObjIDs, ClientRpcParams rpcParams = default) {
            //Debug.Log($"����ͻ��˷��ͺϲ�����,�ϲ���Ŀ{armyObjIDs.Length},�ϲ�hostidΪ{mergeHostID}");
            armyController.MergeArmy(
                GetArmyByID(mergeHostID), GetArmyListByID(armyObjIDs.ToList())
            );
        }

        // ��־���
        public void SplitArmyEvent_Choosen() {
            if (NetworkManager.Singleton.IsClient) {
                if (splitArmyList != null) {
                    // NOTICE: �˴����ʹ��splitArmyList.Clear(),�ƺ�������޷����ѡ�о��ӵ�����;
                    splitArmyList = new List<Army>();
                }

                Debug.Log(armyController.GetArmyChoosen().Count);

                List<ulong> armyObjIDs = GetArmyIDList(armyController.GetArmyChoosen());

                Debug.Log($"������־����¼�,��ֶ�����:{armyObjIDs.Count},��ǰѡ�еľ�����{armyController.GetArmyChoosen().Count}");

                // Ӧ���Ե������ӽ��в�֣�������ֺ�ĵõ�����army id�ַ����������
                foreach(ulong armyObjID in armyObjIDs) {
                    SplitArmyServerRpc(armyObjID);
                }
                
                // �õ��˲�ֽ��
                if(splitArmyList.Count > 0) {
                    //���ò�ֺ�ľ���Ϊ�µ�ѡ�о���
                    armyController.SetArmyUnchoosen();
                    armyController.currentChooseArmy = splitArmyList;
                    armyController.SetArmyChoosen();
                    ArmyHelper.GetInstance().SetArmyStayIn(armyController.currentChooseArmy);

                    //ˢ�����
                    armyController.InvokeArmyPanel();
                }

            } else {
                // ����ģʽ ֱ�ӳ��Բ��
                armyController.SplitArmy_Choosen();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SplitArmyServerRpc(ulong armyObjID, ServerRpcParams rpcParams = default) {
            //Debug.Log($"���������յ���־�������,��ֶ���idΪ{armyObjID}");

            // ��ȡ��Ҫ�����ľ���
            Army amry = GetArmyByID(armyObjID);
            if (armyController.AbleToSplitArmy(amry)) {
                Army splitedArmy = armyController.CreateArmyObj(amry.CurrentProvince);
                NetworkObject networkObject = splitedArmy.GetComponent<NetworkObject>();
                networkObject.Spawn();
                // ����ֽ�� ͬ������������
                SplitArmyClientRpc(GetArmyID(splitedArmy), armyObjID);
            }
        }

        [ClientRpc]
        private void SplitArmyClientRpc(ulong splitedArmyObjID, ulong armyObjID, ClientRpcParams rpcParams = default) {
            // NOTICE: ���ǵ���־���Ҫ��ÿֻҪ��ֵĲ��ӽ��� �Ƴ����½��Ȳ���
            //  ������״̬�£���ʹ��ArmyController��ķ���
            // NOTICE: splitedArmyObjID�ǲ�ֺ�ľ��ӵ�id��armyObjID�ǲ��ǰ�ľ��ӵ�id
            //Debug.Log($"�ͻ��˽��յ���־�������,�����idΪ{armyObjID},��ֺ���¾���idΪ{splitedArmyObjID}");

            Army splitedArmy = GetArmyByID(splitedArmyObjID);
            Army army = GetArmyByID(armyObjID);

            // �ڿͻ��� ��ʼ�������ɵľ���
            armyController.InitArmy(
                army.SplitArmy(),
                army.CurrentProvince, splitedArmy
            );

            splitArmyList.Add(splitedArmy);
            splitArmyList.Add(army);
        }

        /// <summary>
        /// �ƶ�����-����������ͬ��
        /// </summary>
        public void MoveArmyEvent(List<Army> armies, uint destinationID, bool IsWithdraw) {
            if(armies == null || armies.Count == 0) {
                // û�д������
                return;
            }

            //Debug.Log("�ƶ������¼�������");

            if (NetworkManager.IsClient) {
                // ��army����תΪ��Ӧ��networkobjectid������
                List<ulong> armyNetObjIDs = new List<ulong>();
                foreach (Army army in armies) {
                    armyNetObjIDs.Add(GetArmyID(army));
                }

                // ������������ƶ�����
                MoveArmyServerRpc(armyNetObjIDs.ToArray(), destinationID, IsWithdraw);
            } else {
                // ����ģʽ
                Province destination = mapController.GetProvinceByID(destinationID);
                
                // Ϊ���о��������ƶ��¼�
                foreach (Army army in armies) {
                    // ��ȡ�ƶ� ʡ��·��
                    uint curProvinceID = army.CurrentProvince.provinceData.provinceID;
                    Province curProvince = mapController.GetProvinceByID(curProvinceID);
                    List<Province> movePath = mapController.GetMovePath(curProvince, destination);

                    // ��ȡ�ܿ���
                    uint totalMoveCost = ProvinceHelper.GetProvinceMoveCost(movePath);

                    ArmyMoveTask armyMoveTask = new ArmyMoveTask(totalMoveCost, army, movePath);
                    army.SetMoveTask(armyMoveTask, IsWithdraw);
                }

            }

        }

        // TODO: ò�ƻ���Щ����
        [ServerRpc(RequireOwnership = false)]
        private void MoveArmyServerRpc(ulong[] armyObjID, uint destinationID, bool IsWithdraw, ServerRpcParams rpcParams = default) {
            //Debug.Log($"���������յ��ƶ������¼�,�ƶ��յ�{destinationID},������Ŀ{armyObjID.Length}");
            MoveArmyClientRpc(armyObjID, destinationID, IsWithdraw);
        }

        [ClientRpc]
        private void MoveArmyClientRpc(ulong[] armyObjID, uint destinationID, bool IsWithdraw, ClientRpcParams rpcParams = default) {
            
            Province destination = mapController.GetProvinceByID(destinationID);
            
            // ��ö�Ӧ�ľ���
            List<Army> armies = GetArmyListByID(armyObjID.ToList());

            // Ϊ���о��������ƶ��¼�
            foreach (Army army in armies)
            {
                // ��ȡ�ƶ� ʡ��·��
                uint curProvinceID = army.CurrentProvince.provinceData.provinceID;
                Province curProvince = mapController.GetProvinceByID(curProvinceID);
                List<Province> movePath = mapController.GetMovePath(curProvince, destination);

                // ��ȡ�ܿ���
                uint totalMoveCost = ProvinceHelper.GetProvinceMoveCost(movePath);

                ArmyMoveTask armyMoveTask = new ArmyMoveTask(totalMoveCost, army, movePath);
                army.SetMoveTask(armyMoveTask, IsWithdraw);
            }

            //Debug.Log($"�ɹ����þ����ƶ��¼�,�ƶ��ľ�����Ŀ{armies.Count}");
        }

        /// <summary>
        /// ��ָ���ľ��Ӿ�ֹ��ԭ��
        /// </summary>
        public void SetArmyStayInEvent(List<Army> stayTargets) {
            if (!NetworkManager.IsClient) {
                // ����ģʽ
                ArmyHelper.GetInstance().SetArmyStayIn(stayTargets);
            } else {
                SetArmyStayInClientRpc(
                    GetArmyIDList(stayTargets).ToArray()
                );
            }
            
        }

        [ClientRpc]
        private void SetArmyStayInClientRpc(ulong[] armyObjID, ClientRpcParams rpcParams = default) {
            //Debug.Log($"SetArmyStayIn����!������Ŀ:{armyObjID.Length}");
            ArmyHelper.GetInstance().SetArmyStayIn(
                GetArmyListByID(armyObjID.ToList())
            );
        }

        /// <summary>
        /// ��ָ���ľ����˳�ս��
        /// </summary>
        public void ExitCombatEvent(List<Army> stayTargets) {
            if (!NetworkManager.IsClient) {
                // ����ģʽ
                ArmyHelper.GetInstance().ExitCombat(stayTargets);
            } else {
                ExitCombatClientRpc(
                    GetArmyIDList(stayTargets).ToArray()
                );
            }
        }

        [ClientRpc]
        private void ExitCombatClientRpc(ulong[] armyObjID, ClientRpcParams rpcParams = default) {
            ArmyHelper.GetInstance().ExitCombat(
                GetArmyListByID(armyObjID.ToList())
            );
        }

        #endregion

    }


}