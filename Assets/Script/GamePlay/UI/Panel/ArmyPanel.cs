using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Audio;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    public class ArmyPanel : BasePopUI {

        [Header("选中的军队的信息")]
        [SerializeField] Transform armyItemHolder;
        [SerializeField] GameObject armyItemPrefab;

        [Header("按钮功能")]
        [SerializeField] Button deleteButton;
        [SerializeField] Button mergeButton;
        [SerializeField] Button splitButton;
        [SerializeField] Button unchooseButton;

        [Header("军队详情面板")]
        [SerializeField] ArmyDetail armyDetail;

        //是否已经配置完按钮事件
        private bool hasSetButtonEvent = false;

        public void InitArmyPanel(List<Army> choosenArmies) {

            UpdateArmyPanel(choosenArmies);

            if (!hasSetButtonEvent) {
                //TODO：按钮在按下瞬间会触发多次按下事件
                //这是因为按钮多次绑定事件，只能绑定一次

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
            //仅选中了一个单位，则展开细节面板
            if (choosenArmies.Count > 1) {
                armyDetail.gameObject.SetActive(false);
            } else {
                armyDetail.gameObject.SetActive(true);
                armyDetail.UpdateArmyDetail(choosenArmies[0], this);
            }

            RemoveArmyItems();

            //根据选中的军队创建/更新 ui 面板
            foreach (Army army in choosenArmies) {

                // 老方法
                //GameObject armyObject = Instantiate(armyItemPrefab, armyItemHolder);

                // 使用对象池
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


        #region 按钮事件 支持在inspector中挂接 是armycontroller的ui层
        private void RemoveArmy_AllChoosen() {
            Debug.Log("trigger event happen!");
            //TODO：删除所有选中的单位
            // 联网模式下移除军队
            ArmyNetworkCtrl.Instance.RemoveArmyEvent_Choosen();
            // 单机模式
            //ArmyController.Instance.RemoveArmy_Choosen();

            //隐藏面板
            Hide();
        }

        public void RemoveArmy_Single(Army deleteTarget) {
            
            //TODO：从当前的选中军队删去，从所有军队单位中删去
            //TODO：销毁Army的游戏物体
            ArmyController.Instance.RemoveArmy(deleteTarget);

            //选中的物体已经全部删完
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
            
            // 让armycontroller只选中该单位(注意期间会关闭armypanel)
            ArmyController.Instance.SetArmyChoosen_Single(army);
            ArmyController.Instance.InvokeArmyPanel();

            // 如果army panel隐藏了，则显示
            if (Visible)  Show();

            if(!gameObject.activeSelf) gameObject.SetActive(true);

            // 展示detail面板
            armyDetail.gameObject.SetActive(true);
            armyDetail.UpdateArmyDetail(army, this);
        }

        #endregion

    }
}