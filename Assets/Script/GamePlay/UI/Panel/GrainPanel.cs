using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.UI {
    public class GrainPanel : BasePopUI {

        Faction currentFaction;

        [Header("part1")]
        [SerializeField] RateItem grainRateItem;
        [SerializeField] TextItem grainIncome_day;
        [SerializeField] TextItem grainIncome_month;
        [SerializeField] TextItem raidIncome;

        [Header("part2")]
        [SerializeField] TextItem armyCostGrainItem;
        [SerializeField] TextItem raidCostGrainItem;

        [Header("part3")]
        [SerializeField] TextItem totalIncome_lastDay;
        [SerializeField] TextItem totalOutcome_lastDay;
        [SerializeField] TextItem totalCount;

        public void UpdateGrainPanel(Faction faction) {
            currentFaction = faction;
            FactionResource factionResource = faction.Resource;

            // part1
            // 改变征粮率组件
            grainRateItem.InitRateItem((int)factionResource.CountryExpropriateGrainLevel, ChangeCountryGrainRate);
            // 
            grainIncome_day.InitTextItem(null, factionResource.grainIncome_day.ToString("0.0"), 1);
            grainIncome_month.InitTextItem(null, factionResource.grainIncome_month.ToString(), 1);
            raidIncome.InitTextItem(null, factionResource.raidIncome.ToString(), 1);

            // part 2
            armyCostGrainItem.InitTextItem(null, factionResource.armyCostGrain.ToString(), 2);
            raidCostGrainItem.InitTextItem(null, factionResource.raidCostGrain.ToString(), 2);
            
            // part 3
            int totalGrainIncome = factionResource.GetTotalGrainIncome();
            int totalGrainOutcome = factionResource.GetTotalGrainOutcome();

            totalIncome_lastDay.InitTextItem(null, totalGrainIncome.ToString(), 1);
            totalOutcome_lastDay.InitTextItem(null, totalGrainOutcome.ToString(), 2);
            totalCount.InitTextItem(null, (totalGrainIncome + totalGrainOutcome).ToString(), (totalGrainIncome + totalGrainOutcome) > 0 ? 1 : 2);

            // 粮食现在改为日入，故设定月入粮食项不显示
            if (grainIncome_month.gameObject.activeSelf) {
                grainIncome_month.gameObject.SetActive(false);
            }

        }

        private void ChangeCountryGrainRate(int level) {
            ExpropriateGrainLevel grainLevel = (ExpropriateGrainLevel)level;
            if (Enum.IsDefined(typeof(ExpropriateGrainLevel), grainLevel)) {
                // 更改当前国家的征粮率
                currentFaction.ChangeExproGrainLevel(grainLevel);
            }
        }

    }
}