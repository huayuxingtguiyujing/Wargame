using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace WarGame_True.Infrastructure.Excel {
    public class ExcelHandler : MonoBehaviour {

        public static string generalExcelPath = "Assets/Excel/Generals.xlsx";


        [MenuItem("Excel/Test EPPlus Excel")]//���Unity�༭���˵�����������
        public static void LoadExcel() {
            //ָ������ȡ�����ļ�·�����ڱ༭��ģʽ�£�Application.dataPath����Assets�ļ���
            string path = generalExcelPath;

            //�����ļ���fs
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // ���ļ���fs��ΪExcel�ļ�����ʼ����(����������Ĺ���)
            ExcelPackage excel = new ExcelPackage(fs);

            //���ҵ��������ڵĸ�������
            ExcelWorksheets workSheets = excel.Workbook.Worksheets;

            //ֻ����һ�����������߲���
            ExcelWorksheet workSheet = workSheets[1];

            //�������������
            int colCount = workSheet.Dimension.End.Column;
            int rowCount = workSheet.Dimension.End.Row;

            for (int row = 1; row <= rowCount; row++) {
                string rec = "";
                for (int col = 1; col <= colCount; col++) {
                    //��ȡÿ����Ԫ���е�����
                    string text = workSheet.Cells[row, col].Text;
                    rec += text;
                    rec += "_";
                }
                Debug.LogFormat("��������:{0}", rec);
            }

            Debug.Log("complete");
            return;
        }

        /// <summary>
        /// ��ȡexcel�ļ�
        /// </summary>
        /// <param name="rowCallback">�ص����ܻ�ȡÿ�е���������</param>
        /// <param name="countOfAttributes">��Ч����ֵ�ֶΣ����ܳ���ʵ�ʶ���������</param>
        public static void LoadExcel(string path, UnityAction<int, int, ExcelRange> rowCallback, int countOfAttributes) {

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            ExcelPackage excel = new ExcelPackage(fs);
            ExcelWorksheets workSheets = excel.Workbook.Worksheets;

            //ֻ����һ�����������߲���
            ExcelWorksheet workSheet = workSheets[1];

            //�������������
            int colCount = Mathf.Min(countOfAttributes, workSheet.Dimension.End.Column) ;
            int rowCount = workSheet.Dimension.End.Row;

            for (int row = 1; row <= rowCount; row++) {
                //string rec = "";
                //for (int col = 1; col <= colCount; col++) {
                    //��ÿ����Ԫ���е����� ����ص�
                    rowCallback(row, colCount, workSheet.Cells);
                    //rec += workSheet.Cells[row, col].Text;
                    //rec += "_";
                //}
                //Debug.LogFormat("��������:{0}", rec);
            }

        }

        public static void LoadExcel(Object excelFile, UnityAction<int, int, ExcelRange> cellCallback, int countOfAttributes) {
            LoadExcel(AssetDatabase.GetAssetPath(excelFile), cellCallback, countOfAttributes);
        }

        /// <summary>
        /// ��excel���Ϊ�ֵ�����
        /// </summary>
        /// <param name="countOfAttributes">ÿ���ж�������ֵ</param>
        /// <returns></returns>
        public static Dictionary<int, IndividualData> LoadExcelAsDictionary(int countOfAttributes, string path) {

            try {
                // �½��ֵ䣬���ڴ洢����Ϊ��λ�ĸ���������Ԫ
                Dictionary<int, IndividualData> ItemDictionary = new Dictionary<int, IndividualData>();

                // �����ļ���fs
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                ExcelPackage excel = new ExcelPackage(fs);
                ExcelWorksheets workSheets = excel.Workbook.Worksheets;
                // ֻ����һ�����������߲���
                ExcelWorksheet workSheet = workSheets[1];

                int colCount = workSheet.Dimension.End.Column;
                int rowCount = workSheet.Dimension.End.Row;

                for (int row = 2; row <= rowCount; row++) {
                    // �½�һ��������Ԫ����ʼ���ձ�������
                    IndividualData item = new IndividualData(countOfAttributes);
                    for (int col = 1; col <= colCount; col++) {
                        // ����Ԫ���е�����д�������Ԫ
                        item.Values[col - 1] = workSheet.Cells[row, col].Text;
                    }

                    int itemID = Convert.ToInt32(item.Values[0].ToString());
                    ItemDictionary.Add(itemID, item);
                }

                return ItemDictionary;
            } catch (Exception e) {

                throw new Exception($"��ȡexcel�ļ�ʱ��������:{e}");
            }
        }
    
    }
}