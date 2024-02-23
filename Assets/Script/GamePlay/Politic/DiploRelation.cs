using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.Politic {
    [System.Serializable]
    public class DiploRelation {
        [Header("�⽻��ϵ�����ӽ�")]
        public FactionInfo hostFaction;
        [Header("�⽻��ϵ�ı��۲��ӽ�")]
        public FactionInfo targetFaction;

        [Range(-100, 100), Space]
        public int relation = 0;

        [Tooltip("�Ƿ�����")]
        public bool IsAllay {  get; set; }

        [Tooltip("�Ƿ�ս")]
        public bool IsInwar { get; set; }

        [Tooltip("�Ƿ�������")]
        public bool IsRival { get; set; }

        [Tooltip("�Ƿ����ھ�")]
        public bool IsNeighbor { get; set; }

        [Tooltip("�Ƿ�û���κ�̬�ȣ�����̫Զ�����˽�etc��")]
        public bool NoAlititude {  get; set; }

        public DiploRelation() {
            NoAlititude = false;

            hostFaction = null;
            targetFaction = null;

            relation = 0;

            IsAllay = false;
            IsInwar = false;
            IsRival = false;
            IsNeighbor = false;
        }

        public DiploRelation(FactionInfo hostFaction, FactionInfo targetFaction, 
            bool isAllay, bool isInwar, bool isRival, bool isNeighbor) {
            //if (hostFaction == targetFaction) { }

            this.hostFaction = hostFaction;
            this.targetFaction = targetFaction;

            IsAllay = isAllay;
            // ����ֱ�ӿ�ս
            IsInwar = isRival;
            IsRival = isRival;
            IsNeighbor = isNeighbor;

            NoAlititude = false;
        }

        /// <summary>
        /// �����⽻��ϵ�̶�
        /// </summary>
        private void RecaculateRelation() {
            if (IsRival) {
                relation -= 100;
            }

            if (IsInwar) {
                relation -= 50;
            }

            if (IsAllay){
                relation += 50;
            }
        }
    }
}