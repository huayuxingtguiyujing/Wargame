using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.CombatPart {
    /// <summary>
    /// 管理地图上生成的战役物体，发生战役时会将Army挂入到Combat物体下
    /// </summary>
    public class Combat : MonoBehaviour {
        //NOTICE: Combat脚本本身不会控制战役事务，
        //它只是个战役标识物体，标记一场战役已经发生

        [SerializeField] Transform attackerTransform;
        [SerializeField] Transform defenderTransform;

        public List<Army> attackers;
        public List<Army> defenders;

        private Province combatProvince;

        private bool hasInited = false;

        public void InitCombat(List<Army> attackers, List<Army> defenders, Province province) {
            if (!hasInited) {
                hasInited = true;
            }

            combatProvince = province;

            this.attackers = attackers;
            this.defenders = defenders;

            // 将攻击者和防御者的位置放入到对应位置
            // NOTICE: 按理来讲，应该要修正军队模型的位置，但是这里不修  + Army.modelFixOffset
            // 可能会埋雷

            // 2024.1.18发现问题: 军队在移动状态下发生战役会导致物体偏移原定位置，
            // 究其原因，是因为先调用的InitCombat 后调用的SetArmyMoving(让军队处于移动状态)
            // 联网状态下，客户端的军队物体位置会有偏移，应该也是类似原因
            UpdateArmyPos();

        }

        public void UpdateArmyPos() {
            foreach (Army army in attackers) {
                army.gameObject.transform.position = attackerTransform.position;
            }

            foreach (Army army in defenders) {
                army.gameObject.transform.position = defenderTransform.position;
            }
        }

        /// <summary>
        /// 调用此函数，显示战役中的减员信息，加载到游戏物体上
        /// </summary>
        public void UpdateCombatMessage() {
            // TODO：

            //
        }

        /// <summary>
        /// 结束战役，胜利的一方占据省份，失败一方撤退
        /// </summary>
        public void CancelCombat(bool AttackersWin) {
            //TODO：判断谁是胜利一方（或者可以从函数的参数传入）

            /*if (AttackersWin) {
                foreach (Army army in attackers)
                {
                    army.MoveArmy(combatProvince);
                }
            } else {

                //失败者通通杀掉！
                foreach (Army army in defenders)
                {
                    ArmyController.Instance.RemoveArmy(army);
                }
            }*/

            //TODO：设置胜利一方的位置到省份上

            //TODO：让失败的一方溃退，溃退中不可中止、或者进行其他操作，逻辑到Army中写


            Destroy(gameObject);
        }


    }
}