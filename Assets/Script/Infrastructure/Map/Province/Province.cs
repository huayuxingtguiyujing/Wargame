using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.Application.TimeTask;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.Audio;
using WarGame_True.Infrastructure.HexagonGrid.MapObject;
using WarGame_True.Infrastructure.Map.Controller;

namespace WarGame_True.Infrastructure.Map.Provinces{
    /// <summary>
    /// ʡ�ݿ����࣬��ҪHexGrid ����ʡ�ݵ�UI
    /// </summary>
    [RequireComponent(typeof(HexGrid))]
    public class Province : MonoBehaviour , IPointerClickHandler{
        HexGrid hexGrid;

        [Header("ʡ�ݳ�����")]
        public string provinceController;
        [Header("ʡ�ݵ�ǰ������")]
        public string provinceCurrentContrl;

        //[Header("ʡ������")]
        public string provinceName { get => provinceData.provinceName; }
        public uint provinceID { get => provinceData.provinceID; }
        public string provincePos { get; private set; }
        public Vector3 provincePosition { get; set; }

        #region ����ʡ������
        public ProvinceData provinceData;

        public void SetProvinceData(ProvinceData data) {
            provinceData = data;
        }

        /// <summary>
        /// ʡ���Ƿ�ĳ��tag��ռ��
        /// </summary>
        public bool UnderTagControl(string tag) {
            return provinceData.CurrentControlTag == tag;
        }

        /// <summary>
        /// ��ʱ�����ʡ�����ݣ���Ҫ�ڹ����������֣�ע�ᵽʱ����¼���
        /// </summary>
        public void UpdateProvinceData_Day() {
            // ��˰����ʳ���
            provinceData.CollectGrainToStorage();
            provinceData.CollectTaxToStorage();

            // TODO:�ı�ķ϶ȡ����ٶȡ�balabala����ֵ

            // TODO��ˢ��ui

            // ����ʡ�ݵ�ռ��״̬

            // ����ʡ������
            provinceData.UpdateProvinceModify();
        }

        public void UpdateProvinceData_Hour() {

        }

        /// <summary>
        /// ���ʡ�ݵ�˰����
        /// </summary>
        public float GetProvinceTaxNum_Month() {
            
            return provinceData.GetProvinceTax_Month();
        }

        public float GetProvinceTaxNum_Day() {
            // NOTICE: ���ڸ�Ϊÿ�ջ��һ����Դ������ CollectTaxStorage ����
            return provinceData.CollectTaxStorage();
        }

        public float GetProvinceMaintenance() {
            return provinceData.GetProvinceMaintenance();
        }

        public float GetProvinceGrainNum_Month() {
            return provinceData.GetProvinceGrain_Month();
        }

        public float GetProvinceGrainNum_Day() {
            // NOTICE: ���ڸ�Ϊÿ�ջ��һ����Դ������ CollectGrainStorage ����

            // TODO: ���㵱ǰʡ���ϵ�ʡ�ݿ����ߵ����о��ӣ��۳����Ӳ������ѣ����Ͻ�
            return provinceData.CollectGrainStorage();
        }
        
        public void AddLocalGrainCost() {

        }

        #endregion

        #region ʡ�� �������� ����

        private List<Army> armiesInProvince;

        public int ArmyNumInProvince { get => armiesInProvince.Count; }

        public List<Army> ArmiesInProvince { get => armiesInProvince; }


        private bool OnlyExistAnemiesInProvince(string curCtrlTag, out string anemyTag) {
            bool ans = false;
            anemyTag = "";
            foreach (Army armyInProvince in armiesInProvince) {

                // �����еľ��Ӳ���������
                if ( armyInProvince.ArmyActionState == ArmyActionState.Withdrawing) {
                    continue;
                }

                // ��ʡ�ݿ������ǵжԣ�ʡ�ݻᱻռ��
                bool isHostile = PoliticLoader.Instance.IsFactionInWar(curCtrlTag, armyInProvince.ArmyData.ArmyTag);
                if (isHostile) {
                    anemyTag = armyInProvince.ArmyData.ArmyTag;
                    ans = true;
                }

                // ��ʡ��ӵ���ߵľ��ӣ���ʡ�ݲ��ᱻռ��
                if (curCtrlTag == armyInProvince.ArmyData.ArmyTag) {
                    ans = false;
                    anemyTag = armyInProvince.ArmyData.ArmyTag;
                    return ans;
                }
            }
            return ans;
        }

