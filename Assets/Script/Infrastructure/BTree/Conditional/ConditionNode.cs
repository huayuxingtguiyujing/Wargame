using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WarGame_True.Infrastructure.BT {
    /// <summary>
    /// ConditionNode，条件节点，直接继承 SequenceNode
    /// </summary>
    public class ConditionNode : SequenceNode {
        public delegate bool ConditionDele();

        ConditionDele conditionAction;

        public ConditionNode(string name, ConditionDele conditionAction) : base(name){
            this.conditionAction = conditionAction;
        }

        public override NodeResult Excute() {
            if (conditionAction != null) {
                // 执行传入的条件函数
                bool result = conditionAction.Invoke();
                if (result) {
                    // 可以执行,则执行它的子节点（ConditionNode仅支持 单个子节点）
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