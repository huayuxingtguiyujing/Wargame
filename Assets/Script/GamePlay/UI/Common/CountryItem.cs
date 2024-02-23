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
        
        // ��UI CountryItem ��Ӧ��FactionInfo
        public FactionInfo ItemFaction { get; protected set; }

        // ѡ��CountryItemʱ�Ļص�
        public delegate void ChooseCountryItemDelegate(CountryItem factionInfo);

        public ChooseCountryItemDelegate chooseCountryItemCall;

        /// <summary>
        /// ��ʼ��CountryItem
        /// </summary>
        public abstract void InitCountryItem(FactionInfo info);

        /// <summary>
        /// ��ȡ�ù����쵼�ߵ�ͷ��
        /// </summary>
        public abstract Sprite GetCountryLeaderSprite();

        // ����CountryItem��ѡ��
        public virtual void SetChoosen() { }

        public virtual void SetChoosePlayer(string playerName, Color color) { }

        public virtual void SetUnchoosen() {}
    }
}