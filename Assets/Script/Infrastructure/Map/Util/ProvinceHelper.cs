using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.Infrastructure.Map.Util {
    public class ProvinceHelper {
        #region 移动相关
        /// <summary>
        /// 获取省份路径的移动总花费
        /// </summary>
        /// <param name="provinces"></param>
        /// <returns></returns>

        public static uint GetProvinceMoveCost(List<Province> provinces) {
            uint moveCost = 0;
            foreach (Province province in provinces) {
                moveCost += province.provinceData.MoveCost;
            }
            return moveCost;
        }


        /// <summary>
        /// 设置省份处于移动路径上
        /// </summary>
        /// <param name="provinces"></param>
        public static void SetProvinceMoving(List<Province> provinces) {
            foreach (Province province in provinces) {
                province.SetProvinceAsMovePath();
            }
        }

        public static void SetProvinceWithdrawing(List<Province> provinces) {
            foreach (Province province in provinces) {
                province.SetProvinceAsWithdrawPath();
            }
        }

        public static void SetProvinceCloseMovePath(List<Province> provinces) {
            foreach (Province province in provinces) {
                province.SetProvinceCloseMovePath();
            }
        }

        /// <summary>
        /// 设置省份为补给线 所处的省份
        /// </summary>
        public static void SetProvinceSupplyLine(List<Province> provinces) {
            foreach (Province province in provinces) {
                province.SetProvinceAsSupplyLine();
            }
        }

        public static void SetProvinceCloseSupply(List<Province> provinces) {
            foreach (Province province in provinces) {
                province.SetProvinceCloseSupplyLine();
            }
        }

        #endregion

        public static void SetProvinceColor() {

        }

    }
}