        public bool ExistArmy() {
            return armiesInProvince != null && armiesInProvince.Count > 0;
        }

        public bool ExistFriendlyArmy() {
            if (armiesInProvince == null) {
                return false;
            }
            // ��������Ƿ���
            foreach (Army army in armiesInProvince)
            {
                if (army.ArmyData.IsArmyTag(provinceData.OwnerTag)) {
                    return true;
                }
            }
            return false;
        }

        public bool ExistHostileArmy() {
            if (armiesInProvince == null) {
                return false;
            }
            foreach (Army army in armiesInProvince) {
                bool isHostile = PoliticLoader.Instance.IsFactionInWar(provinceData.OwnerTag, army.ArmyData.ArmyTag);
                // ��⵽�о�����
                if (isHostile) return true;
            }
            return false;
        }


        public void RemoveArmyFromProvince(Army army) {
            if (armiesInProvince.Contains(army)) {
                armiesInProvince.Remove(army);
            }

            // ���¾��Ӷ�ʡ�ݵ�Ӱ�죨ʡ�����ݣ�

            // ��ʡ�ݴ��ڼ�������ʱ,�����޵з�����ʱ���ÿ�ռ��
            if (!OnlyExistAnemiesInProvince(provinceData.CurrentControlTag, out string anemyTag)) {
                ForceBreakOccupy();
            }
        }

        public void ArmyArriveInProvince(Army army) {
            if (!armiesInProvince.Contains(army)) {
                armiesInProvince.Add(army);

                // ��鲹��·���Ƿ���Ҫ�ع�
                army.ShouldRebuildSupplyLine();
            }

            // ���¾��Ӷ�ʡ�ݵ�Ӱ�죨ʡ�����ݣ�

            // ��ʡ��ֻ�ез�����ʱ���ж��Ƿ�Ҫ����Χ��
            if (OnlyExistAnemiesInProvince(provinceData.CurrentControlTag, out string anemyTag)) {

                if (currentOccupy == null && !UnderTagControl(anemyTag)) {
                    // ʡ��δ��ռ�� ��û��ռ���¼� ��ʡ�ݿ�ʼ��ռ��
                    Debug.Log("��ʼռ��: amrytag:" + anemyTag + ",��ǰ������:" + provinceData.CurrentControlTag + ",ӵ����:" + provinceData.OwnerTag);
                    StartAOccupyTask(provinceData.CurrentControlTag, anemyTag);
                } else if (currentOccupy != null && currentOccupy.occupyingTag == anemyTag) {
                    // �Ѿ���ռ���¼����������Ǽ�����ռ�� �����
                    currentOccupy.AddOccupyingArmy(army);
                } else {
                    // �Ѿ���ռ���¼�������������������

                }
            }
        }


        public List<Army> GetAnemiesInProvince(string factionTag) {
            List<Army> anemies = new List<Army>();
            foreach (Army armyInProvince in armiesInProvince)
            {
                //���ǵжԹ��� �Ҳ��ڳ����� ���������б���
                bool isHostile = PoliticLoader.Instance.IsFactionInWar(factionTag, armyInProvince.ArmyData.ArmyTag);
                if (isHostile && armyInProvince.ArmyActionState != ArmyActionState.Withdrawing) {
                    anemies.Add(armyInProvince);
                } 
            }

            return anemies;
        }

        /// <summary>
        /// �������飬��������о�ֹ�ľ���
        /// </summary>
        public List<Army> GetStayingArmy(List<Army> armies) {
            return (from army in armies
                    where army.ArmyActionState == ArmyActionState.IsStaying
                    select army).ToList();
        }

