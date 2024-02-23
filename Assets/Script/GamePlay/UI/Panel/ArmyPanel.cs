using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Audio;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    public class ArmyPanel : BasePopUI {

        [Header("ѡ�еľ��ӵ���Ϣ")]
        [SerializeField] Transform armyItemHolder;
        [SerializeField] GameObject armyItemPrefab;

        [Header("��ť����")]
        [SerializeField] Button deleteButton;
        [SerializeField] Button mergeButton;
        [SerializeField] Button splitButton;
        [SerializeField] Button unchooseButton;

        [Header("�����������")]
        [SerializeField] ArmyDetail armyDetail;

        //�Ƿ��Ѿ������갴ť�¼�
        private bool hasSetButtonEvent = false;

        public void InitArmyPanel(List<Army> choosenArmies) {

            UpdateArmyPanel(choosenArmies);

            if (!hasSetButtonEvent) {
                //TODO����ť�ڰ���˲��ᴥ����ΰ����¼�
                //������Ϊ��ť��ΰ��¼���ֻ�ܰ�һ��

                hasSetButtonEvent = true;
                unchooseButton.onClick.AddListener(delegate {
                    armyDetail.gameObject.SetActive(false);
                    ArmyController.Instance.SetArmyUnchoosen();
                });
                mergeButton.onClick.AddListener(MergeChoosenArmy);
                deleteButton.onClick.AddListener(RemoveArmy_AllChoosen);
                splitButton.onClick.AddListener(SplitChoosenArmy);
            }

        }

        public void UpdateArmyPanel(List<Army> choosenArmies) {
            //��ѡ����һ����λ����չ��ϸ�����
            if (choosenArmies.Count > 1) {
                armyDetail.gameObject.SetActive(false);
            } else {
                armyDetail.gameObject.SetActive(true);
                armyDetail.UpdateArmyDetail(choosenArmies[0], this);
            }

            RemoveArmyItems();

            //����ѡ�еľ��Ӵ���/���� ui ���
            foreach (Army army in choosenArmies) {

                // �Ϸ���
                //GameObject armyObject = Instantiate(armyItemPrefab, armyItemHolder);

                // ʹ�ö����
                GameObject armyObject = ObjectPool.GetInstance().GetObject(armyItemPrefab, armyItemHolder);

                ArmyItem armyItem = armyObject.GetComponent<ArmyItem>();
                armyItem.InitArmyItem(army, this);
            }

            bool ableToMerge = ArmyController.Instance.AbleToMergeArmy(ArmyController.Instance.GetArmyChoosen());
            if (ableToMerge) {
                mergeButton.enabled = true;
            } else {
                mergeButton.enabled = false;
            }
        }


        #region ��ť�¼� ֧����inspector�йҽ� ��armycontroller��ui��
        private void RemoveArmy_AllChoosen() {
            Debug.Log("trigger event happen!");
            //TODO��ɾ������ѡ�еĵ�λ
            // ����ģʽ���Ƴ�����
            ArmyNetworkCtrl.Instance.RemoveArmyEvent_Choosen();
            // ����ģʽ
            //ArmyController.Instance.RemoveArmy_Choosen();

            //�������
            Hide();
        }

        public void RemoveArmy_Single(Army deleteTarget) {
            
            //TODO���ӵ�ǰ��ѡ�о���ɾȥ�������о��ӵ�λ��ɾȥ
            //TODO������Army����Ϸ����
            ArmyController.Instance.RemoveArmy(deleteTarget);

            //ѡ�е������Ѿ�ȫ��ɾ��
            if (armyItemHolder.GetComponentsInChildren<ArmyItem>().Length == 0) {
                Hide();
            }
        }

        public void RemoveArmyItems() {
            armyItemHolder.ClearObjChildren();
        }

        private void MergeChoosenArmy() {
            //Army mergeResult = ArmyController.Instance.MergeArmy_Choosen();
            ArmyNetworkCtrl.Instance.MergeArmyEvent_Choosen();

            AudioManager.PlayAudio(AudioEffectName.MergeArmy);
        }

        private void SplitChoosenArmy() {
            ArmyNetworkCtrl.Instance.SplitArmyEvent_Choosen();
            //ArmyController.Instance.SplitArmy_Choosen();

            AudioManager.PlayAudio(AudioEffectName.SplitArmy);
        }
        
        public void ShowArmyDetail(Army army) {
            
            // ��armycontrollerֻѡ�иõ�λ(ע���ڼ��ر�armypanel)
            ArmyController.Instance.SetArmyChoosen_Single(army);
            ArmyController.Instance.InvokeArmyPanel();

            // ���army panel�����ˣ�����ʾ
            if (Visible)  Show();

            if(!gameObject.activeSelf) gameObject.SetActive(true);

            // չʾdetail���
            armyDetail.gameObject.SetActive(true);
            armyDetail.UpdateArmyDetail(army, this);
        }

        #endregion

    }
}