using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.ExtForce;
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
            var importSamplesOperation = new ImportSamplesOperationImportData
            {
                FilePath = Path.GetFullPath(extForceFilePath + fileName),
            };
            ExtForceData item = ExtForceFileHelper.WriteInitialConditionsSamples(extForceFilePath, "quantity", importSamplesOperation, null, true);
            Assert.AreEqual(expectedFileName, item.FileName);
        }

        [Test]
        public void TestReadSourceAndSinkData()
        {
            string testFilePath = TestHelper.GetTestFilePath(@"timFiles\10Columns10Values.tim");

            // setup
            var feature = new Feature2D();
            var extForceFileItem = new ExtForceData { Quantity = ExtForceQuantNames.SourceAndSink };

            // do the import
            SourceAndSink sourceAndSink = ExtForceFileHelper.ReadSourceAndSinkData(testFilePath, feature, extForceFileItem, DateTime.Now);

            // check results
            List<KeyValuePair<string, object>> sourceAndSinkAttributes = sourceAndSink.Feature.Attributes.Where(a => a.Key.StartsWith(SourceAndSinkImportExtensions.TimFileColumnAttributePrefix)).ToList();
            Assert.AreEqual(10, sourceAndSinkAttributes.Count);
            Assert.True(sourceAndSinkAttributes.Select(a => a.Value).OfType<MultiDimensionalArray<double>>().All(v => v.Count == 10));
        }

        [Test]
        public void TestReadSourceAndSinkValues_HandlesNullFunction()
        {
            var sourceAndSink = new SourceAndSink { Data = null };
            var arguments = new object[] { sourceAndSink, string.Empty, DateTime.MinValue };
            var expectedError = string.Format(Resources.Read_SourceAndSink_values_failed__no_function_detected_for_SourceAndSink__0_, sourceAndSink.Name);

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                TypeUtils.CallPrivateStaticMethod(typeof(ExtForceFileHelper), "ReadSourceAndSinkValues", arguments );
            }, 
            expectedError);
        }

        [TestCase(false, HeatFluxModelType.None, "NoSalinityOrTemperature.tim")]
        [TestCase(true, HeatFluxModelType.None, "SalinityOnly.tim")]
        [TestCase(false, HeatFluxModelType.TransportOnly, "TemperatureOnly.tim")]
        [TestCase(true, HeatFluxModelType.TransportOnly, "BothSalinityAndTemperature.tim")]
        public void TestWriteSourceAndSinkData(bool useSalinity, HeatFluxModelType temperature, string fileName)
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
            fmModel.ReferenceTime = new DateTime(2001, 1, 1, 0, 0, 0, 0);
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            var modelDefinition = fmModel.ModelDefinition;
            var useSalinityProperty = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            Assert.NotNull(useSalinityProperty);
            useSalinityProperty.Value = useSalinity;

            var temperatureProperty = modelDefinition.GetModelProperty(KnownProperties.Temperature) ;
            Assert.NotNull(temperatureProperty);
            temperatureProperty.SetValueFromString(((int)temperature).ToString());

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

            var extForceFileItem = new ExtForceData
            {
                Quantity = ExtForceQuantNames.SourceAndSink,
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
            var extForceFileItem = new ExtForceData { Quantity = ExtForceQuantNames.SourceAndSink, FileName = "test.pli" };

            // do the export
            ExtForceFileHelper.WriteSourceAndSinkData(exportedFile, sourceAndSink, fmModel.ReferenceTime, extForceFileItem, true, modelDefinition);
        }

        [Test]
        public void GetPliFileNameReturnsNullIfFeatureDoesNotHaveName()
        {
            var featureData = new SourceAndSink
            {
                Feature = new Feature2D { Geometry = new Point(0.0, 0.0) },
                Data = null
            };
            Assert.IsTrue(string.IsNullOrEmpty(featureData.Feature.Name));
            Assert.IsNull(ExtForceFileHelper.GetPliFileName(featureData));
        }
    }
}