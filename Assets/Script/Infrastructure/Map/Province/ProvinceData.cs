using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Utils;

namespace WarGame_True.Infrastructure.Map.Provinces {
    [System.Serializable]
    public class ProvinceData {

        public uint provinceID = 0;
        public string provinceName = "";

        #region ������Ϣ

        // TODO: ��ʼ��ʱ����Ҫ����, ToString() kao lv ta
        public Vector3 provincePos = Vector3.zero;
        

        //��������
        private string provinceArea = "placeHolder";
        public string ProvinceArea { get => provinceArea; private set => provinceArea = value; }

        //����
        private Terrain terrain;
        public Terrain Terrain { get => terrain; private set => terrain = value; }

        //�Ƿ��к���
        private bool hasRiver;
        public bool HasRiver { get => hasRiver; private set => hasRiver = value; }

        //�ƶ�����
        private uint moveCost = 8;
        public uint MoveCost {
            get {
                return (uint)(moveCost * ReCaculateMoveCost());
            }
            private set => moveCost = value;
        }

        //���ݸ������������ƶ�����
        private float ReCaculateMoveCost() {
            float count = 1;

            // ����������ƶ���Ӱ��
            if (HasRiver) {
                count *= 1.1f;
            }

            //���ݵ��� �����ж�������
            switch (Terrain) {
                case Terrain.Plain:
                    break;
                case Terrain.Ocean:
                    count *= 1.75f;
                    break;
                case Terrain.City:
                    count *= 0.5f;
                    break;
                case Terrain.Desert:
                    count *= 2.0f;
                    break;
                case Terrain.Hill:
                    count *= 1.5f;
                    break;
                case Terrain.Mountainous:
                    count *= 1.25f;
                    break;
                case Terrain.Coast:
                    count *= 0.75f;
                    break;
            }

            return count;
        }


        //�������
        private float armyLoss = 0.01f;
        public float ArmyLoss { get => armyLoss; set => armyLoss = value; }


        #endregion

        #region ������Ϣ
        //�˿ڣ���λ�� k�����˿ھ�����˰����ʳ����������������
        private uint population;
        public uint Population { get => population; set => population = value; }
        public void GainPopulation() {

        }
        public void LossPopulation() {

        }


        //�������� -��Ȼ�������������� ���ᳬ�������˿ڵ�1 / 10
        private uint manPower;
        public uint ManPower { get => manPower; set => manPower = value; }
        public void GainManpower() {
            
        }
        public void LossManpower() {

        }
        public bool AbleToCostManpower(uint costNum) {
            return ManPower >= costNum;
        }

        //���ٶ�
        [Range(0, 100)]
        private uint prosperity;
        public uint Prosperity { get => prosperity; set => prosperity = value; }
        public void RecoverProsperity() {

        }
        public void LossProsperity() {

        }

        //�ķ϶ȣ���ս�����ֺ���ά�������������
        [Range(0, 100)]
        private uint desolation;
        public uint Desolation { get => desolation; set => desolation = value; }
        public void GainDesolation() {

        }
        public void LossDesolation() {

        }


        #region ˰�����
        // ��ǰʡ�ݵĽ��: ʡ��˰�ջ����µ׵��׽�⣬�����ǰ���գ���ֻ�ܻ��80%�������������Ч��etc���½�
        private float currentProvinceTax = 0;
        public float TaxStorage { get => Mathf.Round(currentProvinceTax); private set => currentProvinceTax = value; }
        /// <summary>
        /// ��ʡ�ݵ�˰�մ洢��˰����,ע�ᵽ�ս��¼���
        /// </summary>
        public void CollectTaxToStorage() {
            TaxStorage += GetProvinceTax_Day();
        }
        /// <summary>
        /// ��ǰ���գ����������Ч�ʵ��½����ڲ�ͬȨ��ֵʱ����ǰ���ջ��в�ͬ�ļ���
        /// </summary>
        public float CollectTaxStorage() {
            // TODO������Ȩ��ֵ����ȡ��ı���

            // �õ�ǰ�洢��0
            float rec = TaxStorage;
            TaxStorage = 0;

            // TODO������Ȩ��ֵ������ʡ�ݵ�����Ч�ʣ�������������

            // �����Ŷ�����
            rec *= UnityEngine.Random.Range(0.9f, 1.1f);

            return rec;
        }
        /// <summary>
        /// ��ȡʡ�ݵ�˰��-���½���-������˰Ӧ�����øú���
        /// </summary>
        public float GetProvinceTax_Month() {
            float rec = TaxStorage;
            TaxStorage = 0;
            return rec;
        }
        
