using System;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using NSubstitute;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Builders
{
    /// <summary>
    /// Builder class for <see cref="UnpavedData"/>.
    /// </summary>
    internal class UnpavedDataBuilder
    {
        private readonly UnpavedData unpavedData;

        private UnpavedDataBuilder()
        {
            var catchment = new Catchment();
            var boundaryData = new RainfallRunoffBoundaryData();
            unpavedData = new UnpavedData(catchment);
            unpavedData.BoundarySettings.BoundaryData = boundaryData;
        }

        /// <summary>
        /// Initialize a new builder.
        /// </summary>
        /// <returns> The created builder. </returns>
        public static UnpavedDataBuilder Start() => new UnpavedDataBuilder();

        /// <summary>
        /// Sets the name of the unpaved data and its catchment to the specified name.
        /// </summary>
        /// <param name="name"> The catchment name. </param>
        /// <returns> The current builder. </returns>
        public UnpavedDataBuilder WithName(string name)
        {
            unpavedData.Catchment.Name = name;
            return this;
        }

        /// <summary>
        /// Adds a <see cref="HydroLink"/> from the catchment to a <see cref="ILateralSource"/> with the specified name.
        /// </summary>
        /// <param name="lateralSourceName"> The lateral source name. </param>
        /// <returns> The current builder. </returns>
        public UnpavedDataBuilder WithLinkToLateralSource(string lateralSourceName)
        {
            var lateralSource = Substitute.For<ILateralSource>();
            lateralSource.Name = lateralSourceName;
            AddLinkTo(lateralSource);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="HydroLink"/> from the catchment to a <see cref="RunoffBoundary"/>.
        /// </summary>
        /// <returns> The current builder. </returns>
        public UnpavedDataBuilder WithLinkToRunoffBoundary()
        {
            AddLinkTo(new RunoffBoundary());
            return this;
        }

        /// <summary>
        /// Adds a <see cref="HydroLink"/> from the catchment to a <see cref="RunoffBoundary"/>.
        /// </summary>
        /// <returns> The current builder. </returns>
        public UnpavedDataBuilder WithLinkToWasteWaterTreatmentPlant()
        {
            AddLinkTo(new WasteWaterTreatmentPlant());
            return this;
        }

        /// <summary>
        /// Adds a time series to the boundary data of the catchment with the provided values starting on the start date.
        /// Values are one day apart.
        /// </summary>
        /// <returns> The current builder. </returns>
        public UnpavedDataBuilder WithTimeSeries(DateTime startDate, params double[] values)
        {
            unpavedData.BoundarySettings.BoundaryData.IsConstant = false;

            for (var i = 0; i < values.Length; i++)
            {
                unpavedData.BoundarySettings.BoundaryData.Data[startDate.AddDays(i)] = values[i];
            }

            return this;
        }

        /// <summary>
        /// Adds a constant value to the boundary data of the catchment.
        /// </summary>
        /// <returns> The current builder. </returns>
        public UnpavedDataBuilder WithConstantValue(double value)
        {
            unpavedData.BoundarySettings.BoundaryData.IsConstant = true;
            unpavedData.BoundarySettings.BoundaryData.Value = value;
            return this;
        }

        /// <summary>
        /// Builds the final unpaved data with the built catchment and boundary data.
        /// </summary>
        /// <returns>
        /// The built unpaved data.
        /// </returns>
        public UnpavedData Build() => unpavedData;

        private void AddLinkTo(IHydroObject hydroObject)
        {
            unpavedData.Catchment.Links.Add(new HydroLink(unpavedData.Catchment, hydroObject));
        }
    }
}