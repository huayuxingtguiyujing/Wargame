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

        #region UI���
        [Header("���ذ�ť")]
        [SerializeField] Button hideButton;

        [Header("��������")]
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text areaText;
        [SerializeField] TMP_Text terrain;
        [SerializeField] TerrainBG terrainImage;
        [SerializeField] TMP_Text provinceController;


        [Header("��������")]
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

        [Header("��������")]
        [SerializeField] StatusItem safetyItem;
        [SerializeField] StatusItem adminItem;
        [SerializeField] StatusItem approvalItem;

        [SerializeField] StatusItem owner;
        [SerializeField] StatusItem taxCount;
        [SerializeField] StatusItem taxStorage;

        [Header("ʡ�ݹ���")]
        // ��ļ
        [SerializeField] RecruitPanel recruitPanel;
        [SerializeField] GameObject recruitButtonPrefab;
        [SerializeField] Button recruitButton;

        // ����
        [SerializeField] BasePopUI buildingPanel;
        [SerializeField] GameObject buildingItemPrefab;
        [SerializeField] Button buildButton;

        [Header("����")]
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

            //����ʡ�ݻ�����Ϣ �������
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
            // ��ʼ�������������
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
            // ��ʼ�������������
            taxCount.GetComponent<DetailPopController>().InitPopController(detailPop, province.provinceData.taxMessage);
            taxStorage.GetComponent<DetailPopController>().InitPopController(detailPop, province.provinceData.taxMessage);

            //���� �� ����ʡ�ݵ�һЩ���ܣ�����ļ�������ȣ�
            //TODO:������Լ���ʡ�ݣ�������ļ������
            //TODO:��������Լ���ʡ�ݣ���������ļ������

            //
            detailPop.Hide();

            //�趨 ��ļ��� ������� ��ui
            recruitPanel.Hide();
            buildingPanel.Hide();
            InitChildrenPanel(province, 
                PoliticLoader.Instance.PlayerFaction.FactionInfo.AbleArmyUnit
            );
        }

        public async void InitChildrenPanel(Province province, List<ArmyUnitData> ableArmyUnit) {
            
            //��ʼ�� ��ļ��壬���ݿ��ñ��ֽ�����ļ��ť
            recruitPanel.gameObject.ClearObjChildren();
            foreach (ArmyUnitData unitData in ableArmyUnit) {
                GameObject recruitButton = Instantiate(recruitButtonPrefab, recruitPanel.transform);
                UnityAction recuitCall = () => {
                    province.RecruitArmyEvent(unitData);
                    // ���������Դ�ɹ����򲥷���Ƶ
                    AudioManager.PlayAudio(AudioEffectName.RecruitArmyUnit);
                };
                RecuitButton recuitComponent = recruitButton.GetComponent<RecuitButton>();
                recuitComponent.InitRecuitButton(recuitCall, unitData.armyUnitName, unitData.armyCostMoney, unitData.armyCostManpower, unitData.armyCostBaseDay);
                recruitButton.name = unitData.armyUnitName;

            }

            // ��ʼ�� ������壬�������п��û򲻿��õĽ�����ť
            buildingPanel.gameObject.ClearObjChildren();
            List<Building> AllBuildings = await Building.GetAllBuilding();
            foreach (var building in AllBuildings)
            {
                //Debug.Log("success!");
                GameObject buildingItem = Instantiate(buildingItemPrefab, buildingPanel.transform);
                UnityAction<Building> buildingItemAction = (Building bu) => {
                    // �����ť���ڵ�ǰʡ�ݿ���һ����������
                    province.BuildEvent(bu);
                    // ���������Դ�ɹ����򲥷���Ƶ
                    AudioManager.PlayAudio(AudioEffectName.RecruitArmyUnit);
                };
                buildingItem.name = building.name;

                // TODO: ��Ҫ�жϸý����Ƿ��ڱ����Ѿ��У�����У����޷��ٽ�
                // TODO: ��Ҫ�жϸý����Ƿ��Ѿ��ڽ��죬����ڽ����޷������ť

                buildingItem.GetComponent<BuildingItem>().InitBuildingItem(building, buildingItemAction);
            }
            //buildingPanel.Hide();

            //���Ѿ���ʼ�� �򲻱��ٴ�ִ������ͨ�õĲ���
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

        // TODO : ���ⲿ��д�꣬չ������������
        public void ShowDetailPop() {

        }

    }
}