using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.Politic {
    [System.Serializable]
    public class FactionResource {
        #region ��ǰ����
        int money;
        public int Money { get => money; set => money = value; }

        // ���������֧����
        public float taxIncome_day = 0;       // ͨ����ǰ��˰�� �յ���
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

        public float grainIncome_day = 0;       // TODO: �ñ�����һ���ڻ�ȡ������ʳ��, Ҫд��Ӧ���߼���
        public float grainIncome_month = 0;
        public float raidGrainIncome = 0;       // ������ʳ(�����ò��� �Լ�ȡ����)

        public int GetTotalGrainIncome() {
            return (int)(grainIncome_day + grainIncome_month + raidGrainIncome);
        }

        public int armyCostGrain = 0;
        public int raidCostGrain = 0;
        public int GetTotalGrainOutcome() {
            return (int)(armyCostGrain + raidCostGrain);
        }
        #endregion

        // ��ʳ�洢��
        float grainDeposits;
        public float GrainDeposits { get => grainDeposits; set => grainDeposits = value; }

        // ������
        long totalManpower;
        public long TotalManpower { get => totalManpower; set => totalManpower = value; }

        // ���˿���
        long totalPopulation;
        public long TotalPopulation { get => totalPopulation; set => totalPopulation = value; }

        // ������������ͳ����Ȩ������
        float prestige;
        public float Prestige { get => prestige; set => prestige = value; }

        // Ȩ����Ȩ����ͳ����Ȩ�ڲ���������
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

        #region �������: ˰�ա���ʳ��ʡ��ά���ѡ�����ά����
        // NOTICE: ԭ�ƻ�ÿ�»�ȡһ��˰�պ���ʳ��������ϵͳ��ø��Ӹ��ӣ����ڸ�Ϊÿ�ջ��һ��˰��
        // TODO: �ƻ���ÿ�½���ʱ������һ��˰��
        /// <summary>
        /// ��ȡ˰�պ���ʳ��ÿ�µ��ã�Deprecated
        /// </summary>
        public void GetTaxAndGrain_Month(Province[] Territory) {
            float taxCount = 0;
            float grainCount = 0;
            foreach (Province province in Territory) {
                // Ѱ���ڿ����µ����������˰�պ���ʳ
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
                // Ѱ���ڿ����µ����������˰�պ���ʳ
                if (province.provinceData.UnderControlByOwner()) {

                    taxCount += province.GetProvinceTaxNum_Day();
                    grainCount += province.GetProvinceGrainNum_Day();
                }
            }
            GainMoney(Convert.ToInt32(taxCount));
            GainGrain(Convert.ToInt32(grainCount));

            // ���뵽ÿ��������
            taxIncome_day = taxCount;
            grainIncome_day = grainCount;
        }

        /// <summary>
        /// ���̴�ʡ�ݵĴ洢�л�ȡ˰��
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
        /// ֧��ά���ѣ�ÿ�µ��ã�
        /// </summary>
        /// <param name="Territory"></param>
        public void PayMaintenance(Province[] Territory) {
            
            float maintenanceCount = 0;
            foreach (Province province in Territory)
            {
                // Ѱ���ڿ����µ�������֧��ά����
                if (province.provinceData.UnderControlByOwner()) {
                    
                    maintenanceCount += province.GetProvinceMaintenance();
                }
            }

            provinceMaintenance = maintenanceCount;

            // ά��ʡ��ʧ�ܣ���Ǯ���㣬���������Ժ��
            if (!CostMoney(Convert.ToInt32(maintenanceCount))) {
                //Debug.Log("ά��ʡ��ʧ�ܣ���ɥʧ������Ȩ��������ȫ��ʿ�������ȫ�ֻķ�");
            }
        }

        /// <summary>
        /// ֧������ά�����á�ά�����ӵĲ�����
        /// </summary>
        public float PayArmy(List<Army> FactionArmys) {
            float cost = 0;
            for (int i = 0; i < FactionArmys.Count; i++) {
                // ��ȡ��ֻ���ӵ�ά����
                float costOfThisArmy = FactionArmys[i].ArmyData.GetArmyMaintenanceCost();
                if (CostMoney(Convert.ToInt32(costOfThisArmy))) {
                    // �ܹ�֧��
                    cost += costOfThisArmy;
                    FactionArmys[i].MakeArmyPaied();
                } else {
                    // �޷�֧��,�ᵼ�����غ��
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
                    // ������ʳ��Դ�� ��������
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
        /// Ϊһֻ�����ṩ��ʳ
        /// </summary>
        /// <param name="army"></param>
        public void PaySupply(Army army) {
            // δ�����ޣ����ⲹ��

            // �Ѿ������ޣ�ά�ֿ�֧ƽ��

            army.ArmyData.MakeArmyGetSupply(army.ArmyData.SupplyCost);
        }

        #endregion

        #region ͳ���������˿ڡ�������Ȩ��

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


        #region �������ߣ���˰�ʡ����ʣ�
        // ����˰��
        private TaxLevel countryTaxLevel = TaxLevel.Normal;
        public TaxLevel CountryTaxLevel { get => countryTaxLevel;private set => countryTaxLevel = value; }
        public void ChangeTaxRate(TaxLevel target, Province[] Territory) {
            foreach (Province province in Territory) {
                // Ѱ���ڿ����µ��������ı�˰��
                if (province.provinceData.ProvinceTaxLevel == CountryTaxLevel) {
                    province.provinceData.ProvinceTaxLevel = target;
                }
            }
            CountryTaxLevel = target;

            Debug.Log("�ı��˵�ǰ��˰��:" + CountryTaxLevel.ToString());
        }
        
        // ������
        private ExpropriateGrainLevel countryExpropriateGrainLevel = ExpropriateGrainLevel.Normal;
        public ExpropriateGrainLevel CountryExpropriateGrainLevel { get => countryExpropriateGrainLevel; private set => countryExpropriateGrainLevel = value; }
        public void ChangeExpropriateGrain(ExpropriateGrainLevel target, Province[] Territory) {
            foreach (Province province in Territory) {
                // Ѱ���ڿ����µ�������������������
                if (province.provinceData.ExpropriateGrainLevel == CountryExpropriateGrainLevel) {
                    province.provinceData.ExpropriateGrainLevel = target;
                }
            }
            CountryExpropriateGrainLevel = target;
        }
        
        // ȫ��ʡ��ά������
        private MaintenanceLevel countryMaintenance = MaintenanceLevel.Normal;
        public MaintenanceLevel CountryMaintenance { get => countryMaintenance; private set => countryMaintenance = value; }
        public void ChangeMaintenanceRate(MaintenanceLevel target, Province[] Territory) {
            foreach (Province province in Territory) {
                // Ѱ���ڿ����µ�����������ά����
                if (province.provinceData.ProvinceMateLevel == CountryMaintenance) {
                    province.provinceData.ProvinceMateLevel = target;
                }
            }
            CountryMaintenance = target;
        }
        #endregion

        #region �����ӿ�: �����Դ��������Դ

        /// <summary>
        /// ���ӽ�Ǯ
        /// </summary>
        private void GainMoney(int num) {
            Money += num;
        }

        public bool CostMoney(int costNum) {
            if (Money - costNum > 0) {
                Money -= costNum;
                return true;
            } else {
                //Debug.Log("�����Ǯ������������Ȩ����������Ϊ���Ʒ��Ȩ��Ϊ0ʱ������ȫ��ʿ������֯�ȡ����������ȵ�");
                return false;
            }
        }

        /// <summary>
        /// �����ʳ
        /// </summary>
        public void GainGrain(int num) {
            GrainDeposits += num;
        }

        public bool CostGrain(float costNum) {
            if (GrainDeposits - costNum > 0) {
                GrainDeposits -= costNum;
                return true;
            } else {
                Debug.Log("���������ʳ��������");
                return false;
            }
        }
        #endregion

    }

}