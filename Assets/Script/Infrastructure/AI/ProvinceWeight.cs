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
    /// 省份权重
    /// </summary>
    public class ProvinceWeight {

        #region 对应的省份数据

        public uint HexID;

        public Vector3 HexPos;

        #endregion

        public bool IsCapital;

        // 己方军队数目
        public int ArmyInProvince;
        // 友军军队数目
        public int FriendInProvince;
        // 敌人军队数目（单位: k）
        public int EnemyInProvince;


        // 军事价值,价值越高的省份,越倾向于移动到该位置
        public int MilValue;
        // 经济价值,价值越高,越倾向于在当地建筑、驻守军队
        public int EcoValue;

        // 省份附近两格是否有敌人
        public bool IsProvinceDanger = false;
        // 省份是否处于边境
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
        /// 计算本省份的经济价值，该函数仅计算本地
        /// </summary>
        public void CaculateEcoValue(Province province) {
            EcoValue = 0;

            // 经济数据 影响
            // 人口带来的权重（请见生成省份数据时 人口的随机值）
            EcoValue += (int)province.provinceData.Population / 200000;

            if(province.provinceData.Prosperity > 70) {
                EcoValue += 1;
            }

            if(province.provinceData.Desolation > 70) {
                EcoValue -= 1;
            }

            // 政治数据 影响
            if(province.provinceData.AdminEfficiency > 70) {
                EcoValue += 1;
            }

            if(province.provinceData.PublicSafety > 70) {
                EcoValue += 1;
            }

            // 地理数据 影响
            if(province.provinceData.Terrain == Map.Provinces.Terrain.Desert) {
                EcoValue -= 2;
            }else if (province.provinceData.Terrain == Map.Provinces.Terrain.Hill) {
                EcoValue -= 1;
            }else if (province.provinceData.Terrain == Map.Provinces.Terrain.City) {
                EcoValue += 1;
            }

            // TODO: 判断省份是否是首都

        }

        /// <summary>
        /// 计算本省份的军事价值，该函数仅计算本地
        /// </summary>
        public void CaculateMilValue(string aiTag, AIType aiType, Province province) {
            MilValue = 0;

            ArmyInProvince = province.GetCountryArmy();
            FriendInProvince = province.GetFriendlyArmy();
            EnemyInProvince = province.GetHostileArmy();

            string nearTag = "";
            int enemyValue = 0;         
            // 执行传播逻辑，有敌军的省份会向相邻两格地图传播权重
            IsProvinceDanger = MapController.Instance.CheckProvNeibor(aiTag, ref nearTag, ref enemyValue, province);
            // 判断省份是否在前线
            IsProvinceInFront = MapController.Instance.IsProvinceInFr(province, ref nearTag);

            if (PoliticLoader.Instance.IsFactionInWar(aiTag)) {
                // AI 国家正处于战争

                if (PoliticLoader.Instance.IsFactionInWar(aiTag, nearTag)) {
                    // 临近一个处于战争状态的国家
                    MilValue += aiType.GetMilWeight(3, 4, 5);
                }

                if (EnemyInProvince > 0) {
                    // 省份上上有敌人
                    MilValue += aiType.GetMilWeight(2, 3, 4);
                }

                if (enemyValue > 0 && EnemyInProvince <= 0) {
                    // 省份附近有敌人
                    MilValue += aiType.GetMilWeight(1, 1, 1);
                }

                if (province.OwnerByTag(aiTag) && !province.UnderTagControl(aiTag)) {
                    // 是自己的省份, 但不被自己控制
                    MilValue += aiType.GetMilWeight(5, 4, 3);
                }else if (!IsProvinceInFront && province.OwnerByTag(aiTag)) {
                    // 是自己的省份，且不在边境上
                    MilValue += aiType.GetMilWeight(2, 2, 1);
                } else if(IsProvinceInFront && !province.OwnerByTag(aiTag)) {
                    // 不是自己的省份，且在边境上
                    MilValue += aiType.GetMilWeight(3, 4, 5);
                } else if(!IsProvinceInFront && !province.OwnerByTag(aiTag)){
                    // 不是自己的省份，且不在边境上
                    MilValue += aiType.GetMilWeight(-5, -4, -3);
                }

                if (IsProvinceDanger) {
                    // 省份周围有敌人，加强戒备
                    MilValue += 3;
                }

                if (IsProvinceInFront) {
                    MilValue += 3;
                }

                //Debug.Log($"in war, the mil value: {MilValue}, in danger: {IsProvinceDanger}, in front: {IsProvinceInFront}, near tag: {nearTag}");
            } else {
                // 和平时期 边境省份加强防备
                // TODO: 临近国家与自己关系越不好，越强大，防备越重
                if (IsProvinceInFront) {
                    MilValue += 3;
                }

            }

        }

    }
}