using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Infrastructure.BT;
using WarGame_True.Infrastructure.Map.Controller;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.Infrastructure.AI {
    public class FactionAI : MonoBehaviour {

        public Faction faction;

        // 当前记录在FactionAi中，可以让军队行走的省份地图
        public List<Province> CurAbleMap { get; private set; }

        public Dictionary<Vector3, ProvinceWeight> CurMapWeightDic { get; private set; }

        public AIType AIType;

        SequenceNode RootNode;

        #region 当前势力 各项资源

        bool IsInWar;

        int ArmyNum;
        int ArmyValue;
        int RecrtingArmyNum;        // 正在招募的部队的数目
        int RecrtingArmyValue;      // 正在招募的部队的权重值（所有在招募的部队的 ReinforceCost）

        int MoneyStorage;
        int MoneyOutCome;
        int MoneyInCome;

        int GrainStorage;
        int GrainOutCome;
        int GrainInCome;

        #endregion

        #region 行为树AI 相关变量

        private int recruitValue = 0;

        private int recruitCooling = 0;


        #endregion

        private bool HasInited = false;

        public static FactionAI GetFactionAI(Faction faction) {
            FactionAI aiComponent = faction.gameObject.AddComponent<FactionAI>();
            aiComponent.InitFactionAI(faction);
            return aiComponent;
        }

        public void InitFactionAI(Faction faction) {
            // 仅进行一次初始化
            if (HasInited) {
                //Debug.Log($"Tag: {faction.FactionTag}的AI 已被初始化");
                return;
            }

            this.faction = faction;
            AIType = AIType.GetAmbitiousAIType();
            CurAbleMap = new List<Province>();
            CurMapWeightDic = new Dictionary<Vector3, ProvinceWeight>();

            // 根据 当前可行走的省份 构建 势力图
            AddMap(faction);

            // 建立行为树
            BuildBTree();

            // 挂接更新事件
            TimeSimulator.Instance.SubscribeHourCall(RunAI_Hour);
            TimeSimulator.Instance.SubscribeDayCall(RunAI_Day);

            HasInited = true;
            Debug.Log($"Tag: {faction.FactionTag}的AI已成功初始化, CurMap: {CurAbleMap.Count}");
        }

        private void BuildBTree() {
            RootNode = new SequenceNode("root");

            // 是否应该新招募部队
            ConditionNode ShouldRecruit = new ConditionNode("ShouldRecruit", ShouldRecruitArmy);
            ActionNode RecruitArmy = new ActionNode("RecruitArmy", RecruitSuitArmyNum);
            ShouldRecruit.AddChildren(RecruitArmy);
            RootNode.AddChildren(ShouldRecruit);

            // 是否应该去除一些部队
            ConditionNode ShouldRemove = new ConditionNode("ShouldRemove", ShouldRemoveArmy);
            ActionNode RemoveArmy = new ActionNode("RemoveArmy", RemoveSuitArmyNum);
            ShouldRemove.AddChildren(RemoveArmy);
            RootNode.AddChildren(ShouldRemove);

            // 分配部队到合适的位置
            ActionNode Distribute = new ActionNode("Distribute", DistributeArmy);
            RootNode.AddChildren(Distribute);

        }

        private void OnDestroy() {
            TimeSimulator.Instance.UnsubscribeHourCall(RunAI_Hour);
            TimeSimulator.Instance.UnsubscribeDayCall(RunAI_Day);
        }

        public void RunAI_Hour() {
            if (!enabled) {
                return;
            }
            // 每小时更新
            UpdateCurResource();
            // 运行行为树
            RootNode.Excute();
        }

        public void RunAI_Day() {
            if (!enabled) {
                return;
            }
            
            UpdateMap(CurAbleMap);
        }

        #region 行为树 节点事件

        /// <summary>
        /// 更新当前国家的资源-Action
        /// </summary>
        public void UpdateCurResource() {
            IsInWar = faction.IsFactionInWar();
            
            ArmyNum = faction.GetTotalArmyCount(ref ArmyValue);
            RecrtingArmyNum = faction.GetRecruitingArmyCount(ref RecrtingArmyValue);

            // 更新当前金钱
            // NOTICE: 金钱单位要除以1000 因为1000￥能供养一支最基本的步兵单位
            MoneyStorage = faction.Resource.Money / 1000;
            MoneyInCome = faction.Resource.GetTotalIncome() / 1000;
            MoneyOutCome = faction.Resource.GetTotalOutcome() / 1000;

            // 更新当前粮食
            GrainStorage = (int)faction.Resource.GrainDeposits;
            GrainInCome = faction.Resource.GetTotalGrainIncome();
            GrainOutCome = faction.Resource.GetTotalGrainOutcome();

        }

        /// <summary>
        /// 判断是否应该新建军队-Condition
        /// </summary>
        public bool ShouldRecruitArmy() {
            int recruitValue = 0;

            // 计算当前已有军队的影响
            // NOTICE: 即为  - 当前正在招募的军队的维护总费
            recruitValue +=  - RecrtingArmyValue;

            // 计算财政与粮食的影响
            recruitValue += Mathf.Min((MoneyInCome - MoneyOutCome), (GrainInCome - GrainOutCome));

            // 是否处于赤字，则根据 AI 类型 和 当前资金储备 和 判断是否能新建
            bool isMoneyInDeficit = ((MoneyInCome - MoneyOutCome) < 0);
            if (isMoneyInDeficit) {
                recruitValue += AIType.GetAdmWeight(-6, -5, -5);
            }

            bool isGrainInDeficit = ((GrainInCome - GrainOutCome) < 0);
            if (isGrainInDeficit) {
                recruitValue += AIType.GetAdmWeight(-6, -5, -5);
            }

            // 计算金钱对于 招募 权重的影响
            if (MoneyStorage < MoneyInCome * 3) {
                // 金钱仅有3天的收入储备
                recruitValue += AIType.GetMilWeight(-9, -8, -7);
                recruitValue += AIType.GetAdmWeight(-7, -6, -5);
            }else if (MoneyStorage < MoneyInCome * 5) {
                recruitValue += AIType.GetMilWeight(-7, -6, -5);
                recruitValue += AIType.GetAdmWeight(-5, -4, -3);
            } else if (MoneyStorage < MoneyInCome * 10) {
                recruitValue += AIType.GetMilWeight(-5, -4, -3);
                recruitValue += AIType.GetAdmWeight(-5, -4, -3);
            }

            // 计算粮食对于 招募 权重的影响
            if (GrainStorage < GrainInCome * 3) {
                recruitValue += AIType.GetMilWeight(-9, -8, -7);
                recruitValue += AIType.GetAdmWeight(-7, -6, -5);
            }else if (GrainStorage < GrainInCome * 5) {
                recruitValue += AIType.GetMilWeight(-5, -4, -3);
                recruitValue += AIType.GetAdmWeight(-5, -4, -3);
            } else if (GrainStorage < GrainInCome * 10) {
                recruitValue += AIType.GetMilWeight(-3, -2, -1);
                recruitValue += AIType.GetAdmWeight(-5, -4, -3);
            }

            // 计算 处于战争 对权重的影响
            if (IsInWar) {
                recruitValue += AIType.GetMilWeight(1, 2, 3);
                recruitValue += AIType.GetAdmWeight(2, 3, 4);
            } else {
                recruitValue += AIType.GetMilWeight(-1, -1, -1);
                recruitValue += AIType.GetAdmWeight(-1, -1, -1);
            }

            this.recruitValue = recruitValue;

            // NOTICE: 冷却期，如果上一次新建了一些军队 要过一段时间才会再次允许新建，留一些容错空间
            if (recruitCooling > 0) {
                recruitCooling--;
                //Debug.Log($"不应该新招募部队, cooling: {recruitCooling}, recruitValue: {recruitValue}");
                
                return false;
            }
            //Debug.Log($"判断招募新军结果, recruitValue: {recruitValue}");
            return recruitValue > 0;
        }

        /// <summary>
        /// 招募合适的军队数目-Action
        /// </summary>
        public async void RecruitSuitArmyNum() {

            // 找到合适的招募省份
            // NOTICE: 经济权重高，且军事权重值低，的 recruitValue 个省份，不足则
            int recruitNum = recruitValue;
            List<Province> highSafeValuePrvc = GetPrvcBySafeOrder(CurAbleMap, recruitNum, true);

            // 获得要招募的单位
            // TODO: 合理地安排要招募的单位
            ArmyUnitData armyUnitData = await ArmyUnitHelper.GetArmyUnitDataAsync(null);

            // 设置军队单位集结地点
            Province ArmyGather = null;
            if (highSafeValuePrvc.Count > 0) {
                ArmyGather = highSafeValuePrvc[0];
            }

            // 根据 recruitValue 进行招募
            int i = 0;
            while(recruitNum > 0) {
                if (i >= 0 && i < highSafeValuePrvc.Count) {
                    highSafeValuePrvc[i].RecruitArmyEvent(armyUnitData);
                }
                
                i++;
                // 防止越界
                if(i >= highSafeValuePrvc.Count) {
                    i = 0;
                }
                recruitNum--;
            }

            

            // 设置冷却期，每次招募之后的冷却期为: 24 + 要招募的数量*2
            recruitCooling = 24 + recruitValue * 2;

            Debug.Log($"AI已成功招募, recruitCooling:{recruitCooling}, recruitValue:{recruitValue}, HighSafe: {highSafeValuePrvc.Count}");
        }

        /// <summary>
        /// 判断是否应该移除一些军队-Condition
        /// </summary>
        public bool ShouldRemoveArmy() {
            // TODO: 如果上一次移除了一些军队 要过一段时间才会再次允许移除，留一些容错空间
            return false;
        }

        /// <summary>
        /// 移除掉合适的军队数目-Action
        /// </summary>
        public void RemoveSuitArmyNum() {
            
        }

        /// <summary>
        /// 执行分配军队任务,分配到合适的省份上-Action
        /// </summary>
        public void DistributeArmy() {

            // 计算单只军队合适的单位数目
            int suitUnitNum = 20;

            // 计算合适的军队只数
            int suitArmyNum = ArmyNum / suitUnitNum;

            // 检测是否有自己的军队处于同一个省份，如果有，就合并


            // 找到军事价值最高的几个目标，然后分配军队前往
            // 逻辑:
            // 1.判断邻国数目，邻国数目等于要应对的敌人数目，在他们的边境分别找到最低SafeValue的省份
            // 2.遍历上面的省份，筛选出有敌人军队存在的省份
            // 3.排序筛选结果，敌人军队越少的省份越要分配军队（先打弱军，此处应当有更多AI策略）
            // 4.往这些省份，依次分配 敌人数目 + 1~3 的军队，不足则全去

            // NOTICE: 军队应当有不同的AI状态，
            // a.处于战区: 附加3格有敌人时处于战区, 不分配到其他区域, 会占据附近有利的地形(MilValue最高,且无敌人), 以AI类型判断是否主动挑战（ 多20%军力，多10%，多0% ）
            // b.处于和平区: 可以分配到其他区域
            // c.逃离: 当附近3格的敌人过于强大时（>3倍）,则军队会选择逃到离其三格远的 **最适合逃离** 的位置
            // 5.为了防止有权重相同的省份而导致频繁分配，应当设置一个冷却器，当自己没有新的省份控制变动时，无需改目标

            Debug.Log("excute dustribute army!");


        }

        /// <summary>
        /// 设定要占领的省份目标-Action
        /// </summary>
        public void SetAttackTarget() {

        }

        #endregion

        #region 势力图 构建

        /// <summary>
        /// 更新 势力图 各个省份的权重值
        /// </summary>
        public void UpdateMap(List<Province> CurAbleMap) {

            foreach (var province in CurAbleMap) {
                if (!CurMapWeightDic.ContainsKey(province.provincePosition)) {
                    // 不存在于dic中，新建省份权重
                    ProvinceWeight weight = new ProvinceWeight(faction.FactionTag, AIType, province);
                    CurMapWeightDic.Add(province.provincePosition, weight);
                } else {
                    // 存在于dic中，则更新权重
                    CurMapWeightDic[province.provincePosition].UpdateWeight(faction.FactionTag, AIType, province);
                }
            }
        }

        public void AddMap(Faction faction) {
            CurAbleMap = CurAbleMap.Concat(faction.Territory).ToList();
            // 更新地图权重
            UpdateMap(CurAbleMap);
        }

        public void RemoveMap(Faction faction) {
            foreach (var province in faction.Territory) {
                // 从 CurAbleMap 中移除
                if (CurAbleMap.Contains(province)) {
                    CurAbleMap.Remove(province);
                }

                // 从 Dic 中移除
                if (CurMapWeightDic.ContainsKey(province.provincePosition)) {
                    CurMapWeightDic.Remove(province.provincePosition);
                }
            }
        }


        /// <summary>
        /// 从给定的省份数组中找出经济价值 最低/最高 的指定个省份
        /// </summary>
        /// <param name="provinceList">给定的省份列表</param>
        /// <param name="num">要返回的指定数目</param>
        /// <param name="IsDesc">是否是求经济价值最低的省份</param>
        /// <returns></returns>
        public List<Province> GetPrvcByEcoOrder(List<Province> provinceList, int num, bool IsDesc) {
            
            List<ProvinceWeight> weights = GetWeightList(provinceList);
            // 获取 筛选出的省份
            List<Province> provinces = new List<Province>();

            if (!IsDesc) {
                weights.Sort((rec1, rec2) => rec1.EcoValue.CompareTo(rec2.EcoValue));
            } else {
                weights.Sort((rec1, rec2) => rec2.EcoValue.CompareTo(rec1.EcoValue));
            }

            GetPrvcListByNum(weights, ref provinces, num);
            return provinces;
        }

        public List<Province> GetPrvcByMilOrder(List<Province> provinceList, int num, bool IsDesc) {
            List<ProvinceWeight> weights = GetWeightList(provinceList);
            // 获取 筛选出的省份
            List<Province> provinces = new List<Province>();

            if (!IsDesc) {
                weights.Sort((rec1, rec2) => rec1.MilValue.CompareTo(rec2.MilValue));
            } else {
                weights.Sort((rec1, rec2) => rec2.MilValue.CompareTo(rec1.MilValue));
            }

            GetPrvcListByNum(weights, ref provinces, num);
            return provinces;
        }

        /// <summary>
        /// 获取指定省份当中 最安全/最不安全 的省份列表
        /// </summary>
        public List<Province> GetPrvcBySafeOrder(List<Province> provinceList, int num, bool IsDesc) {
            List<ProvinceWeight> weights = GetWeightList(provinceList);
            // 获取 筛选出的省份
            List<Province> provinces = new List<Province>();

            if (!IsDesc) {
                // 升序排序
                weights.Sort((rec1, rec2) => {
                    // NOTICE: 安全指数 = 经济价值 - 军事价值
                    int safeValue1 = rec1.EcoValue - rec1.MilValue;
                    int safeValue2 = rec2.EcoValue - rec2.MilValue;
                    return safeValue1.CompareTo(safeValue2);
                });
            } else {
                weights.Sort((rec1, rec2) => {
                    int safeValue1 = rec1.EcoValue - rec1.MilValue;
                    int safeValue2 = rec2.EcoValue - rec2.MilValue;
                    return safeValue2.CompareTo(safeValue1);
                });
            }

            GetPrvcListByNum(weights, ref provinces, num);
            return provinces;
        }


        /// <summary>
        /// 将 Province 列表 转为 ProvinceWeight 列表
        /// </summary>
        private List<ProvinceWeight> GetWeightList(List<Province> provinceList) {
            List<Vector3> rec = provinceList.Select((province) => province.provincePosition).ToList();

            // 获取 权重排位
            List<ProvinceWeight> weightList = new List<ProvinceWeight>();
            foreach (var position in rec) {
                weightList.Add(CurMapWeightDic[position]);
            }

            return weightList;
        }

        /// <summary>
        /// 获取传入 ProvinceWeight 数组的前 num 个 Province
        /// </summary>
        private void GetPrvcListByNum(List<ProvinceWeight> weights, ref List<Province> provinces, int num) {
            // NOTICE: 这里采取了随机数
            // 如果筛选出的不足num个，则全加入 / 或者(随机数)仅加入一半
            if (num < weights.Count) {
                int randomInt = UnityEngine.Random.Range(0, 10) + 1;
                if (randomInt <= 4) {
                    num /= 2;
                }
            }

            int i = 0;
            // 获取指定num个数目省份
            while (i < num && i < weights.Count) {
                provinces.Add(MapController.Instance.GetProvinceByID(weights[i].HexID));
                i++;
            }
        }

        #endregion


    }

    
}
