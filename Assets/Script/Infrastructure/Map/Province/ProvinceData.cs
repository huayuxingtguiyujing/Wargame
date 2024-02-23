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

        #region 地理信息

        // TODO: 初始化时候需要考虑, ToString() kao lv ta
        public Vector3 provincePos = Vector3.zero;
        

        //地理区域
        private string provinceArea = "placeHolder";
        public string ProvinceArea { get => provinceArea; private set => provinceArea = value; }

        //地形
        private Terrain terrain;
        public Terrain Terrain { get => terrain; private set => terrain = value; }

        //是否有河流
        private bool hasRiver;
        public bool HasRiver { get => hasRiver; private set => hasRiver = value; }

        //移动花费
        private uint moveCost = 8;
        public uint MoveCost {
            get {
                return (uint)(moveCost * ReCaculateMoveCost());
            }
            private set => moveCost = value;
        }

        //根据各项条件更新移动花费
        private float ReCaculateMoveCost() {
            float count = 1;

            // 计算河流对移动的影响
            if (HasRiver) {
                count *= 1.1f;
            }

            //根据地形 计算行动消耗量
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


        //军队损耗
        private float armyLoss = 0.01f;
        public float ArmyLoss { get => armyLoss; set => armyLoss = value; }


        #endregion

        #region 经济信息
        //人口（单位： k）（人口决定赋税、粮食生产、可用人力）
        private uint population;
        public uint Population { get => population; set => population = value; }
        public void GainPopulation() {

        }
        public void LossPopulation() {

        }


        //可用人力 -自然增长的人力上限 不会超过当地人口的1 / 10
        private uint manPower;
        public uint ManPower { get => manPower; set => manPower = value; }
        public void GainManpower() {
            
        }
        public void LossManpower() {

        }
        public bool AbleToCostManpower(uint costNum) {
            return ManPower >= costNum;
        }

        //繁荣度
        [Range(0, 100)]
        private uint prosperity;
        public uint Prosperity { get => prosperity; set => prosperity = value; }
        public void RecoverProsperity() {

        }
        public void LossProsperity() {

        }

        //荒废度（随战争、灾害、维护不足而提升）
        [Range(0, 100)]
        private uint desolation;
        public uint Desolation { get => desolation; set => desolation = value; }
        public void GainDesolation() {

        }
        public void LossDesolation() {

        }


        #region 税收相关
        // 当前省份的金库: 省份税收会在月底到底金库，如果提前征收，则只能获得80%，并且造成行政效率etc的下降
        private float currentProvinceTax = 0;
        public float TaxStorage { get => Mathf.Round(currentProvinceTax); private set => currentProvinceTax = value; }
        /// <summary>
        /// 将省份的税收存储到税库里,注册到日结事件中
        /// </summary>
        public void CollectTaxToStorage() {
            TaxStorage += GetProvinceTax_Day();
        }
        /// <summary>
        /// 提前征收，会造成行政效率的下降，在不同权威值时，提前征收会有不同的减成
        /// </summary>
        public float CollectTaxStorage() {
            // TODO：根据权威值，获取损耗倍率

            // 让当前存储归0
            float rec = TaxStorage;
            TaxStorage = 0;

            // TODO：根据权威值，降低省份的行政效率，或者其他减成

            // 引入扰动因子
            rec *= UnityEngine.Random.Range(0.9f, 1.1f);

            return rec;
        }
        /// <summary>
        /// 获取省份的税收-按月结算-正常收税应当调用该函数
        /// </summary>
        public float GetProvinceTax_Month() {
            float rec = TaxStorage;
            TaxStorage = 0;
            return rec;
        }
        
        // 基础税率
        private float taxRate = 0.002f;
        public TaxLevel provinceTaxLevel = TaxLevel.Normal;
        public TaxLevel ProvinceTaxLevel { get => provinceTaxLevel; set => provinceTaxLevel = value; }
        public float TaxRate { get {
                //根据当前省份税率 更改基础税率
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
        /// 获取省份的税收-按日结算-进入当地金库（根据人口、行政效率、支持度、治安决定）
        /// </summary>
        public float GetProvinceTax_Day() {
            // 基础税率 千分之2
            // (人口基数 * 基础税率) * (支持率/100) * (行政效率/100 * 2) * (治安/100 * 1.25f) * ((繁荣度/100)^3 * 1.5f) * Min(1, 1.3 - 荒废度/100)
            float taxNum = (Population * TaxRate)
                * (ApprovalRating * 0.01f * 2)
                * (AdminEfficiency * 0.01f)
                * (PublicSafety * 0.01f * 1.25f);
                //* Mathf.Pow(Prosperity * 0.01f, 3) * 1.5f
                //* Mathf.Min(1, 1.3f - Desolation * 0.01f);

            // 将每一项条目加载具体税收信息中
            taxMessage.SetStart("基础", Population * TaxRate, true, true, 0, 0);
            taxMessage.AddMessage("支持率", ApprovalRating * 0.01f * 2 - 1, true, true, 0, 0);
            taxMessage.AddMessage("行政效率", AdminEfficiency * 0.01f - 1, true, true, 0, 0);
            taxMessage.AddMessage("治安", PublicSafety * 0.01f * 1.25f - 1, true, true, 0, 0);
            //taxMessage.LoadMessageItem("繁荣度", Mathf.Pow(Prosperity * 0.01f, 3) * 1.5f - 1);
            //taxMessage.LoadMessageItem("荒废度", Mathf.Min(1, 1.3f - Desolation * 0.01f) - 1);
            taxMessage.SetEnd("总和", Mathf.Round(taxNum), true, true, 0, 0);

            return Mathf.Round(taxNum);
        }

        // 税收的具体信息
        public DetailMessage taxMessage = new DetailMessage();
        #endregion


        #region 粮食相关
        //粮食产出（单位 还没有想好）
        public uint GrainProduce;
        // 当前省份的粮食最大储量: 储量会在月底到达国库，提前征收，会造成损耗，以及民心etc的下降
        private uint grainMaxStorage = 1000;
        public uint GrainMaxStorage { get => grainMaxStorage; private set => grainMaxStorage = value; }
        private uint grainStorage = 0;
        public uint GrainStorage { get => grainStorage; private set => grainStorage = value; }
        public void CollectGrainToStorage() {
            GrainStorage = (uint)Mathf.Min(GrainMaxStorage, GrainStorage + GetGrainProduce_Day());
        }
        public float CollectGrainStorage() {
            
            // 让当前存储归0
            float rec = GrainStorage;
            GrainStorage = 0;

            // 引入扰动因子
            rec *= UnityEngine.Random.Range(0.9f, 1.1f);

            return rec;
        }
        public float GetProvinceGrain_Month() {
            float rec = GrainStorage;
            GrainStorage = 0;
            return rec;
        }

        // 粮食征收
        public ExpropriateGrainLevel ExpropriateGrainLevel;
        private float expropriateGrainRate = 1;
        public float ExpropriateGrainRate { get { 
                // 根据征率 调整粮产
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
        /// 获取省份的粮食产量-按日结算-进入当地粮仓（根据基础产量、行政效率、支持度、治安决定）
        /// </summary>
        public float GetGrainProduce_Day() {
            // (粮食产量 * 征收率) * (支持率/100 * 2) * (行政效率/100) * (治安/100) * ((繁荣度/100)^3 * 2) * Min(1, 1 - 荒废度/100)
            float grainNum = GrainProduce * ExpropriateGrainRate
                * (ApprovalRating * 0.01f * 2)
                * (AdminEfficiency * 0.01f)
                * (PublicSafety * 0.01f);
                //* Mathf.Pow(Prosperity / 100, 3) * 2
                //* Mathf.Min(1, 1 - Desolation / 100);

            // 将每一项条目加载具体粮食产量信息中
            grainMessage.SetStart("基础", GrainProduce * ExpropriateGrainRate);
            grainMessage.AddMessage("支持率", ApprovalRating * 0.01f * 2 - 1);
            grainMessage.AddMessage("行政效率", AdminEfficiency * 0.01f - 1);
            grainMessage.AddMessage("治安", PublicSafety * 0.01f - 1);
            //grainMessage.LoadMessageItem("繁荣度", Mathf.Pow(Prosperity / 100, 3) * 2 - 1);
            //grainMessage.LoadMessageItem("荒废度", Mathf.Min(1, 1 - Desolation / 100) - 1);
            //grainMessage.AddMessage("当地消耗", LocalGrainCost, usePercent: false);
            grainMessage.SetEnd("总和", Mathf.Round(grainNum));

            // 本地粮食消耗，在计算完以上之后，再计算消耗
            grainNum -= OtherGrainCost;

            return Mathf.Round(grainNum);
        }

        // 粮产的具体信息
        public DetailMessage grainMessage = new DetailMessage();

        // TODO: 写完以下部分
        // 本地的粮食消耗（一般来讲 粮食消耗由本地事件等引起）
        public int OtherGrainCost = 0;
        
        // 粮食消耗的具体信息
        public DetailMessage grainCostMessage = new DetailMessage();

        #endregion


        #region 维护费
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
        /// 获取省份的维护费（根据治安、支持度、荒废度决定）
        /// </summary>
        public float GetProvinceMaintenance() {
            // 基础维护费 千分之1
            // (人口基数 * 基础维护费) * (1.2f - 支持率/100) * ((1 - 治安/100) * 1.25f) * (荒废度/100)^3 * 3
            float maintenance = (Population * MaintenanceRate)
                * (1.2f - ApprovalRating * 0.01f)
                * ((1.2f - PublicSafety * 0.01f) * 1.25f)
                * Mathf.Pow(Desolation * 0.01f, 3) * 2;
            return maintenance;
        }
        /// <summary>
        /// 让省份被维护，如果失败，那么会导致一系列后果
        /// </summary>
        public void LetProvinceMaintenance() {

        }
        public void FailProvinceMaintenance() {

        }

        #endregion


        #region 建筑相关
        // TODO: 做建筑该块时需要改

        public bool HasCabage;

        public bool HasSupplyCenter;

        #endregion

        #endregion

        #region 政治信息
        //省份拥有者
        private string ownerTag;
        public string OwnerTag { get => ownerTag; set => ownerTag = value; }

        //当前控制者
        private string currentControlTag;
        public string CurrentControlTag { get => currentControlTag; set => currentControlTag = value; }

        // 治安
        [Range(0, 100)]
        private uint publicSafety;
        public uint PublicSafety { get => publicSafety; set => publicSafety = value; }

        // 行政效率
        [Range(0, 100)]
        private uint adminEfficiency;
        public uint AdminEfficiency { get => adminEfficiency; set => adminEfficiency = value; }

        // 支持度
        [Range(0, 100)]
        private uint approvalRating;
        public uint ApprovalRating { get => approvalRating; set => approvalRating = value; }
        
        // 是否被占领
        public bool UnderControlByOwner() {
            return OwnerTag == CurrentControlTag;
        }

        public void SetControlStatu(string ctrlTag) {
            // TODO: 经历了一次占领 将省份各项数值 调低

            CurrentControlTag = ctrlTag;
        }

        public void SetOwnerStatu(string newOwnerTag) {
            // TODO: 经历了一次易主 将省份各项数值 调低

            OwnerTag = newOwnerTag;
        }
        #endregion

        #region 其他信息（经过以上三个数值的计算得出）

        // 本地的补给度
        public float LocalSupply {
            get {
                
                // 根据地形不同计算倍率
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

                // 繁荣度、荒废度、支持率
                ratio *= (1 + Prosperity * 0.01f) * (1 - Desolation * 0.01f) * ( ApprovalRating * 0.01f);

                // 基础的补给度为 10
                return 10 * ratio;
            }
        }

        public DetailMessage LocalSupplyMes = new DetailMessage();

        // 当前省份拥有的建筑
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
                // 要建造的建筑是补给中心，则注册
                if (building.IsSupplyCenter) {
                    PoliticLoader.Instance.GetFactionByTag(OwnerTag)
                        .RegisterSupplyCenter(provinceID);
                }

                ProvinceBuildings.Add(building);
            }
        }

        public void RemoveBuilding(Building building) {
            if (ExistBuilding(building.BuildingName)) {
                // 移除
                if (building.IsSupplyCenter) {
                    PoliticLoader.Instance.GetFactionByTag(OwnerTag)
                        .UnregisterSupplyCenter(provinceID);
                }
                ProvinceBuildings.Remove(building);
            }
        }

        public bool ExistSupplyCenter() {
            // 直接遍历所有建筑，检查字段
            for (int i = 0; i < ProvinceBuildings.Count; i++) {
                if (ProvinceBuildings[i].IsSupplyCenter) {
                    return true;
                }
            }
            return false;
        }

        // TODO: 应用上 省份修正
        public ProvinceModify CurProvinceModify = new ProvinceModify();

        /// <summary>
        /// 更新省份的修正,注册到日结事件里
        /// </summary>
        public void UpdateProvinceModify() {

            // 将当前所有的省份修正相加
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

            // TODO: 处理出错情况
            if (provincePros.Length < 5) {
                provinceID = 10001;     // 表示错误ID
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

            #region 随机化地理信息
            provinceData.provincePos = position;
            provinceData.ProvinceArea = "no area";
            // 随机选择一个枚举值
            Terrain[] enumValues = (Terrain[])Enum.GetValues(typeof(Terrain));
            System.Random random = new System.Random();
            provinceData.Terrain = enumValues[random.Next(enumValues.Length)];
            provinceData.HasRiver = random.Next(2) == 1;
            #endregion

            #region 随机化经济信息
            provinceData.Population = (uint)UnityEngine.Random.Range(10000, 500000);
            provinceData.ManPower = provinceData.population / 10;

            provinceData.Prosperity = (uint)UnityEngine.Random.Range(1, 101);
            provinceData.Desolation = (uint)UnityEngine.Random.Range(1, 101);

            // 粮食产量
            provinceData.GrainProduce = (uint)UnityEngine.Random.Range(800, 1201);
            #endregion

            #region 随机化政治信息
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
        NoTax,      // 没有税收 0
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
    /// 省份的地理信息
    /// </summary>
    [System.Serializable]
    public class ProvinceGeo {
        //地理区域
        public string provinceArea = "placeHolder";
        //地形
        public Terrain terrain;
        //是否有河流
        public bool hasRiver;

        #region 移动花费
        //移动花费
        public uint moveCost = 8;
        //根据各项条件更新移动花费
        private void ReCaculateMoveCost() {
            float count = 1;


            if (hasRiver) {
                count *= 1.1f;
            }

            //根据地形 计算行动消耗量
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

            // 随机选择一个枚举值
            Terrain[] enumValues = (Terrain[])Enum.GetValues(typeof(Terrain));
            System.Random random = new System.Random();
            provinceGeo.terrain = enumValues[random.Next(enumValues.Length)];

            provinceGeo.hasRiver = random.Next(2) == 1;

            //计算移动消耗
            provinceGeo.ReCaculateMoveCost();
            return provinceGeo;
        }

        public ProvinceGeo(string provinceGeo) {

        }

        public override string ToString() {
            ReCaculateMoveCost();
            return $"{provinceArea},{terrain},{hasRiver},{moveCost}";
        }

        //后勤补给

        //是否有军事建筑

    }

    /// <summary>
    /// 省份的经济信息
    /// </summary>
    [System.Serializable]
    public class ProvinceEco {

        //人口（单位： k）
        public uint population;
        //可用人力
        public uint manPower;

        //繁荣度
        [Range(0, 100)]
        public uint prosperity;

        //荒废度（随战争、灾害而提升）
        [Range(0, 100)]
        public uint desolation;

        //粮食产出（单位 还没有想好）
        public uint grainProduce;

        //其他 产品 产出（盐、铁、马匹、牲畜、布匹、金矿）


        //税收（不应该放在这里计算，应该根据人口、行政效率、支持度、治安决定）

        //维护费（根据治安、支持度、荒废度决定）

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
    /// 省份的政治信息
    /// </summary>
    [System.Serializable]
    public class ProvincePol {
        //省份拥有者
        public string controlTag;
        //当前控制者
        public string currentControlTag;

        //治安
        [Range(0, 100)]
        public uint publicSafety;

        //行政效率
        [Range(0, 100)]
        public uint adminEfficiency;

        //支持度
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