        public List<Army> GetAlliesInProvince(string factionTag) {
            List<Army> allies = new List<Army>();
            foreach (Army armyInProvince in armiesInProvince) {
                //���ǵжԹ��� ���������б���
                bool isAlly = armyInProvince.ArmyData.ArmyTag == factionTag;
                if (isAlly) {
                    allies.Add(armyInProvince);
                }
            }

            return allies;
        }
        
        public void SetInitArmyUI() {
            foreach (Army army in armiesInProvince)
            {
                
            }
        }
        #endregion

        #region ʡ�� �¼�����ļ��ս�ۡ�Χ�ǣ�

        // ��ǰʡ�����ڽ��е���ļ�¼�
        private List<RecruitTask> recruitTaskList;

        // ��ǰʡ�ݽ��е�ս��, Ϊnull ���ʾδ����ս��
        public CombatTask currentCombat { get; private set; }
        [Header("combat prefab")]
        public GameObject combatPrefab;
        GameObject currentCombatObject;
        public void SetCurCombatObj(GameObject obj) {
            currentCombatObject = obj;
        }

        // ��ǰʡ�ݽ��е�ռ������
        public OccupyTask currentOccupy { get; private set; }

        // ��ǰʡ�ݽ��еĽ�������
        public List<BuildTask> buildTaskList { get; private set; }

        #endregion

        public void InitProvince(uint provinceID, Vector3 position) {
            hexGrid = GetComponentInChildren<HexGrid>();

            //����Id
            //provinceID = position.x.ToString() + "_" + position.y.ToString() + "_" + position.z.ToString();
            this.provincePos = provinceID.ToString();
            provincePosition = position;
            //Debug.Log(provincePosition);

            // TODO��д�� ����inspector����ʱ��ִ�д˲�
            //#if UNITY_EDITOR
            //            //����һ�� ProvinceData (����inspector����ʱ��ִ�д˲�)
            // TODO: ���ǵ���д����ģ�飬����Ҫ����������ұ���ʡ������һ�£����Բ������������������
            provinceData = ProvinceData.GetRandomProvinceData(provinceID, position);
            //#endif

            recruitTaskList = new List<RecruitTask>();
            buildTaskList = new List<BuildTask>();

            armiesInProvince = new List<Army>();

            // �����ʡ�����ƣ�������ʡ������
            if (!string.IsNullOrEmpty(provinceData.provinceName)) {
                hexGrid.ShowProvinceName(provinceData.provinceName);
            }

        }

        #region ��ļ���

        /// <summary>
        /// ��ļ��ť �󶨵��¼�
        /// </summary>
        public void RecruitArmyEvent(ArmyUnitData armyUnitData) {
            StartRecruitArmy(armyUnitData);
            // ֪ͨ���������ӵĿͻ���
            MapNetworkCtrl.Instance.StartRecruitArmyEvent(provinceData.provinceID, armyUnitData);
        }

        public void StartRecruitArmy(ArmyUnitData armyUnitData) {
            //Debug.Log("��ʼ��ļ����*1 �� "+ armyUnitData.armyUnitName + "place: " + provincePosition);

            RecruitTask task = new RecruitTask(armyUnitData.armyCostBaseDay, armyUnitData);
            recruitTaskList.Add(task);
            // ��ʾ������
            ShowRecruitProcess();
            // ע�ᵽʱ���¼��� 
            TimeSimulator.Instance.SubscribeDayCall(HandleRecruit);
        }

