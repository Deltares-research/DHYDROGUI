using System;
using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    public class WeirRow : IDisposable, INotifyPropertyChange, IFeatureRowObject
    {
        private readonly NameValidator nameValidator;
        
        public enum FormulaEnum
        {
            [Description("Simple weir")]
            SimpleWeir,

            [Description("Free form weir")]
            FreeFormWeir,

            [Description("General structure")]
            GeneralStructure
        }

        private const string timeSeriesString = "Time series";

        private static readonly GatedWeirFormula defaultGatedWeirFormula = new GatedWeirFormula(true);
        private static readonly SimpleWeirFormula defaultSimpleWeirFormula = new SimpleWeirFormula();
        private static readonly FreeFormWeirFormula defaultFreeFormWeirFormula = new FreeFormWeirFormula();
        private static readonly GeneralStructureWeirFormula defaultGeneralStructureWeirFormula = new GeneralStructureWeirFormula();
        private FormulaEnum? formula;

        private IWeir weir;

        /// <summary>
        /// Initialize a new instance of the <see cref="WeirRow"/> class.
        /// </summary>
        /// <param name="weir"> The weir to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="weir"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public WeirRow(IWeir weir, NameValidator nameValidator)
        {
            Ensure.NotNull(weir, nameof(weir));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            Weir = weir;
            this.nameValidator = nameValidator;

            SetFormula(ConvertFormula(Weir.WeirFormula), false);
        }

        private GatedWeirFormula GatedWeirFormula { get; set; }
        private SimpleWeirFormula SimpleWeirFormula { get; set; }
        private FreeFormWeirFormula FreeFormWeirFormula { get; set; }
        private GeneralStructureWeirFormula GeneralStructureWeirFormula { get; set; }

        private IWeir Weir
        {
            get => weir;
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

        public string Name
        {
            get => Weir.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    weir.Name = value;
                }
            }
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => Weir.LongName;
            set => Weir.LongName = value;
        }

        [ReadOnly(true)]
        public IBranch Branch => Weir.Branch;

        [ReadOnly(true)]
        [DisplayName("Chainage")]
        [DisplayFormat("0.00")]
        public double Chainage => Weir.Chainage;

        public FormulaEnum Formula
        {
            get => formula ?? FormulaEnum.SimpleWeir;
            set
            {
                if (formula == value)
                {
                    return;
                }

                SetFormula(value, true);
            }
        }

        private IWeirFormula WeirFormula => Weir.WeirFormula;

        [DynamicReadOnly]
        [DisplayName("Discharge coefficient")]
        public string DischargeCoefficient
        {
            get => FreeFormWeirFormula.DischargeCoefficient.ToString("0.00", CultureInfo.CurrentCulture);
            set => FreeFormWeirFormula.DischargeCoefficient = double.Parse(value, CultureInfo.CurrentCulture);
        }

        [DynamicReadOnly]
        [DisplayName("Crest width")]
        public string CrestWidth
        {
            get => weir.CrestWidth.ToString("0.00", CultureInfo.CurrentCulture);
            set => weir.CrestWidth = double.Parse(value, CultureInfo.CurrentCulture);
        }

        [DynamicReadOnly]
        [DisplayName("Crest level")]
        public string CrestLevel
        {
            get
            {
                if (weir.IsUsingTimeSeriesForCrestLevel())
                {
                    return timeSeriesString;
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
            get => Weir.AllowNegativeFlow;
            set => Weir.AllowNegativeFlow = value;
        }

        [DisplayName("Allow positive flow")]
        public bool AllowPositiveFlow
        {
            get => Weir.AllowPositiveFlow;
            set => Weir.AllowPositiveFlow = value;
        }

        [DisplayName("Crest shape")]
        [Browsable(false)]
        public CrestShape CrestShape
        {
            get => Weir.CrestShape;
            set => Weir.CrestShape = value;
        }

        [DisplayName("Flow direction")]
        public FlowDirection FlowDirection
        {
            get => Weir.FlowDirection;
            set => Weir.FlowDirection = value;
        }

        [DynamicReadOnly]
        [DisplayName("Correction coefficient")]
        [DisplayFormat("0.00")]
        public double SDischargeCoefficient
        {
            get => SimpleWeirFormula.CorrectionCoefficient;
            set => SimpleWeirFormula.CorrectionCoefficient = value;
        }

        [DynamicReadOnly]
        [DisplayName("Contraction coefficient")]
        [DisplayFormat("0.00")]
        public double GContractionCoefficient
        {
            get => GatedWeirFormula.ContractionCoefficient;
            set => GatedWeirFormula.ContractionCoefficient = value;
        }

        [DynamicReadOnly]
        [DisplayName("Lateral contraction")]
        [DisplayFormat("0.00")]
        public double GLateralContraction
        {
            get => GatedWeirFormula.LateralContraction;
            set => GatedWeirFormula.LateralContraction = value;
        }

        [DynamicReadOnly]
        [DisplayName("Lower edge level")]
        public string GLowerEdge
        {
            get
            {
                if (Weir is IOrifice)
                {
                    if (GatedWeirFormula.IsUsingTimeSeriesForLowerEdgeLevel())
                    {
                        return timeSeriesString;
                    }

                    return GatedWeirFormula.LowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);
                }

                if (Formula == FormulaEnum.GeneralStructure)
                {
                    return GeneralStructureWeirFormula.LowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);
                }

                return "0";
            }
            set
            {
                double newValue = double.Parse(value, CultureInfo.CurrentCulture);
                if (newValue > Weir.CrestLevel)
                {
                    if (Weir is IOrifice)
                    {
                        if (GatedWeirFormula.IsUsingTimeSeriesForLowerEdgeLevel())
                        {
                            throw new InvalidOperationException("Cannot set value from row when using time dependent lower edge level.");
                        }

                        GatedWeirFormula.LowerEdgeLevel = newValue;
                        GatedWeirFormula.GateOpening = newValue - Weir.CrestLevel;
                    }

                    if (Formula == FormulaEnum.GeneralStructure)
                    {
                        GeneralStructureWeirFormula.LowerEdgeLevel = newValue;
                    }
                }
            }
        }

        [ReadOnly(true)]
        [DisplayName("Gate opening")]
        public string GGateOpening
        {
            get
            {
                if (Weir is IOrifice)
                {
                    if (GatedWeirFormula.IsUsingTimeSeriesForLowerEdgeLevel())
                    {
                        return timeSeriesString;
                    }

                    return (GatedWeirFormula.LowerEdgeLevel - weir.CrestLevel).ToString("0.00", CultureInfo.CurrentCulture);
                }

                if (Formula == FormulaEnum.GeneralStructure)
                {
                    return (GeneralStructureWeirFormula.LowerEdgeLevel - weir.CrestLevel).ToString("0.00", CultureInfo.CurrentCulture);
                }

                return "0";
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
            get => GatedWeirFormula.MaxFlowPos;
            set => GatedWeirFormula.MaxFlowPos = value;
        }

        [DynamicReadOnly]
        [DisplayName("Maximum negative flow")]
        [DisplayFormat("0.00")]
        public double GMaxFlowNeg
        {
            get => GatedWeirFormula.MaxFlowNeg;
            set => GatedWeirFormula.MaxFlowNeg = value;
        }

        [DynamicReadOnly]
        [DisplayName("Use maximum positive flow")]
        public bool GUseMaxFlowPos
        {
            get => GatedWeirFormula.UseMaxFlowPos;
            set => GatedWeirFormula.UseMaxFlowPos = value;
        }

        [DynamicReadOnly]
        [DisplayName("Use maximum negative flow")]
        public bool GUseMaxFlowNeg
        {
            get => GatedWeirFormula.UseMaxFlowNeg;
            set => GatedWeirFormula.UseMaxFlowNeg = value;
        }

        [DynamicReadOnly]
        [DisplayName("Use velocity height")]
        public bool UseVelocityHeight
        {
            get => Weir.UseVelocityHeight;
            set => Weir.UseVelocityHeight = value;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Weir = null;
            PropertyChanged = null;
            PropertyChanging = null;
        }

        /// <inheritdoc/>
        public IFeature GetFeature()
        {
            return weir;
        }

        /// <inheritdoc/>
        [Browsable(false)]
        public bool HasParent { get; set; }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler PropertyChanging;

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (weir == null)
            {
                return false;
            }

            if (propertyName == nameof(CrestLevel))
            {
                return weir.IsUsingTimeSeriesForCrestLevel();
            }

            if (propertyName == nameof(CrestWidth))
            {
                return weir.IsUsingTimeSeriesForCrestLevel() || weir.GetStructureType() == StructureType.UniversalWeir;
            }

            if (propertyName == nameof(FlowDirection) || propertyName == nameof(SDischargeCoefficient))
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
                    return GatedWeirFormula.IsUsingTimeSeriesForLowerEdgeLevel();
                }

                return Formula != FormulaEnum.GeneralStructure;
            }

            if (propertyName == nameof(UseVelocityHeight))
            {
                return !(Weir is IOrifice) && Formula == FormulaEnum.FreeFormWeir;
            }

            if (propertyName == nameof(GContractionCoefficient) ||
                propertyName == nameof(GLateralContraction) ||
                propertyName == nameof(GMaxFlowPos) ||
                propertyName == nameof(GMaxFlowNeg) ||
                propertyName == nameof(GUseMaxFlowPos) ||
                propertyName == nameof(GUseMaxFlowNeg))
            {
                return true;
            }

            return false;
        }

        private void SetFormula(FormulaEnum? newFormula, bool setToWeir)
        {
            GatedWeirFormula = defaultGatedWeirFormula;
            SimpleWeirFormula = defaultSimpleWeirFormula;
            FreeFormWeirFormula = defaultFreeFormWeirFormula;
            GeneralStructureWeirFormula = defaultGeneralStructureWeirFormula;

            switch (newFormula)
            {
                case FormulaEnum.SimpleWeir:
                    if (Weir is IOrifice)
                    {
                        break;
                    }

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
            Type type = weirFormula.GetEntityType();

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
    }
}