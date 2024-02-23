using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.ArmyPart {
    // NOTICE: �����о��������������̫�����ˣ��������Ѿ�
    
    /// <summary>
    /// Deprecated�� ���ڼ�����ģ����ײ�����ڵ���ֻ�����ص�ʱ����ui��ʾ��ȷ������
    /// </summary>
    public class ArmyModelDetector : MonoBehaviour {

        // ��ֻ��������
        Army thisArmyData;

        public void InitDetector(Army army) {
            thisArmyData = army;
        }

        // TODO : �ù۲���ģʽ�ع��öδ���

        // TODO : ֻ�м�⵽��������ʱ������ã�

        private void OnTriggerEnter(Collider other) {
            //Debug.Log("collision enter!");
            //����⵽������Army���巢����ײʱ�����ľ��ӵ�ui��ʾ
            ArmyModelDetector detector = other.gameObject.GetComponent<ArmyModelDetector>();
            if(detector != null) {
                Army army = detector.GetComponentInParent<Army>();
                if(army != null) {
                    //Debug.Log("detected army!");
                    UpdateThisArmyUI(army.ArmyData);
                }
            }
        }

        private void OnTriggerExit(Collider other) {
            //��������ײʱ�����¾���ui��ʾ
            ArmyModelDetector detector = other.gameObject.GetComponent<ArmyModelDetector>();
            if (detector != null) {
                Army army = detector.GetComponentInParent<Army>();
                if (army != null) {
                    //Debug.Log("army nomore collision!");

                    //��⵽�������army���岻����ײ������ui
                    UpdateThisArmyUI(army.ArmyData, false);
                }
            }

        }

        private void UpdateThisArmyUI(ArmyData collisionArmyData, bool IsAdd = true) {
            if (IsAdd) {
                //thisArmyData.UpdateArmyUI_Add(collisionArmyData);
            } else {
                //ȥ����armydata
                //thisArmyData.UpdateArmyUI_Reduce(collisionArmyData);
            }
            
        }
    }
}