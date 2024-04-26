using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.HexagonGrid.MapObject {
    [System.Serializable]
    public class River {

        public List<HexGrid> riverPaths;

        public River(List<HexGrid> _riverPaths) {
            int count = _riverPaths.Count;
            if(_riverPaths == null || count < 2) {
                Debug.LogError("river path not valid!");
                return;
            }

            this.riverPaths = _riverPaths;

            // 设置起点
            int i = 0;
            riverPaths[0].SetRiverOutPath(riverPaths[1]);

            for (i = 1; i < riverPaths.Count - 1; i ++) {
                riverPaths[i].SetRiverInPath(riverPaths[i - 1]);
                riverPaths[i].SetRiverOutPath(riverPaths[i + 1]);
            }

            // 设置终点
            riverPaths[count - 1].SetRiverOutPath(riverPaths[count - 2]);

        }

    }
}