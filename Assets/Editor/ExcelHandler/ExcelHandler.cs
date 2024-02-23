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


        [MenuItem("Excel/Test EPPlus Excel")]//添加Unity编辑器菜单项用来读表
        public static void LoadExcel() {
            //指定待读取表格的文件路径。在编辑器模式下，Application.dataPath就是Assets文件夹
            string path = generalExcelPath;

            //建立文件流fs
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // 将文件流fs视为Excel文件，开始访问(第三方插件的功能)
            ExcelPackage excel = new ExcelPackage(fs);

            //查找到工作簿内的各工作表
            ExcelWorksheets workSheets = excel.Workbook.Worksheets;

            //只看第一个工作表，余者不看
            ExcelWorksheet workSheet = workSheets[1];

            //工作表的行列数
            int colCount = workSheet.Dimension.End.Column;
            int rowCount = workSheet.Dimension.End.Row;

            for (int row = 1; row <= rowCount; row++) {
                string rec = "";
                for (int col = 1; col <= colCount; col++) {
                    //读取每个单元格中的数据
                    string text = workSheet.Cells[row, col].Text;
                    rec += text;
                    rec += "_";
                }
                Debug.LogFormat("将领数据:{0}", rec);
            }

            Debug.Log("complete");
            return;
        }

        /// <summary>
        /// 读取excel文件
        /// </summary>
        /// <param name="rowCallback">回调，能获取每行的输入数据</param>
        /// <param name="countOfAttributes">有效属性值字段，不能超过实际读出的列数</param>
        public static void LoadExcel(string path, UnityAction<int, int, ExcelRange> rowCallback, int countOfAttributes) {

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            ExcelPackage excel = new ExcelPackage(fs);
            ExcelWorksheets workSheets = excel.Workbook.Worksheets;

            //只看第一个工作表，余者不看
            ExcelWorksheet workSheet = workSheets[1];

            //工作表的行列数
            int colCount = Mathf.Min(countOfAttributes, workSheet.Dimension.End.Column) ;
            int rowCount = workSheet.Dimension.End.Row;

            for (int row = 1; row <= rowCount; row++) {
                //string rec = "";
                //for (int col = 1; col <= colCount; col++) {
                    //将每个单元格中的数据 传入回调
                    rowCallback(row, colCount, workSheet.Cells);
                    //rec += workSheet.Cells[row, col].Text;
                    //rec += "_";
                //}
                //Debug.LogFormat("将领数据:{0}", rec);
            }

        }

        public static void LoadExcel(Object excelFile, UnityAction<int, int, ExcelRange> cellCallback, int countOfAttributes) {
            LoadExcel(AssetDatabase.GetAssetPath(excelFile), cellCallback, countOfAttributes);
        }

        /// <summary>
        /// 将excel输出为字典数据
        /// </summary>
        /// <param name="countOfAttributes">每行有多少属性值</param>
        /// <returns></returns>
        public static Dictionary<int, IndividualData> LoadExcelAsDictionary(int countOfAttributes, string path) {

            try {
                // 新建字典，用于存储以行为单位的各个操作单元
                Dictionary<int, IndividualData> ItemDictionary = new Dictionary<int, IndividualData>();

                // 建立文件流fs
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                ExcelPackage excel = new ExcelPackage(fs);
                ExcelWorksheets workSheets = excel.Workbook.Worksheets;
                // 只看第一个工作表，余者不看
                ExcelWorksheet workSheet = workSheets[1];

                int colCount = workSheet.Dimension.End.Column;
                int rowCount = workSheet.Dimension.End.Row;

                for (int row = 2; row <= rowCount; row++) {
                    // 新建一个操作单元，开始接收本行数据
                    IndividualData item = new IndividualData(countOfAttributes);
                    for (int col = 1; col <= colCount; col++) {
                        // 将单元格中的数据写入操作单元
                        item.Values[col - 1] = workSheet.Cells[row, col].Text;
                    }

                    int itemID = Convert.ToInt32(item.Values[0].ToString());
                    ItemDictionary.Add(itemID, item);
                }

                return ItemDictionary;
            } catch (Exception e) {

                throw new Exception($"读取excel文件时出现问题:{e}");
            }
        }
    
    }
}