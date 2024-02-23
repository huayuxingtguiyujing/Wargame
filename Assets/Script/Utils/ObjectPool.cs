using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Utils {
    public class ObjectPool {

        #region ����ģʽ
        private static ObjectPool instance;
        public static ObjectPool GetInstance() {
            if(instance == null) {
                instance = new ObjectPool();
            }
            return instance;
        }
        public ObjectPool() {
            pool = new Dictionary<string, List<ObjectPoolMark>>();
        }
        #endregion

        // �����:�����ڶ���ص������ǿɸ��õ�
        private Dictionary<string, List<ObjectPoolMark>> pool;

        /// <summary>
        /// �Ӷ���ػ�ȡ����
        /// </summary>
        /// <remarks>
        /// �ú������ڴ���Instantiate������ʹ�ú���ʱ�������Ԥ�Ƽ���������
        /// </remarks>
        public GameObject GetObject(GameObject prefabObject, Transform parentTransform, Vector3 position, Quaternion quaternion) {
            string prefabName = prefabObject.name;
            GameObject result = null;

            if (pool.ContainsKey(prefabName)) {
                if (pool[prefabName] == null) {
                    pool[prefabName] = new List<ObjectPoolMark>();
                } else if (pool[prefabName].Count > 0) {
                    // ������к��иö�������壬��ȡ������
                    result = pool[prefabName][0].gameObject;
                    result.transform.parent = parentTransform;
                    result.transform.localPosition = position;
                    result.transform.localRotation = quaternion;
                    return result;
                }
            } else {
                pool.Add(prefabName, new List<ObjectPoolMark>());
            }

            result = GameObject.Instantiate(prefabObject, parentTransform);
            result.transform.localPosition = position;
            result.transform.localRotation = quaternion;

            // Ϊ���ɵ���Ϸ����ҽ��ϱ�ǽű������ñ�ǽű���prefabName
            result.AddComponent<ObjectPoolMark>();
            result.GetComponent<ObjectPoolMark>().PrefabName = prefabName;

            return result;
        }

        public GameObject GetObject(GameObject prefabObject, Transform parentTransform) {
            return GetObject(prefabObject, parentTransform, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <remarks>
        /// �ú������ڴ���Destroy����
        /// </remarks>
        public void RecycleObject(GameObject obj) {
            if (obj.GetComponent<ObjectPoolMark>() == null) {
                // δ�ҽӶ���ر��
                return;
            }

            obj.SetActive(false);
            ObjectPoolMark objectPoolMark = obj.GetComponent<ObjectPoolMark>();

            // ���ո����嵽�̳߳���
            if (pool.ContainsKey(objectPoolMark.PrefabName)) {
                pool[objectPoolMark.PrefabName].Add(objectPoolMark);
            } else {
                pool.Add(objectPoolMark.PrefabName, new List<ObjectPoolMark>() { objectPoolMark });
            }

        }

    }
}