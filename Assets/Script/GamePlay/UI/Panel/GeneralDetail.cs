using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.CombatPart;

namespace WarGame_True.GamePlay.UI {
    public class GeneralDetail : BasePopUI {
        
        public void InitGeneralDetail(General general, float YPos) {

            // ��λ����yλ��
            transform.position = new Vector3(transform.position.x, YPos, transform.position.z);

            // ��������ϸ����Ϣ

        }

    }
}