using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.AI {
    public struct AIType {
        public string TypeName;

        // AI ����
        public MilType milType;
        public DipType dipType;
        public AdmType admType;

        /// <summary>
        /// ���²��� AI ����
        /// </summary>
        public enum MilType {
            Attack,     // ������AI
            Balance,    // ƽ����
            Defence     // ������AI
        }

        public enum DipType {
            Peace,          // ��ƽ�ͣ�������������ս��
            venture,        // Ͷ���ͣ�ϲ�����ҷ�Դ�⽻��������ǿ
            Aggressive      // �����ͣ�ϲ����������ս��
        }

        public enum AdmType {
            Hamster,        // �����ͣ�ϲ������Դ�����������
            Balance,        // ƽ����
            Spend           // ϲ����Ǯ������Դ�ͻỨ�����������
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
            return new AIType("����������", MilType.Defence, DipType.Peace, AdmType.Hamster);
        }

        public static AIType GetAmbitiousAIType() {
            return new AIType("Ұ����", MilType.Attack, DipType.Aggressive, AdmType.Spend);
        }

        public static AIType GetAstuteAIType() {
            return new AIType("Ͷ����", MilType.Balance, DipType.venture, AdmType.Balance);
        }

    }

}