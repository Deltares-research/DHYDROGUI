using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Csv.Importer;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.ViewModels;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.ImportExport.GWSW;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class GwswFileImporterTest : GwswFileImporterTestHelper
    {
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

            SewerConnectionWaterType value = SewerConnectionWaterType.StormWater;
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
            Assert.AreEqual(SewerConnectionWaterType.DryWater, value);
        }

        [Test]
        public void GwswAttributeIsValid_ReturnsFalseWithoutLogMessageIfNoTypeIsPresent()
        {
            var invalidAttribute = new GwswAttribute { GwswAttributeType = new GwswAttributeType() };
            CheckThatGwswAttributeValidationLogMessageIsReturned(null, 0, null, string.Empty, invalidAttribute);
        }

        [TestCase("")]
        [TestCase(null)]
        public void GivenGwswAttributeWithEmptyValueAsString_WhenValidatingAttribute_ThenLogMessageIsReturned(string valueAsString)
        {
            const string fileName = "myFile.csv";
            const int lineNumber = 3;
            const string localKey = "XXX_YYY";
            const string key = "MY_KEY";

            var attributeType = new GwswAttributeType
            {
                FileName = fileName,
                LocalKey = localKey,
                Key = key
            };

            var invalidAttribute = new GwswAttribute
            {
                LineNumber = lineNumber,
                ValueAsString = valueAsString,
                GwswAttributeType = attributeType
            };
            
            CheckThatGwswAttributeValidationLogMessageIsReturned(fileName, lineNumber, localKey, key, invalidAttribute);
        }

        [Test]
        public void GwswAttributeIsValid_ReturnsTrueIfEverythingIsPresent()
        {
            var emptyAttribute = new GwswAttribute { ValueAsString = "test", GwswAttributeType = new GwswAttributeType { AttributeType = typeof(string)} };
            Assert.IsTrue(emptyAttribute.IsValidAttribute());
        }

        [Test]
        public void GwswAttributeIsValid_ReturnsFalseIfNoTypeIsPresent()
        {
            var emptyAttribute = new GwswAttribute { GwswAttributeType = new GwswAttributeType() };
            Assert.IsFalse(emptyAttribute.IsValidAttribute());
        }

        [TestCase("")]
        [TestCase(null)]
        public void GwswAttributeIsValid_ReturnsFalseIfValueAsStringIsNullOrEmpty(string valueAsString)
        {
            var invalidAttribute = new GwswAttribute
            {
                ValueAsString = valueAsString,
                GwswAttributeType = new GwswAttributeType()
            };
            Assert.IsFalse(invalidAttribute.IsValidAttribute());
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
        public void GwswAttribute_IsNumerical_GivenInt_Test()
        {
            var attr = new GwswAttribute
            {
                GwswAttributeType = new GwswAttributeType { AttributeType = typeof(int) }
            };
            Assert.IsTrue(attr.IsNumerical());
        }

        [Test]
        public void GwswAttribute_IsNumerical_GivenDouble_Test()
        {
            var attr = new GwswAttribute
            {
                GwswAttributeType = new GwswAttributeType { AttributeType = typeof(double) }
            };
            Assert.IsTrue(attr.IsNumerical());
        }


        [Test]
        public void GwswAttribute_IsNumerical_GivenString_Test()
        {
            var attr = new GwswAttribute
            {
                GwswAttributeType = new GwswAttributeType { AttributeType = typeof(string) }
            };
            Assert.IsFalse(attr.IsNumerical());
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
                        LineNumber = 2,
                        ValueAsString = elementName,
                        GwswAttributeType = new GwswAttributeType { AttributeType = typeof(string) }
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
                var attributeTest = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile.csv", 0, "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks");
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
                        GwswAttributeType = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile", 5, "columnName", "string", attributeOne,
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
                var gwswAttributeType = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile.csv", 0, "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks");
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
                var gwswAttributeType = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile.csv", 0, "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks");
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
                var gwswAttributeType = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile.csv", 0, "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks");
                Assert.IsNotNull(gwswAttributeType);
                Assert.AreEqual(typeof(string), gwswAttributeType.AttributeType);

                var attribute = new GwswAttribute { GwswAttributeType = gwswAttributeType, ValueAsString = valueAsString, LineNumber = 2 };
                Assert.IsNotNull(attribute);

                var auxValue = 0.0;
                var logError =
                    string.Format(
                        Resources
                            .GwswElementExtensions_LogErrorParseType_File__0___line__1___element__2___It_was_not_possible_to_parse_attribute__3__from_type__4__to_type__5__,
                        gwswAttributeType.FileName, attribute.LineNumber, gwswAttributeType.ElementName,
                        gwswAttributeType.Name, attribute.ValueAsString, gwswAttributeType.AttributeType, typeof(double));

                Assert.IsFalse(attribute.TryGetValueAsDouble(out auxValue));
                TestHelper.AssertAtLeastOneLogMessagesContains( () => attribute.TryGetValueAsDouble(out auxValue), logError);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("The following error was given while trying to create a new Gwsw Attribute {0}", e.Message));
            }
        }

        [Test]
        public void GivenGwswAttributeWithEmptyStringAsValue_WhenTryGetDoubleValue_ThenReturnFalseAndDefaultValue()
        {
            var gwswAttributeType = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile.csv", 0, "attributeName", "double", "testCode", "test definition", "mandatory", string.Empty, "remarks");
            var attribute = new GwswAttribute { GwswAttributeType = gwswAttributeType, ValueAsString = string.Empty};

            double doubleValue;
            var gettingValueSucceeded = attribute.TryGetValueAsDouble(out doubleValue);
            Assert.IsFalse(gettingValueSucceeded);
            Assert.That(doubleValue, Is.EqualTo(0.0));
        }

        #endregion

        [Test]
        public void TestImportSewerConnectionsFromGwswWithoutPreviousMappingFails_AndLogMessageIsShown()
        {
            var gwswImporter = new GwswFileImporter();
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            gwswImporter.GwswAttributesDefinition.Clear();
            var message = string.Format(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__, filePath);
            gwswImporter.CsvDelimeter = ';';
            TestHelper.AssertAtLeastOneLogMessagesContains(() => gwswImporter.ImportGwswElementList(filePath), message);
            Assert.IsNull(gwswImporter.ImportGwswElementList(filePath));
        }

        [Test]
        public void TestImport_UnknownFeature_FromGwsw_WithPreviousMapping_Fails_AndLogMessageIsShown()
        {
            var gwswImporter = new GwswFileImporter();
            gwswImporter.CsvDelimeter = ',';
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\UnknownFeature.csv");
            var message = string.Format(Resources.GwswFileImporterBase_ImportItem_Occurrences_on_file__0__will_not_be_mapped_to_any_element_, filePath);
            var importedList = new List<GwswElement>();
            gwswImporter.CsvDelimeter = ';';
            TestHelper.AssertAtLeastOneLogMessagesContains(() => importedList = gwswImporter.ImportGwswElementList(filePath).ToList(), message);
            Assert.IsFalse(importedList.Any());
        }

        [Test]
        public void TestImportFeature_WithUnknownAttribute_FromGwsw_WithPreviousMapping_DoesNotFail_AndLogMessageIsShown()
        {
            var gwswImporter = new GwswFileImporter();
            gwswImporter.CsvDelimeter = ';';

            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\WithUnknownAttribute\Verbinding.csv");
            var message = string.Format(Resources.GwswFileImporterBase_ImportItem_column__0__expectedcolumn__1__of_file__2__was_not_mapped_correctly__, "UNKNOWN_ATTR", "UNI_IDE", filePath);

            var importedList = new List<GwswElement>();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => importedList = gwswImporter.ImportGwswElementList(filePath).ToList(), message);
            Assert.IsFalse(importedList.Any()); //import is cancelled
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
                    Delimiter = GwswFileImporterTest.csvCommaDelimeter,
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

        #region Gwsw Import Elements

        [Test]
        public void ImportFile_WithLoadedDefinition_GivingWrongFilePath_ReturnsNull_AndLogsMessage()
        {
            var filePath = "wrongPath.csv";
            var importer = new GwswFileImporter();

            Assert.IsTrue(importer.GwswAttributesDefinition != null && importer.GwswAttributesDefinition.Any());

            var mssg = string.Format(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Could_not_find_file__0__, filePath);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(filePath), mssg);
        }

        [Test]
        public void GivenConnectionGwswFile_WhenImportingItsSewerFeatures_ThenFeaturesOfDifferentTypesAreImported()
        {
            var importer = new GwswFileImporter();

            Assert.IsTrue(importer.GwswAttributesDefinition != null && importer.GwswAttributesDefinition.Any());
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");

            var importedFeatures = importer.ImportItem(filePath) as List<ISewerFeature>;
            Assert.IsNotNull(importedFeatures);
            Assert.IsTrue(importedFeatures.Any( f => f is IPipe));
            Assert.IsTrue(importedFeatures.Any( f => f is GwswConnectionWeir));
            Assert.IsTrue(importedFeatures.Any( f => f is GwswConnectionPump));
            Assert.IsTrue(importedFeatures.Any( f => f is GwswConnectionOrifice));
        }

        [TestCase("")]
        [TestCase(null)]
        public void ImportFile_WithLoadedDefinition_GivingEmptyStringAsPath_LoadsDefinitionFeaturesList(string importFilePath)
        {
            var definitionPath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            var importer = new GwswFileImporter();
            importer.LoadFeatureFiles(Path.GetDirectoryName(definitionPath));
            Assert.IsTrue(importer.GwswAttributesDefinition != null && importer.GwswAttributesDefinition.Any());

            var importedFeatures = importer.ImportItem(importFilePath) as List<ISewerFeature>;
            Assert.IsNotNull(importedFeatures);
            Assert.IsTrue(importedFeatures.Any(f => f is ISewerConnection));
            Assert.IsTrue(importedFeatures.Any(f => f is ICompartment));
        }

        private static IHydroNetwork ImportFromDefinitionFileAndCheckFeatures(string testFilePath)
        {
            var model = new WaterFlowFMModel();
            var network = model.Network;
            Assert.IsFalse(network.Pipes.Any());
            Assert.IsFalse(network.Manholes.Any());
            Assert.IsFalse(network.SharedCrossSectionDefinitions.Any());
            Assert.IsFalse(network.Pumps.Any());

            var gwswImporter = new GwswFileImporter();
            var filePath = GetFileAndCreateLocalCopy(testFilePath);
            gwswImporter.LoadFeatureFiles(Path.GetDirectoryName(filePath));
            try
            {
                //Definition file is 'comma' separated, but the features are 'semicolon', so we need to change the delimeter.
                gwswImporter.CsvDelimeter = ';';
                gwswImporter.ImportItem(null, model);
            }
            catch (Exception e)
            {
                Assert.Fail("While importing an exception was thrown {0}", e.Message);
            }

            Assert.IsTrue(network.Manholes.Any());
            Assert.IsTrue(network.SharedCrossSectionDefinitions.Any());
            Assert.IsTrue(network.Pipes.Any()); //There are some pipes defined within the verbinding.csv
            Assert.IsTrue(network.Pumps.Any()); //There are some pumps defined within the verbinding.csv

            return network;
        }

        [Test]
        public void TestImportFromDefinitionFileCreatesAllSortOfElementsInNetwork()
        {
            ImportFromDefinitionFileAndCheckFeatures(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
        }

        [Test]
        [TestCase(@"gwswFiles\GWSW_DidactischStelsel\GWSW.hydx_Definitie_DM.csv", 4000)]
        [TestCase(@"gwswFiles\GWSW_Leiden\GWSW.hydx_Definitie_DM.csv", 180000)]
        public void GivenGwswDatabase_WhenImporting_ShouldBeFasterThan(string testFilePath, float maximumImportingTimeInMs)
        {
            var model = new WaterFlowFMModel();
            var network = model.Network;
            Assert.IsFalse(network.Pipes.Any());
            Assert.IsFalse(network.Manholes.Any());
            Assert.IsFalse(network.SharedCrossSectionDefinitions.Any());
            Assert.IsFalse(network.Pumps.Any());

            var gwswImporter = new GwswFileImporter {CsvDelimeter = ';'};
            var filePath = GetFileAndCreateLocalCopy(testFilePath);
            try
            {
                //Definition file is 'comma' separated, but the features are 'semicolon', so we need to change the delimeter.
                TestHelper.AssertIsFasterThan(maximumImportingTimeInMs, () =>
                {
                    gwswImporter.LoadFeatureFiles(Path.GetDirectoryName(filePath));
                    gwswImporter.ImportItem(null, model);
                });
            }
            catch (Exception e)
            {
                Assert.Fail("While importing an exception was thrown {0}", e.Message);
            }

            Assert.IsTrue(network.Manholes.Any());
            Assert.IsTrue(network.SharedCrossSectionDefinitions.Any());
            Assert.IsTrue(network.Pipes.Any()); //There are some pipes defined within the verbinding.csv
            Assert.IsTrue(network.Pumps.Any()); //There are some pumps defined within the verbinding.csv
        }

        [Test]
        public void GivenWswsDatabase_WhenImporting_ThenTheRightAmountOfSewerFeaturesArePresentInTheResultingNetwork()
        {
            var testFilePath = @"gwswFiles\GWSW_DidactischStelsel\GWSW.hydx_Definitie_DM.csv";
            var model = new WaterFlowFMModel();

            var gwswImporter = new GwswFileImporter { CsvDelimeter = ';' };
            var filePath = GetFileAndCreateLocalCopy(testFilePath);
            try
            {
                gwswImporter.LoadFeatureFiles(Path.GetDirectoryName(filePath));
                gwswImporter.ImportItem(null, model);
            }
            catch (Exception e)
            {
                Assert.Fail("While importing an exception was thrown {0}", e.Message);
            }

            var network = model.Network;
            Assert.That(network.Manholes.Count(), Is.EqualTo(76));
            Assert.That(network.OutletCompartments.Count(), Is.EqualTo(4));
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(97));
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(41));

            Assert.That(network.Pumps.Count(), Is.EqualTo(8));
            Assert.That(network.Weirs.Count(), Is.EqualTo(8));
            Assert.That(network.Orifices.Count(), Is.EqualTo(2));

            Assert.That(network.SewerConnections.Count(sc => sc.BranchFeatures.Count >= 2 && sc.BranchFeatures[1] is IPump), Is.EqualTo(8));
            Assert.That(network.SewerConnections.Count(sc => sc.BranchFeatures.Count >= 2 && sc.BranchFeatures[1] is IWeir), Is.EqualTo(8));
            Assert.That(network.SewerConnections.Count(sc => sc.BranchFeatures.Count >= 2 && sc.BranchFeatures[1] is IOrifice), Is.EqualTo(2));
        }

        [Test]
        public void TestImportFromDefinitionFileAndCheckGwswUseCaseImportsAllSewerConnectionsCorrectly()
        {
            var network = ImportFromDefinitionFileAndCheckFeatures(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            Assert.IsNotNull(network);
            Assert.IsNotNull(network.SewerConnections);

            var expectedNumberOfPipes = 81;
            var expectedNumberOfPumps = 8;
            var expectedNumberOfOrifices = 2;
            var expectedNumberOfCrossSections = 41;
            
            Assert.That(network.Pipes.Count(), Is.EqualTo(expectedNumberOfPipes)
                , "Not all pipes have been imported correctly.");
            Assert.That(network.Pumps.Count(), Is.EqualTo(expectedNumberOfPumps)
                , "Not all pumps have been imported correctly.");
            Assert.That(network.Orifices.Count(), Is.EqualTo(expectedNumberOfOrifices)
                , "Not all orifices have been imported correctly.");
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(expectedNumberOfCrossSections)
                , "Not all cross sections have been imported correctly.");
        }

        [Test]
        public void TestImportFromDefinitionFileAndCheckCheckGwswUseCaseImportsAllCompartmentsCorrectly()
        {
            var network = ImportFromDefinitionFileAndCheckFeatures(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            var numberOfManholesInGwsw = 76;
            Assert.IsNotNull(network);

            //CheckManholes
            Assert.IsNotNull(network.Manholes);
            var repeatedManholes = network.Manholes.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            Assert.IsEmpty(repeatedManholes, string.Format("Repeated manhole entries. {0}", String.Concat(repeatedManholes.Select(cmp => cmp.Name + " "))));

            var manholesWithoutPlaceholders = network.Manholes.Where(mh => mh.Compartments.Any()).ToList();
            Assert.AreEqual(numberOfManholesInGwsw, manholesWithoutPlaceholders.Count);

            //Check compartments
            var compartments = manholesWithoutPlaceholders.SelectMany(mh => mh.Compartments).ToList();
            var repeatedCompartments = compartments.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            Assert.IsEmpty(repeatedCompartments, string.Format("Repeated compartments entries. {0}", String.Concat(repeatedCompartments.Select(cmp => cmp.Name + " "))));

            var numberOfCompartmentsInGwsw = 90;
            Assert.AreEqual(numberOfCompartmentsInGwsw, compartments.Count, "Not all compartments were found.");

            //CheckOutlets
            var numberOfOutlets = 4;
            Assert.AreEqual(numberOfOutlets, compartments.Count(cmp => cmp is OutletCompartment), "Not all outlets were found.");
        }

        [Test]
        public void ImportGwswDefinitionFileLoadsAsManyAttributesAsLinesInTheCsv()
        {
            var gwswImporter = new GwswFileImporter();
            var attributesDefinition = gwswImporter.GwswAttributesDefinition;
            Assert.IsNotNull(attributesDefinition);
            Assert.Greater(attributesDefinition.Count, 0, "Defeinition file from resource has not been loaded correctly");
        }

        [Test]
        public void CreateGwswDataTableFromDefinitionFileThenImportFilesAsDataTables()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv"); //should be the same as the resource file
            // Import Csv Definition.
            var gwswImporter = new GwswFileImporter();

            var attributeList = gwswImporter.GwswAttributesDefinition;

            Assert.IsTrue(attributeList.Count > 0, string.Format("Attributes found {0}", attributeList.Count));

            var uniqueFileList = attributeList.GroupBy(i => i.FileName).Select(grp => grp.Key).ToList();
            Assert.AreEqual(uniqueFileList.Count, 12, "Mismatch on found filenames.");

            var csvSettings = new CsvSettings
            {
                Delimiter = csvCommaDelimeter,
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

            Assert.AreEqual(uniqueFileList.Count, importedTables.Count, string.Format("Not all files were imported correctly."));
        }

        #endregion

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
            var gwswImporter = new GwswFileImporter();

            var elementList = GwswFileImportAsGwswElementsWorksCorrectly(gwswImporter, filePath);

            var numberOfLines = File.ReadAllLines(filePath).Length - 1; // we should not include the header
            Assert.AreEqual(numberOfLines, elementList.Count,
                string.Format("There is a mismatch between expected number of elements and imported."));
            var elementTypeFound = gwswImporter.GwswAttributesDefinition.FirstOrDefault(at => at.FileName.Equals(Path.GetFileName(testCasePath)));
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
        public void TestImportSewerConnectionFromFileAssignsNodesWhenTheyExist()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var model = new WaterFlowFMModel();
            var network = model.Network;

            #region Create network

            /*We know these two nodes are referred in the test data*/
            var sourceManholeName = "man001";
            var sourceCompartmentName = "put9";
            var sourceManhole = new Manhole(sourceManholeName);
            var sourceCompartment = new Compartment(sourceCompartmentName);
            sourceManhole.Compartments.Add(sourceCompartment);
            network.Nodes.Add(sourceManhole);

            var targetManholeName = "man001";
            var targetCompartmentName = "put8";
            var targetManhole = new Manhole(targetManholeName);
            var targetCompartment = new Compartment(targetCompartmentName);
            targetManhole.Compartments.Add(targetCompartment);
            network.Nodes.Add(targetManhole);

            #endregion

            var gwswImporter = new GwswFileImporter {CsvDelimeter = ';'};
            var importedConnections = gwswImporter.ImportItem(filePath, model) as List<ISewerFeature>;
            Assert.That(importedConnections, Is.Not.Empty);
            Assert.IsTrue(network.SewerConnections.Any(p => p.Source.Equals(sourceManhole) && p.Target.Equals(targetManhole)));
        }

        [Test]
        public void TestImportStructuresThenImportSewerConnectionsAssignsStructuresValues()
        {
            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;
            var gwswImporter = new GwswFileImporter();

            //Load structures.
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            gwswImporter.CsvDelimeter = ';';
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

            //Load structures.
            var gwswImporter = new GwswFileImporter();
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            gwswImporter.CsvDelimeter = ';';
            var importedStructures = gwswImporter.ImportItem(structuresPath, model) as List<ISewerFeature>;
            Assert.That(importedStructures, Is.Not.Empty);

            //Check placeholders have been created.
            Assert.IsTrue(network.Manholes.Any());

            var outletCompartments = network.Manholes.SelectMany(mh => mh.Compartments.Where(cmp => cmp is OutletCompartment)).ToList();
            Assert.IsTrue(outletCompartments.Any());

            // Now Load connections.
            var compartmentsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var importedCompartments = gwswImporter.ImportItem(compartmentsPath, model) as List<ISewerFeature>;
            Assert.That(importedCompartments, Is.Not.Empty);

            foreach (var compartment in outletCompartments)
            {
                var outlet = compartment as OutletCompartment;
                Assert.IsNotNull(outlet);

                var manhole = network.Manholes.FirstOrDefault(m => m.ContainsCompartmentWithName(outlet.Name));
                Assert.IsNotNull(manhole);
                var extendedOutlet = manhole.GetCompartmentByName(outlet.Name) as OutletCompartment;
                Assert.IsNotNull(extendedOutlet);

                Assert.That(extendedOutlet.SurfaceWaterLevel, Is.EqualTo(outlet.SurfaceWaterLevel),
                    $"the SurfaceWaterLevel of compartment {compartment.Name} was changed after importing the connection.");
            }
        }

        [Test]
        public void WhenImportingSewerProfilesToNetworkAndThenImportingSewerConnectionsToNetwork_ThenSewerConnectionsHaveSewerProfiles()
        {
            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;

            var gwswImporter = new GwswFileImporter();
            //Load sewer profiles
            gwswImporter.CsvDelimeter = ';';
            var sewerProfilesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");
            var importedProfiles = gwswImporter.ImportItem(sewerProfilesPath, model) as List<ISewerFeature>;
            Assert.IsNotEmpty(importedProfiles);

            //Check that sewer profiles have been put into the network
            var numberOfLinesInFile = File.ReadAllLines(sewerProfilesPath).Length - 1;
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(numberOfLinesInFile));

            // Now Load connections.
            var connectionsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(connectionsPath, model) as List<ISewerFeature>;
            Assert.IsNotNull(importedConnections);

            var pipes = network.Branches.OfType<Pipe>().ToList();
            Assert.IsTrue(pipes.Any());
            pipes.ForEach(p => Assert.NotNull(p.CrossSectionDefinition));

            // Check for each pipe that its CrossSectionDefinition is equal to one of the sewer profiles in
            // the SharedCrossSectionDefinitions of the network
            pipes.ForEach(p =>
            {
                var pipeCsDefinition = p.Profile;
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
            
            var model = new WaterFlowFMModel();
            var network = model.Network;

            //Load connections
            var gwswImporter = new GwswFileImporter {CsvDelimeter = ';'};
            var connectionsFilePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(connectionsFilePath, model) as List<ISewerFeature>;
            Assert.IsNotNull(importedConnections);
            Assert.IsNotEmpty(importedConnections);

            // Check the sewer profiles in the network
            var sewerProfileShapeBefore = network.Pipes.FirstOrDefault(p => p.CrossSectionDefinitionName == csdName);
            Assert.IsNotNull(sewerProfileShapeBefore);

            // Load sewer profiles
            var sewerProfilesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");
            var importedProfiles = gwswImporter.ImportItem(sewerProfilesPath, model) as List<ISewerFeature>;
            Assert.IsNotEmpty(importedProfiles);

            //Check that sewer profiles have been put into the network
            var numberOfLinesInFile = File.ReadAllLines(sewerProfilesPath).Length - 1;
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(numberOfLinesInFile));

            // Check the sewer profiles in the network
            var sewerProfileShapeAfter = (CrossSectionStandardShapeWidthHeightBase)network.Pipes.Select(p => p.Profile).FirstOrDefault(d => d.Name == csdName)?.Shape;
            Assert.IsNotNull(sewerProfileShapeAfter);
        }

        [Test]
        public void TestImportOrificesFromStructuresThenImportOrificesAssignsStructuresValues()
        {
            //Create network
            var model = new WaterFlowFMModel();
            var network = model.Network;
            var gwswImporter = new GwswFileImporter();

            //Load structures.
            var structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            var importedStructures = gwswImporter.ImportItem(structuresPath, model);
            Assert.IsNotNull(importedStructures);

            //Check placeholders have been created.
            Assert.IsTrue(network.Branches.Any());

            var orificeStructures = network.SewerConnections.SelectMany(sc => sc.GetStructuresFromBranchFeatures<Orifice>()).ToList();
            Assert.IsTrue(orificeStructures.Any());

            // Now Load connections.
            var compartmentsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var importedConnections = gwswImporter.ImportItem(compartmentsPath, model);
            Assert.IsNotNull(importedConnections);

            foreach (var orifice in orificeStructures)
            {
                var extendedOrifice = network.SewerConnections.SelectMany(sc => sc.GetStructuresFromBranchFeatures<Orifice>()).FirstOrDefault(o => o.Name.Equals(orifice.Name));
                Assert.IsNotNull(extendedOrifice);

                Assert.AreEqual(orifice.CrestLevel, extendedOrifice.CrestLevel, "the attributes from the element do not match");
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
            var gwswImporter = new GwswFileImporter();
            gwswImporter.CsvDelimeter = ';';
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
            var gwswImporter = new GwswFileImporter();
            var importedFeatures = gwswImporter.ImportItem(filePath) as List<ISewerFeature>;
            Assert.IsNotNull(importedFeatures);
            Assert.IsNotEmpty(importedFeatures);

            var fileAsStringList = File.ReadAllLines(filePath);
            var expectedElements = fileAsStringList.Length - 1; // we should not include the header
            
            Assert.That(importedFeatures.Count, Is.EqualTo(expectedElements));
            importedFeatures.ForEach(el => Assert.IsTrue(el is ICompartment));
        }

        [Test]
        public void WhenImportingCompartmentsFromGwswFilesWithTargetHydroNetwork_ThenNetworkIsCorrectlyFilledWithManholes()
        {
            var expectedManholeCount = 76;
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var model = new WaterFlowFMModel();
            var network = model.Network;

            var gwswImporter = new GwswFileImporter();
            var importedManholes = gwswImporter.ImportItem(filePath, model);
            Assert.IsNotNull(importedManholes);
            Assert.IsNotEmpty(network.Manholes);

            // Check that the amount of manholes in the network are as expected (no duplicates or whatsoever)
            var importedCompartments = importedManholes as List<ISewerFeature>;
            Assert.IsNotNull(importedCompartments);
            Assert.IsNotEmpty(importedCompartments);
            Assert.That(network.Manholes.Count(), Is.EqualTo(expectedManholeCount));

            // Check that amount of compartments in the network are the same as were imported by the importer
            var totalCompartmentsInNetwork = network.Manholes.Sum(m => m.Compartments.Count);
            Assert.That(totalCompartmentsInNetwork, Is.EqualTo(importedCompartments.Count));
        }

        [Test]
        public void Given_EmptyFlowFmModel_When_ImportingGwswDirectoryForTheFirstTime_Then_GwswAttributesDefinitionIsFilled()
        {
            var originalDirectoryPath = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            var testDirectoryPath = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDirectoryPath, testDirectoryPath);
            
            //Within the Deltares folder the previous chosen File path is saved, by deleting this folder, the Gwsw import dialog starts without a Gwsw File path.
            var deltaresDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Deltares");
            FileUtils.DeleteIfExists(deltaresDirectory);
            
            var gwswFileImporter = new GwswFileImporter();
            var viewModel = new GwswImportDialogViewModel { Importer = gwswFileImporter };
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            gwswFileImporter.LoadFeatureFiles(null);

            viewModel.SelectedDirectoryPath = testDirectoryPath;
            viewModel.OnDirectorySelected.Execute(null);

            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());
        }

        #endregion



        [Test]
        [Category(TestCategory.DataAccess)]
        public void WhenImporting2PipesAnd3ManholesFromGwswFiles_ThenCalculationPointsAreAddedToNetwork()
        {
            var originalDir = TestHelper.GetTestFilePath(@"gwswFiles\2Connection3Manholes");
            var testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var gwswImporter = new GwswFileImporter
                {
                    FilesToImport =
                    {
                        Path.Combine(testDir, "Knooppunt.csv"),
                        Path.Combine(testDir, "Verbinding.csv")
                    }
                };

                var fmModel = new WaterFlowFMModel();
                gwswImporter.ImportItem(null, fmModel);

                var discretization = fmModel.NetworkDiscretization;
                Assert.That(discretization.Locations.Values.Count, Is.EqualTo(3));

                var coords = discretization.Geometry.Coordinates;
                Assert.That(coords[0], Is.EqualTo(new Coordinate(10, 20, double.NaN)));
                Assert.That(coords[1], Is.EqualTo(new Coordinate(30, 40, double.NaN)));
                Assert.That(coords[2], Is.EqualTo(new Coordinate(30, 40, double.NaN)));
                Assert.That(coords[3], Is.EqualTo(new Coordinate(23, 99, double.NaN)));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenFmModel_WhenImportingOutletFromGwsw_ThenBoundaryConditionsAreGeneratedWithTimeSeries()
        {
            var originalDir = TestHelper.GetTestFilePath(@"gwswFiles\SimpleModelWithOutlet");
            var testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var gwswImporter = new GwswFileImporter
                {
                    FilesToImport =
                    {
                        Path.Combine(testDir, "Knooppunt.csv"),
                        Path.Combine(testDir, "Verbinding.csv"),
                        Path.Combine(testDir, "Kunstwerk.csv"),
                        Path.Combine(testDir, "Profiel.csv")
                    }
                };

                var fmModel = new WaterFlowFMModel();
                gwswImporter.ImportItem(null, fmModel);

                Assert.That(fmModel.BoundaryConditionSets.Count, Is.EqualTo(1));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }
    }
}