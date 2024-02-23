using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WarGame_True.GamePlay.UI {
    public class LoadingPanel : BasePopUI {

        [SerializeField] private SliderBar sliderBar;
        [SerializeField] private TMP_Text tipText;

        // TODO: 封装一个展示当前剧本、展示当前选择的势力的组件
        // 到这里调用 显示玩家选择的剧本和势力

        [Header("提示文本")]
        public List<string> tips;
        private int curTipIndex = 0;

        [Header("更改提示文本的间隔")]
        public float changeTime = 0.8f;
        private float changeTipTime = 0.8f;

        [Header("是否在进入界面时自动关闭")]
        [Tooltip("如果关闭这个字段，那么需要外界关闭加载界面，目前用于网络同步")]
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
                    // 更改当前的提示文本
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