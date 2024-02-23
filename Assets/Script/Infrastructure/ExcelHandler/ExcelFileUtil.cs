using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace WarGame_True.Infrastructure.Excel {
    /// <summary>
    /// 提供一些文件操作方法封装（excel的操作还是放在excelHandler里）
    /// </summary>
    public class ExcelFileUtil {
        

        public static FileInfo[] GetFileByPath(string path) {

            FileInfo[] ans = new FileInfo[0];
            if (Directory.Exists(path)) {

                DirectoryInfo direction = new DirectoryInfo(path);

                //获取指定路径下面的所有资源文件  
                FileInfo[] files = direction.GetFiles("*");
                for (int i = 0; i < files.Length; i++) {
                    //忽略关联文件
                    if (files[i].Name.EndsWith(".meta")) {
                        continue;
                    }
                    Debug.Log("文件:" + files[i].Name + "_" + files[i].FullName + "_" + files[i].DirectoryName);
                }

                // 筛选出相关的文件,忽略无关文件
                var rec = from file in files
                where !file.Name.EndsWith(".meta")
                select file;
                ans = rec.ToArray();
            }

            return ans;
        }

        

        //public static FileClass[] GetFileClassByPath<FileClass>(string path) {
        //    FileInfo[] fileInfos = GetFileByPath(path);
        //}

    }
}