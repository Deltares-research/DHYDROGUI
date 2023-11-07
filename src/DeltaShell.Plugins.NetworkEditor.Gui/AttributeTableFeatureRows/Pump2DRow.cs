using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="Pump2D"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class Pump2DRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly Pump2D pump2D;

        /// <summary>
        /// Initialize a new instance of the <see cref="Pump2DRow"/> class.
        /// </summary>
        /// <param name="pump2D"> The pump 2D to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="pump2D"/> is <c>null</c>.
        /// </exception>
        public Pump2DRow(Pump2D pump2D)
            : base(pump2D)
        {
            Ensure.NotNull(pump2D, nameof(pump2D));
            this.pump2D = pump2D;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => pump2D.Name;
            set => pump2D.Name = value;
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => pump2D.LongName;
            set => pump2D.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => string.Empty;

        [DisplayName("Positive direction")]
        public bool PositiveDirection
        {
            get => pump2D.DirectionIsPositive;
            set => pump2D.DirectionIsPositive = value;
        }

        [DisplayName("Capacity")]
        public double Capacity
        {
            get => pump2D.Capacity;
            set => pump2D.Capacity = value;
        }

        [DisplayName("Start delivery")]
        public double StartDelivery
        {
            get => pump2D.StartDelivery;
            set => pump2D.StartDelivery = value;
        }

        [DisplayName("Stop delivery")]
        public double StopDelivery
        {
            get => pump2D.StopDelivery;
            set => pump2D.StopDelivery = value;
        }

        [DisplayName("Start suction")]
        public double StartSuction
        {
            get => pump2D.StartSuction;
            set => pump2D.StartSuction = value;
        }

        [DisplayName("Stop suction")]
        public double StopSuction
        {
            get => pump2D.StopSuction;
            set => pump2D.StopSuction = value;
        }

        [DisplayName("Control on")]
        public PumpControlDirection ControlOn
        {
            get => pump2D.ControlDirection;
            set => pump2D.ControlDirection = value;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => pump2D.GroupName;
            set => pump2D.GroupName = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="Pump2D"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature() => pump2D;
    }
}