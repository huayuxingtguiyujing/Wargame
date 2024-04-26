using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Infrastructure.HexagonGrid.MapObject;
using WarGame_True.Infrastructure.Map.Controller;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.Infrastructure.AI {
    /// <summary>
    /// ʡ��Ȩ��
    /// </summary>
    public class ProvinceWeight {

        #region ��Ӧ��ʡ������

        public uint HexID;

        public Vector3 HexPos;

        #endregion

        public bool IsCapital;

        // ����������Ŀ
        public int ArmyInProvince;
        // �Ѿ�������Ŀ
        public int FriendInProvince;
        // ���˾�����Ŀ����λ: k��
        public int EnemyInProvince;


        // ���¼�ֵ,��ֵԽ�ߵ�ʡ��,Խ�������ƶ�����λ��
        public int MilValue;
        // ���ü�ֵ,��ֵԽ��,Խ�������ڵ��ؽ�����פ�ؾ���
        public int EcoValue;

        // ʡ�ݸ��������Ƿ��е���
        public bool IsProvinceDanger = false;
        // ʡ���Ƿ��ڱ߾�
        public bool IsProvinceInFront = false;


        public ProvinceWeight() { 
        }

        public ProvinceWeight(string aiTag, AIType aiType,Province province) {
            UpdateWeight(aiTag, aiType, province);
        }

        public void UpdateWeight(string aiTag, AIType aiType, Province province) {
            HexID = province.provinceID;
            HexPos = province.provincePosition;

            CaculateEcoValue(province);
            CaculateMilValue(aiTag, aiType, province);
        }

        /// <summary>
        /// ���㱾ʡ�ݵľ��ü�ֵ���ú��������㱾��
        /// </summary>
        public void CaculateEcoValue(Province province) {
            EcoValue = 0;

            // �������� Ӱ��
            // �˿ڴ�����Ȩ�أ��������ʡ������ʱ �˿ڵ����ֵ��
            EcoValue += (int)province.provinceData.Population / 200000;

            if(province.provinceData.Prosperity > 70) {
                EcoValue += 1;
            }

            if(province.provinceData.Desolation > 70) {
                EcoValue -= 1;
            }

            // �������� Ӱ��
            if(province.provinceData.AdminEfficiency > 70) {
                EcoValue += 1;
            }

            if(province.provinceData.PublicSafety > 70) {
                EcoValue += 1;
            }

            // �������� Ӱ��
            if(province.provinceData.Terrain == Map.Provinces.Terrain.Desert) {
                EcoValue -= 2;
            }else if (province.provinceData.Terrain == Map.Provinces.Terrain.Hill) {
                EcoValue -= 1;
            }else if (province.provinceData.Terrain == Map.Provinces.Terrain.City) {
                EcoValue += 1;
            }

            // TODO: �ж�ʡ���Ƿ����׶�

        }

        /// <summary>
        /// ���㱾ʡ�ݵľ��¼�ֵ���ú��������㱾��
        /// </summary>
        public void CaculateMilValue(string aiTag, AIType aiType, Province province) {
            MilValue = 0;

            ArmyInProvince = province.GetCountryArmy();
            FriendInProvince = province.GetFriendlyArmy();
            EnemyInProvince = province.GetHostileArmy();

            string nearTag = "";
            int enemyValue = 0;         
            // ִ�д����߼����ео���ʡ�ݻ������������ͼ����Ȩ��
            IsProvinceDanger = MapController.Instance.CheckProvNeibor(aiTag, ref nearTag, ref enemyValue, province);
            // �ж�ʡ���Ƿ���ǰ��
            IsProvinceInFront = MapController.Instance.IsProvinceInFr(province, ref nearTag);

            if (PoliticLoader.Instance.IsFactionInWar(aiTag)) {
                // AI ����������ս��

                if (PoliticLoader.Instance.IsFactionInWar(aiTag, nearTag)) {
                    // �ٽ�һ������ս��״̬�Ĺ���
                    MilValue += aiType.GetMilWeight(3, 4, 5);
                }

                if (EnemyInProvince > 0) {
                    // ʡ�������е���
                    MilValue += aiType.GetMilWeight(2, 3, 4);
                }

                if (enemyValue > 0 && EnemyInProvince <= 0) {
                    // ʡ�ݸ����е���
                    MilValue += aiType.GetMilWeight(1, 1, 1);
                }

                if (province.OwnerByTag(aiTag) && !province.UnderTagControl(aiTag)) {
                    // ���Լ���ʡ��, �������Լ�����
                    MilValue += aiType.GetMilWeight(5, 4, 3);
                }else if (!IsProvinceInFront && province.OwnerByTag(aiTag)) {
                    // ���Լ���ʡ�ݣ��Ҳ��ڱ߾���
                    MilValue += aiType.GetMilWeight(2, 2, 1);
                } else if(IsProvinceInFront && !province.OwnerByTag(aiTag)) {
                    // �����Լ���ʡ�ݣ����ڱ߾���
                    MilValue += aiType.GetMilWeight(3, 4, 5);
                } else if(!IsProvinceInFront && !province.OwnerByTag(aiTag)){
                    // �����Լ���ʡ�ݣ��Ҳ��ڱ߾���
                    MilValue += aiType.GetMilWeight(-5, -4, -3);
                }

                if (IsProvinceDanger) {
                    // ʡ����Χ�е��ˣ���ǿ�䱸
                    MilValue += 3;
                }

                if (IsProvinceInFront) {
                    MilValue += 3;
                }

                //Debug.Log($"in war, the mil value: {MilValue}, in danger: {IsProvinceDanger}, in front: {IsProvinceInFront}, near tag: {nearTag}");
            } else {
                // ��ƽʱ�� �߾�ʡ�ݼ�ǿ����
                // TODO: �ٽ��������Լ���ϵԽ���ã�Խǿ�󣬷���Խ��
                if (IsProvinceInFront) {
                    MilValue += 3;
                }

            }

        }

    }
}