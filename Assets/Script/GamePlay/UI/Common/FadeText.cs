using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

namespace WarGame_True.GamePlay.UI {
    /// <summary>
    /// 支持淡入淡出的文本脚本
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class FadeText : MonoBehaviour {

        [SerializeField] TMP_Text noticeText;

        private float noticeWaitTime = 2.0f;
        private float noticeWaitTimeRec = 2.0f;
        private float noticeFadeTime = 2.0f;
        private float noticeFadeTimeRec = 2.0f;

        public void InitText() {
            noticeText.text = "";
        }

        public void ShowNotice(string content, int colorType) {
            Color textColor = Color.white;
            //noticeText.gameObject.SetActive(true);
            switch (colorType) {
                case 0:
                    textColor = Color.white;
                    break;
                case 1:
                    textColor = Color.black;
                    break;
                case 2:
                    textColor = Color.red;
                    break;
                default:
                    textColor = Color.black;
                    break;
            }
            noticeText.text = content;
            noticeText.color = textColor;

            Debug.Log(content);
            StartCoroutine(NoticeTextFade());
        }


        private IEnumerator NoticeTextFade() {
            // 等待中
            while (noticeWaitTimeRec > 0) {
                noticeWaitTimeRec -= Time.deltaTime;
                yield return null;
            }

            while (noticeFadeTimeRec > 0) {
                noticeFadeTimeRec -= Time.deltaTime;
                // 开始fade
                Color curColor = noticeText.color;
                noticeText.color = new Color(curColor.r, curColor.g, curColor.b, noticeFadeTimeRec / noticeFadeTime);
                yield return null;
            }

            // fade结束 清空字符
            noticeText.text = "";
            noticeWaitTimeRec = noticeWaitTime;
            noticeFadeTimeRec = noticeFadeTime;
        }


    }
}