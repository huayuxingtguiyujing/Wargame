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
    /// ����չʾ�������Ϣ������ͨ�������ͬ�ĳ�ʼ������ʵ�ֲ�ͬ���Զ��廯Ч��
    /// </summary>
    public class GeneralHolder : MonoBehaviour {

        General thisGeneral;
        
        [Header("��������")]
        [SerializeField] Image profileImage;
        [SerializeField] TMP_Text generalName;
        [SerializeField] TMP_Text generalCountryTag;

        [Header("���õĽ��칦�ܰ�ť")]
        [SerializeField] Button profileButton;
        [SerializeField] Button generalButton1;
        [SerializeField] Button generalButton2;
        [SerializeField] Button generalButton3;

        /// <summary>
        /// ��ʼ������ui���
        /// </summary>
        /// <param name="general">����Ľ���������</param>
        /// <param name="profileButtonEvent">ͷ��ť�¼�</param>
        /// <param name="generalButton1Event">��ť1�¼�������μ�ui����ͬ</param>
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
                Debug.LogWarning("���ؽ���ͼƬʱ��������!");
            }
            
        }
        

    }
}