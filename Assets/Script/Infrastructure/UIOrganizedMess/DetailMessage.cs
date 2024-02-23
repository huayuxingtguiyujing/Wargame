using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

namespace WarGame_True.Utils {
    /// <summary>
    /// �洢�����ڣ�"����ֵ +20%" ����������
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class DetailMessage {
        // �����Ŀ�ʼ��
        public DetailText StartItem;
        public bool HasStart { get => StartItem != null; }
        
        // �������м�ϸ�ڲ���
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

        // �����Ľ�����
        public DetailText EndItem;
        public bool HasEnd { get => EndItem != null; }


        public int Count { get => Details.Count; }
        public DetailMessage() {
            Details = new List<DetailText>();
        }

        // ����������
        public void AddMessage(string description, float value, bool usePercent = true, bool useARSymbol = true, 
            int desColor = 4, int valueColor = 4, string preDesc = "") {
            if(Details == null) {
                Details = new List<DetailText>();
            }

            int index = FindDetailsIndex(description);
            if (index == -1) {
                // û����ӹ��ü�ֵ
                //Debug.Log("have not add this key: " + description);
                Details.Add(
                    new DetailText(description, value, usePercent, useARSymbol, desColor, valueColor, preDesc)
                );
            } else {
                //Debug.Log("have add this key: " + description);
                // ���Ѿ������� �������У������µĸ��ǵ�
                Details[index] = new DetailText(description, value, usePercent, useARSymbol, desColor, valueColor, preDesc);
            }
                
        }

        public void AddMessage(string preDesc, DetailMessage detailMes) {
            if (Details == null) {
                Details = new List<DetailText>();
            }

            foreach (var detail in detailMes.Details)
            {
                // ���ô�������
                detail.PreDesc = preDesc;
                int index = FindDetailsIndex(detail.Description);
                if (index == -1) {
                    Details.Add(detail);
                } else {
                    // ���Ѿ������� �������У������µĸ��ǵ�
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
    /// ���ڱ�ʾ�����ڣ�"����ֵ +20%" ����Ϣ����
    /// "Description : value"
    /// </summary>
    public class DetailText {

        public string PreDesc = "";             // ���� ����������Ϊ����������Ϊ�������

        public string Description;              // ���� ��������DetailTextMessage����Ϊ�� ( Ψһ��)

        public float Value;

        public bool UsePercent;                 // ʹ�ðٷ�����ʾֵ

        public bool UseAddReduceSymbol;         // ֵΪtrueʱ����ʾ�����value��������� + - ��

        public int DescripColorType;       
        public Color DescripColor;              // �������ֵ���ɫ 0 -��ɫ, 1 -��ɫ, 2 -��ɫ, 3 -��ɫ

        public int ValueColorType;       
        public Color ValueColor;                // value����ɫ

        public DetailText(string description, float value, string preDesc = "") {
            Description = description;
            this.Value = value;
            PreDesc = preDesc;

            this.UsePercent = true;
            this.UseAddReduceSymbol = true;

            // Ĭ������Ϊ��ɫ
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
        /// �������ø��ı� Desc : Value ��������ɫ
        /// </summary>
        private void SetDescValueColor(bool useAddReduceSymbol, int descripColorType, int valueColorType) {
            // ��������� descripColorType ȷ�����ֵ���ɫ
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

            // ��������� valueColorType ȷ��value���ֵ���ɫ
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

            // ���ʹ�üӼ��ű�ʾ ��ôvalue��������ɫ���ڴ˴��趨
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