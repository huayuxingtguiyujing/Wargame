using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.Infrastructure.Map.Provinces;

namespace WarGame_True.Infrastructure.Map.Util {
    public class ProvinceHelper {
        #region �ƶ����
        /// <summary>
        /// ��ȡʡ��·�����ƶ��ܻ���
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
        /// ����ʡ�ݴ����ƶ�·����
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
        /// ����ʡ��Ϊ������ ������ʡ��
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