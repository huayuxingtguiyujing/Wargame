using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

namespace WarGame_True.Utils {
    /// <summary>
    /// 存储类似于："生命值 +20%" 的数据类型
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class DetailMessage {
        // 描述的开始项
        public DetailText StartItem;
        public bool HasStart { get => StartItem != null; }
        
        // 描述的中间细节部分
        public List<DetailText> Details;
        public bool FindKeyInDetails(string Desc) {
            return FindDetails(Desc) != null;
        }

        public DetailText FindDetails(string Desc) {
            int index = FindDetailsIndex(Desc);
            if (index != -1) {
                return Details[index];
            } else {
                return null;
            }
        }

        public int FindDetailsIndex(string Desc) {
            for (int i = 0; i < Details.Count; i++) {
                if (Details[i].Description.Equals(Desc)) {
                    return i;
                }
            }
            return -1;
        }

        // 描述的结束项
        public DetailText EndItem;
        public bool HasEnd { get => EndItem != null; }


        public int Count { get => Details.Count; }
        public DetailMessage() {
            Details = new List<DetailText>();
        }

        // 增加描述项
        public void AddMessage(string description, float value, bool usePercent = true, bool useARSymbol = true, 
            int desColor = 4, int valueColor = 4, string preDesc = "") {
            if(Details == null) {
                Details = new List<DetailText>();
            }

            int index = FindDetailsIndex(description);
            if (index == -1) {
                // 没有添加过该键值
                //Debug.Log("have not add this key: " + description);
                Details.Add(
                    new DetailText(description, value, usePercent, useARSymbol, desColor, valueColor, preDesc)
                );
            } else {
                //Debug.Log("have add this key: " + description);
                // 若已经存在于 本数据中，则用新的覆盖掉
                Details[index] = new DetailText(description, value, usePercent, useARSymbol, desColor, valueColor, preDesc);
            }
                
        }

        public void AddMessage(string preDesc, DetailMessage detailMes) {
            if (Details == null) {
                Details = new List<DetailText>();
            }

            foreach (var detail in detailMes.Details)
            {
                // 设置大类描述
                detail.PreDesc = preDesc;
                int index = FindDetailsIndex(detail.Description);
                if (index == -1) {
                    Details.Add(detail);
                } else {
                    // 若已经存在于 本数据中，则用新的覆盖掉
                    Details[index] = detail;
                }
            }
            
        }

        public void SetStart(string description, float value, bool usePercent = true, bool useARSymbol = true, int desColor = 4, int valueColor = 4, string preDesc = "") {
            StartItem = new DetailText(
                description, value, usePercent, useARSymbol,
                desColor, valueColor, preDesc
            );
        }

        public void SetEnd(string description, float value, bool usePercent = true, bool useARSymbol = true, int desColor = 4, int valueColor = 4, string preDesc = "") {
            EndItem = new DetailText(
                description, value, usePercent, useARSymbol,
                desColor, valueColor, preDesc
            );
        }
    }

    /// <summary>
    /// 用于表示类似于："生命值 +20%" 的信息类型
    /// "Description : value"
    /// </summary>
    public class DetailText {

        public string PreDesc = "";             // 描述 该描述不作为键，用于作为大类分类

        public string Description;              // 描述 该描述在DetailTextMessage中作为键 ( 唯一！)

        public float Value;

        public bool UsePercent;                 // 使用百分数表示值

        public bool UseAddReduceSymbol;         // 值为true时，表示会根据value的正负添加 + - 号

        public int DescripColorType;       
        public Color DescripColor;              // 描述文字的颜色 0 -白色, 1 -绿色, 2 -红色, 3 -黑色

        public int ValueColorType;       
        public Color ValueColor;                // value的颜色

        public DetailText(string description, float value, string preDesc = "") {
            Description = description;
            this.Value = value;
            PreDesc = preDesc;

            this.UsePercent = true;
            this.UseAddReduceSymbol = true;

            // 默认字体为黑色
            this.DescripColorType = 4;
            this.ValueColorType = 4;

            SetDescValueColor(UseAddReduceSymbol, DescripColorType, ValueColorType);
        }

        public DetailText(string description, float value, bool usePercent, bool useAddReduceSymbol, 
            int descripColorType, int valueColorType, string preDesc) {
            Description = description;
            this.Value = value;
            PreDesc = preDesc;

            this.UsePercent = usePercent;
            this.UseAddReduceSymbol = useAddReduceSymbol;
            this.DescripColorType = descripColorType;
            this.ValueColorType = valueColorType;

            SetDescValueColor(useAddReduceSymbol, descripColorType, valueColorType);
        }

        /// <summary>
        /// 用于设置该文本 Desc : Value 的字体颜色
        /// </summary>
        private void SetDescValueColor(bool useAddReduceSymbol, int descripColorType, int valueColorType) {
            // 根据输入的 descripColorType 确定文字的颜色
            switch (descripColorType) {
                case 0:
                    this.DescripColor = Color.white;
                    break;
                case 1:
                    this.DescripColor = Color.green;
                    break;
                case 2:
                    this.DescripColor = Color.red;
                    break;
                case 3:
                    this.DescripColor = Color.yellow;
                    break;
                default:
                    this.DescripColor = Color.black;
                    break;
            }

            // 根据输入的 valueColorType 确定value文字的颜色
            switch (valueColorType) {
                case 0:
                    this.ValueColor = Color.white;
                    break;
                case 1:
                    this.ValueColor = Color.green;
                    break;
                case 2:
                    this.ValueColor = Color.red;
                    break;
                case 3:
                    this.DescripColor = Color.yellow;
                    break;
                default:
                    this.ValueColor = Color.black;
                    break;
            }

            // 如果使用加减号表示 那么value的文字颜色会在此处设定
            if (useAddReduceSymbol) {
                if (Value > 0) {
                    ValueColorType = 1;
                    this.ValueColor = Color.green;
                } else if (Value < 0) {
                    ValueColorType = 2;
                    this.ValueColor = Color.red;
                }
            }

        }
    }

}