using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.Services.Lobbies.Models;
//using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.Application.TimeTask;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Infrastructure.UIOrganizedMess;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    public class CombatPanel : BasePopUI {

        // current handle army
        List<Army> currentHandleArmy;
        // where combat happen
        Province combatProvince;

        CombatSituationSprite SituationSprites;

        [SerializeField] Button closeButton;
        [SerializeField] DetailTextPop detailTextPop;

        [Header("˫������")]
        [SerializeField] GeneralHolder attackerHolder;
        [SerializeField] GeneralHolder defenderHolder;

        [Header("˫������״̬")]
        [SerializeField] SliderBar_Type2 attackerMoraleBar;
        [SerializeField] SliderBar_Type2 attackerSupplyBar;
        [SerializeField] SliderBar_Type2 defenderMoraleBar;
        [SerializeField] SliderBar_Type2 defenderSupplyBar;

        [Header("˫������")]
        [SerializeField] TextItem attackerFrontNum;
        [SerializeField] TextItem attackerRearNum;
        [SerializeField] TextItem attackerWithdrawNum;
        [SerializeField] TextItem attackerTotalNum;

        [SerializeField] TextItem defenderFrontNum;
        [SerializeField] TextItem defenderRearNum;
        [SerializeField] TextItem defenderWithdrawNum;
        [SerializeField] TextItem defenderTotalNum;

        [Header("��ǰս�۽׶�")]// �м�Ŀ���
        [SerializeField] TMP_Text StageText;

        [Header("˫���ɲ��õ�ս��")]// ˫�����·�����

        [Header("˫����ǰ�ӳ�")]
        [SerializeField] Transform attackerCurBuffHolder;
        [SerializeField] Transform defenderCurBuffHolder;
        [SerializeField] GameObject CombatSituationPrefab;

        private List<CombatSituationItem> attackerSituationItems;
        private List<CombatSituationItem> defenderSituationItems;
        /// <summary>
        /// �üӳ�UIͼ���Ƿ����
        /// </summary>
        /// <param name="situationGuid"></param>
        /// <returns></returns>
        private CombatSituationItem FindAttackerSituation(string situationGuid) {
            
            CombatSituationItem situationItem = attackerSituationItems.Find(item => { 
                return item.SituationGuid == situationGuid; 
            });
            return situationItem;
        }
        private CombatSituationItem FindDefenderSituation(string situationGuid) {
            CombatSituationItem situationItem = defenderSituationItems.Find(item => { 
                return item.SituationGuid == situationGuid; 
            });
            return situationItem;
        }



        public void InitCombatPanel(List<Army> army, CombatTask combatTask, Province province) {
           
            SituationSprites = GetComponentInChildren<CombatSituationSprite>();
            attackerSituationItems = new List<CombatSituationItem>();
            defenderSituationItems = new List<CombatSituationItem>();

            detailTextPop.Hide();

            attackerCurBuffHolder.ClearObjChildren();
            defenderCurBuffHolder.ClearObjChildren();

            currentHandleArmy = army;

            closeButton.onClick.AddListener(Hide);

            // Ϊʡ�ݹ�����combatpanel
            LinkPanelToProvince(province);

            UpdateCombatPanel(combatTask);

            Show();

            // TODO���ṩ�ı����ս���Ĺ���

        }

        /// <summary>
        /// ����ˢ��ս���е�˫����ֵ
        /// </summary>
        public void UpdateCombatPanel(CombatTask combatTask) {
            if (!Visible) {
                return;
            }

            if(combatTask == null) {
                return;
            }

            if (combatTask.IsOver) {
                Hide();
            }
            
            // ˢ��˫����ǰ������
            attackerFrontNum.InitTextItem(null, combatTask.GetFrontArmyNum(true).ToString(), colorType:4);
            attackerRearNum.InitTextItem(null, combatTask.GetRearArmyNum(true).ToString(), colorType: 4);
            attackerWithdrawNum.InitTextItem(null, combatTask.GetWithdrawArmyNum(true).ToString(), colorType: 4);
            attackerTotalNum.InitTextItem(null, combatTask.GetTotalArmyNum(true).ToString(), colorType: 4);

            defenderFrontNum.InitTextItem(null, combatTask.GetFrontArmyNum(false).ToString(), colorType: 4);
            defenderRearNum.InitTextItem(null, combatTask.GetRearArmyNum(false).ToString(), colorType: 4);
            defenderWithdrawNum.InitTextItem(null, combatTask.GetWithdrawArmyNum(false).ToString(), colorType: 4);
            defenderTotalNum.InitTextItem(null, combatTask.GetTotalArmyNum(false).ToString(), colorType: 4);

            // �趨˫����ǰ�Ľ���
            attackerHolder.InitGeneralHolder(combatTask.AttackerLeader);
            defenderHolder.InitGeneralHolder(combatTask.DefenderLeader);

            // �趨˫�����ӵ�ʿ��
            combatTask.GetArmyMorale(true, out float maxMorale_Attacker, out float morale_Attacker);
            attackerMoraleBar.InitSliderBar(maxMorale_Attacker, morale_Attacker);

            combatTask.GetArmyMorale(false, out float maxMorale_Defender, out float morale_Defender);
            defenderMoraleBar.InitSliderBar(maxMorale_Defender, morale_Defender);

            // TODO��չʾ��ǰ��ս�۽׶Σ�˫����ս��
            UpdateGeneralStageModify(combatTask.CombatStage.stageName, combatTask.AttackRandomAdd, combatTask.DefendRandomAdd, combatTask.CurAttackerGeneralModify, combatTask.CurDefenderGeneralModify);

        }


        #region �����ʾ�ļӳ�ͼ����

        /// <summary>
        /// ����ս�۵� ��ǰ�׶εĽ���ӳ�
        /// </summary>
        private void UpdateGeneralStageModify(CombatStageEnum combatStage, float attackerRandomModify, float defenderRandomModify, UnitModify attackerModify, UnitModify defenderModify) {
            StageText.text = CombatStage.GetStageChineseName(combatStage);

            // ����˫��������ӳ�
            DetailText attackItem = new DetailText("���Ҫ��", attackerRandomModify);
            SetFloatSituationItem(attackItem, true);
            DetailText defendItem = new DetailText("���Ҫ��", defenderRandomModify);
            SetFloatSituationItem(defendItem, false);

            // ���ϸ��µ�ǰս�۽׶εļӳ�
            foreach (var Detail in attackerModify.ModifyMessage.Details)
            {
                // TODO: ǿ�ҽ�������������Message���֣�Infrastructure�ļ����
                //if (MesPair.Value.itemValue is int) {
                    //SetIntSituationItem(MesPair.Value);
                //} else if(MesPair.Value.itemValue is float) {
                    SetFloatSituationItem(Detail, isAttacker: true);
                //}
            }

            foreach (var Detail in defenderModify.ModifyMessage.Details) {
                SetFloatSituationItem(Detail, isAttacker: false);
            }
        }

        private void SetFloatSituationItem(DetailText Detail, bool isAttacker) {
            bool valueNotZero = (Detail.Value != 0);
            // NOTICE: Detail.Description ��ΪΨһ��ʶ��
            Sprite itemSprite = SituationSprites.GetSpriteByModifyName(Detail.Description, Detail.Value);
            // �����ֶ��趨��Ӧ��ͼƬ

            SetSituationItem(valueNotZero, Detail.Description, Detail.Value, isAttacker, itemSprite);
        }

        /// <summary>
        /// �趨 ���Ӽӳɵ�UIͼ��
        /// </summary>
        /// <param name="valueNotZero">�üӳ�ֵ�Ƿ�Ϊ0</param>
        /// <param name="situationName">�üӳ�ֵ������</param>
        /// <param name="situationDesc">�üӳ�ֵ��������</param>
        private void SetSituationItem(bool valueNotZero, string situationName, float situationValue, bool isAttacker, Sprite itemSprite) {
            CombatSituationItem rec;
            Transform itemParent;
            string situationDesc = situationValue.ToString();
            // �����Ƿ��ǽ��������ҵ����ʵ�SituationItem
            if (isAttacker) {
                rec = FindAttackerSituation(situationName);
                itemParent = attackerCurBuffHolder;
            } else {
                rec = FindDefenderSituation(situationName);
                itemParent = defenderCurBuffHolder;
            }

            if (valueNotZero && rec && !rec.isActive) {
                // �ӳɲ�Ϊ0��ui����ڣ���δ����򼤻���
                rec.gameObject.SetActive(true);
            } else if (valueNotZero && rec == null) {
                // �ӳɲ�Ϊ0��ui����ڣ�����ʹ�����,�����뵽holder��
                GameObject situationObject = Instantiate(CombatSituationPrefab, itemParent);
                CombatSituationItem situationItem = situationObject.GetComponent<CombatSituationItem>();

                // ����ͼ�굯������Ϣ
                DetailMessage detailMessage = new DetailMessage();
                detailMessage.SetStart(situationName, situationValue, false, false, 4, 4);
                situationItem.InitCombatSituationItem(detailTextPop, detailMessage, situationName, situationDesc, itemSprite);
                if (isAttacker) {
                    attackerSituationItems.Add(situationItem);
                } else {
                    defenderSituationItems.Add(situationItem);
                }
            } else if (!valueNotZero && rec != null && rec.isActive) {
                // �ӳ�Ϊ0��ui����ڣ�������
                rec.gameObject.SetActive(false);
            } else if (rec != null) {
                // �������������֮
                rec.SetCombatSituationItem(situationName, situationDesc);
            }
        }

        #endregion

        /// <summary>
        /// ��ʡ����������������
        /// </summary>
        private void LinkPanelToProvince(Province province = null) {
            if (combatProvince != null) {
                combatProvince.UnLinkToCombatPanel();
            }
            combatProvince = province;

            // �����ʡ��Ϊ��ʱ����������
            if(combatProvince != null) {
                combatProvince.LinkToCombatPanel(UpdateCombatPanel);
            }
        }

        public override void Hide() {
            base.Hide();

            //����ʱ��Ҫˢ�� ���صĶ�Ӧʡ��
            LinkPanelToProvince();
        }

    }
}