using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.CombatPart;

namespace WarGame_True.GamePlay.UI {
    /// <summary>
    /// 用于展示将领的信息，可以通过输出不同的初始化参数实现不同的自定义化效果
    /// </summary>
    public class GeneralHolder : MonoBehaviour {

        General thisGeneral;
        
        [Header("基础功能")]
        [SerializeField] Image profileImage;
        [SerializeField] TMP_Text generalName;
        [SerializeField] TMP_Text generalCountryTag;

        [Header("可用的将领功能按钮")]
        [SerializeField] Button profileButton;
        [SerializeField] Button generalButton1;
        [SerializeField] Button generalButton2;
        [SerializeField] Button generalButton3;

        /// <summary>
        /// 初始化将领ui组件
        /// </summary>
        /// <param name="general">传入的将领数据类</param>
        /// <param name="profileButtonEvent">头像按钮事件</param>
        /// <param name="generalButton1Event">按钮1事件，具体参见ui，下同</param>
        public void InitGeneralHolder(General general, UnityAction profileButtonEvent = null,
            UnityAction<General> generalButton1Event = null, UnityAction<General> generalButton2Event = null, UnityAction<General> generalButton3Event = null) {

            thisGeneral = general;

            UpdateGeneral(general);

            if (profileButtonEvent != null && profileButton != null) {
                profileButton.onClick.AddListener(profileButtonEvent);
            }

            if (generalButton1Event != null && generalButton1 != null)
            {
                generalButton1.onClick.AddListener(delegate {
                    generalButton1Event(thisGeneral);
                });
            }

            if (generalButton2Event != null && generalButton2 != null) {
                generalButton2.onClick.AddListener(delegate {
                    generalButton2Event(thisGeneral);
                });
            }

            if (generalButton3Event != null && generalButton3 != null) {
                generalButton3.onClick.AddListener(delegate {
                    generalButton3Event(thisGeneral);
                });
            }
        }

        public async void UpdateGeneral(General general) {
            generalName.text = general.GeneralChineseName;
            generalCountryTag.text = general.GeneralTag;

            try {
                profileImage.sprite = await GeneralProfileHelper.GetProfileAsync(general.GeneralName);
            } catch {
                Debug.LogWarning("加载将领图片时出现问题!");
            }
            
        }
        

    }
}