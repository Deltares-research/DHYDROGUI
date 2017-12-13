using System;
using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class GwswBaseImporterTest: GwswImporterTestHelper
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
                LineNumber = lineNumber,
                LocalKey = localKey,
                Key = key
            };

            var invalidAttribute = new GwswAttribute
            {
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

        #region Test helpers

        private static void CheckThatGwswAttributeValidationLogMessageIsReturned(string fileName, int lineNumber,
            string localKey, string key, GwswAttribute invalidAttribute)
        {
            var expectedMessage = string.Format(
                Resources.GwswElementExtensions_LogInvalidAttribute_File__0___line__1___Column__2____3___contains_invalid_value___4___and_will_not_be_imported_
                , fileName, lineNumber, localKey, key, string.Empty);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => invalidAttribute.IsValidAttribute(), expectedMessage);
        }

        #endregion
    }
}