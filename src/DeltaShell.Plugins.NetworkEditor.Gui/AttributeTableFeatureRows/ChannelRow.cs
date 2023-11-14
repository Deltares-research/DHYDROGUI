using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="IChannel"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class ChannelRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly IChannel channel;

        /// <summary>
        /// Initialize a new instance of the <see cref="ChannelRow"/> class.
        /// </summary>
        /// <param name="channel"> The channel to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="channel"/> is <c>null</c>.
        /// </exception>
        public ChannelRow(IChannel channel)
            : base((INotifyPropertyChanged)channel)
        {
            Ensure.NotNull(channel, nameof(channel));
            this.channel = channel;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => channel.Name;
            set => channel.SetNameIfValid(value);
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => channel.LongName;
            set => channel.LongName = value;
        }

        [DisplayName("From node")]
        public INode Source => channel.Source;

        [DisplayName("To node")]
        public INode Target => channel.Target;

        [DisplayName("Has custom length")]
        public bool IsLengthCustom
        {
            get => channel.IsLengthCustom;
            set => channel.IsLengthCustom = value;
        }

        [DisplayName("Length")]
        [DynamicReadOnly]
        public double Length
        {
            get => channel.Length;
            set => channel.Length = value;
        }

        [DisplayName("Geometry length")]
        public double GeometryLength => channel.GeometryLength;

        [DisplayName("Order number")]
        public int OrderNumber
        {
            get => channel.OrderNumber;
            set => channel.OrderNumber = value;
        }

        [DisplayName("Cross sections")]
        public int CrossSectionCount => channel.CrossSections.Count();

        [DisplayName("Structures")]
        public int StructureCount => channel.Structures.Count();

        [DisplayName("Pumps")]
        public int PumpCount => channel.Pumps.Count();

        [DisplayName("Culverts")]
        public int CulvertCount => channel.Culverts.Count();

        [DisplayName("Bridges")]
        public int BridgeCount => channel.Bridges.Count();

        [DisplayName("Weirs")]
        public int WeirCount => channel.Weirs.Count();

        [DisplayName("Gates")]
        public int GateCount => channel.Gates.Count();

        [DisplayName("Lateral sources")]
        public int LateralSourcesCount => channel.BranchSources.Count();

        /// <summary>
        /// Gets the underlying <see cref="IChannel"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return channel;
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (propertyName == nameof(Length))
            {
                return !IsLengthCustom;
            }

            return false;
        }
    }
}