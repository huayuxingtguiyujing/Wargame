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

        [Header("测试栏")]
        public Province withdrawTarget;

        [Space]
        public static ArmyController Instance;

        [SerializeField] ArmyNetworkCtrl armyNetworkCtrl;

        [Header("各个UI面板")]
        [SerializeField] MapController mapController;
        [SerializeField] ArmyPanel armyPanel;
        [SerializeField] CombatPanel combatPanel;

        [Header("框选样式")]
        public Material drawMat;

        [Header("军队prefab")]
        public GameObject armyPrefab;
        public Transform armyTransform;

        [Header("所有的军队单位")]
        public List<Army> allArmyObject;
        
        public List<Army> currentChooseArmy;

        [Header("默认挂载的armyunit")]
        [SerializeField] ArmyUnitData InfantryArmyUnitData;

        #region 框选 所需 变量
        [Header("控制框选")]
        private Dictionary<GameObject, Vector3> armyWorldPositionDic;
        private Dictionary<GameObject, Vector2> armyScreenPositionDic;

        private Vector2 mouseStartPosition = new Vector2(0, 0);
        private Vector2 mouseEndPosition = new Vector2(0, 0);
        private Vector2 mousePosition = new Vector2(0, 0);

        private bool mouseDown = false;

        #endregion

        #region 移动军队 所需变量
        RaycastHit hit;
        [Header("军队可移动的区域")]
        [SerializeField] LayerMask clickableLayer;
        [SerializeField] int maxDetectDistance = 100;
        #endregion

        void Start() {
            if (Instance == null) Instance = this;

            //armyPanel.InitArmyPanel(currentChooseArmy, this);
            armyPanel.Hide();
            combatPanel.Hide();

            //初始化
            currentChooseArmy = new List<Army>();
            armyWorldPositionDic = new Dictionary<GameObject, Vector3>();
            armyScreenPositionDic = new Dictionary<GameObject, Vector2>();

            //TODO：将场景中所有 army 加入 allArmyObject 当中
            //TODO：将allarmyObject已有的单位放入数据结构中

            //NOTICE：HideFlags？：wtf？
            drawMat.hideFlags = HideFlags.HideAndDontSave;
            drawMat.shader.hideFlags = HideFlags.HideAndDontSave;
        }

        void Update() {
            //鼠标框选逻辑
            MouseRectChooseArmy();
            //点击省份进行移动
            ClickToMoveArmy();
        }

        void OnGUI() {
            if (mouseDown) {
                //绘制可视的选框
                DrawRect();
            }
        }

        #region 鼠标框选
        private void MouseRectChooseArmy() {
            mousePosition = Input.mousePosition;

            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                //按下鼠标左键
                mouseDown = true;
                mouseStartPosition = Input.mousePosition;

            }

            if (Input.GetKeyUp(KeyCode.Mouse0)) {
                //松开鼠标左键
                mouseEndPosition = Input.mousePosition;
                mouseDown = false;

                //判定选中物体
                GetArmyInRect();
            }

            //每次更新物体的屏幕位置
            for (int i = 0; i < allArmyObject.Count; i++) {

                armyWorldPositionDic[allArmyObject[i].gameObject] = allArmyObject[i].transform.position;

                armyScreenPositionDic[allArmyObject[i].gameObject] = Camera.main.WorldToScreenPoint(
                    armyWorldPositionDic[allArmyObject[i].gameObject]
                );
            }
        }

        private void GetArmyInRect() {
            //NOTICE：因为写的逻辑的问题，任何点击都会触发框选，请思考解决方案

            bool cancleChoosenFlag = true;

            bool playChooseAudio = false;

            if (Vector2.Distance(mouseStartPosition, mouseEndPosition) <= 3.0f) {
                // 距离太近，不会触发接下来的逻辑
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

                        //当选中了军队时，取消上一支军队的选中
                        if (cancleChoosenFlag) {
                            SetArmyUnchoosen();
                            cancleChoosenFlag = false;
                        }
                        // 选中了军队 会播放选中音频
                        playChooseAudio = true;
                        //在框选的范围内
                        currentChooseArmy.Add(allArmyObject[i]);
                    }
                }
            } else {
                // NOTICE : 这部分没有用，但不要删
                //点选功能
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
                // TODO: 播放选中军队音频
                AudioManager.PlayAudio(AudioEffectName.ChooseArmy);
            }

            SetArmyChoosen();

            //TODO:选中了正在作战的单位，展开作战面板（只展开其中一支军队）
            CheckToInvokeCombatPanel();

            //TODO:展开并且更新 军队状况ui
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

            //保存摄像机变换矩阵
            GL.PushMatrix();
            //设置用屏幕坐标绘图  
            GL.LoadPixelMatrix();

            Color clr = Color.black;

            GL.Begin(GL.LINES);
            GL.Color(clr);
            GL.Vertex3(mouseStartPosition.x, mouseStartPosition.y, 0);
            GL.Vertex3(mousePosition.x, mouseStartPosition.y, 0);
            GL.End();

            //下
            GL.Begin(GL.LINES);
            GL.Color(clr);
            GL.Vertex3(mouseStartPosition.x, mousePosition.y, 0);
            GL.Vertex3(mousePosition.x, mousePosition.y, 0);
            GL.End();

            //左
            GL.Begin(GL.LINES);
            GL.Color(clr);
            GL.Vertex3(mouseStartPosition.x, mouseStartPosition.y, 0);
            GL.Vertex3(mouseStartPosition.x, mousePosition.y, 0);
            GL.End();

            //右
            GL.Begin(GL.LINES);
            GL.Color(clr);
            GL.Vertex3(mousePosition.x, mouseStartPosition.y, 0);
            GL.Vertex3(mousePosition.x, mousePosition.y, 0);
            GL.End();

            GL.PopMatrix();//还原  
        }

        #endregion

        #region 创建、销毁、合并、选中、取消选中军队
        // 创建与初始化军队
        /// <summary>
        ///  仅创建军队的object
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

        // TODO: 检测是否完成了网络同步部分
        public void InitArmy(ArmyUnit armyUnitData, Province province, Army army, int unitNum = 1) {
            ArmyData initArmyData = new ArmyData(armyUnitData);
            // 根据单位招募的省份创建ArmyData
            initArmyData.ArmyTag = province.provinceData.OwnerTag;
            //  return 
            InitArmy(initArmyData, province, army);
        }

        public void InitArmy(ArmyData armyData, Province province, Army army) {
            //Debug.Log("create army! num: " + province.provinceData.provinceEco.prosperity.ToString() + ", province name:" + province.provincePosition);

            // -------------------------
            //初始化并设置 army 的prefab
            //GameObject armyObject = Instantiate(armyPrefab, armyTransform);
            //Army army = armyObject.GetComponent<Army>();
            //armyObject.transform.position = province.transform.position;
            
            GameObject armyObject = army.gameObject;
            army.InitArmy(armyData, province);
            //--------------------------

            // 添加到国家记录中
            GamePlayState.GetFaction(army.ArmyData.ArmyTag).AddNewArmy(army);
            allArmyObject.Add(army);

            //添加到坐标映射字典中
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
            //Debug.Log("成功生成了军队: " + networkObject.GetHashCode());
            //return army;
        }

        // TODO: 适应网络同步!
        public void CreateArmy_Random() {

            if (mapController.IDProvinceDic == null) {
                Debug.LogWarning("no province!");
                return;
            } else {
                Debug.Log("current province num:" + mapController.IDProvinceDic.Count);
            }

            //获得随机军队数目 随机的省份
            System.Random random = new System.Random();
            int armyUnitNum = random.Next(5);

            //TODO:获得随机省份有问题，会固定在初始省份生成
            Province province = default;
            int randomIndex = random.Next(mapController.IDProvinceDic.Count);
            //Debug.Log(randomIndex);
            province = mapController.IDProvinceDic.Values.ToArray()[randomIndex];

            if (province != null && province != default) {
                // TODO: 记得写完！
                //CreateArmy(new ArmyUnit(InfantryArmyUnitData), province);
            } else {
                Debug.LogWarning("create fail!");
            }

        }
        
        // 移除军队
        // TODO: 适应网络同步!
        public void RemoveArmies(List<Army> armies) {
            foreach (Army army in armies)
            {
                RemoveArmy(army);
            }
        }

        public void RemoveArmy(Army targetArmy) {
            //删除对应的军队单位
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
                    // 从对应的势力中移除
                    GamePlayState.GetFaction(armyObject.ArmyData.ArmyTag).RemoveArmy(armyObject);
                    armyObject.RemoveArmy();
                }
            }
            currentChooseArmy.Clear();
        }

        public void RemoveArmy_All() {
            foreach (Army armyObject in allArmyObject) {
                // 从对应的势力中移除
                GamePlayState.GetFaction(armyObject.ArmyData.ArmyTag).RemoveArmy(armyObject);
                armyObject.RemoveArmy();
            }
            allArmyObject.Clear();
            currentChooseArmy.Clear();
        }

        // TODO: 适应网络同步!
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
                Debug.LogError("没有选中要合并的军队");
                return null;
            }
            
            if(mergeHost == null) {
                // 如果没有传入mergeHost，默认mergeHost为列表倒数第一个
                mergeHost = armies[0];
            }

            if (!armies.Contains(mergeHost)) {
                Debug.LogError("合并目标军队不在选中军队中");
                return null;
            }

            if (AbleToMergeArmy(armies)) {
                //合并前 停止军队移动, 将合并目标从列表中除去
                ArmyHelper.GetInstance().SetArmyStayIn(armies);
                armies.Remove(mergeHost);

                int currentCount = armies.Count;
                for (int i = currentCount - 1; i >= 0; i--) {
                    //将所有单位都加入到其中一个单位中
                    mergeHost.MergeArmy(armies[i]);
                    //销毁其他单位
                    RemoveArmy(armies[i]);

                }

                mergeHost.UpdateArmyUI();

                InvokeArmyPanel();

                return mergeHost;
            }

            return null;
        }

        /// <summary>
        /// 判断当前选中的军队 是否可以合并
        /// </summary>
        public bool AbleToMergeArmy(List<Army> mergetTarget) {

            if (mergetTarget.Count <= 1) {
                return false;
            }

            bool ans = true;
            string provinceId = mergetTarget[0].CurrentProvince.provincePos;
            foreach (Army army in mergetTarget) {
                //判断是否同在一个省份
                ans = ans && provinceId.Equals(army.CurrentProvince.provincePos);

                //判断是否是同一势力（能同时选中 自然是同一势力,应该吧）

                //判断军队本身是否允许合并
                ans = ans && army.AbleToMergeArmy();

                if (!ans) return false;
            }
            return true;
        }

        // TODO: 适应网络同步!
        public void SplitArmy_Choosen() {
            
            // TODO：设置拆分后的军队的将领
            /*int count = currentChooseArmy.Count;
            for (int i = 0; i < count; i++) {
                if (AbleToSplitArmy(currentChooseArmy[i])) {
                    //TODO : !!!会无限拆分，如何停止？？？
                    //TODO： 破案了，是按钮按下时会触发多次拆分事件！！！按钮的问题
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

            //设置拆分后的军队为新的选中
            SetArmyUnchoosen();
            currentChooseArmy = splitResults;
            SetArmyChoosen();
            ArmyHelper.GetInstance().SetArmyStayIn(currentChooseArmy);

            //刷新面板
            InvokeArmyPanel();
        }

        private List<Army> SplitArmy(List<Army> armies) {
            List<Army> splitResults = new List<Army>();

            // TODO：设置拆分后的军队的将领
            int count = armies.Count;
            for (int i = 0; i < count; i++) {
                if (AbleToSplitArmy(armies[i])) {
                    //TODO : !!!会无限拆分，如何停止？？？
                    //TODO： 破案了，是按钮按下时会触发多次拆分事件！！！按钮的问题
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
        /// 设置军队单位为选中的状态
        /// </summary>
        public void SetArmyChoosen() {
            bool onlyOpenOneCombat = true;

            foreach (Army armyObject in currentChooseArmy) {

                //如果处于战斗状态，那么打开战斗面板（仅打开一个处于战斗中的单位）
                if (onlyOpenOneCombat) {
                    if (armyObject.ArmyActionState == ArmyActionState.IsInCombat) {
                        onlyOpenOneCombat = false;

                        //TODO : 打开战斗面板
                    }
                }

                armyObject.SetArmyChoosen();
                armyObject.ShowMovePath();
            }
        }

        public void SetArmyChoosen_Single(Army army) {
            // 清空当前的选中单位
            SetArmyUnchoosen();

            // 选中该单位
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
            //关闭军队面板ui
            armyPanel.Hide();
        }
        #endregion

        #region 移动军队
        private void ClickToMoveArmy() {
            //未选中军队 则返回
            if (currentChooseArmy.Count <= 0) {
                return;
            }

            //NOTICE：射线检测仅能检测到3D物体！要加上3D的collider
            if (Input.GetMouseButtonDown(1)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //射线检测 是否点击省份物体
                if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                    Province destination = hit.collider.gameObject.GetComponent<Province>();
                    if (destination != null) {
                        //Debug.Log(hit.collider.gameObject.name);
                        SetArmyDestination(destination, currentChooseArmy, false);

                        // TODO: 播放移动军队音频
                        AudioManager.PlayAudio(AudioEffectName.ClickMoveArmy);
                    }
                }
            }
        }

        public void SetArmyDestination(Province destination, List<Army> setTargets, bool IsWithdraw) {

            /*// 单机模式，原本逻辑，请勿删除
            foreach (Army army in setTargets) {
                //进行路径计算，获得一个List<Province>()
                List<Province> movePath = mapController.GetMovePath(army.CurrentProvince, destination);
                uint totalMoveCost = ProvinceHelper.GetProvinceMoveCost(movePath);
                // 
                ArmyMoveTask armyMoveTask = new ArmyMoveTask(totalMoveCost, army, movePath);

                // TODO: 同步到联网玩家中
                //armyNetworkCtrl.MoveArmyEvent(army, destination.provinceData.provinceID);
                // 单机情况下，直接调用SetMoveTask
                army.SetMoveTask(armyMoveTask, IsWithdraw);
            }*/

            // 联网状态中
            armyNetworkCtrl.MoveArmyEvent(setTargets, destination.provinceData.provinceID, IsWithdraw);
        }

        /// <summary>
        /// 获取军队的撤退地点
        /// </summary>
        public Province GetWithdrawTarget(string armyTag, Province withdrawStart) {
            //Province des = PoliticLoader.Instance.GetFactionByTag(armyTag).GetWithdrawTarget();
            Province des = mapController.GetWithdrawTarget(withdrawStart, armyTag);

            Debug.Log(des.provinceData.provincePos);
            return des;
        }

        #endregion

        #region 军队补给

        public void ShowArmySupplyLine() {
            foreach (var army in allArmyObject)
            {
                // 检查军队是否建立了补给线
                ShowArmySupplyLine(army);
            }
        }

        public void ShowArmySupplyLine(Army army) {
            // 检查军队是否建立了补给线
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
            // 检查军队是否建立了补给线
            if (army.ArmyData.NeedSupplySupport && army.CurSupplyTask != null) {
                ProvinceHelper.SetProvinceCloseSupply(army.CurSupplyTask.SupplyLine);
            }
        }

        #endregion


        /// <summary>
        /// 唤醒军队面板uI
        /// </summary>
        public void InvokeArmyPanel() {
            
            if (currentChooseArmy.Count <= 0) return;
            armyPanel.Show();

            //Debug.Log("调用了army panel 刷新了它");

            // NOTICE: 有问题！！！会弹不出面板，好奇怪
            //如果已经打开军队面板
            //if (armyPanel.Visible) {
            //armyPanel.UpdateArmyPanel(currentChooseArmy);
            //} else {
            armyPanel.InitArmyPanel(currentChooseArmy);
            //}
        }

        /// <summary>
        /// 唤醒战役面板ui
        /// </summary>
        private void CheckToInvokeCombatPanel() {
            foreach (Army army in currentChooseArmy)
            {
                if(army.ArmyActionState == ArmyActionState.IsInCombat) {
                    // 选中的单位中有正在战斗的，传入事务，唤醒战斗面板
                    if (army.CurrentProvince.currentCombat != null) {
                        //Debug.Log("!!!");
                        InvokeCombatPanel(army);
                    }
                    break;
                }
            }
        }

        public void InvokeCombatPanel(Army army) {

            // 已经在展示一场战役了
            if (combatPanel.Visible) {
                return;
            }

            List<Army> armies = new List<Army> { army };

            // 显示combat面板
            combatPanel.Show();
            combatPanel.InitCombatPanel(armies, army.CurrentProvince.currentCombat, army.CurrentProvince);
        }

    }
}