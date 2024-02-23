using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WarGame_True.Infrastructure.HexagonGrid.MapObject;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.Infrastructure.HexagonGrid.Utils {
    public class HexHelper {

        #region µ¥ÀýÄ£Ê½
        private static HexHelper instance;
        public HexHelper GetHexHelper() {
            if (instance == null) instance = new HexHelper();
            return instance;
        }
        public HexHelper() { }

        #endregion

        


    }
}