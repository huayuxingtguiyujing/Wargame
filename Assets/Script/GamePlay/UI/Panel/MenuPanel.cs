using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WarGame_True.GamePlay.Politic;
using DG;
using DG.Tweening;

namespace WarGame_True.GamePlay.UI {
    public class MenuPanel : MonoBehaviour {

        [Header("ui��Ч-����")]
        [SerializeField] Transform BGText;
        [SerializeField] float TextMoveTime = 20;
        [SerializeField] float TextMoveTarget = 1000;

        [Header("���˵��İ�ť")]
        [SerializeField] Button developButton;
        [SerializeField] Button singlePlayButton;
        [SerializeField] Button multiPlayButton;
        [SerializeField] Button exitgameButton;

        public void InitMenuPanel(UnityAction singlePlayAction, UnityAction multiPlayAction) {
            
            developButton.onClick.AddListener(singlePlayAction);
            singlePlayButton.onClick.AddListener(singlePlayAction);
            multiPlayButton.onClick.AddListener(multiPlayAction);
            exitgameButton.onClick.AddListener(ExitGame);

            // ui��Ч
            BGText.DOMoveY(TextMoveTarget, TextMoveTime);
        }


        public void ExitGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }


    }
}