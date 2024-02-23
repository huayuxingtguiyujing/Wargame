using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    /// <summary>
    /// ������ui��������ʾһ������ֵ�����ľ������
    /// </summary>
    public class DetailTextPop : BasePopUI {

        [SerializeField] Transform ItemParent;
        [SerializeField] GameObject TextItemPrefab;

        public void InitDetailPop(DetailMessage detailMessage) {
            if (!gameObject.activeSelf) {
                gameObject.SetActive(true);
            }

            if (!Visible) {
                Show();
            }

            ItemParent.ClearObjChildren();

            if (detailMessage.HasStart) {
                CreateTextItemObject(
                    detailMessage.StartItem, 
                    detailMessage.StartItem.Value.ToString(), 
                    detailMessage.StartItem.ValueColorType
                );
            }

            foreach (DetailText detailText in detailMessage.Details)
            {
                // ��ȡ������ֵ
                float ValueDes = detailText.Value;
                
                string PercentSymbol = "";
                // �Ƿ���Ҫ��Ӱٷֺ�
                if (detailText.UsePercent) {
                    ValueDes = Mathf.Round(detailText.Value * 100);
                    PercentSymbol = "%";
                }

                string ARSymbol = "";
                // �Ƿ���Ҫ��� "+" "-" ����
                if (detailText.UseAddReduceSymbol)
                {
                    if(detailText.Value > 0) {
                        ARSymbol = "+";
                    } else if(detailText.Value < 0) {
                        ARSymbol = "";
                    }
                }
                
                string ValueText = ARSymbol + ValueDes.ToString() + PercentSymbol;
                //Debug.Log("��ǰ��������ֵ: " + ValueText);

                // ����ui����
                CreateTextItemObject(detailText, ValueText, detailText.ValueColorType);
            }

            if (detailMessage.HasEnd) {
                CreateTextItemObject(
                    detailMessage.EndItem,
                    detailMessage.EndItem.Value.ToString(), 
                    detailMessage.EndItem.ValueColorType
                );
            }

        }

        public void ClearDetailPop() {
            ItemParent.ClearObjChildren();
        }

        /// <summary>
        /// ����һ��TextItemObject
        /// </summary>
        /// <param name="detailText"></param>
        /// <param name="ValueDesc"></param>
        /// <param name="colorType"></param>
        private void CreateTextItemObject(DetailText detailText, string ValueDesc, int valueColor) {
            GameObject textItemObject = Instantiate(TextItemPrefab, ItemParent);
            TextItem textItem = textItemObject.GetComponent<TextItem>();
            textItem.InitTextItem(detailText.Description, ValueDesc, valueColor);
            //float 
            //textItem.GetComponent<RectTransform>().sizeDelta = new Vector2();
            //Debug.Log("�������ı�����: " + detailText.Description);
        }

    }
}