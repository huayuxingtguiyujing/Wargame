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

        // 用于记录一次拆分后的结果，仅用于网络同步时的拆分
        private List<Army> splitArmyList;

        private void Awake() {
            Instance = this;
            splitArmyList = new List<Army>();
            allArmyNetworkList = new NetworkList<NetworkObjectReference>();
        }

        #region 军队操作 网络同步接口

        public List<ulong> GetArmyIDList(List<Army> curChooseArmy) {
            List<ulong> result = new List<ulong>();
            foreach (Army army in curChooseArmy) {
                // 找到对应的networkObject
                NetworkObject netObj = army.GetComponent<NetworkObject>();
                if (netObj == null) {
                    Debug.LogError("未获取到networkObject");
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
                // 找到对应的networkObject
                armies.Add(GetArmyByID(netObjID));
            }
            return armies;
        }

        public Army GetArmyByID(ulong armyNetID) {
            NetworkObject networkObject = GetNetworkObject(armyNetID);
            if (networkObject == null) {
                Debug.LogError($"没有找到id为{armyNetID}的networkobject");
            }
            Army army = networkObject.GetComponent<Army>();
            return army;
        }

        public ulong GetArmyID(Army army) {
            // 找到对应的networkObject
            NetworkObject netObj = army.GetComponent<NetworkObject>();
            if (netObj == null) {
                Debug.LogError("未获取到networkObject");
                return default;
            }

            // 使用此方法时 默认已经启动了netobj
            if (netObj.IsSpawned) {
                return netObj.NetworkObjectId;
            } else {
                Debug.Log("该networkobject还为启动");
                return default;
            }
            
        }

        // 创建军队
        public void CreateArmyEvent(ArmyUnitData armyUnitData, uint provinceID) {
            if (NetworkManager.Singleton.IsServer) {

                //Debug.Log("服务器接收到创建请求");

                // 创建军队的networkobj，并spawn
                Province province = MapController.Instance.GetProvinceByID(provinceID);
                Army army = armyController.CreateArmyObj(province);
                NetworkObject networkObject = army.GetComponent<NetworkObject>();
                networkObject.Spawn();

                // TODO：将新创建的军队加入到networkList中
                allArmyNetworkList.Add(networkObject);
                NetworkGuid networkGuid = System.Guid.NewGuid().ToNetworkGuid();
                // 同步到所有客户端
                CreateArmyClientRpc(networkGuid, networkObject, armyUnitData, provinceID);
            } else if(!NetworkManager.Singleton.IsClient) {
                // 断网模式 直接在本地创建即可
                Province province = MapController.Instance.GetProvinceByID(provinceID);
                Army army = armyController.CreateArmyObj(province);

                // 每个单位生成时都有一个独立的NetworkGuid,用于网络同步
                NetworkGuid networkGuid = System.Guid.NewGuid().ToNetworkGuid();
                ArmyUnit armyUnit = new ArmyUnit(armyUnitData, networkGuid);
                armyController.InitArmy(armyUnit, province, army);
            }

            // 客户端不需要进行任何操作 等待服务器即可

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

            //Debug.Log("客户端完成创建军队: " + networkObject.NetworkObjectId);

        }

        // 移除军队
        public void RemoveArmyEvent_Choosen() {
            //if (NetworkManager.Singleton.IsClient) {
            //    List<ulong> armyObjIDs = GetArmyIDList(armyController.GetArmyChoosen());
            //    RemoveArmyServerRpc(armyObjIDs.ToArray());
            //} else {
            //    // 单机模式 直接移除选中的单位即可
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
            //Debug.Log("服务器收到移除军队请求");

            // 先在各个客户端处移除
            RemoveArmyClientRpc(armyObjIDs);


            // 从networklist从除去
            //for(int i = ArmyNetworkList.Count; i >= 0; i--) {
            //    if (ArmyNetworkList[i] == null) {

            //    }
            //}

            // （先别删！少了移除ArmyNetworkList中的空应用可能会出问题）Despawn 后销毁
            //foreach (ulong netObjID in armyObjIDs) {
            //    NetworkObject networkObject = GetNetworkObject(netObjID);
            //    if (networkObject != null) {
            //        if (ArmyNetworkList.Contains(networkObject)) {
            //            ArmyNetworkList.Remove(networkObject);
            //        }
            //        networkObject.Despawn();
            //        //Destroy(networkObject);
            //    } else {
            //        Debug.LogError($"没有找到netid为{netObjID}的物体");
            //    }
            //}
        }

        [ClientRpc]
        private void RemoveArmyClientRpc(ulong[] armyObjIDs, ClientRpcParams rpcParams = default) {
            List<Army> removeTargets = new List<Army>();
            foreach (ulong netObjID in armyObjIDs)
            {
                // 找到对应的networkObject
                Army army = GetArmyByID(netObjID);
                removeTargets.Add(army);
            }

            // 调用armycontroller的方法移除本地军队
            armyController.RemoveArmies(removeTargets);
        }

        // 合并军队
        public void MergeArmyEvent_Choosen() {
            if (NetworkManager.Singleton.IsClient) {
                List<ulong> armyObjIDs = GetArmyIDList(armyController.GetArmyChoosen());
                //Debug.Log($"触发合并军队事件,合并对象共有:{armyObjIDs.Count}");
                if (armyObjIDs.Count > 1) {
                    ulong mergeHostID = armyObjIDs[0];
                    MergeArmyServerRpc(mergeHostID, armyObjIDs.ToArray());

                    // 合并后设置mergehost为新的选中军队
                    // 刷新选中军队为合并后的军队
                    
                    List<Army> newCurChooseArmy = new List<Army> { GetArmyByID(mergeHostID) };
                    armyController.currentChooseArmy = newCurChooseArmy;

                    armyController.InvokeArmyPanel();
                }

            } else {
                // 单机模式 直接尝试合并
                armyController.MergeArmy_Choosen();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void MergeArmyServerRpc(ulong mergeHostID, ulong[] armyObjIDs, ServerRpcParams rpcParams = default) {
            MergeArmyClientRpc(mergeHostID, armyObjIDs);
        }

        [ClientRpc]
        private void MergeArmyClientRpc(ulong mergeHostID, ulong[] armyObjIDs, ClientRpcParams rpcParams = default) {
            //Debug.Log($"已向客户端发送合并命令,合并数目{armyObjIDs.Length},合并hostid为{mergeHostID}");
            armyController.MergeArmy(
                GetArmyByID(mergeHostID), GetArmyListByID(armyObjIDs.ToList())
            );
        }

        // 拆分军队
        public void SplitArmyEvent_Choosen() {
            if (NetworkManager.Singleton.IsClient) {
                if (splitArmyList != null) {
                    // NOTICE: 此处如果使用splitArmyList.Clear(),似乎会出现无法获得选中军队的问题;
                    splitArmyList = new List<Army>();
                }

                Debug.Log(armyController.GetArmyChoosen().Count);

                List<ulong> armyObjIDs = GetArmyIDList(armyController.GetArmyChoosen());

                Debug.Log($"触发拆分军队事件,拆分对象共有:{armyObjIDs.Count},当前选中的军队有{armyController.GetArmyChoosen().Count}");

                // 应当对单个军队进行拆分，并将拆分后的得到的子army id分发给各个玩家
                foreach(ulong armyObjID in armyObjIDs) {
                    SplitArmyServerRpc(armyObjID);
                }
                
                // 得到了拆分结果
                if(splitArmyList.Count > 0) {
                    //设置拆分后的军队为新的选中军队
                    armyController.SetArmyUnchoosen();
                    armyController.currentChooseArmy = splitArmyList;
                    armyController.SetArmyChoosen();
                    ArmyHelper.GetInstance().SetArmyStayIn(armyController.currentChooseArmy);

                    //刷新面板
                    armyController.InvokeArmyPanel();
                }

            } else {
                // 单机模式 直接尝试拆分
                armyController.SplitArmy_Choosen();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SplitArmyServerRpc(ulong armyObjID, ServerRpcParams rpcParams = default) {
            //Debug.Log($"服务器接收到拆分军队请求,拆分对象id为{armyObjID}");

            // 获取到要操作的军队
            Army amry = GetArmyByID(armyObjID);
            if (armyController.AbleToSplitArmy(amry)) {
                Army splitedArmy = armyController.CreateArmyObj(amry.CurrentProvince);
                NetworkObject networkObject = splitedArmy.GetComponent<NetworkObject>();
                networkObject.Spawn();
                // 将拆分结果 同步到服务器端
                SplitArmyClientRpc(GetArmyID(splitedArmy), armyObjID);
            }
        }

        [ClientRpc]
        private void SplitArmyClientRpc(ulong splitedArmyObjID, ulong armyObjID, ClientRpcParams rpcParams = default) {
            // NOTICE: 考虑到拆分军队要对每只要拆分的部队进行 移除、新建等操作
            //  故联网状态下，不使用ArmyController里的方法
            // NOTICE: splitedArmyObjID是拆分后的军队的id，armyObjID是拆分前的军队的id
            //Debug.Log($"客户端接收到拆分军队命令,拆分者id为{armyObjID},拆分后的新军队id为{splitedArmyObjID}");

            Army splitedArmy = GetArmyByID(splitedArmyObjID);
            Army army = GetArmyByID(armyObjID);

            // 在客户端 初始化新生成的军队
            armyController.InitArmy(
                army.SplitArmy(),
                army.CurrentProvince, splitedArmy
            );

            splitArmyList.Add(splitedArmy);
            splitArmyList.Add(army);
        }

        /// <summary>
        /// 移动军队-适用于联网同步
        /// </summary>
        public void MoveArmyEvent(List<Army> armies, uint destinationID, bool IsWithdraw) {
            if(armies == null || armies.Count == 0) {
                // 没有传入军队
                return;
            }

            //Debug.Log("移动军队事件触发！");

            if (NetworkManager.IsClient) {
                // 将army数组转为对应的networkobjectid的数组
                List<ulong> armyNetObjIDs = new List<ulong>();
                foreach (Army army in armies) {
                    armyNetObjIDs.Add(GetArmyID(army));
                }

                // 向服务器发送移动请求
                MoveArmyServerRpc(armyNetObjIDs.ToArray(), destinationID, IsWithdraw);
            } else {
                // 单机模式
                Province destination = mapController.GetProvinceByID(destinationID);
                
                // 为所有军队设置移动事件
                foreach (Army army in armies) {
                    // 获取移动 省份路径
                    uint curProvinceID = army.CurrentProvince.provinceData.provinceID;
                    Province curProvince = mapController.GetProvinceByID(curProvinceID);
                    List<Province> movePath = mapController.GetMovePath(curProvince, destination);

                    // 获取总开销
                    uint totalMoveCost = ProvinceHelper.GetProvinceMoveCost(movePath);

                    ArmyMoveTask armyMoveTask = new ArmyMoveTask(totalMoveCost, army, movePath);
                    army.SetMoveTask(armyMoveTask, IsWithdraw);
                }

            }

        }

        // TODO: 貌似还有些问题
        [ServerRpc(RequireOwnership = false)]
        private void MoveArmyServerRpc(ulong[] armyObjID, uint destinationID, bool IsWithdraw, ServerRpcParams rpcParams = default) {
            //Debug.Log($"服务器接收到移动军队事件,移动终点{destinationID},军队数目{armyObjID.Length}");
            MoveArmyClientRpc(armyObjID, destinationID, IsWithdraw);
        }

        [ClientRpc]
        private void MoveArmyClientRpc(ulong[] armyObjID, uint destinationID, bool IsWithdraw, ClientRpcParams rpcParams = default) {
            
            Province destination = mapController.GetProvinceByID(destinationID);
            
            // 获得对应的军队
            List<Army> armies = GetArmyListByID(armyObjID.ToList());

            // 为所有军队设置移动事件
            foreach (Army army in armies)
            {
                // 获取移动 省份路径
                uint curProvinceID = army.CurrentProvince.provinceData.provinceID;
                Province curProvince = mapController.GetProvinceByID(curProvinceID);
                List<Province> movePath = mapController.GetMovePath(curProvince, destination);

                // 获取总开销
                uint totalMoveCost = ProvinceHelper.GetProvinceMoveCost(movePath);

                ArmyMoveTask armyMoveTask = new ArmyMoveTask(totalMoveCost, army, movePath);
                army.SetMoveTask(armyMoveTask, IsWithdraw);
            }

            //Debug.Log($"成功设置军队移动事件,移动的军队数目{armies.Count}");
        }

        /// <summary>
        /// 让指定的军队静止在原地
        /// </summary>
        public void SetArmyStayInEvent(List<Army> stayTargets) {
            if (!NetworkManager.IsClient) {
                // 单机模式
                ArmyHelper.GetInstance().SetArmyStayIn(stayTargets);
            } else {
                SetArmyStayInClientRpc(
                    GetArmyIDList(stayTargets).ToArray()
                );
            }
            
        }

        [ClientRpc]
        private void SetArmyStayInClientRpc(ulong[] armyObjID, ClientRpcParams rpcParams = default) {
            //Debug.Log($"SetArmyStayIn触发!军队数目:{armyObjID.Length}");
            ArmyHelper.GetInstance().SetArmyStayIn(
                GetArmyListByID(armyObjID.ToList())
            );
        }

        /// <summary>
        /// 让指定的军队退出战斗
        /// </summary>
        public void ExitCombatEvent(List<Army> stayTargets) {
            if (!NetworkManager.IsClient) {
                // 单机模式
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