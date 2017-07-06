using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class BoundaryLocationFileWriter 
    {
        public static void WriteFile(string targetFile, WaterFlowModel1D waterFlowModel1D)
        {
            var boundLocNodes = waterFlowModel1D.BoundaryConditions.Where(bc => bc.DataType != WaterFlowModel1DBoundaryNodeDataType.None);
            WriteFileBoundaryLocations(targetFile, boundLocNodes, waterFlowModel1D.Network.Nodes);
        }

        public static void WriteFileBoundaryLocations(string targetFile, IEnumerable<WaterFlowModel1DBoundaryNodeData> boundaryNodes, IList<INode> nodes)
        {
            var categories = new List<DelftIniCategory>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.BoundaryLocationsMajorVersion,
                    GeneralRegion.BoundaryLocationsMinorVersion,
                    GeneralRegion.FileTypeName.BoundaryLocation),
            };

            categories.AddRange(
                boundaryNodes.Select(
                    boundaryNodeData =>
                        GenerateBoundaryLocationDefinition(boundaryNodeData, 
                            (int) WaterFlowModel1DHelper.GetBoundaryType(boundaryNodeData))));

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new IniFileWriter().WriteIniFile(categories, targetFile);
        }

        private static DelftIniCategory GenerateBoundaryLocationDefinition(WaterFlowModel1DBoundaryNodeData boundaryNodeData, int nodeType)
        {
            var definition = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            definition.AddProperty(BoundaryRegion.NodeId.Key, boundaryNodeData.Node.Name, BoundaryRegion.NodeId.Description);
            definition.AddProperty(BoundaryRegion.Type.Key, nodeType, BoundaryRegion.Type.Description);
            if (boundaryNodeData.SaltConditionType != SaltBoundaryConditionType.None)
            {
                definition.AddProperty(BoundaryRegion.ThatcherHarlemanCoeff.Key, boundaryNodeData.ThatcherHarlemannCoefficient, BoundaryRegion.ThatcherHarlemanCoeff.Description, BoundaryRegion.ThatcherHarlemanCoeff.Format);
            }
            return definition;
        }
        
    }
}