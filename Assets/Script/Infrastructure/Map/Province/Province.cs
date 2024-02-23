using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.Application.TimeTask;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.Audio;
using WarGame_True.Infrastructure.HexagonGrid.MapObject;
using WarGame_True.Infrastructure.Map.Controller;

namespace WarGame_True.Infrastructure.Map.Provinces{
    /// <summary>
    /// 省份控制类，需要HexGrid 控制省份的UI
    /// </summary>
    [RequireComponent(typeof(HexGrid))]
    public class Province : MonoBehaviour , IPointerClickHandler{
        HexGrid hexGrid;

        [Header("省份持有者")]
        public string provinceController;
        [Header("省份当前控制者")]
        public string provinceCurrentContrl;

        //[Header("省份名称")]
        public string provinceName { get => provinceData.provinceName; }
        public uint provinceID { get => provinceData.provinceID; }
        public string provincePos { get; private set; }
        public Vector3 provincePosition { get; set; }

        #region 管理省份数据
        public ProvinceData provinceData;

        public void SetProvinceData(ProvinceData data) {
            provinceData = data;
        }

        /// <summary>
        /// 省份是否被某个tag所占领
        /// </summary>
        public bool UnderTagControl(string tag) {
            return provinceData.CurrentControlTag == tag;
        }

        /// <summary>
        /// 随时间更改省份数据，需要在国家势力部分，注册到时间结事件中
        /// </summary>
        public void UpdateProvinceData_Day() {
            // 收税和粮食入库
            provinceData.CollectGrainToStorage();
            provinceData.CollectTaxToStorage();

            // TODO:改变荒废度、繁荣度、balabala等数值

            // TODO：刷新ui

            // 更改省份的占领状态

            // 更新省份修正
            provinceData.UpdateProvinceModify();
        }

        public void UpdateProvinceData_Hour() {

        }

        /// <summary>
        /// 获得省份的税收量
        /// </summary>
        public float GetProvinceTaxNum_Month() {
            
            return provinceData.GetProvinceTax_Month();
        }

        public float GetProvinceTaxNum_Day() {
            // NOTICE: 现在改为每日获得一次资源，调用 CollectTaxStorage 函数
            return provinceData.CollectTaxStorage();
        }

        public float GetProvinceMaintenance() {
            return provinceData.GetProvinceMaintenance();
        }

        public float GetProvinceGrainNum_Month() {
            return provinceData.GetProvinceGrain_Month();
        }

        public float GetProvinceGrainNum_Day() {
            // NOTICE: 现在改为每日获得一次资源，调用 CollectGrainStorage 函数

            // TODO: 计算当前省份上的省份控制者的所有军队，扣除军队补给花费，再上缴
            return provinceData.CollectGrainStorage();
        }
        
        public void AddLocalGrainCost() {

        }

        #endregion

        #region 省份 军队数据 管理

        private List<Army> armiesInProvince;

        public int ArmyNumInProvince { get => armiesInProvince.Count; }

        public List<Army> ArmiesInProvince { get => armiesInProvince; }


        private bool OnlyExistAnemiesInProvince(string curCtrlTag, out string anemyTag) {
            bool ans = false;
            anemyTag = "";
            foreach (Army armyInProvince in armiesInProvince) {

                // 撤退中的军队不计入其中
                if ( armyInProvince.ArmyActionState == ArmyActionState.Withdrawing) {
                    continue;
                }

                // 与省份控制者是敌对，省份会被占领
                bool isHostile = PoliticLoader.Instance.IsFactionInWar(curCtrlTag, armyInProvince.ArmyData.ArmyTag);
                if (isHostile) {
                    anemyTag = armyInProvince.ArmyData.ArmyTag;
                    ans = true;
                }

                // 是省份拥有者的军队，则省份不会被占领
                if (curCtrlTag == armyInProvince.ArmyData.ArmyTag) {
                    ans = false;
                    anemyTag = armyInProvince.ArmyData.ArmyTag;
                    return ans;
                }
            }
            return ans;
        }

        public bool ExistArmy() {
            return armiesInProvince != null && armiesInProvince.Count > 0;
        }

        public bool ExistFriendlyArmy() {
            if (armiesInProvince == null) {
                return false;
            }
            // 遍历检查是否是
            foreach (Army army in armiesInProvince)
            {
                if (army.ArmyData.IsArmyTag(provinceData.OwnerTag)) {
                    return true;
                }
            }
            return false;
        }

