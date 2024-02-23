
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WarGame_True.Infrastructure.HexagonGrid.MapObject;
using WarGame_True.Infrastructure.Map.Controller;
using WarGame_True.Infrastructure.Map.Util;

namespace WarGame_True.Infrastructure.Map.Provinces {
    /// <summary>
    /// 该脚本的作用是将 CSV 文件里编写的省份信息，注入到每一个省份里
    /// </summary>
    [RequireComponent(typeof(MapController))]
    public class ProvinceInject : MonoBehaviour {

        [SerializeField] private MapController mapController;

        [Header("省份 配置信息")]
        [SerializeField] private TextAsset provinceSetInfo;

        [Header("省份配置 导出位置")]
        [SerializeField] private string exportLocation;

        // 省份信息文件
        private string MapFileName = "currentMap.csv";


        private void Awake() {
            exportLocation = Application.persistentDataPath;
        }

        /// <summary>
        /// 导出当前的 省份配置信息
        /// </summary>
        public void ExportCurrentMapToJson() {
#if UNITY_EDITOR
            mapController.InitMap();
#endif

            foreach (KeyValuePair<uint, Province> keyValuePair in mapController.IDProvinceDic)
            {
                ProvinceData provinceData = keyValuePair.Value.provinceData;
                FileUtil fileUtil = new FileUtil();
                //写入到json文件中
                fileUtil.WriteJsonFile(Application.persistentDataPath + "test.json", provinceData);
                
            }
            
        }

        public void ExportCurrentMapToCSV() {
#if UNITY_EDITOR
            mapController.InitMap();
#endif
            string filePath = Application.persistentDataPath + "/" + MapFileName;

            //创建 csv 文件中
            if (!File.Exists(filePath)) {
                File.Create(filePath).Dispose();
            }

            //UTF-8方式保存 csv
            using (StreamWriter stream = new StreamWriter(filePath, false, Encoding.UTF8)) {
                //Debug.Log("province count: " + mapController.HexGirdProvince.Count + "_");
                foreach (Province province in mapController.IDProvinceDic.Values) {
                    ProvinceData provinceData = province.provinceData;
                    string str = provinceData.ToString();
                    stream.WriteLine(str);
                    //Debug.Log(str);
                }
            }

            Debug.Log("省份信息保存完毕! " + "存储路径:" + filePath);
        }

        /// <summary>
        /// 将文件省份配置信息 导入当前地图
        /// </summary>
        public async Task ImportCSVToCurrentMap() {
#if UNITY_EDITOR
            mapController.InitMap();
#endif

            string filePath = Application.persistentDataPath + "/" + MapFileName;

            // 创建 csv 文件中
            if (!File.Exists(filePath)) {
                File.Create(filePath).Dispose();
            }

            // NOTICE: ProvinceInject 读取省份数据是基于stream Reader的 ReadLineAsync，操作很耗时
            // 用CSV文本导入要注意输入的顺序
            using (StreamReader stream = new StreamReader(filePath, Encoding.UTF8, false)) {
                while (!stream.EndOfStream) {
                    //string str = stream.ReadLine();
                    string str = await stream.ReadLineAsync();
                    ProvinceData provinceData = new ProvinceData(str);

                    // find id in map, and put data into province
                    uint provinceID = provinceData.provinceID;
                    if (mapController.IDProvinceDic.ContainsKey(provinceID)) {
                        mapController.IDProvinceDic[provinceID].SetProvinceData(provinceData);
                        //Debug.Log(str);
                    }

                    // find position in map, and put data into province
                    //Vector3 position = provinceData.provincePos;
                    //if (mapController.PosProvinceDic.ContainsKey(position)) {
                    //    mapController.PosProvinceDic[position].SetProvinceData(provinceData);
                    //    Debug.Log(str);
                    //}
                }
            }

            Debug.Log("导出省份数据完毕！" + "存储路径:" + filePath);
        }

        public void ImportCSVToCurrentMap(TextAsset provinceDataFile) {
            string[] lineData = provinceDataFile.text.Split('\n');
            
            for (int i = 0; i < lineData.Length; i++) {
                string str = lineData[i];
                // 读取省份数据文件的每一行，然后转为provincedata
                ProvinceData provinceData = new ProvinceData(str);
                uint provinceID = provinceData.provinceID;
                if (mapController.IDProvinceDic.ContainsKey(provinceID)) {
                    mapController.IDProvinceDic[provinceID].SetProvinceData(provinceData);
                    //Debug.Log(str);
                }
            }
        }

        public void ClearMapStorage() {
            mapController.PosProvinceDic.Clear();
            mapController.IDProvinceDic.Clear();

            string filePath = Application.persistentDataPath + "/" + MapFileName;

            //创建 csv 文件中
            if (!File.Exists(filePath)) {
                return;
            }

            using (StreamWriter stream = new StreamWriter(filePath, false, Encoding.UTF8)) {
                stream.WriteLine("no data,no data, no data");
            }

            Debug.Log("已经清空省份信息! " + "存储路径:" + filePath);
        }
    }
}