using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.GamePlay.UI {
    public class CountryItem_Dip : CountryItem_Normal {

        Faction itemFaction;

        [Header("该势力详细信息")]
        [SerializeField] StatusItem armyItem;
        [SerializeField] StatusItem manpowerItem;
        [SerializeField] StatusItem provinceNumItem;

        [SerializeField] StatusItem anthItem;
        [SerializeField] StatusItem prestigeItem;

        [Header("外交关系值")]
        [SerializeField] TMP_Text yourAtitudeText;
        [SerializeField] TMP_Text hisAtitudeText;

        [Header("外交操作和间谍操作")]
        [SerializeField] Button dipButton;
        [SerializeField] Button spyButton;

        public override void InitCountryItem(FactionInfo info) {
            base.InitCountryItem(info);

            itemFaction = PoliticLoader.Instance.GetFactionByInfo(info);
            if (itemFaction != null) {

                // 加载军力、人力、省份数目、权威、威望
                armyItem.SetStatusItem(itemFaction.GetTotalArmyNum().ToString());
                string totalManpowerStr = (itemFaction.GetTotalManpower() / 1000).ToString() + "k";
                manpowerItem.SetStatusItem(totalManpowerStr);
                provinceNumItem.SetStatusItem(itemFaction.GetProvinceNum().ToString());

                anthItem.SetStatusItem(itemFaction.Resource.Authority.ToString());
                prestigeItem.SetStatusItem(itemFaction.Resource.Prestige.ToString());

                Debug.Log("the detail message is: " + itemFaction.GetTotalArmyNum() + "_" + itemFaction.GetTotalManpower() + "_" + itemFaction.GetProvinceNum());

                // 关闭选择按钮
                chooseButton.enabled = false;

                // 间谍操作和外交操作
                dipButton.onClick.AddListener(DipButtonEvent);
                spyButton.onClick.AddListener(SpyButtonEvent);

            } else {
                // faction为空时 那也没办法咯
            }

        }

        public void SetDipRelation(DiploRelation diploRelation) {
            
            // 找到两个tag的外交关系
            DiploRelation[] diploRelationRec = PoliticLoader.Instance.GetDiploRelationByTag(
                diploRelation.hostFaction.FactionTag, 
                diploRelation.targetFaction.FactionTag
            );
            // 设置关系数值
            SetRelationNum(diploRelationRec[0].relation, ref yourAtitudeText);
            SetRelationNum(diploRelationRec[1].relation, ref hisAtitudeText);
        }


        private void SetRelationNum(int relation, ref TMP_Text setTarget) {
            if(relation > 0) {
                // 绿色
                setTarget.color = Color.green;
            } else if(relation < 0) {
                // 红色
                setTarget.color = Color.red;
            } else if(relation == 0) {
                // 白色
                setTarget.color = Color.black;
            }
            setTarget.text = relation.ToString();
        }

        /// <summary>
        /// 外交按钮事件
        /// </summary>
        private void DipButtonEvent() {

            // 打开外交面板进行操作
        }

        private void SpyButtonEvent() {

            // 打开间谍面板进行操作

        }

    }
}