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

        private int generalPropertyNum = 22;        //��ǰexcel�ֶα���22���ֶ�
        private int factionPropertyNum = 5;

        private string generalObjectPath = "Assets/Prefab/General";
        private string factionObjectPath = "Assets/Prefab/Politic/Faction";

        private string title1 = "�������ݲ���";
        private string title2 = "��ϵ���ݲ���";
        private string generalTitle = "General Object Path";
        private string factionTitle = "Faction Object Path";

        [MenuItem("Excel/ExcelHandler")]
        public static void ShowWindow() {
            GetWindow<ExcelHandlerWindow>("ExcelHandler");
        }

        private async void OnGUI() {
            //GUILayout.Label("Excel Handler", EditorStyles.boldLabel);

            GUILayout.Label(title1);
            generalExcelFile = EditorGUILayout.ObjectField("����excel�ļ�", generalExcelFile, typeof(Object), true);
            generalExcelPath = EditorGUILayout.TextField("����excel·��", generalExcelPath);
            generalPropertyNum = EditorGUILayout.IntField("����excel��������", generalPropertyNum);
            GUILayout.Space(2);
            generalObjectPath = EditorGUILayout.TextField("����Object�����ļ���", generalObjectPath);
            if (GUILayout.Button("���뵽General Objects��", GUILayout.Width(300), GUILayout.Height(30))) {
                ImportGeneralExcelToObjects(generalObjectPath, generalExcelFile, generalPropertyNum);
            }
            if (GUILayout.Button("������General Excel�ļ���", GUILayout.Width(300), GUILayout.Height(30))) {
                ExportGeneralObjectsToExcel();
            }

            GUILayout.Space(15);

            GUILayout.Label(title2);
            factionExcelFile = EditorGUILayout.ObjectField("��ϵexcel�ļ�", factionExcelFile, typeof(Object), true);
            factionExcelPath = EditorGUILayout.TextField("��ϵexcel·��", factionExcelPath);
            factionPropertyNum = EditorGUILayout.IntField("��ϵexcel��������", factionPropertyNum);
            GUILayout.Space(2);
            factionObjectPath = EditorGUILayout.TextField("��ϵObject�����ļ���", factionObjectPath);
            if (GUILayout.Button("���뵽Faction Objects��", GUILayout.Width(300), GUILayout.Height(30))) {
                ImportFactionExcelToObjects();
            }
            if (GUILayout.Button("������Faction Excel�ļ���", GUILayout.Width(300), GUILayout.Height(30))) {
                ExportFactionObjectsToExcel();
            }

            /*// ��ק�¼�����ʱ�������ļ����ڵ�λ��
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
        /// ʱ�̸���excel�ļ���·��
        /// </summary>
        private void UpdateExcelFilePath() {
            if (generalExcelFile != null) {
                // ��⵽��ǰ�Ľ����ļ���Դ�����仯�󣬸���·��
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
        /// �������excel�����ļ����뵽objects��
        /// </summary>
        private async void ImportGeneralExcelToObjects(string path, Object excelFile, int generalPropertyNum){
            
            // ��ȡ���еĽ��������� scriptObject 
            FileInfo[] fileInfos = ExcelFileUtil.GetFileByPath(path);
            List<GeneralData> generalObjList = new List<GeneralData>();
            foreach (FileInfo fileInfo in fileInfos)
            {
                try {
                    string generalPath = generalObjectPath + "/" + fileInfo.Name;
                    
                    // NOTICE��Addressableʹ���첽������������
                    GeneralData generalObj = await Addressables.LoadAssetAsync<GeneralData>(generalPath).Task;
                    if (generalObj != null) generalObjList.Add(generalObj);
                } catch {
                    throw new System.Exception("��ȡ��������ʱ����");
                }
            }
            Dictionary<string, GeneralData> generalDic = generalObjList.ToDictionary(generalObj => generalObj.generalName);
            //Debug.Log(generalDic.Count);
             
            /*// չʾ��ȡ����GeneralData
            Debug.Log(generalObjectClasses.Count);
            // ���������࣬�������ñ�װ�ص�����������
            foreach (GeneralData generalObject in generalObjectClasses)
            {
                // TODO : ����װ���߼���ôд
                generalObject.generalDescription = "���ʱ�ڵĽ���";
                Debug.Log(generalObject.name);
            }*/

            UnityAction<int, int, ExcelRange> rowCall = (rowNum, colCount, rowdata) => {
                
                // ��һ����tagʶ���,���ý���� scriptobject
                if (generalDic.ContainsKey(rowdata[rowNum, 1].Text)) {
                    // TODO: ��Ҫ�޸ĸöδ��룬excel�Ĵ���� ���ܷ��ڷ�general�ļ�����
                    //generalDic[rowdata[rowNum, 1].Text].SetGeneralData(rowdata, rowNum);
                }

            /*����
            NOTICE: ��Ҫ��1��ʼ����
                string rec = "";
            for (int col = 1; col <= colCount; col++) {
                rec += rowdata[rowNum, col].Text + "_";
                // ���ݵ�һ��GeneralName�ֶΣ����¶�Ӧ��generaldata
            }
            Debug.Log(rec);*/
            };
            ExcelHandler.LoadExcel(generalExcelFile, rowCall, generalPropertyNum);

            Debug.Log("�ѳɹ���excel���ݵ��뵽����object�У�");
        }

        private void ExportGeneralObjectsToExcel() {

        }

        /// <summary>
        /// ����ϵ��excel�����ļ����뵽objects��
        /// </summary>
        private void ImportFactionExcelToObjects() {

        }

        private void ExportFactionObjectsToExcel() {

        }


    }
}