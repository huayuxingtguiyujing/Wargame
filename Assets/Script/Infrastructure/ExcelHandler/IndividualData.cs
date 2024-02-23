using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.Excel {
    public class IndividualData {
        public string[] Values;
        public IndividualData(int Columns) {
            Values = new string[Columns];
        }

    }
}