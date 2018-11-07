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
        public static IList<INetworkLocation> Convert(IEnumerable<DelftIniCategory> categories, IList<IBranch> networkBranches, IList<FileReadingException> fileReadingExceptions)
        {
            IList<INetworkLocation> networkLocations = new List<INetworkLocation>();
            IList<string> errorMessages = new List<string>();
            foreach (var branchCategory in categories.Where(category => category.Name == NetworkDefinitionRegion.IniBranchHeader))
            {
                try
                {
                    var readBranchDiscretization = ReadDiscretizationDefinition(branchCategory, networkBranches);
                    if (readBranchDiscretization != null)
                        networkLocations.AddRange(readBranchDiscretization);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }

            if (errorMessages.Count > 0)
            {
                var fileReadingException = FileReadingException.GetReportAsException("network discretization", errorMessages);
                fileReadingExceptions.Add(fileReadingException);
            }

            return networkLocations;
        }

        private static ICollection<INetworkLocation> ReadDiscretizationDefinition(IDelftIniCategory branchCategory, IEnumerable<IBranch> networkBranches)
        {
            // Find branch in network
            var idValue = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Id.Key);
            var branch = networkBranches.FirstOrDefault(b => b.Name == idValue);
            
            // Optional Discretization Properties
            var gridPointsCount = branchCategory.ReadProperty<int>(NetworkDefinitionRegion.GridPointsCount.Key, true);
            var gridPointsX = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointX.Key, true);
            var gridPointsY = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointY.Key, true);
            var gridPointsOffsets = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointOffsets.Key, true);
            var gridPointsNames = branchCategory.ReadPropertiesToListOfType<string>(NetworkDefinitionRegion.GridPointNames.Key, true, ';');

            if (gridPointsCount == 0) return null;

            ValidateDataModel(branch, idValue, gridPointsCount, gridPointsX.Count, gridPointsY.Count, gridPointsOffsets.Count, gridPointsNames.Count);

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

        private static void ValidateDataModel(IBranch branch, string idValue, int gridPointsCount, int gridPointsXCount, int gridPointsYCount, int gridPointsOffsetsCount, int gridPointsNamesCount)
        {
            if (branch == null)
                throw new FileReadingException($"Could not add discretization points for branch with id '{idValue}', because it was not found in the network.");

            if (gridPointsCount != gridPointsXCount)
                throw new FileReadingException($"The amount of x-coordinates defined for discretisation points on branch '{idValue}' does not match the defined amount of discretisation points.");

            if (gridPointsCount != gridPointsYCount)
                throw new FileReadingException($"The amount of y-coordinates defined for discretisation points on branch '{idValue}' does not match the defined amount of discretisation points.");

            if (gridPointsCount != gridPointsOffsetsCount)
                throw new FileReadingException($"The amount of offsets defined for discretisation points on branch '{idValue}' does not match the defined amount of discretisation points.");

            if (gridPointsCount != gridPointsNamesCount)
                throw new FileReadingException($"The amount of names defined for discretisation points on branch '{idValue}' does not match the defined amount of discretisation points.");
        }
    }
}