        public bool ExistHostileArmy() {
            if (armiesInProvince == null) {
                return false;
            }
            foreach (Army army in armiesInProvince) {
                bool isHostile = PoliticLoader.Instance.IsFactionInWar(provinceData.OwnerTag, army.ArmyData.ArmyTag);
                // 检测到敌军部队
                if (isHostile) return true;
            }
            return false;
        }


        public void RemoveArmyFromProvince(Army army) {
            if (armiesInProvince.Contains(army)) {
                armiesInProvince.Remove(army);
            }

            // 更新军队对省份的影响（省份数据）

            // 当省份存在己方部队时,或者无敌方部队时，置空占领
            if (!OnlyExistAnemiesInProvince(provinceData.CurrentControlTag, out string anemyTag)) {
                ForceBreakOccupy();
            }
        }

        public void ArmyArriveInProvince(Army army) {
            if (!armiesInProvince.Contains(army)) {
                armiesInProvince.Add(army);

                // 检查补给路线是否需要重构
                army.ShouldRebuildSupplyLine();
            }

            // 更新军队对省份的影响（省份数据）

            // 当省份只有敌方部队时，判断是否要发生围城
            if (OnlyExistAnemiesInProvince(provinceData.CurrentControlTag, out string anemyTag)) {

                if (currentOccupy == null && !UnderTagControl(anemyTag)) {
                    // 省份未被占领 且没有占领事件 则省份开始被占领
                    Debug.Log("开始占领: amrytag:" + anemyTag + ",当前控制者:" + provinceData.CurrentControlTag + ",拥有者:" + provinceData.OwnerTag);
                    StartAOccupyTask(provinceData.CurrentControlTag, anemyTag);
                } else if (currentOccupy != null && currentOccupy.occupyingTag == anemyTag) {
                    // 已经有占领事件发生，且是己方在占领 则加入
                    currentOccupy.AddOccupyingArmy(army);
                } else {
                    // 已经有占领事件发生，但是是其他方

                }
            }
        }


        public List<Army> GetAnemiesInProvince(string factionTag) {
            List<Army> anemies = new List<Army>();
            foreach (Army armyInProvince in armiesInProvince)
            {
                //若是敌对国家 且不在撤退中 则加入敌人列表中
                bool isHostile = PoliticLoader.Instance.IsFactionInWar(factionTag, armyInProvince.ArmyData.ArmyTag);
                if (isHostile && armyInProvince.ArmyActionState != ArmyActionState.Withdrawing) {
                    anemies.Add(armyInProvince);
                } 
            }

            return anemies;
        }

        /// <summary>
        /// 传入数组，获得数组中静止的军队
        /// </summary>
        public List<Army> GetStayingArmy(List<Army> armies) {
            return (from army in armies
                    where army.ArmyActionState == ArmyActionState.IsStaying
                    select army).ToList();
        }

        public List<Army> GetAlliesInProvince(string factionTag) {
            List<Army> allies = new List<Army>();
            foreach (Army armyInProvince in armiesInProvince) {
                //若是敌对国家 则加入敌人列表中
                bool isAlly = armyInProvince.ArmyData.ArmyTag == factionTag;
                if (isAlly) {
                    allies.Add(armyInProvince);
                }
            }

            return allies;
        }
        
        public void SetInitArmyUI() {
            foreach (Army army in armiesInProvince)
            {
                
            }
        }
        #endregion

        #region 省份 事件（招募、战役、围城）

        // 当前省份正在进行的招募事件
        private List<RecruitTask> recruitTaskList;

        // 当前省份进行的战役, 为null 则表示未发生战役
        public CombatTask currentCombat { get; private set; }
        [Header("combat prefab")]
        public GameObject combatPrefab;
        GameObject currentCombatObject;
        public void SetCurCombatObj(GameObject obj) {
            currentCombatObject = obj;
        }

        // 当前省份进行的占领事务
        public OccupyTask currentOccupy { get; private set; }

        // 当前省份进行的建筑事务
        public List<BuildTask> buildTaskList { get; private set; }

        #endregion