        // ����˰��
        private float taxRate = 0.002f;
        public TaxLevel provinceTaxLevel = TaxLevel.Normal;
        public TaxLevel ProvinceTaxLevel { get => provinceTaxLevel; set => provinceTaxLevel = value; }
        public float TaxRate { get {
                //���ݵ�ǰʡ��˰�� ���Ļ���˰��
                switch (ProvinceTaxLevel) {
                    case TaxLevel.NoTax:
                        taxRate = 0.0005f;
                        break;
                    case TaxLevel.Low:
                        taxRate = 0.001f;
                        break;
                    case TaxLevel.Normal:
                        taxRate = 0.002f;
                        break;
                    case TaxLevel.High:
                        taxRate = 0.003f;
                        break;
                    case TaxLevel.Squeeze:
                        taxRate = 0.005f;
                        break;
                }
                return taxRate;
            } private set => taxRate = value; }
        /// <summary>
        /// ��ȡʡ�ݵ�˰��-���ս���-���뵱�ؽ�⣨�����˿ڡ�����Ч�ʡ�֧�ֶȡ��ΰ�������
        /// </summary>
        public float GetProvinceTax_Day() {
            // ����˰�� ǧ��֮2
            // (�˿ڻ��� * ����˰��) * (֧����/100) * (����Ч��/100 * 2) * (�ΰ�/100 * 1.25f) * ((���ٶ�/100)^3 * 1.5f) * Min(1, 1.3 - �ķ϶�/100)
            float taxNum = (Population * TaxRate)
                * (ApprovalRating * 0.01f * 2)
                * (AdminEfficiency * 0.01f)
                * (PublicSafety * 0.01f * 1.25f);
                //* Mathf.Pow(Prosperity * 0.01f, 3) * 1.5f
                //* Mathf.Min(1, 1.3f - Desolation * 0.01f);

            // ��ÿһ����Ŀ���ؾ���˰����Ϣ��
            taxMessage.SetStart("����", Population * TaxRate, true, true, 0, 0);
            taxMessage.AddMessage("֧����", ApprovalRating * 0.01f * 2 - 1, true, true, 0, 0);
            taxMessage.AddMessage("����Ч��", AdminEfficiency * 0.01f - 1, true, true, 0, 0);
            taxMessage.AddMessage("�ΰ�", PublicSafety * 0.01f * 1.25f - 1, true, true, 0, 0);
            //taxMessage.LoadMessageItem("���ٶ�", Mathf.Pow(Prosperity * 0.01f, 3) * 1.5f - 1);
            //taxMessage.LoadMessageItem("�ķ϶�", Mathf.Min(1, 1.3f - Desolation * 0.01f) - 1);
            taxMessage.SetEnd("�ܺ�", Mathf.Round(taxNum), true, true, 0, 0);

            return Mathf.Round(taxNum);
        }

        // ˰�յľ�����Ϣ
        public DetailMessage taxMessage = new DetailMessage();
        #endregion


        #region ��ʳ���
        //��ʳ��������λ ��û����ã�
        public uint GrainProduce;
        // ��ǰʡ�ݵ���ʳ�����: ���������µ׵�����⣬��ǰ���գ��������ģ��Լ�����etc���½�
        private uint grainMaxStorage = 1000;
        public uint GrainMaxStorage { get => grainMaxStorage; private set => grainMaxStorage = value; }
        private uint grainStorage = 0;
        public uint GrainStorage { get => grainStorage; private set => grainStorage = value; }
        public void CollectGrainToStorage() {
            GrainStorage = (uint)Mathf.Min(GrainMaxStorage, GrainStorage + GetGrainProduce_Day());
        }
        public float CollectGrainStorage() {
            
            // �õ�ǰ�洢��0
            float rec = GrainStorage;
            GrainStorage = 0;

            // �����Ŷ�����
            rec *= UnityEngine.Random.Range(0.9f, 1.1f);

            return rec;
        }
        public float GetProvinceGrain_Month() {
            float rec = GrainStorage;
            GrainStorage = 0;
            return rec;
        }

