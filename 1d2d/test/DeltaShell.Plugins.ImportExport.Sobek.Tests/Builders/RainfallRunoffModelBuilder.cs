using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.Builders
{
    /// <summary>
    /// Builder class for <see cref="RainfallRunoffModelBuilder"/>.
    /// </summary>
    internal class RainfallRunoffModelBuilder
    {
        private const string lateralSourceName = "LateralSource_1D_1";
        private const string hydroLinkName = "HydroLink_1D_1";
        private readonly RainfallRunoffModel rainfallRunoffModel;

        private RainfallRunoffModelBuilder()
        {
            rainfallRunoffModel = new RainfallRunoffModel() {};
        }

        /// <summary>
        /// Initialize a new builder.
        /// </summary>
        /// <returns> The created builder. </returns>
        public static RainfallRunoffModelBuilder Start() => new RainfallRunoffModelBuilder();

        /// <summary>
        /// Adds a <see cref="UnpavedData"/> to the <see cref="RainfallRunoffModel"/> with the specified name.
        /// </summary>
        /// <param name="catchmentName">The catchment name.</param>
        /// <returns> The current builder. </returns>
        public RainfallRunoffModelBuilder WithUnpavedCatchmentWithName(string catchmentName)
        {
            SobekRRLink[] links = CreateLinks(catchmentName);
            rainfallRunoffModel.LateralToCatchmentLookup.Add(lateralSourceName, links);
            var unpavedData = new UnpavedData(new Catchment() { Name = catchmentName });
            rainfallRunoffModel.ModelData.Add(unpavedData);
            return this;
        }

        /// <summary>
        /// Builds the final <see cref="RainfallRunoffModel"/>.
        /// </summary>
        /// <returns>
        /// The built rainfallRunoffModel.
        /// </returns>
        public RainfallRunoffModel Build() => rainfallRunoffModel;

        private static SobekRRLink[] CreateLinks(string catchmentName)
        {
            SobekRRLink[] links =
            {
                new SobekRRLink()
                {
                    Id = hydroLinkName,
                    NodeFromId = catchmentName,
                    NodeToId = lateralSourceName
                }
            };
            return links;
        }
    }
}