using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using WarGame_True.GamePlay.Application.TimeTask;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.Audio;
using WarGame_True.Infrastructure.Map.Controller;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Infrastructure.Map.Util;
using WarGame_True.States;

namespace WarGame_True.GamePlay.ArmyPart {

    public class ArmyController : MonoBehaviour {

        [Header("������")]
        public Province withdrawTarget;

        [Space]
        public static ArmyController Instance;

        [SerializeField] ArmyNetworkCtrl armyNetworkCtrl;

        [Header("����UI���")]
        [SerializeField] MapController mapController;
        [SerializeField] ArmyPanel armyPanel;
        [SerializeField] CombatPanel combatPanel;

        [Header("��ѡ��ʽ")]
        public Material drawMat;

        [Header("����prefab")]
        public GameObject armyPrefab;
        public Transform armyTransform;

        [Header("���еľ��ӵ�λ")]
        public List<Army> allArmyObject;
        
        public List<Army> currentChooseArmy;

        [Header("Ĭ�Ϲ��ص�armyunit")]
        [SerializeField] ArmyUnitData InfantryArmyUnitData;

        #region ��ѡ ���� ����
        [Header("���ƿ�ѡ")]
        private Dictionary<GameObject, Vector3> armyWorldPositionDic;
        private Dictionary<GameObject, Vector2> armyScreenPositionDic;

        private Vector2 mouseStartPosition = new Vector2(0, 0);
        private Vector2 mouseEndPosition = new Vector2(0, 0);
        private Vector2 mousePosition = new Vector2(0, 0);

        private bool mouseDown = false;

        #endregion

        #region �ƶ����� �������
        RaycastHit hit;
        [Header("���ӿ��ƶ�������")]
        [SerializeField] LayerMask clickableLayer;
        [SerializeField] int maxDetectDistance = 100;
        #endregion

        void Start() {
            if (Instance == null) Instance = this;

            //armyPanel.InitArmyPanel(currentChooseArmy, this);
            armyPanel.Hide();
            combatPanel.Hide();

            //��ʼ��
            currentChooseArmy = new List<Army>();
            armyWorldPositionDic = new Dictionary<GameObject, Vector3>();
            armyScreenPositionDic = new Dictionary<GameObject, Vector2>();

            //TODO�������������� army ���� allArmyObject ����
            //TODO����allarmyObject���еĵ�λ�������ݽṹ��

            //NOTICE��HideFlags����wtf��
            drawMat.hideFlags = HideFlags.HideAndDontSave;
            drawMat.shader.hideFlags = HideFlags.HideAndDontSave;
        }

        void Update() {
            //����ѡ�߼�
            MouseRectChooseArmy();
            //���ʡ�ݽ����ƶ�
            ClickToMoveArmy();
        }

        void OnGUI() {
            if (mouseDown) {
                //���ƿ��ӵ�ѡ��
                DrawRect();
            }
        }

        #region ����ѡ
        private void MouseRectChooseArmy() {
            mousePosition = Input.mousePosition;

            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                //����������
                mouseDown = true;
                mouseStartPosition = Input.mousePosition;

            }

            if (Input.GetKeyUp(KeyCode.Mouse0)) {
                //�ɿ�������
                mouseEndPosition = Input.mousePosition;
                mouseDown = false;

                //�ж�ѡ������
                GetArmyInRect();
            }

            //ÿ�θ����������Ļλ��
            for (int i = 0; i < allArmyObject.Count; i++) {

                armyWorldPositionDic[allArmyObject[i].gameObject] = allArmyObject[i].transform.position;

                armyScreenPositionDic[allArmyObject[i].gameObject] = Camera.main.WorldToScreenPoint(
                    armyWorldPositionDic[allArmyObject[i].gameObject]
                );
            }
        }

