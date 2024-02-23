using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.Infrastructure.NetworkPackage;

namespace WarGame_True.Utils {
    public class GuidObject {

        [HideInInspector, SerializeField]
        private string m_Guid;

        public Guid Guid {
            get {
                return new Guid(m_Guid);
            }
        }

        public void InitThisUnitGuid() {
            if (m_Guid == null) {
                m_Guid = "";
            }

            //新生成一个guid
            m_Guid = System.Guid.NewGuid().ToString();

            //Debug.Log(m_Guid.ToString());
        }

        public void InitThisUnitGuid(NetworkGuid networkGuid){
            m_Guid = networkGuid.ToGuid().ToString();
        }
    }
}