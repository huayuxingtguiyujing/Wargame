using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Infrastructure.Audio;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    [DisallowMultipleComponent]
    public class ProvincePanel : BasePopUI {

        public static ProvincePanel ActiveProvincePanel;

        #region UI组件
        [Header("隐藏按钮")]
        [SerializeField] Button hideButton;

        [Header("地理属性")]
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text areaText;
        [SerializeField] TMP_Text terrain;
        [SerializeField] TerrainBG terrainImage;
        [SerializeField] TMP_Text provinceController;


        [Header("经济属性")]
        [SerializeField] StatusItem populationItem;
        [SerializeField] StatusItem manpowerItem;
        [SerializeField] StatusItem prosperityItem;
        [SerializeField] StatusItem desolationItem;

        [SerializeField] StatusItem grainItem;
        [SerializeField] StatusItem grainProduceItem;
        [SerializeField] StatusItem grainStorageItem;

        [SerializeField] StatusItem saltItem;
        [SerializeField] StatusItem ironItem;
        [SerializeField] StatusItem horseItem;
        [SerializeField] StatusItem clothItem;

        [Header("政治属性")]
        [SerializeField] StatusItem safetyItem;
        [SerializeField] StatusItem adminItem;
        [SerializeField] StatusItem approvalItem;

        [SerializeField] StatusItem owner;
        [SerializeField] StatusItem taxCount;
        [SerializeField] StatusItem taxStorage;

        [Header("省份功能")]
        // 招募
        [SerializeField] RecruitPanel recruitPanel;
        [SerializeField] GameObject recruitButtonPrefab;
        [SerializeField] Button recruitButton;

        // 建筑
        [SerializeField] BasePopUI buildingPanel;
        [SerializeField] GameObject buildingItemPrefab;
        [SerializeField] Button buildButton;

        [Header("其他")]
        [SerializeField] DetailTextPop detailPop;
        #endregion

        private bool HasInitButton = false;

        private void Awake() {
            ActiveProvincePanel = this;
            Hide();

            hideButton.onClick.AddListener(Hide);

        }

        public void InitProvincePanel(Province province) {
            if(ActiveProvincePanel == null) {
                ActiveProvincePanel = this;
            }

            //设置省份基础信息 到面板上
            nameText.text = province.provinceData.provinceName;
            areaText.text = province.provinceData.ProvinceArea;
            terrain.text = province.provinceData.Terrain.ToString();
            terrainImage.SetTerrainBG(province.provinceData.Terrain);
            provinceController.text = province.provinceData.OwnerTag.ToString();


            populationItem.SetStatusItem(province.provinceData.Population.ToString());
            manpowerItem.SetStatusItem(province.provinceData.ManPower.ToString());
            prosperityItem.SetStatusItem(province.provinceData.Prosperity.ToString());
            desolationItem.SetStatusItem(province.provinceData.Desolation.ToString());

            grainItem.SetStatusItem(province.provinceData.GrainProduce.ToString());
            grainProduceItem.SetStatusItem(province.provinceData.GetGrainProduce_Day().ToString());
            grainStorageItem.SetStatusItem(province.provinceData.GrainStorage.ToString());
            // 初始化弹出框控制器
            grainProduceItem.GetComponent<DetailPopController>().InitPopController(detailPop, province.provinceData.grainMessage);
            grainStorageItem.GetComponent<DetailPopController>().InitPopController(detailPop, province.provinceData.grainMessage);

            saltItem.SetStatusItem(province.provinceData.GrainProduce.ToString());
            ironItem.SetStatusItem(province.provinceData.GrainProduce.ToString());
            horseItem.SetStatusItem(province.provinceData.GrainProduce.ToString());
            clothItem.SetStatusItem(province.provinceData.GrainProduce.ToString());

            safetyItem.SetStatusItem(province.provinceData.PublicSafety.ToString()); ;
            adminItem.SetStatusItem(province.provinceData.AdminEfficiency.ToString()); ;
            approvalItem.SetStatusItem(province.provinceData.ApprovalRating.ToString()); ;

            //owner.SetStatusItem(provinceData.provincePol..ToString());
            owner.SetStatusItem("no owner");
            taxCount.SetStatusItem(province.provinceData.GetProvinceTax_Day().ToString());
            taxStorage.SetStatusItem(province.provinceData.TaxStorage.ToString());
            // 初始化弹出框控制器
            taxCount.GetComponent<DetailPopController>().InitPopController(detailPop, province.provinceData.taxMessage);
            taxStorage.GetComponent<DetailPopController>().InitPopController(detailPop, province.provinceData.taxMessage);

            //禁用 或 启用省份的一些功能（如招募、建筑等）
            //TODO:如果是自己的省份，可以招募，启用
            //TODO:如果不是自己的省份，不可以招募，禁用

            //
            detailPop.Hide();

            //设定 征募面板 建筑面板 的ui
            recruitPanel.Hide();
            buildingPanel.Hide();
            InitChildrenPanel(province, 
                PoliticLoader.Instance.PlayerFaction.FactionInfo.AbleArmyUnit
            );
        }

        public async void InitChildrenPanel(Province province, List<ArmyUnitData> ableArmyUnit) {
            
            //初始化 征募面板，根据可用兵种建立招募按钮
            recruitPanel.gameObject.ClearObjChildren();
            foreach (ArmyUnitData unitData in ableArmyUnit) {
                GameObject recruitButton = Instantiate(recruitButtonPrefab, recruitPanel.transform);
                UnityAction recuitCall = () => {
                    province.RecruitArmyEvent(unitData);
                    // 如果花费资源成功，则播放音频
                    AudioManager.PlayAudio(AudioEffectName.RecruitArmyUnit);
                };
                RecuitButton recuitComponent = recruitButton.GetComponent<RecuitButton>();
                recuitComponent.InitRecuitButton(recuitCall, unitData.armyUnitName, unitData.armyCostMoney, unitData.armyCostManpower, unitData.armyCostBaseDay);
                recruitButton.name = unitData.armyUnitName;

            }

            // 初始化 建筑面板，建立所有可用或不可用的建筑按钮
            buildingPanel.gameObject.ClearObjChildren();
            List<Building> AllBuildings = await Building.GetAllBuilding();
            foreach (var building in AllBuildings)
            {
                //Debug.Log("success!");
                GameObject buildingItem = Instantiate(buildingItemPrefab, buildingPanel.transform);
                UnityAction<Building> buildingItemAction = (Building bu) => {
                    // 点击按钮后，在当前省份开启一个建筑事务
                    province.BuildEvent(bu);
                    // 如果花费资源成功，则播放音频
                    AudioManager.PlayAudio(AudioEffectName.RecruitArmyUnit);
                };
                buildingItem.name = building.name;

                // TODO: 需要判断该建筑是否在本地已经有，如果有，则无法再建
                // TODO: 需要判断该建筑是否已经在建造，如果在建，无法点击按钮

                buildingItem.GetComponent<BuildingItem>().InitBuildingItem(building, buildingItemAction);
            }
            //buildingPanel.Hide();

            //若已经初始化 则不必再次执行下列通用的操作
            if (!HasInitButton) {
                HasInitButton = true;

                recruitButton.onClick.AddListener(delegate {
                    if (recruitPanel.Visible) {
                        recruitPanel.Hide();
                    } else {
                        recruitPanel.Show();
                    }
                });
                
                buildButton.onClick.AddListener(delegate {
                    if (buildingPanel.Visible) {
                        buildingPanel.Hide();
                    } else {
                        buildingPanel.Show();
                    }
                });

            }

        }

        // TODO : 将这部分写完，展现完整的修正
        public void ShowDetailPop() {

        }

    }
}