using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.UI;

namespace WarGame_True.GamePlay.Politic {
    /// <summary>
    /// 装载 Bookmark 信息进入游戏中
    /// </summary>
    public class PoliticLoader : MonoBehaviour {

        public static PoliticLoader Instance;

        [SerializeField] PoliticNetworkLoader politicNetwork;

        [Header("当前剧本")]
        public BookMarks bookMarks;

        [Header("当前游玩的国家")]
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
            // 根据factioninfo
            foreach (Faction faction in BookMarkFactions)
            {
                if (faction.FactionInfo == factionInfo) {
                    PlayerFaction = faction;
                }
            }
        }

        [Header("政治相关的面板UI")]
        [SerializeField] private PoliticPanel politicPanel;
        [SerializeField] private FactionPanel factionPanel;
        [SerializeField] private MapPanel mapPanel;


        [Header("当前的势力表")]
        public List<Faction> BookMarkFactions;
        
        public void InitPolitic(Transform factionParent) {
            //单例模式
            if(Instance == null) {
                Instance = this;
            }

            BookMarkFactions = new List<Faction>();

            foreach (FactionInfo factionInfo in bookMarks.BookMarkFactionInfo)
            {
                // 根据factioninfo 建立faction
                Faction faction = Faction.GetBaseFaction(factionInfo, factionParent);
                BookMarkFactions.Add(faction);
                faction.InitFaction();
            }

            foreach (Faction faction in BookMarkFactions)
            {
                foreach (Faction otherFaction in BookMarkFactions)
                {
                    //初始化外交关系
                    faction.AddDiploRelation(faction, otherFaction);
                }
            }

            //政治势力ui面板
            politicPanel.InitPoliticPanel(BookMarkFactions);

            // 地图
            mapPanel.InitMapPanel();

            //PlayerFaction = BookMarkFactions[0];
            // TODO：客户端没有设置PlayerFaction！
            PlayerFaction = GetFactionByInfo(ApplicationController.PlayerFaction);

            // 初始化国家势力界面
            factionPanel.InitFactionPanel(PlayerFaction);
        }


        #region 获取国家势力相关信息
        /// <summary>
        /// 根据给定的tag，寻找对应的国家势力
        /// </summary>
        public Faction GetFactionByTag(string tag) {
            foreach (Faction faction in BookMarkFactions) {
                if (faction.FactionInfo.FactionTag == tag) {
                    return faction;
                }
            }
            Debug.Log("不存在指定tag的国家:" + tag);
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
        /// 根据给定的tag，返回两个tag之间的外交关系
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
                // 出错，一律认为不在战争中
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

        #region 网络同步 各个势力信息

        #endregion

    }
}