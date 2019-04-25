using System;
using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    public class WeirPropertiesRow : IDisposable, INotifyPropertyChange, IFeatureRowObject
    {
        private FormulaEnum formula;
        private static readonly GatedWeirFormula StGatedWeirFormula = new GatedWeirFormula();
        private static readonly PierWeirFormula StPierWeirFormula = new PierWeirFormula();
        private static readonly RiverWeirFormula StRiverWeirFormula = new RiverWeirFormula();
        private static readonly SimpleWeirFormula StSimpleWeirFormula = new SimpleWeirFormula();
        private static readonly FreeFormWeirFormula StFreeFormWeirFormula = new FreeFormWeirFormula();
        private static readonly GeneralStructureWeirFormula StGeneralStructureWeirFormula = new GeneralStructureWeirFormula();
        private IWeir weir;

        public WeirPropertiesRow(IWeir weir)
        {
            Weir = weir;
            SetFormula(ConvertFormula(Weir.WeirFormula), false);
        }

        private IWeir Weir
        {
            get { return weir; }
            set
            {
                if (weir != null)
                {
                    ((INotifyPropertyChanged)weir).PropertyChanged -= WeirPropertiesRowPropertyChanged;
                }
                weir = value;
                if (weir != null)
                {
                    ((INotifyPropertyChanged)weir).PropertyChanged += WeirPropertiesRowPropertyChanged;
                }
            }
        }

        // weir properties
        public string Name
        {
            get { return Weir.Name; }
            set { Weir.Name = value; }
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get { return Weir.LongName; }
            set { Weir.LongName = value; }
        }

        [ReadOnly(true)]
        public IBranch Branch
        {
            get { return Weir.Branch; }
        }

        [ReadOnly(true)]
        [DisplayName("Chainage [m]")]
        [DisplayFormat("0.00")]
        public double Chainage
        {
            get { return Weir.Chainage; }
        }

        public FormulaEnum Formula
        {
            get { return formula; }
            set
            {
                if (formula == value)
                {
                    return;
                }
                SetFormula(value, true);
            }
        }

        private IWeirFormula WeirFormula
        {
            get { return Weir.WeirFormula; }
        }

        protected string CrestWidthTimeSeriesString = "Time series";
        [DynamicReadOnly]
        [DisplayName(GuiParameterNames.CrestWidth)]
        public string CrestWidth
        {
            get
            {
                return weir.CrestWidth.ToString("0.00", CultureInfo.CurrentCulture);
            }
            set
            {
                weir.CrestWidth = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        protected string CrestLevelTimeSeriesString = "Time series";
        [DynamicReadOnly]
        [DisplayName(GuiParameterNames.CrestLevel)]
        public string CrestLevel
        {
            get
            {
                if (weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries)
                {
                    return CrestLevelTimeSeriesString;
                }
                return weir.CrestLevel.ToString("0.00", CultureInfo.CurrentCulture);
            }
            set
            {
                if (weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries)
                {
                    throw new InvalidOperationException("Cannot set value from row when using time dependent crest level.");
                }
                weir.CrestLevel = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        [DisplayName("Allow negative flow")]
        public bool AllowNegativeFlow
        {
            get { return Weir.AllowNegativeFlow; }
            set { Weir.AllowNegativeFlow = value; }
        }

        [DisplayName("Allow positive flow")]
        public bool AllowPositiveFlow
        {
            get { return Weir.AllowPositiveFlow; }
            set { Weir.AllowPositiveFlow = value; }
        }

        [DisplayName("Crest shape")]
        public CrestShape CrestShape
        {
            get { return Weir.CrestShape; }
            set { Weir.CrestShape = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Discharge coefficient (simple)")]
        public FlowDirection FlowDirection
        {
            get { return Weir.FlowDirection; }
            set { Weir.FlowDirection = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Discharge coefficient")]
        [DisplayFormat("0.00")]
        public double SDischargeCoefficient
        {
            get { return SimpleWeirFormula.DischargeCoefficient; }
            set { SimpleWeirFormula.DischargeCoefficient = value; }
        }
        
        [DynamicReadOnly]
        [DisplayName("Lateral contraction")]
        [DisplayFormat("0.00")]
        public double SLateralContraction
        {
            get { return SimpleWeirFormula.LateralContraction; }
            set { SimpleWeirFormula.LateralContraction = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Contraction coefficient")]
        [DisplayFormat("0.00")]
        public double GContractionCoefficient
        {
            get { return GatedWeirFormula.ContractionCoefficient; }
            set { GatedWeirFormula.ContractionCoefficient = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Lateral contraction")]
        [DisplayFormat("0.00")]
        public double GLateralContraction
        {
            get { return GatedWeirFormula.LateralContraction; }
            set { GatedWeirFormula.LateralContraction = value; }
        }

        protected string OpeningLevelTimeSeriesString = "Time series";
        [DynamicReadOnly]
        [DisplayName("Lower edge level")]
        public string GLowerEdge
        {
            get
            {
                if (Formula == FormulaEnum.GatedWeir)
                {
                    if (GatedWeirFormula.CanBeTimedependent && GatedWeirFormula.UseLowerEdgeLevelTimeSeries)
                    {
                        return OpeningLevelTimeSeriesString;
                    }
                    return (GatedWeirFormula.GateOpening + Weir.CrestLevel).ToString(CultureInfo.CurrentCulture);
                }
                if (Formula == FormulaEnum.GeneralStructure)
                {
                    return (GeneralStructureWeirFormula.GateOpening + Weir.CrestLevel).ToString(CultureInfo.CurrentCulture);
                }
                return "0";
            }
            set
            {
                var newValue = double.Parse(value, CultureInfo.CurrentCulture);
                if (newValue > Weir.CrestLevel)
                {
                    if (Formula == FormulaEnum.GatedWeir)
                    {
                        if (GatedWeirFormula.CanBeTimedependent && GatedWeirFormula.UseLowerEdgeLevelTimeSeries)
                        {
                            throw new InvalidOperationException("Cannot set value from row when using time dependent lower edge level.");
                        }
                        GatedWeirFormula.GateOpening = newValue - Weir.CrestLevel;
                    }
                    if (Formula == FormulaEnum.GeneralStructure)
                    {
                        GeneralStructureWeirFormula.GateOpening = newValue - Weir.CrestLevel;
                    }
                }
            }
        }

        [DynamicReadOnly]
        [DisplayName("Gate opening")]
        public string GGateOpening
        {
            get
            {
                if (Formula == FormulaEnum.GatedWeir)
                {
                    if (GatedWeirFormula.CanBeTimedependent && GatedWeirFormula.UseLowerEdgeLevelTimeSeries)
                    {
                        return OpeningLevelTimeSeriesString;
                    }
                    return GatedWeirFormula.GateOpening.ToString("0.00", CultureInfo.CurrentCulture);
                }
                if (Formula == FormulaEnum.GeneralStructure)
                {
                    return GeneralStructureWeirFormula.GateOpening.ToString("0.00", CultureInfo.CurrentCulture);
                }
                return "0";
            }
            set
            {
                if (Formula == FormulaEnum.GatedWeir)
                {
                    if (GatedWeirFormula.CanBeTimedependent && GatedWeirFormula.UseLowerEdgeLevelTimeSeries)
                    {
                        throw new InvalidOperationException("Cannot set value from row when using time dependent gate opening.");
                    }
                    GatedWeirFormula.GateOpening = double.Parse(value, CultureInfo.CurrentCulture);
                }
                if (Formula == FormulaEnum.GeneralStructure)
                {
                    GeneralStructureWeirFormula.GateOpening = double.Parse(value, CultureInfo.CurrentCulture);
                }
            }
        }

        [DynamicReadOnly]
        [DisplayName("Maximum positive flow")]
        [DisplayFormat("0.00")]
        public double GMaxFlowPos
        {
            get { return GatedWeirFormula.MaxFlowPos; }
            set { GatedWeirFormula.MaxFlowPos = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Maximum negative flow")]
        [DisplayFormat("0.00")]
        public double GMaxFlowNeg
        {
            get { return GatedWeirFormula.MaxFlowNeg; }
            set { GatedWeirFormula.MaxFlowNeg = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Use maximum positive flow")]
        public bool GUseMaxFlowPos
        {
            get { return GatedWeirFormula.UseMaxFlowPos; }
            set { GatedWeirFormula.UseMaxFlowPos = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Use maximum negative flow")]
        public bool GUseMaxFlowNeg
        {
            get { return GatedWeirFormula.UseMaxFlowNeg; }
            set { GatedWeirFormula.UseMaxFlowNeg = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Number of piers")]
        public int PNumberOfPiers
        {
            get { return PierWeirFormula.NumberOfPiers; }
            set { PierWeirFormula.NumberOfPiers = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Upstream face flow")]
        [DisplayFormat("0.00")]
        public double PUpstreamFacePos
        {
            get { return PierWeirFormula.UpstreamFacePos; }
            set { PierWeirFormula.UpstreamFacePos = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Upstream face reverse")]
        [DisplayFormat("0.00")]
        public double PUpstreamFaceNeg
        {
            get { return PierWeirFormula.UpstreamFaceNeg; }
            set { PierWeirFormula.UpstreamFaceNeg = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Design head flow")]
        [DisplayFormat("0.00")]
        public double PDesignHeadPos
        {
            get { return PierWeirFormula.DesignHeadPos; }
            set { PierWeirFormula.DesignHeadPos = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Design head reverse")]
        [DisplayFormat("0.00")]
        public double PDesignHeadNeg
        {
            get { return PierWeirFormula.DesignHeadNeg; }
            set { PierWeirFormula.DesignHeadNeg = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Pier contraction flow")]
        [DisplayFormat("0.00")]
        public double PPierContractionPos
        {
            get { return PierWeirFormula.PierContractionPos; }
            set { PierWeirFormula.PierContractionPos = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Pier contraction reverse")]
        [DisplayFormat("0.00")]
        public double PPierContractionNeg
        {
            get { return PierWeirFormula.PierContractionNeg; }
            set { PierWeirFormula.PierContractionNeg = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Abutment contraction flow")]
        [DisplayFormat("0.00")]
        public double PAbutmentContractionPos
        {
            get { return PierWeirFormula.AbutmentContractionPos; }
            set { PierWeirFormula.AbutmentContractionPos = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Abutment Contraction Reverse")]
        [DisplayFormat("0.00")]
        public double PAbutmentContractionNeg
        {
            get { return PierWeirFormula.AbutmentContractionNeg; }
            set { PierWeirFormula.AbutmentContractionNeg = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Flow correction")]
        [DisplayFormat("0.00")]
        public double RCorrectionCoefficientPos
        {
            get { return RiverWeirFormula.CorrectionCoefficientPos; }
            set { RiverWeirFormula.CorrectionCoefficientPos = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Reverse correction")]
        [DisplayFormat("0.00")]
        public double RCorrectionCoefficientNeg
        {
            get { return RiverWeirFormula.CorrectionCoefficientNeg; }
            set { RiverWeirFormula.CorrectionCoefficientNeg = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Flow submerge")]
        [DisplayFormat("0.00")]
        public double RSubmergeLimitPos
        {
            get { return RiverWeirFormula.SubmergeLimitPos; }
            set { RiverWeirFormula.SubmergeLimitPos = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Reverse submerge")]
        [DisplayFormat("0.00")]
        public double RSubmergeLimitNeg
        {
            get { return RiverWeirFormula.SubmergeLimitNeg; }
            set { RiverWeirFormula.SubmergeLimitNeg = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Discharge coefficient")]
        [DisplayFormat("0.00")]
        public double FDischargeCoefficient
        {
            get { return FreeFormWeirFormula.DischargeCoefficient; }
            set { FreeFormWeirFormula.DischargeCoefficient = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Positive gate flow")]
        [DisplayFormat("0.00")]
        public double APositiveFreeGateFlow
        {
            get { return GeneralStructureWeirFormula.PositiveFreeGateFlow; }
            set { GeneralStructureWeirFormula.PositiveFreeGateFlow = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Negative gate flow")]
        [DisplayFormat("0.00")]
        public double ANegativeFreeGateFlow
        {
            get { return GeneralStructureWeirFormula.NegativeFreeGateFlow; }
            set { GeneralStructureWeirFormula.NegativeFreeGateFlow = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Positive drowned gate flow")]
        [DisplayFormat("0.00")]
        public double APositiveDrownedGateFlow
        {
            get { return GeneralStructureWeirFormula.PositiveDrownedGateFlow; }
            set { GeneralStructureWeirFormula.PositiveDrownedGateFlow = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Negative drowned gate flow")]
        [DisplayFormat("0.00")]
        public double ANegativeDrownedGateFlow
        {
            get { return GeneralStructureWeirFormula.NegativeDrownedGateFlow; }
            set { GeneralStructureWeirFormula.NegativeDrownedGateFlow = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Positive weir flow")]
        [DisplayFormat("0.00")]
        public double APositiveFreeWeirFlow
        {
            get { return GeneralStructureWeirFormula.PositiveFreeWeirFlow; }
            set { GeneralStructureWeirFormula.PositiveFreeWeirFlow = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Negative weir flow")]
        [DisplayFormat("0.00")]
        public double ANegativeFreeWeirFlow
        {
            get { return GeneralStructureWeirFormula.NegativeFreeWeirFlow; }
            set { GeneralStructureWeirFormula.NegativeFreeWeirFlow = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Positive drowned weir flow")]
        [DisplayFormat("0.00")]
        public double APositiveDrownedWeirFlow
        {
            get { return GeneralStructureWeirFormula.PositiveDrownedWeirFlow; }
            set { GeneralStructureWeirFormula.PositiveDrownedWeirFlow = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Negative drowned weir flow")]
        [DisplayFormat("0.00")]
        public double ANegativeDrownedWeirFlow
        {
            get { return GeneralStructureWeirFormula.NegativeDrownedWeirFlow; }
            set { GeneralStructureWeirFormula.NegativeDrownedWeirFlow = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Positive contraction coefficient")]
        [DisplayFormat("0.00")]
        public double APositiveContractionCoefficient
        {
            get { return GeneralStructureWeirFormula.PositiveContractionCoefficient; }
            set { GeneralStructureWeirFormula.PositiveContractionCoefficient = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Negative contraction coefficient")]
        [DisplayFormat("0.00")]
        public double ANegativeContractionCoefficient
        {
            get { return GeneralStructureWeirFormula.NegativeContractionCoefficient; }
            set { GeneralStructureWeirFormula.NegativeContractionCoefficient = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Level upstream 1")]
        [DisplayFormat("0.00")]
        public double ABedLevelLeftSideOfStructure
        {
            get { return GeneralStructureWeirFormula.BedLevelLeftSideOfStructure; }
            set { GeneralStructureWeirFormula.BedLevelLeftSideOfStructure = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Level upstream 2")]
        [DisplayFormat("0.00")]
        public double ABedLevelLeftSideStructure
        {
            get { return GeneralStructureWeirFormula.BedLevelLeftSideStructure; }
            set { GeneralStructureWeirFormula.BedLevelLeftSideStructure = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Level crest")]
        [DisplayFormat("0.00")]
        public double ABedLevelStructureCentre
        {
            get { return GeneralStructureWeirFormula.BedLevelStructureCentre; }
            set { GeneralStructureWeirFormula.BedLevelStructureCentre = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Level downstream 1")]
        [DisplayFormat("0.00")]
        public double ABedLevelRightSideStructure
        {
            get { return GeneralStructureWeirFormula.BedLevelRightSideStructure; }
            set { GeneralStructureWeirFormula.BedLevelRightSideStructure = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Level downstream 2")]
        [DisplayFormat("0.00")]
        public double ABedLevelRightSideOfStructure
        {
            get { return GeneralStructureWeirFormula.BedLevelRightSideOfStructure; }
            set { GeneralStructureWeirFormula.BedLevelRightSideOfStructure = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Width upstream 1")]
        [DisplayFormat("0.00")]
        public double AWidthLeftSideOfStructure
        {
            get { return GeneralStructureWeirFormula.WidthLeftSideOfStructure; }
            set { GeneralStructureWeirFormula.WidthLeftSideOfStructure = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Width upstream 2")]
        [DisplayFormat("0.00")]
        public double AWidthStructureLeftSide
        {
            get { return GeneralStructureWeirFormula.WidthStructureLeftSide; }
            set { GeneralStructureWeirFormula.WidthStructureLeftSide = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Width crest")]
        [DisplayFormat("0.00")]
        public double AWidthStructureCentre
        {
            get { return GeneralStructureWeirFormula.WidthStructureCentre; }
            set { GeneralStructureWeirFormula.WidthStructureCentre = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Width downstream 1")]
        [DisplayFormat("0.00")]
        public double AWidthStructureRightSide
        {
            get { return GeneralStructureWeirFormula.WidthStructureRightSide; }
            set { GeneralStructureWeirFormula.WidthStructureRightSide = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Width downstream 2")]
        [DisplayFormat("0.00")]
        public double AWidthRightSideOfStructure
        {
            get { return GeneralStructureWeirFormula.WidthRightSideOfStructure; }
            set { GeneralStructureWeirFormula.WidthRightSideOfStructure = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Extra resistance enabled")]
        private bool AUseExtraResistance
        {
            get { return GeneralStructureWeirFormula.UseExtraResistance; }
            set { GeneralStructureWeirFormula.UseExtraResistance = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Extra resistance")]
        [DisplayFormat("0.00")]
        public double AExtraResistance
        {
            get { return GeneralStructureWeirFormula.ExtraResistance; }
            set { GeneralStructureWeirFormula.ExtraResistance = value; }
        }
        
        [Browsable(false)]
        public bool HasParent { get; set; }

        public IFeature GetFeature()
        {
            return weir;
        }

        public void Dispose()
        {
            Weir = null;
            PropertyChanged = null;
            PropertyChanging = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;


        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (weir == null) return false;
            if (propertyName == "CrestLevel")
            {
                return weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries;
            }
            if (propertyName == "FlowDirection" || propertyName == "SDischargeCoefficient" || propertyName == "SLateralContraction")
            {
                return Formula != FormulaEnum.SimpleWeir;
            }

            if (propertyName == "GLowerEdge" || propertyName == "GGateOpening")
            {
                if (Formula == FormulaEnum.GatedWeir)
                {
                    return GatedWeirFormula.CanBeTimedependent && GatedWeirFormula.UseLowerEdgeLevelTimeSeries;
                }
                return Formula != FormulaEnum.GeneralStructure;
            }

            if (propertyName == "GContractionCoefficient" || propertyName == "GLateralContraction" || propertyName == "GMaxFlowPos" || 
                propertyName == "GMaxFlowNeg" || propertyName == "GUseMaxFlowPos" || propertyName == "GUseMaxFlowNeg")
            {
                return Formula != FormulaEnum.GatedWeir;
            }

            if (propertyName == "PNumberOfPiers" || propertyName == "PUpstreamFacePos" || propertyName == "PUpstreamFaceNeg" ||
                propertyName == "PDesignHeadPos" || propertyName == "PDesignHeadNeg" || propertyName == "PPierContractionPos" ||
                propertyName == "PPierContractionNeg" || propertyName == "PAbutmentContractionPos" || propertyName == "PAbutmentContractionNeg")
            {
                return Formula != FormulaEnum.PierWeir;
            }

            if (propertyName == "RCorrectionCoefficientPos" || propertyName == "RCorrectionCoefficientNeg" || 
                propertyName == "RSubmergeLimitPos" || propertyName == "RSubmergeLimitNeg" )
            {
                return Formula != FormulaEnum.RiverWeir;
            }

            if (propertyName == "FDischargeCoefficient")
            {
                return Formula != FormulaEnum.FreeFormWeir;
            }
            if (propertyName == "APositiveFreeGateFlow" || propertyName == "ANegativeFreeGateFlow" || propertyName == "APositiveDrownedGateFlow" ||
                propertyName == "ANegativeDrownedGateFlow" || propertyName == "APositiveFreeWeirFlow" || propertyName == "ANegativeFreeWeirFlow" ||
                propertyName == "APositiveDrownedWeirFlow" || propertyName == "ANegativeDrownedWeirFlow" || propertyName == "APositiveContractionCoefficient" ||
                propertyName == "ANegativeContractionCoefficient" || propertyName == "ABedLevelLeftSideOfStructure" || propertyName == "ABedLevelLeftSideStructure" ||
                propertyName == "ABedLevelStructureCentre" || propertyName == "ABedLevelRightSideStructure" || propertyName == "ABedLevelRightSideOfStructure" ||
                propertyName == "AWidthLeftSideOfStructure" || propertyName == "AWidthStructureLeftSide" || propertyName == "AWidthStructureCentre" ||
                propertyName == "AWidthStructureRightSide" || propertyName == "AWidthRightSideOfStructure" || propertyName == "AUseExtraResistance" ||
                propertyName == "AExtraResistance")
            {
                return Formula != FormulaEnum.GeneralStructure;
            }

            return false;
        }

        private GatedWeirFormula GatedWeirFormula { get; set; }
        private PierWeirFormula PierWeirFormula { get; set; }
        private RiverWeirFormula RiverWeirFormula { get; set; }
        private SimpleWeirFormula SimpleWeirFormula { get; set; }
        private FreeFormWeirFormula FreeFormWeirFormula { get; set; }
        private GeneralStructureWeirFormula GeneralStructureWeirFormula { get; set; }

        private void SetFormula(FormulaEnum newFormula, bool setToWeir)
        {
            GatedWeirFormula = StGatedWeirFormula;
            PierWeirFormula = StPierWeirFormula;
            RiverWeirFormula = StRiverWeirFormula;
            SimpleWeirFormula = StSimpleWeirFormula;
            FreeFormWeirFormula = StFreeFormWeirFormula;
            GeneralStructureWeirFormula = StGeneralStructureWeirFormula;
            switch (newFormula)
            {
                case FormulaEnum.GatedWeir:
                    if (setToWeir)
                    {
                        Weir.WeirFormula = new GatedWeirFormula();
                    }
                    GatedWeirFormula = (GatedWeirFormula)Weir.WeirFormula;
                    break;
                case FormulaEnum.PierWeir:
                    if (setToWeir)
                    {
                        Weir.WeirFormula = new PierWeirFormula();
                    }
                    PierWeirFormula = (PierWeirFormula)Weir.WeirFormula;
                    break;
                case FormulaEnum.RiverWeir:
                    if (setToWeir)
                    {
                        Weir.WeirFormula = new RiverWeirFormula();
                    }
                    RiverWeirFormula = (RiverWeirFormula)Weir.WeirFormula;
                    break;
                case FormulaEnum.SimpleWeir:
                    if (setToWeir)
                    {
                        Weir.WeirFormula = new SimpleWeirFormula();
                    }
                    SimpleWeirFormula = (SimpleWeirFormula)Weir.WeirFormula;
                    break;
                case FormulaEnum.FreeFormWeir:
                    if (setToWeir)
                    {
                        Weir.WeirFormula = new FreeFormWeirFormula();
                    }
                    FreeFormWeirFormula = (FreeFormWeirFormula)Weir.WeirFormula;
                    break;
                case FormulaEnum.GeneralStructure:
                    if (setToWeir)
                    {
                        Weir.WeirFormula = new GeneralStructureWeirFormula();
                    }
                    GeneralStructureWeirFormula = (GeneralStructureWeirFormula)Weir.WeirFormula;
                    break;
            }
            formula = newFormula;
        }

        private void WeirPropertiesRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WeirFormula")
            {
                SetFormula(ConvertFormula(Weir.WeirFormula), false);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        private static FormulaEnum ConvertFormula(IWeirFormula weirFormula)
        {
            var type = weirFormula.GetEntityType();

            if (type == typeof(GatedWeirFormula))
            {
                return FormulaEnum.GatedWeir;
            }
            if (type == typeof(PierWeirFormula))
            {
                return FormulaEnum.PierWeir;
            }
            if (type == typeof(RiverWeirFormula))
            {
                return FormulaEnum.RiverWeir;
            }
            if (type == typeof(SimpleWeirFormula))
            {
                return FormulaEnum.SimpleWeir;
            }
            if (type == typeof(FreeFormWeirFormula))
            {
                return FormulaEnum.FreeFormWeir;
            }
            if (type == typeof(GeneralStructureWeirFormula))
            {
                return FormulaEnum.GeneralStructure;
            }
            throw new NotImplementedException("Should not get here");
        }

        public enum FormulaEnum
        {
            [Description("Gated weir")]
            GatedWeir,
            [Description("Pier weir")]
            PierWeir,
            [Description("River weir")]
            RiverWeir,
            [Description("Simple weir")]
            SimpleWeir,
            [Description("Free form weir")]
            FreeFormWeir,
            [Description("General structure")]
            GeneralStructure
        }
    }
}