        // ��ʳ����
        public ExpropriateGrainLevel ExpropriateGrainLevel;
        private float expropriateGrainRate = 1;
        public float ExpropriateGrainRate { get { 
                // �������� ��������
                switch (ExpropriateGrainLevel) {
                    case ExpropriateGrainLevel.NoExpropriate:
                        expropriateGrainRate = 0.1f;
                        break;
                    case ExpropriateGrainLevel.Low:
                        expropriateGrainRate = 0.5f;
                        break;
                    case ExpropriateGrainLevel.Normal:
                        expropriateGrainRate = 1;
                        break;
                    case ExpropriateGrainLevel.High:
                        expropriateGrainRate = 1.5f;
                        break;
                    case ExpropriateGrainLevel.Squeeze:
                        expropriateGrainRate = 2f;
                        break;
                }
                return expropriateGrainRate;
            } private set => expropriateGrainRate = value; }
        /// <summary>
        /// ��ȡʡ�ݵ���ʳ����-���ս���-���뵱�����֣����ݻ�������������Ч�ʡ�֧�ֶȡ��ΰ�������
        /// </summary>
        public float GetGrainProduce_Day() {
            // (��ʳ���� * ������) * (֧����/100 * 2) * (����Ч��/100) * (�ΰ�/100) * ((���ٶ�/100)^3 * 2) * Min(1, 1 - �ķ϶�/100)
            float grainNum = GrainProduce * ExpropriateGrainRate
                * (ApprovalRating * 0.01f * 2)
                * (AdminEfficiency * 0.01f)
                * (PublicSafety * 0.01f);
                //* Mathf.Pow(Prosperity / 100, 3) * 2
                //* Mathf.Min(1, 1 - Desolation / 100);

            // ��ÿһ����Ŀ���ؾ�����ʳ������Ϣ��
            grainMessage.SetStart("����", GrainProduce * ExpropriateGrainRate);
            grainMessage.AddMessage("֧����", ApprovalRating * 0.01f * 2 - 1);
            grainMessage.AddMessage("����Ч��", AdminEfficiency * 0.01f - 1);
            grainMessage.AddMessage("�ΰ�", PublicSafety * 0.01f - 1);
            //grainMessage.LoadMessageItem("���ٶ�", Mathf.Pow(Prosperity / 100, 3) * 2 - 1);
            //grainMessage.LoadMessageItem("�ķ϶�", Mathf.Min(1, 1 - Desolation / 100) - 1);
            //grainMessage.AddMessage("��������", LocalGrainCost, usePercent: false);
            grainMessage.SetEnd("�ܺ�", Mathf.Round(grainNum));

            // ������ʳ���ģ��ڼ���������֮���ټ�������
            grainNum -= OtherGrainCost;

            return Mathf.Round(grainNum);
        }

        // �����ľ�����Ϣ
        public DetailMessage grainMessage = new DetailMessage();

        // TODO: д�����²���
        // ���ص���ʳ���ģ�һ������ ��ʳ�����ɱ����¼�������
        public int OtherGrainCost = 0;
        
        // ��ʳ���ĵľ�����Ϣ
        public DetailMessage grainCostMessage = new DetailMessage();

        #endregion


