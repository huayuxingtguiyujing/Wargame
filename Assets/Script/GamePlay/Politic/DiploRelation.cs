using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.Politic {
    [System.Serializable]
    public class DiploRelation {
        [Header("外交关系的主视角")]
        public FactionInfo hostFaction;
        [Header("外交关系的被观测视角")]
        public FactionInfo targetFaction;

        [Range(-100, 100), Space]
        public int relation = 0;

        [Tooltip("是否盟友")]
        public bool IsAllay {  get; set; }

        [Tooltip("是否开战")]
        public bool IsInwar { get; set; }

        [Tooltip("是否是死敌")]
        public bool IsRival { get; set; }

        [Tooltip("是否是邻居")]
        public bool IsNeighbor { get; set; }

        [Tooltip("是否没有任何态度（距离太远、不了解etc）")]
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
            // 死敌直接开战
            IsInwar = isRival;
            IsRival = isRival;
            IsNeighbor = isNeighbor;

            NoAlititude = false;
        }

        /// <summary>
        /// 计算外交关系程度
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