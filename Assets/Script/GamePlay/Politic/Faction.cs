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
    /// ���ڼ�¼һ����ϵ��������Ϣ
    /// </summary>
    [System.Serializable]
    public class Faction : MonoBehaviour {

        [Header("�������� �����Ĺؼ���Ϣ")]
        private FactionInfo factionInfo;
        public FactionInfo FactionInfo { get => factionInfo;private set => factionInfo = value; }

        public string FactionTag { get => FactionInfo == null ? "NoTag" : FactionInfo.FactionTag; }

        public bool UseAI = true;

        private FactionAI FactionAI;

        #region ��������Դ
        FactionResource resource;
        public FactionResource Resource { get => resource; private set => resource = value; }
        
        /// <summary>
        /// ��ȡ���������е����� ��Ŀ
        /// </summary>
        public long GetTotalManpower() {
            Resource.CaculateTotalManpower(Territory.ToArray());
            return Resource.TotalManpower;
        }
        
        public int GetTotalIncome() {
            return Resource.GetTotalIncome() - Resource.GetTotalOutcome();
        }

        // ����˰�ʡ�������ά�����õĻص���Ŀǰ��������ͬ��
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


        #region �⽻��ϵ
        // �⽻��ϵͼ Tag - �⽻��ϵ ��ӳ��
        // ��ʼ�Ǹþ籾����������
        // ����ݳ������е�����������ʼ��֮
        Dictionary<string, DiploRelation> diploRelations = new Dictionary<string, DiploRelation>();
        public void AddDiploRelation(Faction hostFaction, Faction targetFaction) {
            if (hostFaction.FactionInfo.FactionTag == targetFaction.FactionInfo.FactionTag) {
                // ���Ὠ����������⽻��ϵ
                return;
            }

            bool isRival = hostFaction.FactionInfo.IsRival(targetFaction.FactionInfo);
            DiploRelation diploRelation = new DiploRelation(
                hostFaction.FactionInfo, targetFaction.FactionInfo, false, false, isRival, false
            );
            diploRelations.Add(targetFaction.FactionInfo.FactionTag, diploRelation);

            //Debug.Log("�⽻��ϵ����:" + diploRelation.hostFaction.FactionTag + "_" + diploRelation.targetFaction.FactionTag);
        }

        public DiploRelation GetDiploRelation(string tag) {
            if (diploRelations.ContainsKey(tag)) {
                return diploRelations[tag];
            } else {
                return new DiploRelation();
            }
        }

        /// <summary>
        /// ��øù������е��⽻��ϵ��Ϣ
        /// </summary>
        public List<DiploRelation> GetAllDiploRelations() {
            return diploRelations.Values.ToList();
        }

        public bool IsFactionInWar() {
            foreach (var dipRelation in diploRelations.Values.ToList())
            {
                // �������ù��⽻��ϵ IsInWar�ֶ�
                if (dipRelation.IsInwar) {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region �����뽫��
        // NOTICE: ���ӵ�����ͬ������ArmyController������
        // ��ǰ���ҵ����о���
        private List<Army> factionArmys;
        public List<Army> FactionArmys { get => factionArmys; set => factionArmys = value; }

        public int GetTotalArmyCount(ref int ArmyValue) {
            ArmyValue = 0;
            if (FactionArmys != null) {
                // ���� ���о��� ����ά���ѵĿ����ܺ�
                foreach (var army in FactionArmys)
                {
                    ArmyValue += (int)army.ArmyData.GetArmyMaintenanceCost() / 1000;
                }
            }
            return FactionArmys.Count;
        }

        /// <summary>
        /// ��ȡ��ǰ������ļ�ľ�����Ŀ
        /// </summary>
        /// <param name="RecrValue">������ļ�Ĳ��ӵ�Ȩ�أ�Cost��</param>
        /// <returns></returns>
        public int GetRecruitingArmyCount(ref int RecrValue) {
            int ans = 0;
            RecrValue = 0;
            foreach (var province in Territory)
            {
                if(province.recruitTaskList != null) {
                    foreach (var task in province.recruitTaskList)
                    {
                        // ����Ȩ��: ά�ָ�ֻ���ӵĿ��� (Ҫ��1000)
                        RecrValue += (int)task.armyUnitData.armyReinforcementCost / 1000;
                    }
                    ans += province.recruitTaskList.Count;
                }
            }
            return ans;
        }
        
        /// <summary>
        /// ������о�����������
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
        /// ���¾��Ӳ�����
        /// </summary>
        public void UpdateArmySupplyLine(List<Army> FactionArmys) {
            foreach (var army in FactionArmys)
            {
                // ��ֻ������Ҫ �������� ����
                if (army.ArmyData.NeedSupplySupport) {
                    BuildSupplyLine(army);
                } else {
                    CancelSupplyLine(army);
                }
            }
        }


        // ��������ص�
        public delegate void GeneralChangeDelegate(string generalTag);

        // ��ǰ���ҵ����н���
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
        /// Ϊ�����½����죬�Ƴ������еĽ���
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
        /// ָ�ɽ��쵽���ӣ��Ƴ�����
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
        /// �жϽ����Ƿ��Ѿ�������
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
        /// ���ؽ������ڵľ��ӵ�λ
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

        #region �����������

        public Province GetCapital() {
            if (string.IsNullOrEmpty(factionInfo.CapitalProvinceID)) {
                // �׶��ֶ�Ϊ�յ����
                return Territory[0];
            }

            // �����׶���ID Ѱ�Ҷ�Ӧ��ʡ��
            uint capitalID = uint.Parse(factionInfo.CapitalProvinceID);
            Province capital = MapController.Instance.GetProvinceByID(capitalID);
            if (capital != null) {
                return capital;
            } else {
                return Territory[0];
            }
        }

        // ��ǰӵ�еĵ���
        public List<Province> Territory;
        // Ҳ�ǵ�ǰӵ�е����� ֻ������string���ͣ�x����_y����_z���꣩
        public List<string> TerritoryInitString;

        // ��ǰ���еĲ�������
        public List<Province> AllSupplyCenter;

        public int GetProvinceNum() {
            return Territory.Count;
        }

        /// <summary>
        /// �����ȡһ�����ʵĳ���ʡ��Ŀ��(����������)
        /// </summary>
        public Province GetWithdrawTarget() {
            // TODO��Ӧ������ʡ�ݳ���Ȩ�أ�ѡ�������������ȫ��ʡ�ݳ���

            // Ŀǰ���߼� ������˵�һ�� δ��ս δΧ�� δ��ռ�� ��ʡ����
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

                // ʡ�����Ѿ�û�к��ʵĿɹ����˵������� ��ֹ����ѭ��
                count++;
                if (count >= territoryCount) {
                    return Territory[index];
                }
            }
            return ans;
        }
        
        /// <summary>
        /// ���¸������д��в������ĵ�ʡ��
        /// </summary>
        public List<Province> UpdateAllSupplyCenter() {
            if (AllSupplyCenter == null) {
                AllSupplyCenter = new List<Province>();
            }
            AllSupplyCenter.Clear();
            foreach (var province in Territory)
            {
                // ������� SupplyCenter ��ô��ӽ���
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
            // ��� �Ѿ���¼��ʡ�� ���Ƴ�
            if (AllSupplyCenter.Contains(province)) {
                AllSupplyCenter.Remove(province);
            }
        }

        /// <summary>
        /// Ϊָ���ľ��ӽ���/ά��һ�����ʵ�����/������
        /// </summary>
        public void BuildSupplyLine(Army targetArmy) {
            if(targetArmy.CurSupplyTask != null) {
                // TODO: ��������� SupplyTask ��ˢ��֮
                return;
            } else {
                // TODO: �������û�� SupplyTask �򹹽�һ�� SupplyTask
                // TODO: ����㣨�����׶�/����Ĳ������ģ� ���յ㣨��������λ�ã���������һ�������·��
                Province capital = GetCapital();
                List<Province> supplyPath = MapController.Instance.GetSupplyPath(capital, targetArmy.CurrentProvince);
                ArmySupplyTask armySupplyTask = new ArmySupplyTask(targetArmy, capital, supplyPath);

                // TODO: ��ȡ�뵱ǰ��������� ��������

                // TODO: ���Ϊ������ʡ�����ȣ��û������Զ���������λ�ã�

                // TODO: ���� Army �� SetSupplyTask;
                targetArmy.SetSupplyTask(armySupplyTask);

                //Debug.Log("�����˲������񣬳�����Ϊ: " + capital.provinceData.provinceID);
            }

            return;
        }

        /// <summary>
        /// ����ָ�����ӵĲ���������
        /// </summary>
        public void CancelSupplyLine(Army targetArmy) {
            if (targetArmy.CurSupplyTask == null) {
                // ��ֻ����û�� supplyTask
                return;
            }

            targetArmy.CancelSupplyTask();
        }

        #endregion

        #region ���캯��
        public static Faction GetBaseFaction(FactionInfo factionInfo, Transform parentTransform) {
            GameObject factionObj = new GameObject();
            factionObj.transform.parent = parentTransform;
            factionObj.name = factionInfo.FactionName;

            //������ ����faction��Ϣ
            Faction factionComponent = factionObj.AddComponent<Faction>();
            factionComponent.FactionInfo = factionInfo;
            factionComponent.TerritoryInitString = factionInfo.ProvincesInBeginning;
            factionComponent.Territory = new List<Province>();
            factionComponent.Resource = new FactionResource();

            // ��ȡ����
            factionComponent.FactionArmys = new List<Army>();
            factionComponent.FactionGenerals = new List<General>();
            foreach (GeneralData data in factionInfo.FactionGenerals)
            {
                factionComponent.CreateGeneral(new General(data));
            }

            return factionComponent;
        }

        public void InitFaction() {

            // ע��ʱ������¼�
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
        /// ע�ᵽ�ս��¼���
        /// </summary>
        public void UpdateFaction_Day() {

            // ֧��ʡ��ά����
            Resource.PayMaintenance(Territory.ToArray());
            // ˢ��ʡ��״̬
            foreach (Province province in Territory)
            {
                province.UpdateProvinceData_Day();
            }

            // ��ʡ���л��˰�պ���ʳ
            // NOTICE:����Ӧ����ÿ�»����Դ�ģ�����ֹ̫���ӣ���Ϊÿ��
            if (Territory.Count > 0) {
                Resource.GetTaxAndGrain_Day(Territory.ToArray());
            }

            // ֧������ά�����ã��Ƹ�����ʳ��
            Resource.PayArmy(FactionArmys);
            Resource.PaySupply(FactionArmys);

            // TODO: ���ӵ���ʳ���ģ�������ά����
            UpdateArmySupplyLine(FactionArmys);

            // NOTICE: UpdateArmyData_Day Ӧ�÷ŵ�Army�д�����������������޷������Ѿ�ϵͳ

            // TODO: ������ά��

            // TODO: ���Ż�ý�Ǯ��Ƶ
            AudioManager.PlayAudio(AudioEffectName.GainResource);
        }

        public void UpdateFaction_Hour() {

            foreach (Province province in Territory) {
                province.UpdateProvinceData_Hour();
            }


        }

        public void UpdateFaction_Month() {

            // ���˰�� �� ��ʳ
            //if (Territory.Count > 0) {
            //    Resource.GetTaxAndGrain_Month(Territory.ToArray());
            //}
            

        }

    }
}