        public void InitProvince(uint provinceID, Vector3 position) {
            hexGrid = GetComponentInChildren<HexGrid>();

            //创建Id
            //provinceID = position.x.ToString() + "_" + position.y.ToString() + "_" + position.z.ToString();
            this.provincePos = provinceID.ToString();
            provincePosition = position;
            //Debug.Log(provincePosition);

            // TODO：写成 仅在inspector操作时会执行此步
            //#if UNITY_EDITOR
            //            //创建一个 ProvinceData (仅在inspector操作时会执行此步)
            // TODO: 考虑到在写联网模块，必须要保持所有玩家本地省份数据一致，所以不能再随机生成数据了
            provinceData = ProvinceData.GetRandomProvinceData(provinceID, position);
            //#endif

            recruitTaskList = new List<RecruitTask>();
            buildTaskList = new List<BuildTask>();

            armiesInProvince = new List<Army>();

            // 如果有省份名称，则设置省份名称
            if (!string.IsNullOrEmpty(provinceData.provinceName)) {
                hexGrid.ShowProvinceName(provinceData.provinceName);
            }

        }

        #region 招募相关

        /// <summary>
        /// 招募按钮 绑定的事件
        /// </summary>
        public void RecruitArmyEvent(ArmyUnitData armyUnitData) {
            StartRecruitArmy(armyUnitData);
            // 通知到其他连接的客户端
            MapNetworkCtrl.Instance.StartRecruitArmyEvent(provinceData.provinceID, armyUnitData);
        }

        public void StartRecruitArmy(ArmyUnitData armyUnitData) {
            //Debug.Log("开始招募军队*1 ： "+ armyUnitData.armyUnitName + "place: " + provincePosition);

            RecruitTask task = new RecruitTask(armyUnitData.armyCostBaseDay, armyUnitData);
            recruitTaskList.Add(task);
            // 显示进度条
            ShowRecruitProcess();
            // 注册到时间事件中 
            TimeSimulator.Instance.SubscribeDayCall(HandleRecruit);
        }

        private void HandleRecruit() {
            if (recruitTaskList == null) {
                return;
            }

            //不存在招募事件，从 TimeSimulator 中移除
            if (recruitTaskList.Count == 0) {
                TimeSimulator.Instance.UnsubscribeDayCall(HandleRecruit);
                return;
            }

            // 仅执行第一个招募事务
            if (recruitTaskList.Count >= 1) {
                recruitTaskList[0].CountTask();
                // 更新招募进度条
                ShowRecruitProcess();
                if (recruitTaskList[0].IsOver == true) {
                    // 联网同步,在服务器创建后，spawn同步
                    ArmyNetworkCtrl.Instance.CreateArmyEvent(recruitTaskList[0].armyUnitData, this.provinceData.provinceID);
                    
                    recruitTaskList.Remove(recruitTaskList[0]);

                    if(recruitTaskList.Count >= 1) {
                        ShowRecruitProcess();
                    } else {
                        HideRecruitProcess();
                    }
                }
            }

            /* 同时执行所有的招募事件 （勿删）
             * for(int i = recruitTaskList.Count - 1; i >= 0; i--) {
                recruitTaskList[i].CountTask();
                hexGrid.ShowRecruitProcess((int)recruitTaskList[i].costDay, (int)recruitTaskList[i].costDay -(int)recruitTaskList[i].lastDay);
                if (recruitTaskList[i].IsOver == true) {
                    //TODO: 招募结束，在省份初始化军队，同时从列表中移除
                    //Debug.Log("招募结束！:" + this.provincePosition + "_" + provinceData.provinceEco.prosperity);
                    //ArmyUnit armyUnitData = new ArmyUnit(recruitTaskList[i].armyUnitData);

                    // 联网同步,在服务器创建后，spawn同步
                    ArmyNetworkCtrl.Instance.CreateArmyEvent(recruitTaskList[i].armyUnitData, this.provinceData.provinceID);
                    // 单机模式下，仅在客户端处创建(将要被替代)
                    //ArmyController.Instance.CreateArmy(armyUnitData, this);
                    hexGrid.HideRecruitProcess();
                    recruitTaskList.Remove(recruitTaskList[i]);
                }
            }*/

        }

        public void ShowRecruitProcess() {
            if (recruitTaskList.Count >= 1) {
                // 若招募队列中有招募事务，则更新该事务的进度，默认第一个
                hexGrid.ShowRecruitProcess(recruitTaskList[0].costTime, recruitTaskList[0].lastTime, recruitTaskList.Count.ToString());
            }
        }

