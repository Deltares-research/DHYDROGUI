using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class UnpavedDataRow : RainfallRunoffDataRow<UnpavedData>
    {
        [Description("Area Id")]
        public string AreaName
        {
            get { return data.Name; }
        }

        [Description("Groundwater Area (m²)")]
        public double TotalAreaForGroundWaterCalculations
        {
            get { return data.TotalAreaForGroundWaterCalculations; }
            set { data.TotalAreaForGroundWaterCalculations = value; }
        }

        [Description("Area Grass (m²)")]
        public double GrassArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Grass); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Grass, value); }
        }

        [Description("Area Corn (m²)")]
        public double CornArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Corn); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Corn, value); }
        }

        [Description("Area Potatoes (m²)")]
        public double PotatoesArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Potatoes); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Potatoes, value); }
        }

        [Description("Area Sugarbeet (m²)")]
        public double SugarbeetArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Sugarbeet); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Sugarbeet, value); }
        }

        [Description("Area Grain (m²)")]
        public double GrainArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Grain); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Grain, value); }
        }

        [Description("Area Miscellaneous (m²)")]
        public double MiscellaneousArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Miscellaneous); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Miscellaneous, value); }
        }

        [Description("Area Non Arable (m²)")]
        public double NonArableArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.NonArableLand); }
            set { SetAreaForCrop(UnpavedEnums.CropType.NonArableLand, value); }
        }

        [Description("Area Greenhouse (m²)")]
        public double GreenhouseArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.GreenhouseArea); }
            set { SetAreaForCrop(UnpavedEnums.CropType.GreenhouseArea, value); }
        }

        [Description("Area Orchard (m²)")]
        public double OrchardArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Orchard); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Orchard, value); }
        }

        [Description("Area Bulbous Plants (m²)")]
        public double BulbousPlantsArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.BulbousPlants); }
            set { SetAreaForCrop(UnpavedEnums.CropType.BulbousPlants, value); }
        }

        [Description("Area Foliage Forest (m²)")]
        public double FoliageForestArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.FoliageForest); }
            set { SetAreaForCrop(UnpavedEnums.CropType.FoliageForest, value); }
        }

        [Description("Area Pine Forest (m²)")]
        public double PineForestArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.PineForest); }
            set { SetAreaForCrop(UnpavedEnums.CropType.PineForest, value); }
        }

        [Description("Area Nature (m²)")]
        public double NatureArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Nature); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Nature, value); }
        }

        [Description("Area Fallow (m²)")]
        public double FallowArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Fallow); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Fallow, value); }
        }

        [Description("Area Vegetables (m²)")]
        public double VegetablesArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Vegetables); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Vegetables, value); }
        }

        [Description("Area Flowers (m²)")]
        public double FlowersArea
        {
            get { return GetAreaForCrop(UnpavedEnums.CropType.Flowers); }
            set { SetAreaForCrop(UnpavedEnums.CropType.Flowers, value); }
        }

        [Description("Surface Level (m AD)")]
        public double SurfaceLevel
        {
            get { return data.SurfaceLevel; }
            set { data.SurfaceLevel = value; }
        }

        [Description("Soil Type (-)")]
        public UnpavedEnums.SoilType SoilType
        {
            get { return data.SoilType; }
            set { data.SoilType = value; }
        }

        [Description("Soil Type CapSim (-)")]
        public UnpavedEnums.SoilTypeCapsim SoilTypeCapsim
        {
            get { return data.SoilTypeCapsim; }
            set { data.SoilTypeCapsim = value; } 
        }

        [Description("Thickness Groundwater Layer (m)")]
        public double GroundWaterLayerThickness
        {
            get { return data.GroundWaterLayerThickness; }
            set { data.GroundWaterLayerThickness = value; }
        }

        [Description("Maximum Allowed Groundwater Level (m AD)")]
        public double MaximumAllowedGroundWaterLevel
        {
            get { return data.MaximumAllowedGroundWaterLevel; }
            set { data.MaximumAllowedGroundWaterLevel = value; }
        }

        [Description("Initial Groundwater Level Type")]
        public UnpavedEnums.GroundWaterSourceType InitialGroundWaterLevelSource
        {
            get { return data.InitialGroundWaterLevelSource; }
            set { data.InitialGroundWaterLevelSource = value; }
        }

        [Description("Constant Initial Groundwater Level (m bel. surf.)")]
        public double? InitialGroundWaterLevelConstant
        {
            get { return GroundwaterIsConstant ? data.InitialGroundWaterLevelConstant : (double?) null; }
            set
            {
                if (value != null && GroundwaterIsConstant)
                {
                    data.InitialGroundWaterLevelConstant = value.Value;
                }
            }
        }

        private bool GroundwaterIsConstant
        {
            get { return data.InitialGroundWaterLevelSource == UnpavedEnums.GroundWaterSourceType.Constant; }
        }

        [Description("Maximum Land Storage (mm)")]
        public double MaximumLandStorage
        {
            get { return data.MaximumLandStorage; }
            set { data.MaximumLandStorage = value; }
        }

        [Description("Initial Land Storage (mm)")]
        public double InitialLandStorage
        {
            get { return data.InitialLandStorage; }
            set { data.InitialLandStorage = value; }
        }

        [Description("Infiltration Capacity (mm/h)")]
        public double InfiltrationCapacity
        {
            get { return data.InfiltrationCapacity; }
            set { data.InfiltrationCapacity = value; }
        }

        [Description("Seepage Type")]
        public UnpavedEnums.SeepageSourceType SeepageSource
        {
            get { return data.SeepageSource; }
            set { data.SeepageSource = value; }
        }

        [Description("Constant Seepage (mm/day)")]
        public double? SeepageConstant
        {
            get { return SeepageIsConstant ? data.SeepageConstant : (double?) null; }
            set
            {
                if (value != null && SeepageIsConstant)
                {
                    data.SeepageConstant = value.Value;
                }
            }
        }

        [Description("Meteo station")]
        public string MeteoStationName
        {
            get { return data.MeteoStationName; }
            set { data.MeteoStationName = value; }
        }

        [Description("Area adjustment factor")]
        public double AreaAdjustmentFactor
        {
            get { return data.AreaAdjustmentFactor; }
            set { data.AreaAdjustmentFactor = value; }
        }

        private bool SeepageIsConstant
        {
            get { return data.SeepageSource == UnpavedEnums.SeepageSourceType.Constant; }
        }

        public double SeepageH0HydraulicResistance
        {
            get { return data.SeepageH0HydraulicResistance; }
            set { data.SeepageH0HydraulicResistance = value; }
        }

        #region Drainage Formula

        [Description("Drainage Computation")]
        public UnpavedEnums.DrainageComputationOption DrainageFormula
        {
            get
            {
                if (data.DrainageFormula is ErnstDrainageFormula)
                {
                    return UnpavedEnums.DrainageComputationOption.Ernst;
                }
                if (data.DrainageFormula is KrayenhoffVanDeLeurDrainageFormula)
                {
                    return UnpavedEnums.DrainageComputationOption.KrayenhoffVdLeur;
                }
                return UnpavedEnums.DrainageComputationOption.DeZeeuwHellinga;
            }
            set { SwitchToDrainageFormula(value); }
        }


        [Description("De Zeeuw / Ernst - Surface Runoff (1/day or day)")]
        public double? ErnstZeeuwSurfaceValue
        {
            get { return ErnstZeeuw != null ? ErnstZeeuw.SurfaceRunoff : (double?) null; }
            set
            {
                if (ErnstZeeuw != null && value != null)
                {
                    ErnstZeeuw.SurfaceRunoff = value.Value;
                }
            }
        }

        [Description("De Zeeuw / Ernst - Level 1 (m AD)")]
        public double? ErnstZeeuwLevelOne
        {
            get { return ErnstZeeuw != null && ErnstZeeuw.LevelOneValue != 0.0 ? ErnstZeeuw.LevelOneTo : (double?) null; }
            set
            {
                if (ErnstZeeuw != null && value != null)
                {
                    ErnstZeeuw.LevelOneTo = value.Value;
                }
            }
        }

        [Description("De Zeeuw / Ernst - Level 1 Runoff (1/day or day)")]
        public double? ErnstZeeuwLevelOneValue
        {
            get { return ErnstZeeuw != null ? ErnstZeeuw.LevelOneValue : (double?) null; }
            set
            {
                if (ErnstZeeuw != null && value != null)
                {
                    ErnstZeeuw.LevelOneValue = value.Value;
                }
            }
        }

        [Description("De Zeeuw / Ernst - Level 2 (m AD)")]
        public double? ErnstZeeuwLevelTwo
        {
            get { return ErnstZeeuw != null && ErnstZeeuw.LevelTwoValue != 0.0 ? ErnstZeeuw.LevelTwoTo : (double?) null; }
            set
            {
                if (ErnstZeeuw != null && value != null)
                {
                    ErnstZeeuw.LevelTwoTo = value.Value;
                }
            }
        }

        [Description("De Zeeuw / Ernst - Level 2 Runoff (1/day or day)")]
        public double? ErnstZeeuwLevelTwoValue
        {
            get { return ErnstZeeuw != null ? ErnstZeeuw.LevelTwoValue : (double?) null; }
            set
            {
                if (ErnstZeeuw != null && value != null)
                {
                    ErnstZeeuw.LevelTwoValue = value.Value;
                }
            }
        }

        [Description("De Zeeuw / Ernst - Level 3 (m AD)")]
        public double? ErnstZeeuwLevelThree
        {
            get { return ErnstZeeuw != null && ErnstZeeuw.LevelThreeValue != 0.0 ? ErnstZeeuw.LevelThreeTo : (double?) null; }
            set
            {
                if (ErnstZeeuw != null && value != null)
                {
                    ErnstZeeuw.LevelThreeTo = value.Value;
                }
            }
        }

        [Description("De Zeeuw / Ernst - Level 3 Runoff (1/day or day)")]
        public double? ErnstZeeuwLevelThreeValue
        {
            get { return ErnstZeeuw != null ? ErnstZeeuw.LevelThreeValue : (double?) null; }
            set
            {
                if (ErnstZeeuw != null && value != null)
                {
                    ErnstZeeuw.LevelThreeValue = value.Value;
                }
            }
        }

        [Description("De Zeeuw / Ernst - Remaining Runoff (1/day or day)")]
        public double? ErnstZeeuwLevelInfiniteValue
        {
            get { return ErnstZeeuw != null ? ErnstZeeuw.InfiniteDrainageLevelRunoff : (double?) null; }
            set
            {
                if (ErnstZeeuw != null && value != null)
                {
                    ErnstZeeuw.InfiniteDrainageLevelRunoff = value.Value;
                }
            }
        }

        [Description("De Zeeuw / Ernst - Horizontal Inflow (1/day or day)")]
        public double? ErnstZeeuwHorizontalValue
        {
            get { return ErnstZeeuw != null ? ErnstZeeuw.HorizontalInflow : (double?) null; }
            set
            {
                if (ErnstZeeuw != null && value != null)
                {
                    ErnstZeeuw.HorizontalInflow = value.Value;
                }
            }
        }

        [Description("Krayenhoff vd Leur - Reservoir Coefficient (day)")]
        public double? KrayenhoffReservoirCoefficient
        {
            get { return Krayenhoff != null ? Krayenhoff.ResevoirCoefficient : (double?) null; }
            set
            {
                if (Krayenhoff != null && value != null)
                {
                    Krayenhoff.ResevoirCoefficient = value.Value;
                }
            }
        }

        private ErnstDeZeeuwHellingaDrainageFormulaBase ErnstZeeuw
        {
            get { return data.DrainageFormula as ErnstDeZeeuwHellingaDrainageFormulaBase; }
        }

        private KrayenhoffVanDeLeurDrainageFormula Krayenhoff
        {
            get { return data.DrainageFormula as KrayenhoffVanDeLeurDrainageFormula; }
        }

        private void SwitchToDrainageFormula(UnpavedEnums.DrainageComputationOption userInput)
        {
            switch (userInput)
            {
                case UnpavedEnums.DrainageComputationOption.DeZeeuwHellinga:
                    data.SwitchDrainageFormula<DeZeeuwHellingaDrainageFormula>();
                    break;
                case UnpavedEnums.DrainageComputationOption.Ernst:
                    data.SwitchDrainageFormula<ErnstDrainageFormula>();
                    break;
                case UnpavedEnums.DrainageComputationOption.KrayenhoffVdLeur:
                    data.SwitchDrainageFormula<KrayenhoffVanDeLeurDrainageFormula>();
                    break;
            }
        }

        #endregion
        
        private double GetAreaForCrop(UnpavedEnums.CropType cropType)
        {
            return data.AreaPerCrop[cropType];
        }

        private void SetAreaForCrop(UnpavedEnums.CropType cropType, double value)
        {
            data.AreaPerCrop[cropType] = value;
        }
    }
}