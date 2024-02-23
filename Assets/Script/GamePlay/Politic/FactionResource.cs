using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.Politic {
    [System.Serializable]
    public class FactionResource {
        #region 当前国库
        int money;
        public int Money { get => money; set => money = value; }

        // 各个收入项、支出项
        public float taxIncome_day = 0;       // 通过提前征税征 收到的
        public float taxIncome_month = 0;
        public int raidIncome = 0;
        public int dipIncome = 0;
        public int GetTotalIncome() {
            return (int)(taxIncome_day + taxIncome_month + raidIncome + dipIncome);
            
        }

        public float provinceMaintenance = 0;
        public float armyMaintenance = 0;
        public int generalMaintenance = 0;
        public int GetTotalOutcome() {
            return (int)(provinceMaintenance + armyMaintenance + generalMaintenance);
        }

        public float grainIncome_day = 0;       // TODO: 该变量是一天内获取到的粮食量, 要写相应的逻辑！
        public float grainIncome_month = 0;
        public float raidGrainIncome = 0;       // 劫掠粮食(可能用不上 自己取舍下)

        public int GetTotalGrainIncome() {
            return (int)(grainIncome_day + grainIncome_month + raidGrainIncome);
        }

        public int armyCostGrain = 0;
        public int raidCostGrain = 0;
        public int GetTotalGrainOutcome() {
            return (int)(armyCostGrain + raidCostGrain);
        }
        #endregion

        // 粮食存储量
        float grainDeposits;
        public float GrainDeposits { get => grainDeposits; set => grainDeposits = value; }

        // 总人力
        long totalManpower;
        public long TotalManpower { get => totalManpower; set => totalManpower = value; }

        // 总人口数
        long totalPopulation;
        public long TotalPopulation { get => totalPopulation; set => totalPopulation = value; }

        // 威望：威望是统治政权的名声
        float prestige;
        public float Prestige { get => prestige; set => prestige = value; }

        // 权威：权威是统治政权内部的凝聚力
        float authority;
        public float Authority { get => authority; set => authority = value; }
        

        public FactionResource() {
            Money = 1000;
            GrainDeposits = 900;
            TotalManpower = 65520;
            TotalPopulation = 605538;

            Prestige = 51;
            Authority = 68;
        }

        #region 经济相关: 税收、粮食、省份维护费、军队维护费
        // NOTICE: 原计划每月获取一次税收和粮食，但会让系统变得更加复杂，现在改为每日获得一定税收
        // TODO: 计划在每月结算时额外获得一次税收
        /// <summary>
        /// 获取税收和粮食（每月调用）Deprecated
        /// </summary>
        public void GetTaxAndGrain_Month(Province[] Territory) {
            float taxCount = 0;
            float grainCount = 0;
            foreach (Province province in Territory) {
                // 寻找在控制下的领土，获得税收和粮食
                if (province.provinceData.UnderControlByOwner()) {

                    taxCount += province.GetProvinceTaxNum_Month();
                    grainCount += province.GetProvinceGrainNum_Month();
                }
            }
            GainMoney(Convert.ToInt32(taxCount));
            GainGrain(Convert.ToInt32(grainCount));

            taxIncome_month = taxCount;
            grainIncome_month = grainCount;
        }

        public void GetTaxAndGrain_Day(Province[] Territory) {
            float taxCount = 0;
            float grainCount = 0;
            foreach (Province province in Territory) {
                // 寻找在控制下的领土，获得税收和粮食
                if (province.provinceData.UnderControlByOwner()) {

                    taxCount += province.GetProvinceTaxNum_Day();
                    grainCount += province.GetProvinceGrainNum_Day();
                }
            }
            GainMoney(Convert.ToInt32(taxCount));
            GainGrain(Convert.ToInt32(grainCount));

            // 加入到每日收入中
            taxIncome_day = taxCount;
            grainIncome_day = grainCount;
        }

        /// <summary>
        /// 立刻从省份的存储中获取税收
        /// </summary>
        public void GetTax_DayCollect(int num) {
            taxIncome_day = num;
            GainMoney(num);
        }

        public void GetGrain_DayCollect(int num) {
            grainIncome_day = num;
            GainGrain(num);
        }

        /// <summary>
        /// 支付维护费（每月调用）
        /// </summary>
        /// <param name="Territory"></param>
        public void PayMaintenance(Province[] Territory) {
            
            float maintenanceCount = 0;
            foreach (Province province in Territory)
            {
                // 寻找在控制下的领土，支付维护费
                if (province.provinceData.UnderControlByOwner()) {
                    
                    maintenanceCount += province.GetProvinceMaintenance();
                }
            }

            provinceMaintenance = maintenanceCount;

            // 维护省份失败，金钱不足，将触发恶性后果
            if (!CostMoney(Convert.ToInt32(maintenanceCount))) {
                //Debug.Log("维护省份失败，将丧失威望和权威，降低全局士气，造成全局荒废");
            }
        }

        /// <summary>
        /// 支付军队维护费用、维护军队的补给线
        /// </summary>
        public float PayArmy(List<Army> FactionArmys) {
            float cost = 0;
            for (int i = 0; i < FactionArmys.Count; i++) {
                // 获取该只军队的维护费
                float costOfThisArmy = FactionArmys[i].ArmyData.GetArmyMaintenanceCost();
                if (CostMoney(Convert.ToInt32(costOfThisArmy))) {
                    // 能够支付
                    cost += costOfThisArmy;
                    FactionArmys[i].MakeArmyPaied();
                } else {
                    // 无法支付,会导致严重后果
                    FactionArmys[i].MakeArmyUnpaied();
                }

            }
            armyMaintenance = cost;
            return (uint)cost;
        }

        public float PaySupply(List<Army> FactionArmys) {
            float cost = 0;
            foreach (var army in FactionArmys)
            {
                if (army.CanGetSupply) {
                    float costOfArmy = army.ArmyData.SupplyCost;
                    // 消耗粮食资源以 供给军队
                    if (CostGrain(costOfArmy)) {
                        cost += costOfArmy;
                        army.ArmyData.MakeArmyGetSupply(costOfArmy);
                    }
                }
            }
            armyCostGrain = (int)cost;
            return cost;
        }

        /// <summary>
        /// 为一只军队提供粮食
        /// </summary>
        /// <param name="army"></param>
        public void PaySupply(Army army) {
            // 未到上限，额外补充

            // 已经到上限，维持开支平衡

            army.ArmyData.MakeArmyGetSupply(army.ArmyData.SupplyCost);
        }

        #endregion

        #region 统计人力、人口、威望、权威

        public void CaculateTotalManpower(Province[] Territory) {
            long rec = 0;
            foreach (var province in Territory)
            {
                rec += province.provinceData.ManPower;
            }
            TotalManpower = rec;
        }

        public void CaculateTotalPopulation(Province[] Territory) {

        }

        public void CaculatePrestige() {

        }

        public void CaculateAuthority() {

        }
        #endregion


        #region 调整政策（调税率、征率）
        // 国家税率
        private TaxLevel countryTaxLevel = TaxLevel.Normal;
        public TaxLevel CountryTaxLevel { get => countryTaxLevel;private set => countryTaxLevel = value; }
        public void ChangeTaxRate(TaxLevel target, Province[] Territory) {
            foreach (Province province in Territory) {
                // 寻找在控制下的领土，改变税率
                if (province.provinceData.ProvinceTaxLevel == CountryTaxLevel) {
                    province.provinceData.ProvinceTaxLevel = target;
                }
            }
            CountryTaxLevel = target;

            Debug.Log("改变了当前的税率:" + CountryTaxLevel.ToString());
        }
        
        // 征粮率
        private ExpropriateGrainLevel countryExpropriateGrainLevel = ExpropriateGrainLevel.Normal;
        public ExpropriateGrainLevel CountryExpropriateGrainLevel { get => countryExpropriateGrainLevel; private set => countryExpropriateGrainLevel = value; }
        public void ChangeExpropriateGrain(ExpropriateGrainLevel target, Province[] Territory) {
            foreach (Province province in Territory) {
                // 寻找在控制下的领土，调整征粮费用
                if (province.provinceData.ExpropriateGrainLevel == CountryExpropriateGrainLevel) {
                    province.provinceData.ExpropriateGrainLevel = target;
                }
            }
            CountryExpropriateGrainLevel = target;
        }
        
        // 全国省份维护费用
        private MaintenanceLevel countryMaintenance = MaintenanceLevel.Normal;
        public MaintenanceLevel CountryMaintenance { get => countryMaintenance; private set => countryMaintenance = value; }
        public void ChangeMaintenanceRate(MaintenanceLevel target, Province[] Territory) {
            foreach (Province province in Territory) {
                // 寻找在控制下的领土，调整维护费
                if (province.provinceData.ProvinceMateLevel == CountryMaintenance) {
                    province.provinceData.ProvinceMateLevel = target;
                }
            }
            CountryMaintenance = target;
        }
        #endregion

        #region 基础接口: 获得资源、花费资源

        /// <summary>
        /// 增加金钱
        /// </summary>
        private void GainMoney(int num) {
            Money += num;
        }

        public bool CostMoney(int costNum) {
            if (Money - costNum > 0) {
                Money -= costNum;
                return true;
            } else {
                //Debug.Log("国库的钱不够啦！将以权威和威望作为替代品，权威为0时将降低全局士气、组织度、将领能力等等");
                return false;
            }
        }

        /// <summary>
        /// 获得粮食
        /// </summary>
        public void GainGrain(int num) {
            GrainDeposits += num;
        }

        public bool CostGrain(float costNum) {
            if (GrainDeposits - costNum > 0) {
                GrainDeposits -= costNum;
                return true;
            } else {
                Debug.Log("国库里的粮食不够啦！");
                return false;
            }
        }
        #endregion

    }

}