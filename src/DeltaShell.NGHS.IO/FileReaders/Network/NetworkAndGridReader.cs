using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.NGHS.IO.FileReaders.Network
{
    public static class NetworkAndGridReader
    {
        public static void ReadFile(string filename, IHydroNetwork network, IDiscretization discretization)
        {
            if (!File.Exists(filename)) throw new FileReadingException(String.Format("Could not read file {0} properly, it doesn't exist.", filename));
            var categories = new DelftIniReader().ReadDelftIniFile(filename);
            if (categories.Count == 0) throw new FileReadingException(String.Format("Could not read file {0} properly, it seems empty", filename));
            
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();  

            IList<IHydroNode> nodes = new List<IHydroNode>();
            foreach (var nodeCategory in categories.Where(category => category.Name == NetworkDefinitionRegion.IniNodeHeader))
            {
                try
                {
                    var readNode = ReadNodeDefinition(nodeCategory);
                    if (nodes.Contains(readNode) || nodes.FirstOrDefault(n => n.Name == readNode.Name) != null)
                        throw new FileReadingException(string.Format("Node with id {0} already exists, there cannot be any duplicate Node ids", readNode.Name));
                    nodes.Add(readNode);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read node", fileReadingException));
                }
            }

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading nodes an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }
            network.Nodes.AddRange(nodes);

            IList<IChannel> branches = new List<IChannel>();
            foreach (var branchCategory in categories.Where(category => category.Name == NetworkDefinitionRegion.IniBranchHeader))
            {
                try
                {
                    var readBranch = ReadBranchDefinition(branchCategory, network);
                    if (branches.Contains(readBranch) || branches.FirstOrDefault(b => b.Name == readBranch.Name) != null)
                        throw new FileReadingException(string.Format("branch with id {0} is already read, id's CAN NOT be duplicates!", readBranch.Name));
                    branches.Add(readBranch);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read branch", fileReadingException));
                }
            }
            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading branches an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }

            network.Branches.AddRange(branches);

            IList<INetworkLocation> discretizations = new List<INetworkLocation>();
            foreach (var branchCategory in categories.Where(category => category.Name == NetworkDefinitionRegion.IniBranchHeader))
            {
                try
                {
                    var readBranchDiscretization = ReadDiscretizationDefinition(branchCategory, network);
                    if (readBranchDiscretization != null)
                        discretizations.AddRange(readBranchDiscretization);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read branch discretization", fileReadingException));
                }
            }
            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading discretizations for the branches an error occured : {0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }
            discretization.Locations.Values.AddRange(discretizations);
        }

        private static ICollection<INetworkLocation> ReadDiscretizationDefinition(DelftIniCategory branchCategory, IHydroNetwork network)
        {
            // Find branch in network
            var idValue = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Id.Key);
            var branch = network.Branches.FirstOrDefault(b => b.Name == idValue);
            if (branch == null) throw new FileReadingException(string.Format("Branch with id : {0} not found in network, could not add a discretization (if available) to a branch which is not in the network...", idValue));
            
            // Optional Discretization Properties
            var gridPointsCount = branchCategory.ReadProperty<int>(NetworkDefinitionRegion.GridPointsCount.Key, true);
            var gridPointsX = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointX.Key, true);
            var gridPointsY = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointY.Key, true);
            var gridPointsOffsets = branchCategory.ReadPropertiesToListOfType<double>(NetworkDefinitionRegion.GridPointOffsets.Key, true);
            var gridPointsNames = branchCategory.ReadPropertiesToListOfType<string>(NetworkDefinitionRegion.GridPointNames.Key, true, ';');

            if (gridPointsCount == 0 )
                return null;
            if (gridPointsCount != gridPointsX.Count  ||
                gridPointsCount != gridPointsY.Count ||
                gridPointsCount != gridPointsOffsets.Count ||
                gridPointsCount != gridPointsNames.Count)
                throw new FileReadingException("The size of the gridPointsCount doesn't match the array length of gridPointsX, gridPointsY, gridPointsOffsets or gridPointsNames");
            
            ICollection<INetworkLocation> discretizationForThisBranch = new List<INetworkLocation>();

            // Restore Discretization from file
            for (var i = 0; i < gridPointsCount; i++)
            {
                discretizationForThisBranch.Add(new NetworkLocation()
                {
                    Branch = branch,
                    Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(gridPointsOffsets[i]),
                    Geometry = new Point(gridPointsX[i], gridPointsY[i]),
                    Name = gridPointsNames[i]
                });
            }
            return discretizationForThisBranch;
        }

        private static IHydroNode ReadNodeDefinition(IDelftIniCategory nodeCategory)
        {
            // Essential Node Properties
            var idProperty = nodeCategory.ReadProperty<string>(NetworkDefinitionRegion.Id.Key);
            var xCoordinate = nodeCategory.ReadProperty<double>(NetworkDefinitionRegion.X.Key);
            var yCoordinate = nodeCategory.ReadProperty<double>(NetworkDefinitionRegion.Y.Key);
            
            // Optional Node Properties - don't throw exception if not available
            var nameProperty = nodeCategory.ReadProperty<string>(NetworkDefinitionRegion.Name.Key, true) ?? string.Empty;
            
            // Restore Node from file
            return new HydroNode
            {
                Name = idProperty,
                LongName = nameProperty,
                Geometry = new Point(xCoordinate, yCoordinate)
            };
        }

        private static IChannel ReadBranchDefinition(IDelftIniCategory branchCategory, IHydroNetwork network)
        {
            // Essential Branch Properties
            var idValue = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Id.Key);
            var fromNodeValue = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.FromNode.Key);
            var toNodeValue = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.ToNode.Key);
            var branchOrderValue = branchCategory.ReadProperty<int>(NetworkDefinitionRegion.BranchOrder.Key);

            INode fromNode = null;
            if (!string.IsNullOrEmpty(fromNodeValue))
            {
                fromNode = network.Nodes.FirstOrDefault(n => n.Name == fromNodeValue);
                if (fromNode == null)
                {
                    throw new FileReadingException(
                        string.Format("Unable to parse Branch property: {0}, Node not found in Network.{1}",
                            NetworkDefinitionRegion.FromNode.Key, Environment.NewLine));
                }
            }

            INode toNode = null;
            if (!string.IsNullOrEmpty(toNodeValue))
            {
                toNode = network.Nodes.FirstOrDefault(n => n.Name == toNodeValue);
                if (toNode == null)
                {
                    throw new FileReadingException(
                        string.Format("Unable to parse Branch property: {0}, Node not found in Network.{1}",
                            NetworkDefinitionRegion.FromNode.Key, Environment.NewLine));
                }
            }
            var geometryString = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Geometry.Key);


            // Optional Branch Properties
            var name = branchCategory.ReadProperty<string>(NetworkDefinitionRegion.Name.Key, true) ?? string.Empty;

            return new Channel
            {
                Name = idValue,
                Geometry = GeometryFromWKT.Parse(geometryString),
                LongName = name,
                Source = fromNode,
                Target = toNode,
                OrderNumber = branchOrderValue
            };
        }

    }
}
