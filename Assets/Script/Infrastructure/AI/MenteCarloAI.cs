using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarGame_True.Infrastructure.AI {


    /// <summary>
    /// �������Ľڵ�
    /// </summary>
    public class Node {
        public int wins = 0;        // Ӯ�˵Ĵ���
        public int visit = 0;       // ģ����ܴ���

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
            // ����ԭ��: UCT����
            // Q(child) / N(child) + c * Sqrt(Sqrt(N) / N(child))
            return children.OrderByDescending((child) => {
                return child.wins / child.visit + Mathf.Sqrt(Mathf.Sqrt(visit) / child.visit);
            }).First();
        }

        public virtual void Simulation() {
            // ����ڵ㲻����ȫ��չ���򴴽��ӽڵ㣬������ģ��
            // ����ڵ���ȫ��չ�������ѡ��һ���ӽڵ����ģ��

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
    /// ���ؿ����������� MCTSѧϰ
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

        #region MCTS�Ļ�������

        public Node SelectNode(Node node) {
            while (node.children.Any()) {
                node = node.SelectNode();
            }
            return node;
        }

        public int Simulate(Node node) {
            // ����Ϸ����ģ��
            // TODO: 
            node.Simulation();

            return (UnityEngine.Random.Range(0, 2) == 0) ? 1 : 0;
        }

        public void BackPropagate(Node node, int result) {
            // ���򴫲� ���¸��ڵ�
            while (node != null) {
                node.UpdateStats(result);
                node = node.parent;
            }
        }

        #endregion

    }

    /// /////////////////////////////// /////////////////////////////////////////////
    // ������Fingergame����
    // Fingergame����Ϸ��˫��ÿ�ֳ�ȭ���ȳ�һ��1~5�����֣�ÿ������ֻ����һ��
    // ����ʤ��1�֣�С�����1�֣�ƽ�ֲ��÷֣����Ƚ�˫��˭�ķ�������

    public class FingersNode : Node {

        private int curScore;
        private List<int> lastNum = new List<int>() { 1, 2, 3, 4, 5};
        private List<int> totalNum = new List<int>() { 1, 2, 3, 4, 5};

        private List<int> record = new List<int>();

        public FingersNode() { }
        
        public FingersNode(int wins, int visit, List<Node> children, Node parent) : base(wins, visit, children, parent) {
            
        }


        public override void Simulation() {
            // ����ڵ㲻����ȫ��չ���򴴽��ӽڵ㣬������ģ��
            // ����ڵ���ȫ��չ�������ѡ��һ���ӽڵ����ģ��

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
            // �����ѡһ����ѡ��
        }

    }

    public class FingersGame {

        public void MainLogic() { 
            
        }


    }

}