using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.BT {
    public abstract class BaseNode {

        public string Name;

        public List<BaseNode> Children { get; protected set; }

        // ִ��֮�����ͣ���
        public float Interval;

        // �ڵ��Ƿ����
        public bool IsActive;

        // �ڵ��Ƿ�������
        public bool IsRunning;

        protected BaseNode(string name) {
            Name = name;
            Children = new List<BaseNode>();
        }


        /// <summary>
        /// ִ�иýڵ���߼�
        /// </summary>
        public virtual NodeResult Excute() {
            return NodeResult.Failed;
        }

        /// <summary>
        /// ���ѽڵ�
        /// </summary>
        public virtual void Active() {
            IsActive = true;
            foreach (var Node in Children)
            {
                Node.Active();
            }
        }

        public virtual void AddChildren(BaseNode node) {
            if (Children == null) {
                Children = new List<BaseNode>();
            }

            if (!Children.Contains(node)) {
                Children.Add(node);
            }
        }

        public virtual void RemoveChildren(BaseNode node) {
            if (Children == null) {
                Children = new List<BaseNode>();
            }

            if (Children.Contains(node)) {
                Children.Remove(node);
            }
        }

        public virtual void ClearChildren() {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].ClearChildren();
            }
            Children.Clear();
        }

    }

    public enum NodeResult {
        Success,
        Failed
    }
}