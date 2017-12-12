using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class GwswFeatureImporterTest: GwswImporterTestHelper
    {
        #region Gwsw Import tests

        [TestCase(@"gwswFiles\BOP.csv")]
        [TestCase(@"gwswFiles\Debiet.csv")]
        [TestCase(@"gwswFiles\GroeneDaken.csv")]
        [TestCase(@"gwswFiles\ItObject.csv")]
        [TestCase(@"gwswFiles\Knooppunt.csv")]
        [TestCase(@"gwswFiles\Kunstwerk.csv")]
        [TestCase(@"gwswFiles\Meta.csv")]
        [TestCase(@"gwswFiles\Nwrw.csv")]
        [TestCase(@"gwswFiles\Oppervlak.csv")]
        [TestCase(@"gwswFiles\Profiel.csv")]
        [TestCase(@"gwswFiles\Verbinding.csv")]
        [TestCase(@"gwswFiles\Verloop.csv")]
        public void ImportGwswCsvFileWithLoadedGwswDefinition(string testCasePath)
        {
            var filePath = GetFileAndCreateLocalCopy(testCasePath);
            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();
            gwswImporter.ImportGwswDefinitionFile(gwswImporter.definitionFilePath);

            var elementList = GwswFileImportAsGwswElementsWorksCorrectly(gwswImporter, filePath);

            var numberOfLines = File.ReadAllLines(filePath).Length - 1; // we should not include the header
            Assert.AreEqual(numberOfLines, elementList.Count,
                string.Format("There is a mismatch between expected number of elements and imported."));
            var elementTypeFound = gwswImporter.GwswAttributesDefinition.FirstOrDefault( at => at.FileName.Equals(Path.GetFileName(testCasePath)) );
            if (elementTypeFound == null)
            {
                Assert.Fail("Test failed because no element name was found mapped to this file name.");
            }

            if (numberOfLines != 0)
            {
                var numberOfColumns = File.ReadLines(filePath).First().Split(csvSemiColonDelimeter)
                    .Where(s => !s.Equals(string.Empty)).ToList().Count;
                foreach (var element in elementList)
                {
                    Assert.AreEqual(elementTypeFound.ElementName, element.ElementTypeName);
                    Assert.AreEqual(numberOfColumns, element.GwswAttributeList.Count,
                        string.Format("There is a mismatch between expected and imported attributes for element {0}",
                            element.ElementTypeName));
                }
            }
        }


        [Test]
        public void TestImportSewerConnectionsFromGwswWithMappingSucceeds()
        {
            //Load Sewer Connections.
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var model = new WaterFlowFMModel();
            var network = model.Network;
            Assert.IsFalse(network.Pipes.Any());

            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();
            var importedItems = gwswImporter.ImportItem(filePath, model);
            Assert.IsNotNull(importedItems);
            Assert.IsTrue(network.SewerConnections.Any());

            //There should be some pipes.
            Assert.IsTrue(network.Pipes.Any());

            var importedSewerItems = importedItems as List<INetworkFeature>;
            Assert.IsNotNull(importedSewerItems);
            importedSewerItems.ToList().ForEach(sc => Assert.IsNotNull(sc as SewerConnection));

            //Check imported list has been added to the network pipes.
            Assert.AreEqual(importedSewerItems, network.SewerConnections.ToList());
        }

        [Test]
        public void TestImportSewerConnectionFromFileAssignsNodesWhenTheyExist()
        {
            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;

            /*We know these two nodes are referred in the test data*/
            var startNodeName = "man001";
            var startCompartmentName = "put9";
            var startNode = new Manhole(startNodeName);
            var startCompartment = new Compartment(startCompartmentName);
            startNode.Compartments.Add(startCompartment);
            network.Nodes.Add(startNode);

            var endNodeName = "man001";
            var endCompartmentName = "put8";
            var endNode = new Manhole(endNodeName);
            var endCompartment = new Compartment(endCompartmentName);
            endNode.Compartments.Add(endCompartment);
            network.Nodes.Add(endNode);

            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();
            //Load pipes.
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(filePath, model);
            Assert.IsNotNull(importedConnections);

            Assert.IsTrue(network.SewerConnections.Any());
            Assert.IsFalse(network.SewerConnections.Any(p => p.Source == null), "Source node has not been created during import process.");
            Assert.IsFalse(network.SewerConnections.Any(p => p.Target == null), "Target node has not been created during import process.");

            Assert.IsTrue(network.SewerConnections.Any(p => p.Source.Equals(startNode) && p.Target.Equals(endNode)));
        }

        [Test]
        public void TestImportPipesFromFileCreatesNodesWhenTheyDoNotExist()
        {
            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;

            /*We know these two nodes are referred in the test data*/
            var expectedStartNodeName = "put9";
            var expectedEndNodeName = "put8";

            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();
            //Load pipes.
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedPipes = gwswImporter.ImportItem(filePath, model);
            Assert.IsNotNull(importedPipes);

            Assert.IsTrue(network.Pipes.Any());
            Assert.IsFalse(network.Pipes.Any(p => p.Source == null), "Source node has not been created during import process.");
            Assert.IsFalse(network.Pipes.Any(p => p.Target == null), "Target node has not been created during import process.");
            Assert.IsTrue(network.Pipes.Any(p => p.SourceCompartment.Name.Equals(expectedStartNodeName) && p.TargetCompartment.Name.Equals(expectedEndNodeName)));

            //Checking manhole name is stored as id
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartmentWithName(expectedStartNodeName)));
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartmentWithName(expectedEndNodeName)));
        }

        [Test]
        public void TestImportStructuresThenImportSewerConnectionsAssignsStructuresValues()
        {
            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;
            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();

            //Load structures.
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            var importedStructures = gwswImporter.ImportItem(structuresPath, model);
            Assert.IsNotNull(importedStructures);

            //Check placeholders have been created.
            Assert.IsTrue(network.SewerConnections.Any());
            Assert.IsTrue(network.Structures.Any());

            var structuresPh = network.Structures.Where(s => !(s is CompositeBranchStructure)).ToList();

            // Now Load connections.
            var connectionsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(connectionsPath, model);
            Assert.IsNotNull(importedConnections);

            Assert.AreEqual(structuresPh.Count, network.Structures.Count(s => !(s is CompositeBranchStructure)));
            foreach (var structure in structuresPh)
            {
                var replacedStructure = network.Structures.FirstOrDefault(s => s.Name.Equals(structure.Name));
                Assert.AreEqual(structure, replacedStructure, "the attributes from the element do not match");
            }

        }

        [Test]
        public void TestImportOutletsFromStructuresThenImportNodesAssignsStructuresValues()
        {
            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;

            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();
            //Load structures.
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            var importedStructures = gwswImporter.ImportItem(structuresPath, model);
            Assert.IsNotNull(importedStructures);

            //Check placeholders have been created.
            Assert.IsTrue(network.Manholes.Any());

            var outletCompartments = network.Manholes.SelectMany(mh => mh.Compartments.Where(cmp => cmp is OutletCompartment)).ToList();
            Assert.IsTrue(outletCompartments.Any());

            // Now Load connections.
            var compartmentsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var importedCompartments = gwswImporter.ImportItem(compartmentsPath, model);
            Assert.IsNotNull(importedCompartments);

            foreach (var compartment in outletCompartments)
            {
                var outlet = compartment as OutletCompartment;
                Assert.IsNotNull(outlet);

                var manhole = network.Manholes.FirstOrDefault(m => m.ContainsCompartmentWithName(outlet.Name));
                Assert.IsNotNull(manhole);
                var extendedOutlet = manhole.GetCompartmentByName(outlet.Name) as OutletCompartment;
                Assert.IsNotNull(extendedOutlet);

                Assert.AreEqual(outlet.SurfaceWaterLevel, extendedOutlet.SurfaceWaterLevel, "the attributes from the element do not match");
            }
        }

        [Test]
        public void WhenImportingSewerProfilesToNetworkAndThenImportingSewerConnectionsToNetwork_ThenSewerConnectionsHaveSewerProfiles()
        {
            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;

            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();
            //Load sewer profiles
            var sewerProfilesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");
            var importedProfiles = gwswImporter.ImportItem(sewerProfilesPath, model) as List<INetworkFeature>;
            Assert.IsEmpty(importedProfiles);

            //Check that sewer profiles have been put into the network
            var numberOfLinesInFile = File.ReadAllLines(sewerProfilesPath).Length - 1;
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(numberOfLinesInFile));

            // Now Load connections.
            var connectionsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(connectionsPath, model) as List<INetworkFeature>;
            Assert.IsNotNull(importedConnections);

            var pipes = network.Branches.OfType<Pipe>().ToList();
            Assert.IsTrue(pipes.Any());
            pipes.ForEach(p => Assert.NotNull(p.SewerProfileDefinition));

            // Check for each pipe that its SewerProfileDefinition is equal to one of the sewer profiles in
            // the SharedCrossSectionDefinitions of the network
            pipes.ForEach(p =>
            {
                var pipeCsDefinition = p.SewerProfileDefinition;
                var sharedCsDefinition = network.SharedCrossSectionDefinitions.FirstOrDefault(csd => csd.Name == pipeCsDefinition.Name);
                Assert.NotNull(sharedCsDefinition);
                Assert.That(pipeCsDefinition.Width, Is.EqualTo(sharedCsDefinition.Width));

                var pipeShape = pipeCsDefinition.Shape;
                var sharedCsShape = ((CrossSectionDefinitionStandard)sharedCsDefinition).Shape;
                Assert.That(pipeShape.Type, Is.EqualTo(sharedCsShape.Type));

                var pipeWidthHeightShape = pipeShape as CrossSectionStandardShapeWidthHeightBase;
                var sharedWidthHeightShape = sharedCsShape as CrossSectionStandardShapeWidthHeightBase;
                if (pipeWidthHeightShape != null && sharedWidthHeightShape != null)
                {
                    Assert.That(pipeWidthHeightShape.Height, Is.EqualTo(sharedWidthHeightShape.Height));
                }
            });
        }

        [Test]
        public void WhenImportingSewerConnectionsToNetworkAndThenImportingSewerProfilesToNetwork_ThenSewerConnectionsHaveTheCorrectSewerProfiles()
        {
            const string csdName = "PRO2";
            const string csdNameForAddedProfile = "PRO6";

            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;

            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();
            //Load connections
            var connectionsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(connectionsPath, model) as List<INetworkFeature>;
            Assert.IsNotNull(importedConnections);

            // Retrieve one profile to later compare to the same one after loading the sewer profiles
            var csDefinitionBefore = (CrossSectionDefinitionStandard)network.SharedCrossSectionDefinitions.FirstOrDefault(crossSectionDefinition => crossSectionDefinition.Name == csdName);
            Assert.NotNull(csDefinitionBefore);
            var csShapeBefore = (CrossSectionStandardShapeWidthHeightBase)csDefinitionBefore.Shape;
            Assert.NotNull(csShapeBefore);
            var amountOfProfilesBefore = network.SharedCrossSectionDefinitions.Count;
            Assert.That(network.SharedCrossSectionDefinitions.Count(cs => cs.Name == csdNameForAddedProfile), Is.EqualTo(0));

            // Check the sewer profiles in the network
            var sewerProfileShapeBefore = (CrossSectionStandardShapeWidthHeightBase)network.Pipes.Select(p => p.SewerProfileDefinition).FirstOrDefault(d => d.Name == csdName)?.Shape;
            Assert.NotNull(sewerProfileShapeBefore);

            // Now Load sewer profiles
            var sewerProfilesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");
            var importedProfiles = gwswImporter.ImportItem(sewerProfilesPath, model) as List<INetworkFeature>;
            Assert.IsEmpty(importedProfiles);

            //Check that sewer profiles have been put into the network
            var numberOfLinesInFile = File.ReadAllLines(sewerProfilesPath).Length - 1;
            var networkCsDefinitions = network.SharedCrossSectionDefinitions;
            Assert.That(networkCsDefinitions.Count, Is.EqualTo(numberOfLinesInFile));

            //Get the same profile as before loading the profiles
            var sharedCsDefinitions = network.SharedCrossSectionDefinitions;
            var testProfileAfter = sharedCsDefinitions.FirstOrDefault(crossSectionDefinition => crossSectionDefinition.Name == csdName);
            Assert.NotNull(testProfileAfter);
            var csShapeAfter = (CrossSectionStandardShapeWidthHeightBase)((CrossSectionDefinitionStandard)testProfileAfter).Shape;
            Assert.NotNull(csShapeAfter);
            Assert.That(sharedCsDefinitions.Count >= amountOfProfilesBefore);
            Assert.That(sharedCsDefinitions.Count(cs => cs.Name == csdNameForAddedProfile), Is.EqualTo(1));

            // Check the sewer profiles in the network
            var sewerProfileShapeAfter = (CrossSectionStandardShapeWidthHeightBase)network.Pipes.Select(p => p.SewerProfileDefinition).FirstOrDefault(d => d.Name == csdName)?.Shape;
            Assert.NotNull(sewerProfileShapeAfter);

            // Compare properties of shapes found in SharedCrossSectionDefinitions
            Assert.AreNotEqual(csShapeAfter.Width, csShapeBefore.Width);
            Assert.AreNotEqual(csShapeAfter.Height, csShapeBefore.Height);
            Assert.AreNotEqual(csShapeAfter.Type, csShapeBefore.Type);

            // Compare properties of shapes found in SharedCrossSectionDefinitions
            Assert.AreNotEqual(sewerProfileShapeAfter.Width, sewerProfileShapeBefore.Width);
            Assert.AreNotEqual(sewerProfileShapeAfter.Height, sewerProfileShapeBefore.Height);
            Assert.AreNotEqual(sewerProfileShapeAfter.Type, sewerProfileShapeBefore.Type);
        }

        [Test]
        public void TestImportOrificesFromStructuresThenImportOrificesAssignsStructuresValues()
        {
            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;
            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();

            //Load structures.
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            var importedStructures = gwswImporter.ImportItem(structuresPath, model);
            Assert.IsNotNull(importedStructures);

            //Check placeholders have been created.
            Assert.IsTrue(network.Branches.Any());

            var orificeStructures = network.Branches.OfType<SewerConnectionOrifice>().ToList();
            Assert.IsTrue(orificeStructures.Any());

            // Now Load connections.
            var compartmentsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(compartmentsPath, model);
            Assert.IsNotNull(importedConnections);

            foreach (var orifice in orificeStructures)
            {
                var extendedOrifice = network.Branches.OfType<SewerConnectionOrifice>().FirstOrDefault(b => b.Name.Equals(orifice.Name));
                Assert.IsNotNull(extendedOrifice);

                Assert.AreEqual(orifice.Bottom_Level, extendedOrifice.Bottom_Level, "the attributes from the element do not match");
            }

        }

        [Test]
        public void TestImportSewerConnectionReplacesExistingOne()
        {
            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;
            /*We know these two nodes are referred in the test data*/
            var replacedConnection = "lei1";
            var length = 1000;
            var sewerConnection = new SewerConnection() { Name = replacedConnection, Length = length };
            network.Branches.Add(sewerConnection);

            Assert.AreEqual(1, network.Branches.Count(n => n.Name.Equals(replacedConnection)));
            Assert.AreEqual(sewerConnection, network.Branches.First(n => n.Name.Equals(replacedConnection)));

            //Load Sewer Connections
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();
            var importedConnections = gwswImporter.ImportItem(filePath, model);
            Assert.IsNotNull(importedConnections);

            Assert.AreEqual(1, network.SewerConnections.Count(n => n.Name.Equals(replacedConnection)));
            Assert.AreNotEqual(length, network.SewerConnections.First(n => n.Name.Equals(replacedConnection)).Length);
        }

        [Test]
        public void WhenImportingCompartmentsFromGwswFilesWithoutTargetHydroNetwork_ThenImportIsSuccessfullAsGwswElement()
        {
            //Load manholeNodes
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();
            var networkFeatures = gwswImporter.ImportItem(filePath) as IEnumerable<INetworkFeature>;
            Assert.IsNotNull(networkFeatures);

            var fileAsStringList = File.ReadAllLines(filePath);
            var expectedElements = fileAsStringList.Length - 1; // we should not include the header

            var listElements = networkFeatures.ToList();
            Assert.IsNotNull(listElements);
            Assert.That(listElements.Count, Is.EqualTo(expectedElements));
            listElements.ForEach( el => Assert.IsTrue(el is INode));
        }

        [Test]
        public void WhenImportingCompartmentsFromGwswFilesWithTargetHydroNetwork_ThenNetworkIsCorrectlyFilledWithManholes()
        {
            //Load GWSW definition
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var model = new WaterFlowFMModel();
            var network = model.Network;
            Assert.IsFalse(network.Manholes.Any());

            //Load compartments and fill the network with corresponding manholes
            var gwswImporter = GetFeatureImporterWithDefinitionFilePath();
            var importedManholes = gwswImporter.ImportItem(filePath, model);
            Assert.IsNotNull(importedManholes);
            Assert.IsNotEmpty(network.Manholes);

            // Check that the amount of manholes in the network are as expected (no duplicates or whatsoever)
            var importedCompartmentsList = importedManholes as List<INetworkFeature>;
            Assert.NotNull(importedCompartmentsList);
            var expectedManholeCount = importedCompartmentsList.OfType<Manhole>().Select(c => c.Name).Distinct().Count();
            Assert.That(network.Manholes.Count(), Is.EqualTo(expectedManholeCount));

            // Check that amount of compartments in the network are the same as were imported by the importer
            var totalCompartmentsInNetwork = network.Manholes.Sum(m => m.Compartments.Count);
            Assert.That(totalCompartmentsInNetwork, Is.EqualTo(importedCompartmentsList.Count));
        }

        #endregion

        #region Helpers

        private static GwswFeatureImporter GetFeatureImporterWithDefinitionFilePath()
        {
            var importer = new GwswFeatureImporter();
            Assert.IsNotNull(importer);
            importer.definitionFilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv"); ;

            return importer;
        }

        private static IList<GwswElement> GwswFileImportAsGwswElementsWorksCorrectly(GwswBaseImporter importer, string filePath, bool continousTesting = false)
        {
            var importedElementList = importer.ImportGwswElementList(filePath);
            Assert.IsNotNull(importedElementList, string.Format("The .csv file {0}, could not be imported.", filePath));

            var fileAsStringList = File.ReadAllLines(filePath);
            var numberOfLines = fileAsStringList.Length - 1; // we should not include the header
            var rowsCount = importedElementList.Count;
            if (rowsCount != numberOfLines && !continousTesting)
            {
                //Check there are no repeated columns in the .CSV
                var repeatedElements = fileAsStringList[0].Split(GwswBaseImporterTest.csvCommaDelimeter).GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key)
                    .ToList();

                /* If there were repeated columns, that table will simply not be imported, and the user will receive a log message saying so. 
                  as for this test, we are interested onto continuing, so we can ignore when a column is repeated. Its corresponding test will fail. */
                if (repeatedElements.Count == 0)
                {
                    Assert.AreEqual(numberOfLines, rowsCount, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));
                }
            }

            return importedElementList;
        }

        #endregion
    }
}