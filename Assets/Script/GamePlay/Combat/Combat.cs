using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.CombatPart {
    /// <summary>
    /// �����ͼ�����ɵ�ս�����壬����ս��ʱ�ὫArmy���뵽Combat������
    /// </summary>
    public class Combat : MonoBehaviour {
        //NOTICE: Combat�ű����������ս������
        //��ֻ�Ǹ�ս�۱�ʶ���壬���һ��ս���Ѿ�����

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

            // �������ߺͷ����ߵ�λ�÷��뵽��Ӧλ��
            // NOTICE: ����������Ӧ��Ҫ��������ģ�͵�λ�ã��������ﲻ��  + Army.modelFixOffset
            // ���ܻ�����

            // 2024.1.18��������: �������ƶ�״̬�·���ս�ۻᵼ������ƫ��ԭ��λ�ã�
            // ����ԭ������Ϊ�ȵ��õ�InitCombat ����õ�SetArmyMoving(�þ��Ӵ����ƶ�״̬)
            // ����״̬�£��ͻ��˵ľ�������λ�û���ƫ�ƣ�Ӧ��Ҳ������ԭ��
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
        /// ���ô˺�������ʾս���еļ�Ա��Ϣ�����ص���Ϸ������
        /// </summary>
        public void UpdateCombatMessage() {
            // TODO��

            //
        }

        /// <summary>
        /// ����ս�ۣ�ʤ����һ��ռ��ʡ�ݣ�ʧ��һ������
        /// </summary>
        public void CancelCombat(bool AttackersWin) {
            //TODO���ж�˭��ʤ��һ�������߿��ԴӺ����Ĳ������룩

            /*if (AttackersWin) {
                foreach (Army army in attackers)
                {
                    army.MoveArmy(combatProvince);
                }
            } else {

                //ʧ����ͨͨɱ����
                foreach (Army army in defenders)
                {
                    ArmyController.Instance.RemoveArmy(army);
                }
            }*/

            //TODO������ʤ��һ����λ�õ�ʡ����

            //TODO����ʧ�ܵ�һ�����ˣ������в�����ֹ�����߽��������������߼���Army��д


            Destroy(gameObject);
        }


    }
}