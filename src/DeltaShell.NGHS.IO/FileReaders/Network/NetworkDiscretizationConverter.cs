using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.FileReaders.Network
{
    public static class NetworkDiscretizationConverter
    {
        public static IList<INetworkLocation> Convert(IEnumerable<DelftIniCategory> categories, IList<IBranch> networkBranches, IList<string> errorMessages)
        {
            IList<INetworkLocation> networkLocations = new List<INetworkLocation>();
            foreach (var branchCategory in categories.Where(category => category.Name == NetworkDefinitionRegion.IniBranchHeader))
            {
                try
                {
                    var readBranchDiscretization = ReadDiscretizationDefinition(branchCategory, networkBranches);
                    networkLocations.AddRange(readBranchDiscretization);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }

            return networkLocations;
        }

        private static ICollection<INetworkLocation> ReadDiscretizationDefinition(IDelftIniCategory branchCategory, IEnumerable<IBranch> networkBranches)
        {
            var branchName = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Id.Key);
            var branch = networkBranches.FirstOrDefault(b => b.Name == branchName);
            
            // Optional Discretization Properties
            var gridPointsCount = branchCategory.ReadProperty<int>(NetworkDefinitionRegion.GridPointsCount.Key, true);
            var gridPointsX = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointX.Key, true);
            var gridPointsY = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointY.Key, true);
            var gridPointsOffsets = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointOffsets.Key, true);
            var gridPointsNames = branchCategory.ReadPropertiesToListOfType<string>(NetworkDefinitionRegion.GridPointNames.Key, true, ';');

            if (gridPointsX == null && gridPointsY == null && gridPointsOffsets == null & gridPointsNames == null)
            {
                return new List<INetworkLocation>();
            }

            var errorMessages = ValidateDataModel(branch, branchName, gridPointsCount, gridPointsX, gridPointsY, gridPointsOffsets, gridPointsNames).ToList();
            if(errorMessages.Any())
                throw new Exception(string.Join(Environment.NewLine, errorMessages));

            ICollection<INetworkLocation> discretizationForThisBranch = new List<INetworkLocation>();
            for (var i = 0; i < gridPointsCount; i++)
            {
                discretizationForThisBranch.Add(new NetworkLocation
                {
                    Branch = branch,
                    Chainage = gridPointsOffsets[i],
                    Geometry = new Point(gridPointsX[i], gridPointsY[i]),
                    Name = gridPointsNames[i]
                });
            }
            return discretizationForThisBranch;
        }

        private static IEnumerable<string> ValidateDataModel(IBranch branch, string branchName, int gridPointsCount, IList<double> gridPointsX, IList<double> gridPointsY, IList<double> gridPointsOffsets, IList<string> gridPointsNames)
        {
            if (branch == null)
            {
                yield return $"Could not add discretization points for branch with id '{branchName}', because it was not found in the network.";
                yield break;
            }

            var errorMessages = ValidatePropertyPresence(branchName, gridPointsX, gridPointsY, gridPointsOffsets, gridPointsNames).ToList();
            foreach (var errorMessage in errorMessages)
                yield return errorMessage;

            if (errorMessages.Any()) yield break; // When there are missing properties, do not validate further

            foreach (var errorMessage in ValidateDataQuantities(branchName, gridPointsCount, gridPointsX, gridPointsY, gridPointsOffsets, gridPointsNames))
                yield return errorMessage;

            foreach (var errorMessage in ValidateOffsetValues(branch, gridPointsOffsets))
                yield return errorMessage;
        }

        private static IEnumerable<string> ValidatePropertyPresence(string branchName, IList<double> gridPointsX, IList<double> gridPointsY, IList<double> gridPointsOffsets, IList<string> gridPointsNames)
        {
            if (gridPointsOffsets == null)
            {
                yield return $"The {NetworkDefinitionRegion.GridPointOffsets.Key} property is missing for branch '{branchName}'";
            }

            if (gridPointsX == null)
            {
                yield return $"The {NetworkDefinitionRegion.GridPointX.Key} property is missing for branch '{branchName}'";
            }

            if (gridPointsY == null)
            {
                yield return $"The {NetworkDefinitionRegion.GridPointY.Key} property is missing for branch '{branchName}'";
            }

            if (gridPointsNames == null)
            {
                yield return $"The {NetworkDefinitionRegion.GridPointNames.Key} property is missing for branch '{branchName}'";
            }
        }

        private static IEnumerable<string> ValidateDataQuantities(string branchName, int gridPointsCount, IList<double> gridPointsX, IList<double> gridPointsY, IList<double> gridPointsOffsets, IList<string> gridPointsNames)
        {
            if (gridPointsCount == 0)
                yield return $"There are zero discretization points defined for branch with id '{branchName}";

            if (gridPointsCount != gridPointsX.Count)
                yield return $"The amount of x-coordinates defined for discretization points on branch '{branchName}' does not match the defined amount of discretisation points.";

            if (gridPointsCount != gridPointsY.Count)
                yield return $"The amount of y-coordinates defined for discretization points on branch '{branchName}' does not match the defined amount of discretisation points.";

            if (gridPointsCount != gridPointsOffsets.Count)
                yield return $"The amount of offsets defined for discretization points on branch '{branchName}' does not match the defined amount of discretisation points.";

            if (gridPointsCount != gridPointsNames.Count)
                yield return $"The amount of names defined for discretization points on branch '{branchName}' does not match the defined amount of discretisation points.";
        }

        private static IEnumerable<string> ValidateOffsetValues(IBranch branch, IList<double> gridPointsOffsets)
        {
            for (var n = 0; n < gridPointsOffsets.Count; n++)
            {
                if (n < gridPointsOffsets.Count - 1 && gridPointsOffsets[n + 1] <= gridPointsOffsets[n])
                {
                    yield return $"Network location offsets of branch '{branch.Name}' are not ordered.";
                }
            }
        }
    }
}
