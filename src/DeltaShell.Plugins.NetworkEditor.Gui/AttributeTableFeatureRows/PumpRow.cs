using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="IPump"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class PumpRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly IPump pump;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="PumpRow"/> class.
        /// </summary>
        /// <param name="pump"> The pump to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="pump"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public PumpRow(IPump pump, NameValidator nameValidator)
            : base((INotifyPropertyChanged)pump)
        {
            Ensure.NotNull(pump, nameof(pump));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.pump = pump;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => pump.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    pump.Name = value;
                }
            }
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => pump.LongName;
            set => pump.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => pump.Branch.Name;

        [DisplayName("Positive direction")]
        public bool PositiveDirection
        {
            get => pump.DirectionIsPositive;
            set => pump.DirectionIsPositive = value;
        }

        [DisplayName("Capacity")]
        public double Capacity
        {
            get => pump.Capacity;
            set => pump.Capacity = value;
        }

        [DisplayName("Start delivery")]
        public double StartDelivery
        {
            get => pump.StartDelivery;
            set => pump.StartDelivery = value;
        }

        [DisplayName("Stop delivery")]
        public double StopDelivery
        {
            get => pump.StopDelivery;
            set => pump.StopDelivery = value;
        }

        [DisplayName("Start suction")]
        public double StartSuction
        {
            get => pump.StartSuction;
            set => pump.StartSuction = value;
        }

        [DisplayName("Stop suction")]
        public double StopSuction
        {
            get => pump.StopSuction;
            set => pump.StopSuction = value;
        }

        [DisplayName("Control on")]
        public PumpControlDirection ControlOn
        {
            get => pump.ControlDirection;
            set => pump.ControlDirection = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="IPump"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return pump;
        }
    }
}