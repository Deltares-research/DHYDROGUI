using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// View model for <see cref="PumpShapeEditViewModel"/>.
    /// </summary>
    [Entity]
    public class PumpShapeEditViewModel
    {
        private readonly IPump pump;

        /// <summary>
        /// Initializes a new instance of the <see cref="PumpShapeEditViewModel"/> class.
        /// </summary>
        /// <param name="pump">The pump for this view model.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="pump"/> is <c>null</c>.</exception>
        public PumpShapeEditViewModel(IPump pump)
        {
            Ensure.NotNull(pump, nameof(pump));

            this.pump = pump;
        }

        /// <summary>
        /// Gets or sets the start suction of the pump.
        /// </summary>
        public double StartSuction
        {
            get => pump.StartSuction;
            set => pump.StartSuction = value;
        }

        /// <summary>
        /// Gets the description for the start suction.
        /// </summary>
        public string StartSuctionDescription => Resources.PumpShapeEditViewModel_StartSuctionDescription;

        /// <summary>
        /// Gets or sets the stop suction of the pump.
        /// </summary>
        public double StopSuction
        {
            get => pump.StopSuction;
            set => pump.StopSuction = value;
        }

        /// <summary>
        /// Gets the description for the stop suction.
        /// </summary>
        public string StopSuctionDescription => Resources.PumpShapeEditViewModel_StopSuctionDescription;

        /// <summary>
        /// Gets or sets the start delivery of the pump.
        /// </summary>
        public double StartDelivery
        {
            get => pump.StartDelivery;
            set => pump.StartDelivery = value;
        }

        /// <summary>
        /// Gets the description for the start delivery.
        /// </summary>
        public string StartDeliveryDescription => Resources.PumpShapeEditViewModel_StartDeliveryDescription;

        /// <summary>
        /// Gets or sets the stop delivery of the pump.
        /// </summary>
        public double StopDelivery
        {
            get => pump.StopDelivery;
            set => pump.StopDelivery = value;
        }

        /// <summary>
        /// Gets the description for the stop delivery.
        /// </summary>
        public string StopDeliveryDescription => Resources.PumpShapeEditViewModel_StopDeliveryDescription;

        /// <summary>
        /// Gets or sets the capacity of the pump.
        /// </summary>
        public double Capacity
        {
            get => pump.Capacity;
            set => pump.Capacity = value;
        }

        /// <summary>
        /// Gets the description for the capacity.
        /// </summary>
        public string CapacityDescription => Resources.PumpShapeEditViewModel_CapacityDescription;
    }
}