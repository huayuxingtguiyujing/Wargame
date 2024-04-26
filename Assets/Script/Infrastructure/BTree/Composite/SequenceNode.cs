using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace WarGame_True.Infrastructure.BT {
    /// <summary>
    /// Sequence逻辑节点，每次执行，有序地执行各个子结点,当一个子结点结束后才执行下一个
    /// 严格按照节点A、B、C的顺序执行，当最后的行为C结束后，BTSequence结束。
    /// </summary>
    public class SequenceNode : BaseNode {

        private BaseNode activeNode;

        public SequenceNode(string name) : base(name) {
            
        }

        public override NodeResult Excute() {
            NodeResult result = NodeResult.Success;
            NodeResult rec;

            for (int i = 0; i < Children.Count; i++) {
                activeNode = Children[i];
                rec = Children[i].Excute();

                // 若有一个节点执行失败 则结果失败
                if (rec == NodeResult.Failed) {
                    result = rec;
                }
            }

            return result;
        }

    }
}