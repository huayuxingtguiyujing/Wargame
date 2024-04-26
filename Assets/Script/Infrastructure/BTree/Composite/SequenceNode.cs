using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace WarGame_True.Infrastructure.BT {
    /// <summary>
    /// Sequence�߼��ڵ㣬ÿ��ִ�У������ִ�и����ӽ��,��һ���ӽ��������ִ����һ��
    /// �ϸ��սڵ�A��B��C��˳��ִ�У���������ΪC������BTSequence������
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

                // ����һ���ڵ�ִ��ʧ�� ����ʧ��
                if (rec == NodeResult.Failed) {
                    result = rec;
                }
            }

            return result;
        }

    }
}