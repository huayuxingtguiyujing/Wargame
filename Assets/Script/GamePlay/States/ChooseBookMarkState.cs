using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.GameState;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.SceneUtil;

namespace WarGame_True.States {
    /// <summary>
    /// 客户端 的 选择剧本和角色的 中心管理器
    /// </summary>
    public class ChooseBookMarkState : GameStateBehaviour {
        
        [SerializeField] ChoosePanel choosePanel;

        [Header("当前剧本")]
        [SerializeField] BookMarks currentBookMarks;

        public override GameState ActiveState => GameState.FactionSelect;

        protected override void Start() {
            base.Start();
            //choosePanel.InitChoosePanel(currentBookMarks, StartGame, ReturnMenu);

        }

        private void StartGame() {
            if (choosePanel.currentChoosenFaction == null) {
                Debug.Log("请先选择扮演的国家");
            }

            ApplicationController.Instance.SetPlayerFaction(choosePanel.currentChoosenFaction);

            // TODO: 写个场景管理器 狠卡现在！！！
            //SceneManager.LoadScene("HexLearnScene");
            StartCoroutine(WarSceneManager.Instance.LoadScene("HexLearnScene"));
        }

        private void ReturnMenu() {
            //SceneManager.LoadScene("MenuScene");
            StartCoroutine(WarSceneManager.Instance.LoadScene("MenuScene"));
        }

    }
}