        #region ά����
        private MaintenanceLevel maintenanceLevel = MaintenanceLevel.Normal;
        public MaintenanceLevel ProvinceMateLevel { get => maintenanceLevel; set => maintenanceLevel = value; }
        private float maintenanceRate = 0.001f;
        public float MaintenanceRate { get {
                switch (ProvinceMateLevel) {
                    case MaintenanceLevel.VeryLow:
                        maintenanceRate = 0;
                        break;
                    case MaintenanceLevel.Low:
                        maintenanceRate = 0.0005f;
                        break;
                    case MaintenanceLevel.Normal:
                        maintenanceRate = 0.001f;
                        break;
                    case MaintenanceLevel.High:
                        maintenanceRate = 0.002f;
                        break;
                    case MaintenanceLevel.VeryHigh:
                        maintenanceRate = 0.003f;
                        break;
                }
                return maintenanceRate;
            } private set => maintenanceRate = value; }
        /// <summary>
        /// ��ȡʡ�ݵ�ά���ѣ������ΰ���֧�ֶȡ��ķ϶Ⱦ�����
        /// </summary>
        public float GetProvinceMaintenance() {
            // ����ά���� ǧ��֮1
            // (�˿ڻ��� * ����ά����) * (1.2f - ֧����/100) * ((1 - �ΰ�/100) * 1.25f) * (�ķ϶�/100)^3 * 3
            float maintenance = (Population * MaintenanceRate)
                * (1.2f - ApprovalRating * 0.01f)
                * ((1.2f - PublicSafety * 0.01f) * 1.25f)
                * Mathf.Pow(Desolation * 0.01f, 3) * 2;
            return maintenance;
        }
        /// <summary>
        /// ��ʡ�ݱ�ά�������ʧ�ܣ���ô�ᵼ��һϵ�к��
        /// </summary>
        public void LetProvinceMaintenance() {

        }
        public void FailProvinceMaintenance() {

        }

        #endregion


        #region �������
        // TODO: �������ÿ�ʱ��Ҫ��

        public bool HasCabage;

        public bool HasSupplyCenter;

        #endregion

        #endregion

        #region ������Ϣ
        //ʡ��ӵ����
        private string ownerTag;
        public string OwnerTag { get => ownerTag; set => ownerTag = value; }

        //��ǰ������
        private string currentControlTag;
        public string CurrentControlTag { get => currentControlTag; set => currentControlTag = value; }

        // �ΰ�
        [Range(0, 100)]
        private uint publicSafety;
        public uint PublicSafety { get => publicSafety; set => publicSafety = value; }

        // ����Ч��
        [Range(0, 100)]
        private uint adminEfficiency;
        public uint AdminEfficiency { get => adminEfficiency; set => adminEfficiency = value; }

        // ֧�ֶ�
        [Range(0, 100)]
        private uint approvalRating;
        public uint ApprovalRating { get => approvalRating; set => approvalRating = value; }
        
        // �Ƿ�ռ��
        public bool UnderControlByOwner() {
            return OwnerTag == CurrentControlTag;
        }

        public void SetControlStatu(string ctrlTag) {
            // TODO: ������һ��ռ�� ��ʡ�ݸ�����ֵ ����

            CurrentControlTag = ctrlTag;
        }

        public void SetOwnerStatu(string newOwnerTag) {
            // TODO: ������һ������ ��ʡ�ݸ�����ֵ ����

            OwnerTag = newOwnerTag;
        }
        #endregion

        #region ������Ϣ����������������ֵ�ļ���ó���

        // ���صĲ�����
        public float LocalSupply {
            get {
                
                // ���ݵ��β�ͬ���㱶��
                float ratio = 1;
                switch (terrain) {
                    case Terrain.City:
                        ratio *= 2;
                        break;
                    case Terrain.Plain:
                        break;
                    case Terrain.Hill:
                        ratio *= 0.8f;
                        break;
                    case Terrain.Desert:
                        ratio *= 0.2f;
                        break;
                    case Terrain.Mountainous:
                        ratio *= 0.5f;
                        break;
                    case Terrain.Coast:
                        ratio *= 1.2f;
                        break;
                }

                // ���ٶȡ��ķ϶ȡ�֧����
                ratio *= (1 + Prosperity * 0.01f) * (1 - Desolation * 0.01f) * ( ApprovalRating * 0.01f);

                // �����Ĳ�����Ϊ 10
                return 10 * ratio;
            }
        }

        public DetailMessage LocalSupplyMes = new DetailMessage();

        // ��ǰʡ��ӵ�еĽ���
        public List<Building> ProvinceBuildings = new List<Building>();