        public void HideRecruitProcess() {
            hexGrid.HideRecruitProcess();
        }

        #endregion

        #region 建筑相关
        // TODO: 尚未测试

        public void BuildEvent(Building building) {
            StartBuild(building);
            // TODO: 通知到其他客户端，进行建筑
        }

        public void StartBuild(Building building) {
            // TODO: 判断本地是否有该建筑，有则不键

            Debug.Log("开启一个建筑事务!");

            BuildTask buildTask = new BuildTask(building);
            buildTaskList.Add(buildTask);
            // 显示进度条
            ShowBuildProcess();
            // 注册到时间结事件
            TimeSimulator.Instance.SubscribeDayCall(HandleBuild);
        }

        public void HandleBuild() {
            if (buildTaskList == null) {
                return;
            }

            //不存在招募事件，从 TimeSimulator 中移除
            if (buildTaskList.Count == 0) {
                TimeSimulator.Instance.UnsubscribeDayCall(HandleBuild);
                return;
            }

            // 仅执行第一个招募事务
            if (buildTaskList.Count >= 1) {
                buildTaskList[0].CountTask();
                ShowBuildProcess();

                // 事务已经结束
                if (buildTaskList[0].IsOver == true) {
                    // TODO:  联网同步 省份数据
                    // TODO: 在ProvinceData中添加该建筑信息
                    provinceData.AddBuilding(buildTaskList[0].CurBuilding);

                    buildTaskList.Remove(buildTaskList[0]);

                    if (buildTaskList.Count >= 1) {
                        ShowBuildProcess();
                    } else {
                        HideBuildProcess();
                    }
                }
            }
        }

        public void ShowBuildProcess() {
            if (buildTaskList.Count >= 1) {
                hexGrid.ShowBuildProcess(buildTaskList[0].costTime, buildTaskList[0].lastTime, buildTaskList.Count.ToString());
            }
        }

        public void HideBuildProcess() {
            hexGrid.HideBuildProcess();
        }

        #endregion

        #region 战役相关

        public void StartCombatEvent(List<Army> attacker, List<Army> defender) {
            Debug.Log("a combat start!" + "attacker num:" + attacker.Count + "defender num:" + defender.Count);

            MapNetworkCtrl.Instance.StartCombatEvent(this, attacker, defender);

        }

        public Combat StartCombatInProvince(List<Army> attacker, List<Army> defender) {
            //开启一个 combat 物体
            currentCombatObject = Instantiate(combatPrefab);
            currentCombatObject.transform.position = transform.position;
            Combat combat = currentCombatObject.GetComponentInChildren<Combat>();
            combat.InitCombat(attacker, defender, this);

            //新建一个combattask
            CombatTask combatTask = new CombatTask(999, attacker, defender);
            currentCombat = combatTask;

            //注册到小时结事件中
            TimeSimulator.Instance.SubscribeHourCall(HandleCombat);

            return combat;
        }

        private void HandleCombat() {
            if (currentCombat == null) {
                return;
            }

            // 时刻更新军队物体位置
            currentCombatObject.GetComponent<Combat>().UpdateArmyPos();

            // 网络同步
            MapNetworkCtrl.Instance.HandleCombat(this);

            // 加载到对战面板上
            UpdateCombatPanel(currentCombat);

            currentCombat.CountTask();
            if (currentCombat.IsOver) {

                // TODO : 1.获取结算数据
                // TODO : 2.将结算数据加载到结算面板上
                CombatMessage combatMessage = currentCombat.CombatCountMessage;

                /*//结束双方对战方的战斗状态
                ArmyHelper.GetInstance().ExitCombat(currentCombat.Attackers);
                ArmyHelper.GetInstance().ExitCombat(currentCombat.Defenders);

                //TODO : 战胜方留在本省份，失败方撤退
                ArmyController.Instance.SetArmyDestination(
                    ArmyController.Instance.withdrawTarget,
                    currentCombat.Attackers, true
                );
                ArmyHelper.GetInstance().SetArmyStayIn(currentCombat.Defenders);*/

                // 销毁战役物体,结束当前战役
                currentCombatObject.GetComponentInChildren<Combat>().CancelCombat(true);

                TimeSimulator.Instance.UnsubscribeHourCall(HandleCombat);
                currentCombat.ForceToComplete();
                MapNetworkCtrl.Instance.CombatOverEvent(provinceData.provinceID);

                //战斗结束
                Debug.Log($"combat over,army in this province:{armiesInProvince.Count}");
            }
        }

