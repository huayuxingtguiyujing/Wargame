using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.GamePlay.Politic {
    /// <summary>
    /// װ�� Bookmark ��Ϣ������Ϸ��
    /// </summary>
    public class PoliticLoader : MonoBehaviour {

        public static PoliticLoader Instance;

        [SerializeField] PoliticNetworkLoader politicNetwork;

        [Header("��ǰ�籾")]
        public BookMarks bookMarks;

        [Header("��ǰ����Ĺ���")]
        private Faction playerFaction;
        public Faction PlayerFaction {
            get {
                if(playerFaction == null) {
                    SetPlayerFaction(ApplicationController.PlayerFaction);
                }
                return playerFaction;
            }
            private set {
                playerFaction = value;
            }
        }
        public void SetPlayerFaction(FactionInfo factionInfo) {
            // ����factioninfo
            foreach (Faction faction in BookMarkFactions)
            {
                if (faction.FactionInfo == factionInfo) {
                    PlayerFaction = faction;
                }
            }
        }

        [Header("������ص����UI")]
        [SerializeField] private PoliticPanel politicPanel;
        [SerializeField] private FactionPanel factionPanel;
        [SerializeField] private MapPanel mapPanel;


        [Header("��ǰ��������")]
        public List<Faction> BookMarkFactions;
        
        public void InitPolitic(Transform factionParent) {
            //����ģʽ
            if(Instance == null) {
                Instance = this;
            }

            BookMarkFactions = new List<Faction>();

            foreach (FactionInfo factionInfo in bookMarks.BookMarkFactionInfo)
            {
                // ����factioninfo ����faction
                Faction faction = Faction.GetBaseFaction(factionInfo, factionParent);
                BookMarkFactions.Add(faction);
                faction.InitFaction();
            }

            foreach (Faction faction in BookMarkFactions)
            {
                foreach (Faction otherFaction in BookMarkFactions)
                {
                    //��ʼ���⽻��ϵ
                    faction.AddDiploRelation(faction, otherFaction);
                }
            }

            //��������ui���
            politicPanel.InitPoliticPanel(BookMarkFactions);

            // ��ͼ
            mapPanel.InitMapPanel();

            //PlayerFaction = BookMarkFactions[0];
            // TODO���ͻ���û������PlayerFaction��
            PlayerFaction = GetFactionByInfo(ApplicationController.PlayerFaction);

            // ��ʼ��������������
            factionPanel.InitFactionPanel(PlayerFaction);
        }


        #region ��ȡ�������������Ϣ
        /// <summary>
        /// ���ݸ�����tag��Ѱ�Ҷ�Ӧ�Ĺ�������
        /// </summary>
        public Faction GetFactionByTag(string tag) {
            foreach (Faction faction in BookMarkFactions) {
                if (faction.FactionInfo.FactionTag == tag) {
                    return faction;
                }
            }
            Debug.Log("������ָ��tag�Ĺ���:" + tag);
            return null;
        }

        public Faction GetFactionByInfo(FactionInfo factionInfo) {
            foreach (var faction in BookMarkFactions) {
                //Debug.Log(factionInfo == null);
                //Debug.Log(factionInfo.FactionGuid == null);
                if (factionInfo.FactionGuid == faction.FactionInfo.FactionGuid) {
                    return faction;
                }
            }
            return null;
        }

        /// <summary>
        /// ���ݸ�����tag����������tag֮����⽻��ϵ
        /// </summary>
        public DiploRelation[] GetDiploRelationByTag(string tag1, string tag2) {
            //Debug.Log("the tag1 is: " + tag1 + ", the tag2 is: " + tag2);
            DiploRelation[] diploRelations = new DiploRelation[2];

            diploRelations[0] = GetFactionByTag(tag1).GetDiploRelation(tag2);
            diploRelations[1] = GetFactionByTag(tag2).GetDiploRelation(tag1);
            return diploRelations;
        }

        public DiploRelation[] GetFactionDiploRelation(string tag) {
            return GetFactionByTag(tag).GetAllDiploRelations().ToArray();
        }

        public bool IsFactionInWar(string tag1, string tag2) {
            if (tag1 == tag2) return false;

            try {
                DiploRelation[] rec = GetDiploRelationByTag(tag1, tag2);
                return rec[0].IsInwar && rec[1].IsInwar;
            } catch {
                // ����һ����Ϊ����ս����
                return false;
            }

        }

        public Color GetFactionColor(string tag) {
            try {
                return GetFactionByTag(tag).FactionInfo.FactionColor;
            } catch {
                //Debug.Log(tag);
                //Debug.Log(GetFactionByTag(tag) == null);
                return new Color(1, 1, 1, 0.6f);
            }

        }

        #endregion

        #region ����ͬ�� ����������Ϣ

        #endregion

    }
}