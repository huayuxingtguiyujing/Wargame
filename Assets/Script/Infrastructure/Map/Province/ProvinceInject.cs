
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
    /// �ýű��������ǽ� CSV �ļ����д��ʡ����Ϣ��ע�뵽ÿһ��ʡ����
    /// </summary>
    [RequireComponent(typeof(MapController))]
    public class ProvinceInject : MonoBehaviour {

        [SerializeField] private MapController mapController;

        [Header("ʡ�� ������Ϣ")]
        [SerializeField] private TextAsset provinceSetInfo;

        [Header("ʡ������ ����λ��")]
        [SerializeField] private string exportLocation;

        // ʡ����Ϣ�ļ�
        private string MapFileName = "currentMap.csv";


        private void Awake() {
            exportLocation = Application.persistentDataPath;
        }

        /// <summary>
        /// ������ǰ�� ʡ��������Ϣ
        /// </summary>
        public void ExportCurrentMapToJson() {
#if UNITY_EDITOR
            mapController.InitMap();
#endif

            foreach (KeyValuePair<uint, Province> keyValuePair in mapController.IDProvinceDic)
            {
                ProvinceData provinceData = keyValuePair.Value.provinceData;
                FileUtil fileUtil = new FileUtil();
                //д�뵽json�ļ���
                fileUtil.WriteJsonFile(Application.persistentDataPath + "test.json", provinceData);
                
            }
            
        }

        public void ExportCurrentMapToCSV() {
#if UNITY_EDITOR
            mapController.InitMap();
#endif
            string filePath = Application.persistentDataPath + "/" + MapFileName;

            //���� csv �ļ���
            if (!File.Exists(filePath)) {
                File.Create(filePath).Dispose();
            }

            //UTF-8��ʽ���� csv
            using (StreamWriter stream = new StreamWriter(filePath, false, Encoding.UTF8)) {
                //Debug.Log("province count: " + mapController.HexGirdProvince.Count + "_");
                foreach (Province province in mapController.IDProvinceDic.Values) {
                    ProvinceData provinceData = province.provinceData;
                    string str = provinceData.ToString();
                    stream.WriteLine(str);
                    //Debug.Log(str);
                }
            }

            Debug.Log("ʡ����Ϣ�������! " + "�洢·��:" + filePath);
        }

        /// <summary>
        /// ���ļ�ʡ��������Ϣ ���뵱ǰ��ͼ
        /// </summary>
        public async Task ImportCSVToCurrentMap() {
#if UNITY_EDITOR
            mapController.InitMap();
#endif

            string filePath = Application.persistentDataPath + "/" + MapFileName;

            // ���� csv �ļ���
            if (!File.Exists(filePath)) {
                File.Create(filePath).Dispose();
            }

            // NOTICE: ProvinceInject ��ȡʡ�������ǻ���stream Reader�� ReadLineAsync�������ܺ�ʱ
            // ��CSV�ı�����Ҫע�������˳��
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

            Debug.Log("����ʡ��������ϣ�" + "�洢·��:" + filePath);
        }

        public void ImportCSVToCurrentMap(TextAsset provinceDataFile) {
            string[] lineData = provinceDataFile.text.Split('\n');
            
            for (int i = 0; i < lineData.Length; i++) {
                string str = lineData[i];
                // ��ȡʡ�������ļ���ÿһ�У�Ȼ��תΪprovincedata
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

            //���� csv �ļ���
            if (!File.Exists(filePath)) {
                return;
            }

            using (StreamWriter stream = new StreamWriter(filePath, false, Encoding.UTF8)) {
                stream.WriteLine("no data,no data, no data");
            }

            Debug.Log("�Ѿ����ʡ����Ϣ! " + "�洢·��:" + filePath);
        }
    }
}