        public bool ExistBuilding(string buildingName) {
            Building ans = GetBuilding(buildingName);
            return ans != null ? true: false;
        }

        public Building GetBuilding(string buildingName) {
            for (int i = 0; i < ProvinceBuildings.Count; i++) {
                if (ProvinceBuildings[i].BuildingName == buildingName) {
                    return ProvinceBuildings[i];
                }
            }
            return null;
        }

        public void AddBuilding(Building building) {
            if (!ExistBuilding(building.BuildingName)) {
                // Ҫ����Ľ����ǲ������ģ���ע��
                if (building.IsSupplyCenter) {
                    PoliticLoader.Instance.GetFactionByTag(OwnerTag)
                        .RegisterSupplyCenter(provinceID);
                }

                ProvinceBuildings.Add(building);
            }
        }

        public void RemoveBuilding(Building building) {
            if (ExistBuilding(building.BuildingName)) {
                // �Ƴ�
                if (building.IsSupplyCenter) {
                    PoliticLoader.Instance.GetFactionByTag(OwnerTag)
                        .UnregisterSupplyCenter(provinceID);
                }
                ProvinceBuildings.Remove(building);
            }
        }

        public bool ExistSupplyCenter() {
            // ֱ�ӱ������н���������ֶ�
            for (int i = 0; i < ProvinceBuildings.Count; i++) {
                if (ProvinceBuildings[i].IsSupplyCenter) {
                    return true;
                }
            }
            return false;
        }

        // TODO: Ӧ���� ʡ������
        public ProvinceModify CurProvinceModify = new ProvinceModify();

        /// <summary>
        /// ����ʡ�ݵ�����,ע�ᵽ�ս��¼���
        /// </summary>
        public void UpdateProvinceModify() {

            // ����ǰ���е�ʡ���������
            CurProvinceModify.ResetProvinceModify();
            foreach (var building in ProvinceBuildings)
            {
                CurProvinceModify += building.ProvinceModify;
            }
        }

        #endregion


        public ProvinceData() { }
        
        /// <summary>
        /// set properties according to the str(csv line)
        /// </summary>
        public ProvinceData(string str) {
            string[] provincePros = str.Split(",");

            // TODO: ����������
            if (provincePros.Length < 5) {
                provinceID = 10001;     // ��ʾ����ID
                return;
            }

            provinceID = uint.Parse(provincePros[0]);
            provinceName = provincePros[1];
            
            try {
                provincePos = provincePros[2].TryTransToVector3();
                ProvinceArea = provincePros[3];
                Terrain = (Terrain)Enum.Parse(typeof(Terrain), provincePros[4]);
                HasRiver = bool.Parse(provincePros[5]);
                MoveCost = uint.Parse(provincePros[6]);
            } catch (Exception ex) {
                ProvinceArea = "failToLoad";
                Terrain = Terrain.Plain;
                HasRiver = false;
                MoveCost = 10;
            }

            try {
                Population = uint.Parse(provincePros[7]);
                ManPower = uint.Parse(provincePros[8]);
                Prosperity = uint.Parse(provincePros[9]);
                Desolation = uint.Parse(provincePros[10]);
                GrainProduce = uint.Parse(provincePros[11]);
            } catch {
                Population = 1;
                ManPower = 1;
                Prosperity = 1;
                Desolation = 1;
                GrainProduce = 1;
            }

            try {
                TaxRate = float.Parse(provincePros[12]);
                MaintenanceRate = float.Parse(provincePros[13]);
                TaxStorage = float.Parse(provincePros[14]);
                GrainStorage = uint.Parse(provincePros[15]);
                GrainMaxStorage = uint.Parse(provincePros[16]);
                ExpropriateGrainRate = float.Parse(provincePros[17]);
            } catch {
                TaxRate = 1;
                MaintenanceRate = 1;
                TaxStorage = 1;
                GrainStorage = 1;
                GrainMaxStorage = 1;
                ExpropriateGrainRate = 1;
            }

            try {
                OwnerTag = provincePros[18];
                CurrentControlTag = provincePros[19];
                PublicSafety = uint.Parse(provincePros[20]);
                AdminEfficiency = uint.Parse(provincePros[21]);
                ApprovalRating = uint.Parse(provincePros[22]);
            } catch {
                OwnerTag = "fail_load";
                CurrentControlTag = "fail_load";
                PublicSafety = 1;
                AdminEfficiency = 1;
                ApprovalRating = 1;
            }

        }

