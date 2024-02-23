using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    /// <summary>
    /// 弹出的ui，用于显示一个属性值修正的具体组成
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
                // 获取该修正值
                float ValueDes = detailText.Value;
                
                string PercentSymbol = "";
                // 是否需要添加百分号
                if (detailText.UsePercent) {
                    ValueDes = Mathf.Round(detailText.Value * 100);
                    PercentSymbol = "%";
                }

                string ARSymbol = "";
                // 是否需要添加 "+" "-" 符号
                if (detailText.UseAddReduceSymbol)
                {
                    if(detailText.Value > 0) {
                        ARSymbol = "+";
                    } else if(detailText.Value < 0) {
                        ARSymbol = "";
                    }
                }
                
                string ValueText = ARSymbol + ValueDes.ToString() + PercentSymbol;
                //Debug.Log("当前该描述的值: " + ValueText);

                // 创建ui物体
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
        /// 创建一个TextItemObject
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
            //Debug.Log("创建了文本物体: " + detailText.Description);
        }

    }
}