using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="IPipe"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class PipeRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly IPipe pipe;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="PipeRow"/> class.
        /// </summary>
        /// <param name="pipe"> The pipe to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="pipe"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public PipeRow(IPipe pipe, NameValidator nameValidator)
            : base(pipe)
        {
            Ensure.NotNull(pipe, nameof(pipe));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.pipe = pipe;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => pipe.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    pipe.Name = value;
                }
            }
        }

        [DisplayName("From manhole")]
        public string FromManhole => pipe.Source.Name;

        [DisplayName("To manhole")]
        public string ToManhole => pipe.Target.Name;

        [DisplayName("From compartment")]
        public string FromCompartment => pipe.SourceCompartment?.Name;

        [DisplayName("To compartment")]
        public string ToCompartment => pipe.TargetCompartment?.Name;

        [DisplayName("Geometry length (m)")]
        public double GeometryLength => pipe.GeometryLength;

        [DisplayName("Length (m)")]
        public double Length
        {
            get => pipe.Length;
            set => pipe.Length = value;
        }

        [DisplayName("Order number")]
        public int OrderNumber
        {
            get => pipe.OrderNumber;
            set => pipe.OrderNumber = value;
        }

        [DisplayName("Invert level from")]
        public double InvertLevelFrom
        {
            get => pipe.LevelSource;
            set => pipe.LevelSource = value;
        }

        [DisplayName("Invert level to")]
        public double InvertLevelTo
        {
            get => pipe.LevelTarget;
            set => pipe.LevelTarget = value;
        }

        [DisplayName("Sewer type")]
        public SewerConnectionWaterType SewerType
        {
            get => pipe.WaterType;
            set => pipe.WaterType = value;
        }

        [DisplayName("Sewer special connection type")]
        public SewerConnectionSpecialConnectionType SewerSpecialConnectionType => pipe.SpecialConnectionType;

        [DisplayName("Type")]
        public CrossSectionStandardShapeType? ProfileType => pipe.Profile?.ShapeType;

        [DisplayName("Width")]
        public double Width => pipe.CrossSection.Definition.Width;

        [DisplayName("Definition")]
        public string DefinitionName
        {
            get => pipe.DefinitionName;
            set => pipe.DefinitionName = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="IPipe"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return pipe;
        }
    }
}