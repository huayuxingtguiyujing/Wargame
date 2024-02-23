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

        [Header("��������ϸ��Ϣ")]
        [SerializeField] StatusItem armyItem;
        [SerializeField] StatusItem manpowerItem;
        [SerializeField] StatusItem provinceNumItem;

        [SerializeField] StatusItem anthItem;
        [SerializeField] StatusItem prestigeItem;

        [Header("�⽻��ϵֵ")]
        [SerializeField] TMP_Text yourAtitudeText;
        [SerializeField] TMP_Text hisAtitudeText;

        [Header("�⽻�����ͼ������")]
        [SerializeField] Button dipButton;
        [SerializeField] Button spyButton;

        public override void InitCountryItem(FactionInfo info) {
            base.InitCountryItem(info);

            itemFaction = PoliticLoader.Instance.GetFactionByInfo(info);
            if (itemFaction != null) {

                // ���ؾ�����������ʡ����Ŀ��Ȩ��������
                armyItem.SetStatusItem(itemFaction.GetTotalArmyNum().ToString());
                string totalManpowerStr = (itemFaction.GetTotalManpower() / 1000).ToString() + "k";
                manpowerItem.SetStatusItem(totalManpowerStr);
                provinceNumItem.SetStatusItem(itemFaction.GetProvinceNum().ToString());

                anthItem.SetStatusItem(itemFaction.Resource.Authority.ToString());
                prestigeItem.SetStatusItem(itemFaction.Resource.Prestige.ToString());

                Debug.Log("the detail message is: " + itemFaction.GetTotalArmyNum() + "_" + itemFaction.GetTotalManpower() + "_" + itemFaction.GetProvinceNum());

                // �ر�ѡ��ť
                chooseButton.enabled = false;

                // ����������⽻����
                dipButton.onClick.AddListener(DipButtonEvent);
                spyButton.onClick.AddListener(SpyButtonEvent);

            } else {
                // factionΪ��ʱ ��Ҳû�취��
            }

        }

        public void SetDipRelation(DiploRelation diploRelation) {
            
            // �ҵ�����tag���⽻��ϵ
            DiploRelation[] diploRelationRec = PoliticLoader.Instance.GetDiploRelationByTag(
                diploRelation.hostFaction.FactionTag, 
                diploRelation.targetFaction.FactionTag
            );
            // ���ù�ϵ��ֵ
            SetRelationNum(diploRelationRec[0].relation, ref yourAtitudeText);
            SetRelationNum(diploRelationRec[1].relation, ref hisAtitudeText);
        }


        private void SetRelationNum(int relation, ref TMP_Text setTarget) {
            if(relation > 0) {
                // ��ɫ
                setTarget.color = Color.green;
            } else if(relation < 0) {
                // ��ɫ
                setTarget.color = Color.red;
            } else if(relation == 0) {
                // ��ɫ
                setTarget.color = Color.black;
            }
            setTarget.text = relation.ToString();
        }

        /// <summary>
        /// �⽻��ť�¼�
        /// </summary>
        private void DipButtonEvent() {

            // ���⽻�����в���
        }

        private void SpyButtonEvent() {

            // �򿪼�������в���

        }

    }
}