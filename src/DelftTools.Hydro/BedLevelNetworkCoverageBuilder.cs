using System;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro
{
    public delegate double CrossSectionProfileFunction(CrossSections.ICrossSectionDefinition definition);

    /// <summary>
    /// Builds or extracts cross section profile data from a network route.
    /// </summary>
    public static class BedLevelNetworkCoverageBuilder
    {
        public const string BedLevelCoverageName = "Bed level";
        public const string LeftEmbankmentCoverageName = "Left embankment";
        public const string RightEmbankmentCoverageName = "Right embankment";
        public const string LowestEmbankmentCoverageName = "Lowest embankment";
        public const string HighestEmbankmentCoverageName = "Highest embankment";

        /// <summary>
        /// Builds a coverage based on the application of the CrossSectionProfileFunction upon a cross section definition.
        /// </summary>
        /// <param name="network"></param>
        /// <returns></returns>
        public static INetworkCoverage BuildCoverageFromNetwork(IHydroNetwork network, CrossSectionProfileFunction crossSectionProfileFunction, string coverageName)
        {
            if (network == null)
                throw new InvalidOperationException("No network defined");

            // Get the lowest value for each cross section and add the values to the coverage
            var networkCoverage = new NetworkCoverage(coverageName, false, coverageName, "m AD") { Network = network };
            networkCoverage.Arguments[0].Name = "x";
            networkCoverage.Arguments[0].Unit = new Unit() {Symbol = "m"};
            
            // disable segmentation for performance
            SegmentGenerationMethod segmentGenerationMethod = networkCoverage.SegmentGenerationMethod;
            networkCoverage.SegmentGenerationMethod = SegmentGenerationMethod.None;

            bool editDisabled = EditActionSettings.Disabled;
            EditActionSettings.Disabled = false;
                // HACK: switch off edit action attribute in set values in function to allow updating during undo/redo.
            foreach (var crossSection in network.CrossSections)
            {
                var value = crossSectionProfileFunction(crossSection.Definition);
                if (!double.IsNaN(value))
                {
                    var location = new NetworkLocation(crossSection.Branch, crossSection.Chainage);
                    networkCoverage[location] = value;
                }
            }
            EditActionSettings.Disabled = editDisabled;
            
            //switch back to previous segmentation method
            networkCoverage.SegmentGenerationMethod = segmentGenerationMethod;

            return networkCoverage;
        }

        /// <summary>
        /// Builds a coverage based on the network cross sections and points on the route.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static INetworkCoverage BuildCoverageFromRoute(INetworkCoverage route, CrossSectionProfileFunction crossSectionProfileFunction, string coverageName)
        {
            if (route == null)
                throw new InvalidOperationException("No route defined");


            var networkCoverage = BuildCoverageFromNetwork(route.Network as IHydroNetwork, crossSectionProfileFunction, coverageName);

            // can Locations be null here?
            networkCoverage.Locations.InterpolationType = InterpolationType.Linear;

            //suspend segment generation
            SegmentGenerationMethod segmentGenerationMethod = networkCoverage.SegmentGenerationMethod;
            networkCoverage.SegmentGenerationMethod = SegmentGenerationMethod.None;

            //extend coverage with the locations in the route.
            bool editDisabled = EditActionSettings.Disabled;
            EditActionSettings.Disabled = false;
                // HACK: switch off edit action attribute in set values in function to allow updating during undo/redo.
            foreach (INetworkLocation location in route.Locations.Values)
            {
                var networkLocation = (NetworkLocation) location.Clone();
                if (!networkCoverage.Locations.Values.Contains(networkLocation))
                {
                    networkCoverage[networkLocation] = networkCoverage.Evaluate(networkLocation);
                }
            }
            EditActionSettings.Disabled = editDisabled;
            
            // reenable segmentation for performance
            networkCoverage.SegmentGenerationMethod = segmentGenerationMethod;

            return networkCoverage;
        }

        public static INetworkCoverage BuildBedLevelCoverage(IHydroNetwork hydroNetwork)
        {
            return BuildCoverageFromNetwork(hydroNetwork, csd => csd.LowestPoint, BedLevelCoverageName);
        }

        public static INetworkCoverage BuildBedLevelCoverage(INetworkCoverage route)
        {
            return BuildCoverageFromRoute(route, csd => csd.LowestPoint, BedLevelCoverageName);
        }
        
        public static INetworkCoverage BuildLeftEmbankmentCoverage(IHydroNetwork hydroNetwork)
        {
            return BuildCoverageFromNetwork(hydroNetwork, csd => csd.LeftEmbankment, LeftEmbankmentCoverageName);
        }

        public static INetworkCoverage BuildLeftEmbankmentCoverage(INetworkCoverage route)
        {
            return BuildCoverageFromRoute(route, csd => csd.LeftEmbankment, LeftEmbankmentCoverageName);
        }

        public static INetworkCoverage BuildRightEmbankmentCoverage(IHydroNetwork hydroNetwork)
        {
            return BuildCoverageFromNetwork(hydroNetwork, csd => csd.RightEmbankment, RightEmbankmentCoverageName);
        }

        public static INetworkCoverage BuildRightEmbankmentCoverage(INetworkCoverage route)
        {
            return BuildCoverageFromRoute(route, csd => csd.RightEmbankment, RightEmbankmentCoverageName);
        }

        public static INetworkCoverage BuildLowestEmbankmentCoverage(IHydroNetwork hydroNetwork)
        {
            return BuildCoverageFromNetwork(hydroNetwork, csd => Math.Min(csd.LeftEmbankment, csd.RightEmbankment), LowestEmbankmentCoverageName);
        }

        public static INetworkCoverage BuildLowestEmbankmentCoverage(INetworkCoverage route)
        {
            return BuildCoverageFromRoute(route, csd => Math.Min(csd.LeftEmbankment, csd.RightEmbankment), LowestEmbankmentCoverageName);
        }

        public static INetworkCoverage BuildHighestEmbankmentCoverage(IHydroNetwork hydroNetwork)
        {
            return BuildCoverageFromNetwork(hydroNetwork, csd => Math.Max(csd.LeftEmbankment, csd.RightEmbankment), HighestEmbankmentCoverageName);
        }

        public static INetworkCoverage BuildHighestEmbankmentCoverage(INetworkCoverage route)
        {
            return BuildCoverageFromRoute(route, csd => Math.Max(csd.LeftEmbankment, csd.RightEmbankment), HighestEmbankmentCoverageName);
        }
    }
}