        private void HandleRecruit() {
            if (recruitTaskList == null) {
                return;
            }

            //��������ļ�¼����� TimeSimulator ���Ƴ�
            if (recruitTaskList.Count == 0) {
                TimeSimulator.Instance.UnsubscribeDayCall(HandleRecruit);
                return;
            }

            // ��ִ�е�һ����ļ����
            if (recruitTaskList.Count >= 1) {
                recruitTaskList[0].CountTask();
                // ������ļ������
                ShowRecruitProcess();
                if (recruitTaskList[0].IsOver == true) {
                    // ����ͬ��,�ڷ�����������spawnͬ��
                    ArmyNetworkCtrl.Instance.CreateArmyEvent(recruitTaskList[0].armyUnitData, this.provinceData.provinceID);
                    
                    recruitTaskList.Remove(recruitTaskList[0]);

                    if(recruitTaskList.Count >= 1) {
                        ShowRecruitProcess();
                    } else {
                        HideRecruitProcess();
                    }
                }
            }

            /* ͬʱִ�����е���ļ�¼� ����ɾ��
             * for(int i = recruitTaskList.Count - 1; i >= 0; i--) {
                recruitTaskList[i].CountTask();
                hexGrid.ShowRecruitProcess((int)recruitTaskList[i].costDay, (int)recruitTaskList[i].costDay -(int)recruitTaskList[i].lastDay);
                if (recruitTaskList[i].IsOver == true) {
                    //TODO: ��ļ��������ʡ�ݳ�ʼ�����ӣ�ͬʱ���б����Ƴ�
                    //Debug.Log("��ļ������:" + this.provincePosition + "_" + provinceData.provinceEco.prosperity);
                    //ArmyUnit armyUnitData = new ArmyUnit(recruitTaskList[i].armyUnitData);

                    // ����ͬ��,�ڷ�����������spawnͬ��
                    ArmyNetworkCtrl.Instance.CreateArmyEvent(recruitTaskList[i].armyUnitData, this.provinceData.provinceID);
                    // ����ģʽ�£����ڿͻ��˴�����(��Ҫ�����)
                    //ArmyController.Instance.CreateArmy(armyUnitData, this);
                    hexGrid.HideRecruitProcess();
                    recruitTaskList.Remove(recruitTaskList[i]);
                }
            }*/

        }

        public void ShowRecruitProcess() {
            if (recruitTaskList.Count >= 1) {
                // ����ļ����������ļ��������¸�����Ľ��ȣ�Ĭ�ϵ�һ��
                hexGrid.ShowRecruitProcess(recruitTaskList[0].costTime, recruitTaskList[0].lastTime, recruitTaskList.Count.ToString());
            }
        }

        public void HideRecruitProcess() {
            hexGrid.HideRecruitProcess();
        }

        #endregion

        #region �������
        // TODO: ��δ����

        public void BuildEvent(Building building) {
            StartBuild(building);
            // TODO: ֪ͨ�������ͻ��ˣ����н���
        }

        public void StartBuild(Building building) {
            // TODO: �жϱ����Ƿ��иý��������򲻼�

            Debug.Log("����һ����������!");

            BuildTask buildTask = new BuildTask(building);
            buildTaskList.Add(buildTask);
            // ��ʾ������
            ShowBuildProcess();
            // ע�ᵽʱ����¼�
            TimeSimulator.Instance.SubscribeDayCall(HandleBuild);
        }

        public void HandleBuild() {
            if (buildTaskList == null) {
                return;
            }

            //��������ļ�¼����� TimeSimulator ���Ƴ�
            if (buildTaskList.Count == 0) {
                TimeSimulator.Instance.UnsubscribeDayCall(HandleBuild);
                return;
            }

            // ��ִ�е�һ����ļ����
            if (buildTaskList.Count >= 1) {
                buildTaskList[0].CountTask();
                ShowBuildProcess();

                // �����Ѿ�����
                if (buildTaskList[0].IsOver == true) {
                    // TODO:  ����ͬ�� ʡ������
                    // TODO: ��ProvinceData����Ӹý�����Ϣ
                    provinceData.AddBuilding(buildTaskList[0].CurBuilding);

                    buildTaskList.Remove(buildTaskList[0]);

                    if (buildTaskList.Count >= 1) {
                        ShowBuildProcess();
                    } else {
                        HideBuildProcess();
                    }
                }
            }
        }

