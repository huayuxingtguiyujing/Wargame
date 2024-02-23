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

        //��Ϊ ���� ����ĸ��任���
        [SerializeField] Transform countryTransform;

        [Header("����������")]
        [SerializeField] NetworkHook networkHook;
        [SerializeField] GamePlayServerState gamePlayServerState;
        [SerializeField] TimeNetworkSimulator timeNetworkSimulator;

        [Header("���õ�ͼ����Դ ������")]
        public CombatSituationSprite SituationSpriteHolder;

        [Header("��ǰ���ڵĹ���")]
        static Faction[] factions;

        public override GameState ActiveState => GameState.SingleWarGame;

        #region Network/MonoBehaviour
        private void Awake() {
            // Ĭ��״̬�� ������timeNetworkSimulator������ʱ����
            timeNetworkSimulator.enabled = false;

            // TODO: �޸������������� OnNetworkSpawn ������
            networkHook.OnNetworkSpawnHook += OnNetworkSpawn;
            networkHook.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        protected override void Start() {
            base.Start();
            // ��ʼ����ͼ����
            mapController.InitMap();
            // �Ӵ洢�ж�ȡʡ������
            mapController.SetProvinceData();

            // ��ʼ����������
            politicLoader.InitPolitic(countryTransform);
            // ����������������ʡ�ݹ���
            //mapController.InitTerritory_ByPos(politicLoader.BookMarkFactions);
            mapController.InitTerritory_ByID(politicLoader.BookMarkFactions);

            // ��¼��ǰ�Ĺ���
            factions = countryTransform.GetComponentsInChildren<Faction>();
            
            // ��ʼ������״̬������
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