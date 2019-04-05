using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.NetCdf;
using GeoAPI.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class SobekToFMExporter : IFileExporter
    {
        public string Name
        {
            get { return "Sobek to FM (network)"; }
        }

        public string Category { get { return "General"; } }
        public string Description
        {
            get { return string.Empty; }
        }

        public bool Export(object item, string path)
        {
            var flow1DModel = (WaterFlowModel1D) item;

            ExportGrid(path, flow1DModel);

            ExportCrossSections(path, flow1DModel);

            return true;
        }

        private static void ExportGrid(string path, WaterFlowModel1D flow1DModel)
        {
            var file = NetCdfFile.CreateNew(path);

            try
            {
                var computationalGrid = flow1DModel.NetworkDiscretization;

                NetworkCoverageHelper.UpdateSegments(computationalGrid);

                var bedLevel = BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(flow1DModel.Network);

                var numLinks = computationalGrid.Segments.Values.Count;

                var nodes = new Dictionary<Coordinate, int>();
                var linkArray = new int[numLinks,2];
                var linkTypeArray = new int[numLinks];

                for (var i = 0; i < numLinks; i++)
                {
                    var link = computationalGrid.Segments.Values[i];

                    var fromNode = computationalGrid.Locations.Values.First(
                        loc => Equals(loc.Branch, link.Branch) && Math.Abs(loc.Chainage - link.Chainage) < 0.00001);

                    int fromIndex;
                    if (!nodes.TryGetValue(fromNode.Geometry.Coordinate, out fromIndex))
                    {
                        fromIndex = nodes.Count;
                        nodes.Add(fromNode.Geometry.Coordinate, fromIndex);
                    }

                    var toNode = computationalGrid.Locations.Values.First(
                        loc => Equals(loc.Branch, link.Branch) && Math.Abs(loc.Chainage - link.EndChainage) < 0.00001);

                    int toIndex;
                    if (!nodes.TryGetValue(toNode.Geometry.Coordinate, out toIndex))
                    {
                        toIndex = nodes.Count;
                        nodes.Add(toNode.Geometry.Coordinate, toIndex);
                    }

                    linkArray[i, 0] = fromIndex + 1;
                    linkArray[i, 1] = toIndex + 1;
                    linkTypeArray[i] = 1;
                }

                var numNodes = nodes.Keys.Count;

                var nodesX = new double[numNodes];
                var nodesY = new double[numNodes];
                var nodesZ = new double[numNodes];

                var nodeIndex = 0;
                foreach (var node in nodes.Keys)
                {
                    nodesX[nodeIndex] = node.X;
                    nodesY[nodeIndex] = node.Y;
                    nodesZ[nodeIndex] = (double) bedLevel.Evaluate(node);
                    nodeIndex++;
                }

                // define dimensions
                var nodeDim = file.AddDimension("nNetNode", numNodes);
                var linkDim = file.AddDimension("nNetLink", numLinks);
                var linkPtsDim = file.AddDimension("nNetPoints", 2);

                // create node variables
                var nodeXVar = file.AddVariable("NetNode_x", typeof (double), new[] {nodeDim});
                var nodeYVar = file.AddVariable("NetNode_y", typeof (double), new[] {nodeDim});
                var nodeZVar = file.AddVariable("NetNode_z", typeof (double), new[] {nodeDim});

                // create link variables
                var linkVar = file.AddVariable("NetLink", typeof (int), new[] {linkDim, linkPtsDim});
                var linkTypeVar = file.AddVariable("NetLinkType", typeof (int), new[] {linkDim});

                // write all values
                file.Create();
                file.Write(nodeXVar, nodesX);
                file.Write(nodeYVar, nodesY);
                file.Write(nodeZVar, nodesZ);
                file.Write(linkVar, new[] {0, 0}, new[] {numLinks, 2}, NetCdfFileHelper.FlattenArray<int>(linkArray));
                file.Write(linkTypeVar, linkTypeArray);
            }
            finally
            {
                file.Close();
            }
        }

        private static void ExportCrossSections(string path, WaterFlowModel1D flow1DModel)
        {
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileNameWithoutExtension(path);

            var csDefFilePath = Path.Combine(directory, fileName + "_profdef.txt");
            var csLocFilePath = Path.Combine(directory, fileName + "_profloc.xyz");

            using (CultureUtils.SwitchToInvariantCulture())
            using (var csDefFile = File.Create(csDefFilePath))
            using (var csLocFile = File.Create(csLocFilePath))
            using (var csDefWriter = new StreamWriter(csDefFile))
            using (var csLocWriter = new StreamWriter(csLocFile))
            {
                var network = flow1DModel.Network;
                var allDefinitions = network.CrossSections.Select(GetActualDefinition).Distinct(new DefinitionComparer()).ToList();
                
                var id = 1;
                foreach (var def in allDefinitions)
                {
                    var width = def.Width;
                    csDefWriter.WriteLine("PROFNR={0}     TYPE=3             WIDTH={1}", id, width);
                    id++;
                }

                foreach (var cs in network.CrossSections)
                {
                    var lengthIndexedLine = new LengthIndexedLine(cs.Branch.Geometry);
                    var csCoordinate = lengthIndexedLine.ExtractPoint(cs.Chainage);

                    var x = csCoordinate.X;
                    var y = csCoordinate.Y;

                    var defIndex = allDefinitions.FindIndex(def => DefinitionComparer.Equal(def, GetActualDefinition(cs)));
                    csLocWriter.WriteLine("{0} {1} {2}", x, y, defIndex+1);
                }
            }
        }

        private class DefinitionComparer : IEqualityComparer<ICrossSectionDefinition>
        {
            public static bool Equal(ICrossSectionDefinition x, ICrossSectionDefinition y)
            {
                return new DefinitionComparer().Equals(x, y);
            }

            public bool Equals(ICrossSectionDefinition x, ICrossSectionDefinition y)
            {
                // per request Herman, compare on Width for now:
                return Math.Abs(x.Width - y.Width) < 0.001;
                // future code:
                //return x.Equals(y);
            }

            public int GetHashCode(ICrossSectionDefinition obj)
            {
                return obj.Width.GetHashCode();
            }
        }

        private static ICrossSectionDefinition GetActualDefinition(ICrossSection crossSection)
        {
            var def = crossSection.Definition;
            return def.IsProxy ? ((CrossSectionDefinitionProxy) def).InnerDefinition : def;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (WaterFlowModel1D);
        }

        public string FileFilter
        {
            get { return "FM grid file (*_net.nc)|*_net.nc"; }
        }

        public Bitmap Icon { get{ return Properties.Resources.unstruc; } }
        public bool CanExportFor(object item)
        {
            var model = item as WaterFlowModel1D;
            return model != null && model.NetworkDiscretization != null && model.NetworkDiscretization.GetValues().Count > 0;
        }
    }
}