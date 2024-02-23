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
    /// �������� �����Ĺؼ���Ϣ,������Tag������
    /// </summary>
    [CreateAssetMenu(fileName = "FactionInfo", menuName = "WarGame/FactionInfo", order = 0)]
    public class FactionInfo : GuidScriptableObject {
        public Guid FactionGuid { get => Guid;}

        [Header("��ϵTag Ψһ")]
        public string FactionTag;
        [Header("��ϵ����")]
        public string FactionName;
        public string FactionPosition;
        [Header("��ϵ����")]
        public string FactionDes;
        [Header("��ϵ��ɫ")]
        public Color FactionColor;

        //��ϵ�쵼ͷ��
        public static async Task<Sprite> GetFactionLeaderProfile(string factionTag) {
            //Assets/Resources/Leaders/ZW_Leader.jpg
            Sprite sprite = await Addressables.LoadAssetAsync<Sprite>("Assets/Sprite/Leaders/" + factionTag + "_Leader.jpg").Task;
            if(sprite == null) {
                sprite = await Addressables.LoadAssetAsync<Sprite>("Assets/Sprite/General/Noleader2.png").Task;
            }
            return sprite;
        }

        [Header("��ϵ��Ϊ�߼�")]
        public FactionType factionType;

        [Header("��ϵ����")]
        public List<FactionInfo> Rivals;
        public bool IsRival(FactionInfo factionInfo) {
            return Rivals.Contains(factionInfo) && factionInfo.Rivals.Contains(this);
        }

        [Header("��ϵ����")]
        public List<ArmyUnitData> AbleArmyUnit;

        [Header("��ϵ�׶�")]
        public string CapitalProvinceID;

        [Header("�籾��ʼʱ���Ƶ�ʡ��")]
        [Tooltip("�����Ӧ��ʡ��id")]
        public List<string> ProvincesInBeginning;

        [Header("��ϵ����")]
        public List<GeneralData> FactionGenerals;

        // TODO: ��Ҫ�ṩ��ȡ����������˰�������Ľӿ�
    }
}