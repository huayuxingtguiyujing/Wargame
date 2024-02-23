using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.Application.TimeTask;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Infrastructure.Map.Util;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// 一支军团
    /// </summary>
    public class Army : ArmyGUI , ArmyStateMachine {

        // 挂接的armydata
        ArmyData armyData;
        public ArmyData ArmyData { get => armyData;private set => armyData = value; }
        /// <summary>
        ///  结算部队，去除无人力的单位，若军队没有有战斗力的部队，销毁该单位
        /// </summary>
        public void SettlementArmyData() {
            ArmyData.SettleArmyData();

            try {
                if (ArmyData.ArmyUnitDatas.Count <= 0) {
                    RemoveArmy();
                }
            } catch {
                Debug.LogError("无法正常移除有问题的军队!");
            }
            
        }
        
        // 当前的位置
        Province currentProvince;
        public Province CurrentProvince { get => currentProvince;private set => currentProvince = value; }
        
        // 军队当前的状态（移动、静止、战斗）
        private ArmyActionState armyActionState;
        public ArmyActionState ArmyActionState { get => armyActionState; private set => armyActionState = value; }
        

        /*private void Start() {
            //TODO:在start中初始化军队是不合理的，应当交给 armycontroller 集中控制
            InitArmy(4, null);
        }*/

        /// <summary>
        /// 初始化军队 数值、UI显示
        /// </summary>
        public void InitArmy(ArmyUnit armyUnitData, Province province, int num = 1) {
            ArmyData initArmyData = new ArmyData(armyUnitData);
            InitArmy(initArmyData, province);
        }

        public async void InitArmy(ArmyData initArmyData, Province province) {
            
            //armyModelDetector.InitDetector(this);

            this.ArmyData = initArmyData;

            //TODO: 确定军队的归属tag不应该在此执行
            this.ArmyData.ArmyTag = initArmyData.ArmyTag;

            //TODO: 根据归属tag设置军队模型样式
            MeshRenderer render = gameObject.GetComponentInChildren<MeshRenderer>();
            render.material.color = PoliticLoader.Instance.GetFactionColor(this.ArmyData.ArmyTag);

            // 将军队的维护注册到日结事件中
            TimeSimulator.Instance.SubscribeDayCall(ArmyDayCount);

            Sprite generalSprite = await GeneralProfileHelper.GetProfileAsync(ArmyData.CurrentGeneral.GeneralName);
            SetArmyGeneral(generalSprite);
            //SetArmyGeneral(GeneralProfileHelper.GetProfile(ArmyData.CurrentGeneral.GeneralName));

            SetArmyUnChoosen();

            // 设定所在省份
            CurrentProvince = province;
            CurrentProvince.ArmyArriveInProvince(this);

            // 最后更新军队ui、设置将军头像
            UpdateAmryUI(ArmyData, CurrentProvince.ArmyNumInProvince);

        }

        private void OnDestroy() {
            // 撤去注册的事件
            TimeSimulator.Instance.UnsubscribeDayCall(ArmyDayCount);
        }

        #region 对军队的操作：移动、合并、拆分、etc

        // 军队当前的移动事务
        private ArmyMoveTask currentMoveTask;

        public void SetMoveTask(ArmyMoveTask armyMoveTask, bool IsWithdraw) {
            // 处于不能移动的状态
            if (!AbleToMoveArmy()) {
                return;
            }

            //隐藏原先的移动路径
            HideMovePath();

            //设置军队归位
            SetArmyStayIn();

            currentMoveTask = armyMoveTask;

            if (IsWithdraw) {
                //设置军队撤退
                SetArmyWithdraw(currentMoveTask.PeekProvince);
            } else {
                //设置军队移动
                SetArmyMoving(currentMoveTask.PeekProvince);
            }

            //如果该军队被选中,则显示移动路径
            if (ArmyController.Instance.IsArmyChoosen(this)) {
                ShowMovePath();
            }

            //注册到 timesimulator 的小时结事件中
            TimeSimulator.Instance.SubscribeHourCall(HandleMoveArmy);
        }

        private void HandleMoveArmy() {
            // 当前没有移动事务，取消注册
            if (currentMoveTask == null) {
                TimeSimulator.Instance.UnsubscribeHourCall(HandleMoveArmy);
            }

            currentMoveTask.CountTask();
            if (currentMoveTask != null && currentMoveTask.IsOver) {
                TimeSimulator.Instance.UnsubscribeHourCall(HandleMoveArmy);

                //关闭正在移动状态
                if (ArmyActionState == ArmyActionState.Withdrawing) {
                    ForceArmyStayIn();
                } else {
                    SetArmyStayIn();
                }
            }
        }

        /// <summary>
        /// 移动军队位置到指定的省份
        /// </summary>
        public void MoveArmy(Province province) {
            
            CurrentProvince.RemoveArmyFromProvince(this);

            CurrentProvince = province;

            CurrentProvince.ArmyArriveInProvince(this);

            //设置军队的位置到指定的省份
            SetArmyGUIPos(CurrentProvince.transform.position);

            //检测是否发生战斗
            CheckCombat();
        }

        /// <summary>
        /// 突发事件，打断军队移动，如战斗etc
        /// </summary>
        public void ForceArmyStayIn() {
            //判断当前是否有移动事务，若有，则结束之
            HideMovePath();
            if (currentMoveTask != null) {
                currentMoveTask.ForceToComplete();
                TimeSimulator.Instance.UnsubscribeHourCall(HandleMoveArmy);
                currentMoveTask = null;
            }

            armyActionState = ArmyActionState.IsStaying;

            SetArmyGUIPos(CurrentProvince.transform.position);
        }

        /// <summary>
        /// 显示当前的移动路径
        /// </summary>
        public void ShowMovePath() {
            if (currentMoveTask == null || currentMoveTask.IsOver) {
                return;
            }
            //Debug.Log("path count:" + currentMoveTask.MovePath.Count);
            if (ArmyActionState == ArmyActionState.IsMoving) {
                ProvinceHelper.SetProvinceMoving(currentMoveTask.MovePath);
            } else if (ArmyActionState == ArmyActionState.Withdrawing) {
                ProvinceHelper.SetProvinceWithdrawing(currentMoveTask.MovePath);
            }
        }

        public void HideMovePath() {
            if (currentMoveTask == null || currentMoveTask.IsOver) {
                return;
            }
            //Debug.Log("path count:" + currentMoveTask.MovePath.Count);
            ProvinceHelper.SetProvinceCloseMovePath(currentMoveTask.MovePath);
        }
        
        public bool AbleToMergeArmy() {
            return ArmyActionState != ArmyActionState.IsInCombat && ArmyActionState != ArmyActionState.Withdrawing;
        }

        public void MergeArmy(Army army) {
            army.CurrentProvince.RemoveArmyFromProvince(army);
            ArmyData.AddArmyData(army.ArmyData);
        }

        public bool AbleToSplitArmy() {
            return ArmyData.ArmyUnitDatas.Count > 1;
        }

        public ArmyData SplitArmy() {
            ArmyData splitResult = ArmyData.SplitArmyData();
            UpdateAmryUI(ArmyData, CurrentProvince.ArmyNumInProvince);
            return splitResult;
        }

        /// <summary>
        /// 移除该军队单位
        /// </summary>
        public void RemoveArmy() {
            // 移除军队前，先停下
            ForceArmyStayIn();

            CurrentProvince.RemoveArmyFromProvince(this);
            
            // 处于联网或单机模式有不同的销毁处理
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if(networkObject == null || !networkObject.IsSpawned) {
                // 单机模式 直接销毁
                Destroy(this.gameObject);
            } else {
                // 联机模式 仅有持有者可以销毁
                if (networkObject.IsSpawned && networkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId) {
                    networkObject.Despawn();
                    Destroy(this.gameObject);
                }
            }
        }
        
        #endregion

        #region 军队战斗相关

        /// <summary>
        /// 检测战斗是否发生
        /// </summary>
        private void CheckCombat() {
            //CurrentProvince.;

            //TODO：检测战斗是否已经发生了，如果发生，则判断是否要加入之


            // 检测当前省份是否存在敌对军队
            List<Army> anemies = CurrentProvince.GetAnemiesInProvince(armyData.ArmyTag);
            if(anemies.Count <= 0) {
                //未发生战斗
                return;
            } else {
                //发生战斗

                //1. 使得amry和敌人进入战斗状态
                foreach (Army anemy in anemies)
                {
                    anemy.EnterCombat();
                }

                //2. 获得省份中的所有相同tag的army单位，加入战斗
                List<Army> allies = CurrentProvince.GetAlliesInProvince(armyData.ArmyTag);
                foreach (Army ally in allies)
                {
                    ally.EnterCombat();
                }
                
                //3. 省份触发战役
                CurrentProvince.StartCombatEvent(allies, anemies);

                //4. 唤起战斗界面 TODO: 应该放在armyController中，在选中了处于战斗状态军队时 弹出
                ArmyController.Instance.InvokeCombatPanel(this);
            }
        }

        public void EnterCombat() {
            //停止移动，结束移动事务
            if (ArmyActionState == ArmyActionState.IsMoving) {
                SetArmyStayIn();
            } else if (ArmyActionState == ArmyActionState.Withdrawing) {
                // 需要确保 撤退中的军队不会被追击
                return;
            }

            ArmyActionState = ArmyActionState.IsInCombat;
        }

        public void ExitCombat() {
            ArmyActionState = ArmyActionState.IsStaying;
        }

        #endregion

        #region 军队状态机
        //TODO : 将涉及军队状态变化的方法全部集中到这些接口中

        public bool AbleToMoveArmy() {
            return this.ArmyActionState != ArmyActionState.IsInCombat && this.ArmyActionState != ArmyActionState.Withdrawing;
        }

        /// <summary>
        /// 开启军队移动状态，设定移动的目标
        /// </summary>
        public void SetArmyMoving(Province province) {
            //不能往本身所在的省份移动
            if (province == CurrentProvince) return;

            if (province == null) province = CurrentProvince;

            armyActionState = ArmyActionState.IsMoving;
            //让模型看起来像是在移动一样（就是把位置偏向指定省份的方向）
            SetArmyGUIMoving(CurrentProvince.transform.position, province.transform.position);
        }

        public bool AbleToStayIn() {
            return this.ArmyActionState != ArmyActionState.IsInCombat;
        }

        /// <summary>
        /// 让军队停止行动，停在当前所在的省份
        /// </summary>
        public void SetArmyStayIn() {
            if (!AbleToStayIn()) {
                return;
            }

            //强制让军队结束移动
            ForceArmyStayIn();
        }

        public bool AbleToWithdraw() {
            //TODO : 实现撤退判断
            // 当士气在一定值时，可以撤退
            return true;
        }

        /// <summary>
        /// 让军队撤退
        /// </summary>
        public void SetArmyWithdraw(Province province) {
            //TODO : 结束战役，结束战斗
            if (ArmyActionState == ArmyActionState.IsInCombat) {

            }

            SetArmyMoving(province);
            //Debug.Log("军队正在撤退中!");
            // 设置军队移动，再设置状态为撤退（为移动上锁）
            armyActionState = ArmyActionState.Withdrawing;
        }

        #endregion


        #region 军队状态更新: 补给、磨损、士气恢复、维护费用、人力恢复

        public ArmySupplyTask CurSupplyTask { get; private set; }

        public void ArmyDayCount() {

            // 每日更新结算军队的各项数值
            CountArmyStatu();

            // 

            // TODO：
        }

        /// <summary>
        /// 随时间自动恢复军队的各项数值
        /// </summary>
        public void CountArmyStatu() {
            if(armyActionState == ArmyActionState.IsInCombat) {
                return;
            }

            // TODO: 在provinceData中新添加一个 本地补给 的字段，用于作为localGrain的来源
            float localGrain = CurrentProvince.provinceData.GetGrainProduce_Day();
            ArmyData.CountArmyStatu(localGrain);

            // TODO：目前是有问题的！
            int armyNumInProvince = 1;
            if (CurrentProvince != null) {
                armyNumInProvince = CurrentProvince.ArmyNumInProvince;
            }
            
            UpdateAmryUI(ArmyData, armyNumInProvince);
        }

        public void RecoverArmy_Immediately(float Morale, float Supply, uint Manpower) {
            ArmyData.RecoverArmy_Immediately(Morale, Supply, Manpower);
        }

        // TODO: 完成这部分逻辑
        // 这只军队已经拿到工资啦（晚唐的骄兵悍将，你懂的）
        public void MakeArmyPaied() {

        }

        // TODO: 完成这部分逻辑
        public void MakeArmyUnpaied() {
            // TODO: 欠钱，军队UI发生改变
        }
        
        /// <summary>
        /// 该只军队目前是否满足输送补给的条件
        /// </summary>
        public bool CanGetSupply { get {
            // 满足以下两个条件时，可以为军队运输补给
            // 1.当前补给事务不为空
            // 2.CurSupplyTask中允许 GetSupply
            return CurSupplyTask != null && CurSupplyTask.CanGetSupply();
        }}

        /// <summary>
        /// 申请建立补给线
        /// </summary>
        public void SetSupplyTask(ArmySupplyTask supplyTask) {
            // TODO: 向Faction申请建立粮道，用最短路径算法balabala
            if (CurSupplyTask != null) {
                CurSupplyTask.ForceToComplete();
            }
            CurSupplyTask = supplyTask;
            TimeSimulator.Instance.SubscribeDayCall(HandleSupplyTask);
        }
        
        public void HandleSupplyTask() {
            if (CurSupplyTask == null) {
                return;
            }

            CurSupplyTask.CountTask();
            //ArmyController.Instance.

            if (CurSupplyTask.IsOver) {
                // 如果补给事务 到期了，再重开并刷新

            }
        }

        /// <summary>
        /// 结束当前军队的补给线事务, 交由外界调用
        /// </summary>
        public void CancelSupplyTask() {
            if (CurSupplyTask != null) {
                CurSupplyTask.ForceToComplete();
            }
            // 取消时间结事件
            TimeSimulator.Instance.UnsubscribeDayCall(HandleSupplyTask);
            CurSupplyTask = null;
        }

        public void ShouldRebuildSupplyLine() {
            if(CurSupplyTask == null) {
                return;
            }
            CurSupplyTask.ShouldRebuildSupplyLine();
        }

        #endregion

        #region 将领
        public async void ChangeGeneral(General general) {
            // 更改ArmyData中的general
            ArmyData.AssignGeneral(general);
            // 更改将领头像
            Sprite sprite = await GeneralProfileHelper.GetProfileAsync(general.GeneralName);
            SetArmyGeneral(sprite);
        }

        #endregion

        /// <summary>
        /// 更新军队UI
        /// </summary>
        public void UpdateArmyUI() {
            // 根据ArmyData旗下的所有ArmyUnit的状态更新之
            armyData.ReCaculateStatu();
            UpdateAmryUI(armyData, CurrentProvince.ArmyNumInProvince);
        }

    }


    public enum ArmyActionState {
        IsStaying,
        IsMoving,
        IsInCombat,
        Withdrawing,
        NoOtherState
    }


    /// <summary>
    /// 军队目前状态的状态机
    /// </summary>
    public interface ArmyStateMachine {
        public void EnterCombat();
        public void ExitCombat();

        public bool AbleToStayIn();
        public void SetArmyStayIn();

        public bool AbleToMoveArmy();
        public void SetArmyMoving(Province province);

        public bool AbleToWithdraw();
        public void SetArmyWithdraw(Province province);
    }
}