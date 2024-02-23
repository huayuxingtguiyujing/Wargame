using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.Infrastructure.UIOrganizedMess;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.CombatPart {
    /// <summary>
    /// �����࣬�����˽����������Ϣ
    /// </summary>
    [System.Serializable]
    public class General {

        string generalName;

        string generalChineseName;

        string generalDescription;

        string generalTag;              //��������������
        public string GeneralName { get => generalName;private set => generalName = value; }
        public string GeneralChineseName { get => generalChineseName; private set => generalChineseName = value; }
        public string GeneralDescription { get => generalDescription; private set => generalDescription = value; }
        public string GeneralTag { get => generalTag; private set => generalTag = value; }

        // �Ծ��ӵ�����
        float organizationModify;
        uint maxCommandUnits;
        int visibleGrid;
        public float OrganizationModify { get => organizationModify; private set => organizationModify = value; }
        public uint MaxCommandUnits { get => maxCommandUnits; private set => maxCommandUnits = value; }
        public int VisibleGrid { get => visibleGrid; private set => visibleGrid = value; }

        // ��ͬս���׶��¶Ծ��ӵ�λ������
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

        #region �������� �������� �ľ�̬�ֶ�
        // д���е���̱˵ʵ��
        public static string NormalModifyDescrip = "���� ";
        public static string StandOffModifyDescrip = "���Ž׶� ";
        public static string EngagementModifyDescrip = "��ս�׶� ";
        public static string OpenBattleModifyDescrip = "��ս�׶� ";
        public static string SiegeModifyDescrip = "Χ�ǽ׶� ";
        public static string WithdrawModifyDescrip = "���˽׶� ";
        #endregion

        public DetailMessage generalModifyMessage { get; private set; }
        // TODO: �ڽ������ ����Ͻ��������������
        private void UpdateModifyMessage() {
            generalModifyMessage.AddMessage(NormalModifyDescrip, NormalModify.ModifyMessage);
            generalModifyMessage.AddMessage(StandOffModifyDescrip, StandOffModify.ModifyMessage);
            generalModifyMessage.AddMessage(EngagementModifyDescrip, EngagementModify.ModifyMessage);
            generalModifyMessage.AddMessage(OpenBattleModifyDescrip, OpenBattleModify.ModifyMessage);
            generalModifyMessage.AddMessage(SiegeModifyDescrip, SiegeModify.ModifyMessage);
            generalModifyMessage.AddMessage(WithdrawModifyDescrip, WithdrawModify.ModifyMessage);
        }

        // TODO��
        // �������� - ���ݶԾ��ӵ�λ���������
        public List<GeneralTrait> GeneralTraits { get; private set; }

        // ����ɲ��õ�ս��
        public List<Tactic> GeneralTactics { get; private set; }

        public General() { }

        /// <summary>
        /// ʹ��Ԥ����Ľ�����ϢScriptableObject��ʼ��
        /// </summary>
        public General(GeneralData generalData) {
            GeneralName = generalData.generalName;
            GeneralChineseName = generalData.generalChineseName;
            GeneralDescription = generalData.generalDescription;
            GeneralTag = generalData.generalTag;

            OrganizationModify = generalData.organizationModify;
            MaxCommandUnits = generalData.maxCommandUnits;
            VisibleGrid = generalData.visibleGrid;

            //Debug.Log("�������: modify:" + (generalData.engagementModify == null));
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

        // TODO: ��ȡ���������˵��
        //public 

        #region ���������
        public bool Equals(General general2) {
            return GeneralName == general2.GeneralName && GeneralTag == general2.GeneralTag;
        }

        public override bool Equals(object obj) {
            base.Equals(obj);
            General general2 = (General)obj;
            return GeneralName == general2.GeneralName && GeneralTag == general2.GeneralTag;
        }

        // NOTICE: ����������� ���޷��п��ˣ�
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