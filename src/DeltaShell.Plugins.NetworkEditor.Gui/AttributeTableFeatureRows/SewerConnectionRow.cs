using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Guards;
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

        /// <summary>
        /// Initialize a new instance of the <see cref="SewerConnectionRow"/> class.
        /// </summary>
        /// <param name="sewerConnection"> The sewer connection to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="sewerConnection"/> is <c>null</c>.
        /// </exception>
        public SewerConnectionRow(ISewerConnection sewerConnection)
            : base(sewerConnection)
        {
            Ensure.NotNull(sewerConnection, nameof(sewerConnection));
            this.sewerConnection = sewerConnection;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => sewerConnection.Name;
            set => sewerConnection.Name = value;
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
        public IFeature GetFeature() => sewerConnection;
    }
}