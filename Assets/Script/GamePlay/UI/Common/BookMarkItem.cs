using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.GamePlay.UI {
    public class BookMarkItem : MonoBehaviour {

        private BookMarks m_BookMarks;
        [Header("UI���")]
        [SerializeField] Button itemButton;
        [SerializeField] TMP_Text bookmarkName;
        [SerializeField] TMP_Text bookmarkTime;

        public void InitBookMark(BookMarks bookMarks) {
            //TODO�����֮��ˢ�µ�ͼ��ˢ�¾籾����
            //itemButton.onClick.AddListener();

            m_BookMarks = bookMarks;

            bookmarkName.text = bookMarks.bookMarkName;
            bookmarkTime.text = bookMarks.startTime;
        }
    }
}