using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.GamePlay.UI {
    public class PoliticPanel : MonoBehaviour {
        [Header("ItemListTrans")]
        [SerializeField] private Transform itemParent;
        [Header("prefab")]
        [SerializeField] private GameObject CountryListItemPrefab;

        public void InitPoliticPanel(List<Faction> bookMarkFactions) {
            //初始化 国家列表
            foreach (Faction faction in bookMarkFactions)
            {
                GameObject itemObject = Instantiate(CountryListItemPrefab, itemParent);
                CountryListItem countryListItem = itemObject.GetComponent<CountryListItem>();
                countryListItem.InitPoliticPanelItem(faction.FactionInfo);
            }
        }
        
    }
}