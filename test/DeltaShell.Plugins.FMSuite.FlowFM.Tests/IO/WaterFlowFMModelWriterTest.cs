using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.NetworkEditor;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    public class WaterFlowFMModelWriterTest
    {
        private string tempDirectory;

        [SetUp]
        public void SetUp()
        {
            tempDirectory = FileUtils.CreateTempDirectory();
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(tempDirectory);
        }

        [Test]
        public void ModelWithPipe_Write_Files()
        {
            var mduPath = Path.Combine(tempDirectory, "MyModel.mdu");
            var model = new WaterFlowFMModel
            {
                MduFilePath = mduPath
            };

            var geometry1 = new LineString(new[]{ new Coordinate(0, 0), new Coordinate(0, 100) });
            SewerFactory.SetDefaultSettingPipeAndAddToNetwork(model.Network, new Pipe { Name = "pipe1", Geometry = geometry1 });

            //action write
            WaterFlowFMModelWriter.Write(model);

            var ugridPath = Path.Combine(tempDirectory, model.Name + "_net.nc");
            Assert.True(File.Exists(ugridPath)); //UGrid file

            var retrievedUgridData = RetrieveDataFromUGrid(ugridPath);
 
            Assert.That(retrievedUgridData.Branches.Count(), Is.EqualTo(1));
            Assert.That(retrievedUgridData.Branches.FirstOrDefault()?.Name, Is.EqualTo("pipe1"));
            Assert.That(retrievedUgridData.Nodes.Count(), Is.EqualTo(2));
            Assert.That(retrievedUgridData.DiscretizationPoints.Count(), Is.EqualTo(2));
        }

        private static RetrievedUgridData RetrieveDataFromUGrid(string path)
        {
            var disctretization =  UGridToNetworkAdapter.LoadNetworkAndDiscretisation(path);
            var result = new RetrievedUgridData
            {
                Branches = new List<IBranch>(disctretization.Network.Branches),
                Nodes = new List<INode>(disctretization.Network.Nodes),
                DiscretizationPoints = new List<INetworkLocation>(disctretization.Locations.Values)
            };

            return result;
        }

        private class RetrievedUgridData
        {
            public IEnumerable<IBranch> Branches { get; set; }
            public IEnumerable<INode> Nodes { get; set; }
            public IEnumerable<INetworkLocation> DiscretizationPoints { get; set; }
        }


    }
}
