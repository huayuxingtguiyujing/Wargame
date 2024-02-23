using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;

namespace WarGame_True.GamePlay.UI {
    public class ArmyUnitItem : MonoBehaviour {

        [SerializeField] TMP_Text armyNameText;
        [SerializeField] StatusItem numItem;

        [SerializeField] StatusItem maxMoraleItem;
        [SerializeField] StatusItem moraleItem;
        [SerializeField] StatusItem supplyItem;

        [Header("士气与补给条")]
        [SerializeField] SliderBar moraleSlider;

        public void UpdateUnitItem(ArmyUnit armyUnitData) {
            armyNameText.text = armyUnitData.ArmyUnitName;

            numItem.SetStatusItem(armyUnitData.ArmyCurrentManpower.ToString());

            maxMoraleItem.SetStatusItem(armyUnitData.ArmyBaseMaxMorale.ToString());
            moraleItem.SetStatusItem(armyUnitData.ArmyBaseMorale.ToString());
            
            moraleSlider.UpdateSliderBar(armyUnitData.ArmyBaseMaxMorale, armyUnitData.ArmyBaseMorale);
            //supplySlider.UpdateSliderBar(armyUnitData.SupplyBaseMax, armyUnitData.CurrentSupply);
        }
    }
}