        public static ProvinceData GetRandomProvinceData(uint id, Vector3 position) {
            ProvinceData provinceData = new ProvinceData();
            //provinceData.provinceGeo = ProvinceGeo.GetRandomProvinceGeo();
            //provinceData.provinceEco = ProvinceEco.GetRandomProvinceEco();
            //provinceData.provincePol = ProvincePol.GetRandomProvincePol();

            provinceData.provinceID = id;
            provinceData.provinceName = "";

            #region �����������Ϣ
            provinceData.provincePos = position;
            provinceData.ProvinceArea = "no area";
            // ���ѡ��һ��ö��ֵ
            Terrain[] enumValues = (Terrain[])Enum.GetValues(typeof(Terrain));
            System.Random random = new System.Random();
            provinceData.Terrain = enumValues[random.Next(enumValues.Length)];
            provinceData.HasRiver = random.Next(2) == 1;
            #endregion

            #region �����������Ϣ
            provinceData.Population = (uint)UnityEngine.Random.Range(10000, 500000);
            provinceData.ManPower = provinceData.population / 10;

            provinceData.Prosperity = (uint)UnityEngine.Random.Range(1, 101);
            provinceData.Desolation = (uint)UnityEngine.Random.Range(1, 101);

            // ��ʳ����
            provinceData.GrainProduce = (uint)UnityEngine.Random.Range(800, 1201);
            #endregion

            #region �����������Ϣ
            provinceData.OwnerTag = "no owner";
            provinceData.CurrentControlTag = "no controller";
            provinceData.PublicSafety = (uint)UnityEngine.Random.Range(60, 81);
            provinceData.AdminEfficiency = (uint)UnityEngine.Random.Range(60, 81);
            provinceData.ApprovalRating = (uint)UnityEngine.Random.Range(60, 81);
            #endregion

            return provinceData;
        }

        public override string ToString() {
            return $"{provinceID},{provinceName}," +
                $"{provincePos.TransToString()},{ProvinceArea},{Terrain},{HasRiver},{MoveCost}," 
                + $"{Population},{ManPower},{Prosperity},{Desolation},{GrainProduce}," +
                $"{TaxRate},{MaintenanceRate},{TaxStorage},{GrainStorage},{GrainMaxStorage},{ExpropriateGrainRate}," 
                + $"{OwnerTag},{CurrentControlTag},{PublicSafety},{AdminEfficiency},{ApprovalRating}";
        }

        
    }

    public enum TaxLevel {
        NoTax,      // û��˰�� 0
        Low,        // 0.001f
        Normal,     // 0.002f
        High,       // 0.003f
        Squeeze     // 0.005f
    }


    public enum MaintenanceLevel {
        VeryLow,
        Low,
        Normal,
        High,
        VeryHigh
    }

    public enum ExpropriateGrainLevel {
        NoExpropriate,
        Low,
        Normal,
        High,
        Squeeze
    }

    // Deprecated -- 
    /// <summary>
    /// ʡ�ݵĵ�����Ϣ
    /// </summary>
    [System.Serializable]
    public class ProvinceGeo {
        //��������
        public string provinceArea = "placeHolder";
        //����
        public Terrain terrain;
        //�Ƿ��к���
        public bool hasRiver;

        #region �ƶ�����
        //�ƶ�����
        public uint moveCost = 8;
        //���ݸ������������ƶ�����
        private void ReCaculateMoveCost() {
            float count = 1;


            if (hasRiver) {
                count *= 1.1f;
            }

            //���ݵ��� �����ж�������
            switch (terrain) {
                case Terrain.Plain:
                    break;
                case Terrain.Ocean:
                    count *= 1.75f;
                    break;
                case Terrain.City:
                    count *= 0.5f;
                    break;
                case Terrain.Desert:
                    count *= 2.0f;
                    break;
                case Terrain.Hill:
                    count *= 1.5f;
                    break;
                case Terrain.Mountainous:
                    count *= 1.25f;
                    break;
                case Terrain.Coast:
                    count *= 0.75f;
                    break;
            }

            float moveCost = this.moveCost;
            moveCost *= count;
            this.moveCost = (uint)moveCost;
        }
        #endregion

