using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.UIOrganizedMess;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.CombatPart {
    /// <summary>
    /// 将领类，包含了将领的所有信息
    /// </summary>
    [System.Serializable]
    public class General {

        string generalName;

        string generalChineseName;

        string generalDescription;

        string generalTag;              //军队所属的势力
        public string GeneralName { get => generalName;private set => generalName = value; }
        public string GeneralChineseName { get => generalChineseName; private set => generalChineseName = value; }
        public string GeneralDescription { get => generalDescription; private set => generalDescription = value; }
        public string GeneralTag { get => generalTag; private set => generalTag = value; }

        // 对军队的修正
        float organizationModify;
        uint maxCommandUnits;
        int visibleGrid;
        public float OrganizationModify { get => organizationModify; private set => organizationModify = value; }
        public uint MaxCommandUnits { get => maxCommandUnits; private set => maxCommandUnits = value; }
        public int VisibleGrid { get => visibleGrid; private set => visibleGrid = value; }

        // 不同战斗阶段下对军队单位的修正
        UnitModify normalModify;
        public UnitModify NormalModify { get => normalModify; private set => normalModify = value; }

        UnitModify standOffModify;
        public UnitModify StandOffModify { get => standOffModify; private set => standOffModify = value; }

        UnitModify siegeModify;
        public UnitModify SiegeModify { get => siegeModify; private set => siegeModify = value; }

        UnitModify engagementModify;
        public UnitModify EngagementModify { get => engagementModify; private set => engagementModify = value; }

        UnitModify openBattleModify;
        public UnitModify OpenBattleModify { get => openBattleModify; private set => openBattleModify = value; }

        UnitModify withdrawModify;
        public UnitModify WithdrawModify { get => withdrawModify; private set => withdrawModify = value; }

        #region 将领修正 中文名称 的静态字段
        // 写的有点脑瘫说实话
        public static string NormalModifyDescrip = "正常 ";
        public static string StandOffModifyDescrip = "对峙阶段 ";
        public static string EngagementModifyDescrip = "接战阶段 ";
        public static string OpenBattleModifyDescrip = "合战阶段 ";
        public static string SiegeModifyDescrip = "围城阶段 ";
        public static string WithdrawModifyDescrip = "撤退阶段 ";
        #endregion

        public DetailMessage generalModifyMessage { get; private set; }
        // TODO: 在将领面板 添加上将领的能力描述栏
        private void UpdateModifyMessage() {
            generalModifyMessage.AddMessage(NormalModifyDescrip, NormalModify.ModifyMessage);
            generalModifyMessage.AddMessage(StandOffModifyDescrip, StandOffModify.ModifyMessage);
            generalModifyMessage.AddMessage(EngagementModifyDescrip, EngagementModify.ModifyMessage);
            generalModifyMessage.AddMessage(OpenBattleModifyDescrip, OpenBattleModify.ModifyMessage);
            generalModifyMessage.AddMessage(SiegeModifyDescrip, SiegeModify.ModifyMessage);
            generalModifyMessage.AddMessage(WithdrawModifyDescrip, WithdrawModify.ModifyMessage);
        }

        // TODO：
        // 将领特质 - 根据对军队单位的修正获得
        public List<GeneralTrait> GeneralTraits { get; private set; }

        // 将领可采用的战术
        public List<Tactic> GeneralTactics { get; private set; }

        public General() { }

        /// <summary>
        /// 使用预定义的将领信息ScriptableObject初始化
        /// </summary>
        public General(GeneralData generalData) {
            GeneralName = generalData.generalName;
            GeneralChineseName = generalData.generalChineseName;
            GeneralDescription = generalData.generalDescription;
            GeneralTag = generalData.generalTag;

            OrganizationModify = generalData.organizationModify;
            MaxCommandUnits = generalData.maxCommandUnits;
            VisibleGrid = generalData.visibleGrid;

            //Debug.Log("将领测试: modify:" + (generalData.engagementModify == null));
            NormalModify = generalData.normalModify;
            NormalModify.UpdateMessageItemList();
            StandOffModify = generalData.standOffModify;
            StandOffModify.UpdateMessageItemList();
            SiegeModify = generalData.siegeModify;
            SiegeModify.UpdateMessageItemList();
            EngagementModify = generalData.engagementModify;
            EngagementModify.UpdateMessageItemList();
            OpenBattleModify = generalData.openBattleModify;
            OpenBattleModify.UpdateMessageItemList();
            WithdrawModify = generalData.withdrawModify;
            WithdrawModify.UpdateMessageItemList();

            GeneralTraits = generalData.generalTraits;
            GeneralTactics = generalData.generalTactics;
        }

        public static General GetNoLeader(string tag) {
            General general = new General();

            general.GeneralName = "Noleader";
            general.GeneralDescription = "";
            general.GeneralTag = tag;

            general.OrganizationModify = 0;
            general.MaxCommandUnits = 0;
            general.VisibleGrid = 0;

            general.NormalModify = new UnitModify(-0.2f);
            general.StandOffModify = new UnitModify(-0.2f);
            general.SiegeModify = new UnitModify(-0.2f);
            general.EngagementModify = new UnitModify(-0.2f);
            general.OpenBattleModify = new UnitModify(-0.2f);
            general.WithdrawModify = new UnitModify(-0.2f);

            general.GeneralTraits = new List<GeneralTrait>();
            general.GeneralTactics = new List<Tactic>();

            return general;
        }

        // TODO: 获取将领的能力说明
        //public 

        #region 重载运算符
        public bool Equals(General general2) {
            return GeneralName == general2.GeneralName && GeneralTag == general2.GeneralTag;
        }

        public override bool Equals(object obj) {
            base.Equals(obj);
            General general2 = (General)obj;
            return GeneralName == general2.GeneralName && GeneralTag == general2.GeneralTag;
        }

        // NOTICE: 重载了运算符 就无法判空了！
        //public static bool operator ==(General general1, General general2) {
        //    if (general1 == null && general2 == null) {
        //        return true;
        //    }
        //    if((general1 == null && general2 != null) || (general1 != null && general2 == null)) {
        //        return false;
        //    }
        //    return general1.GeneralName == general2.GeneralName && general1.GeneralTag == general2.GeneralTag;
        //}

        //public static bool operator !=(General general1, General general2) {

        //    return !(general1 == general2);
        //}

        #endregion
    }

}