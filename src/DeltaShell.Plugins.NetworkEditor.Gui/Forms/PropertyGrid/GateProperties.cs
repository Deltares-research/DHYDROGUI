using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "GateProperties_DisplayName")]
    public class GateProperties : ObjectProperties<IGate>
    {
        private NameValidator nameValidator = NameValidator.CreateDefault();
        
        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    data.Name = value;
                }
            }
        }

        [Category("General")]
        [DisplayName("Sill level")]
        [Description("Level of the gate sill above datum.")]
        [PropertyOrder(1)]
        [DynamicReadOnly]
        public string SillLevel
        {
            get
            {
                if (data.UseSillLevelTimeSeries)
                {
                    return "Time series";
                }
                return data.SillLevel.ToString(CultureInfo.CurrentCulture);
            }
            set
            {
                double level;
                if (double.TryParse(value, out level))
                {
                    data.SillLevel = level;
                }
            }
        }

        [Category("General")]
        [DisplayName("Sill level input")]
        [PropertyOrder(2)]
        [Description("Use a time series for the sill level or use a time constant value")]
        public TimeDependency UseSillLevelTimeSeries
        {
            get { return data.UseSillLevelTimeSeries ? TimeDependency.TimeDependent : TimeDependency.Constant; }
            set { data.UseSillLevelTimeSeries = value == TimeDependency.TimeDependent; }
        }

        [Category("General")]
        [PropertyOrder(3)]
        [DisplayName("Door height")]
        [Description("Height of the gate door")]
        public double DoorHeight
        {
            get { return data.DoorHeight; }
            set { data.DoorHeight = value; }
        }

        [Category("General")]
        [PropertyOrder(4)]
        [DisplayName("Horizontal opening direction")]
        [Description("Horizontal opening direction of gate doors")]
        public GateOpeningDirection HorizontalOpeningDirection
        {
            get { return data.HorizontalOpeningDirection; }
            set { data.HorizontalOpeningDirection = value; }
        }

        [Category("General")]
        [DisplayName("Lower edge level")]
        [Description("Level of the gate lower edge above datum.")]
        [PropertyOrder(5)]
        [DynamicReadOnly]
        public string LowerEdgeLevel
        {
            get
            {
                if (data.UseLowerEdgeLevelTimeSeries)
                {
                    return "Time series";
                }
                return data.LowerEdgeLevel.ToString(CultureInfo.CurrentCulture);
            }
            set
            {
                double level;
                if (double.TryParse(value, out level))
                {
                    data.LowerEdgeLevel = level;
                }
            }
        }

        [Category("General")]
        [DisplayName("Lower edge level input")]
        [PropertyOrder(6)]
        [Description("Use a time series for the lower edge level or use a time constant value")]
        public TimeDependency UseLowerEdgeLevelTimeSeries
        {
            get { return data.UseLowerEdgeLevelTimeSeries ? TimeDependency.TimeDependent : TimeDependency.Constant; }
            set { data.UseLowerEdgeLevelTimeSeries = value == TimeDependency.TimeDependent; }
        }


        [Category("General")]
        [DisplayName("Gate opening width")]
        [Description("Opening width of the gate")]
        [PropertyOrder(7)]
        [DynamicReadOnly]
        public string GateOpeningWidth
        {
            get
            {
                if (data.UseOpeningWidthTimeSeries)
                {
                    return "Time series";
                }
                return data.OpeningWidth.ToString(CultureInfo.CurrentCulture);
            }
            set
            {
                double openingWidth;
                if (double.TryParse(value, out openingWidth))
                {
                    data.OpeningWidth = openingWidth;
                }
            }
        }

        [Category("General")]
        [DisplayName("Gate opening width input")]
        [Description("Use a time series for the opening width or use a time constant value")]
        [PropertyOrder(8)]        
        public TimeDependency UseOpeningWidthTimeSeries
        {
            get { return data.UseOpeningWidthTimeSeries ? TimeDependency.TimeDependent : TimeDependency.Constant; }
            set { data.UseOpeningWidthTimeSeries = value == TimeDependency.TimeDependent; }
        }

        [Category("General")]
        [DisplayName("Sill width")]
        [PropertyOrder(9)]
        [Description("Width (in [m]) of the gate sill")]
        [DynamicReadOnly]
        public double SillWidth
        {
            get { return data.SillWidth; }
            set { data.SillWidth = value; }
        }

        [Category("General")]
        [DisplayName("Use sill width")]
        [PropertyOrder(10)]
        [Description("Use sill width or use gate geometry")]
        public bool UseSillWidth
        {
            get { return data.SillWidth > 0; }
            set { data.SillWidth = (value ? data.Geometry.Length : 0.0); }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (propertyName == nameof(SillLevel))
            {
                return data.UseSillLevelTimeSeries;
            }
            if (propertyName == nameof(LowerEdgeLevel))
            {
                return data.UseLowerEdgeLevelTimeSeries;
            }
            if (propertyName == nameof(GateOpeningWidth))
            {
                return data.UseOpeningWidthTimeSeries;
            }
            if(propertyName == nameof(SillWidth))
            {
                return data.SillWidth <= 0;
            }
            return false;
        }
        
        /// <summary>
        /// Get or set the <see cref="NameValidator"/> for this instance.
        /// Property is initialized with a default name validator. 
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public NameValidator NameValidator
        {
            get => nameValidator;
            set
            {
                Ensure.NotNull(value, nameof(value));
                nameValidator = value;
            }
        }
    }
}