        public ProvinceGeo() { }

        public static ProvinceGeo GetRandomProvinceGeo() {
            ProvinceGeo provinceGeo = new ProvinceGeo();

            // ���ѡ��һ��ö��ֵ
            Terrain[] enumValues = (Terrain[])Enum.GetValues(typeof(Terrain));
            System.Random random = new System.Random();
            provinceGeo.terrain = enumValues[random.Next(enumValues.Length)];

            provinceGeo.hasRiver = random.Next(2) == 1;

            //�����ƶ�����
            provinceGeo.ReCaculateMoveCost();
            return provinceGeo;
        }

        public ProvinceGeo(string provinceGeo) {

        }

        public override string ToString() {
            ReCaculateMoveCost();
            return $"{provinceArea},{terrain},{hasRiver},{moveCost}";
        }

        //���ڲ���

        //�Ƿ��о��½���

    }

    /// <summary>
    /// ʡ�ݵľ�����Ϣ
    /// </summary>
    [System.Serializable]
    public class ProvinceEco {

        //�˿ڣ���λ�� k��
        public uint population;
        //��������
        public uint manPower;

        //���ٶ�
        [Range(0, 100)]
        public uint prosperity;

        //�ķ϶ȣ���ս�����ֺ���������
        [Range(0, 100)]
        public uint desolation;

        //��ʳ��������λ ��û����ã�
        public uint grainProduce;

        //���� ��Ʒ �������Ρ�������ƥ�����󡢲�ƥ�����


        //˰�գ���Ӧ�÷���������㣬Ӧ�ø����˿ڡ�����Ч�ʡ�֧�ֶȡ��ΰ�������

        //ά���ѣ������ΰ���֧�ֶȡ��ķ϶Ⱦ�����

        public ProvinceEco() { }

        public static ProvinceEco GetRandomProvinceEco() {
            ProvinceEco provinceEco = new ProvinceEco();
            provinceEco.population = (uint)UnityEngine.Random.Range(10000, 100000);
            provinceEco.manPower = provinceEco.population /10;

            provinceEco.prosperity = (uint)UnityEngine.Random.Range(1, 101);
            provinceEco.desolation = (uint)UnityEngine.Random.Range(1, 101);

            provinceEco.grainProduce = (uint)UnityEngine.Random.Range(1, 101);

            return provinceEco;
        }

        public ProvinceEco(string provinceEco) {

        }

        public override string ToString() {
            return $"{population},{manPower},{prosperity},{desolation},{grainProduce}";
        }
    }

    /// <summary>
    /// ʡ�ݵ�������Ϣ
    /// </summary>
    [System.Serializable]
    public class ProvincePol {
        //ʡ��ӵ����
        public string controlTag;
        //��ǰ������
        public string currentControlTag;

        //�ΰ�
        [Range(0, 100)]
        public uint publicSafety;

        //����Ч��
        [Range(0, 100)]
        public uint adminEfficiency;

        //֧�ֶ�
        [Range(0, 100)]
        public uint approvalRating;

        public ProvincePol() { }

        public static ProvincePol GetRandomProvincePol() {
            ProvincePol provincePol = new ProvincePol();

            provincePol.controlTag = "no owner";
            provincePol.currentControlTag = "no controller";
            provincePol.publicSafety = (uint)UnityEngine.Random.Range(1, 101);
            provincePol.adminEfficiency = (uint)UnityEngine.Random.Range(1, 101);
            provincePol.approvalRating = (uint)UnityEngine.Random.Range(1, 101);

            return provincePol;
        }

        public ProvincePol(string provincePol) {

        }

        public override string ToString() {
            return $"{controlTag},{currentControlTag},{publicSafety},{adminEfficiency},{approvalRating}";
        }

    }


}