using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    public class DiplomaticPanel : BasePopUI {

        Faction currentFaction;

        //[Space]
        //[SerializeField] Button closeButton;

        [Header("�⽻���")]
        [SerializeField] GameObject countryItemDipPrefab;
        [SerializeField] Transform countryItemsParent;

        public void UpdateDipPanel(Faction faction) {

            currentFaction = faction;

            countryItemsParent.gameObject.ClearObjChildren();
            List<DiploRelation> diploRelations = currentFaction.GetAllDiploRelations();
            // �����⽻��ϵ ��ʼ���⽻�������
            foreach (DiploRelation dip in diploRelations)
            {
                GameObject countryItemObject = Instantiate(countryItemDipPrefab, countryItemsParent);
                CountryItem_Dip countryItem_Dip = countryItemObject.GetComponent<CountryItem_Dip>();
                countryItem_Dip.InitCountryItem(dip.targetFaction);
                countryItem_Dip.SetDipRelation(dip);
            }

        }


    }
}