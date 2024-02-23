using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Utils {
    public class ObjectPool {

        #region 单例模式
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

        // 对象池:包含在对象池的物体是可复用的
        private Dictionary<string, List<ObjectPoolMark>> pool;

        /// <summary>
        /// 从对象池获取物体
        /// </summary>
        /// <remarks>
        /// 该函数用于代替Instantiate函数，使用函数时，输入的预制件不能重名
        /// </remarks>
        public GameObject GetObject(GameObject prefabObject, Transform parentTransform, Vector3 position, Quaternion quaternion) {
            string prefabName = prefabObject.name;
            GameObject result = null;

            if (pool.ContainsKey(prefabName)) {
                if (pool[prefabName] == null) {
                    pool[prefabName] = new List<ObjectPoolMark>();
                } else if (pool[prefabName].Count > 0) {
                    // 对象池中含有该对象的物体，则取出复用
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

            // 为生成的游戏物体挂接上标记脚本，设置标记脚本的prefabName
            result.AddComponent<ObjectPoolMark>();
            result.GetComponent<ObjectPoolMark>().PrefabName = prefabName;

            return result;
        }

        public GameObject GetObject(GameObject prefabObject, Transform parentTransform) {
            return GetObject(prefabObject, parentTransform, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// 回收物体
        /// </summary>
        /// <remarks>
        /// 该函数用于代替Destroy函数
        /// </remarks>
        public void RecycleObject(GameObject obj) {
            if (obj.GetComponent<ObjectPoolMark>() == null) {
                // 未挂接对象池标记
                return;
            }

            obj.SetActive(false);
            ObjectPoolMark objectPoolMark = obj.GetComponent<ObjectPoolMark>();

            // 回收该物体到线程池中
            if (pool.ContainsKey(objectPoolMark.PrefabName)) {
                pool[objectPoolMark.PrefabName].Add(objectPoolMark);
            } else {
                pool.Add(objectPoolMark.PrefabName, new List<ObjectPoolMark>() { objectPoolMark });
            }

        }

    }
}