        private void GetArmyInRect() {
            //NOTICE����Ϊд���߼������⣬�κε�����ᴥ����ѡ����˼���������

            bool cancleChoosenFlag = true;

            bool playChooseAudio = false;

            if (Vector2.Distance(mouseStartPosition, mouseEndPosition) <= 3.0f) {
                // ����̫�������ᴥ�����������߼�
                return;
            }

            if (mouseStartPosition != mouseEndPosition) {
                for (int i = 0; i < allArmyObject.Count; i++) {
                    //Vector2 position = armyObjectScreenPosition[i];
                    Vector2 position = armyScreenPositionDic[allArmyObject[i].gameObject];
                    Vector2 start = mouseStartPosition;
                    Vector2 end = mouseEndPosition;
                    if ((position.x > start.x && position.x < end.x && position.y > start.y && position.y < end.y) ||
                        (position.x > start.x && position.x < end.x && position.y < start.y && position.y > end.y) ||
                        (position.x < start.x && position.x > end.x && position.y > start.y && position.y < end.y) ||
                        (position.x < start.x && position.x > end.x && position.y < start.y && position.y > end.y)) {

                        //��ѡ���˾���ʱ��ȡ����һ֧���ӵ�ѡ��
                        if (cancleChoosenFlag) {
                            SetArmyUnchoosen();
                            cancleChoosenFlag = false;
                        }
                        // ѡ���˾��� �Ქ��ѡ����Ƶ
                        playChooseAudio = true;
                        //�ڿ�ѡ�ķ�Χ��
                        currentChooseArmy.Add(allArmyObject[i]);
                    }
                }
            } else {
                // NOTICE : �ⲿ��û���ã�����Ҫɾ
                //��ѡ����
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(
                    mouseStartPosition.x,
                    mouseStartPosition.y,
                    Camera.main.farClipPlane
                ));
                Ray ray = new Ray(
                    Camera.main.transform.position,
                    mouseWorldPosition - Camera.main.transform.position
                );
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    if (hit.transform.tag == "army") {
                        currentChooseArmy.Add(hit.transform.gameObject.GetComponent<Army>());
                    }
                }

            }

            if (playChooseAudio) {
                // TODO: ����ѡ�о�����Ƶ
                AudioManager.PlayAudio(AudioEffectName.ChooseArmy);
            }

            SetArmyChoosen();

            //TODO:ѡ����������ս�ĵ�λ��չ����ս��壨ֻչ������һ֧���ӣ�
            CheckToInvokeCombatPanel();

            //TODO:չ�����Ҹ��� ����״��ui
            InvokeArmyPanel();
        }

        public void GetArmyByClick(Army army) {
            SetArmyUnchoosen();
            currentChooseArmy.Add(army);
            SetArmyChoosen();
            InvokeArmyPanel();
        }

        private void DrawRect() {
            drawMat.SetPass(0);

            //����������任����
            GL.PushMatrix();
            //��������Ļ�����ͼ  
            GL.LoadPixelMatrix();

            Color clr = Color.black;

            GL.Begin(GL.LINES);
            GL.Color(clr);
            GL.Vertex3(mouseStartPosition.x, mouseStartPosition.y, 0);
            GL.Vertex3(mousePosition.x, mouseStartPosition.y, 0);
            GL.End();

            //��
            GL.Begin(GL.LINES);
            GL.Color(clr);
            GL.Vertex3(mouseStartPosition.x, mousePosition.y, 0);
            GL.Vertex3(mousePosition.x, mousePosition.y, 0);
            GL.End();

            //��
            GL.Begin(GL.LINES);
            GL.Color(clr);
            GL.Vertex3(mouseStartPosition.x, mouseStartPosition.y, 0);
            GL.Vertex3(mouseStartPosition.x, mousePosition.y, 0);
            GL.End();

            //��
            GL.Begin(GL.LINES);
            GL.Color(clr);
            GL.Vertex3(mousePosition.x, mouseStartPosition.y, 0);
            GL.Vertex3(mousePosition.x, mousePosition.y, 0);
            GL.End();

            GL.PopMatrix();//��ԭ  
        }

        #endregion

        #region ���������١��ϲ���ѡ�С�ȡ��ѡ�о���
        // �������ʼ������
        /// <summary>
        ///  ���������ӵ�object
        /// </summary>
        /// <param name="province"></param>
        /// <returns></returns>
        public Army CreateArmyObj(Province province) {
            //GameObject armyObject = Instantiate(armyPrefab, armyTransform);
            GameObject armyObject = Instantiate(armyPrefab);        // no parent
            Army army = armyObject.GetComponent<Army>();
            armyObject.transform.position = province.transform.position;
            return army;
        }

        // TODO: ����Ƿ����������ͬ������
        public void InitArmy(ArmyUnit armyUnitData, Province province, Army army, int unitNum = 1) {
            ArmyData initArmyData = new ArmyData(armyUnitData);
            // ���ݵ�λ��ļ��ʡ�ݴ���ArmyData
            initArmyData.ArmyTag = province.provinceData.OwnerTag;
            //  return 
            InitArmy(initArmyData, province, army);
        }

        public void InitArmy(ArmyData armyData, Province province, Army army) {
            //Debug.Log("create army! num: " + province.provinceData.provinceEco.prosperity.ToString() + ", province name:" + province.provincePosition);

            // -------------------------
            //��ʼ�������� army ��prefab
            //GameObject armyObject = Instantiate(armyPrefab, armyTransform);
            //Army army = armyObject.GetComponent<Army>();
            //armyObject.transform.position = province.transform.position;
            
            GameObject armyObject = army.gameObject;
            army.InitArmy(armyData, province);
            //--------------------------

            // ��ӵ����Ҽ�¼��
            GamePlayState.GetFaction(army.ArmyData.ArmyTag).AddNewArmy(army);
            allArmyObject.Add(army);

            //��ӵ�����ӳ���ֵ���
            if (!armyWorldPositionDic.ContainsKey(armyObject)) {
                armyWorldPositionDic.Add(armyObject, armyObject.transform.position);
            } else {
                armyWorldPositionDic[armyObject] = armyObject.transform.position;
            }

            if (!armyScreenPositionDic.ContainsKey(armyObject)) {
                armyScreenPositionDic.Add(armyObject, Vector3.zero);
            } else {
                armyScreenPositionDic[armyObject] = Vector3.zero;
            }

            //NetworkObject networkObject = armyObject.GetComponent<NetworkObject>();
            //Debug.Log("�ɹ������˾���: " + networkObject.GetHashCode());
            //return army;
        }

        // TODO: ��Ӧ����ͬ��!
        public void CreateArmy_Random() {

            if (mapController.IDProvinceDic == null) {
                Debug.LogWarning("no province!");
                return;
            } else {
                Debug.Log("current province num:" + mapController.IDProvinceDic.Count);
            }

            //������������Ŀ �����ʡ��
            System.Random random = new System.Random();
            int armyUnitNum = random.Next(5);

            //TODO:������ʡ�������⣬��̶��ڳ�ʼʡ������
            Province province = default;
            int randomIndex = random.Next(mapController.IDProvinceDic.Count);
            //Debug.Log(randomIndex);
            province = mapController.IDProvinceDic.Values.ToArray()[randomIndex];

            if (province != null && province != default) {
                // TODO: �ǵ�д�꣡
                //CreateArmy(new ArmyUnit(InfantryArmyUnitData), province);
            } else {
                Debug.LogWarning("create fail!");
            }

        }
        
        // �Ƴ�����
        // TODO: ��Ӧ����ͬ��!
        public void RemoveArmies(List<Army> armies) {
            foreach (Army army in armies)
            {
                RemoveArmy(army);
            }
        }

        public void RemoveArmy(Army targetArmy) {
            //ɾ����Ӧ�ľ��ӵ�λ
            if (allArmyObject.Contains(targetArmy)) {
                GamePlayState.GetFaction(targetArmy.ArmyData.ArmyTag).RemoveArmy(targetArmy);
                allArmyObject.Remove(targetArmy);
            }

            if (currentChooseArmy.Contains(targetArmy)) {
                GamePlayState.GetFaction(targetArmy.ArmyData.ArmyTag).RemoveArmy(targetArmy);
                currentChooseArmy.Remove(targetArmy);
            }

            targetArmy.RemoveArmy();
        }

        public void RemoveArmy_Choosen() {
            foreach (Army armyObject in currentChooseArmy) {
                if (allArmyObject.Contains(armyObject)) {
                    allArmyObject.Remove(armyObject);
                    // �Ӷ�Ӧ���������Ƴ�
                    GamePlayState.GetFaction(armyObject.ArmyData.ArmyTag).RemoveArmy(armyObject);
                    armyObject.RemoveArmy();
                }
            }
            currentChooseArmy.Clear();
        }

        public void RemoveArmy_All() {
            foreach (Army armyObject in allArmyObject) {
                // �Ӷ�Ӧ���������Ƴ�
                GamePlayState.GetFaction(armyObject.ArmyData.ArmyTag).RemoveArmy(armyObject);
                armyObject.RemoveArmy();
            }
            allArmyObject.Clear();
            currentChooseArmy.Clear();
        }

        // TODO: ��Ӧ����ͬ��!
        public Army MergeArmy_Choosen() {
            
            Army mergeHost = MergeArmy(null, currentChooseArmy);

            SetArmyUnchoosen();
            currentChooseArmy = new List<Army>(){ mergeHost};
            SetArmyChoosen();

            InvokeArmyPanel();

            return mergeHost;

        }

        public Army MergeArmy(Army mergeHost, List<Army> armies) {
            if(armies == null || armies.Count <= 0) {
                Debug.LogError("û��ѡ��Ҫ�ϲ��ľ���");
                return null;
            }
            
            if(mergeHost == null) {
                // ���û�д���mergeHost��Ĭ��mergeHostΪ�б�����һ��
                mergeHost = armies[0];
            }

            if (!armies.Contains(mergeHost)) {
                Debug.LogError("�ϲ�Ŀ����Ӳ���ѡ�о�����");
                return null;
            }

            if (AbleToMergeArmy(armies)) {
                //�ϲ�ǰ ֹͣ�����ƶ�, ���ϲ�Ŀ����б��г�ȥ
                ArmyHelper.GetInstance().SetArmyStayIn(armies);
                armies.Remove(mergeHost);

                int currentCount = armies.Count;
                for (int i = currentCount - 1; i >= 0; i--) {
                    //�����е�λ�����뵽����һ����λ��
                    mergeHost.MergeArmy(armies[i]);
                    //����������λ
                    RemoveArmy(armies[i]);

                }

                mergeHost.UpdateArmyUI();

                InvokeArmyPanel();

                return mergeHost;
            }

            return null;
        }

        /// <summary>
        /// �жϵ�ǰѡ�еľ��� �Ƿ���Ժϲ�
        /// </summary>
        public bool AbleToMergeArmy(List<Army> mergetTarget) {

            if (mergetTarget.Count <= 1) {
                return false;
            }

            bool ans = true;
            string provinceId = mergetTarget[0].CurrentProvince.provincePos;
            foreach (Army army in mergetTarget) {
                //�ж��Ƿ�ͬ��һ��ʡ��
                ans = ans && provinceId.Equals(army.CurrentProvince.provincePos);

                //�ж��Ƿ���ͬһ��������ͬʱѡ�� ��Ȼ��ͬһ����,Ӧ�ðɣ�

                //�жϾ��ӱ����Ƿ�����ϲ�
                ans = ans && army.AbleToMergeArmy();

                if (!ans) return false;
            }
            return true;
        }

        // TODO: ��Ӧ����ͬ��!
        public void SplitArmy_Choosen() {
            
            // TODO�����ò�ֺ�ľ��ӵĽ���
            /*int count = currentChooseArmy.Count;
            for (int i = 0; i < count; i++) {
                if (AbleToSplitArmy(currentChooseArmy[i])) {
                    //TODO : !!!�����޲�֣����ֹͣ������
                    //TODO�� �ư��ˣ��ǰ�ť����ʱ�ᴥ����β���¼���������ť������
                    Army army = CreateArmyObj(currentChooseArmy[i].CurrentProvince);
                    InitArmy(
                        currentChooseArmy[i].SplitArmy(),
                        currentChooseArmy[i].CurrentProvince, army
                    );
                    splitResults.Add(army);
                } else {
                    //Debug.Log("this army can not be split!");
                }

                splitResults.Add(currentChooseArmy[i]);
            }*/

            List<Army> splitResults = SplitArmy(currentChooseArmy);

            //Debug.Log(splitResults.Count);

            //���ò�ֺ�ľ���Ϊ�µ�ѡ��
            SetArmyUnchoosen();
            currentChooseArmy = splitResults;
            SetArmyChoosen();
            ArmyHelper.GetInstance().SetArmyStayIn(currentChooseArmy);

            //ˢ�����
            InvokeArmyPanel();
        }

        private List<Army> SplitArmy(List<Army> armies) {
            List<Army> splitResults = new List<Army>();

            // TODO�����ò�ֺ�ľ��ӵĽ���
            int count = armies.Count;
            for (int i = 0; i < count; i++) {
                if (AbleToSplitArmy(armies[i])) {
                    //TODO : !!!�����޲�֣����ֹͣ������
                    //TODO�� �ư��ˣ��ǰ�ť����ʱ�ᴥ����β���¼���������ť������
                    Army army = CreateArmyObj(armies[i].CurrentProvince);
                    InitArmy(
                        armies[i].SplitArmy(),
                        armies[i].CurrentProvince, army
                    );
                    splitResults.Add(army);
                } else {
                    Debug.LogError("this army can not be split!");
                }

                splitResults.Add(armies[i]);
            }
            return splitResults;
        }

        public bool AbleToSplitArmy(Army army) {
            return army.AbleToSplitArmy();
        }

        public List<Army> GetArmyChoosen() {
            return currentChooseArmy;
        }

        public bool IsArmyChoosen(Army army) {
            foreach (Army curChooseArmy in currentChooseArmy)
            {
                if(curChooseArmy == army) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ���þ��ӵ�λΪѡ�е�״̬
        /// </summary>
        public void SetArmyChoosen() {
            bool onlyOpenOneCombat = true;

            foreach (Army armyObject in currentChooseArmy) {

                //�������ս��״̬����ô��ս����壨����һ������ս���еĵ�λ��
                if (onlyOpenOneCombat) {
                    if (armyObject.ArmyActionState == ArmyActionState.IsInCombat) {
                        onlyOpenOneCombat = false;

                        //TODO : ��ս�����
                    }
                }

                armyObject.SetArmyChoosen();
                armyObject.ShowMovePath();
            }
        }

        public void SetArmyChoosen_Single(Army army) {
            // ��յ�ǰ��ѡ�е�λ
            SetArmyUnchoosen();

            // ѡ�иõ�λ
            currentChooseArmy.Add(army);
            army.SetArmyChoosen();
            army.ShowMovePath();
        }

        public void SetArmyUnchoosen() {
            foreach (Army armyObject in currentChooseArmy) {
                armyObject.SetArmyUnChoosen();
                armyObject.HideMovePath();
            }

            currentChooseArmy.Clear();
            //�رվ������ui
            armyPanel.Hide();
        }
        #endregion

        #region �ƶ�����
        private void ClickToMoveArmy() {
            //δѡ�о��� �򷵻�
            if (currentChooseArmy.Count <= 0) {
                return;
            }

            //NOTICE�����߼����ܼ�⵽3D���壡Ҫ����3D��collider
            if (Input.GetMouseButtonDown(1)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //���߼�� �Ƿ���ʡ������
                if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                    Province destination = hit.collider.gameObject.GetComponent<Province>();
                    if (destination != null) {
                        //Debug.Log(hit.collider.gameObject.name);
                        SetArmyDestination(destination, currentChooseArmy, false);

                        // TODO: �����ƶ�������Ƶ
                        AudioManager.PlayAudio(AudioEffectName.ClickMoveArmy);
                    }
                }
            }
        }

        public void SetArmyDestination(Province destination, List<Army> setTargets, bool IsWithdraw) {

            /*// ����ģʽ��ԭ���߼�������ɾ��
            foreach (Army army in setTargets) {
                //����·�����㣬���һ��List<Province>()
                List<Province> movePath = mapController.GetMovePath(army.CurrentProvince, destination);
                uint totalMoveCost = ProvinceHelper.GetProvinceMoveCost(movePath);
                // 
                ArmyMoveTask armyMoveTask = new ArmyMoveTask(totalMoveCost, army, movePath);

                // TODO: ͬ�������������
                //armyNetworkCtrl.MoveArmyEvent(army, destination.provinceData.provinceID);
                // ��������£�ֱ�ӵ���SetMoveTask
                army.SetMoveTask(armyMoveTask, IsWithdraw);
            }*/

            // ����״̬��
            armyNetworkCtrl.MoveArmyEvent(setTargets, destination.provinceData.provinceID, IsWithdraw);
        }

        /// <summary>
        /// ��ȡ���ӵĳ��˵ص�
        /// </summary>
        public Province GetWithdrawTarget(string armyTag, Province withdrawStart) {
            //Province des = PoliticLoader.Instance.GetFactionByTag(armyTag).GetWithdrawTarget();
            Province des = mapController.GetWithdrawTarget(withdrawStart, armyTag);

            Debug.Log(des.provinceData.provincePos);
            return des;
        }

        #endregion

        #region ���Ӳ���

        public void ShowArmySupplyLine() {
            foreach (var army in allArmyObject)
            {
                // �������Ƿ����˲�����
                ShowArmySupplyLine(army);
            }
        }

        public void ShowArmySupplyLine(Army army) {
            // �������Ƿ����˲�����
            if (army.ArmyData.NeedSupplySupport && army.CurSupplyTask != null) {
                ProvinceHelper.SetProvinceSupplyLine(army.CurSupplyTask.SupplyLine);
            }
        }

        public void HideArmySupplyLine() {
            foreach (var army in allArmyObject) {
                HideArmySupplyLine(army);
            }
        }

        public void HideArmySupplyLine(Army army) {
            // �������Ƿ����˲�����
            if (army.ArmyData.NeedSupplySupport && army.CurSupplyTask != null) {
                ProvinceHelper.SetProvinceCloseSupply(army.CurSupplyTask.SupplyLine);
            }
        }

        #endregion


        /// <summary>
        /// ���Ѿ������uI
        /// </summary>
        public void InvokeArmyPanel() {
            
            if (currentChooseArmy.Count <= 0) return;
            armyPanel.Show();

            //Debug.Log("������army panel ˢ������");

            // NOTICE: �����⣡�����ᵯ������壬�����
            //����Ѿ��򿪾������
            //if (armyPanel.Visible) {
            //armyPanel.UpdateArmyPanel(currentChooseArmy);
            //} else {
            armyPanel.InitArmyPanel(currentChooseArmy);
            //}
        }

        /// <summary>
        /// ����ս�����ui
        /// </summary>
        private void CheckToInvokeCombatPanel() {
            foreach (Army army in currentChooseArmy)
            {
                if(army.ArmyActionState == ArmyActionState.IsInCombat) {
                    // ѡ�еĵ�λ��������ս���ģ��������񣬻���ս�����
                    if (army.CurrentProvince.currentCombat != null) {
                        //Debug.Log("!!!");
                        InvokeCombatPanel(army);
                    }
                    break;
                }
            }
        }

        public void InvokeCombatPanel(Army army) {

            // �Ѿ���չʾһ��ս����
            if (combatPanel.Visible) {
                return;
            }

            List<Army> armies = new List<Army> { army };

            // ��ʾcombat���
            combatPanel.Show();
            combatPanel.InitCombatPanel(armies, army.CurrentProvince.currentCombat, army.CurrentProvince);
        }

    }
}