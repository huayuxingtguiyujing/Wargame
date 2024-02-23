using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.GamePlay.ArmyPart {
    /// <summary>
    /// ���������ui��ʾ��ui����������룩
    /// </summary>
    public class ArmyGUI : MonoBehaviour {

        #region ���� ui��gameobject�ı���
        [SerializeField] protected ArmyModelDetector armyModelDetector;

        [Header("�������UI")]
        [SerializeField] Image leaderProfile;
        [SerializeField] SliderBar armyMorale;
        [SerializeField] SliderBar armySupply;
        [SerializeField] TMP_Text armyNumText;

        [Header("��ʡ���ϵ����о���")]
        [SerializeField] TMP_Text armyNumInProvinceText;

        [Header("����ģ���������")]
        [SerializeField] SpriteRenderer circleSprite;

        #endregion

        #region ���������״̬
        protected void SetArmyGUIPos(Vector3 provincePos) {
            transform.position = provincePos;
        }

        protected void SetArmyGUIMoving(Vector3 origin, Vector3 target) {
            transform.position = origin + (target - origin) / 4;
        }

        #endregion

        #region ����Army UI
        /// <summary>
        /// ֱ�����þ��ӵ�UI, ���� ���졢����ʿ���������̶�etc, ���ݵ���ArmyData����
        /// </summary>
        protected void UpdateAmryUI(ArmyData armyData, int armyNumInProvince) {
            armyMorale.UpdateSliderBar(armyData.MaxMorale, armyData.CurMorale, true);
            // ���þ��ӵ�����,����ʹ����slider�е����,��ֹӰ��,���Ա�����ں���
            armyNumText.text = armyData.ArmyNum.ToString();
            armySupply.UpdateSliderBar(armyData.MaxSupply, armyData.CurSupply);
            //Debug.Log(armyData.armyNum.ToString());

            // ��ǰʡ���ϵľ�������
            armyNumInProvinceText.text = "x" + armyNumInProvince.ToString();

            // ��ǰ���ӵ�ʿ���׶�

        }

        /*      ����ArmyModelDetector�Ĵ���,��ɾ
                /// <summary>
                /// ֱ�����þ���ui�����ݶ��ArmyData����ui�������ڵ����������ͬһʡ��ʱ��
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
                /// ����ui���Լ�ȥһ��armydata�ķ�ʽ
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
        /// [Deprecated] ֱ��ͨ���������ֵ �ı���ӵ�ui
        /// </summary>
        public virtual void UpdateArmyUI(uint damageNum, float moraleDamage) {
            // ����Army��ʵ�ָú���
        }

        /// <summary>
        /// ���õ�λ��ѡ��
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