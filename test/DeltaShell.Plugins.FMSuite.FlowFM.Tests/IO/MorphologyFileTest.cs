using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class MorphologyFileTest
    {
        [Test]
        [TestCase("1")]
        [TestCase("2")]
        [TestCase("3")]
        public void Read_WithSedimentModelNumberBetween0And4_ThenThrowFormatException(string sedimentModelNumber)
        {
            // Setup
            var modelDefinition = new WaterFlowFMModelDefinition();
            modelDefinition.GetModelProperty(KnownProperties.SedimentModelNumber).SetValueAsString(sedimentModelNumber);

            // Call
            void Call() => MorphologyFile.Read(Arg<string>.Is.Anything, modelDefinition);

            // Assert
            Assert.Throws<FormatException>(
                Call, "Sediment model numbers 1, 2 & 3 are not supported.");
        }

        [Test]
        [TestCase("0", "C:/myFile.mor", false)]
        [TestCase("0", "", false)]
        [TestCase("4", "C:/myFile.mor", true)]
        [TestCase("4", "", false)]
        public void Read_WithSedimentModelNumberAndMorFile_ThenUseMorphologySedimentHasExpectedValue(string sedimentModelNumber, string morFilePath, bool expectedUseMorSed)
        {
            // Setup
            var modelDefinition = new WaterFlowFMModelDefinition();
            modelDefinition.GetModelProperty(KnownProperties.MorFile).SetValueAsString(morFilePath);
            modelDefinition.GetModelProperty(KnownProperties.SedimentModelNumber).SetValueAsString(sedimentModelNumber);

            // Call
            MorphologyFile.Read(Arg<string>.Is.Anything, modelDefinition);

            // Assert
            Assert.That(modelDefinition.UseMorphologySediment, Is.EqualTo(expectedUseMorSed));
        }

        [Test]
        public void GivenAnMduWithMorphologyFileWithUnknownProperties_WhenReadingAndWriting_ThenTheCorrectPropertiesAreCreatedAndCorrectlyWrittenToTheFile()
        {
            #region Load 

            // Given
            var mduFilePath = TestHelper.GetTestFilePath(@"sedmor\FlowFMCustomProperties\FlowFMCustomPropertiesSedMor.mdu");

            // When
            var importedModel = new WaterFlowFMModel(mduFilePath);

            // Then
            Assert.NotNull(importedModel, "Model was not imported.");
            var modelDefinition = importedModel.ModelDefinition;
            ValidateAllUnknownProperties(modelDefinition);

            #endregion

            #region Save

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                mduFilePath = Path.Combine(tempDir, "FlowFMWithCustomProperties.mdu");

                // When
                new MduFile().Write(mduFilePath, modelDefinition, importedModel.Area, importedModel.FixedWeirsProperties, sedimentModelData: importedModel);
                importedModel = new WaterFlowFMModel(mduFilePath);

                // Then
                Assert.NotNull(importedModel);
                modelDefinition = importedModel.ModelDefinition;
                ValidateAllUnknownProperties(modelDefinition);
            });

            #endregion
        }

        /// <summary>
        /// GIVEN a morphology file with case insensitive properties
        /// WHEN Reading
        /// THEN no unknown properties are given
        /// </summary>
        [Test]
        public void GivenAMorphologyFileWithCaseInsensitiveProperties_WhenReading_ThenNoUnknownPropertiesAreGiven()
        {
            // Given
            string mduFilePath =
                TestHelper.GetTestFilePath(@"sedmor\FlowFMCaseInsensitiveProperties\FlowFMCaseInsensitivePropertiesSedMor.mdu");
            var modelDefinition = new WaterFlowFMModelDefinition();
            modelDefinition.GetModelProperty(KnownProperties.MorFile).Value = "MorCaseInsensitiveProperties.mor";

            // When
            List<string> logMessages = TestHelper.GetAllRenderedMessages(
                                                     () => MorphologyFile.Read(mduFilePath, modelDefinition),
                                                     Level.Warn)
                                                 .ToList();

            // Then
            Assert.AreEqual(0, logMessages.Count, "No warning messages were expected to be generated.");

            IEventedList<WaterFlowFMProperty> properties = modelDefinition.Properties;
            List<WaterFlowFMProperty> propertiesMorphologyCategory =
                properties.Where(p => p.PropertyDefinition.Category.Equals(MorphologyFile.Header)
                                   && p.PropertyDefinition.UnknownPropertySource.Equals(PropertySource.MorphologyFile))
                          .ToList();


            Assert.AreEqual(0, propertiesMorphologyCategory.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAMorphologyFileWithUnknownProperties_WhenReading_ThenTheCorrectWarningsAreGiven()
        {
            // Given
            string mduFilePath =
                TestHelper.GetTestFilePath(@"sedmor\FlowFMCustomProperties\FlowFMCustomPropertiesSedMor.mdu");
            var modelDefinition = new WaterFlowFMModelDefinition();
            modelDefinition.GetModelProperty(KnownProperties.MorFile).Value = "MorCustomProperties.mor";
            modelDefinition.GetModelProperty(KnownProperties.SedimentModelNumber).SetValueAsString("4");

            // When
            List<string> logMessages = TestHelper.GetAllRenderedMessages(
                                                     () => MorphologyFile.Read(mduFilePath, modelDefinition),
                                                     Level.Warn)
                                                 .ToList();

            // Then
            Assert.AreEqual(1, logMessages.Count, "One grouped warning message was expected to be generated.");
            AssertMessageContainsWarningForEachUnknownProperty(logMessages[0]);
        }

        private static void ValidateAllUnknownProperties(WaterFlowFMModelDefinition modelDefinition)
        {
            IEventedList<WaterFlowFMProperty> properties = modelDefinition.Properties;

            List<WaterFlowFMProperty> propertiesMorphologyCategory = 
                properties.Where(p => p.PropertyDefinition.Category.Equals(MorphologyFile.Header)
                                   && p.PropertyDefinition.UnknownPropertySource.Equals(PropertySource.MorphologyFile))
                          .ToList();

            Assert.AreEqual(4, propertiesMorphologyCategory.Count);
            ValidatePropertiesCategory(propertiesMorphologyCategory, MorphologyFile.Header, KnownProperties.morphology);

            const string customCategoryName = "CustomCategory";
            List<WaterFlowFMProperty> propertiesUnknownCategory = 
                properties.Where(p => p.PropertyDefinition.FileCategoryName.Equals(customCategoryName)).ToList();
            ValidatePropertiesCategory(propertiesUnknownCategory, customCategoryName, customCategoryName);
        }

        private static void ValidatePropertiesCategory(List<WaterFlowFMProperty> properties, 
                                                       string categoryName,
                                                       string fileCategoryName)
        {
            Assert.IsTrue(properties.All(p => p.PropertyDefinition.UnknownPropertySource.Equals(PropertySource.MorphologyFile)));
            Assert.IsTrue(properties.All(p => p.PropertyDefinition.FileCategoryName.Equals(fileCategoryName)));
            Assert.IsTrue(properties.All(p => p.PropertyDefinition.Category.Equals(categoryName)));

            ValidateProperty(properties, "CustomStringProp", "\"777\"");
            ValidateProperty(properties, "CustomBoolProp", "1");
            ValidateProperty(properties, "CustomDoubleProp", "7.77");
            ValidateProperty(properties, "CustomIntProp", "777");
        }

        private static void ValidateProperty(List<WaterFlowFMProperty> properties, string propertyName, string propertyValue)
        {
            var customStringProperty = properties.FirstOrDefault(p => p.PropertyDefinition.FilePropertyName.Equals(propertyName));
            Assert.NotNull(customStringProperty);
            Assert.AreEqual(propertyValue, customStringProperty.Value);
        }

        [Test]
        public void GivenAModelDefinitionWithTwoUnknownPropertiesAndOneUnknownCategory_WhenWriting_ThenExpectedLinesAreWrittenToFile()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Given
                string morFilePath = Path.Combine(tempDir.Path, "morfile.mor");
                const string customCategoryName = "custom_category";
                const string customPropertyName = "custom_property";
                const string value1 = "123";
                const string value2 = "456";

                WaterFlowFMModelDefinition modelDefinition = 
                    CreateModelDefinitionWithCustomCategoryAndProperties(customPropertyName, customCategoryName, value1, value2);

                // When
                MorphologyFile.Save(morFilePath, modelDefinition);

                // Then
                string[] lines = File.ReadAllLines(morFilePath);
                Assert.AreEqual($"[{MorphologyFile.Header}]", lines[4]);
                Assert.AreEqual($"    {customPropertyName}       = {value1}                    ", lines[34]);
                Assert.AreEqual($"[{customCategoryName}]", lines[35]);
                Assert.AreEqual($"    {customPropertyName}       = {value2}                    ", lines[36]);
            }
        }

        private static WaterFlowFMModelDefinition CreateModelDefinitionWithCustomCategoryAndProperties(string customPropertyName, string customCategoryName, string value1, string value2)
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            WaterFlowFMPropertyDefinition propertyDefinitionMorphologyCategory =
                WaterFlowFMPropertyDefinitionCreator.CreateForCustomProperty(KnownProperties.morphology,
                                                                              customPropertyName,
                                                                              "",
                                                                              PropertySource.MorphologyFile);

            WaterFlowFMPropertyDefinition propertyDefinitionCustomCategory =
                WaterFlowFMPropertyDefinitionCreator.CreateForCustomProperty(customCategoryName,
                                                                              customPropertyName,
                                                                              "",
                                                                              PropertySource.MorphologyFile);

            var customPropertyMorphologyCategory = new WaterFlowFMProperty(propertyDefinitionMorphologyCategory, value1);
            var customPropertyCustomCategory = new WaterFlowFMProperty(propertyDefinitionCustomCategory, value2);

            modelDefinition.AddProperty(customPropertyMorphologyCategory);
            modelDefinition.AddProperty(customPropertyCustomCategory);

            return modelDefinition;
        }

        [Test]
        public void SaveMorWithBoundaryConditionsFile()
        {
            string morFile = Path.GetTempFileName();
            try
            {
                var modelDefinition = new WaterFlowFMModelDefinition();
                modelDefinition.ModelName = "myModelName";

                var boundary = new Feature2D
                {
                    Name = "Boundary1",
                    Geometry = new LineString(Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0 * i)).ToArray())
                };

                modelDefinition.Boundaries.AddRange(new[] { boundary });
                modelDefinition.BoundaryConditionSets.AddRange(new[]
                {
                    new BoundaryConditionSet {Feature = boundary}
                });

                var morbc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed,
                                                       BoundaryConditionDataType.TimeSeries)
                {
                    Feature = modelDefinition.Boundaries[0]
                };

                var startTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
                var stopTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StopTime).Value;

                morbc1.AddPoint(0);
                var data = morbc1.GetDataAtPoint(0);
                FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 7);

                modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] { morbc1 });

                WaterFlowFMPropertyDefinition def =
                    WaterFlowFMPropertyDefinitionCreator.CreateForCustomProperty(KnownProperties.morphology,
                                                                                  "myprop",
                                                                                  string.Empty);
                var prop = new WaterFlowFMProperty(def, "801");
                modelDefinition.AddProperty(prop);

                MorphologyFile.Save(morFile, modelDefinition);

                var morWritten = File.ReadAllText(morFile);
                Assert.That(morWritten, Is.StringContaining("[" + MorphologyFile.GeneralHeader + "]"));
                Assert.That(morWritten, Is.StringContaining("[" + MorphologyFile.Header + "]"));
                Assert.That(morWritten, Is.StringContaining("myprop"));
                Assert.That(morWritten, Is.StringContaining("801"));
                Assert.That(morWritten, Is.StringContaining(MorphologyFile.BcFile));
                Assert.That(morWritten, Is.StringContaining(modelDefinition.ModelName + BcmFile.Extension));
                Assert.That(morWritten, Is.StringContaining(modelDefinition.ModelName + BcmFile.Extension));
                Assert.That(morWritten, Is.StringContaining("[" + MorphologyFile.BoundaryHeader + "]"));
                Assert.That(morWritten, Is.StringContaining(MorphologyFile.BoundaryName));
                Assert.That(morWritten, Is.StringContaining(boundary.Name));
                Assert.That(morWritten, Is.StringContaining(MorphologyFile.BoundaryBedCondition));
                Assert.That(morWritten, Is.StringContaining("= " + (int)BoundaryConditionQuantityTypeConverter.ConvertFlowBoundaryConditionQuantityTypeToMorphologyBoundaryConditionQuantityType(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)));


            }
            finally
            {
                FileUtils.DeleteIfExists(morFile);
            }
        }

        [Test]
        public void SaveLoadMorWithBoundaryConditionsFile()
        {
            string morFile = Path.GetTempFileName();
            try
            {
                var modelDefinition = new WaterFlowFMModelDefinition();
                modelDefinition.ModelName = "myModelName";

                var boundary = new Feature2D
                {
                    Name = "Boundary1",
                    Geometry = new LineString(
                        Enumerable.Range(0, 10).Select(i => new Coordinate(0, 10.0 * i)).ToArray())
                };
                modelDefinition.Boundaries.AddRange(new[] {boundary});
                modelDefinition.BoundaryConditionSets.AddRange(new[]
                {
                    new BoundaryConditionSet {Feature = boundary}
                });

                var morbc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed,
                        BoundaryConditionDataType.TimeSeries)
                    {Feature = modelDefinition.Boundaries[0]};
                var startTime = (DateTime) modelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
                var stopTime = (DateTime) modelDefinition.GetModelProperty(GuiProperties.StopTime).Value;

                morbc1.AddPoint(0);
                var data = morbc1.GetDataAtPoint(0);
                FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 7);

                modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] {morbc1});

                var def = WaterFlowFMPropertyDefinitionCreator.CreateForCustomProperty(KnownProperties.morphology,
                                                                                        "myprop", string.Empty);
                var prop = new WaterFlowFMProperty(def, "801");
                modelDefinition.AddProperty(prop);

                MorphologyFile.Save(morFile, modelDefinition);

                /* Write pli file seperately, is not responsibility of MorphologyFile */
                var bndExtForceFile = new BndExtForceFile {WriteToDisk = true};
                TypeUtils.SetField(bndExtForceFile, "bndExtFilePath", morFile);
                TypeUtils.CallPrivateMethod(bndExtForceFile, "WritePolyLines", modelDefinition.BoundaryConditionSets);

                /* Write bcm file seperately, is not responsibility of MorphologyFile */
                var bcmFile = new BcmFile();
                var bcmFileName = Path.Combine(Path.GetDirectoryName(morFile), modelDefinition.ModelName + BcmFile.Extension);
                bcmFile.Write(modelDefinition.BoundaryConditionSets, bcmFileName, new BcmFileFlowBoundaryDataBuilder());

                var newDefinition = new WaterFlowFMModelDefinition();
                newDefinition.GetModelProperty(KnownProperties.MorFile).Value = morFile;
                newDefinition.GetModelProperty(KnownProperties.SedimentModelNumber).SetValueAsString("4");
                MorphologyFile.Read(morFile, newDefinition);
                var readBoundaryConditionSet = newDefinition.BoundaryConditionSets.FirstOrDefault();
                Assert.IsNotNull(readBoundaryConditionSet);
                Assert.That(boundary.Name, Is.EqualTo(readBoundaryConditionSet.Feature.Name));
                var readMorbc = readBoundaryConditionSet.BoundaryConditions.FirstOrDefault() as FlowBoundaryCondition;
                Assert.IsNotNull(readMorbc);
                Assert.That(morbc1.FlowQuantity, Is.EqualTo(readMorbc.FlowQuantity));
                Assert.That(morbc1.DataType, Is.EqualTo(readMorbc.DataType));
                var readData = readMorbc.GetDataAtPoint(0);
                var valuesBefore = data.Components[0].GetValues<double>();
                var valuesRead = readData.Components[0].GetValues<double>();
                Assert.That(valuesBefore.Count, Is.EqualTo(valuesRead.Count));
                for (int i = 0; i < valuesBefore.Count; i++)
                {
                    Assert.That(valuesBefore[i], Is.EqualTo(valuesRead[i]).Within(0.000001));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(morFile);
            }
        }

        private static void FillTimeSeries(IFunction function, Func<int, double> mapping, DateTime start, DateTime stop, int steps)
        {
            TimeSpan deltaT = stop - start;
            IEnumerable<DateTime> times = Enumerable.Range(0, steps).Select(i => start + new TimeSpan(i * deltaT.Ticks));
            IEnumerable<double> values = Enumerable.Range(0, steps).Select(mapping);
            FunctionHelper.SetValuesRaw(function.Arguments[0], times);
            FunctionHelper.SetValuesRaw(function.Components[0], values);
        }

        private static void AssertMessageContainsWarningForEachUnknownProperty(string message)
        {
            AssertMessageContainsWarningForProperty(message, "CustomStringProp", 35);
            AssertMessageContainsWarningForProperty(message, "CustomBoolProp", 36);
            AssertMessageContainsWarningForProperty(message, "CustomDoubleProp", 37);
            AssertMessageContainsWarningForProperty(message, "CustomIntProp", 38);

            AssertMessageContainsWarningForProperty(message, "CustomStringProp", 40);
            AssertMessageContainsWarningForProperty(message, "CustomBoolProp", 41);
            AssertMessageContainsWarningForProperty(message, "CustomDoubleProp", 42);
            AssertMessageContainsWarningForProperty(message, "CustomIntProp", 43);

        }

        private static void AssertMessageContainsWarningForProperty(string message, string propertyName, int lineNumber)
        {
            string expectedMessage = string.Format(
                Resources.MorphologySediment_ReadCategoryProperties_Unsupported_keyword___0___at_line___1___detected_and_will_be_passed_to_the_computational_core__Note_that_some_data_or_the_connection_to_linked_files_may_be_lost_,
                propertyName, 
                lineNumber);
            Assert.IsTrue(message.Contains(expectedMessage), $"The following warning is missing: <{expectedMessage}>");
        }
    }
}