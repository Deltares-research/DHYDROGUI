using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class GwswFileImporterTest
    {
        private static char csvDelimeter = ',';

        #region Gwsw Attribute tests

        [Test]
        public void GwswAttributeReturnsElementNameWithoutExtension()
        {
            var elementName = "test_element";
            var attributeTest = new GwswAttributeType()
            {
                ElementName = elementName + ".csv",
            };
            Assert.AreEqual(elementName, attributeTest.ElementName);
            
            /* If the name is originally given without extension, it should remain the same.*/
            attributeTest = new GwswAttributeType()
            {
                ElementName = elementName,
            };
            Assert.AreEqual(elementName, attributeTest.ElementName);
        }

        [Test]
        [TestCase("string", typeof(string))]
        [TestCase("double", typeof(double))]
        public void GwswAttibuteAssignesATypeToTheValue(string typeAsString, Type expectedType)
        {
            try
            {
                var attributeTest = new GwswAttributeType("testFile.csv", 0, "attributeName", typeAsString, "testCode", "test definition", "mandatory", "remarks");
                Assert.IsNotNull(attributeTest);
                Assert.AreEqual(expectedType, attributeTest.AttributeType);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("The following error was given while trying to create a new Gwsw Attribute {0}", e.Message));
            }
        }

        #endregion

        #region Gwsw Import tests

        [Test]
        public void ImportGwswDefinitionFileLoadsAsManyAttributesAsLinesInTheCsv()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            var gwswImporter = new GwswFileImporterBase();
            var attributeTable = gwswImporter.ImportDefinitionFile(filePath);
            Assert.IsNotNull(attributeTable);

            var numberOfLines = File.ReadAllLines(filePath).Length - 1; // we should not include the header
            var rowsCount = attributeTable.Rows.Count;
            Assert.AreEqual(numberOfLines, rowsCount, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));

            Assert.AreEqual(numberOfLines, gwswImporter.AttributesDefinition.Count, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));
        }

        [Test]
        public void CreateGwswDataTableFromDefinitionFileThenImportFilesAsDataTables()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            // Import Csv Definition.
            var gwswImporter = new GwswFileImporterBase();
            var definitionTable = gwswImporter.ImportDefinitionFile(filePath);

            var attributeList = new List<GwswAttributeType>();

            foreach (DataRow row in definitionTable.Rows)
            {
                var attributeFile = row.ItemArray[0].ToString();
                var attributeElement = row.ItemArray[1].ToString();
                var attributeName = row.ItemArray[2].ToString();
                var attributeCode = row.ItemArray[3].ToString();
                var attributeCodeInt = row.ItemArray[4].ToString();
                var attributeDefinition = row.ItemArray[5].ToString();
                var attributeType = row.ItemArray[6].ToString();

                var attribute = new GwswAttributeType()
                {
                    Name = attributeName,
                    ElementName = attributeElement,
                    Definition = attributeDefinition,
                    FileName = attributeFile,
                    Key = attributeCodeInt,
                    LocalKey = attributeCode,
                    AttributeType = FMParser.GetClrType(attributeName, attributeType, ref attributeDefinition,
                        attributeFile, definitionTable.Rows.IndexOf(row)),
                };

                attributeList.Add(attribute);
            }

            Assert.IsTrue(attributeList.Count > 0, string.Format("Attributes found {0}", attributeList.Count));

            var uniqueFileList = attributeList.GroupBy( i => i.FileName).Select( grp => grp.Key).ToList();
            Assert.AreEqual(uniqueFileList.Count, 13, "Mismatch on found filenames.");

            var csvSettings = new CsvSettings
            {
                Delimiter = csvDelimeter,
                FirstRowIsHeader = true,
                SkipEmptyLines = true
            };

            var importedTables = new List<DataTable>();

            //Read each one of the files.
            foreach (var fileName in uniqueFileList)
            {
                var directoryName = Path.GetDirectoryName(filePath);
                var elementFilePath = Path.Combine(directoryName, fileName);
                Assert.IsTrue(File.Exists(elementFilePath), string.Format("Could not find file {0}", elementFilePath));

                //Import file elements based on their attributes
                var fileAttributes = attributeList.Where(at => at.FileName.Equals(fileName)).ToList();
                var fileColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>();
                //Create column mapping
                fileAttributes.ForEach( 
                    attr => 
                    fileColumnMapping.Add(
                        new CsvRequiredField(attr.Key, attr.AttributeType),
                        new CsvColumnInfo(fileAttributes.IndexOf(attr), CultureInfo.InvariantCulture)));

                var mapping = new CsvMappingData()
                {
                    Settings = csvSettings,
                    FieldToColumnMapping = fileColumnMapping
                };

                var importedElementTable = GwswFileImportAsDataTableWorksCorrectly(elementFilePath, mapping, true);
                importedTables.Add(importedElementTable);
            }

            Assert.AreEqual( uniqueFileList.Count, importedTables.Count, string.Format("Not all files were imported correctly."));
        }

        [Test]
        public void ImportGwswFilesFromDefinitionFileLoadsAllElements()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            try
            {
                var network = new HydroNetwork();
                var importedObject = gwswImporter.ImportFilesFromDefinitionFile(filePath, network);
                Assert.IsNotNull(importedObject);

                var uniqueFileList = gwswImporter.AttributesDefinition.GroupBy(i => i.FileName).Select(grp => grp.Key).ToList();
                var expectedNumberOfElements = 0;

                foreach (var fileName in uniqueFileList)
                {
                    var directoryName = Path.GetDirectoryName(filePath);
                    var elementFilePath = Path.Combine(directoryName, fileName);
                    Assert.IsTrue(File.Exists(elementFilePath));
                    expectedNumberOfElements += File.ReadAllLines(elementFilePath).Length - 1;
                }

                Assert.AreNotEqual(expectedNumberOfElements, 0, "No elements were read correctly, so the test cannot compare imported and elements in the file.");
                Assert.AreEqual(expectedNumberOfElements, importedObject.Count, "Not all elements were imported correctly. Other tests might be failing due to this.");
            }
            catch (Exception e)
            {
                Assert.Fail("While importing an exception was thrown {0}", e.Message);
            }
        }

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
        [TestCase(@"gwswFiles\Profiel_duplicate_column_Id.csv")]
        public void ImportGwswCsvFileWithLoadedGwswDefinition(string testCasePath)
        {
            //Load GWSW definition
            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            var gwswImporter = new GwswFileImporterBase();
            Assert.NotNull(gwswImporter);

            //No need to test this as already has existent tests
            gwswImporter.ImportDefinitionFile(definitionfilePath);
            Assert.NotNull( gwswImporter.AttributesDefinition );

            var filePath = GetFileAndCreateLocalCopy(testCasePath);
            var mappingData = gwswImporter.CreateCsvMappingDataForFile(filePath);

            var elementList = GwswFileImportAsGwswElementsWorksCorrectly(gwswImporter, filePath);

            var importedCsv = new CsvImporter().ImportCsv(filePath, mappingData);
            Assert.IsNotNull(importedCsv, string.Format("The .csv file {0}, could not be imported.", filePath));

            var numberOfLines = File.ReadAllLines(filePath).Length - 1; // we should not include the header
            Assert.AreEqual(numberOfLines, elementList.Count, string.Format("There is a mismatch between expected number of elements and imported."));
            var elementTypeFound = gwswImporter.AttributesDefinition.FirstOrDefault(at => at.FileName.Equals(Path.GetFileName(testCasePath)));
            if (elementTypeFound == null)
            {
                Assert.Fail("Test failed because no element name was found mapped to this file name.");    
            }

            if (numberOfLines != 0)
            {
                var numberOfColumns = File.ReadLines(filePath).First().Split(mappingData.Settings.Delimiter).Where(s => !s.Equals(string.Empty)).ToList().Count;
                foreach (var element in elementList)
                {
                    Assert.AreEqual(elementTypeFound.ElementName, element.ElementTypeName);
                    Assert.AreEqual(numberOfColumns, element.GwswAttributeList.Count, string.Format("There is a mismatch between expected and imported attributes for element {0}", element.ElementTypeName));
                }
            }
        }
        
        [TestCase(@"gwswFiles\BOP.csv")]
        [TestCase(@"gwswFiles\Debiet.csv")]
        [TestCase(@"gwswFiles\GroeneDaken.csv")]
        [TestCase(@"gwswFiles\ItObject.csv")]
        [TestCase(@"gwswFiles\Knooppunt.csv")]
        [TestCase(@"gwswFiles\KunstWerk.csv")]
        [TestCase(@"gwswFiles\Meta.csv")]
        [TestCase(@"gwswFiles\Nwrw.csv")]
        [TestCase(@"gwswFiles\Oppervlak.csv")]
        [TestCase(@"gwswFiles\Profiel.csv")]
        [TestCase(@"gwswFiles\Verbinding.csv")]
        [TestCase(@"gwswFiles\Verloop.csv")]
        [TestCase(@"gwswFiles\Profiel_duplicate_column_Id.csv")]
        public void ImportGwswCsvFileWithLoadedGwswDefinitionAsDataTables(string testCasePath)
        {
            //Load GWSW definition
            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            //No need to test this as already has existent tests
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            var filePath = GetFileAndCreateLocalCopy(testCasePath);
            var mappingData = gwswImporter.CreateCsvMappingDataForFile(filePath);

            GwswFileImportAsDataTableWorksCorrectly(filePath, mappingData);
        }

        [Test]
        public void ImportCsvDebietFileUsingGwswFileImporterAndHardcodedMapping()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Debiet.csv");
            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = ';',
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("UNI_IDE", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("DEB_TYP", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("VER_IDE", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AVV_ENH", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_OPP", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_TOE", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                }
            };

            GwswFileImportAsDataTableWorksCorrectly(filePath, mappingData);
        }

        [Test]
        public void ImportGwswDefinitionFileWithHardcodedMapping()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("Bestandsnaam", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ElementName", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Kolomnaam", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Code", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Code_International", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Definitie", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Type", typeof (string)),
                        new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Eenheid", typeof (string)),
                        new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Verplicht", typeof (string)),
                        new CsvColumnInfo(8, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Opmerking", typeof (string)),
                        new CsvColumnInfo(9, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckCsvIsImportedCorrectly(filePath, mappingData);
        }

        #endregion

        #region Gwsw Import Elements

        [Test]
        public void TestImportFromDefinitionFileCreatesAllSortOfElementsInNetwork()
        {
            var network = new HydroNetwork();
            Assert.IsFalse(network.Pipes.Any());
            Assert.IsFalse(network.Manholes.Any());
            Assert.IsFalse(network.SewerProfiles.Any());
            Assert.IsFalse(network.Pumps.Any()); 

            var gwswImporter = new GwswFileImporterBase();
            try
            {
                gwswImporter.ImportFilesFromDefinitionFile(@"gwswFiles\GWSW.hydx_Definitie_DM.csv", network);
            }
            catch (Exception e)
            {
                Assert.Fail("While importing an exception was thrown {0}", e.Message);
            }

            Assert.IsTrue(network.Manholes.Any());
            Assert.IsTrue(network.SewerProfiles.Any());
            Assert.IsTrue(network.Pipes.Any());//There are some pipes defined within the verbinding.csv
            Assert.IsTrue(network.Pumps.Any());//There are some pumps defined within the verbinding.csv
        }

        [Test]
        public void TestImportSewerConnectionsFromGwswWithoutPreviousMappingFails()
        {
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNull( gwswImporter.AttributesDefinition );

            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            Assert.IsNull(gwswImporter.ImportItem(filePath));
        }

        [Test]
        public void TestImportSewerConnectionsFromGwswWithMappingSucceeds()
        {
            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            Assert.IsNotNull(gwswImporter.AttributesDefinition);
            Assert.IsNotEmpty(gwswImporter.AttributesDefinition);

            //Load pipes.
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedObject = gwswImporter.ImportItem(filePath);

            Assert.IsNotNull(importedObject);
            var listElements = importedObject as List<GwswElement>;
            Assert.IsNotNull(listElements);

            var fileAsStringList = File.ReadAllLines(filePath);
            var expectedElements = fileAsStringList.Length - 1; // we should not include the header
            Assert.AreEqual(expectedElements, listElements.Count);

            listElements.ForEach( el => Assert.AreEqual(SewerFeatureType.Connection.ToString(), el.ElementTypeName));

            //Now import them as IHydroNetworkFeature
            var network = new HydroNetwork();
            Assert.IsFalse( network.Pipes.Any() );

            var importedItems = gwswImporter.ImportItem(filePath, network);
            Assert.IsNotNull(importedItems);
            Assert.IsTrue(network.SewerConnections.Any());
            
            //There should be some pipes.
            Assert.IsTrue(network.Pipes.Any());

            var importedSewerItems = importedItems as List<INetworkFeature>;
            Assert.IsNotNull(importedSewerItems);
            
            importedSewerItems.ToList().ForEach( pipe => Assert.IsNotNull( pipe as SewerConnection));
            Assert.AreEqual(listElements.Count, importedSewerItems.OfType<SewerConnection>().Count());

            //Check imported list has been added to the network pipes.
            Assert.AreEqual(importedSewerItems, network.SewerConnections.ToList());
        }

        [Test]
        public void TestImportSewerConnectionFromFileAssignsNodesWhenTheyExist()
        {
            //Create network
            var network = new HydroNetwork();
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

            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            //Load pipes.
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(filePath, network);
            Assert.IsNotNull(importedConnections);

            Assert.IsTrue(network.SewerConnections.Any());
            Assert.IsFalse(network.SewerConnections.Any(p => p.Source == null), "Source node has not been created during import process.");
            Assert.IsFalse(network.SewerConnections.Any(p => p.Target == null), "Target node has not been created during import process.");

            Assert.IsTrue(network.SewerConnections.Any( p => p.Source.Equals( startNode ) && p.Target.Equals( endNode )));
        }

        [Test]
        public void TestImportPipesFromFileCreatesNodesWhenTheyDoNotExist()
        {
            //Create network
            var network = new HydroNetwork();
            /*We know these two nodes are referred in the test data*/
            var expectedStartNodeName = "put9";
            var expectedEndNodeName = "put8";

            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            //Load pipes.
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedPipes = gwswImporter.ImportItem(filePath, network);
            Assert.IsNotNull(importedPipes);

            Assert.IsTrue(network.Pipes.Any());
            Assert.IsFalse(network.Pipes.Any( p => p.Source == null), "Source node has not been created during import process.");
            Assert.IsFalse(network.Pipes.Any(p => p.Target == null), "Target node has not been created during import process.");
            Assert.IsTrue(network.Pipes.Any(p => p.SourceCompartment.Name.Equals(expectedStartNodeName) && p.TargetCompartment.Name.Equals(expectedEndNodeName)));
            
            //Checking manhole name is stored as id
            Assert.IsTrue(network.Manholes.Any( n => n.ContainsCompartment(expectedStartNodeName)));
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartment(expectedEndNodeName)));
        }

        [Test]
        public void TestImportStructuresThenImportSewerConnectionsAssignsStructuresValues()
        {
            //Create network
            var network = new HydroNetwork();

            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            //Load structures.
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            var importedStructures = gwswImporter.ImportItem(structuresPath, network);
            Assert.IsNotNull(importedStructures);
            
            //Check placeholders have been created.
            Assert.IsTrue(network.SewerConnections.Any());
            Assert.IsTrue(network.Structures.Any());

            var structuresPh = network.Structures.Where( s => !(s is CompositeBranchStructure)).ToList();

            // Now Load connections.
            var connectionsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(connectionsPath, network);
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
            var network = new HydroNetwork();

            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            //Load structures.
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            var importedStructures = gwswImporter.ImportItem(structuresPath, network);
            Assert.IsNotNull(importedStructures);

            //Check placeholders have been created.
            Assert.IsTrue(network.Manholes.Any());

            var outletCompartments = network.Manholes.SelectMany( mh => mh.Compartments.Where( cmp => cmp is OutletCompartment)).ToList();
            Assert.IsTrue(outletCompartments.Any());

            // Now Load connections.
            var compartmentsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var importedCompartments = gwswImporter.ImportItem(compartmentsPath, network);
            Assert.IsNotNull(importedCompartments);

            foreach (var compartment in outletCompartments)
            {
                var outlet = compartment as OutletCompartment;
                Assert.IsNotNull(outlet);

                var manhole = network.Manholes.FirstOrDefault( m => m.ContainsCompartment(outlet.Name));
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
            var network = new HydroNetwork();

            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            //Load sewer profiles
            var sewerProfilesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");
            var importedProfiles = gwswImporter.ImportItem(sewerProfilesPath, network) as List<INetworkFeature>;
            Assert.IsNotNull(importedProfiles);

            //Check that sewer profiles have been put into the network
            Assert.That(network.SewerProfiles.Count, Is.EqualTo(importedProfiles.Count));
            var sewerProfileNames = network.SewerProfiles.Select(sp => sp.Name);
            importedProfiles.ForEach(f =>
            {
                var profile = f as ICrossSection;
                Assert.NotNull(profile);
                Assert.IsTrue(sewerProfileNames.Contains(profile.Name));
            });

            // Now Load connections.
            var connectionsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(connectionsPath, network) as List<INetworkFeature>;
            Assert.IsNotNull(importedConnections);

            var pipes = network.Branches.OfType<Pipe>().ToList();
            Assert.IsTrue(pipes.Any());
            pipes.ForEach(p => Assert.NotNull(p.SewerProfile));
        }

        [Test]
        public void WhenImportingSewerConnectionsToNetworkAndThenImportingSewerProfilesToNetwork_ThenSewerConnectionsHaveTheCorrectSewerProfiles()
        {
            //Create network
            var network = new HydroNetwork();

            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            //Load connections
            var connectionsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(connectionsPath, network) as List<INetworkFeature>;
            Assert.IsNotNull(importedConnections);

            // Retrieve one profile to later compare to the same one after loading the sewer profiles
            var csDefinitionBefore = (CrossSectionDefinitionStandard) network.SewerProfiles.FirstOrDefault(sp => sp.Name == "PRO2")?.Definition;
            Assert.NotNull(csDefinitionBefore);
            var csShapeBefore = (CrossSectionStandardShapeWidthHeightBase)csDefinitionBefore.Shape;
            Assert.NotNull(csShapeBefore);

            // Now Load sewer profiles
            var sewerProfilesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");
            var importedProfiles = gwswImporter.ImportItem(sewerProfilesPath, network) as List<INetworkFeature>;
            Assert.IsNotNull(importedProfiles);

            //Check that sewer profiles have been put into the network
            Assert.That(network.SewerProfiles.Count, Is.EqualTo(importedProfiles.Count));
            var sewerProfileNames = network.SewerProfiles.Select(sp => sp.Name);
            importedProfiles.ForEach(f =>
            {
                var profile = f as ICrossSection;
                Assert.NotNull(profile);
                Assert.IsTrue(sewerProfileNames.Contains(profile.Name));
            });

            //Get the same profile as before loading the profiles
            var testProfileAfter = network.SewerProfiles.FirstOrDefault(sp => sp.Name == csDefinitionBefore.Name);
            Assert.NotNull(testProfileAfter);
            var csShapeAfter = (CrossSectionStandardShapeWidthHeightBase)((CrossSectionDefinitionStandard) testProfileAfter.Definition).Shape;
            Assert.NotNull(csShapeAfter);

            // Compare the width and height to the one before
            Assert.AreNotEqual(csShapeAfter.Width, csShapeBefore.Width);
            Assert.AreNotEqual(csShapeAfter.Height, csShapeBefore.Height);
            Assert.AreNotEqual(csShapeAfter.Type, csShapeBefore.Type);
        }

        [Test]
        public void TestImportOrificesFromStructuresThenImportOrificesAssignsStructuresValues()
        {
            //Create network
            var network = new HydroNetwork();

            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            //Load structures.
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            var importedStructures = gwswImporter.ImportItem(structuresPath, network);
            Assert.IsNotNull(importedStructures);

            //Check placeholders have been created.
            Assert.IsTrue(network.Branches.Any());

            var orificeStructures = network.Branches.OfType<SewerConnectionOrifice>().ToList();
            Assert.IsTrue(orificeStructures.Any());

            // Now Load connections.
            var compartmentsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(compartmentsPath, network);
            Assert.IsNotNull(importedConnections);

            foreach (var orifice in orificeStructures)
            {
                var extendedOrifice = network.Branches.OfType<SewerConnectionOrifice>().FirstOrDefault(b => b.Name.Equals(orifice.Name));
                Assert.IsNotNull(extendedOrifice);

                Assert.AreEqual(orifice.Bottom_Level, extendedOrifice.Bottom_Level, "the attributes from the element do not match");
            }

        }

        [Test]
        public void TestImportElementReplacesExistingOne()
        {
            //Create network
            var network = new HydroNetwork();
            /*We know these two nodes are referred in the test data*/
            var replacedConnection = "lei1";
            var length = 1000;
            var sewerConnection = new SewerConnection(){ Name = replacedConnection, Length = length };
            network.Branches.Add(sewerConnection);

            Assert.AreEqual(1, network.Branches.Count(n => n.Name.Equals(replacedConnection)));
            Assert.AreEqual(sewerConnection, network.Branches.First(n => n.Name.Equals(replacedConnection)));

            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            //Load pipes.
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(filePath, network);
            Assert.IsNotNull(importedConnections);

            Assert.AreEqual(1, network.SewerConnections.Count(n => n.Name.Equals(replacedConnection)));
            Assert.AreNotEqual(length, network.SewerConnections.First(n => n.Name.Equals(replacedConnection)).Length);
        }

        [Test]
        public void WhenImportingCompartmentsFromGwswFilesWithoutTargetHydroNetwork_ThenImportIsSuccessfull()
        {
            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            Assert.IsNotNull(gwswImporter.AttributesDefinition);
            Assert.IsNotEmpty(gwswImporter.AttributesDefinition);

            //Load manholeNodes
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var importedObject = gwswImporter.ImportItem(filePath);

            Assert.IsNotNull(importedObject);
            var listElements = importedObject as List<GwswElement>;
            Assert.IsNotNull(listElements);

            var fileAsStringList = File.ReadAllLines(filePath);
            var expectedElements = fileAsStringList.Length - 1; // we should not include the header
            Assert.That(listElements.Count, Is.EqualTo(expectedElements));
            listElements.ForEach(el => Assert.AreEqual(SewerFeatureType.Node.ToString(), el.ElementTypeName));
        }

        [Test]
        public void WhenImportingCompartmentsFromGwswFilesWithTargetHydroNetwork_ThenNetworkIsCorrectlyFilledWithManholes()
        {
            //Load GWSW definition
            var gwswImporter = new GwswFileImporterBase();
            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswImporter.ImportDefinitionFile(definitionfilePath);

            Assert.IsNotNull(gwswImporter.AttributesDefinition);
            Assert.IsNotEmpty(gwswImporter.AttributesDefinition);
            
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var network = new HydroNetwork();
            Assert.IsFalse(network.Manholes.Any());

            //Load compartments and fill the network with corresponding manholes
            var importedCompartments = gwswImporter.ImportItem(filePath, network);
            Assert.IsNotNull(importedCompartments);
            Assert.IsNotEmpty(network.Manholes);

            // Check that the amount of manholes in the network are as expected
            var importedCompartmentsList = importedCompartments as List<INetworkFeature>;
            Assert.NotNull(importedCompartmentsList);
            var expectedManholeCount = importedCompartmentsList.OfType<Compartment>().Select(c => c.ParentManhole.Name).Distinct().Count();
            Assert.That(network.Manholes.Count(), Is.EqualTo(expectedManholeCount));

            // Check that amount of compartments in the network are the same as were imported by the importer
            var totalCompartmentsInNetwork = network.Manholes.Sum(m => m.Compartments.Count);
            Assert.That(totalCompartmentsInNetwork, Is.EqualTo(importedCompartmentsList.Count));
        }

        #endregion

        #region Helpers

        private static List<GwswElement> GwswFileImportAsGwswElementsWorksCorrectly(GwswFileImporterBase importer, string filePath, bool continousTesting = false)
        {
            var importedObject = importer.ImportItem(filePath);
            var importedElementList = importedObject as List<GwswElement>;
            Assert.IsNotNull(importedElementList, string.Format("The .csv file {0}, could not be imported.", filePath));

            var fileAsStringList = File.ReadAllLines(filePath);
            var numberOfLines = fileAsStringList.Length - 1; // we should not include the header
            var rowsCount = importedElementList.Count;
            if (rowsCount != numberOfLines && !continousTesting)
            {
                //Check there are no repeated columns in the .CSV
                var repeatedElements = fileAsStringList[0].Split(csvDelimeter).GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key)
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

        private static DataTable GwswFileImportAsDataTableWorksCorrectly(string filePath, CsvMappingData mappingData, bool continousTesting = false)
        {
            var importer = new GwswFileImporterBase();
            Assert.IsNotNull(importer);

            var importedTable = importer.ImportFileAsDataTable(filePath, mappingData);
            Assert.IsNotNull(importedTable, string.Format("The .csv file {0}, could not be imported.", filePath));

            var fileAsStringList = File.ReadAllLines(filePath);
            var numberOfLines = fileAsStringList.Length - 1; // we should not include the header
            var rowsCount = importedTable.Rows.Count;
            if (rowsCount != numberOfLines && !continousTesting)
            {
                //Check there are no repeated columns in the .CSV
                var repeatedElements = fileAsStringList[0].Split(csvDelimeter).GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key)
                    .ToList();
                
                /* If there were repeated columns, that table will simply not be imported, and the user will receive a log message saying so. 
                  as for this test, we are interested onto continuing, so we can ignore when a column is repeated. Its corresponding test will fail. */
                if (repeatedElements.Count == 0)
                {
                    Assert.AreEqual(numberOfLines, rowsCount, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));
                }
            }

            return importedTable;
        }

        private static void CheckCsvIsImportedCorrectly(string filePath, CsvMappingData mappingData)
        {
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            try
            {
                var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
                Assert.IsNotNull(importedCsv, string.Format("The .csv file {0}, could not be imported.", filePath));

                var numberOfLines = File.ReadAllLines(filePath).Length - 1; // we should not include the header
                var rowsCount = importedCsv.Rows.Count;
                Assert.AreEqual(numberOfLines, rowsCount, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private string GetFileAndCreateLocalCopy(string path)
        {
            var filePath = TestHelper.GetTestFilePath(path);
            Assert.IsTrue(File.Exists(filePath), string.Format("File {0} could not be located", filePath));

            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath), string.Format("File {0} could not be located", filePath));

            return filePath;
        }

        #endregion
    }
}