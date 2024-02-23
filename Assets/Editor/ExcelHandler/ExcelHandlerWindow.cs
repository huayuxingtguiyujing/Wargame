using OfficeOpenXml;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.Infrastructure.Excel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace ExcelHandlerEditor {
    public class ExcelHandlerWindow : EditorWindow {
        private Object generalExcelFile;
        private Object factionExcelFile;

        private string generalExcelPath = "";
        private string factionExcelPath = "";

        private int generalPropertyNum = 22;        //当前excel字段表有22个字段
        private int factionPropertyNum = 5;

        private string generalObjectPath = "Assets/Prefab/General";
        private string factionObjectPath = "Assets/Prefab/Politic/Faction";

        private string title1 = "将领数据操作";
        private string title2 = "派系数据操作";
        private string generalTitle = "General Object Path";
        private string factionTitle = "Faction Object Path";

        [MenuItem("Excel/ExcelHandler")]
        public static void ShowWindow() {
            GetWindow<ExcelHandlerWindow>("ExcelHandler");
        }

        private async void OnGUI() {
            //GUILayout.Label("Excel Handler", EditorStyles.boldLabel);

            GUILayout.Label(title1);
            generalExcelFile = EditorGUILayout.ObjectField("将领excel文件", generalExcelFile, typeof(Object), true);
            generalExcelPath = EditorGUILayout.TextField("将领excel路径", generalExcelPath);
            generalPropertyNum = EditorGUILayout.IntField("将领excel属性列数", generalPropertyNum);
            GUILayout.Space(2);
            generalObjectPath = EditorGUILayout.TextField("将领Object所在文件夹", generalObjectPath);
            if (GUILayout.Button("导入到General Objects中", GUILayout.Width(300), GUILayout.Height(30))) {
                ImportGeneralExcelToObjects(generalObjectPath, generalExcelFile, generalPropertyNum);
            }
            if (GUILayout.Button("导出到General Excel文件中", GUILayout.Width(300), GUILayout.Height(30))) {
                ExportGeneralObjectsToExcel();
            }

            GUILayout.Space(15);

            GUILayout.Label(title2);
            factionExcelFile = EditorGUILayout.ObjectField("派系excel文件", factionExcelFile, typeof(Object), true);
            factionExcelPath = EditorGUILayout.TextField("派系excel路径", factionExcelPath);
            factionPropertyNum = EditorGUILayout.IntField("派系excel属性列数", factionPropertyNum);
            GUILayout.Space(2);
            factionObjectPath = EditorGUILayout.TextField("派系Object所在文件夹", factionObjectPath);
            if (GUILayout.Button("导入到Faction Objects中", GUILayout.Width(300), GUILayout.Height(30))) {
                ImportFactionExcelToObjects();
            }
            if (GUILayout.Button("导出到Faction Excel文件中", GUILayout.Width(300), GUILayout.Height(30))) {
                ExportFactionObjectsToExcel();
            }

            /*// 拖拽事件发生时，更新文件所在的位置
            if (Event.current.type == EventType.DragPerform) {
                DragAndDrop.AcceptDrag();
                foreach (Object obj in DragAndDrop.objectReferences) {
                    if (obj != null) {
                        //if (title1.Equals("Excel File 1")) {
                            generalObjectPath = AssetDatabase.GetAssetPath(obj);
                        //} else if (title2.Equals("Excel File 2")) {
                            factionObjectPath = AssetDatabase.GetAssetPath(obj);
                        //}
                    }
                }
            }*/

            UpdateExcelFilePath();
        }

        /// <summary>
        /// 时刻更新excel文件的路径
        /// </summary>
        private void UpdateExcelFilePath() {
            if (generalExcelFile != null) {
                // 检测到当前的将领文件来源发生变化后，更新路径
                string newExcelFilePath = AssetDatabase.GetAssetPath(generalExcelFile);
                if (newExcelFilePath != generalExcelPath) {
                    generalExcelPath = newExcelFilePath;
                }
            } else {
                generalExcelPath = "";
            }

            if (factionExcelFile != null) {
                string newExcelFilePath = AssetDatabase.GetAssetPath(factionExcelFile);
                if (newExcelFilePath != factionExcelPath) {
                    factionExcelPath = newExcelFilePath;
                }
            } else {
                factionExcelPath = "";
            }
        }
    
        /// <summary>
        /// 将将领的excel配置文件导入到objects中
        /// </summary>
        private async void ImportGeneralExcelToObjects(string path, Object excelFile, int generalPropertyNum){
            
            // 获取所有的将领数据类 scriptObject 
            FileInfo[] fileInfos = ExcelFileUtil.GetFileByPath(path);
            List<GeneralData> generalObjList = new List<GeneralData>();
            foreach (FileInfo fileInfo in fileInfos)
            {
                try {
                    string generalPath = generalObjectPath + "/" + fileInfo.Name;
                    
                    // NOTICE：Addressable使用异步方法，加载慢
                    GeneralData generalObj = await Addressables.LoadAssetAsync<GeneralData>(generalPath).Task;
                    if (generalObj != null) generalObjList.Add(generalObj);
                } catch {
                    throw new System.Exception("获取将领数据时出错！");
                }
            }
            Dictionary<string, GeneralData> generalDic = generalObjList.ToDictionary(generalObj => generalObj.generalName);
            //Debug.Log(generalDic.Count);
             
            /*// 展示获取到的GeneralData
            Debug.Log(generalObjectClasses.Count);
            // 遍历将领类，根据配置表装载到将领数据中
            foreach (GeneralData generalObject in generalObjectClasses)
            {
                // TODO : 想想装载逻辑怎么写
                generalObject.generalDescription = "五代时期的将领";
                Debug.Log(generalObject.name);
            }*/

            UnityAction<int, int, ExcelRange> rowCall = (rowNum, colCount, rowdata) => {
                
                // 第一列是tag识别符,设置将领的 scriptobject
                if (generalDic.ContainsKey(rowdata[rowNum, 1].Text)) {
                    // TODO: 需要修改该段代码，excel的处理包 不能放在非general文件夹下
                    //generalDic[rowdata[rowNum, 1].Text].SetGeneralData(rowdata, rowNum);
                }

            /*测试
            NOTICE: 需要从1开始计数
                string rec = "";
            for (int col = 1; col <= colCount; col++) {
                rec += rowdata[rowNum, col].Text + "_";
                // 根据第一个GeneralName字段，更新对应的generaldata
            }
            Debug.Log(rec);*/
            };
            ExcelHandler.LoadExcel(generalExcelFile, rowCall, generalPropertyNum);

            Debug.Log("已成功将excel数据导入到将领object中！");
        }

        private void ExportGeneralObjectsToExcel() {

        }

        /// <summary>
        /// 将派系的excel配置文件导入到objects中
        /// </summary>
        private void ImportFactionExcelToObjects() {

        }

        private void ExportFactionObjectsToExcel() {

        }


    }
}