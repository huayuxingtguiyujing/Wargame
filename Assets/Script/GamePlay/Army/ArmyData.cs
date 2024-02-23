using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// һ֧���ŵ������࣬���������ɶ�ֻ����
    /// </summary>
    [System.Serializable]
    public class ArmyData {

        [Tooltip("�������ƣ������ظ�")]
        public string armyName;

        [Tooltip("�������ĸ�����")]
        public string ArmyTag { get; set; }
        public bool IsArmyTag(string tag) { return ArmyTag == tag;}

        [Tooltip("��������Ŀ")]
        public uint ArmyNum;
        public uint ArmyInfantryNum;
        public uint ArmyCavalryNum;
        public uint ArmyBaggageNum;
        
        // ѵ���� �����е�λ��ƽ��ֵ
        public float Discipine;

        // ʿ�� �����е�λ��ƽ��ֵ
        public float CurMorale;
        public float MaxMorale;

        // ����
        public float CurSupply;
        public float SupplyCost;
        public float MaxSupply;

        // ��������Դ
        public DetailMessage SupplyDetail;
        public float LocalSupplySupport;
        //public float ;
        public bool NeedSupplySupport;              // ��Ҫ�������������� �Թ�����ֻ����
        public float SupplyFromTrans = 0;           // �����������ṩ�Ĳ���

        /// <summary>
        /// Ϊ�������Ͳ���
        /// </summary>
        public void MakeArmyGetSupply(float SupplyAdd) {
            // TODO: ������ �ṩ�Ĳ���ֵ ��ô�㣿����������
            ChangeArmyCurSupply(SupplyAdd);
            SupplyDetail.AddMessage("��������", SupplyAdd);
        }

        public void ChangeArmyCurSupply(float changeValue) {
            // ���ӵĵ�ǰ���� �������0 �������Я����
            CurSupply = Mathf.Clamp(CurSupply + changeValue, 0, MaxSupply);
        }

        // ��ǰ�þ������� ���о��ӵ�λ�� �ı��ֵʱ�����
        private List<ArmyUnit> armyUnitDatas;
        public List<ArmyUnit> ArmyUnitDatas { get => armyUnitDatas; set => armyUnitDatas = value; }

        /// <summary>
        /// ���¼����������µ�λ����������
        /// </summary>
        public void RecaculateArmyUnitManNums() {
            uint rec = 0;
            foreach (var armyUnit in ArmyUnitDatas)
            {
                rec += armyUnit.ArmyCurrentManpower;
            }
            ArmyNum = rec;
        }

        // ��ǰս�۽׶εļӳ�
        private UnitModify lastStageModify = new UnitModify();

        // ��֯��: ��֯���Ǿ����ж���ս��ʱ�ı�ҪƷ
        private int currentOrganization;
        public int CurrentOrganization { get => currentOrganization; private set => currentOrganization = value; }


        #region �������

        // ��ǰ����: �������Ӱ�쵥λ����
        private General currentGeneral;
        public General CurrentGeneral {
            get {
                if (currentGeneral == null) {
                    currentGeneral = General.GetNoLeader(ArmyTag);
                }
                return currentGeneral;
            }
            private set => currentGeneral = value;
        }
        public void AssignGeneral(General general) {
            // �Ƴ���һ������
            RemoveGeneral();

            // TODO: ���Ľ��쵱ǰ��Ϣ������ֻ�ܱ�������һ�����ӵ�λ��

            CurrentGeneral = general;
            // ������Ե�λ������ֵ ���ӵ���λ��
            AddGeneralModify(CurrentGeneral.NormalModify);
        }
        public void RemoveGeneral() {
            if (CurrentGeneral != null) {
                RemoveGeneralModify(CurrentGeneral.NormalModify);
                //TODO : ��ǰ�н���ʱ����Ҫ���Ľ��쵱ǰ��Ϣ
            }

            CurrentGeneral = General.GetNoLeader(ArmyTag);
            // ������Ե�λ������ֵ ���ӵ���λ��
            AddGeneralModify(CurrentGeneral.NormalModify);
        }

        // TODO��д��
        /// <summary>
        /// ������Ծ������Ե�����ֵ ���ӵ�ÿ����λ��
        /// </summary>
        private void AddGeneralModify(UnitModify unitModify) {
            foreach (ArmyUnit armyUnit in ArmyUnitDatas) {
                armyUnit.unitModify.AddUnitModify(unitModify);
            }
        }

        private void RemoveGeneralModify(UnitModify unitModify) {
            foreach (ArmyUnit armyUnit in ArmyUnitDatas) {
                armyUnit.unitModify.ReduceUnitModify(unitModify);
            }
        }

        #endregion

        public ArmyData() {
            armyName = "army01";
            ArmyTag = "NoOwner";

            ArmyNum = 0;
            ArmyInfantryNum = 0;
            ArmyCavalryNum = 0;
            ArmyBaggageNum = 0;

            Discipine = 0;
            MaxMorale = 1;
            CurMorale = 0;

            ArmyUnitDatas = new List<ArmyUnit>();
        }

        public ArmyData(ArmyData armyData) {
            armyName = armyData.armyName + "1";
            ArmyTag = armyData.ArmyTag;

            ArmyNum = 0;
            ArmyInfantryNum = 0;
            ArmyCavalryNum = 0;
            ArmyBaggageNum = 0;

            Discipine = armyData.Discipine;
            MaxMorale = armyData.MaxMorale;
            CurMorale = armyData.CurMorale;

            CurSupply = armyData.CurSupply;

            SupplyDetail = new DetailMessage();

            // TODO��������캯��û�а�����壬Ҫ��
            ArmyUnitDatas = new List<ArmyUnit>();
        }

        public ArmyData(ArmyUnit armyUnitData) {
            armyName = "army01";
            ArmyTag = "NoOwner";
            ArmyNum = armyUnitData.ArmyCostManpower;

            Discipine = armyUnitData.ArmyBaseDiscipine;
            MaxMorale = armyUnitData.ArmyBaseMaxMorale;
            CurMorale = armyUnitData.ArmyBaseMorale;

            // ����ʱ����Ĭ�ϲ���
            CurSupply = armyUnitData.SupplyBaseMax;

            SupplyDetail = new DetailMessage();

            ArmyUnitDatas = new List<ArmyUnit> {
                armyUnitData
            };

            // ��ʼʱ �����޽���
            RemoveGeneral();

            ReCaculateStatu();
        }

        #region ��� �ϲ�����
        /// <summary>
        /// ����֧��������ӵ�λ��������ֵ��Ҫ���¼���
        /// </summary>
        public void AddUnit(ArmyUnit armyUnit) {
            if (ArmyUnitDatas == null) {
                ArmyUnitDatas = new List<ArmyUnit>();
            }

            //NOTICE�����߳��п��ܻ�����ͬ����Guid�������������ʼ����
            // ��ʼ��guid
            //armyUnitData.InitThisUnitGuid();

            // ���뵽armyunitdatas
            ArmyUnitDatas.Add(armyUnit);
            ReCaculateStatu();
        }

        public void AddArmyData(ArmyData armyData) {
            // ��λ����
            foreach (var armyUnitData in armyData.ArmyUnitDatas) {
                AddUnit(armyUnitData);
            }
            ReCaculateStatu();
            // ��������
            ChangeArmyCurSupply(armyData.CurSupply);
        }

        public void SettleArmyData() {
            int count = ArmyUnitDatas.Count;
            for (int i = count - 1; i >= 0; i--) {
                if (!ArmyUnitDatas[i].IsAlive) {
                    RemoveUnit(ArmyUnitDatas[i]);
                }
            }
        }

        public void RemoveUnit(ArmyUnit armyUnit) {
            if (ArmyUnitDatas.Contains(armyUnit))
            {
                ArmyUnitDatas.Remove(armyUnit);
            }
            
            ReCaculateStatu();
        }

        /// <summary>
        /// ����ǰ�ľ��Ӳ�ֳ���ֻ ���ӵ�λƽ������ �����²�ֵ�ArmyData
        /// </summary>
        public ArmyData SplitArmyData() {
            int totalCount = ArmyUnitDatas.Count / 2;

            // �����԰��
            ChangeArmyCurSupply(-CurSupply / 2);
            ArmyData armyData = new ArmyData(this);

            for (int i = 0; i < totalCount; i++) {
                //�ӵ�ǰ�������Ƴ���λ �����뵽�µ�armydata��
                armyData.AddUnit(ArmyUnitDatas[i]);
                ArmyUnitDatas.Remove(ArmyUnitDatas[i]);
            }

            // �²�ֵľ��� �趨Ϊ�޽���
            armyData.RemoveGeneral();

            ReCaculateStatu();

            return armyData;
        }
        #endregion

        #region ����״̬����: ������ĥ��ʿ���ָ���ά�����á������ָ�
        /// <summary>
        /// ��ʱ��������״̬: �ָ�������ʿ����������ģ���ע�ᵽ�ս��¼���
        /// </summary>
        public void CountArmyStatu(float localGrainSupport) {
            foreach (ArmyUnit armyUnit in ArmyUnitDatas)
            {
                // ����ʧȥʿ��
                armyUnit.LossMorale();
                armyUnit.LossManpower();

                // ����ָ�
                armyUnit.RecoverMorale();
                armyUnit.RecoverManpower();
                // NOTICE: ������Ӧ�ð���λ���㣬Ӧ�ð����Ӽ���

            }

            // TODO: 
            if(localGrainSupport < SupplyCost) {
                // �����ṩ����ʳ �������� �����뽨������
                NeedSupplySupport = true;
                //RequestGrainSupport();
            } else {
                NeedSupplySupport = false;
            }
            LocalSupplySupport = localGrainSupport;
            SupplyDetail.AddMessage("���ز���", localGrainSupport);

            // ���²���ʱ������ SupplyFromTrans
            CountArmySupply(localGrainSupport + SupplyFromTrans);

            ReCaculateStatu();
        }

        /// <summary>
        /// ������ӵĲ����仯
        /// </summary>
        public void CountArmySupply(float supplySupport) {
            // ÿһ�μ���󣬶���� SupplyFromTrans ��Ϊ0
            SupplyFromTrans = 0;
            ChangeArmyCurSupply(-SupplyCost + supplySupport);
        }

        public void RecoverArmy_Immediately(float Morale, float Supply, uint Manpower) {
            foreach (ArmyUnit armyUnit in ArmyUnitDatas) {
                // ֱ�ӻָ����ӵ�״̬
                armyUnit.RecoverMorale(Morale);
                armyUnit.RecoverManpower(Manpower);
            }
            Debug.Log("army recover status!");
            ReCaculateStatu();
        }

        /// <summary>
        /// ��ȡά����ֻ���ӵ���������
        /// </summary>
        public float GetArmyMaintenanceCost() {
            uint maintenanceCost = 0;
            foreach (var unit in ArmyUnitDatas) {
                maintenanceCost += unit.ArmyReinforcementCost;
            }
            return maintenanceCost;
        }

        /// <summary>
        /// ��ֻ�����Ѿ��õ������������ƵĽ����������㶮�ģ�
        /// </summary>
        public void MakeArmyPaied() {

        }

        public void MakeArmyUnpaied() {
            // TODO: ǷǮ�����Ӹ�����ֵ�½������������ѱ�
        }
        
        #endregion

        #region ������Ӽӳɡ���ֵ
        // ��ǰ
        public MoraleStage curArmyMoraleStage;

        public SupplyStage curArmySupplyStage;

        // ��ǰ ʿ��/���� �׶� �ļӳ�
        UnitModify moraleModify = new UnitModify();

        UnitModify supplyModify = new UnitModify();

        /// <summary>
        /// �������еľ��ӵ�λ ���¼�����ӵ���������ֵ
        /// </summary>
        public void ReCaculateStatu() {
            // TODO: д�겻ͬ�Ĳ�����ʿ���Ծ���ս������Ӱ��
            CaculateMoraleSupplyStage();

            float newMaxMorale = 0;
            float newMorale = 0;
            float newDiscipine = 0;

            float totalSupplyCost = 0;
            float unitMaxSupply = 0;

            uint newArmyNum = 0;
            uint newInfantryNum = 0;
            uint newCavalryNum = 0;
            uint newBaggageNum = 0;

            foreach (ArmyUnit armyUnit in ArmyUnitDatas) {
                newMaxMorale += armyUnit.ArmyBaseMaxMorale;
                newMorale += armyUnit.ArmyBaseMorale;
                newDiscipine += armyUnit.ArmyBaseDiscipine;
                newArmyNum += armyUnit.ArmyCurrentManpower;

                totalSupplyCost += armyUnit.SupplyDayCost;
                unitMaxSupply += armyUnit.SupplyBaseMax;

                switch (armyUnit.ArmyUnitType) {
                    case ArmyUnitType.Infantry:
                        newInfantryNum += armyUnit.ArmyCurrentManpower;
                        break;
                    case ArmyUnitType.Cavalry:
                        newCavalryNum += armyUnit.ArmyCurrentManpower;
                        break;
                    case ArmyUnitType.Baggage:
                        newBaggageNum += armyUnit.ArmyCurrentManpower;
                        break;
                }
            }
            MaxMorale = newMaxMorale / ArmyUnitDatas.Count;
            CurMorale = newMorale / ArmyUnitDatas.Count;
            Discipine = newDiscipine / ArmyUnitDatas.Count;

            SupplyCost = totalSupplyCost;
            // һֻ���ӵĲ���Я������,�����е�λ�Ĳ���Я��������ĺ�
            MaxSupply = unitMaxSupply;

            ArmyNum = newArmyNum;
            ArmyInfantryNum = newInfantryNum;
            ArmyCavalryNum = newCavalryNum;
            ArmyBaggageNum = newBaggageNum;
        }

        /// <summary>
        /// Ӧ�ý����ڲ�ͬս�۽׶ε���������ͬʱ�Ƴ�����һ���׶ε�����
        /// </summary>
        /// <param name="combatStage">ָ����ս�۽׶�</param>
        /// <returns></returns>
        public UnitModify CaculateGeneralModifyInCombat(CombatStageEnum combatStage) {
            // �Ƴ�֮ǰ�ľ��Ӽӳ�
            if (lastStageModify != null) {
                RemoveGeneralModify(lastStageModify);
            }

            if (combatStage == CombatStageEnum.StandOff) {
                AddGeneralModify(CurrentGeneral.StandOffModify);
                return CurrentGeneral.StandOffModify;
            } else if (combatStage == CombatStageEnum.Siege) {
                AddGeneralModify(CurrentGeneral.SiegeModify);
                return CurrentGeneral.SiegeModify;
            } else if (combatStage == CombatStageEnum.Engagement) {
                AddGeneralModify(CurrentGeneral.EngagementModify);
                return CurrentGeneral.EngagementModify;
            } else if (combatStage == CombatStageEnum.OpenBattle) {
                AddGeneralModify(CurrentGeneral.OpenBattleModify);
                return CurrentGeneral.OpenBattleModify;
            } else if (combatStage == CombatStageEnum.WithdrawAndPursuit) {
                AddGeneralModify(CurrentGeneral.WithdrawModify);
                return CurrentGeneral.WithdrawModify;
            } else {
                return CurrentGeneral.StandOffModify;
            }
        }

        /// <summary>
        /// ���㲻ͬ ʿ���׶� �� ����״�� �Ծ��ӵ�λ��Ӱ��
        /// </summary>
        private void CaculateMoraleSupplyStage() {

            // ʿ�� = Ѫ��;
            float moraleRatio = CurMorale / MaxMorale;
            moraleModify.ResetModify();
            if (moraleRatio >= 1.25f) {
                // ʿ�� > 125% ʱ ���е�λ���� +20%
                moraleModify.armyAttackModify = 0.2f;
                curArmyMoraleStage = MoraleStage.VeryHigh;
            } else if (moraleRatio < 1.25f && moraleRatio >= 1.0f) {
                // ʿ�� > 100% ʱ ���е�λ���� +10%
                moraleModify.armyAttackModify = 0.1f;
                curArmyMoraleStage = MoraleStage.High;
            } else if (moraleRatio <= 0.5f && moraleRatio > 0.25f) {
                // ʿ�� < 50% ʱ ���е�λ���� -10%
                moraleModify.armyDefendModify = -0.1f;
                curArmyMoraleStage = MoraleStage.Low;
            } else if (moraleRatio <= 0.25f) {
                // ʿ�� < 25% ʱ ���е�λ���� -20%�������ƶ���� +5% 
                moraleModify.armyDefendModify = -0.2f;
                moraleModify.moveLossModify = 0.05f;
                curArmyMoraleStage = MoraleStage.VeryLow;
            } else {
                curArmyMoraleStage = MoraleStage.Normal;
            }
            // �����ƶ����: ÿ�ξ����ƶ�������ʧ��ʿ�������������������ֵ���㣩
            // Q: Ϊʲôʿ�������ʱ���ƶ���������أ�
            // A: ��Ϊʿ�����ܹ���

            // ��ǰ�����̶�
            float supplyRatio = CurSupply / MaxSupply;
            supplyModify.ResetModify();
            if (supplyRatio <= 0.5f && supplyRatio > 0.25f) {
                // ������ < 50% ʱ, �����ƶ���� +1%, ���Ӳ�Ա�ٶ� -10%
                moraleModify.moveLossModify = 0.01f;
                moraleModify.recoverManpowerModify = -0.10f;
                curArmySupplyStage = SupplyStage.Middle;
            } else if (supplyRatio <= 0.25 && supplyRatio > 0.1f) {
                // ������ < 25% ʱ�������ƶ���� +3%�����Ӳ�Ա�ٶ� -25%
                moraleModify.moveLossModify = 0.03f;
                moraleModify.recoverManpowerModify = -0.25f;
                curArmySupplyStage = SupplyStage.Low;
            } else if (supplyRatio <= 0.1f && supplyRatio > 0) {
                // ������ < 10% ʱ�������ƶ���� +5%�����Ӳ�Ա�ٶ� -50%
                moraleModify.moveLossModify = 0.05f;
                moraleModify.recoverManpowerModify = -0.50f;
                curArmySupplyStage = SupplyStage.VeryLow;
            } else if (supplyRatio == 0) {
                // ������ = 0   ʱ, �����ƶ���� +8%�����Ӳ�Ա�ٶ� -75%�����е�λ���� -20%
                moraleModify.moveLossModify = 0.08f;
                moraleModify.recoverManpowerModify = -0.75f;
                moraleModify.armyDefendModify = -0.2f;
                curArmySupplyStage = SupplyStage.EatMan;
            } else {
                curArmySupplyStage = SupplyStage.Normal;
            }
            // Q: �����������˵����ʳΪ0ʱ�����ƶ��Ͳ�������
            // A: �Եģ���Ҷ�֪������ʱ��Ͳ�Ҫ�Ҷ�

            // ���µ�ÿ����λ��
            foreach (ArmyUnit armyUnit in ArmyUnitDatas) {
                armyUnit.CaculateMoraleSupplyStage(moraleModify, supplyModify);
            }

        }

        #endregion

    }

    
    public enum MoraleStage {
        VeryLow,        // ʿ�� < 25% ʱ ���е�λ���� -20%�������ƶ���� +5% 
        Low,            // ʿ�� < 50% ʱ ���е�λ���� -10%
        Normal,         // ʿ�� > 50%��<100%
        High,           // ʿ�� > 100% ʱ ���е�λ���� +10%
        VeryHigh,       // ʿ�� > 125% ʱ ���е�λ���� +20%
    }

    public enum SupplyStage {
        Normal,         // ������ > 50% ʱ
        Middle,         // ������ < 50% ʱ, �����ƶ���� +1%, ���Ӳ�Ա�ٶ� -10%
        Low,            // ������ < 25% ʱ�������ƶ���� +3%�����Ӳ�Ա�ٶ� -25%
        VeryLow,        // ������ < 10% ʱ�������ƶ���� +5%�����Ӳ�Ա�ٶ� -50%
        EatMan          // ������ = 0   ʱ, �����ƶ���� +8%�����Ӳ�Ա�ٶ� -75%�����е�λ���� -20%
    }

}