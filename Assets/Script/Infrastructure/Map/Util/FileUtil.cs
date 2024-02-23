
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using Newtonsoft.Json;
using UnityEngine;

namespace WarGame_True.Infrastructure.Map.Util {
    public class FileUtil {

        #region Json文件处理
        ///<summary>
        /// 将数据写入json文件，如果文件路径不存在则创建文件
        ///</summary>
        public void WriteJsonFile<T>(string filePath, T data) {
            //string recString = JsonConvert.SerializeObject(data);

            //File.WriteAllText(filePath, recString);
        }

        public string ReadJsonFile(string filePath) {
            //Debug.Log("filepath:" + filePath);
            return File.ReadAllText(filePath);
        }

        public void DeleteJsonFile(string filePath) {
            //Debug.Log("filepath:" + filePath);
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
        }

        public void RenameJsonFile<T>(string filePath, string newFilePath, T data) {
            if (File.Exists(filePath)) {
                File.Move(filePath, newFilePath);
                WriteJsonFile(newFilePath, data);
            }
        }
        #endregion

    }
}