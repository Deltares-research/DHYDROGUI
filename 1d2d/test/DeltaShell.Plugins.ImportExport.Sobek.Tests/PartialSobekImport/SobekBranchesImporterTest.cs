using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekBranchesImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportNetworkWithNodeWithEqualsSymbolInName()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network3\Network.TP";
            var branchesFromSobekImporter = new SobekBranchesImporter
                                                {
                                                    TargetObject = new HydroNetwork(), 
                                                    PathSobek = pathToSobekNetwork
                                                };

            branchesFromSobekImporter.Import();
            
            var network = (HydroNetwork) branchesFromSobekImporter.TargetObject;
            Assert.AreEqual(18, network.Nodes.Count);
            Assert.AreEqual(17, network.Branches.Count);
            Assert.AreEqual("42=theanswer", network.Nodes.ElementAt(3).Name);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportBranches()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";
            var branchesFromSobekImporter = new SobekBranchesImporter
                                                {
                                                    TargetObject = new HydroNetwork(), 
                                                    PathSobek = pathToSobekNetwork
                                                };

            branchesFromSobekImporter.Import();

            var network = (HydroNetwork) branchesFromSobekImporter.TargetObject;
            Assert.AreEqual(2, network.Nodes.Count);
            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(130, network.Branches[0].Geometry.Coordinates.Length);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportNodesWithStorageTables_Results_In_CompartmentsWithStorageTables()
        {
            var targetObject = new HydroNetwork();

            using (var tempDir = new TemporaryDirectory())
            {
                string pathToSobekNetwork = tempDir.CopyTestDataFileAndDirectoryToTempDirectory(@"network_storagetables\Network.TP");
                
                var branchesFromSobekImporter = new SobekBranchesImporter
                {
                    TargetObject = targetObject,
                    PathSobek = pathToSobekNetwork
                };

                try
                {
                    branchesFromSobekImporter.Import();
                }
                catch (ArgumentException e)
                {
                    Assert.Fail("Exception: " + e.Message);
                }
            }

            //NODE id '8' ty 1 ct sw PDIN 1 0 '' pdin
            //TBLE
            //    -0.4 100 <
            //    0.1 100 <
            //    0.11 0.64 <
            //    tble  ss 100 ml 0.65 node
            
            Compartment compartmentWithStorage = targetObject.Compartments.FirstOrDefault(c => c.Name.Equals("8"));
            Assert.NotNull(compartmentWithStorage, "Test compartment for checking not found");
            
            Assert.IsTrue(compartmentWithStorage.UseTable, "Use table for storage has not been set");
            
            //check content table
            IFunction table = compartmentWithStorage.Storage;
            Assert.IsNotNull(table, "No storage data (table) has been imported");
            Assert.AreEqual(1,table.Arguments.Count);
            Assert.AreEqual(1,table.Components.Count);
            IMultiDimensionalArray valuesHeight = table.Arguments[0].Values;
            IMultiDimensionalArray valuesStorage = table.Components[0].Values;
            Assert.AreEqual(-0.4,valuesHeight[0]);
            Assert.AreEqual(0.1,valuesHeight[1]);
            Assert.AreEqual(0.11,valuesHeight[2]);
            Assert.AreEqual(100,valuesStorage[0]);
            Assert.AreEqual(100,valuesStorage[1]);
            Assert.AreEqual(0.64,valuesStorage[2]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportBranchesWithNonUniqueNamesExpectUsefulErrorMessage()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network4\Network.TP";
            var branchesFromSobekImporter = new SobekBranchesImporter
            {
                TargetObject = new HydroNetwork(),
                PathSobek = pathToSobekNetwork
            };

            try
            {
                branchesFromSobekImporter.Import();
            }
            catch (ArgumentException e)
            {
                var messageBegin = "The following entries were not unique in";
                var messageEnd = $@"\network4\NETWORK.CP: {Environment.NewLine}13 at indices (12, 13)";
                Assert.IsTrue(e.Message.StartsWith(messageBegin), "begin");
                Assert.IsTrue(e.Message.EndsWith(messageEnd), "end");
                return;
            }
            Assert.Fail("Expected exception");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportBranchesToExistingNetwork()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";
            var branchesFromSobekImporter = new SobekBranchesImporter();
            var hydroNetwork = PartialSobekImporterTestHelper.GetTestNetwork();
            var branchName = hydroNetwork.Branches.First().Name;

            branchesFromSobekImporter.TargetObject = hydroNetwork;
            branchesFromSobekImporter.PathSobek = pathToSobekNetwork;
            branchesFromSobekImporter.Import();

            var network = (HydroNetwork) branchesFromSobekImporter.TargetObject;
            Assert.AreEqual(4, network.Nodes.Count);
            Assert.AreEqual(2, network.Branches.Count);
            Assert.IsNotNull(network.Branches.FirstOrDefault(b => b.Name == branchName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateExistingBranch()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";
            var branchesFromSobekImporter = new SobekBranchesImporter
                                                {
                                                    TargetObject = new HydroNetwork(), 
                                                    PathSobek = pathToSobekNetwork
                                                };

            branchesFromSobekImporter.Import();

            var network = (HydroNetwork) branchesFromSobekImporter.TargetObject;

            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(130, network.Branches[0].Geometry.Coordinates.Length);
            Assert.IsFalse(network.Nodes.First().IsConnectedToMultipleBranches);
            Assert.IsFalse(network.Nodes.Last().IsConnectedToMultipleBranches);

            network.Branches[0].Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(0, 100)});
            Assert.AreEqual(2, network.Branches[0].Geometry.Coordinates.Length);

            branchesFromSobekImporter.Import();

            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(130, network.Branches[0].Geometry.Coordinates.Length);
            Assert.IsFalse(network.Nodes.First().IsConnectedToMultipleBranches);
            Assert.IsFalse(network.Nodes.Last().IsConnectedToMultipleBranches);
        }

        [Test]
        [Category(TestCategory.DataAccess)] // Nodes.dat 2124 - 213
        public void ImportBranchesWithInterpolationOverNodes()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\39_000.lit\Network.TP";
            var branchesFromSobekImporter = new SobekBranchesImporter
                                                {
                                                    TargetObject = new HydroNetwork(), 
                                                    PathSobek = pathToSobekNetwork
                                                };

            branchesFromSobekImporter.Import();

            var network = (HydroNetwork) branchesFromSobekImporter.TargetObject;
            Assert.AreEqual(13, network.Nodes.Count);
            Assert.AreEqual(8, network.Branches.Count);
            Assert.AreEqual(1, network.Branches.First(b => b.Name == "1").OrderNumber);
            Assert.AreEqual(1, network.Branches.First(b => b.Name == "2").OrderNumber);
            Assert.AreEqual(2, network.Branches.First(b => b.Name == "5").OrderNumber);
            Assert.AreEqual(2, network.Branches.First(b => b.Name == "7").OrderNumber);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportPipesFromSobekUrbanModel()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Groesbeek.lit\Network.TP";
            var branchesFromSobekImporter = new SobekBranchesImporter
            {
                TargetObject = new HydroNetwork(),
                PathSobek = pathToSobekNetwork
            };

            branchesFromSobekImporter.Import();

            var network = (HydroNetwork)branchesFromSobekImporter.TargetObject;
            Assert.AreEqual(868, network.Nodes.Count);
            Assert.AreEqual(914, network.Branches.Count);
            Assert.AreEqual(868, network.Manholes.Count());
            Assert.AreEqual(914, network.SewerConnections.Count());
            Assert.AreEqual(892, network.Pipes.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportExternalStructures_Result_Manhole_Compartment_Outlet_InnerConnectionFor_ExternalWeir()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\waard08.lit\network.tp";
            var branchesFromSobekImporter = new SobekBranchesImporter
            {
                TargetObject = new HydroNetwork(),
                PathSobek = pathToSobekNetwork
            };

            branchesFromSobekImporter.Import();

            var network = (HydroNetwork)branchesFromSobekImporter.TargetObject;
            var manhole13_680O = network.Manholes.FirstOrDefault(m => m.Name == "13-680O");

            Assert.IsNotNull(manhole13_680O);
            Assert.AreEqual(2,manhole13_680O.Compartments.Count);
            Assert.IsNotNull(manhole13_680O.Compartments.FirstOrDefault(c => c.Name == "13-680O"));
            Assert.IsNotNull(manhole13_680O.Compartments.FirstOrDefault(c => c.Name == "tmp13-680O"));
            Assert.IsTrue(manhole13_680O.Compartments.Any(c => c is OutletCompartment));
            Assert.AreEqual(1, manhole13_680O.InternalConnections().Count());
            Assert.IsNotNull(manhole13_680O.InternalConnections().FirstOrDefault(ic => ic.Name == "tmp13-680O"));
        }
    }
}
