using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Utils {
    /// <summary>
    /// 标记一个GameObject是被对象池管理
    /// </summary>
    public class ObjectPoolMark : MonoBehaviour {

        private string prefabName;

        private bool firstSetName = true;
        
        // prefabName 表示创建该物体时使用的prefab的名称，仅可以修改一次
        public string PrefabName { get => prefabName;
            set {
                if (firstSetName) {
                    firstSetName = false;
                    prefabName = value;
                }
            }
        }

    }
}