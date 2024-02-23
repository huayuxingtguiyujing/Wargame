using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.UI {
    public class EconomyPanel : BasePopUI {

        Faction currentFaction;

        [Header("part1")]
        [SerializeField] RateItem taxRateItem;
        [SerializeField] TextItem taxIncome_day;
        [SerializeField] TextItem taxIncome_month;
        [SerializeField] TextItem raidIncome;

        [Header("part2")]
        [SerializeField] RateItem maintenanceRateItem;
        [SerializeField] TextItem provinceMaintenance;
        [SerializeField] TextItem armyMaintenance;
        [SerializeField] TextItem generalMaintenance;

        [Header("part3")]
        [SerializeField] TextItem totalIncome_lastDay;
        [SerializeField] TextItem totalOutcome_lastDay;
        [SerializeField] TextItem totalCount;

        public void UpdateEconomyPanel(Faction faction) {

            currentFaction = faction;

            FactionResource factionResource = faction.Resource;

            // part 1
            taxRateItem.InitRateItem((int)factionResource.CountryTaxLevel, ChangeCountryTaxRate);
            // 
            taxIncome_day.InitTextItem(null, factionResource.taxIncome_day.ToString("0.0"), 1);
            taxIncome_month.InitTextItem(null, factionResource.taxIncome_month.ToString(), 1);
            raidIncome.InitTextItem(null, factionResource.raidIncome.ToString(), 1);

            // part 2
            maintenanceRateItem.InitRateItem((int)factionResource.CountryMaintenance, ChangeCountryMaintenanceRate);
            // 
            provinceMaintenance.InitTextItem(null, factionResource.provinceMaintenance.ToString(), 2);
            armyMaintenance.InitTextItem(null, factionResource.armyMaintenance.ToString(), 2);
            generalMaintenance.InitTextItem(null, factionResource.generalMaintenance.ToString(), 2);

            // part 3
            int totalIncome = factionResource.GetTotalIncome();
            int totalOutcome = factionResource.GetTotalOutcome();

            totalIncome_lastDay.InitTextItem(null, totalIncome.ToString(), 1);
            totalOutcome_lastDay.InitTextItem(null, totalOutcome.ToString(), 2);
            totalCount.InitTextItem(null, (totalIncome - totalOutcome).ToString(), (totalIncome - totalOutcome) > 0 ?1:2);

            // 粮食现在改为日入，故设定月入粮食项不显示
            if (taxIncome_month.gameObject.activeSelf) {
                taxIncome_month.gameObject.SetActive(false);
            }
        }

        private void ChangeCountryTaxRate(int level) {
            TaxLevel taxLevel = (TaxLevel)level;
            if (Enum.IsDefined(typeof(TaxLevel), taxLevel)) {
                // 更改当前国家的税率
                currentFaction.ChangeTaxLevel(taxLevel);

            }
            
        }

        private void ChangeCountryMaintenanceRate(int level) {
            MaintenanceLevel maintenanceLevel = (MaintenanceLevel)level;
            if (Enum.IsDefined(typeof(MaintenanceLevel), maintenanceLevel)) {
                currentFaction.ChangeMaintenanceLevel(maintenanceLevel);
            }
            
        }

    }
}