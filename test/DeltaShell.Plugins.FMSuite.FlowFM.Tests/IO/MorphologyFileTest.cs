using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class MorphologyFileTest
    {
        [Test]
        public void LoadAndSaveMorFlowFMWithCustomProperties()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"sedmor\FlowFMCustomProperties\FlowFMCustomPropertiesSedMor.mdu");
            var flowFM = new WaterFlowFMModel(mduFilePath);
            Assert.NotNull(flowFM);
            TestMorphologyContainsAllUnknownProperties(flowFM.ModelDefinition);

            /* Check if properties have been written again. */
            var mduFile = new MduFile();
            const string saveToDir = "LoadAndSaveMorFlowFM";
            Directory.CreateDirectory(saveToDir);
            var mduFileSaveToPath = Path.Combine(saveToDir, "FlowFMWithCustomProperties.mdu");
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFileSaveToPath, flowFM.ModelDefinition, flowFM.Area, null, null,null, null,null, null, allFixedWeirsAndCorrespondingProperties);

            /* Check if properties have been written again. */
            var newFlowFM = new WaterFlowFMModel(mduFileSaveToPath);
            Assert.NotNull(newFlowFM);
            var newModelDefinition = flowFM.ModelDefinition;
            TestMorphologyContainsAllUnknownProperties(newModelDefinition);
        }

        private static void TestMorphologyContainsAllUnknownProperties(WaterFlowFMModelDefinition modelDefinition)
        {
            Assert.NotNull(modelDefinition);

            /*Test check if model contains custom (unknown) properties */
            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(MorphologyFile.MorphologyUnknownProperty) &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomStringProp") &&
                     p.Value.Equals("\"123\"")));
            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(MorphologyFile.MorphologyUnknownProperty) &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomBoolProp") &&
                     p.Value.Equals("1")));
            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(MorphologyFile.MorphologyUnknownProperty) &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomDoubleProp") &&
                     p.Value.Equals("1.23")));
            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(MorphologyFile.MorphologyUnknownProperty) &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomIntProp") &&
                     p.Value.Equals("123")));
        }

        [Test]
        public void SaveMorFile()
        {
            var morFile = Path.GetTempFileName();
            try
            {
                var modelDefinition = new WaterFlowFMModelDefinition();
                var def = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(KnownProperties.morphology,
                    "myprop", string.Empty);
                var prop = new WaterFlowFMProperty(def, "801");
                modelDefinition.AddProperty(prop);

                MorphologyFile.Save(morFile, modelDefinition);
                var morWritten = File.ReadAllText(morFile);
                Assert.IsTrue(morWritten.Contains(MorphologyFile.GeneralHeader));
                Assert.IsTrue(morWritten.Contains(MorphologyFile.Header));
                Assert.IsTrue(morWritten.Contains("myprop"));
                Assert.IsTrue(morWritten.Contains("801"));
            }
            finally
            {
                FileUtils.DeleteIfExists(morFile);
            }
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
                Assert.IsTrue(morWritten.Contains("[" + MorphologyFile.GeneralHeader + "]"));
                Assert.IsTrue(morWritten.Contains("[" + MorphologyFile.Header + "]"));
                Assert.IsTrue(morWritten.Contains("myprop"));
                Assert.IsTrue(morWritten.Contains("801"));
                Assert.IsTrue(morWritten.Contains(MorphologyFile.BcFile));
                Assert.IsTrue(morWritten.Contains(modelDefinition.ModelName + BcmFile.Extension));
                Assert.IsTrue(morWritten.Contains(modelDefinition.ModelName + BcmFile.Extension));
                Assert.IsTrue(morWritten.Contains("[" + MorphologyFile.BoundaryHeader + "]"));
                Assert.IsTrue(morWritten.Contains(MorphologyFile.BoundaryName));
                Assert.IsTrue(morWritten.Contains(boundary.Name));
                Assert.IsTrue(morWritten.Contains(MorphologyFile.BoundaryBedCondition));
                Assert.IsTrue(morWritten.Contains("= " + (int)BoundaryConditionQuantityTypeConverter.ConvertFlowBoundaryConditionQuantityTypeToMorphologyBoundaryConditionQuantityType(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)));


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