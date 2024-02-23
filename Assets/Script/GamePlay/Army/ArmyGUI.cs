using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// 军队物体的ui显示（ui与数据相分离）
    /// </summary>
    public class ArmyGUI : MonoBehaviour {

        #region 调整 ui、gameobject的变量
        [SerializeField] protected ArmyModelDetector armyModelDetector;

        [Header("军队相关UI")]
        [SerializeField] Image leaderProfile;
        [SerializeField] SliderBar armyMorale;
        [SerializeField] SliderBar armySupply;
        [SerializeField] TMP_Text armyNumText;

        [Header("该省份上的所有军队")]
        [SerializeField] TMP_Text armyNumInProvinceText;

        [Header("调整模型所需变量")]
        [SerializeField] SpriteRenderer circleSprite;

        #endregion

        #region 军队物体的状态
        protected void SetArmyGUIPos(Vector3 provincePos) {
            transform.position = provincePos;
        }

        protected void SetArmyGUIMoving(Vector3 origin, Vector3 target) {
            transform.position = origin + (target - origin) / 4;
        }

        #endregion

        #region 管理Army UI
        /// <summary>
        /// 直接设置军队的UI, 更新 将领、更新士气、补给程度etc, 根据单个ArmyData更新
        /// </summary>
        protected void UpdateAmryUI(ArmyData armyData, int armyNumInProvince) {
            armyMorale.UpdateSliderBar(armyData.MaxMorale, armyData.CurMorale, true);
            // 设置军队的人数,由于使用了slider中的组件,防止影响,所以必须放在后面
            armyNumText.text = armyData.ArmyNum.ToString();
            armySupply.UpdateSliderBar(armyData.MaxSupply, armyData.CurSupply);
            //Debug.Log(armyData.armyNum.ToString());

            // 当前省份上的军队人数
            armyNumInProvinceText.text = "x" + armyNumInProvince.ToString();

            // 当前军队的士气阶段

        }

        /*      适配ArmyModelDetector的代码,勿删
                /// <summary>
                /// 直接设置军队ui，根据多个ArmyData设置ui（适用于当多个军队在同一省份时）
                /// </summary>
                public void InitArmyUI(ArmyData[] armyDatas) {
                    overlapCount = 0;

                    overlapNum = 0;
                    overlapMorale = 0;
                    overlapMaxMorale = 0;

                    foreach (ArmyData armyData in armyDatas) {
                        overlapCount++;

                        overlapNum += armyData.armyNum;
                        overlapMorale += armyData.morale;
                        overlapMaxMorale += armyData.maxMorale;
                    }

                    UpdateArmyUI();
                }


                public void UpdateArmyUI_Add(ArmyData armyData) {

                    overlapCount++;

                    overlapNum += armyData.armyNum;
                    overlapMorale += armyData.morale;
                    overlapMaxMorale += armyData.maxMorale;

                    UpdateArmyUI();
                }

                /// <summary>
                /// 更新ui，以减去一个armydata的方式
                /// </summary>
                /// <param name="armyData"></param>
                public void UpdateArmyUI_Reduce(ArmyData armyData) {
                    overlapCount--;

                    overlapNum -= armyData.armyNum;
                    overlapMorale -= armyData.morale;
                    overlapMaxMorale -= armyData.maxMorale;

                    UpdateArmyUI();
                }
        */

        /// <summary>
        /// [Deprecated] 直接通过传入的数值 改变军队的ui
        /// </summary>
        public virtual void UpdateArmyUI(uint damageNum, float moraleDamage) {
            // 请在Army中实现该函数
        }

        /// <summary>
        /// 设置单位被选中
        /// </summary>
        public void SetArmyChoosen() {
            circleSprite.gameObject.SetActive(true);
        }

        public void SetArmyUnChoosen() {
            circleSprite.gameObject.SetActive(false);
        }

        #endregion

        public void SetArmyGeneral(Sprite generalSprite) {
            leaderProfile.sprite = generalSprite;
        }

    }
}