using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="Gate2D"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class Gate2DRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly Gate2D gate2D;

        /// <summary>
        /// Initialize a new instance of the <see cref="Gate2DRow"/> class.
        /// </summary>
        /// <param name="gate2D"> The gate 2D to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="gate2D"/> is <c>null</c>.
        /// </exception>
        public Gate2DRow(Gate2D gate2D)
            : base(gate2D)
        {
            Ensure.NotNull(gate2D, nameof(gate2D));
            this.gate2D = gate2D;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => gate2D.Name;
            set => gate2D.SetNameIfValid(value);
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => gate2D.LongName;
            set => gate2D.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => string.Empty;

        [DisplayName("Group name")]
        public string GroupName
        {
            get => gate2D.GroupName;
            set => gate2D.GroupName = value;
        }

        [DisplayName("SillLevel")]
        public double SillLevel
        {
            get => gate2D.SillLevel;
            set => gate2D.SillLevel = value;
        }

        [DisplayName("LowerEdgeLevel")]
        public double LowerEdgeLevel
        {
            get => gate2D.LowerEdgeLevel;
            set => gate2D.LowerEdgeLevel = value;
        }

        [DisplayName("OpeningWidth")]
        public double OpeningWidth
        {
            get => gate2D.OpeningWidth;
            set => gate2D.OpeningWidth = value;
        }

        [DisplayName("DoorHeight")]
        public double DoorHeight
        {
            get => gate2D.DoorHeight;
            set => gate2D.DoorHeight = value;
        }

        [DisplayName("HorizontalOpeningDirection")]
        public GateOpeningDirection HorizontalOpeningDirection
        {
            get => gate2D.HorizontalOpeningDirection;
            set => gate2D.HorizontalOpeningDirection = value;
        }

        [DisplayName("SillWidth")]
        public double SillWidth
        {
            get => gate2D.SillWidth;
            set => gate2D.SillWidth = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="Gate2D"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return gate2D;
        }
    }
}