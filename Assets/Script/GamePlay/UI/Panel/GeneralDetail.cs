using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.CombatPart;

namespace WarGame_True.GamePlay.UI {
    public class GeneralDetail : BasePopUI {
        
        public void InitGeneralDetail(General general, float YPos) {

            // 定位面板的y位置
            transform.position = new Vector3(transform.position.x, YPos, transform.position.z);

            // 创建将领细节信息

        }

    }
}