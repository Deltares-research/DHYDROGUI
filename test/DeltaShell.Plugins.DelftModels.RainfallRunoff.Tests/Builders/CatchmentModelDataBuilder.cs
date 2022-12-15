using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using NSubstitute;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Builders
{
    /// <summary>
    /// Builder class for <see cref="CatchmentModelData"/>.
    /// </summary>
    internal class CatchmentModelDataBuilder
    {
        private readonly CatchmentModelData catchmentModelData;

        private CatchmentModelDataBuilder()
        {
            catchmentModelData = new TestCatchmentModelData(new Catchment());
        }

        /// <summary>
        /// Initialize a new builder.
        /// </summary>
        /// <returns> The created builder. </returns>
        public static CatchmentModelDataBuilder Start() => new CatchmentModelDataBuilder();

        /// <summary>
        /// Sets the name of the catchment model data and its catchment to the specified name.
        /// </summary>
        /// <param name="name"> The catchment name. </param>
        /// <returns> The current builder. </returns>
        public CatchmentModelDataBuilder WithName(string name)
        {
            catchmentModelData.Catchment.Name = name;
            return this;
        }

        /// <summary>
        /// Adds a <see cref="HydroLink"/> from the catchment to a <see cref="ILateralSource"/> with the specified name.
        /// </summary>
        /// <param name="lateralSourceName"> The lateral source name. </param>
        /// <returns> The current builder. </returns>
        public CatchmentModelDataBuilder WithLinkToLateralSource(string lateralSourceName)
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
        public CatchmentModelDataBuilder WithLinkToRunoffBoundary()
        {
            AddLinkTo(new RunoffBoundary());
            return this;
        }

        /// <summary>
        /// Adds a <see cref="HydroLink"/> from the catchment to a <see cref="RunoffBoundary"/>.
        /// </summary>
        /// <returns> The current builder. </returns>
        public CatchmentModelDataBuilder WithLinkToWasteWaterTreatmentPlant()
        {
            AddLinkTo(new WasteWaterTreatmentPlant());
            return this;
        }

        /// <summary>
        /// Builds the final catchment model data.
        /// </summary>
        /// <returns>
        /// The built unpaved data.
        /// </returns>
        public CatchmentModelData Build() => catchmentModelData;

        private void AddLinkTo(IHydroObject hydroObject)
        {
            catchmentModelData.Catchment.Links.Add(new HydroLink(catchmentModelData.Catchment, hydroObject));
        }

        private class TestCatchmentModelData : CatchmentModelData
        {
            public TestCatchmentModelData(Catchment catchment) : base(catchment) {}
        }
    }
}