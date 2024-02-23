using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.Application.TimeTask;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Infrastructure.Map.Util;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// һ֧����
    /// </summary>
    public class Army : ArmyGUI , ArmyStateMachine {

        // �ҽӵ�armydata
        ArmyData armyData;
        public ArmyData ArmyData { get => armyData;private set => armyData = value; }
        /// <summary>
        ///  ���㲿�ӣ�ȥ���������ĵ�λ��������û����ս�����Ĳ��ӣ����ٸõ�λ
        /// </summary>
        public void SettlementArmyData() {
            ArmyData.SettleArmyData();

            try {
                if (ArmyData.ArmyUnitDatas.Count <= 0) {
                    RemoveArmy();
                }
            } catch {
                Debug.LogError("�޷������Ƴ�������ľ���!");
            }
            
        }
        
        // ��ǰ��λ��
        Province currentProvince;
        public Province CurrentProvince { get => currentProvince;private set => currentProvince = value; }
        
        // ���ӵ�ǰ��״̬���ƶ�����ֹ��ս����
        private ArmyActionState armyActionState;
        public ArmyActionState ArmyActionState { get => armyActionState; private set => armyActionState = value; }
        

        /*private void Start() {
            //TODO:��start�г�ʼ�������ǲ�����ģ�Ӧ������ armycontroller ���п���
            InitArmy(4, null);
        }*/

        /// <summary>
        /// ��ʼ������ ��ֵ��UI��ʾ
        /// </summary>
        public void InitArmy(ArmyUnit armyUnitData, Province province, int num = 1) {
            ArmyData initArmyData = new ArmyData(armyUnitData);
            InitArmy(initArmyData, province);
        }

        public async void InitArmy(ArmyData initArmyData, Province province) {
            
            //armyModelDetector.InitDetector(this);

            this.ArmyData = initArmyData;

            //TODO: ȷ�����ӵĹ���tag��Ӧ���ڴ�ִ��
            this.ArmyData.ArmyTag = initArmyData.ArmyTag;

            //TODO: ���ݹ���tag���þ���ģ����ʽ
            MeshRenderer render = gameObject.GetComponentInChildren<MeshRenderer>();
            render.material.color = PoliticLoader.Instance.GetFactionColor(this.ArmyData.ArmyTag);

            // �����ӵ�ά��ע�ᵽ�ս��¼���
            TimeSimulator.Instance.SubscribeDayCall(ArmyDayCount);

            Sprite generalSprite = await GeneralProfileHelper.GetProfileAsync(ArmyData.CurrentGeneral.GeneralName);
            SetArmyGeneral(generalSprite);
            //SetArmyGeneral(GeneralProfileHelper.GetProfile(ArmyData.CurrentGeneral.GeneralName));

            SetArmyUnChoosen();

            // �趨����ʡ��
            CurrentProvince = province;
            CurrentProvince.ArmyArriveInProvince(this);

            // �����¾���ui�����ý���ͷ��
            UpdateAmryUI(ArmyData, CurrentProvince.ArmyNumInProvince);

        }

        private void OnDestroy() {
            // ��ȥע����¼�
            TimeSimulator.Instance.UnsubscribeDayCall(ArmyDayCount);
        }

        #region �Ծ��ӵĲ������ƶ����ϲ�����֡�etc

        // ���ӵ�ǰ���ƶ�����
        private ArmyMoveTask currentMoveTask;

        public void SetMoveTask(ArmyMoveTask armyMoveTask, bool IsWithdraw) {
            // ���ڲ����ƶ���״̬
            if (!AbleToMoveArmy()) {
                return;
            }

            //����ԭ�ȵ��ƶ�·��
            HideMovePath();

            //���þ��ӹ�λ
            SetArmyStayIn();

            currentMoveTask = armyMoveTask;

            if (IsWithdraw) {
                //���þ��ӳ���
                SetArmyWithdraw(currentMoveTask.PeekProvince);
            } else {
                //���þ����ƶ�
                SetArmyMoving(currentMoveTask.PeekProvince);
            }

            //����þ��ӱ�ѡ��,����ʾ�ƶ�·��
            if (ArmyController.Instance.IsArmyChoosen(this)) {
                ShowMovePath();
            }

            //ע�ᵽ timesimulator ��Сʱ���¼���
            TimeSimulator.Instance.SubscribeHourCall(HandleMoveArmy);
        }

        private void HandleMoveArmy() {
            // ��ǰû���ƶ�����ȡ��ע��
            if (currentMoveTask == null) {
                TimeSimulator.Instance.UnsubscribeHourCall(HandleMoveArmy);
            }

            currentMoveTask.CountTask();
            if (currentMoveTask != null && currentMoveTask.IsOver) {
                TimeSimulator.Instance.UnsubscribeHourCall(HandleMoveArmy);

                //�ر������ƶ�״̬
                if (ArmyActionState == ArmyActionState.Withdrawing) {
                    ForceArmyStayIn();
                } else {
                    SetArmyStayIn();
                }
            }
        }

        /// <summary>
        /// �ƶ�����λ�õ�ָ����ʡ��
        /// </summary>
        public void MoveArmy(Province province) {
            
            CurrentProvince.RemoveArmyFromProvince(this);

            CurrentProvince = province;

            CurrentProvince.ArmyArriveInProvince(this);

            //���þ��ӵ�λ�õ�ָ����ʡ��
            SetArmyGUIPos(CurrentProvince.transform.position);

            //����Ƿ���ս��
            CheckCombat();
        }

        /// <summary>
        /// ͻ���¼�����Ͼ����ƶ�����ս��etc
        /// </summary>
        public void ForceArmyStayIn() {
            //�жϵ�ǰ�Ƿ����ƶ��������У������֮
            HideMovePath();
            if (currentMoveTask != null) {
                currentMoveTask.ForceToComplete();
                TimeSimulator.Instance.UnsubscribeHourCall(HandleMoveArmy);
                currentMoveTask = null;
            }

            armyActionState = ArmyActionState.IsStaying;

            SetArmyGUIPos(CurrentProvince.transform.position);
        }

        /// <summary>
        /// ��ʾ��ǰ���ƶ�·��
        /// </summary>
        public void ShowMovePath() {
            if (currentMoveTask == null || currentMoveTask.IsOver) {
                return;
            }
            //Debug.Log("path count:" + currentMoveTask.MovePath.Count);
            if (ArmyActionState == ArmyActionState.IsMoving) {
                ProvinceHelper.SetProvinceMoving(currentMoveTask.MovePath);
            } else if (ArmyActionState == ArmyActionState.Withdrawing) {
                ProvinceHelper.SetProvinceWithdrawing(currentMoveTask.MovePath);
            }
        }

        public void HideMovePath() {
            if (currentMoveTask == null || currentMoveTask.IsOver) {
                return;
            }
            //Debug.Log("path count:" + currentMoveTask.MovePath.Count);
            ProvinceHelper.SetProvinceCloseMovePath(currentMoveTask.MovePath);
        }
        
        public bool AbleToMergeArmy() {
            return ArmyActionState != ArmyActionState.IsInCombat && ArmyActionState != ArmyActionState.Withdrawing;
        }

        public void MergeArmy(Army army) {
            army.CurrentProvince.RemoveArmyFromProvince(army);
            ArmyData.AddArmyData(army.ArmyData);
        }

        public bool AbleToSplitArmy() {
            return ArmyData.ArmyUnitDatas.Count > 1;
        }

        public ArmyData SplitArmy() {
            ArmyData splitResult = ArmyData.SplitArmyData();
            UpdateAmryUI(ArmyData, CurrentProvince.ArmyNumInProvince);
            return splitResult;
        }

        /// <summary>
        /// �Ƴ��þ��ӵ�λ
        /// </summary>
        public void RemoveArmy() {
            // �Ƴ�����ǰ����ͣ��
            ForceArmyStayIn();

            CurrentProvince.RemoveArmyFromProvince(this);
            
            // ���������򵥻�ģʽ�в�ͬ�����ٴ���
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if(networkObject == null || !networkObject.IsSpawned) {
                // ����ģʽ ֱ������
                Destroy(this.gameObject);
            } else {
                // ����ģʽ ���г����߿�������
                if (networkObject.IsSpawned && networkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId) {
                    networkObject.Despawn();
                    Destroy(this.gameObject);
                }
            }
        }
        
        #endregion

        #region ����ս�����

        /// <summary>
        /// ���ս���Ƿ���
        /// </summary>
        private void CheckCombat() {
            //CurrentProvince.;

            //TODO�����ս���Ƿ��Ѿ������ˣ�������������ж��Ƿ�Ҫ����֮


            // ��⵱ǰʡ���Ƿ���ڵжԾ���
            List<Army> anemies = CurrentProvince.GetAnemiesInProvince(armyData.ArmyTag);
            if(anemies.Count <= 0) {
                //δ����ս��
                return;
            } else {
                //����ս��

                //1. ʹ��amry�͵��˽���ս��״̬
                foreach (Army anemy in anemies)
                {
                    anemy.EnterCombat();
                }

                //2. ���ʡ���е�������ͬtag��army��λ������ս��
                List<Army> allies = CurrentProvince.GetAlliesInProvince(armyData.ArmyTag);
                foreach (Army ally in allies)
                {
                    ally.EnterCombat();
                }
                
                //3. ʡ�ݴ���ս��
                CurrentProvince.StartCombatEvent(allies, anemies);

                //4. ����ս������ TODO: Ӧ�÷���armyController�У���ѡ���˴���ս��״̬����ʱ ����
                ArmyController.Instance.InvokeCombatPanel(this);
            }
        }

        public void EnterCombat() {
            //ֹͣ�ƶ��������ƶ�����
            if (ArmyActionState == ArmyActionState.IsMoving) {
                SetArmyStayIn();
            } else if (ArmyActionState == ArmyActionState.Withdrawing) {
                // ��Ҫȷ�� �����еľ��Ӳ��ᱻ׷��
                return;
            }

            ArmyActionState = ArmyActionState.IsInCombat;
        }

        public void ExitCombat() {
            ArmyActionState = ArmyActionState.IsStaying;
        }

        #endregion

        #region ����״̬��
        //TODO : ���漰����״̬�仯�ķ���ȫ�����е���Щ�ӿ���

        public bool AbleToMoveArmy() {
            return this.ArmyActionState != ArmyActionState.IsInCombat && this.ArmyActionState != ArmyActionState.Withdrawing;
        }

        /// <summary>
        /// ���������ƶ�״̬���趨�ƶ���Ŀ��
        /// </summary>
        public void SetArmyMoving(Province province) {
            //�������������ڵ�ʡ���ƶ�
            if (province == CurrentProvince) return;

            if (province == null) province = CurrentProvince;

            armyActionState = ArmyActionState.IsMoving;
            //��ģ�Ϳ������������ƶ�һ�������ǰ�λ��ƫ��ָ��ʡ�ݵķ���
            SetArmyGUIMoving(CurrentProvince.transform.position, province.transform.position);
        }

        public bool AbleToStayIn() {
            return this.ArmyActionState != ArmyActionState.IsInCombat;
        }

        /// <summary>
        /// �þ���ֹͣ�ж���ͣ�ڵ�ǰ���ڵ�ʡ��
        /// </summary>
        public void SetArmyStayIn() {
            if (!AbleToStayIn()) {
                return;
            }

            //ǿ���þ��ӽ����ƶ�
            ForceArmyStayIn();
        }

        public bool AbleToWithdraw() {
            //TODO : ʵ�ֳ����ж�
            // ��ʿ����һ��ֵʱ�����Գ���
            return true;
        }

        /// <summary>
        /// �þ��ӳ���
        /// </summary>
        public void SetArmyWithdraw(Province province) {
            //TODO : ����ս�ۣ�����ս��
            if (ArmyActionState == ArmyActionState.IsInCombat) {

            }

            SetArmyMoving(province);
            //Debug.Log("�������ڳ�����!");
            // ���þ����ƶ���������״̬Ϊ���ˣ�Ϊ�ƶ�������
            armyActionState = ArmyActionState.Withdrawing;
        }

        #endregion


        #region ����״̬����: ������ĥ��ʿ���ָ���ά�����á������ָ�

        public ArmySupplyTask CurSupplyTask { get; private set; }

        public void ArmyDayCount() {

            // ÿ�ո��½�����ӵĸ�����ֵ
            CountArmyStatu();

            // 

            // TODO��
        }

        /// <summary>
        /// ��ʱ���Զ��ָ����ӵĸ�����ֵ
        /// </summary>
        public void CountArmyStatu() {
            if(armyActionState == ArmyActionState.IsInCombat) {
                return;
            }

            // TODO: ��provinceData�������һ�� ���ز��� ���ֶΣ�������ΪlocalGrain����Դ
            float localGrain = CurrentProvince.provinceData.GetGrainProduce_Day();
            ArmyData.CountArmyStatu(localGrain);

            // TODO��Ŀǰ��������ģ�
            int armyNumInProvince = 1;
            if (CurrentProvince != null) {
                armyNumInProvince = CurrentProvince.ArmyNumInProvince;
            }
            
            UpdateAmryUI(ArmyData, armyNumInProvince);
        }

        public void RecoverArmy_Immediately(float Morale, float Supply, uint Manpower) {
            ArmyData.RecoverArmy_Immediately(Morale, Supply, Manpower);
        }

        // TODO: ����ⲿ���߼�
        // ��ֻ�����Ѿ��õ������������ƵĽ����������㶮�ģ�
        public void MakeArmyPaied() {

        }

        // TODO: ����ⲿ���߼�
        public void MakeArmyUnpaied() {
            // TODO: ǷǮ������UI�����ı�
        }
        
        /// <summary>
        /// ��ֻ����Ŀǰ�Ƿ��������Ͳ���������
        /// </summary>
        public bool CanGetSupply { get {
            // ����������������ʱ������Ϊ�������䲹��
            // 1.��ǰ��������Ϊ��
            // 2.CurSupplyTask������ GetSupply
            return CurSupplyTask != null && CurSupplyTask.CanGetSupply();
        }}

        /// <summary>
        /// ���뽨��������
        /// </summary>
        public void SetSupplyTask(ArmySupplyTask supplyTask) {
            // TODO: ��Faction���뽨�������������·���㷨balabala
            if (CurSupplyTask != null) {
                CurSupplyTask.ForceToComplete();
            }
            CurSupplyTask = supplyTask;
            TimeSimulator.Instance.SubscribeDayCall(HandleSupplyTask);
        }
        
        public void HandleSupplyTask() {
            if (CurSupplyTask == null) {
                return;
            }

            CurSupplyTask.CountTask();
            //ArmyController.Instance.

            if (CurSupplyTask.IsOver) {
                // ����������� �����ˣ����ؿ���ˢ��

            }
        }

        /// <summary>
        /// ������ǰ���ӵĲ���������, ����������
        /// </summary>
        public void CancelSupplyTask() {
            if (CurSupplyTask != null) {
                CurSupplyTask.ForceToComplete();
            }
            // ȡ��ʱ����¼�
            TimeSimulator.Instance.UnsubscribeDayCall(HandleSupplyTask);
            CurSupplyTask = null;
        }

        public void ShouldRebuildSupplyLine() {
            if(CurSupplyTask == null) {
                return;
            }
            CurSupplyTask.ShouldRebuildSupplyLine();
        }

        #endregion

        #region ����
        public async void ChangeGeneral(General general) {
            // ����ArmyData�е�general
            ArmyData.AssignGeneral(general);
            // ���Ľ���ͷ��
            Sprite sprite = await GeneralProfileHelper.GetProfileAsync(general.GeneralName);
            SetArmyGeneral(sprite);
        }

        #endregion

        /// <summary>
        /// ���¾���UI
        /// </summary>
        public void UpdateArmyUI() {
            // ����ArmyData���µ�����ArmyUnit��״̬����֮
            armyData.ReCaculateStatu();
            UpdateAmryUI(armyData, CurrentProvince.ArmyNumInProvince);
        }

    }


    public enum ArmyActionState {
        IsStaying,
        IsMoving,
        IsInCombat,
        Withdrawing,
        NoOtherState
    }


    /// <summary>
    /// ����Ŀǰ״̬��״̬��
    /// </summary>
    public interface ArmyStateMachine {
        public void EnterCombat();
        public void ExitCombat();

        public bool AbleToStayIn();
        public void SetArmyStayIn();

        public bool AbleToMoveArmy();
        public void SetArmyMoving(Province province);

        public bool AbleToWithdraw();
        public void SetArmyWithdraw(Province province);
    }
}