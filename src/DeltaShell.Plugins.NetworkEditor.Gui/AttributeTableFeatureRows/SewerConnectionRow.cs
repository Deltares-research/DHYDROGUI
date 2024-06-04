using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="ISewerConnection"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class SewerConnectionRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly ISewerConnection sewerConnection;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="SewerConnectionRow"/> class.
        /// </summary>
        /// <param name="sewerConnection"> The sewer connection to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="sewerConnection"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public SewerConnectionRow(ISewerConnection sewerConnection, NameValidator nameValidator)
            : base(sewerConnection)
        {
            Ensure.NotNull(sewerConnection, nameof(sewerConnection));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.sewerConnection = sewerConnection;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => sewerConnection.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    sewerConnection.Name = value;
                }
            }
        }

        [DisplayName("From manhole")]
        public string FromManhole => sewerConnection.Source.Name;

        [DisplayName("To manhole")]
        public string ToManhole => sewerConnection.Target.Name;

        [DisplayName("From compartment")]
        public string FromCompartment => sewerConnection.SourceCompartment?.Name;

        [DisplayName("To compartment")]
        public string ToCompartment => sewerConnection.TargetCompartment?.Name;

        [DisplayName("Geometry length (m)")]
        public double GeometryLength => sewerConnection.GeometryLength;

        [DisplayName("Length (m)")]
        public double Length
        {
            get => sewerConnection.Length;
            set => sewerConnection.Length = value;
        }

        [DisplayName("Order number")]
        public int OrderNumber
        {
            get => sewerConnection.OrderNumber;
            set => sewerConnection.OrderNumber = value;
        }

        [DisplayName("Invert level from")]
        public double InvertLevelFrom
        {
            get => sewerConnection.LevelSource;
            set => sewerConnection.LevelSource = value;
        }

        [DisplayName("Invert level to")]
        public double InvertLevelTo
        {
            get => sewerConnection.LevelTarget;
            set => sewerConnection.LevelTarget = value;
        }

        [DisplayName("Sewer type")]
        public SewerConnectionWaterType SewerType
        {
            get => sewerConnection.WaterType;
            set => sewerConnection.WaterType = value;
        }

        [DisplayName("Sewer special connection type")]
        public SewerConnectionSpecialConnectionType SewerSpecialConnectionType => sewerConnection.SpecialConnectionType;

        [DisplayName("Definition")]
        public string DefinitionName
        {
            get => sewerConnection.DefinitionName;
            set => sewerConnection.DefinitionName = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="ISewerConnection"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return sewerConnection;
        }
    }
}