        /// <summary>
        /// 控制战役进程，用于客户端在网络同步时使用，不使用CombatTask
        /// </summary>
        public void HandleCombat_Network(BattlePlaceNetwork[] frontAtUnits, BattlePlaceNetwork[] frontDeUnits, BattlePlaceNetwork[] rearAtUnits, BattlePlaceNetwork[] rearDeUnits,
            BattlePlaceNetwork[] withdrawAtUnits, BattlePlaceNetwork[] withdrawDeUnits) {
            // 时刻更新军队物体位置
            if (currentCombatObject != null) {
                currentCombatObject.GetComponent<Combat>().UpdateArmyPos();
            }

            if (currentCombat != null) {
                currentCombat.UpdateCombatUnits(
                    frontAtUnits, frontDeUnits,
                    rearAtUnits, rearDeUnits,
                    withdrawAtUnits, withdrawDeUnits
                );
            }

            // 更新战役界面
            UpdateCombatPanel(currentCombat);

        }

        /// <summary>
        /// 开启一个接收网络同步数据的CombatTask，本地不进行战役逻辑运行，不执行CombatTask的CountTask
        /// </summary>
        public void StartCombatNetwork(List<Army> attackers, List<Army> defenders) {
            if (currentCombat == null) {
                currentCombat = new CombatTask(999, attackers, defenders);
            }
        }

        /// <summary>
        /// 强行结束当地的combatTask
        /// </summary>
        public void CancelCombat() {
            currentCombat = null;
        }

        // 加载到combatPanel面板
        public delegate void UpdateCombatPanelCall(CombatTask combatTask);
        private event UpdateCombatPanelCall updateCombatPanelCall;
        public void UpdateCombatPanel(CombatTask combatTask) {
            if (updateCombatPanelCall != null) {
                updateCombatPanelCall.Invoke(combatTask);
            }
        }

        /// <summary>
        /// 与combatpanel面板建立连接 / 切断连接
        /// </summary>
        public void LinkToCombatPanel(UpdateCombatPanelCall call) {
            updateCombatPanelCall = call;
        }

        public void UnLinkToCombatPanel() {
            updateCombatPanelCall = null;
        }

        #endregion

        #region 占领相关

        private void StartAOccupyTask(string ctrlTag, string occupyTag) {
            List<Army> anemies = GetAnemiesInProvince(ctrlTag);
            
            // TODO: 根据军队、将领、各项属性设定攻城花费时间
            currentOccupy = new OccupyTask(this, anemies, 5);
            hexGrid.ShowOccupyProcess(currentOccupy.totalOccupyCost, currentOccupy.currentOccupyCost);

            //注册到日结事件中
            TimeSimulator.Instance.SubscribeDayCall(HandleOccupy);

        }

        private void HandleOccupy() {
            if (currentOccupy == null) {
                TimeSimulator.Instance.UnsubscribeDayCall(HandleOccupy);
                return;
            }

            // 获得当前省份的所有正在占领的敌人，更新之
            List<Army> occupyingAnemies = GetStayingArmy(GetAnemiesInProvince(provinceData.OwnerTag));
            currentOccupy.UpdateOccupyingArmy(occupyingAnemies);

            // 推进占领进度
            hexGrid.ShowOccupyProcess(currentOccupy.totalOccupyCost, currentOccupy.currentOccupyCost);

            currentOccupy.CountTask();
            if (currentOccupy.IsOver) {

                if (currentOccupy.IsSuccess) {
                    // 事务成功结束，将省份设定为被占领，同时调整省份数值
                    SetProvinceOccupiedStatu(provinceData.OwnerTag, currentOccupy.occupyingTag);
                    Debug.Log("占领行动结束，当前的省份拥有者:" + provinceData.OwnerTag + ",控制者:" + provinceData.CurrentControlTag);

                    // 事务未成功结束
                }

                ForceBreakOccupy();

                // 移除占领事件
                TimeSimulator.Instance.UnsubscribeDayCall(HandleOccupy);
            }
        }

        private void ForceBreakOccupy() {
            // 置空占领事件
            hexGrid.HideOccupyProcess();
            currentOccupy = null;
        }
        
        #endregion

        #region 省份上的军队（战斗、磨损、治安、劫掠、占领）

        #endregion

