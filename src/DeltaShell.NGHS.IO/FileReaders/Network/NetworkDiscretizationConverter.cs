using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.FileReaders.Network
{
    public static class NetworkDiscretizationConverter
    {
        public static IList<INetworkLocation> Convert(IList<DelftIniCategory> categories, IHydroNetwork network)
        {
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            IList<INetworkLocation> networkLocations = new List<INetworkLocation>();
            foreach (var branchCategory in categories.Where(category => category.Name == NetworkDefinitionRegion.IniBranchHeader))
            {
                try
                {
                    var readBranchDiscretization = ReadDiscretizationDefinition(branchCategory, network);
                    if (readBranchDiscretization != null)
                        networkLocations.AddRange(readBranchDiscretization);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read branch discretization", fileReadingException));
                }
            }

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException($"While reading discretizations for the branches an error occured : {Environment.NewLine} {string.Join(Environment.NewLine, innerExceptionMessages)}");
            }

            return networkLocations;
        }

        private static ICollection<INetworkLocation> ReadDiscretizationDefinition(DelftIniCategory branchCategory, IHydroNetwork network)
        {
            // Find branch in network
            var idValue = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Id.Key);
            var branch = network.Branches.FirstOrDefault(b => b.Name == idValue);
            if (branch == null)
                throw new FileReadingException($"Branch with id '{idValue}' not found in network, could not add a discretization (if available) to a branch which is not in the network...");

            // Optional Discretization Properties
            var gridPointsCount = branchCategory.ReadProperty<int>(NetworkDefinitionRegion.GridPointsCount.Key, true);
            var gridPointsX = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointX.Key, true);
            var gridPointsY = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointY.Key, true);
            var gridPointsOffsets = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointOffsets.Key, true);
            var gridPointsNames = branchCategory.ReadPropertiesToListOfType<string>(NetworkDefinitionRegion.GridPointNames.Key, true, ';');

            if (gridPointsCount == 0) return null;

            if (gridPointsCount != gridPointsX.Count ||
                gridPointsCount != gridPointsY.Count ||
                gridPointsCount != gridPointsOffsets.Count ||
                gridPointsCount != gridPointsNames.Count)
                throw new FileReadingException("The size of the gridPointsCount doesn't match the array length of gridPointsX, gridPointsY, gridPointsOffsets or gridPointsNames");

            ICollection<INetworkLocation> discretizationForThisBranch = new List<INetworkLocation>();

            // Restore Discretization from file
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
    }
}
