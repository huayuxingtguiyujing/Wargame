using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Application;

namespace WarGame_True.GamePlay.Politic {
    /// <summary>
    /// ��ʷ�籾����������ʷʱ�����е�����������״��etc�����ڳ�ʼ����Ϸ
    /// </summary>
    [CreateAssetMenu(fileName = "BookMarks", menuName = "WarGame/BookMarks")]
    public class BookMarks : ScriptableObject {
        [Header("�籾��ʼ-�������")]
        public string startTime;
        public string endTime;

        [Header("�籾����")]
        public string bookMarkName;
        public string bookMarkNotice = "";

        [Header("�þ籾�е���������")]
        public List<FactionInfo> BookMarkFactionInfo;

        [Header("�Ƽ�����")]
        public List<FactionInfo> RecommandFactionInfo;
    }
}