        public void ShowBuildProcess() {
            if (buildTaskList.Count >= 1) {
                hexGrid.ShowBuildProcess(buildTaskList[0].costTime, buildTaskList[0].lastTime, buildTaskList.Count.ToString());
            }
        }

        public void HideBuildProcess() {
            hexGrid.HideBuildProcess();
        }

        #endregion

        #region ս�����

        public void StartCombatEvent(List<Army> attacker, List<Army> defender) {
            Debug.Log("a combat start!" + "attacker num:" + attacker.Count + "defender num:" + defender.Count);

            MapNetworkCtrl.Instance.StartCombatEvent(this, attacker, defender);

        }

        public Combat StartCombatInProvince(List<Army> attacker, List<Army> defender) {
            //����һ�� combat ����
            currentCombatObject = Instantiate(combatPrefab);
            currentCombatObject.transform.position = transform.position;
            Combat combat = currentCombatObject.GetComponentInChildren<Combat>();
            combat.InitCombat(attacker, defender, this);

            //�½�һ��combattask
            CombatTask combatTask = new CombatTask(999, attacker, defender);
            currentCombat = combatTask;

            //ע�ᵽСʱ���¼���
            TimeSimulator.Instance.SubscribeHourCall(HandleCombat);

            return combat;
        }

        private void HandleCombat() {
            if (currentCombat == null) {
                return;
            }

            // ʱ�̸��¾�������λ��
            currentCombatObject.GetComponent<Combat>().UpdateArmyPos();

            // ����ͬ��
            MapNetworkCtrl.Instance.HandleCombat(this);

            // ���ص���ս�����
            UpdateCombatPanel(currentCombat);

            currentCombat.CountTask();
            if (currentCombat.IsOver) {

                // TODO : 1.��ȡ��������
                // TODO : 2.���������ݼ��ص����������
                CombatMessage combatMessage = currentCombat.CombatCountMessage;

                /*//����˫����ս����ս��״̬
                ArmyHelper.GetInstance().ExitCombat(currentCombat.Attackers);
                ArmyHelper.GetInstance().ExitCombat(currentCombat.Defenders);

                //TODO : սʤ�����ڱ�ʡ�ݣ�ʧ�ܷ�����
                ArmyController.Instance.SetArmyDestination(
                    ArmyController.Instance.withdrawTarget,
                    currentCombat.Attackers, true
                );
                ArmyHelper.GetInstance().SetArmyStayIn(currentCombat.Defenders);*/

                // ����ս������,������ǰս��
                currentCombatObject.GetComponentInChildren<Combat>().CancelCombat(true);

                TimeSimulator.Instance.UnsubscribeHourCall(HandleCombat);
                currentCombat.ForceToComplete();
                MapNetworkCtrl.Instance.CombatOverEvent(provinceData.provinceID);

                //ս������
                Debug.Log($"combat over,army in this province:{armiesInProvince.Count}");
            }
        }

        /// <summary>
        /// ����ս�۽��̣����ڿͻ���������ͬ��ʱʹ�ã���ʹ��CombatTask
        /// </summary>
        public void HandleCombat_Network(BattlePlaceNetwork[] frontAtUnits, BattlePlaceNetwork[] frontDeUnits, BattlePlaceNetwork[] rearAtUnits, BattlePlaceNetwork[] rearDeUnits,
            BattlePlaceNetwork[] withdrawAtUnits, BattlePlaceNetwork[] withdrawDeUnits) {
            // ʱ�̸��¾�������λ��
            if (currentCombatObject != null) {
                currentCombatObject.GetComponent<Combat>().UpdateArmyPos();
            }

            if (currentCombat != null) {
                currentCombat.UpdateCombatUnits(
                    frontAtUnits, frontDeUnits,
                    rearAtUnits, rearDeUnits,
                    withdrawAtUnits, withdrawDeUnits
                );
            }

            // ����ս�۽���
            UpdateCombatPanel(currentCombat);

        }

