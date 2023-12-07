using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses
{
    [DisplayName("Catchment")]
    public class CatchmentProperties : ObjectProperties<Catchment>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CatchmentProperties));
        private NameValidator nameValidator = NameValidator.CreateDefault();

        [Browsable(false)]
        internal CatchmentModelData CatchmentData { get; set; }

        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get => data.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    data.Name = value;
                }
            }
        }

        [Category("General")]
        [PropertyOrder(1)]
        public string LongName
        {
            get => data.LongName;
            set => data.LongName = value;
        }

        public CatchmentType CatchmentType => data.CatchmentType;

        [Category("General")]
        [DisplayName("Catchment type")]
        [PropertyOrder(2)]
        public virtual CatchmentTypes CatchmentTypes
        {
            get => data.CatchmentType.Types;
            set => data.CatchmentTypes = value;
        }

        [Description("Catchment area based on input data used for computation.\n" +
                     "This can differ from the geometry area")]
        [Category("General")]
        [DisplayName("Computation area (m²)")]
        [PropertyOrder(3)]
        public double ComputationArea
        {
            get => CatchmentData?.CalculationArea ?? double.NaN;
            set
            {
                if (CatchmentData == null)
                {
                    log.Error($"Could not set {data.Name} computation area");
                    return;
                }

                CatchmentData.CalculationArea = value;
            }
        }

        [Description("Catchment area based on geometry.")]
        [Category("General")]
        [DisplayName("Geometry area (m²)")]
        [PropertyOrder(4)]
        public double GeometryArea => data.Geometry?.Area ?? double.NaN;

        [Description("If TRUE, catchment geometry is derived from computation area")]
        [Category("General")]
        [DisplayName("Generated geometry")]
        [PropertyOrder(5)]
        public bool IsDefaultGeometry => data.IsGeometryDerivedFromAreaSize;

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