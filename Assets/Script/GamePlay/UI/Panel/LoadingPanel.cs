using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WarGame_True.GamePlay.UI {
    public class LoadingPanel : BasePopUI {

        [SerializeField] private SliderBar sliderBar;
        [SerializeField] private TMP_Text tipText;

        // TODO: ��װһ��չʾ��ǰ�籾��չʾ��ǰѡ������������
        // ��������� ��ʾ���ѡ��ľ籾������

        [Header("��ʾ�ı�")]
        public List<string> tips;
        private int curTipIndex = 0;

        [Header("������ʾ�ı��ļ��")]
        public float changeTime = 0.8f;
        private float changeTipTime = 0.8f;

        [Header("�Ƿ��ڽ������ʱ�Զ��ر�")]
        [Tooltip("����ر�����ֶΣ���ô��Ҫ���رռ��ؽ��棬Ŀǰ��������ͬ��")]
        public bool HideInEnterScene = true;

        private void Awake() {
            //gameObject.SetActive(false);
            if (HideInEnterScene) {
                Hide();
            }
        }

        public void Update() {
            if (Visible)
            {
                changeTipTime -= Time.deltaTime;
                if (changeTipTime <= 0) {
                    changeTipTime = changeTime;
                    // ���ĵ�ǰ����ʾ�ı�
                    curTipIndex++;
                    if(curTipIndex >= tips.Count) {
                        curTipIndex = 0;
                    }
                    tipText.text = tips[curTipIndex];
                }
            }
        }

        public void SetLoadingProcess(float maxProcess, float curProcess) {
            sliderBar.UpdateSliderBar(maxProcess, curProcess, true);
        }

    }
}