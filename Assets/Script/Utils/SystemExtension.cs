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
        /// �����ȡ�ֵ��е�һ�Լ�ֵ��,���ֵ��С�������1
        /// </summary>
        public static KeyValuePair<TKey, TValue> GetRandomPair<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) {
            //NOTICE: ��dictionaryΪ1ʱ��ת���ɵ����鳤��Ϊ0
            // ��ʱ ����ת��Ϊ���飬ֱ�ӷ��ؽ��е�Ԫ��
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
                //�����ȡһ��index
                System.Random random = new System.Random();
                int index = random.Next(list.Length);
                //Debug.Log("the index is:" + index + ",dic count is:" + dictionary.Count);
                return list[index];
            } catch {
                throw new ArgumentOutOfRangeException("���������ֵ䷶Χ,������������");
            }
            
        }
    }

    public static class StringExtension {

        public static Vector3 TryTransToVector3(this string positionStr) {
            Vector3 errorPosition = Vector3.zero;
            string[] terPos = positionStr.Split("_");

            //�������������Ƿ���ȷ
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