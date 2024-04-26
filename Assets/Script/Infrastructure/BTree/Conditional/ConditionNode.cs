using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WarGame_True.Infrastructure.BT {
    /// <summary>
    /// ConditionNode�������ڵ㣬ֱ�Ӽ̳� SequenceNode
    /// </summary>
    public class ConditionNode : SequenceNode {
        public delegate bool ConditionDele();

        ConditionDele conditionAction;

        public ConditionNode(string name, ConditionDele conditionAction) : base(name){
            this.conditionAction = conditionAction;
        }

        public override NodeResult Excute() {
            if (conditionAction != null) {
                // ִ�д������������
                bool result = conditionAction.Invoke();
                if (result) {
                    // ����ִ��,��ִ�������ӽڵ㣨ConditionNode��֧�� �����ӽڵ㣩
                    base.Excute();
                    return NodeResult.Success;
                } else {
                    return NodeResult.Failed;
                }
            } else {
                return NodeResult.Success;
            }
        }

    }
}