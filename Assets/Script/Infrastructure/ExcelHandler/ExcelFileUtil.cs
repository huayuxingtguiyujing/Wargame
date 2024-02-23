using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace WarGame_True.Infrastructure.Excel {
    /// <summary>
    /// �ṩһЩ�ļ�����������װ��excel�Ĳ������Ƿ���excelHandler�
    /// </summary>
    public class ExcelFileUtil {
        

        public static FileInfo[] GetFileByPath(string path) {

            FileInfo[] ans = new FileInfo[0];
            if (Directory.Exists(path)) {

                DirectoryInfo direction = new DirectoryInfo(path);

                //��ȡָ��·�������������Դ�ļ�  
                FileInfo[] files = direction.GetFiles("*");
                for (int i = 0; i < files.Length; i++) {
                    //���Թ����ļ�
                    if (files[i].Name.EndsWith(".meta")) {
                        continue;
                    }
                    Debug.Log("�ļ�:" + files[i].Name + "_" + files[i].FullName + "_" + files[i].DirectoryName);
                }

                // ɸѡ����ص��ļ�,�����޹��ļ�
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