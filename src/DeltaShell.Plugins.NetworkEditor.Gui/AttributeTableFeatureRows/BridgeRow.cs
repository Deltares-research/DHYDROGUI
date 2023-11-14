using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="IBridge"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class BridgeRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly IBridge bridge;

        /// <summary>
        /// Initialize a new instance of the <see cref="BridgeRow"/> class.
        /// </summary>
        /// <param name="bridge"> The bridge to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="bridge"/> is <c>null</c>.
        /// </exception>
        public BridgeRow(IBridge bridge)
            : base((INotifyPropertyChanged)bridge)
        {
            Ensure.NotNull(bridge, nameof(bridge));
            this.bridge = bridge;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => bridge.Name;
            set => bridge.SetNameIfValid(value);
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => bridge.LongName;
            set => bridge.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => bridge.Branch.Name;

        [DisplayName("Shape")]
        public BridgeType BridgeType

        {
            get => bridge.BridgeType;
            set => bridge.BridgeType = value;
        }

        [DynamicReadOnly]
        [DisplayName("Length")]
        public double BridgeLength
        {
            get => bridge.Length;
            set => bridge.Length = value;
        }

        [DisplayName("Flow direction")]
        public FlowDirection FlowDirection
        {
            get => bridge.FlowDirection;
            set => bridge.FlowDirection = value;
        }

        [DynamicReadOnly]
        [DisplayName("Inlet loss coefficient")]
        public double InletLossCoefficient
        {
            get => bridge.InletLossCoefficient;
            set => bridge.InletLossCoefficient = value;
        }

        [DynamicReadOnly]
        [DisplayName("Outlet loss coefficient")]
        public double OutletLossCoefficient
        {
            get => bridge.OutletLossCoefficient;
            set => bridge.OutletLossCoefficient = value;
        }

        [DynamicReadOnly]
        [DisplayName("Roughness type")]
        public BridgeFrictionType FrictionType
        {
            get => bridge.FrictionType;
            set => bridge.FrictionType = value;
        }

        [DynamicReadOnly]
        [DisplayName("Roughness")]
        public double Friction
        {
            get => bridge.Friction;
            set => bridge.Friction = value;
        }

        [DynamicReadOnly]
        [DisplayName("Shift")]
        public double Shift
        {
            get => bridge.Shift;
            set => bridge.Shift = value;
        }

        [DynamicReadOnly]
        [DisplayName("Width")]
        public double Width
        {
            get => bridge.Width;
            set => bridge.Width = value;
        }

        [DynamicReadOnly]
        [DisplayName("Height")]
        public double Height
        {
            get => bridge.Height;
            set => bridge.Height = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="IBridge"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return bridge;
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (IsReadOnlyWhenIsPillar(propertyName))
            {
                return bridge.IsPillar;
            }

            if (IsEditableWhenIsRectangle(propertyName))
            {
                return !bridge.IsRectangle;
            }

            return false;
        }

        private bool IsEditableWhenIsRectangle(string propertyName)
        {
            return propertyName == nameof(Width) ||
                   propertyName == nameof(Height);
        }

        private bool IsReadOnlyWhenIsPillar(string propertyName)
        {
            return propertyName == nameof(BridgeLength) ||
                   propertyName == nameof(FrictionType) ||
                   propertyName == nameof(Friction) ||
                   propertyName == nameof(InletLossCoefficient) ||
                   propertyName == nameof(OutletLossCoefficient);
        }
    }
}