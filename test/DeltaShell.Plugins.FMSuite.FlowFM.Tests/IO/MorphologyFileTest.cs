using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class MorphologyFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnMduWithMorphologyFileWithUnknownProperties_WhenReadingAndWriting_ThenTheCorrectPropertiesAreCreatedAndCorrectlyWrittenToTheFile()
        {
            #region Load 

            // Given
            var mduFilePath = TestHelper.GetTestFilePath(@"sedmor\FlowFMCustomProperties\FlowFMCustomPropertiesSedMor.mdu");

            // When
            var importedModel = new WaterFlowFMModel.WaterFlowFMModel(mduFilePath);

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
                importedModel = new WaterFlowFMModel.WaterFlowFMModel(mduFilePath);

                // Then
                Assert.NotNull(importedModel);
                modelDefinition = importedModel.ModelDefinition;
                ValidateAllUnknownProperties(modelDefinition);
            });

            #endregion
        }

        private static void ValidateAllUnknownProperties(WaterFlowFMModelDefinition modelDefinition)
        {
            var properties = modelDefinition.Properties;

            var propertiesMorphologyCategory = properties.Where(p => p.PropertyDefinition.Category.Equals(MorphologyFile.Header)
                                                                     && p.PropertyDefinition.UnknownPropertySource.Equals(PropertySource.MorphologyFile))
                                                         .ToList();
            Assert.AreEqual(4, propertiesMorphologyCategory.Count);
            ValidatePropertiesCategory(propertiesMorphologyCategory, MorphologyFile.Header);

            const string customCategoryName = "CustomCategory";
            var propertiesUnknownCategory = properties.Where(p => p.PropertyDefinition.FileCategoryName.Equals(customCategoryName)).ToList();
            ValidatePropertiesCategory(propertiesUnknownCategory, customCategoryName);
        }

        private static void ValidatePropertiesCategory(List<WaterFlowFMProperty> properties, string categoryName)
        {
            Assert.IsTrue(properties.All(p => p.PropertyDefinition.UnknownPropertySource.Equals(PropertySource.MorphologyFile)));
            Assert.IsTrue(properties.All(p => p.PropertyDefinition.FileCategoryName.Equals(categoryName)));
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
        [Category(TestCategory.DataAccess)]
        public void GivenAModelDefinitionWithTwoUnknownPropertiesAndOneUnknownCategory_WhenWriting_ThenExpectedLinesAreWrittenToFile()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                var morFilePath = Path.Combine(tempDir, "morfile.mor");
                var customCategoryName = "custom_category";
                var customPropertyName = "custom_property";
                var value1 = "123";
                var value2 = "456";

                var modelDefinition = CreateModelDefinitionWithCustomCategoryAndProperties(customPropertyName, customCategoryName, value1, value2);

                // When
                MorphologyFile.Save(morFilePath, modelDefinition);

                // Then
                var lines = File.ReadAllLines(morFilePath);
                Assert.AreEqual($"[{MorphologyFile.Header}]", lines[4]);
                Assert.AreEqual($"    {customPropertyName}       = {value1}                    ", lines[34]);
                Assert.AreEqual($"[{customCategoryName}]", lines[35]);
                Assert.AreEqual($"    {customPropertyName}       = {value2}                    ", lines[36]);
            });
        }

        private static WaterFlowFMModelDefinition CreateModelDefinitionWithCustomCategoryAndProperties(string customPropertyName, string customCategoryName, string value1, string value2)
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var propertyDefinitionMorphologyCategory = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(
                KnownProperties.morphology,
                customPropertyName,
                "",
                PropertySource.MorphologyFile);

            var propertyDefinitionCustomCategory = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(
                customCategoryName,
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
            var morFile = Path.GetTempFileName();
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
                    { Feature = modelDefinition.Boundaries[0] };
                var startTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
                var stopTime = (DateTime)modelDefinition.GetModelProperty(GuiProperties.StopTime).Value;

                morbc1.AddPoint(0);
                var data = morbc1.GetDataAtPoint(0);
                FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 7);

                modelDefinition.BoundaryConditionSets[0].BoundaryConditions.AddRange(new[] { morbc1 });

                var def = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(KnownProperties.morphology,
                    "myprop", string.Empty);
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
            var morFile = Path.GetTempFileName();
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

                var def = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(KnownProperties.morphology,
                    "myprop", string.Empty);
                var prop = new WaterFlowFMProperty(def, "801");
                modelDefinition.AddProperty(prop);

                MorphologyFile.Save(morFile, modelDefinition);

                /* Write pli file seperately, is not responsibility of MorphologyFile */
                var bndExtForceFile = new BndExtForceFile {WriteToDisk = true};
                TypeUtils.SetField(bndExtForceFile, "filePath", morFile);
                TypeUtils.CallPrivateMethod(bndExtForceFile, "WritePolyLines", modelDefinition.BoundaryConditionSets);

                /* Write bcm file seperately, is not responsibility of MorphologyFile */
                var bcmFile = new BcmFile();
                var bcmFileName = Path.Combine(Path.GetDirectoryName(morFile), modelDefinition.ModelName + BcmFile.Extension);
                bcmFile.Write(modelDefinition.BoundaryConditionSets, bcmFileName, new BcmFileFlowBoundaryDataBuilder());

                var newDefinition = new WaterFlowFMModelDefinition();
                newDefinition.GetModelProperty(KnownProperties.MorFile).Value = morFile;
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
            var deltaT = stop - start;
            var times = Enumerable.Range(0, steps).Select(i => start + new TimeSpan(i * deltaT.Ticks));
            var values = Enumerable.Range(0, steps).Select(mapping);
            FunctionHelper.SetValuesRaw(function.Arguments[0], times);
            FunctionHelper.SetValuesRaw(function.Components[0], values);
        }
    }
}