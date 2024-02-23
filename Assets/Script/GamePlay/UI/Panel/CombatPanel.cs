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

        [Header("双方将领")]
        [SerializeField] GeneralHolder attackerHolder;
        [SerializeField] GeneralHolder defenderHolder;

        [Header("双方军队状态")]
        [SerializeField] SliderBar_Type2 attackerMoraleBar;
        [SerializeField] SliderBar_Type2 attackerSupplyBar;
        [SerializeField] SliderBar_Type2 defenderMoraleBar;
        [SerializeField] SliderBar_Type2 defenderSupplyBar;

        [Header("双方人数")]
        [SerializeField] TextItem attackerFrontNum;
        [SerializeField] TextItem attackerRearNum;
        [SerializeField] TextItem attackerWithdrawNum;
        [SerializeField] TextItem attackerTotalNum;

        [SerializeField] TextItem defenderFrontNum;
        [SerializeField] TextItem defenderRearNum;
        [SerializeField] TextItem defenderWithdrawNum;
        [SerializeField] TextItem defenderTotalNum;

        [Header("当前战役阶段")]// 中间的框体
        [SerializeField] TMP_Text StageText;

        [Header("双方可采用的战术")]// 双方的下方框体

        [Header("双方当前加成")]
        [SerializeField] Transform attackerCurBuffHolder;
        [SerializeField] Transform defenderCurBuffHolder;
        [SerializeField] GameObject CombatSituationPrefab;

        private List<CombatSituationItem> attackerSituationItems;
        private List<CombatSituationItem> defenderSituationItems;
        /// <summary>
        /// 该加成UI图标是否存在
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

            // 为省份挂载上combatpanel
            LinkPanelToProvince(province);

            UpdateCombatPanel(combatTask);

            Show();

            // TODO：提供改变军队战术的功能

        }

        /// <summary>
        /// 不断刷新战役中的双方数值
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
            
            // 刷新双方当前的人数
            attackerFrontNum.InitTextItem(null, combatTask.GetFrontArmyNum(true).ToString(), colorType:4);
            attackerRearNum.InitTextItem(null, combatTask.GetRearArmyNum(true).ToString(), colorType: 4);
            attackerWithdrawNum.InitTextItem(null, combatTask.GetWithdrawArmyNum(true).ToString(), colorType: 4);
            attackerTotalNum.InitTextItem(null, combatTask.GetTotalArmyNum(true).ToString(), colorType: 4);

            defenderFrontNum.InitTextItem(null, combatTask.GetFrontArmyNum(false).ToString(), colorType: 4);
            defenderRearNum.InitTextItem(null, combatTask.GetRearArmyNum(false).ToString(), colorType: 4);
            defenderWithdrawNum.InitTextItem(null, combatTask.GetWithdrawArmyNum(false).ToString(), colorType: 4);
            defenderTotalNum.InitTextItem(null, combatTask.GetTotalArmyNum(false).ToString(), colorType: 4);

            // 设定双方当前的将领
            attackerHolder.InitGeneralHolder(combatTask.AttackerLeader);
            defenderHolder.InitGeneralHolder(combatTask.DefenderLeader);

            // 设定双方军队的士气
            combatTask.GetArmyMorale(true, out float maxMorale_Attacker, out float morale_Attacker);
            attackerMoraleBar.InitSliderBar(maxMorale_Attacker, morale_Attacker);

            combatTask.GetArmyMorale(false, out float maxMorale_Defender, out float morale_Defender);
            defenderMoraleBar.InitSliderBar(maxMorale_Defender, morale_Defender);

            // TODO：展示当前的战役阶段，双方的战术
            UpdateGeneralStageModify(combatTask.CombatStage.stageName, combatTask.AttackRandomAdd, combatTask.DefendRandomAdd, combatTask.CurAttackerGeneralModify, combatTask.CurDefenderGeneralModify);

        }


        #region 面板显示的加成图标项

        /// <summary>
        /// 更新战役的 当前阶段的将领加成
        /// </summary>
        private void UpdateGeneralStageModify(CombatStageEnum combatStage, float attackerRandomModify, float defenderRandomModify, UnitModify attackerModify, UnitModify defenderModify) {
            StageText.text = CombatStage.GetStageChineseName(combatStage);

            // 更新双方的随机加成
            DetailText attackItem = new DetailText("随机要素", attackerRandomModify);
            SetFloatSituationItem(attackItem, true);
            DetailText defendItem = new DetailText("随机要素", defenderRandomModify);
            SetFloatSituationItem(defendItem, false);

            // 不断更新当前战役阶段的加成
            foreach (var Detail in attackerModify.ModifyMessage.Details)
            {
                // TODO: 强烈建议你完善完善Message部分（Infrastructure文件夹里）
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
            // NOTICE: Detail.Description 作为唯一标识符
            Sprite itemSprite = SituationSprites.GetSpriteByModifyName(Detail.Description, Detail.Value);
            // 根据字段设定对应的图片

            SetSituationItem(valueNotZero, Detail.Description, Detail.Value, isAttacker, itemSprite);
        }

        /// <summary>
        /// 设定 军队加成的UI图像
        /// </summary>
        /// <param name="valueNotZero">该加成值是否为0</param>
        /// <param name="situationName">该加成值的名称</param>
        /// <param name="situationDesc">该加成值具体描述</param>
        private void SetSituationItem(bool valueNotZero, string situationName, float situationValue, bool isAttacker, Sprite itemSprite) {
            CombatSituationItem rec;
            Transform itemParent;
            string situationDesc = situationValue.ToString();
            // 根据是否是进攻方，找到合适的SituationItem
            if (isAttacker) {
                rec = FindAttackerSituation(situationName);
                itemParent = attackerCurBuffHolder;
            } else {
                rec = FindDefenderSituation(situationName);
                itemParent = defenderCurBuffHolder;
            }

            if (valueNotZero && rec && !rec.isActive) {
                // 加成不为0，ui项存在，且未激活，则激活它
                rec.gameObject.SetActive(true);
            } else if (valueNotZero && rec == null) {
                // 加成不为0，ui项不存在，否则就创建它,并加入到holder中
                GameObject situationObject = Instantiate(CombatSituationPrefab, itemParent);
                CombatSituationItem situationItem = situationObject.GetComponent<CombatSituationItem>();

                // 设置图标弹出框信息
                DetailMessage detailMessage = new DetailMessage();
                detailMessage.SetStart(situationName, situationValue, false, false, 4, 4);
                situationItem.InitCombatSituationItem(detailTextPop, detailMessage, situationName, situationDesc, itemSprite);
                if (isAttacker) {
                    attackerSituationItems.Add(situationItem);
                } else {
                    defenderSituationItems.Add(situationItem);
                }
            } else if (!valueNotZero && rec != null && rec.isActive) {
                // 加成为0，ui项存在，隐藏它
                rec.gameObject.SetActive(false);
            } else if (rec != null) {
                // 其余情况，更新之
                rec.SetCombatSituationItem(situationName, situationDesc);
            }
        }

        #endregion

        /// <summary>
        /// 将省份与该面板连接起来
        /// </summary>
        private void LinkPanelToProvince(Province province = null) {
            if (combatProvince != null) {
                combatProvince.UnLinkToCombatPanel();
            }
            combatProvince = province;

            // 传入的省份为空时，等于重置
            if(combatProvince != null) {
                combatProvince.LinkToCombatPanel(UpdateCombatPanel);
            }
        }

        public override void Hide() {
            base.Hide();

            //隐藏时需要刷新 挂载的对应省份
            LinkPanelToProvince();
        }

    }
}