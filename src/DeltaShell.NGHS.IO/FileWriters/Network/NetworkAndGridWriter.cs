using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public class NetworkAndGridWriter
    {
        public static void WriteFile(string targetFile, IHydroNetwork hydroNetwork, IDiscretization discretization)
        {
            var categories = new List<DelftIniCategory>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.NetworkDefinitionsMajorVersion,
                    GeneralRegion.NetworkDefinitionsMinorVersion, 
                    GeneralRegion.FileTypeName.NetworkDefinition)
            };

            categories.AddRange(hydroNetwork.HydroNodes.Select(GenerateNodeDefinition));

            categories.AddRange(hydroNetwork.Branches.Select(branch => GenerateBranchDefinitions(discretization, branch)));
            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }

        private static DelftIniCategory GenerateNodeDefinition(IHydroNode hydroNode)
        {
            var nodeDefinition = new DelftIniCategory(NetworkDefinitionRegion.IniNodeHeader);
            nodeDefinition.AddProperty(NetworkDefinitionRegion.Id.Key, hydroNode.Name);
            nodeDefinition.AddProperty(NetworkDefinitionRegion.Name.Key, hydroNode.LongName);
            
            if (hydroNode.Geometry == null) return nodeDefinition;

            nodeDefinition.AddProperty(NetworkDefinitionRegion.X.Key, hydroNode.Geometry.Coordinate.X, null, NetworkDefinitionRegion.X.Format);
            nodeDefinition.AddProperty(NetworkDefinitionRegion.Y.Key, hydroNode.Geometry.Coordinate.Y, null, NetworkDefinitionRegion.Y.Format);
            
            return nodeDefinition;
        }

        private static DelftIniCategory GenerateBranchDefinitions(IDiscretization discretization, IBranch branch)
        {
            // id and name
            var branchDefinition = new DelftIniCategory(NetworkDefinitionRegion.IniBranchHeader);
            branchDefinition.AddProperty(NetworkDefinitionRegion.Id.Key, branch.Name);
            var channel = branch as IChannel;
            if (channel != null)
            {
                branchDefinition.AddProperty(NetworkDefinitionRegion.Name.Key, channel.LongName);
            }

            // from and to node
            branchDefinition.AddProperty(NetworkDefinitionRegion.FromNode.Key, branch.Source.Name);
            branchDefinition.AddProperty(NetworkDefinitionRegion.ToNode.Key, branch.Target.Name);

            // order
            branchDefinition.AddProperty(NetworkDefinitionRegion.BranchOrder.Key, branch.OrderNumber);

            branchDefinition.AddProperty(NetworkDefinitionRegion.Geometry.Key, branch.Geometry.AsText());

            // grid points
            var discretizationPoints = discretization.Locations.Values.Where(l => l.Branch == branch).OrderBy(l => l.Chainage);

            var gridPointOffsets = discretizationPoints.Select(gridPoint => gridPoint.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(gridPoint.Chainage)).ToList();
            branchDefinition.AddProperty(NetworkDefinitionRegion.GridPointsCount.Key, gridPointOffsets.Count);

            var gridPointXCoordinates = discretizationPoints.Select(gridPoint => gridPoint.Geometry.Coordinates[0].X);
            branchDefinition.AddProperty(NetworkDefinitionRegion.GridPointX.Key, gridPointXCoordinates, null, NetworkDefinitionRegion.GridPointX.Format);

            var gridPointYCoordinates = discretizationPoints.Select(gridPoint => gridPoint.Geometry.Coordinates[0].Y);
            branchDefinition.AddProperty(NetworkDefinitionRegion.GridPointY.Key, gridPointYCoordinates, null, NetworkDefinitionRegion.GridPointY.Format);

            branchDefinition.AddProperty(NetworkDefinitionRegion.GridPointOffsets.Key, gridPointOffsets, null, NetworkDefinitionRegion.GridPointOffsets.Format);

            var gridPointNames = discretizationPoints.Select(gridPoint => gridPoint.Name);
            branchDefinition.AddProperty(NetworkDefinitionRegion.GridPointNames.Key, string.Join(";", gridPointNames));

            // branch definition completed
            return branchDefinition;
        }
    }
}