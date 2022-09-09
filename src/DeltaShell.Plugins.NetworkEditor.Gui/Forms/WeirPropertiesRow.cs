using System;
using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
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
        private FormulaEnum? formula;
        private static readonly GatedWeirFormula StGatedWeirFormula = new GatedWeirFormula(true);
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
        [DisplayName("Chainage")]
        [DisplayFormat("0.00")]
        public double Chainage
        {
            get { return Weir.Chainage; }
        }
        
        
        public FormulaEnum Formula
        {
            get { return formula ?? FormulaEnum.SimpleWeir; }
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
        
        [DynamicReadOnly]
        [DisplayName("Discharge coefficient")]
        public string DischargeCoefficient
        {
            get
            {
                return FreeFormWeirFormula.DischargeCoefficient.ToString("0.00", CultureInfo.CurrentCulture);
            }
            set
            {
                FreeFormWeirFormula.DischargeCoefficient = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }
        
        protected string CrestWidthTimeSeriesString = "Time series";
        [DynamicReadOnly]
        [DisplayName("Crest width")]
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
        [DisplayName("Crest level")]
        public string CrestLevel
        {
            get
            {
                if (weir.IsUsingTimeSeriesForCrestLevel())
                {
                    return CrestLevelTimeSeriesString;
                }
                return weir.CrestLevel.ToString("0.00", CultureInfo.CurrentCulture);
            }
            set
            {
                if (weir.IsUsingTimeSeriesForCrestLevel())
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
        [Browsable(false)]
        public CrestShape CrestShape
        {
            get { return Weir.CrestShape; }
            set { Weir.CrestShape = value; }
        }

        [DisplayName("Flow direction")]
        public FlowDirection FlowDirection
        {
            get { return Weir.FlowDirection; }
            set { Weir.FlowDirection = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Correction coefficient")]
        [DisplayFormat("0.00")]
        public double SDischargeCoefficient
        {
            get { return SimpleWeirFormula.CorrectionCoefficient; }
            set { SimpleWeirFormula.CorrectionCoefficient = value; }
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
                if (Weir is IOrifice)
                {
                    if (GatedWeirFormula.CanBeTimedependent && GatedWeirFormula.UseLowerEdgeLevelTimeSeries)
                    {
                        return OpeningLevelTimeSeriesString;
                    }
                    return (GatedWeirFormula.GateOpening + Weir.CrestLevel).ToString(CultureInfo.CurrentCulture);
                }
                if (Formula == FormulaEnum.GeneralStructure)
                {
                    return (GeneralStructureWeirFormula.LowerEdgeLevel).ToString(CultureInfo.CurrentCulture);
                }
                return "0";
            }
            set
            {
                var newValue = double.Parse(value, CultureInfo.CurrentCulture);
                if (newValue > Weir.CrestLevel)
                {
                    if (Weir is IOrifice)
                    {
                        if (GatedWeirFormula.CanBeTimedependent && GatedWeirFormula.UseLowerEdgeLevelTimeSeries)
                        {
                            throw new InvalidOperationException("Cannot set value from row when using time dependent lower edge level.");
                        }
                        GatedWeirFormula.GateOpening = newValue - Weir.CrestLevel;
                    }
                    if (Formula == FormulaEnum.GeneralStructure)
                    {
                        GeneralStructureWeirFormula.LowerEdgeLevel = newValue;
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
                if (Weir is IOrifice)
                {
                    if (GatedWeirFormula.CanBeTimedependent && GatedWeirFormula.UseLowerEdgeLevelTimeSeries)
                    {
                        return OpeningLevelTimeSeriesString;
                    }
                    return GatedWeirFormula.GateOpening.ToString("0.00", CultureInfo.CurrentCulture);
                }
                if (Formula == FormulaEnum.GeneralStructure)
                {
                    return (GeneralStructureWeirFormula.LowerEdgeLevel - weir.CrestLevel).ToString("0.00", CultureInfo.CurrentCulture);
                }
                return "0";
            }
            set
            {
                if (Weir is IOrifice)
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
        [DisplayName("Gate height")]
        public string GGateHeight
        {
            get
            {
                if (Formula == FormulaEnum.GeneralStructure)
                {
                    return GeneralStructureWeirFormula.GateHeight.ToString("0.00", CultureInfo.CurrentCulture);
                }
                return "0";
            }
            set
            {
                if (Formula == FormulaEnum.GeneralStructure)
                {
                    GeneralStructureWeirFormula.GateHeight = double.Parse(value, CultureInfo.CurrentCulture);
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
        [DisplayName("Use velocity height")]
        public bool UseVelocityHeight
        {
            get { return Weir.UseVelocityHeight; }
            set { Weir.UseVelocityHeight = value; }
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
            if (propertyName == nameof(CrestLevel))
            {
                return weir.IsUsingTimeSeriesForCrestLevel();
            }
            if (propertyName == nameof(CrestWidth))
            {
                return weir.IsUsingTimeSeriesForCrestLevel() || weir.GetStructureType() == StructureType.UniversalWeir;
            }
            if (propertyName == nameof(FlowDirection) || propertyName == nameof(SDischargeCoefficient) /*|| propertyName == nameof(SLateralContraction)*/)
            {
                return Formula != FormulaEnum.SimpleWeir;
            }
            if (propertyName == nameof(DischargeCoefficient))
            {
                return Formula != FormulaEnum.FreeFormWeir;
            }

            if (propertyName == nameof(GGateOpening))
            {
                return true;
            }
            if (propertyName == nameof(GLowerEdge) || propertyName == nameof(GGateHeight))
            {
                if (Weir is IOrifice)
                {
                    return GatedWeirFormula.CanBeTimedependent && GatedWeirFormula.UseLowerEdgeLevelTimeSeries;
                }
                return Formula != FormulaEnum.GeneralStructure;
            }

            if (propertyName == nameof(UseVelocityHeight))
            {
                return !(Weir is IOrifice) && Formula == FormulaEnum.FreeFormWeir;
            }

            if (propertyName == nameof(GContractionCoefficient) || propertyName == nameof(GLateralContraction) || propertyName == nameof(GMaxFlowPos) || 
                propertyName == nameof(GMaxFlowNeg) || propertyName == nameof(GUseMaxFlowPos) || propertyName == nameof(GUseMaxFlowNeg))
            {
                return true;
            }

            return false;
        }

        private GatedWeirFormula GatedWeirFormula { get; set; }
        private PierWeirFormula PierWeirFormula { get; set; }
        private RiverWeirFormula RiverWeirFormula { get; set; }
        private SimpleWeirFormula SimpleWeirFormula { get; set; }
        private FreeFormWeirFormula FreeFormWeirFormula { get; set; }
        private GeneralStructureWeirFormula GeneralStructureWeirFormula { get; set; }

        private void SetFormula(FormulaEnum? newFormula, bool setToWeir)
        {
            GatedWeirFormula = StGatedWeirFormula;
            PierWeirFormula = StPierWeirFormula;
            RiverWeirFormula = StRiverWeirFormula;
            SimpleWeirFormula = StSimpleWeirFormula;
            FreeFormWeirFormula = StFreeFormWeirFormula;
            GeneralStructureWeirFormula = StGeneralStructureWeirFormula;
            switch (newFormula)
            {
                case FormulaEnum.SimpleWeir:
                    if (Weir is IOrifice) break;
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

            if (Weir is IOrifice)
            {
                if (setToWeir)
                {
                    Weir.WeirFormula = new GatedWeirFormula(true);
                }
                GatedWeirFormula = (GatedWeirFormula)Weir.WeirFormula;
            }
            formula = newFormula;
        }

        private void WeirPropertiesRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WeirFormula))
            {
                SetFormula(ConvertFormula(Weir.WeirFormula), false);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        private static FormulaEnum? ConvertFormula(IWeirFormula weirFormula)
        {
            var type = weirFormula.GetEntityType();

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

            if (weirFormula is GatedWeirFormula)
            {
                return null; //because we don't care for orifices
            }
            throw new NotImplementedException("Should not get here");
        }

        public enum FormulaEnum
        {
            [Description("Simple weir")]
            SimpleWeir,
            [Description("Free form weir")]
            FreeFormWeir,
            [Description("General structure")]
            GeneralStructure
        }
    }
}