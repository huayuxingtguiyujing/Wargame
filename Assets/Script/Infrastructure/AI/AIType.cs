using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.AI {
    public struct AIType {
        public string TypeName;

        // AI 类型
        public MilType milType;
        public DipType dipType;
        public AdmType admType;

        /// <summary>
        /// 军事策略 AI 类型
        /// </summary>
        public enum MilType {
            Attack,     // 进攻型AI
            Balance,    // 平衡性
            Defence     // 防御型AI
        }

        public enum DipType {
            Peace,          // 和平型，不会主动发起战争
            venture,        // 投机型，喜欢左右逢源外交，欺弱媚强
            Aggressive      // 侵略型，喜欢主动发起战争
        }

        public enum AdmType {
            Hamster,        // 仓鼠型，喜欢存资源，更在意赤字
            Balance,        // 平衡性
            Spend           // 喜欢烧钱，有资源就会花，不在意赤字
        }

        public AIType(string typeName, MilType milType, DipType dipType, AdmType admType) {
            TypeName = typeName;
            this.milType = milType;
            this.dipType = dipType;
            this.admType = admType;
        }

        public int GetMilWeight(int de, int ba, int at) {
            switch (milType) {
                case AIType.MilType.Defence:
                    return de;
                case AIType.MilType.Balance:
                    return ba;
                case AIType.MilType.Attack:
                    return at;
            }
            return 0;
        }

        public int GetAdmWeight(int ha, int ba, int sp) {
            switch (admType) {
                case AIType.AdmType.Hamster:
                    return ha;
                case AIType.AdmType.Balance:
                    return ba;
                case AIType.AdmType.Spend:
                    return sp;
            }
            return 0;
        }

        public static AIType GetNormalAIType() {
            return new AIType("保境安民型", MilType.Defence, DipType.Peace, AdmType.Hamster);
        }

        public static AIType GetAmbitiousAIType() {
            return new AIType("野心型", MilType.Attack, DipType.Aggressive, AdmType.Spend);
        }

        public static AIType GetAstuteAIType() {
            return new AIType("投机型", MilType.Balance, DipType.venture, AdmType.Balance);
        }

    }

}