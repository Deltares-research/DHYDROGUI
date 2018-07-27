using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class TimFileExporterTest
    {
        [TestCase(false, false, false, false, false, "00000.tim")]
        [TestCase(false, false, false, false, true, "00001.tim")]
        [TestCase(false, false, false, true, false, "00010.tim")]
        [TestCase(false, false, false, true, true, "00011.tim")]
        [TestCase(false, false, true, false, false, "00100.tim")]
        [TestCase(false, false, true, false, true, "00101.tim")]
        [TestCase(false, false, true, true, false, "00110.tim")]
        [TestCase(false, false, true, true, true, "00111.tim")]
        [TestCase(false, true, false, false, false, "01000.tim")]
        [TestCase(false, true, false, false, true, "01001.tim")]
        [TestCase(false, true, false, true, false, "01010.tim")]
        [TestCase(false, true, false, true, true, "01011.tim")]
        [TestCase(false, true, true, false, false, "01100.tim")]
        [TestCase(false, true, true, false, true, "01101.tim")]
        [TestCase(false, true, true, true, false, "01110.tim")]
        [TestCase(false, true, true, true, true, "01111.tim")]
        [TestCase(true, false, false, false, false, "10000.tim")]
        [TestCase(true, false, false, false, true, "10001.tim")]
        [TestCase(true, false, false, true, false, "10010.tim")]
        [TestCase(true, false, false, true, true, "10011.tim")]
        [TestCase(true, false, true, false, false, "10100.tim")]
        [TestCase(true, false, true, false, true, "10101.tim")]
        [TestCase(true, false, true, true, false, "10110.tim")]
        [TestCase(true, false, true, true, true, "10111.tim")]
        [TestCase(true, true, false, false, false, "11000.tim")]
        [TestCase(true, true, false, false, true, "11001.tim")]
        [TestCase(true, true, false, true, false, "11010.tim")]
        [TestCase(true, true, false, true, true, "11011.tim")]
        [TestCase(true, true, true, false, false, "11100.tim")]
        [TestCase(true, true, true, false, true, "11101.tim")]
        [TestCase(true, true, true, true, false, "11110.tim")]
        [TestCase(true, true, true, true, true, "11111.tim")]

        public void TestExport_SourceAndSinks(bool useSalinity, bool useTemperature, bool useMorSed, bool useSecFlow, bool tracersPresent, string fileName)
        {
            var expectedFile = TestHelper.GetTestFilePath(@"timFiles\" + fileName);

            // setup
            var sourceAndSink = new SourceAndSink();
            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            var fractionList = new List<SedimentFraction>
            {
                new SedimentFraction {Name = "Fraction_1"},
                new SedimentFraction {Name = "Fraction_2"}
            };

            var tracerList = new List<string>() { "Tracer_1", "Tracer_2" };
            var tracerBoundaryConditions = new List<FlowBoundaryCondition>();
            foreach (var tracer in tracerList)
            {
                tracerBoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty){ TracerName = tracer });
            }

            var boundarySet = new BoundaryConditionSet();

            var model = new WaterFlowFMModel
            {
                SourcesAndSinks = { sourceAndSink },
                BoundaryConditionSets = { boundarySet },
            };

            model.SedimentFractions.AddRange(fractionList);
            if (tracersPresent)
            {
                model.TracerDefinitions.AddRange(tracerList);
                boundarySet.BoundaryConditions.AddRange(tracerBoundaryConditions);
            }

            var modelDefinition = fmModel.ModelDefinition;

            modelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = useSalinity;
            modelDefinition.GetModelProperty(GuiProperties.UseTemperature).Value = useTemperature;
            modelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = useMorSed;
            modelDefinition.GetModelProperty(KnownProperties.SecondaryFlow).Value = useSecFlow;

            var timeVariable = sourceAndSink.Function.Arguments.FirstOrDefault(c => c.Name == SourceAndSink.TimeVariableName);
            Assert.NotNull(timeVariable);
            var timeIndex0 = new DateTime(2018, 07, 11, 00, 00, 00, DateTimeKind.Utc);
            timeVariable.Values.AddRange(new List<DateTime> { timeIndex0, timeIndex0.AddYears(1), timeIndex0.AddYears(2) });

            AddVariableWithRange(sourceAndSink, SourceAndSink.DischargeVariableName, 1, 2, 3);
            AddVariableWithRange(sourceAndSink, SourceAndSink.SalinityVariableName, 2, 3, 4);
            AddVariableWithRange(sourceAndSink, SourceAndSink.TemperatureVariableName, 3, 4, 5);
            AddVariableWithRange(sourceAndSink, "Fraction_1", 4, 5, 6);
            AddVariableWithRange(sourceAndSink, "Fraction_2", 44, 55, 66);
            AddVariableWithRange(sourceAndSink, SourceAndSink.SecondaryFlowVariableName, 5, 6, 7);
            if (tracersPresent)
            {
                AddVariableWithRange(sourceAndSink, "Tracer_1", 6, 7, 8);
                AddVariableWithRange(sourceAndSink, "Tracer_2", 66, 77, 88);
            }

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

        private static void AddVariableWithRange(SourceAndSink ss, string name, int n1, int n2, int n3)
        {
            var function = ss.Function;
            var variable = function.Components.FirstOrDefault(c => c.Name == name);
            Assert.NotNull(variable);
            variable.Values.Clear();
            variable.Values.AddRange(new List<double> {n1, n2, n3});
        }


        [Test]
        public void TestExport_SourceAndSinks_WithMissingFunction()
        {
            // setup
            var sourceAndSink = new SourceAndSink() { Data = null };
            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            // do the export
            var exporter = new TimFileExporter()
            {
                GetModelForSourceAndSink = input => fmModel
            };
            Assert.IsFalse(exporter.Export(sourceAndSink, string.Empty));
            // check results
            var expectedLogMessage = string.Format(Resources.Could_not_export_data_for_SourceAndSink___0___no_Function_was_found, sourceAndSink.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(()=> exporter.Export(sourceAndSink, string.Empty), expectedLogMessage);
        }
    }
}
