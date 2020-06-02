using System.Collections.Generic;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    public interface INetworkSideViewData
    {
        /// <summary>
        /// Gets the route dependend network
        /// </summary>
        INetwork Network { get; }

        /// <summary>
        /// Gets or sets the network route data
        /// </summary>
        INetworkCoverage NetworkRoute { get; }

        /// <summary>
        /// Gets the maximum Y value for a route (can be a structure OffsetZ or coverage value)
        /// </summary>
        double ZMaxValue { get; }

        /// <summary>
        /// Gets the minumum Y value for a route (can be a structure OffsetZ or coverage value)
        /// </summary>
        double ZMinValue { get; }

        /// <summary>
        /// Gets or sets the water level network coverage
        /// </summary>
        INetworkCoverage WaterLevelNetworkCoverage { get; }

        /// <summary>
        /// Gets the bed level network coverage
        /// </summary>
        /// <remarks>
        /// The bed level network coverage will only be set (for caching purpose)
        /// when the NetworkRoute (NetworkCoverage) is set.
        /// To get a new rebuilded BedLevelNetworkCovarage object use the
        /// function BuildBottomLevelNetworkCoverage()
        /// </remarks>
        INetworkCoverage BedLevelNetworkCoverage { get; } // { return BedLevelNetworkCoverageBuilder.BuildCoverage(route); }

        // todo refactor this interface?
        IList<INetworkCoverage> AllNetworkCoverages { get; }
        IList<IFeatureCoverage> AllFeatureCoverages { get; }
        IList<INetworkCoverage> RenderedNetworkCoverages { get; }
        IList<IFeatureCoverage> RenderedFeatureCoverages { get; }

        void AddRenderedCoverage(INetworkCoverage networkCoverage);
        void RemoveRenderedCoverage(INetworkCoverage networkCoverage);
        void AddRenderedCoverage(IFeatureCoverage featureCoverage);
        void RemoveRenderedCoverage(IFeatureCoverage featureCoverage);
    }
}