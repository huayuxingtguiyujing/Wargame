using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Utils {

    public static class TransformExtension {
        public static void ClearObjChildren(this Transform targetContent) {
            //清空界面的子物体
            Transform[] indicatorChildren = targetContent.GetComponentsInChildren<Transform>();
            foreach (Transform tran in indicatorChildren) {
                if (tran == null) continue;
                if (tran.gameObject == targetContent.gameObject) continue;
                // 思考：要不要把对象池的回收功能放到这里呢？
                GameObject.DestroyImmediate(tran.gameObject);
            }
        }

        public static void ClearObjChildren(this GameObject targetContent) {
            targetContent.transform.ClearObjChildren();
        }
    }
}