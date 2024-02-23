using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.CombatPart;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.Politic {
    /// <summary>
    /// 用于区分 势力的关键信息,包括了Tag，名称
    /// </summary>
    [CreateAssetMenu(fileName = "FactionInfo", menuName = "WarGame/FactionInfo", order = 0)]
    public class FactionInfo : GuidScriptableObject {
        public Guid FactionGuid { get => Guid;}

        [Header("派系Tag 唯一")]
        public string FactionTag;
        [Header("派系名称")]
        public string FactionName;
        public string FactionPosition;
        [Header("派系介绍")]
        public string FactionDes;
        [Header("派系颜色")]
        public Color FactionColor;

        //派系领导头像
        public static async Task<Sprite> GetFactionLeaderProfile(string factionTag) {
            //Assets/Resources/Leaders/ZW_Leader.jpg
            Sprite sprite = await Addressables.LoadAssetAsync<Sprite>("Assets/Sprite/Leaders/" + factionTag + "_Leader.jpg").Task;
            if(sprite == null) {
                sprite = await Addressables.LoadAssetAsync<Sprite>("Assets/Sprite/General/Noleader2.png").Task;
            }
            return sprite;
        }

        [Header("派系行为逻辑")]
        public FactionType factionType;

        [Header("派系死敌")]
        public List<FactionInfo> Rivals;
        public bool IsRival(FactionInfo factionInfo) {
            return Rivals.Contains(factionInfo) && factionInfo.Rivals.Contains(this);
        }

        [Header("派系兵种")]
        public List<ArmyUnitData> AbleArmyUnit;

        [Header("派系首都")]
        public string CapitalProvinceID;

        [Header("剧本开始时控制的省份")]
        [Tooltip("输入对应的省份id")]
        public List<string> ProvincesInBeginning;

        [Header("派系将领")]
        public List<GeneralData> FactionGenerals;

        // TODO: 需要提供获取军队人数、税收总数的接口
    }
}