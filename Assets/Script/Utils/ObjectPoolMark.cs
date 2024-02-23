using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Utils {
    /// <summary>
    /// ���һ��GameObject�Ǳ�����ع���
    /// </summary>
    public class ObjectPoolMark : MonoBehaviour {

        private string prefabName;

        private bool firstSetName = true;
        
        // prefabName ��ʾ����������ʱʹ�õ�prefab�����ƣ��������޸�һ��
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