        /// <summary>
        /// ����һ����������ͬ�����ݵ�CombatTask�����ز�����ս���߼����У���ִ��CombatTask��CountTask
        /// </summary>
        public void StartCombatNetwork(List<Army> attackers, List<Army> defenders) {
            if (currentCombat == null) {
                currentCombat = new CombatTask(999, attackers, defenders);
            }
        }

        /// <summary>
        /// ǿ�н������ص�combatTask
        /// </summary>
        public void CancelCombat() {
            currentCombat = null;
        }

        // ���ص�combatPanel���
        public delegate void UpdateCombatPanelCall(CombatTask combatTask);
        private event UpdateCombatPanelCall updateCombatPanelCall;
        public void UpdateCombatPanel(CombatTask combatTask) {
            if (updateCombatPanelCall != null) {
                updateCombatPanelCall.Invoke(combatTask);
            }
        }

        /// <summary>
        /// ��combatpanel��彨������ / �ж�����
        /// </summary>
        public void LinkToCombatPanel(UpdateCombatPanelCall call) {
            updateCombatPanelCall = call;
        }

        public void UnLinkToCombatPanel() {
            updateCombatPanelCall = null;
        }

        #endregion

        #region ռ�����

        private void StartAOccupyTask(string ctrlTag, string occupyTag) {
            List<Army> anemies = GetAnemiesInProvince(ctrlTag);
            
            // TODO: ���ݾ��ӡ����졢���������趨���ǻ���ʱ��
            currentOccupy = new OccupyTask(this, anemies, 5);
            hexGrid.ShowOccupyProcess(currentOccupy.totalOccupyCost, currentOccupy.currentOccupyCost);

            //ע�ᵽ�ս��¼���
            TimeSimulator.Instance.SubscribeDayCall(HandleOccupy);

        }

        private void HandleOccupy() {
            if (currentOccupy == null) {
                TimeSimulator.Instance.UnsubscribeDayCall(HandleOccupy);
                return;
            }

            // ��õ�ǰʡ�ݵ���������ռ��ĵ��ˣ�����֮
            List<Army> occupyingAnemies = GetStayingArmy(GetAnemiesInProvince(provinceData.OwnerTag));
            currentOccupy.UpdateOccupyingArmy(occupyingAnemies);

            // �ƽ�ռ�����
            hexGrid.ShowOccupyProcess(currentOccupy.totalOccupyCost, currentOccupy.currentOccupyCost);

            currentOccupy.CountTask();
            if (currentOccupy.IsOver) {

                if (currentOccupy.IsSuccess) {
                    // ����ɹ���������ʡ���趨Ϊ��ռ�죬ͬʱ����ʡ����ֵ
                    SetProvinceOccupiedStatu(provinceData.OwnerTag, currentOccupy.occupyingTag);
                    Debug.Log("ռ���ж���������ǰ��ʡ��ӵ����:" + provinceData.OwnerTag + ",������:" + provinceData.CurrentControlTag);

                    // ����δ�ɹ�����
                }

                ForceBreakOccupy();

                // �Ƴ�ռ���¼�
                TimeSimulator.Instance.UnsubscribeDayCall(HandleOccupy);
            }
        }

        private void ForceBreakOccupy() {
            // �ÿ�ռ���¼�
            hexGrid.HideOccupyProcess();
            currentOccupy = null;
        }
        
        #endregion

        #region ʡ���ϵľ��ӣ�ս����ĥ���ΰ������ӡ�ռ�죩

        #endregion

        #region Colider�¼�

        private void OnMouseEnter() {
            //���Ŀǰ��ui�ϣ�����
            if (EventSystem.current.IsPointerOverGameObject()) {
                return;
            }
            //Debug.Log("you enter this province!");
            hexGrid.SetHexGridActive();

            // ��ʡ����ʾ


        }

        private void OnMouseExit() {
            hexGrid.SetHexGridNormal();

            // �ر�ʡ����ʾ

        }

