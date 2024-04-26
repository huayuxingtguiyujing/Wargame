using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.Infrastructure.DebugPack {
    public class FactionCommand : Command {

        string factionTag;

        bool IsHandleAI;
        bool EnableAI;

        public FactionCommand(string factionTag, bool isHandleAI, bool enableAI) {
            this.factionTag = factionTag;
            IsHandleAI = isHandleAI;
            EnableAI = enableAI;
            //Debug.Log($"faction command init, handleAI: {isHandleAI}, enableAI: {enableAI}");
        }

        public override void Excute() {
            //Debug.Log("excute faction command");
            HandleFactionAI(EnableAI);
        }

        private void HandleFactionAI(bool EnableAI) {
            if(!IsHandleAI) {
                return;   
            }

            if(EnableAI) {
                PoliticLoader.Instance.EnableAI(factionTag);
            } else {
                PoliticLoader.Instance.DisableAI(factionTag);
            }
        }

    }

    public enum FactionCommandType {
        EnableAI,
        DisableAI,
    }
}