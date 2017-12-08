using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class GwswBaseImporterTest: GwswImporterTestHelper
    {
        private static GwswDefinitionImporter gwswDefinitionFileImporter;

        [TestFixtureSetUp]
        public void TestFixture()
        {
            //Load GWSW definition
            var definitionfilePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            gwswDefinitionFileImporter = new GwswDefinitionImporter() { ImportWithFiles = false };
            Assert.NotNull(gwswDefinitionFileImporter);

            //No need to test this as already has existent tests
            gwswDefinitionFileImporter.ImportItem(definitionfilePath);
            Assert.IsNotNull(gwswDefinitionFileImporter.GwswAttributesDefinition);
            Assert.IsNotEmpty(gwswDefinitionFileImporter.GwswAttributesDefinition);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {

        }

        #region Gwsw Attribute tests

        [Test]
        public void GetEnumTypeFromGwswAttribute_ReturnsDefaultValueAndLogMessage_IfNotFound()
        {
            var elementName = "test_element";
            var attributeTest = new GwswAttribute
            {
                ValueAsString = elementName,
                GwswAttributeType = new GwswAttributeType { AttributeType = typeof(string) }
            };

            SewerConnectionWaterType value = SewerConnectionWaterType.FlowingRainWater;
            //Just to make sure the test is setting the default value later on.
            Assert.AreNotEqual(default(SewerConnectionWaterType), value);

            var msg = String.Format(
                Resources
                    .SewerFeatureFactory_GetValueFromDescription_Type__0__is_not_recognized__please_check_the_syntax,
                elementName);
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => value = attributeTest.GetValueFromDescription<SewerConnectionWaterType>(), msg);

            Assert.IsNotNull(value);
            Assert.AreEqual(default(SewerConnectionWaterType), value);
        }

        [Test]
        public void GetEnumTypeFromGwswAttribute_ReturnsCorrectValue_IfFound()
        {
            var elementName = "DWA";
            var attributeTest = new GwswAttribute
            {
                ValueAsString = elementName,
                GwswAttributeType = new GwswAttributeType { AttributeType = typeof(string)}
            };

            var value = attributeTest.GetValueFromDescription<SewerConnectionWaterType>();
            Assert.IsNotNull(value);
            Assert.AreEqual(SewerConnectionWaterType.DryWeatherRainage, value);
        }

        [Test]
        public void GwswAttributeIsValid_ReturnsFalseWithoutLogMessageIfNoTypeIsPresent()
        {
            var emptyAttribute = new GwswAttribute { GwswAttributeType = new GwswAttributeType()};
            var value = true;

            var msg = string.Format(Resources.GwswElementExtensions_LogInvalidAttribute_File__0___line__1___Attribute__2__is_not_valid_and_will_not_be_imported_, null, 0, null);
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => value = emptyAttribute.IsValidAttribute(), msg);
            Assert.IsFalse(value);
        }

        [Test]
        public void GwswAttributeIsValid_ReturnsTrueIfEverythingIsPresent()
        {
            var emptyAttribute = new GwswAttribute { ValueAsString = "test", GwswAttributeType = new GwswAttributeType(){AttributeType = typeof(string)} };
            Assert.IsTrue(emptyAttribute.IsValidAttribute());
        }

        [Test]
        public void GwswAttributeIsValid_ReturnsFalseIfNoTypeIsPresent()
        {
            var emptyAttribute = new GwswAttribute { GwswAttributeType = new GwswAttributeType() };
            Assert.IsFalse(emptyAttribute.IsValidAttribute());
        }

        [Test]
        public void GwswAttribute_IsTypeOfInt_Test()
        {
            var attr = new GwswAttribute
            {
                GwswAttributeType = new GwswAttributeType {AttributeType = typeof(int)}
            };
            Assert.IsTrue(attr.IsTypeOf(typeof(int)));
            Assert.IsFalse(attr.IsTypeOf(typeof(double)));
            Assert.IsFalse(attr.IsTypeOf(typeof(string)));
        }

        [Test]
        public void GwswAttribute_IsTypeOfDouble_Test()
        {
            var attr = new GwswAttribute
            {
                GwswAttributeType = new GwswAttributeType { AttributeType = typeof(double) }
            };
            Assert.IsFalse(attr.IsTypeOf(typeof(int)));
            Assert.IsTrue(attr.IsTypeOf(typeof(double)));
            Assert.IsFalse(attr.IsTypeOf(typeof(string)));
        }

        [Test]
        public void GwswAttribute_IsTypeOfString_Test()
        {
            var attr = new GwswAttribute
            {
                GwswAttributeType = new GwswAttributeType { AttributeType = typeof(string) }
            };
            Assert.IsFalse(attr.IsTypeOf(typeof(int)));
            Assert.IsFalse(attr.IsTypeOf(typeof(double)));
            Assert.IsTrue(attr.IsTypeOf(typeof(string)));
        }

        [Test]
        public void GetElementLine_ReturnsLineIfAvailable()
        {
            var elementName = "DWA";
            var gwswElement = new GwswElement
            {
                GwswAttributeList = new List<GwswAttribute>
                {
                    new GwswAttribute
                    {
                        ValueAsString = elementName,
                        GwswAttributeType = new GwswAttributeType {AttributeType = typeof(string), LineNumber = 2}
                    }
                }
            };
            Assert.AreEqual(2, gwswElement.GetElementLine());
        }

        [Test]
        public void GetElementLine_ReturnsZeroIfNotAvailable()
        {
            var elementName = "DWA";
            var gwswElement = new GwswElement
            {
                GwswAttributeList = new List<GwswAttribute>
                {
                    new GwswAttribute
                    {
                        ValueAsString = elementName,
                    }
                }
            };
            Assert.AreEqual(0, gwswElement.GetElementLine());
        }

        [Test]
        public void GwswAttributeReturnsElementNameWithoutExtension()
        {
            var elementName = "test_element";
            var attributeTest = new GwswAttributeType
            {
                ElementName = elementName + ".csv",
            };
            Assert.AreEqual(elementName, attributeTest.ElementName);
            
            /* If the name is originally given without extension, it should remain the same.*/
            attributeTest = new GwswAttributeType
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
                var attributeTest = new GwswAttributeType("testFile.csv", 0, "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks");
                Assert.IsNotNull(attributeTest);
                Assert.AreEqual(expectedType, attributeTest.AttributeType);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("The following error was given while trying to create a new Gwsw Attribute {0}", e.Message));
            }
        }

        [Test]
        public void GwswElementExtensionsGetAttributeFromList()
        {
            var attributeOne = "attributeOne";
            var attributeTwo = "attributeTwo";
            var valueAsString = "valueAttrOne";
            var gwswElement = new GwswElement
            {
                ElementTypeName = "test",
                GwswAttributeList = new List<GwswAttribute>
                {
                    new GwswAttribute
                    {
                        GwswAttributeType = new GwswAttributeType("testFile", 5, "columnName", "string", attributeOne,
                            "unkownDefinition", "mandatoryMaybe", string.Empty, "noRemarks"),
                        ValueAsString = valueAsString
                    },
                }
            };

            var retrievedAttr = gwswElement.GetAttributeFromList(attributeOne);
            Assert.IsNotNull(retrievedAttr);
            Assert.AreEqual(valueAsString, retrievedAttr.ValueAsString);

            Assert.IsNull(gwswElement.GetAttributeFromList(attributeTwo));
        }

        [Test]
        public void GwswElementExtensionsGetValidStringValueSucceeds()
        {
            var expectedValue = "test";
            string valueAsString = expectedValue;
            string typeAsString = "string";
            try
            {
                var gwswAttributeType = new GwswAttributeType("testFile.csv", 0, "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks");
                Assert.IsNotNull(gwswAttributeType);
                Assert.AreEqual(typeof(string), gwswAttributeType.AttributeType);

                var attribute = new GwswAttribute { GwswAttributeType = gwswAttributeType, ValueAsString = valueAsString };
                Assert.IsNotNull(attribute);

                var testVariable = attribute.GetValidStringValue();
                Assert.AreEqual(expectedValue, testVariable);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("The following error was given while trying to create a new Gwsw Attribute {0}", e.Message));
            }
        }


        [Test]
        public void GwswElementExtensionsTryGetValueAsDoubleSucceeds()
        {
            var expectedValue = 100.0;
            string valueAsString = expectedValue.ToString(CultureInfo.InvariantCulture);
            string typeAsString = "double";
            var testVariable = 0.0;
            try
            {
                var gwswAttributeType = new GwswAttributeType("testFile.csv", 0, "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks");
                Assert.IsNotNull(gwswAttributeType);
                Assert.AreEqual(typeof(double), gwswAttributeType.AttributeType);

                var attribute = new GwswAttribute {GwswAttributeType = gwswAttributeType, ValueAsString = valueAsString };
                Assert.IsNotNull(attribute);

                Assert.IsTrue(attribute.TryGetValueAsDouble(out testVariable));
                Assert.AreEqual(expectedValue, testVariable);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("The following error was given while trying to create a new Gwsw Attribute {0}", e.Message));
            }
        }

        
        [Test]
        public void GwswElementExtensionsSetValueIfPossibleForDoubleFailsWithStringValueAndLogsMessage()
        {
            string valueAsString = "stringValue";
            string typeAsString = "string";
            try
            {
                var gwswAttributeType = new GwswAttributeType("testFile.csv", 0, "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks");
                Assert.IsNotNull(gwswAttributeType);
                Assert.AreEqual(typeof(string), gwswAttributeType.AttributeType);

                var attribute = new GwswAttribute { GwswAttributeType = gwswAttributeType, ValueAsString = valueAsString };
                Assert.IsNotNull(attribute);

                var auxValue = 0.0;
                var logError =
                    string.Format(
                        Resources
                            .GwswElementExtensions_LogErrorParseType_File__0___line__1___element__2___It_was_not_possible_to_parse_attribute__3__from_type__4__to_type__5__,
                        gwswAttributeType.FileName, gwswAttributeType.LineNumber, gwswAttributeType.ElementName,
                        gwswAttributeType.Name, attribute.ValueAsString, gwswAttributeType.AttributeType, typeof(double));

                Assert.IsFalse(attribute.TryGetValueAsDouble(out auxValue));
                TestHelper.AssertAtLeastOneLogMessagesContains( () => attribute.TryGetValueAsDouble(out auxValue), logError);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("The following error was given while trying to create a new Gwsw Attribute {0}", e.Message));
            }
        }

        #endregion

        #region Gwsw Import tests

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
                    Delimiter = GwswBaseImporterTest.csvCommaDelimeter,
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
            var network = new HydroNetwork();
            var gwswImporter = GetFeatureImporterWithLoadedDefinition();
            var elementList = GwswFileImportAsGwswElementsWorksCorrectly(gwswImporter, filePath);

            var numberOfLines = File.ReadAllLines(filePath).Length - 1; // we should not include the header
            Assert.AreEqual(numberOfLines, elementList.Count,
                string.Format("There is a mismatch between expected number of elements and imported."));
            var elementTypeFound =
                gwswDefinitionFileImporter.GwswAttributesDefinition.FirstOrDefault(
                    at => at.FileName.Equals(Path.GetFileName(testCasePath)));
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
        public void TestImportSewerConnectionsFromGwswWithoutPreviousMappingFails_AndLogMessageIsShown()
        {
            var gwswImporter = new GwswBaseImporter();
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var message = string.Format(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__, filePath);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => gwswImporter.ImportGwswElementList(filePath), message);
            Assert.IsNull(gwswImporter.ImportGwswElementList(filePath));
        }

        [Test]
        public void TestImportSewerConnectionsFromGwswWithMappingSucceeds()
        {
            var gwswImporter = GetFeatureImporterWithLoadedDefinition();

            //Load pipes.
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var listElements = gwswImporter.ImportGwswElementList(filePath);
            Assert.IsNotNull(listElements);

            var fileAsStringList = File.ReadAllLines(filePath);
            var expectedElements = fileAsStringList.Length - 1; // we should not include the header
            Assert.AreEqual(expectedElements, listElements.Count);

            listElements.ForEach(el => Assert.AreEqual(SewerFeatureType.Connection.ToString(), el.ElementTypeName));

            //Now import them as IHydroNetworkFeature
            var network = new HydroNetwork();
            Assert.IsFalse(network.Pipes.Any());

            var importedItems = gwswImporter.ImportItem(filePath, network);
            Assert.IsNotNull(importedItems);
            Assert.IsTrue(network.SewerConnections.Any());

            //There should be some pipes.
            Assert.IsTrue(network.Pipes.Any());

            var importedSewerItems = importedItems as List<INetworkFeature>;
            Assert.IsNotNull(importedSewerItems);

            importedSewerItems.ToList().ForEach(pipe => Assert.IsNotNull(pipe as SewerConnection));
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

            var gwswImporter = GetFeatureImporterWithLoadedDefinition();
            //Load pipes.
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(filePath, network);
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
            var network = new HydroNetwork();
            /*We know these two nodes are referred in the test data*/
            var expectedStartNodeName = "put9";
            var expectedEndNodeName = "put8";

            var gwswImporter = GetFeatureImporterWithLoadedDefinition();
            //Load pipes.
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedPipes = gwswImporter.ImportItem(filePath, network);
            Assert.IsNotNull(importedPipes);

            Assert.IsTrue(network.Pipes.Any());
            Assert.IsFalse(network.Pipes.Any(p => p.Source == null), "Source node has not been created during import process.");
            Assert.IsFalse(network.Pipes.Any(p => p.Target == null), "Target node has not been created during import process.");
            Assert.IsTrue(network.Pipes.Any(p => p.SourceCompartment.Name.Equals(expectedStartNodeName) && p.TargetCompartment.Name.Equals(expectedEndNodeName)));

            //Checking manhole name is stored as id
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartment(expectedStartNodeName)));
            Assert.IsTrue(network.Manholes.Any(n => n.ContainsCompartment(expectedEndNodeName)));
        }

        [Test]
        public void TestImportStructuresThenImportSewerConnectionsAssignsStructuresValues()
        {
            //Create network
            var network = new HydroNetwork();
            var gwswImporter = GetFeatureImporterWithLoadedDefinition();

            //Load structures.
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            var importedStructures = gwswImporter.ImportItem(structuresPath, network);
            Assert.IsNotNull(importedStructures);

            //Check placeholders have been created.
            Assert.IsTrue(network.SewerConnections.Any());
            Assert.IsTrue(network.Structures.Any());

            var structuresPh = network.Structures.Where(s => !(s is CompositeBranchStructure)).ToList();

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
            var gwswImporter = GetFeatureImporterWithLoadedDefinition();
            //Load structures.
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            var importedStructures = gwswImporter.ImportItem(structuresPath, network);
            Assert.IsNotNull(importedStructures);

            //Check placeholders have been created.
            Assert.IsTrue(network.Manholes.Any());

            var outletCompartments = network.Manholes.SelectMany(mh => mh.Compartments.Where(cmp => cmp is OutletCompartment)).ToList();
            Assert.IsTrue(outletCompartments.Any());

            // Now Load connections.
            var compartmentsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var importedCompartments = gwswImporter.ImportItem(compartmentsPath, network);
            Assert.IsNotNull(importedCompartments);

            foreach (var compartment in outletCompartments)
            {
                var outlet = compartment as OutletCompartment;
                Assert.IsNotNull(outlet);

                var manhole = network.Manholes.FirstOrDefault(m => m.ContainsCompartment(outlet.Name));
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
            var gwswImporter = GetFeatureImporterWithLoadedDefinition();

            //Load sewer profiles
            var sewerProfilesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");
            var importedProfiles = gwswImporter.ImportItem(sewerProfilesPath, network) as List<INetworkFeature>;
            Assert.IsEmpty(importedProfiles);

            //Check that sewer profiles have been put into the network
            var numberOfLinesInFile = File.ReadAllLines(sewerProfilesPath).Length - 1;
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(numberOfLinesInFile));

            // Now Load connections.
            var connectionsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(connectionsPath, network) as List<INetworkFeature>;
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
            var network = new HydroNetwork();
            var gwswImporter = GetFeatureImporterWithLoadedDefinition();

            //Load connections
            var connectionsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(connectionsPath, network) as List<INetworkFeature>;
            Assert.IsNotNull(importedConnections);

            // Retrieve one profile to later compare to the same one after loading the sewer profiles
            var csDefinitionBefore = (CrossSectionDefinitionStandard) network.SharedCrossSectionDefinitions.FirstOrDefault(crossSectionDefinition => crossSectionDefinition.Name == csdName);
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
            var importedProfiles = gwswImporter.ImportItem(sewerProfilesPath, network) as List<INetworkFeature>;
            Assert.IsEmpty(importedProfiles);

            //Check that sewer profiles have been put into the network
            var numberOfLinesInFile = File.ReadAllLines(sewerProfilesPath).Length - 1;
            var networkCsDefinitions = network.SharedCrossSectionDefinitions;
            Assert.That(networkCsDefinitions.Count, Is.EqualTo(numberOfLinesInFile));

            //Get the same profile as before loading the profiles
            var sharedCsDefinitions = network.SharedCrossSectionDefinitions;
            var testProfileAfter = sharedCsDefinitions.FirstOrDefault(crossSectionDefinition => crossSectionDefinition.Name == csdName);
            Assert.NotNull(testProfileAfter);
            var csShapeAfter = (CrossSectionStandardShapeWidthHeightBase)((CrossSectionDefinitionStandard) testProfileAfter).Shape;
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
            var network = new HydroNetwork();
            var gwswImporter = GetFeatureImporterWithLoadedDefinition();

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
        public void TestImportSewerConnectionReplacesExistingOne()
        {
            //Create network
            var network = new HydroNetwork();
            /*We know these two nodes are referred in the test data*/
            var replacedConnection = "lei1";
            var length = 1000;
            var sewerConnection = new SewerConnection() { Name = replacedConnection, Length = length };
            network.Branches.Add(sewerConnection);

            Assert.AreEqual(1, network.Branches.Count(n => n.Name.Equals(replacedConnection)));
            Assert.AreEqual(sewerConnection, network.Branches.First(n => n.Name.Equals(replacedConnection)));

            //Load GWSW definition
            var gwswImporter = GetFeatureImporterWithLoadedDefinition();

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
            var gwswImporter = GetFeatureImporterWithLoadedDefinition();

            //Load manholeNodes
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var listElements = gwswImporter.ImportGwswElementList(filePath);
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
            var gwswImporter = GetFeatureImporterWithLoadedDefinition();

            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var network = new HydroNetwork();
            Assert.IsFalse(network.Manholes.Any());

            //Load compartments and fill the network with corresponding manholes
            var importedManholes = gwswImporter.ImportItem(filePath, network);
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

        private static GwswBaseImporter GetFeatureImporterWithLoadedDefinition()
        {
            var importer = new GwswBaseImporter();
            Assert.IsNotNull(importer);
            importer.GwswAttributesDefinition = gwswDefinitionFileImporter.GwswAttributesDefinition;
            return importer;
        }

        private static List<GwswElement> GwswFileImportAsGwswElementsWorksCorrectly(GwswBaseImporter importer, string filePath, bool continousTesting = false)
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