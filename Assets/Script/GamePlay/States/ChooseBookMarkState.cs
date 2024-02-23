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
    /// �ͻ��� �� ѡ��籾�ͽ�ɫ�� ���Ĺ�����
    /// </summary>
    public class ChooseBookMarkState : GameStateBehaviour {
        
        [SerializeField] ChoosePanel choosePanel;

        [Header("��ǰ�籾")]
        [SerializeField] BookMarks currentBookMarks;

        public override GameState ActiveState => GameState.FactionSelect;

        protected override void Start() {
            base.Start();
            //choosePanel.InitChoosePanel(currentBookMarks, StartGame, ReturnMenu);

        }

        private void StartGame() {
            if (choosePanel.currentChoosenFaction == null) {
                Debug.Log("����ѡ����ݵĹ���");
            }

            ApplicationController.Instance.SetPlayerFaction(choosePanel.currentChoosenFaction);

            // TODO: д������������ �ݿ����ڣ�����
            //SceneManager.LoadScene("HexLearnScene");
            StartCoroutine(WarSceneManager.Instance.LoadScene("HexLearnScene"));
        }

        private void ReturnMenu() {
            //SceneManager.LoadScene("MenuScene");
            StartCoroutine(WarSceneManager.Instance.LoadScene("MenuScene"));
        }

    }
}