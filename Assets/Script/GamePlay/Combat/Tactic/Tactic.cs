using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.CombatPart {
    /// <summary>
    /// TODO: ����֮��ս������ÿ����ͬ�Ľ׶�ʹ��
    /// </summary>
    [System.Serializable]
    public class Tactic {

        // ��Ҫ���ĵ� ��֯��
        public int OrganizationCost = 20;

        // �������� ����
        public float armyAttackModify = 0;
        public float armyDefendModify = 0;

        // ������Χ����
        public int armyAttackScopeModify = 0;

        // �Ƽ�����
        public float armyArmorPenetrateModify = 0;

        // ʿ��������������
        public float armyMoraleAttackModify = 0;
        public float armyMoraleDefendModify = 0;
        public float armyMoraleModifyInRear = 0;
        public float armyMoraleModifyToRear = 0;

        // �Եз���������
        public int supplyAttack = 0;
        public int supplyRecover = 0;
    }
}