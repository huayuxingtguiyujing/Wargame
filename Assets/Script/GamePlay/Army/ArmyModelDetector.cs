using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.ArmyPart {
    // NOTICE: 经过研究决定，这个方法太复杂了，放弃了已经
    
    /// <summary>
    /// Deprecated！ 用于检测军队模型碰撞，用于当多只军队重叠时，让ui显示正确的数字
    /// </summary>
    public class ArmyModelDetector : MonoBehaviour {

        // 该只军队自身
        Army thisArmyData;

        public void InitDetector(Army army) {
            thisArmyData = army;
        }

        // TODO : 用观察者模式重构该段代码

        // TODO : 只有检测到己方军队时，会调用！

        private void OnTriggerEnter(Collider other) {
            //Debug.Log("collision enter!");
            //当检测到与其他Army物体发生碰撞时，更改军队的ui显示
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
            //当不再碰撞时，更新军队ui显示
            ArmyModelDetector detector = other.gameObject.GetComponent<ArmyModelDetector>();
            if (detector != null) {
                Army army = detector.GetComponentInParent<Army>();
                if (army != null) {
                    //Debug.Log("army nomore collision!");

                    //检测到与另外的army物体不再碰撞，更新ui
                    UpdateThisArmyUI(army.ArmyData, false);
                }
            }

        }

        private void UpdateThisArmyUI(ArmyData collisionArmyData, bool IsAdd = true) {
            if (IsAdd) {
                //thisArmyData.UpdateArmyUI_Add(collisionArmyData);
            } else {
                //去除该armydata
                //thisArmyData.UpdateArmyUI_Reduce(collisionArmyData);
            }
            
        }
    }
}