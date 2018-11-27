using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Validators;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public static class FiniteVolumeDiscretizationProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FiniteVolumeDiscretizationProvider));

        /// <summary>
        /// Returns a finite discretization, based on <paramref name="networkDiscretization"/> and according to <paramref name="finiteVolumeDiscretizationType"/>
        /// </summary>
        /// <param name="networkDiscretization">The discretization that must be turned into a finite discretization</param>
        /// <param name="finiteVolumeDiscretizationType">The type of finite discretization that should be set</param>
        /// <returns>A finite discretization of a specific type</returns>
        public static IDiscretization CreateFiniteDiscretization(IDiscretization networkDiscretization, FiniteVolumeDiscretizationType finiteVolumeDiscretizationType)
        {
            var finiteVolumeDiscretization = new Discretization
                                                 {
                                                     Network = networkDiscretization.Network,
                                                     Name = "Finite Volume Discretization"
                                                 };

            SetFiniteDiscretizationValues(networkDiscretization, finiteVolumeDiscretization, finiteVolumeDiscretizationType);
            
            return finiteVolumeDiscretization;
        }

        /// <summary>
        /// Sets a finite discretization to <paramref name="finiteVolumeDiscretization"/>, based on <paramref name="networkDiscretization"/> and according to <paramref name="finiteVolumeDiscretizationType"/>
        /// </summary>
        /// <param name="networkDiscretization">The discretization that must be turned into a finite discretization</param>
        /// <param name="finiteVolumeDiscretization">The discretization which the finite discretization must be set to</param>
        /// <param name="finiteVolumeDiscretizationType">The type of finite discretization that should be set</param>
        public static void SetFiniteDiscretizationValues(IDiscretization networkDiscretization, IDiscretization finiteVolumeDiscretization, FiniteVolumeDiscretizationType finiteVolumeDiscretizationType)
        {
            if (networkDiscretization.Network != finiteVolumeDiscretization.Network)
            {
                finiteVolumeDiscretization.Network = networkDiscretization.Network;
            }

            switch (finiteVolumeDiscretizationType)
            {
                case FiniteVolumeDiscretizationType.OnGridPoints:
                    {
                        finiteVolumeDiscretization.Clear();

                        if (!NetworkDiscretizationIsValid(networkDiscretization)) return;

                        SetOnGridPointsDiscretizationValues(networkDiscretization, finiteVolumeDiscretization);
                        
                        return;
                    }

                case FiniteVolumeDiscretizationType.OnReachSegments:
                    {
                        finiteVolumeDiscretization.Clear();

                        if (!NetworkDiscretizationIsValid(networkDiscretization)) return;
                        
                        SetOnReachSegmentsDiscretizationValues(networkDiscretization, finiteVolumeDiscretization);
                        
                        return;
                    }

                case FiniteVolumeDiscretizationType.None:
                    return;

                default:
                    throw new NotSupportedException(string.Format("Finite volume discretizationType '{0}' is not supported yet", finiteVolumeDiscretizationType));
            }
        }

        private static bool NetworkDiscretizationIsValid(IDiscretization networkDiscretization)
        {
            if (networkDiscretization.SegmentGenerationMethod == SegmentGenerationMethod.None || networkDiscretization.Locations.Values.Count == 0)
            {
                return false;
            }

            var validationReport = DiscretizationValidator.Validate(networkDiscretization);

            if (validationReport.Severity() == ValidationSeverity.Error)
            {
                Log.Warn("An empty finite discretization will be created because the original discretization is invalid (see validation report)");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Adds grid points to <param name="finiteVolumeDiscretization"/> based on <param name="originalNetworkDiscretization"/> for discretization type "OnGridPoints"
        /// </summary>
        private static void SetOnGridPointsDiscretizationValues(IDiscretization originalNetworkDiscretization, IDiscretization finiteVolumeDiscretization)
        {
            var finiteVolumeStructureNetworkLocations = GetFiniteVolumeStructureNetworkLocations(originalNetworkDiscretization.Network).ToList();

            foreach (var branch in originalNetworkDiscretization.Network.Branches)
            {
                finiteVolumeDiscretization[new NetworkLocation(branch, 0)] = 0.0; // Add a location at the beginning of the branch
                finiteVolumeDiscretization[new NetworkLocation(branch, branch.Length)] = 0.0; // Add a location at the end of the branch

                var networkLocations = originalNetworkDiscretization.Locations.Values.Where(l => l.Branch == branch).ToList();
                var finiteVolumeStructureNetworkLocationsForBranch = finiteVolumeStructureNetworkLocations.Where(s => s.Branch == branch);

                for (var i = 1; i < networkLocations.Count(); i++)
                {
                    var firstLocationOffset = networkLocations[i - 1].Chainage;
                    var secondLocationOffset = networkLocations[i].Chainage;
                    var structureBetweenLocations = finiteVolumeStructureNetworkLocationsForBranch.FirstOrDefault(bs => bs.Chainage > firstLocationOffset && bs.Chainage < secondLocationOffset);

                    var offset = structureBetweenLocations != null
                        ? structureBetweenLocations.Chainage // Add a location at the offset of the structure
                        : firstLocationOffset + (secondLocationOffset - firstLocationOffset) / 2.0; // Or add a new location between the two offset locations

                    // Optimization: No snapping needed, as edge cases are already covered.
                    finiteVolumeDiscretization[new NetworkLocation(branch, offset)] = 0.0;
                }
            }
        }

        /// <summary>
        /// Adds grid points to <param name="finiteVolumeDiscretization"/> based on <param name="originalNetworkDiscretization"/> for discretization type "OnReachSegments"
        /// </summary>
        private static void SetOnReachSegmentsDiscretizationValues(IDiscretization originalNetworkDiscretization, IDiscretization finiteVolumeDiscretization)
        {
            foreach (var networkLocation in originalNetworkDiscretization.Locations.Values)
            {
                finiteVolumeDiscretization[new NetworkLocation(networkLocation.Branch, networkLocation.Chainage)] = 0.0;
            }

            // Add a location for each composite structure, except for composite structures that only contain extra resistances
            var finiteVolumeStructureNetworkLocations = GetFiniteVolumeStructureNetworkLocations(finiteVolumeDiscretization.Network);

            foreach (var networkLocation in finiteVolumeStructureNetworkLocations)
            {
                finiteVolumeDiscretization[networkLocation] = 0.0;
            }
        }

        private static IEnumerable<INetworkLocation> GetFiniteVolumeStructureNetworkLocations(INetwork network)
        {
            foreach (var structure in network.BranchFeatures.OfType<IStructure1D>())
            {
                var compositeStructure = structure as CompositeBranchStructure;

                // Don't return network locations for composite structures that only contain extra resistances
                if (compositeStructure != null && compositeStructure.Structures.All(s => s is IExtraResistance))
                {
                    continue;
                }
                
                // Don't return network locations for extra resistances
                if (structure is IExtraResistance) continue;

                yield return new NetworkLocation(structure.Branch, structure.Chainage);   
            }
        }
    }
}
