using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Utils {

    public static class TransformExtension {
        public static void ClearObjChildren(this Transform targetContent) {
            //��ս����������
            Transform[] indicatorChildren = targetContent.GetComponentsInChildren<Transform>();
            foreach (Transform tran in indicatorChildren) {
                if (tran == null) continue;
                if (tran.gameObject == targetContent.gameObject) continue;
                // ˼����Ҫ��Ҫ�Ѷ���صĻ��չ��ܷŵ������أ�
                GameObject.DestroyImmediate(tran.gameObject);
            }
        }

        public static void ClearObjChildren(this GameObject targetContent) {
            targetContent.transform.ClearObjChildren();
        }
    }
}