using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace WarGame_True.Utils {
    public static class SystemExtension {

        /// <summary>
        /// 随机获取字典中的一对键值对,该字典大小必须大于1
        /// </summary>
        public static KeyValuePair<TKey, TValue> GetRandomPair<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) {
            //NOTICE: 当dictionary为1时，转化成的数组长度为0
            // 此时 不能转化为数组，直接返回仅有的元素
            if (dictionary.Count == 1) {
                foreach (var pair in dictionary)
                {
                    return pair;
                }
            }
            
            
            KeyValuePair<TKey, TValue>[] list = dictionary.ToArray();

            if(list.Length == 1) {
                return list[0];
            }

            try {
                //随机获取一个index
                System.Random random = new System.Random();
                int index = random.Next(list.Length);
                //Debug.Log("the index is:" + index + ",dic count is:" + dictionary.Count);
                return list[index];
            } catch {
                throw new ArgumentOutOfRangeException("索引超出字典范围,或发生其他问题");
            }
            
        }
    }

    public static class StringExtension {

        public static Vector3 TryTransToVector3(this string positionStr) {
            Vector3 errorPosition = Vector3.zero;
            string[] terPos = positionStr.Split("_");

            //检查输入的坐标是否正确
            if (terPos.Length != 3) return errorPosition;

            return new Vector3(
                int.Parse(terPos[0]),
                int.Parse(terPos[1]),
                int.Parse(terPos[2])
            );
        }

    }

    public static class Vector3Extension {
        public static string TransToString(this Vector3 position) {
            
            return position.x.ToString() + "_" + position.y.ToString() + "_" + position.z.ToString();
        }
    }
}