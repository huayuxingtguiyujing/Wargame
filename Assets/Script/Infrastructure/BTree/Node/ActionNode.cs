using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WarGame_True.Infrastructure.BT {
    public class ActionNode : BaseNode {

        UnityAction action;

        public ActionNode(string name, UnityAction action):base(name) {
            this.action = action;
        }

        public override NodeResult Excute() {
            
            try {
                action?.Invoke();
                return NodeResult.Success;
            } catch {
                return NodeResult.Failed;
            }
        }

        public override void AddChildren(BaseNode node) {
            Debug.LogError("Action node can not add child");
        }

        public override void RemoveChildren(BaseNode node) {
            Debug.LogError("Action node can not remove child");
        }

    }
}