        #region Colider事件

        private void OnMouseEnter() {
            //鼠标目前在ui上，跳过
            if (EventSystem.current.IsPointerOverGameObject()) {
                return;
            }
            //Debug.Log("you enter this province!");
            hexGrid.SetHexGridActive();

            // 打开省份提示


        }

        private void OnMouseExit() {
            hexGrid.SetHexGridNormal();

            // 关闭省份提示

        }

        private void OnMouseDown() {
            // 使用OnPointerClick
            /*if (EventSystem.current.IsPointerOverGameObject()) {
                return;
            }

            *//* 省份ui已经开启 禁止点击
             * if (!ProvincePanel.ActiveProvincePanel.Visible) {
                ProvincePanel.ActiveProvincePanel.Show();
            } else {
                //省份ui 已经开启
                return;
            }*//*

            ProvincePanel.ActiveProvincePanel.Show();
            ProvincePanel.ActiveProvincePanel.InitProvincePanel(this);


            hexGrid.SetHexGridChoose();*/
        }

        private void OnMouseUp() {
            hexGrid.SetHexGridNormal();
        }

        public void OnPointerClick(PointerEventData eventData) {

            //NOTICE: 使用OnMouseDown时，点击ui会穿透ui触发物体事件，所以改用IPOinterClick

            //展示省份界面UI
            //if (!ProvincePanel.ActiveProvincePanel.Visible) {

            //} else {
            //    //省份ui 已经开启
            //    return;
            //}
            //Debug.Log("you click this province!");
            if (eventData.button == PointerEventData.InputButton.Left) {
                ProvincePanel.ActiveProvincePanel.Show();
                ProvincePanel.ActiveProvincePanel.InitProvincePanel(this);

                hexGrid.SetHexGridChoose();
                AudioManager.PlayAudio(AudioEffectName.ButtonClick01);
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                //军队移动 逻辑，建议放到armycontrl中
            }
        }

        #endregion

        #region 设置省份的状态
        public void SetProvinceControlStatu(string contrl, string currentContrl, UnityEngine.Color color) {
            provinceData.SetOwnerStatu(contrl);
            provinceData.CurrentControlTag = currentContrl;

            SetProvinceOccupiedStatu(provinceData.OwnerTag, provinceData.CurrentControlTag);

            hexGrid.SetHexGridBG(color);
        }

        public void SetProvinceOccupiedStatu(string contrl, string currentContrl) {
            
            if (contrl != currentContrl) {
                // 如果拥有者与控制者 不一样，设置占领状态
                provinceData.SetControlStatu(currentContrl);
                hexGrid.SetHexGridOccupied(
                    PoliticLoader.Instance.GetFactionColor(currentContrl)
                );
            } else {
                hexGrid.SetHexGridUnoccupied();
            }
        }

        public void SetProvinceLostControl() {
            hexGrid.SetHexGridBG(new UnityEngine.Color(1, 1, 1, 0.2f), false);
        }

        public void SetProvinceAsMovePath() {
            hexGrid.SetHexGridAsMovePath();
        }

        public void SetProvinceAsWithdrawPath() {
            hexGrid.SetHexGridAsWithdrawPath();
        }

        public void SetProvinceCloseMovePath() {
            hexGrid.SetHexGridNoInteract();
        }

        public void SetProvinceAsSupplyLine() {
            hexGrid.SetHexGridSupplyLine();
        }

        public void SetProvinceCloseSupplyLine() {
            hexGrid.SetHexGridNoInterBase();
        }

        public void SetProvinceNormal() {
            hexGrid.SetHexGridNormal();
        }
        
        #endregion

        /// <summary>
        /// 获取相邻省份的位置
        /// </summary>
        public List<Vector3> GetNeighborProvinces() {
            List<Vector3> neighborPosition = new List<Vector3>();

            //NOTICE：见HexagonStruct类中，获取邻居节点的部分
            List<Vector3> direction = new List<Vector3> {
                new Vector3(1, 0, -1), 
                new Vector3(1, -1, 0), 
                new Vector3(0, -1, 1),
                new Vector3(-1, 0, 1), 
                new Vector3(-1, 1, 0), 
                new Vector3(0, 1, -1)
            };
            foreach (Vector3 dir in direction)
            {
                neighborPosition.Add(hexGrid.hexPosition + dir);
            }
            
            return neighborPosition;
        }

    }

}
