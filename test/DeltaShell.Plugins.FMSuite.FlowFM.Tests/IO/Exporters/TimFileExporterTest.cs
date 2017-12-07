using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class TimFileExporterTest
    {
        [TestCase(false, false, "NoSalinityOrTemperature.tim")]
        [TestCase(true, false, "SalinityOnly.tim")]
        [TestCase(false, true, "TemperatureOnly.tim")]
        [TestCase(true, true, "BothSalinityAndTemperature.tim")]
        public void TestExport_SourceAndSinks(bool useSalinity, bool useTemperature, string fileName)
        {
            var expectedFile = TestHelper.GetTestFilePath(@"timFiles\" + fileName);

            // setup
            var sourceAndSink = new SourceAndSink();
            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            var modelDefinition = fmModel.ModelDefinition;
            var salinityProperty = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            salinityProperty.Value = useSalinity;

            var tempertureProperty = modelDefinition.GetModelProperty(GuiProperties.UseTemperature);
            tempertureProperty.Value = useTemperature;
            
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

            // do the export
            var exportedFile = Path.Combine(FileUtils.CreateTempDirectory(), fileName);
            FileUtils.DeleteIfExists(exportedFile);
            var exporter = new TimFileExporter()
            {
                GetModelForSourceAndSink = input => fmModel
            };
            exporter.Export(sourceAndSink, exportedFile);

            // check results
            Assert.IsTrue(FileUtils.FilesAreEqual(expectedFile, exportedFile));

            // final cleanup
            FileUtils.DeleteIfExists(exportedFile);
        }
    }
}
