using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;

namespace WarGame_True.GamePlay.UI {
    [RequireComponent(typeof(CombatPanel))]
    /// <summary>
    /// 挂接在CombatPanel上，为CombatPanel提供各种战役图片
    /// </summary>
    public class CombatSituationSprite : MonoBehaviour {

        public Sprite RandomDiceSprite;
        public Sprite UnknownSprite;

        #region  各项加成 对应的图片
        [Header("各项加成对应的图片")]
        public Sprite maxMoraleSprite_Rise;
        public Sprite maxMoraleSprite_Down;
        public Sprite recoverMoraleSprite;

        public Sprite discipineSprite_Rise;
        public Sprite discipineSprite_Down;

        public Sprite speedSprite;

        public Sprite armyAttackSprite_Rise;
        public Sprite armyAttackSprite_Down;
        public Sprite armyDefendSprite_Rise;
        public Sprite armyDefendSprite_Down;

        public Sprite armyAttackScopeSprite;

        public Sprite armyArmorPenetrateSprite;
        public Sprite armyArmorCladkSprite;

        public Sprite armyMoraleAttackSprite_Rise;
        public Sprite armyMoraleAttackSprite_Down;
        public Sprite armyMoraleDefendSprite_Rise;
        public Sprite armyMoraleDefendSprite_Down;

        public Sprite armyMoraleInRearSprite ;
        public Sprite armyMoraleToRearSprite;

        public Sprite supplyCostSprite;
        public Sprite supplyMaxSprite;
        #endregion

        public Sprite GetSpriteByModifyName(string modifyName, float num) {
            if(modifyName == UnitModify.armyArmorCladkModifyDescrip) {
                return armyArmorCladkSprite;
            } else if (modifyName == UnitModify.armyArmorPenetrateModifyDescrip) {
                return armyArmorPenetrateSprite;
            } else if (modifyName == UnitModify.armyAttackModifyDescrip) {
                return num > 0 ? armyAttackSprite_Rise : armyAttackSprite_Down;
            } else if (modifyName == UnitModify.armyDefendModifyDescrip) {
                return num > 0 ? armyDefendSprite_Rise : armyDefendSprite_Down;
            } else if (modifyName == UnitModify.armyMoraleAttackModifyDescrip) {
                return num > 0 ? armyMoraleAttackSprite_Rise : armyMoraleAttackSprite_Down;
            } else if (modifyName == UnitModify.armyMoraleDefendModifyDescrip) {
                return num > 0 ? armyMoraleDefendSprite_Rise : armyMoraleDefendSprite_Down;
            } else if (modifyName == UnitModify.armyMoraleModifyInRearDescrip) {
                return armyMoraleInRearSprite;
            } else if (modifyName == UnitModify.armyMoraleModifyToRearDescrip) {
                return armyMoraleToRearSprite;
            } else if (modifyName == UnitModify.armyAttackScopeModifyDescrip) {
                return armyAttackScopeSprite;
            } else if (modifyName == UnitModify.supplyCostModiyDescrip) {
                return supplyCostSprite;
            } else if (modifyName == UnitModify.supplyMaxModifyDescrip) {
                return supplyMaxSprite;
            } else if (modifyName == UnitModify.discipineModifyDescrip) {
                return num > 0 ? discipineSprite_Rise : discipineSprite_Down;
            } else if (modifyName == UnitModify.maxMoraleModifyDescrip) {
                return num > 0 ? maxMoraleSprite_Rise : maxMoraleSprite_Down;
            } else if (modifyName == UnitModify.recoverMoraleModifyDescrip) {
                return recoverMoraleSprite;
            } else {
                // TODO: 添加上新的三个修正字段
                return UnknownSprite;
            }

        }

    }
}