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

        // ��ǰ��¼��FactionAi�У������þ������ߵ�ʡ�ݵ�ͼ
        public List<Province> CurAbleMap { get; private set; }

        public Dictionary<Vector3, ProvinceWeight> CurMapWeightDic { get; private set; }

        public AIType AIType;

        SequenceNode RootNode;

        #region ��ǰ���� ������Դ

        bool IsInWar;

        int ArmyNum;
        int ArmyValue;
        int RecrtingArmyNum;        // ������ļ�Ĳ��ӵ���Ŀ
        int RecrtingArmyValue;      // ������ļ�Ĳ��ӵ�Ȩ��ֵ����������ļ�Ĳ��ӵ� ReinforceCost��

        int MoneyStorage;
        int MoneyOutCome;
        int MoneyInCome;

        int GrainStorage;
        int GrainOutCome;
        int GrainInCome;

        #endregion

        #region ��Ϊ��AI ��ر���

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
            // ������һ�γ�ʼ��
            if (HasInited) {
                //Debug.Log($"Tag: {faction.FactionTag}��AI �ѱ���ʼ��");
                return;
            }

            this.faction = faction;
            AIType = AIType.GetAmbitiousAIType();
            CurAbleMap = new List<Province>();
            CurMapWeightDic = new Dictionary<Vector3, ProvinceWeight>();

            // ���� ��ǰ�����ߵ�ʡ�� ���� ����ͼ
            AddMap(faction);

            // ������Ϊ��
            BuildBTree();

            // �ҽӸ����¼�
            TimeSimulator.Instance.SubscribeHourCall(RunAI_Hour);
            TimeSimulator.Instance.SubscribeDayCall(RunAI_Day);

            HasInited = true;
            Debug.Log($"Tag: {faction.FactionTag}��AI�ѳɹ���ʼ��, CurMap: {CurAbleMap.Count}");
        }

        private void BuildBTree() {
            RootNode = new SequenceNode("root");

            // �Ƿ�Ӧ������ļ����
            ConditionNode ShouldRecruit = new ConditionNode("ShouldRecruit", ShouldRecruitArmy);
            ActionNode RecruitArmy = new ActionNode("RecruitArmy", RecruitSuitArmyNum);
            ShouldRecruit.AddChildren(RecruitArmy);
            RootNode.AddChildren(ShouldRecruit);

            // �Ƿ�Ӧ��ȥ��һЩ����
            ConditionNode ShouldRemove = new ConditionNode("ShouldRemove", ShouldRemoveArmy);
            ActionNode RemoveArmy = new ActionNode("RemoveArmy", RemoveSuitArmyNum);
            ShouldRemove.AddChildren(RemoveArmy);
            RootNode.AddChildren(ShouldRemove);

            // ���䲿�ӵ����ʵ�λ��
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
            // ÿСʱ����
            UpdateCurResource();
            // ������Ϊ��
            RootNode.Excute();
        }

        public void RunAI_Day() {
            if (!enabled) {
                return;
            }
            
            UpdateMap(CurAbleMap);
        }

        #region ��Ϊ�� �ڵ��¼�

        /// <summary>
        /// ���µ�ǰ���ҵ���Դ-Action
        /// </summary>
        public void UpdateCurResource() {
            IsInWar = faction.IsFactionInWar();
            
            ArmyNum = faction.GetTotalArmyCount(ref ArmyValue);
            RecrtingArmyNum = faction.GetRecruitingArmyCount(ref RecrtingArmyValue);

            // ���µ�ǰ��Ǯ
            // NOTICE: ��Ǯ��λҪ����1000 ��Ϊ1000���ܹ���һ֧������Ĳ�����λ
            MoneyStorage = faction.Resource.Money / 1000;
            MoneyInCome = faction.Resource.GetTotalIncome() / 1000;
            MoneyOutCome = faction.Resource.GetTotalOutcome() / 1000;

            // ���µ�ǰ��ʳ
            GrainStorage = (int)faction.Resource.GrainDeposits;
            GrainInCome = faction.Resource.GetTotalGrainIncome();
            GrainOutCome = faction.Resource.GetTotalGrainOutcome();

        }

        /// <summary>
        /// �ж��Ƿ�Ӧ���½�����-Condition
        /// </summary>
        public bool ShouldRecruitArmy() {
            int recruitValue = 0;

            // ���㵱ǰ���о��ӵ�Ӱ��
            // NOTICE: ��Ϊ  - ��ǰ������ļ�ľ��ӵ�ά���ܷ�
            recruitValue +=  - RecrtingArmyValue;

            // �����������ʳ��Ӱ��
            recruitValue += Mathf.Min((MoneyInCome - MoneyOutCome), (GrainInCome - GrainOutCome));

            // �Ƿ��ڳ��֣������ AI ���� �� ��ǰ�ʽ𴢱� �� �ж��Ƿ����½�
            bool isMoneyInDeficit = ((MoneyInCome - MoneyOutCome) < 0);
            if (isMoneyInDeficit) {
                recruitValue += AIType.GetAdmWeight(-6, -5, -5);
            }

            bool isGrainInDeficit = ((GrainInCome - GrainOutCome) < 0);
            if (isGrainInDeficit) {
                recruitValue += AIType.GetAdmWeight(-6, -5, -5);
            }

            // �����Ǯ���� ��ļ Ȩ�ص�Ӱ��
            if (MoneyStorage < MoneyInCome * 3) {
                // ��Ǯ����3������봢��
                recruitValue += AIType.GetMilWeight(-9, -8, -7);
                recruitValue += AIType.GetAdmWeight(-7, -6, -5);
            }else if (MoneyStorage < MoneyInCome * 5) {
                recruitValue += AIType.GetMilWeight(-7, -6, -5);
                recruitValue += AIType.GetAdmWeight(-5, -4, -3);
            } else if (MoneyStorage < MoneyInCome * 10) {
                recruitValue += AIType.GetMilWeight(-5, -4, -3);
                recruitValue += AIType.GetAdmWeight(-5, -4, -3);
            }

            // ������ʳ���� ��ļ Ȩ�ص�Ӱ��
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

            // ���� ����ս�� ��Ȩ�ص�Ӱ��
            if (IsInWar) {
                recruitValue += AIType.GetMilWeight(1, 2, 3);
                recruitValue += AIType.GetAdmWeight(2, 3, 4);
            } else {
                recruitValue += AIType.GetMilWeight(-1, -1, -1);
                recruitValue += AIType.GetAdmWeight(-1, -1, -1);
            }

            this.recruitValue = recruitValue;

            // NOTICE: ��ȴ�ڣ������һ���½���һЩ���� Ҫ��һ��ʱ��Ż��ٴ������½�����һЩ�ݴ�ռ�
            if (recruitCooling > 0) {
                recruitCooling--;
                //Debug.Log($"��Ӧ������ļ����, cooling: {recruitCooling}, recruitValue: {recruitValue}");
                
                return false;
            }
            //Debug.Log($"�ж���ļ�¾����, recruitValue: {recruitValue}");
            return recruitValue > 0;
        }

        /// <summary>
        /// ��ļ���ʵľ�����Ŀ-Action
        /// </summary>
        public async void RecruitSuitArmyNum() {

            // �ҵ����ʵ���ļʡ��
            // NOTICE: ����Ȩ�ظߣ��Ҿ���Ȩ��ֵ�ͣ��� recruitValue ��ʡ�ݣ�������
            int recruitNum = recruitValue;
            List<Province> highSafeValuePrvc = GetPrvcBySafeOrder(CurAbleMap, recruitNum, true);

            // ���Ҫ��ļ�ĵ�λ
            // TODO: ����ذ���Ҫ��ļ�ĵ�λ
            ArmyUnitData armyUnitData = await ArmyUnitHelper.GetArmyUnitDataAsync(null);

            // ���þ��ӵ�λ����ص�
            Province ArmyGather = null;
            if (highSafeValuePrvc.Count > 0) {
                ArmyGather = highSafeValuePrvc[0];
            }

            // ���� recruitValue ������ļ
            int i = 0;
            while(recruitNum > 0) {
                if (i >= 0 && i < highSafeValuePrvc.Count) {
                    highSafeValuePrvc[i].RecruitArmyEvent(armyUnitData);
                }
                
                i++;
                // ��ֹԽ��
                if(i >= highSafeValuePrvc.Count) {
                    i = 0;
                }
                recruitNum--;
            }

            

            // ������ȴ�ڣ�ÿ����ļ֮�����ȴ��Ϊ: 24 + Ҫ��ļ������*2
            recruitCooling = 24 + recruitValue * 2;

            Debug.Log($"AI�ѳɹ���ļ, recruitCooling:{recruitCooling}, recruitValue:{recruitValue}, HighSafe: {highSafeValuePrvc.Count}");
        }

        /// <summary>
        /// �ж��Ƿ�Ӧ���Ƴ�һЩ����-Condition
        /// </summary>
        public bool ShouldRemoveArmy() {
            // TODO: �����һ���Ƴ���һЩ���� Ҫ��һ��ʱ��Ż��ٴ������Ƴ�����һЩ�ݴ�ռ�
            return false;
        }

        /// <summary>
        /// �Ƴ������ʵľ�����Ŀ-Action
        /// </summary>
        public void RemoveSuitArmyNum() {
            
        }

        /// <summary>
        /// ִ�з����������,���䵽���ʵ�ʡ����-Action
        /// </summary>
        public void DistributeArmy() {

            // ���㵥ֻ���Ӻ��ʵĵ�λ��Ŀ
            int suitUnitNum = 20;

            // ������ʵľ���ֻ��
            int suitArmyNum = ArmyNum / suitUnitNum;

            // ����Ƿ����Լ��ľ��Ӵ���ͬһ��ʡ�ݣ�����У��ͺϲ�


            // �ҵ����¼�ֵ��ߵļ���Ŀ�꣬Ȼ��������ǰ��
            // �߼�:
            // 1.�ж��ڹ���Ŀ���ڹ���Ŀ����ҪӦ�Եĵ�����Ŀ�������ǵı߾��ֱ��ҵ����SafeValue��ʡ��
            // 2.���������ʡ�ݣ�ɸѡ���е��˾��Ӵ��ڵ�ʡ��
            // 3.����ɸѡ��������˾���Խ�ٵ�ʡ��ԽҪ������ӣ��ȴ��������˴�Ӧ���и���AI���ԣ�
            // 4.����Щʡ�ݣ����η��� ������Ŀ + 1~3 �ľ��ӣ�������ȫȥ

            // NOTICE: ����Ӧ���в�ͬ��AI״̬��
            // a.����ս��: ����3���е���ʱ����ս��, �����䵽��������, ��ռ�ݸ��������ĵ���(MilValue���,���޵���), ��AI�����ж��Ƿ�������ս�� ��20%��������10%����0% ��
            // b.���ں�ƽ��: ���Է��䵽��������
            // c.����: ������3��ĵ��˹���ǿ��ʱ��>3����,����ӻ�ѡ���ӵ���������Զ�� **���ʺ�����** ��λ��
            // 5.Ϊ�˷�ֹ��Ȩ����ͬ��ʡ�ݶ�����Ƶ�����䣬Ӧ������һ����ȴ�������Լ�û���µ�ʡ�ݿ��Ʊ䶯ʱ�������Ŀ��

            Debug.Log("excute dustribute army!");


        }

        /// <summary>
        /// �趨Ҫռ���ʡ��Ŀ��-Action
        /// </summary>
        public void SetAttackTarget() {

        }

        #endregion

        #region ����ͼ ����

        /// <summary>
        /// ���� ����ͼ ����ʡ�ݵ�Ȩ��ֵ
        /// </summary>
        public void UpdateMap(List<Province> CurAbleMap) {

            foreach (var province in CurAbleMap) {
                if (!CurMapWeightDic.ContainsKey(province.provincePosition)) {
                    // ��������dic�У��½�ʡ��Ȩ��
                    ProvinceWeight weight = new ProvinceWeight(faction.FactionTag, AIType, province);
                    CurMapWeightDic.Add(province.provincePosition, weight);
                } else {
                    // ������dic�У������Ȩ��
                    CurMapWeightDic[province.provincePosition].UpdateWeight(faction.FactionTag, AIType, province);
                }
            }
        }

        public void AddMap(Faction faction) {
            CurAbleMap = CurAbleMap.Concat(faction.Territory).ToList();
            // ���µ�ͼȨ��
            UpdateMap(CurAbleMap);
        }

        public void RemoveMap(Faction faction) {
            foreach (var province in faction.Territory) {
                // �� CurAbleMap ���Ƴ�
                if (CurAbleMap.Contains(province)) {
                    CurAbleMap.Remove(province);
                }

                // �� Dic ���Ƴ�
                if (CurMapWeightDic.ContainsKey(province.provincePosition)) {
                    CurMapWeightDic.Remove(province.provincePosition);
                }
            }
        }


        /// <summary>
        /// �Ӹ�����ʡ���������ҳ����ü�ֵ ���/��� ��ָ����ʡ��
        /// </summary>
        /// <param name="provinceList">������ʡ���б�</param>
        /// <param name="num">Ҫ���ص�ָ����Ŀ</param>
        /// <param name="IsDesc">�Ƿ����󾭼ü�ֵ��͵�ʡ��</param>
        /// <returns></returns>
        public List<Province> GetPrvcByEcoOrder(List<Province> provinceList, int num, bool IsDesc) {
            
            List<ProvinceWeight> weights = GetWeightList(provinceList);
            // ��ȡ ɸѡ����ʡ��
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
            // ��ȡ ɸѡ����ʡ��
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
        /// ��ȡָ��ʡ�ݵ��� �ȫ/���ȫ ��ʡ���б�
        /// </summary>
        public List<Province> GetPrvcBySafeOrder(List<Province> provinceList, int num, bool IsDesc) {
            List<ProvinceWeight> weights = GetWeightList(provinceList);
            // ��ȡ ɸѡ����ʡ��
            List<Province> provinces = new List<Province>();

            if (!IsDesc) {
                // ��������
                weights.Sort((rec1, rec2) => {
                    // NOTICE: ��ȫָ�� = ���ü�ֵ - ���¼�ֵ
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
        /// �� Province �б� תΪ ProvinceWeight �б�
        /// </summary>
        private List<ProvinceWeight> GetWeightList(List<Province> provinceList) {
            List<Vector3> rec = provinceList.Select((province) => province.provincePosition).ToList();

            // ��ȡ Ȩ����λ
            List<ProvinceWeight> weightList = new List<ProvinceWeight>();
            foreach (var position in rec) {
                weightList.Add(CurMapWeightDic[position]);
            }

            return weightList;
        }

        /// <summary>
        /// ��ȡ���� ProvinceWeight �����ǰ num �� Province
        /// </summary>
        private void GetPrvcListByNum(List<ProvinceWeight> weights, ref List<Province> provinces, int num) {
            // NOTICE: �����ȡ�������
            // ���ɸѡ���Ĳ���num������ȫ���� / ����(�����)������һ��
            if (num < weights.Count) {
                int randomInt = UnityEngine.Random.Range(0, 10) + 1;
                if (randomInt <= 4) {
                    num /= 2;
                }
            }

            int i = 0;
            // ��ȡָ��num����Ŀʡ��
            while (i < num && i < weights.Count) {
                provinces.Add(MapController.Instance.GetProvinceByID(weights[i].HexID));
                i++;
            }
        }

        #endregion


    }

    
}
