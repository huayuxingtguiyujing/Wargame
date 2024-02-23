using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Application;

namespace WarGame_True.GamePlay.Politic {
    /// <summary>
    /// 历史剧本，包含该历史时期所有的势力、经济状况etc，用于初始化游戏
    /// </summary>
    [CreateAssetMenu(fileName = "BookMarks", menuName = "WarGame/BookMarks")]
    public class BookMarks : ScriptableObject {
        [Header("剧本起始-结束年代")]
        public string startTime;
        public string endTime;

        [Header("剧本介绍")]
        public string bookMarkName;
        public string bookMarkNotice = "";

        [Header("该剧本中的所有势力")]
        public List<FactionInfo> BookMarkFactionInfo;

        [Header("推荐游玩")]
        public List<FactionInfo> RecommandFactionInfo;
    }
}