        private void OnMouseDown() {
            // ʹ��OnPointerClick
            /*if (EventSystem.current.IsPointerOverGameObject()) {
                return;
            }

            *//* ʡ��ui�Ѿ����� ��ֹ���
             * if (!ProvincePanel.ActiveProvincePanel.Visible) {
                ProvincePanel.ActiveProvincePanel.Show();
            } else {
                //ʡ��ui �Ѿ�����
                return;
            }*//*

            ProvincePanel.ActiveProvincePanel.Show();
            ProvincePanel.ActiveProvincePanel.InitProvincePanel(this);


            hexGrid.SetHexGridChoose();*/
        }

        private void OnMouseUp() {
            hexGrid.SetHexGridNormal();
        }

        public void OnPointerClick(PointerEventData eventData) {

            //NOTICE: ʹ��OnMouseDownʱ�����ui�ᴩ͸ui���������¼������Ը���IPOinterClick

            //չʾʡ�ݽ���UI
            //if (!ProvincePanel.ActiveProvincePanel.Visible) {

            //} else {
            //    //ʡ��ui �Ѿ�����
            //    return;
            //}
            //Debug.Log("you click this province!");
            if (eventData.button == PointerEventData.InputButton.Left) {
                ProvincePanel.ActiveProvincePanel.Show();
                ProvincePanel.ActiveProvincePanel.InitProvincePanel(this);

                hexGrid.SetHexGridChoose();
                AudioManager.PlayAudio(AudioEffectName.ButtonClick01);
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                //�����ƶ� �߼�������ŵ�armycontrl��
            }
        }

        #endregion

        #region ����ʡ�ݵ�״̬
        public void SetProvinceControlStatu(string contrl, string currentContrl, UnityEngine.Color color) {
            provinceData.SetOwnerStatu(contrl);
            provinceData.CurrentControlTag = currentContrl;

            SetProvinceOccupiedStatu(provinceData.OwnerTag, provinceData.CurrentControlTag);

            hexGrid.SetHexGridBG(color);
        }

        public void SetProvinceOccupiedStatu(string contrl, string currentContrl) {
            
            if (contrl != currentContrl) {
                // ���ӵ����������� ��һ��������ռ��״̬
                provinceData.SetControlStatu(currentContrl);
                hexGrid.SetHexGridOccupied(
                    PoliticLoader.Instance.GetFactionColor(currentContrl)
                );
            } else {
                hexGrid.SetHexGridUnoccupied();
            }
        }

        public void SetProvinceLostControl() {
            hexGrid.SetHexGridBG(new UnityEngine.Color(1, 1, 1, 0.2f), false);
        }

        public void SetProvinceAsMovePath() {
            hexGrid.SetHexGridAsMovePath();
        }

        public void SetProvinceAsWithdrawPath() {
            hexGrid.SetHexGridAsWithdrawPath();
        }

        public void SetProvinceCloseMovePath() {
            hexGrid.SetHexGridNoInteract();
        }

        public void SetProvinceAsSupplyLine() {
            hexGrid.SetHexGridSupplyLine();
        }

        public void SetProvinceCloseSupplyLine() {
            hexGrid.SetHexGridNoInterBase();
        }

        public void SetProvinceNormal() {
            hexGrid.SetHexGridNormal();
        }
        
        #endregion

        /// <summary>
        /// ��ȡ����ʡ�ݵ�λ��
        /// </summary>
        public List<Vector3> GetNeighborProvinces() {
            List<Vector3> neighborPosition = new List<Vector3>();

            //NOTICE����HexagonStruct���У���ȡ�ھӽڵ�Ĳ���
            List<Vector3> direction = new List<Vector3> {
                new Vector3(1, 0, -1), 
                new Vector3(1, -1, 0), 
                new Vector3(0, -1, 1),
                new Vector3(-1, 0, 1), 
                new Vector3(-1, 1, 0), 
                new Vector3(0, 1, -1)
            };
            foreach (Vector3 dir in direction)
            {
                neighborPosition.Add(hexGrid.hexPosition + dir);
            }
            
            return neighborPosition;
        }

    }

}
