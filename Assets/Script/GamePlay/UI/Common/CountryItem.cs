using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WarGame_True.GamePlay.Politic;

namespace WarGame_True.GamePlay.UI {
    
    public abstract class CountryItem : MonoBehaviour {
        
        // 该UI CountryItem 对应的FactionInfo
        public FactionInfo ItemFaction { get; protected set; }

        // 选中CountryItem时的回调
        public delegate void ChooseCountryItemDelegate(CountryItem factionInfo);

        public ChooseCountryItemDelegate chooseCountryItemCall;

        /// <summary>
        /// 初始化CountryItem
        /// </summary>
        public abstract void InitCountryItem(FactionInfo info);

        /// <summary>
        /// 获取该国家领导者的头像
        /// </summary>
        public abstract Sprite GetCountryLeaderSprite();

        // 设置CountryItem被选中
        public virtual void SetChoosen() { }

        public virtual void SetChoosePlayer(string playerName, Color color) { }

        public virtual void SetUnchoosen() {}
    }
}