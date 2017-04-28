using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    internal static class BoundaryLocationFileWriterTestExtensions
    {
        internal static INode AddNode(this IHydroNetwork network)
        {
            string name = NetworkHelper.GetUniqueName("Node{0:D3}", network.Nodes, "Node");
            var newNode = network.NewNode();
            newNode.Name = name;
            network.Nodes.Add(newNode);

            return newNode;
        }
    }

    [TestFixture]
    public class BoundaryLocationFileWriterTest
    {
        [Test]
        public void TestBoundaryLocationsFileWriterGivesExpectedResults()
        {
            File.Delete(FileWriterTestHelper.ModelFileNames.BoundaryLocations);
            var boundLocNodes = new List<WaterFlowModel1DBoundaryNodeData>();
            IHydroNetwork network = new HydroNetwork();
            
            var nwNodes = new List<INode>() { network.AddNode(), network.AddNode(), network.AddNode(), network.AddNode(), network.AddNode() };
            boundLocNodes.Add(new WaterFlowModel1DBoundaryNodeData() { Feature = nwNodes[0], DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries });
            boundLocNodes.Add(new WaterFlowModel1DBoundaryNodeData() { Feature = nwNodes[1], DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries });
            boundLocNodes.Add(new WaterFlowModel1DBoundaryNodeData() { Feature = nwNodes[2], DataType = WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable });
            boundLocNodes.Add(new WaterFlowModel1DBoundaryNodeData() { Feature = nwNodes[3], DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant });
            boundLocNodes.Add(new WaterFlowModel1DBoundaryNodeData() { Feature = nwNodes[4], DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant });

            BoundaryLocationFileWriter.WriteFileBoundaryLocations(FileWriterTestHelper.ModelFileNames.BoundaryLocations, boundLocNodes, nwNodes);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.BoundaryLocations);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            var general = categories.Where(c => c.Name == GeneralRegion.IniHeader).ToList().First();
            var filetypeProperty = general.Properties.First(p => p.Name == GeneralRegion.FileType.Key);
            Assert.AreEqual(GeneralRegion.FileTypeName.BoundaryLocation, filetypeProperty.Value);
            
            Assert.AreEqual(5, categories.Count(b => b.Name == BoundaryRegion.BoundaryHeader));
            
            var content = categories.Where(c => c.Name == BoundaryRegion.BoundaryHeader).ToList();
            
            var nodeIdProperty = content.ElementAt(0).Properties.First(p => p.Name == BoundaryRegion.NodeId.Key);
            Assert.AreEqual("Node001", nodeIdProperty.Value);

            var typeProperty = content.ElementAt(0).Properties.First(p => p.Name == BoundaryRegion.Type.Key);
            Assert.AreEqual(((int)BoundaryType.Level).ToString(), typeProperty.Value);

            nodeIdProperty = content.ElementAt(1).Properties.First(p => p.Name == BoundaryRegion.NodeId.Key);
            Assert.AreEqual("Node002", nodeIdProperty.Value);

            typeProperty = content.ElementAt(1).Properties.First(p => p.Name == BoundaryRegion.Type.Key);
            Assert.AreEqual(((int)BoundaryType.Discharge).ToString(), typeProperty.Value);

            nodeIdProperty = content.ElementAt(2).Properties.First(p => p.Name == BoundaryRegion.NodeId.Key);
            Assert.AreEqual("Node003", nodeIdProperty.Value);

            typeProperty = content.ElementAt(2).Properties.First(p => p.Name == BoundaryRegion.Type.Key);
            Assert.AreEqual(((int)BoundaryType.Discharge).ToString(), typeProperty.Value);

            nodeIdProperty = content.ElementAt(3).Properties.First(p => p.Name == BoundaryRegion.NodeId.Key);
            Assert.AreEqual("Node004", nodeIdProperty.Value);

            typeProperty = content.ElementAt(3).Properties.First(p => p.Name == BoundaryRegion.Type.Key);
            Assert.AreEqual(((int)BoundaryType.Discharge).ToString(), typeProperty.Value);

            nodeIdProperty = content.ElementAt(4).Properties.First(p => p.Name == BoundaryRegion.NodeId.Key);
            Assert.AreEqual("Node005", nodeIdProperty.Value);

            typeProperty = content.ElementAt(4).Properties.First(p => p.Name == BoundaryRegion.Type.Key);
            Assert.AreEqual(((int)BoundaryType.Level).ToString(), typeProperty.Value);
        }
    }
}