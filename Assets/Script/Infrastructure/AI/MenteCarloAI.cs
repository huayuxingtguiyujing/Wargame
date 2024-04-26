using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarGame_True.Infrastructure.AI {


    /// <summary>
    /// 搜索树的节点
    /// </summary>
    public class Node {
        public int wins = 0;        // 赢了的次数
        public int visit = 0;       // 模拟的总次数

        public List<Node> children;
        public Node parent;

        public int nodeValue;

        public Node() { }

        public Node(int wins, int visit, List<Node> children, Node parent) {
            this.wins = wins;
            this.visit = visit;
            this.children = children;
            this.parent = parent;
        }


        public Node SelectNode() {
            // 排序原则: UCT函数
            // Q(child) / N(child) + c * Sqrt(Sqrt(N) / N(child))
            return children.OrderByDescending((child) => {
                return child.wins / child.visit + Mathf.Sqrt(Mathf.Sqrt(visit) / child.visit);
            }).First();
        }

        public virtual void Simulation() {
            // 如果节点不是完全扩展，则创建子节点，并进行模拟
            // 如果节点完全扩展，则随机选择一个子节点进行模拟

            // 
            // 
        }

        public virtual void UpdateStats(int result) {
            visit++;
            wins += result;
        }

        public virtual bool IsExpanded() {
            return false;
        }

        public virtual void Expand() { }

    }


    /// <summary>
    /// 蒙特卡洛搜索树类 MCTS学习
    /// </summary>
    public class MenteCarloTree {

        public Node root;

        public void MainLogic() {
            //root = new Node();
            for (int i = 0; i < 1000; i++) {
                Node selectedNode = SelectNode(root);
                int simulatedResult = Simulate(selectedNode);
                BackPropagate(selectedNode, simulatedResult);
            }
        }

        #region MCTS的基本步骤

        public Node SelectNode(Node node) {
            while (node.children.Any()) {
                node = node.SelectNode();
            }
            return node;
        }

        public int Simulate(Node node) {
            // 对游戏进行模拟
            // TODO: 
            node.Simulation();

            return (UnityEngine.Random.Range(0, 2) == 0) ? 1 : 0;
        }

        public void BackPropagate(Node node, int result) {
            // 反向传播 更新父节点
            while (node != null) {
                node.UpdateStats(result);
                node = node.parent;
            }
        }

        #endregion

    }

    /// /////////////////////////////// /////////////////////////////////////////////
    // 以下是Fingergame部分
    // Fingergame的游戏，双方每轮出拳，比出一个1~5的数字，每个数字只能用一次
    // 大则胜加1分，小则输扣1分，平局不得分，最后比较双方谁的分数更多

    public class FingersNode : Node {

        private int curScore;
        private List<int> lastNum = new List<int>() { 1, 2, 3, 4, 5};
        private List<int> totalNum = new List<int>() { 1, 2, 3, 4, 5};

        private List<int> record = new List<int>();

        public FingersNode() { }
        
        public FingersNode(int wins, int visit, List<Node> children, Node parent) : base(wins, visit, children, parent) {
            
        }


        public override void Simulation() {
            // 如果节点不是完全扩展，则创建子节点，并进行模拟
            // 如果节点完全扩展，则随机选择一个子节点进行模拟

            // 
            // 
        }

        public override void UpdateStats(int result) {
            base.UpdateStats(result);
            
        }

        public override bool IsExpanded() {
            return children.Count >= 5;
        }

        public override void Expand() { 
            // 随机挑选一个可选的
        }

    }

    public class FingersGame {

        public void MainLogic() { 
            
        }


    }

}