using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ExtForceFileHelperTest
    {
        [TestCase("C:\\Folder\\AnotherFolder\\MoreFolder\\HW1995", "\\filename_something.xyz", "HW1995\\filename_something.xyz")]
        [TestCase("C:\\Folder\\AnotherFolder\\MoreFolder\\HW1995","\\YesThereIsAnotherFolder\\filename_something.xyz", "HW1995\\YesThereIsAnotherFolder\\filename_something.xyz")]
        public void WriteInitialConditionsSamplesTest(string extForceFilePath, string fileName, string expectedFileName)
        {
            var importSamplesOperation = new ImportSamplesSpatialOperationExtension
            {
                                FilePath = Path.GetFullPath(extForceFilePath + fileName),
            };
            ExtForceFileItem item = ExtForceFileHelper.WriteInitialConditionsSamples(extForceFilePath, "quantity", importSamplesOperation, null, true);
            Assert.AreEqual(expectedFileName, item.FileName);
        }

        [TestCase(false, false)] // None
        [TestCase(true, false)] // Salinity only
        [TestCase(false, true)] // Temperature only
        [TestCase(true, true)] // Both
        public void TestReadSourceAndSinkData(bool useSalinity, bool useTemperature)
        {
            var testFilePath = TestHelper.GetTestFilePath(@"timFiles\testFile.tim");
            
            // setup
            var feature = new Feature2D();
            var extForceFileItem = new ExtForceFileItem(ExtForceQuantNames.SourceAndSink);
            var modelDefinition = new WaterFlowFMModelDefinition();

            var useSalinityProperty = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            Assert.NotNull(useSalinityProperty);
            useSalinityProperty.Value = useSalinity;

            var useTemperatureProperty = modelDefinition.GetModelProperty(GuiProperties.UseTemperature);
            Assert.NotNull(useTemperatureProperty);
            useTemperatureProperty.Value = useTemperature;

            // do the import
            var sourceAndSink = ExtForceFileHelper.ReadSourceAndSinkData(testFilePath, feature, extForceFileItem, DateTime.Now, modelDefinition);

            // check results
            var function = sourceAndSink.Function;
            var dischargeVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.DischargeVariableName);
            Assert.NotNull(dischargeVariable);

            var dischargeValues = ((MultiDimensionalArray<double>)dischargeVariable.Values).ToList();
            Assert.IsTrue(dischargeValues.All(v => v >= double.Epsilon));

            var salinityVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.SalinityVariableName);
            Assert.NotNull(salinityVariable);

            var salinityValues = ((MultiDimensionalArray<double>)salinityVariable.Values).ToList();
            Assert.AreEqual(useSalinity, salinityValues.All(v => v >= double.Epsilon));
            Assert.AreEqual(!useSalinity, salinityValues.All(v => v < double.Epsilon));

            var temperatureVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.TemperatureVariableName);
            Assert.NotNull(temperatureVariable);

            var temperatureValues = ((MultiDimensionalArray<double>)temperatureVariable.Values).ToList();
            Assert.AreEqual(useTemperature, temperatureValues.All(v => v >= double.Epsilon));
            Assert.AreEqual(!useTemperature, temperatureValues.All(v => v < double.Epsilon));
        }

        [Test]
        public void TestReadSourceAndSinkValues_HandlesNullFunction()
        {
            var sourceAndSink = new SourceAndSink { Data = null };
            var arguments = new object[] { sourceAndSink, string.Empty, DateTime.MinValue, new WaterFlowFMModelDefinition() };
            var expectedError = string.Format(Resources.Read_SourceAndSink_values_failed__no_function_detected_for_SourceAndSink__0_, sourceAndSink.Name);

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                TypeUtils.CallPrivateStaticMethod(typeof(ExtForceFileHelper), "ReadSourceAndSinkValues", arguments );
            }, 
            expectedError);
        }

        [Test]
        public void TestReadSourceAndSinkValues_HandlesNullComponent()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"timFiles\testFile.tim");

            var sourceAndSink = new SourceAndSink();
            // Remove temperatureComponent
            sourceAndSink.Function.RemoveComponentByName(SourceAndSink.TemperatureVariableName);

            var modelDefinition = new WaterFlowFMModelDefinition();

            var useSalinityProperty = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            Assert.NotNull(useSalinityProperty);
            useSalinityProperty.Value = true;

            var useTemperatureProperty = modelDefinition.GetModelProperty(GuiProperties.UseTemperature);
            Assert.NotNull(useTemperatureProperty);
            useTemperatureProperty.Value = true;

            var arguments = new object[] { sourceAndSink, testFilePath, DateTime.Now, modelDefinition };
            var expectedError = string.Format(Resources.Read_SourceAndSink_values_failed__could_not_determine_component_values_for_SourceAndSink__0_, sourceAndSink.Name);

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                TypeUtils.CallPrivateStaticMethod(typeof(ExtForceFileHelper), "ReadSourceAndSinkValues", arguments);
            },
            expectedError);
        }

        [TestCase(false, false, "NoSalinityOrTemperature.tim")]
        [TestCase(true, false, "SalinityOnly.tim")]
        [TestCase(false, true, "TemperatureOnly.tim")]
        [TestCase(true, true, "BothSalinityAndTemperature.tim")]
        public void TestWriteSourceAndSinkData(bool useSalinity, bool useTemperature, string fileName)
        {
            var expectedFile = TestHelper.GetTestFilePath(@"timFiles\" + fileName);

            // setup
            var sourceAndSink = new SourceAndSink
            {
                Feature = new Feature2D
                {
                    Geometry = new Point(0.0, 0.0)
                }
            };

            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            var modelDefinition = fmModel.ModelDefinition;
            var useSalinityProperty = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            Assert.NotNull(useSalinityProperty);
            useSalinityProperty.Value = useSalinity;

            var useTemperatureProperty = modelDefinition.GetModelProperty(GuiProperties.UseTemperature);
            Assert.NotNull(useTemperatureProperty);
            useTemperatureProperty.Value = useTemperature;

            var function = sourceAndSink.Function;

            var timeVariable = function.Arguments.FirstOrDefault(c => c.Name == SourceAndSink.TimeVariableName);
            Assert.NotNull(timeVariable);
            var timeIndex0 = new DateTime(1984, 11, 11, 11, 11, 11, DateTimeKind.Utc);
            timeVariable.Values.AddRange(new List<DateTime> { timeIndex0, timeIndex0.AddMinutes(10), timeIndex0.AddMinutes(20) });

            var dischargeVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.DischargeVariableName);
            Assert.NotNull(dischargeVariable);
            dischargeVariable.Values.Clear();
            dischargeVariable.Values.AddRange(new List<double> { 123.456, 234.567, 345.678 });

            var salinityVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.SalinityVariableName);
            Assert.NotNull(salinityVariable);
            salinityVariable.Values.Clear();
            salinityVariable.Values.AddRange(new List<double> { 123.456, 234.567, 345.678 });

            var temperatureVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.TemperatureVariableName);
            Assert.NotNull(temperatureVariable);
            temperatureVariable.Values.Clear();
            temperatureVariable.Values.AddRange(new List<double> { 123.456, 234.567, 345.678 });

            var exportedFile = Path.Combine(FileUtils.CreateTempDirectory(), fileName);
            FileUtils.DeleteIfExists(exportedFile);

            var extForceFileItem = new ExtForceFileItem(ExtForceQuantNames.SourceAndSink)
            {
                FileName = fileName.Replace(".tim", ".pli")
            };

            // do the export
            ExtForceFileHelper.WriteSourceAndSinkData(exportedFile, sourceAndSink, fmModel.ReferenceTime, extForceFileItem, true, modelDefinition);

            // check results
            Assert.IsTrue(FileUtils.FilesAreEqual(expectedFile, exportedFile));
            FileUtils.DeleteIfExists(exportedFile);
        }

        [Test]
        public void TestWriteSourceAndSinkData_HandlesNullFunction()
        {
            // setup
            var sourceAndSink = new SourceAndSink
            {
                Feature = new Feature2D { Geometry = new Point(0.0, 0.0) },
                Data = null
            };

            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);
            var modelDefinition = fmModel.ModelDefinition;

            var exportedFile = Path.Combine(FileUtils.CreateTempDirectory(), "test.tim");
            FileUtils.DeleteIfExists(exportedFile);
            var extForceFileItem = new ExtForceFileItem(ExtForceQuantNames.SourceAndSink) { FileName = "test.pli" };

            // do the export
            ExtForceFileHelper.WriteSourceAndSinkData(exportedFile, sourceAndSink, fmModel.ReferenceTime, extForceFileItem, true, modelDefinition);
        }
    }
}