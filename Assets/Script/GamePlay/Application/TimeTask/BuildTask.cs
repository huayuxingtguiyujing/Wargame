using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.GamePlay.Application.TimeTask {
    public class BuildTask : BaseTask {

        public Building CurBuilding;

        public BuildTask(Building building) : base(TaskType.Day, (uint)building.CostTime) {
            CurBuilding = building;
        }
    }
}