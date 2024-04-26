using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.Application.TimeTask;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.Infrastructure.AI;
using WarGame_True.Infrastructure.Audio;
using WarGame_True.Infrastructure.Map.Controller;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.Politic {
    /// <summary>
    /// 用于记录一个派系的所有信息
    /// </summary>
    [System.Serializable]
    public class Faction : MonoBehaviour {

        [Header("用于区分 势力的关键信息")]
        private FactionInfo factionInfo;
        public FactionInfo FactionInfo { get => factionInfo;private set => factionInfo = value; }

        public string FactionTag { get => FactionInfo == null ? "NoTag" : FactionInfo.FactionTag; }

        public bool UseAI = true;

        private FactionAI FactionAI;

        #region 势力的资源
        FactionResource resource;
        public FactionResource Resource { get => resource; private set => resource = value; }
        
        /// <summary>
        /// 获取该势力所有的人力 数目
        /// </summary>
        public long GetTotalManpower() {
            Resource.CaculateTotalManpower(Territory.ToArray());
            return Resource.TotalManpower;
        }
        
        public int GetTotalIncome() {
            return Resource.GetTotalIncome() - Resource.GetTotalOutcome();
        }

        // 更改税率、征粮、维护费用的回调，目前用于网络同步
        public delegate void ChangeLevelDelegate(string factionTagStr, TaxLevel taxLevel, ExpropriateGrainLevel grainLevel, MaintenanceLevel maintenanceLevel);
        
        public ChangeLevelDelegate ChangeLevelEvent;

        public void ChangeTaxLevel(TaxLevel taxLevel) {
            Resource.ChangeTaxRate(taxLevel, Territory.ToArray());
            if(ChangeLevelEvent != null) {
                ChangeLevelEvent(factionInfo.FactionTag, Resource.CountryTaxLevel, Resource.CountryExpropriateGrainLevel, Resource.CountryMaintenance);
            }
        }

        public void ChangeExproGrainLevel(ExpropriateGrainLevel grainLevel) {
            Resource.ChangeExpropriateGrain(grainLevel, Territory.ToArray());
            if (ChangeLevelEvent != null) {
                ChangeLevelEvent(factionInfo.FactionTag, Resource.CountryTaxLevel, Resource.CountryExpropriateGrainLevel, Resource.CountryMaintenance);
            }
        }

        public void ChangeMaintenanceLevel(MaintenanceLevel maintenanceLevel) {
            Resource.ChangeMaintenanceRate(maintenanceLevel, Territory.ToArray());
            if (ChangeLevelEvent != null) {
                ChangeLevelEvent(factionInfo.FactionTag, Resource.CountryTaxLevel, Resource.CountryExpropriateGrainLevel, Resource.CountryMaintenance);
            }
        }
        
        public void SetLevelDirectly(TaxLevel taxLevel, ExpropriateGrainLevel grainLevel, MaintenanceLevel maintenanceLevel) {
            Resource.ChangeTaxRate(taxLevel, Territory.ToArray());
            Resource.ChangeExpropriateGrain(grainLevel, Territory.ToArray());
            Resource.ChangeMaintenanceRate(maintenanceLevel, Territory.ToArray());
        }
        
        #endregion


        #region 外交关系
        // 外交关系图 Tag - 外交关系 的映射
        // 起始是该剧本的所有势力
        // 会根据场上已有的所有势力初始化之
        Dictionary<string, DiploRelation> diploRelations = new Dictionary<string, DiploRelation>();
        public void AddDiploRelation(Faction hostFaction, Faction targetFaction) {
            if (hostFaction.FactionInfo.FactionTag == targetFaction.FactionInfo.FactionTag) {
                // 不会建立与自身的外交关系
                return;
            }

            bool isRival = hostFaction.FactionInfo.IsRival(targetFaction.FactionInfo);
            DiploRelation diploRelation = new DiploRelation(
                hostFaction.FactionInfo, targetFaction.FactionInfo, false, false, isRival, false
            );
            diploRelations.Add(targetFaction.FactionInfo.FactionTag, diploRelation);

            //Debug.Log("外交关系建立:" + diploRelation.hostFaction.FactionTag + "_" + diploRelation.targetFaction.FactionTag);
        }

        public DiploRelation GetDiploRelation(string tag) {
            if (diploRelations.ContainsKey(tag)) {
                return diploRelations[tag];
            } else {
                return new DiploRelation();
            }
        }

        /// <summary>
        /// 获得该国家所有的外交关系信息
        /// </summary>
        public List<DiploRelation> GetAllDiploRelations() {
            return diploRelations.Values.ToList();
        }

        public bool IsFactionInWar() {
            foreach (var dipRelation in diploRelations.Values.ToList())
            {
                // 仅检查与该国外交关系 IsInWar字段
                if (dipRelation.IsInwar) {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 军队与将领
        // NOTICE: 军队的联网同步交由ArmyController来控制
        // 当前国家的所有军队
        private List<Army> factionArmys;
        public List<Army> FactionArmys { get => factionArmys; set => factionArmys = value; }

        public int GetTotalArmyCount(ref int ArmyValue) {
            ArmyValue = 0;
            if (FactionArmys != null) {
                // 遍历 所有军队 计算维护费的开销总和
                foreach (var army in FactionArmys)
                {
                    ArmyValue += (int)army.ArmyData.GetArmyMaintenanceCost() / 1000;
                }
            }
            return FactionArmys.Count;
        }

        /// <summary>
        /// 获取当前正在招募的军队数目
        /// </summary>
        /// <param name="RecrValue">正在招募的部队的权重（Cost）</param>
        /// <returns></returns>
        public int GetRecruitingArmyCount(ref int RecrValue) {
            int ans = 0;
            RecrValue = 0;
            foreach (var province in Territory)
            {
                if(province.recruitTaskList != null) {
                    foreach (var task in province.recruitTaskList)
                    {
                        // 部队权重: 维持该只部队的开销 (要除1000)
                        RecrValue += (int)task.armyUnitData.armyReinforcementCost / 1000;
                    }
                    ans += province.recruitTaskList.Count;
                }
            }
            return ans;
        }
        
        /// <summary>
        /// 获得所有军队人数总数
        /// </summary>
        public ulong GetTotalArmyNum() {
            ulong num = 0;
            FactionArmys.ForEach(army => {
                army.ArmyData.RecaculateArmyUnitManNums();
                num += army.ArmyData.ArmyNum;
            });
            return num;
        }

        public void AddNewArmy(Army army) {
            if (FactionArmys == null) {
                FactionArmys = new List<Army>();
            }
            if (!FactionArmys.Contains(army)) {
                FactionArmys.Add(army);
            }
        }

        public void RemoveArmy(Army army) {
            if(FactionArmys == null) {
                FactionArmys = new List<Army>();
            }
            if (FactionArmys.Contains(army)) {
                FactionArmys.Remove(army);
            }
        }


        /// <summary>
        /// 更新军队补给线
        /// </summary>
        public void UpdateArmySupplyLine(List<Army> FactionArmys) {
            foreach (var army in FactionArmys)
            {
                // 该只军队需要 从外运输 补给
                if (army.ArmyData.NeedSupplySupport) {
                    BuildSupplyLine(army);
                } else {
                    CancelSupplyLine(army);
                }
            }
        }


        // 将领操作回调
        public delegate void GeneralChangeDelegate(string generalTag);

        // 当前国家的所有将领
        private List<General> factionGenerals;
        public List<General> FactionGenerals { get => factionGenerals; set => factionGenerals = value; }

        public General GetGeneralByTag(string tag) {
            foreach (General general in FactionGenerals)
            {
                if(general.GeneralTag == tag) {
                    return general;
                }
            }
            return null;
        }

        /// <summary>
        /// 为势力新建将领，移除势力中的将领
        /// </summary>
        public void CreateGeneral(General general) {
            if (FactionGenerals.Contains(general)) return;
            FactionGenerals.Add(general);
        }

        //public void Action<string> RemoveGeneralDelegate;

        public void RemoveGeneral(General general) {
            if (!FactionGenerals.Contains(general)) return;
            FactionGenerals.Remove(general);
        }

        /// <summary>
        /// 指派将领到军队，移除任命
        /// </summary>
        public void AssignGeneralToArmy(General general, Army army) {
            army.ChangeGeneral(general);
        }

        public void RemoveGeneralFromArmy(General general) {
            if (IsInAssign(general)) {
                Army targetArmy = GetGeneralArmy(general);
                targetArmy.ChangeGeneral(General.GetNoLeader(FactionInfo.FactionTag));
            }
        }
        
        /// <summary>
        /// 判断将领是否已经被任命
        /// </summary>
        public bool IsInAssign(General general) {
            foreach (Army army in FactionArmys)
            {
                if(army.ArmyData.CurrentGeneral.Equals(general)) {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 返回将领所在的军队单位
        /// </summary>
        /// <returns></returns>
        public Army GetGeneralArmy(General general) {
            foreach (Army army in FactionArmys) {
                if (army.ArmyData.CurrentGeneral.Equals(general)) {
                    return army;
                }
            }
            return null;
        }
        #endregion

        #region 管理控制区域

        public Province GetCapital() {
            if (string.IsNullOrEmpty(factionInfo.CapitalProvinceID)) {
                // 首都字段为空的情况
                return Territory[0];
            }

            // 根据首都的ID 寻找对应的省份
            uint capitalID = uint.Parse(factionInfo.CapitalProvinceID);
            Province capital = MapController.Instance.GetProvinceByID(capitalID);
            if (capital != null) {
                return capital;
            } else {
                return Territory[0];
            }
        }

        // 当前拥有的地区
        public List<Province> Territory;
        // 也是当前拥有的区域 只不过是string类型（x坐标_y坐标_z坐标）
        public List<string> TerritoryInitString;

        // 当前所有的补给中心
        public List<Province> AllSupplyCenter;

        public int GetProvinceNum() {
            return Territory.Count;
        }

        /// <summary>
        /// 随机获取一个合适的撤退省份目标(仅本国领土)
        /// </summary>
        public Province GetWithdrawTarget() {
            // TODO：应当计算省份撤退权重，选择离军队最近、最安全的省份撤退

            // 目前的逻辑 随机撤退到一个 未开战 未围城 未被占领 的省份上
            Province ans = null;
            int territoryCount = Territory.Count;
            int count = 0;
            while(ans == null) {
                System.Random rnd = new System.Random();
                int index = rnd.Next(territoryCount);
                if (Territory[index].currentCombat == null 
                    && Territory[index].currentOccupy == null
                    && !Territory[index].provinceData.UnderControlByOwner()
                    ) {
                    ans = Territory[index];
                }

                // 省份中已经没有合适的可供撤退的领土了 防止无限循环
                count++;
                if (count >= territoryCount) {
                    return Territory[index];
                }
            }
            return ans;
        }
        
        /// <summary>
        /// 重新更新所有带有补给中心的省份
        /// </summary>
        public List<Province> UpdateAllSupplyCenter() {
            if (AllSupplyCenter == null) {
                AllSupplyCenter = new List<Province>();
            }
            AllSupplyCenter.Clear();
            foreach (var province in Territory)
            {
                // 如果存在 SupplyCenter 那么添加进入
                if (province.provinceData.ExistSupplyCenter()) {
                    AllSupplyCenter.Add(province);
                }   
            }
            return AllSupplyCenter;
        }

        public void RegisterSupplyCenter(uint provinceID) {
            Province province = MapController.Instance.GetProvinceByID(provinceID);
            if(province == null) {
                return;
            }

            if (!AllSupplyCenter.Contains(province)) {
                AllSupplyCenter.Add(province);
            }
        }

        public void UnregisterSupplyCenter(uint provinceID) {
            Province province = MapController.Instance.GetProvinceByID(provinceID);
            if (province == null) {
                return;
            }
            // 如果 已经记录该省份 则移除
            if (AllSupplyCenter.Contains(province)) {
                AllSupplyCenter.Remove(province);
            }
        }

        /// <summary>
        /// 为指定的军队建立/维护一条合适的粮道/补给线
        /// </summary>
        public void BuildSupplyLine(Army targetArmy) {
            if(targetArmy.CurSupplyTask != null) {
                // TODO: 如果军队有 SupplyTask 则刷新之
                return;
            } else {
                // TODO: 如果军队没有 SupplyTask 则构建一个 SupplyTask
                // TODO: 从起点（国家首都/最近的补给中心） 到终点（军队所在位置），构建出一条合理的路径
                Province capital = GetCapital();
                List<Province> supplyPath = MapController.Instance.GetSupplyPath(capital, targetArmy.CurrentProvince);
                ArmySupplyTask armySupplyTask = new ArmySupplyTask(targetArmy, capital, supplyPath);

                // TODO: 获取离当前军队最近的 补给中心

                // TODO: 标记为粮道的省份优先（用户可以自定义粮道的位置）

                // TODO: 调用 Army 的 SetSupplyTask;
                targetArmy.SetSupplyTask(armySupplyTask);

                //Debug.Log("建立了补给事务，出发点为: " + capital.provinceData.provinceID);
            }

            return;
        }

        /// <summary>
        /// 结束指定军队的补给线事务
        /// </summary>
        public void CancelSupplyLine(Army targetArmy) {
            if (targetArmy.CurSupplyTask == null) {
                // 该只军队没有 supplyTask
                return;
            }

            targetArmy.CancelSupplyTask();
        }

        #endregion

        #region 构造函数
        public static Faction GetBaseFaction(FactionInfo factionInfo, Transform parentTransform) {
            GameObject factionObj = new GameObject();
            factionObj.transform.parent = parentTransform;
            factionObj.name = factionInfo.FactionName;

            //添加组件 配置faction信息
            Faction factionComponent = factionObj.AddComponent<Faction>();
            factionComponent.FactionInfo = factionInfo;
            factionComponent.TerritoryInitString = factionInfo.ProvincesInBeginning;
            factionComponent.Territory = new List<Province>();
            factionComponent.Resource = new FactionResource();

            // 获取将领
            factionComponent.FactionArmys = new List<Army>();
            factionComponent.FactionGenerals = new List<General>();
            foreach (GeneralData data in factionInfo.FactionGenerals)
            {
                factionComponent.CreateGeneral(new General(data));
            }

            return factionComponent;
        }

        public void InitFaction() {

            // 注册时间结算事件
            TimeSimulator.Instance.SubscribeMonthCall(UpdateFaction_Month);
            TimeSimulator.Instance.SubscribeDayCall(UpdateFaction_Day);
            TimeSimulator.Instance.SubscribeHourCall(UpdateFaction_Hour);
        }

        private void OnDestroy() {
            TimeSimulator.Instance.UnsubscribeMonthCall(UpdateFaction_Month);
            TimeSimulator.Instance.UnsubscribeDayCall(UpdateFaction_Day);
            TimeSimulator.Instance.UnsubscribeHourCall(UpdateFaction_Hour);
            
        }
        
        #endregion

        /// <summary>
        /// 注册到日结事件中
        /// </summary>
        public void UpdateFaction_Day() {

            // 支付省份维护费
            Resource.PayMaintenance(Territory.ToArray());
            // 刷新省份状态
            foreach (Province province in Territory)
            {
                province.UpdateProvinceData_Day();
            }

            // 从省份中获得税收和粮食
            // NOTICE:本来应该是每月获得资源的，但防止太复杂，改为每日
            if (Territory.Count > 0) {
                Resource.GetTaxAndGrain_Day(Territory.ToArray());
            }

            // 支付军队维护费用（财赋、粮食）
            Resource.PayArmy(FactionArmys);
            Resource.PaySupply(FactionArmys);

            // TODO: 军队的粮食消耗，粮道的维护，
            UpdateArmySupplyLine(FactionArmys);

            // NOTICE: UpdateArmyData_Day 应该放到Army中处理，而不是这里，否则无法兼容叛军系统

            // TODO: 建筑的维护

            // TODO: 播放获得金钱音频
            AudioManager.PlayAudio(AudioEffectName.GainResource);
        }

        public void UpdateFaction_Hour() {

            foreach (Province province in Territory) {
                province.UpdateProvinceData_Hour();
            }


        }

        public void UpdateFaction_Month() {

            // 获得税收 和 粮食
            //if (Territory.Count > 0) {
            //    Resource.GetTaxAndGrain_Month(Territory.ToArray());
            //}
            

        }

    }
}