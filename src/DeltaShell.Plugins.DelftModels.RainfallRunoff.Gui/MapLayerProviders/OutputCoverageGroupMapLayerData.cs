using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.MapLayerProviders
{
    /// <summary>
    /// A data transfer object used to create the output coverage group map layers for the Rainfall Runoff model.
    /// </summary>
    public class OutputCoverageGroupMapLayerData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputCoverageGroupMapLayerData"/> class.
        /// </summary>
        /// <param name="name"> The name of the group layer. </param>
        /// <param name="coverages"> The coverages that should be in this group. </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or whitespace.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="coverages"/> is <c>null</c>.
        /// </exception>
        public OutputCoverageGroupMapLayerData(string name, IEnumerable<ICoverage> coverages)
        {
            Ensure.NotNullOrWhiteSpace(name, nameof(name));
            Ensure.NotNull(coverages, nameof(coverages));

            Name = name;
            Coverages = coverages;
        }

        /// <summary>
        /// The name of the group layer.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The coverages in this output coverage group.
        /// </summary>
        public IEnumerable<ICoverage> Coverages { get; }
    }
}