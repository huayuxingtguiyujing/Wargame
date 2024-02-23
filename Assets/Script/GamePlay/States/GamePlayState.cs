using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.GameState;
using WarGame_True.GamePlay.Politic;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.Map.Controller;
using WarGame_True.Infrastructure.NetworkPackage;

namespace WarGame_True.States {
    [RequireComponent(typeof(TimeSimulator))]
    [RequireComponent(typeof(PoliticLoader))]
    [RequireComponent(typeof(ArmyController))]
    public class GamePlayState : GameStateBehaviour {

        [SerializeField] PoliticLoader politicLoader;

        [SerializeField] MapController mapController;

        //作为 国家 物体的父变换组件
        [SerializeField] Transform countryTransform;

        [Header("网络相关组件")]
        [SerializeField] NetworkHook networkHook;
        [SerializeField] GamePlayServerState gamePlayServerState;
        [SerializeField] TimeNetworkSimulator timeNetworkSimulator;

        [Header("常用的图标资源 管理器")]
        public CombatSituationSprite SituationSpriteHolder;

        [Header("当前存在的国家")]
        static Faction[] factions;

        public override GameState ActiveState => GameState.SingleWarGame;

        #region Network/MonoBehaviour
        private void Awake() {
            // 默认状态下 不启用timeNetworkSimulator，联网时启用
            timeNetworkSimulator.enabled = false;

            // TODO: 修复不能正常启动 OnNetworkSpawn 的问题
            networkHook.OnNetworkSpawnHook += OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        protected override void Start() {
            base.Start();
            // 初始化地图网格
            mapController.InitMap();
            // 从存储中读取省份数据
            mapController.SetProvinceData();

            // 初始化政治势力
            politicLoader.InitPolitic(countryTransform);
            // 根据政治势力设置省份归属
            //mapController.InitTerritory_ByPos(politicLoader.BookMarkFactions);
            mapController.InitTerritory_ByID(politicLoader.BookMarkFactions);

            // 记录当前的国家
            factions = countryTransform.GetComponentsInChildren<Faction>();
            
            // 初始化联网状态相关组件
            gamePlayServerState.InitGamePlayNetwork(countryTransform);
        }

        protected override void OnDestroy() {
            networkHook.OnNetworkSpawnHook -= OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook -= OnNetworkDespawn;
        }

        public void OnNetworkSpawn() {
            timeNetworkSimulator.enabled = true;
            timeNetworkSimulator.InitTimeNetwork();
        }

        public void OnNetworkDespawn() {

        }
        #endregion

        public static bool ValidTag(string tag) {
            foreach (Faction fac in factions) {
                if (fac.FactionInfo.FactionTag == tag) {
                    return true;
                }
            }
            return false;
        }

        public static Faction GetFaction(string tag) {
            if(factions == null || factions.Length <= 0) {
                Debug.Log("no country alive!");
                return null;
            }

            if (!ValidTag(tag)) {
                Debug.Log("not a valid tag!");
                return null;
            }

            foreach (Faction fac in factions)
            {
                if(fac.FactionInfo.FactionTag == tag) {
                    return fac;
                }
            }
            return null;
        }



    }
}