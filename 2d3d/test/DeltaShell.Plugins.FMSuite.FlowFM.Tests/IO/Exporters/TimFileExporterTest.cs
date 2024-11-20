using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class TimFileExporterTest
    {
        private TimFileExporter exporter;
        private string tempDir;

        [SetUp]
        public void Setup()
        {
            exporter = new TimFileExporter();
        }

        [OneTimeSetUp]
        public void SetupFixture()
        {
            tempDir = FileUtils.CreateTempDirectory();
        }

        [OneTimeTearDown]
        public void TeardownFixture()
        {
            FileUtils.DeleteIfExists(tempDir);
        }

        [Test]
        public void TestExport_SourceAndSinks_WithMissingFunction()
        {
            // setup
            var sourceAndSink = new SourceAndSink() {Data = null};
            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            // do the export
            exporter.GetModelForSourceAndSink = sink => fmModel;

            Assert.IsFalse(exporter.Export(sourceAndSink, string.Empty));
            // check results
            string expectedLogMessage = string.Format(Resources.Could_not_export_data_for_SourceAndSink___0___no_Function_was_found, sourceAndSink.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(sourceAndSink, string.Empty), expectedLogMessage);
        }

        [Test]
        public void GivenAnTimFileExporterAndAnIBoundaryConditionWithDataTypeTimeSeries_WhenExportIsCalled_ThenTrueIsReturned()
        {
            string path = Path.Combine(tempDir, "myFile.tmp");

            var mocks = new MockRepository();

            // Set up conditions for test
            var itemMock = mocks.DynamicMock<IBoundaryCondition>();
            var functionMock = mocks.DynamicMock<IFunction>();

            itemMock.Expect(n => n.DataType).Return(BoundaryConditionDataType.TimeSeries).Repeat.Any();
            itemMock.Expect(n => n.GetDataAtPoint(Arg<int>.Is.Anything)).Return(functionMock).Repeat.Any();

            // Mocks to make the TimFile.Write work
            // Suggested fix dependency injection of TimFile and mock.
            var firstMock = mocks.DynamicMock<IVariable<DateTime>>();

            functionMock.Expect(n => n.Arguments).Return(null)
                        .WhenCalled(x => x.ReturnValue = new EventedList<IVariable> {firstMock}).Repeat.Any();
            functionMock.Expect(n => n.Components).Return(null)
                        .WhenCalled(x => x.ReturnValue = new EventedList<IVariable>()).Repeat.Any();

            var emptyValues = new MultiDimensionalArray<DateTime>();
            firstMock.Expect(n => n.Values).Return(emptyValues).Repeat.Any();

            mocks.ReplayAll();

            try
            {
                Assert.That(exporter.Export(itemMock, path), Is.True);
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void GivenAnTimFileExporterAndAnItemWhichIsNotAIBoundaryConditionSourceAndSinkOrHeatFluxModel_WhenExportIsCalled_ThenFalseIsReturned()
        {
            Assert.That(exporter.Export(null, Arg<string>.Is.Anything), Is.False);
        }

        [Test]
        public void GivenAnTimFileExporter_WhenExporterIsCalledWithAHeatFluxModel_ThenTrueIsReturned()
        {
            string path = Path.Combine(tempDir, "myFile.tmp");

            var heatFluxModel = new HeatFluxModel {Type = HeatFluxModelType.Composite};

            var fmModel = new WaterFlowFMModel();
            exporter.GetModelForHeatFluxModel = input => fmModel;

            try
            {
                Assert.That(exporter.Export(heatFluxModel, path), Is.True);
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void GivenAnExporterWithAnItemWhichWillCauseAnExceptionInTheWriter_WhenExportIsCalled_ThenAnErrorIsLoggedAndFalseIsReturned()
        {
            string path = Path.Combine(tempDir, "myFile.tmp");

            var mocks = new MockRepository();

            // Set up conditions for test
            var itemMock = mocks.DynamicMock<IBoundaryCondition>();
            var functionMock = mocks.DynamicMock<IFunction>();

            itemMock.Expect(n => n.DataType).Return(BoundaryConditionDataType.TimeSeries).Repeat.Any();
            itemMock.Expect(n => n.GetDataAtPoint(Arg<int>.Is.Anything)).Return(functionMock).Repeat.Any();

            mocks.ReplayAll();

            try
            {
                Assert.That(exporter.Export(itemMock, path), Is.False);

                string expectedLogMessage =
                    Resources.TimFileExporter_Export_Failed_to_export_data_to__0____1_.Split('{')[0];
                TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(itemMock, Arg<string>.Is.Anything), expectedLogMessage);
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void GivenAnTimFileExporter_WhenCategoryIsCalled_ThenGeneralIsReturned()
        {
            Assert.That(exporter.Category, Is.EqualTo("General"));
        }

        [Test]
        public void GivenAnTimFileExporter_WhenNameIsCalled_ThenTheNameIsReturned()
        {
            const string expectedValue = "Time series to .tim file";
            Assert.That(exporter.Name, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenAnTimFileExporter_WhenSourceTypesIsCalled_ThenTheCorrectSourceTypesAreReturned()
        {
            Assert.That(exporter.SourceTypes().Count(), Is.EqualTo(2));
            Assert.That(exporter.SourceTypes().Contains(typeof(SourceAndSink)));
            Assert.That(exporter.SourceTypes().Contains(typeof(HeatFluxModel)));
        }

        [Test]
        public void GivenAnTimFileExporter_WhenFileFilterIsCalled_ThenTheCorrectFileFilterIsReturned()
        {
            const string expectedValue = "Time series file|*.tim";
            Assert.That(exporter.FileFilter, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenAnTimFileExporter_WhenCanExportIsCalledWithAnyObject_ThenTrueIsReturned()
        {
            Assert.That(exporter.CanExportFor(Arg<object>.Is.Anything), Is.True);
        }

        [Test]
        public void GivenAnTimFileExporter_WhenForcingTypesIsCalled_ThenTheCorrectForcingTypesAreReturned()
        {
            Assert.That(exporter.ForcingTypes.Count(), Is.EqualTo(1));
            Assert.That(exporter.ForcingTypes.Contains(BoundaryConditionDataType.TimeSeries));
        }

        [TestCase(false, HeatFluxModelType.None, false, false, false, "00000.tim")]
        [TestCase(false, HeatFluxModelType.None, false, false, true, "00001.tim")]
        [TestCase(false, HeatFluxModelType.None, false, true, false, "00010.tim")]
        [TestCase(false, HeatFluxModelType.None, false, true, true, "00011.tim")]
        [TestCase(false, HeatFluxModelType.None, true, false, false, "00100.tim")]
        [TestCase(false, HeatFluxModelType.None, true, false, true, "00101.tim")]
        [TestCase(false, HeatFluxModelType.None, true, true, false, "00110.tim")]
        [TestCase(false, HeatFluxModelType.None, true, true, true, "00111.tim")]
        [TestCase(false, HeatFluxModelType.TransportOnly, false, false, false, "01000.tim")]
        [TestCase(false, HeatFluxModelType.TransportOnly, false, false, true, "01001.tim")]
        [TestCase(false, HeatFluxModelType.TransportOnly, false, true, false, "01010.tim")]
        [TestCase(false, HeatFluxModelType.TransportOnly, false, true, true, "01011.tim")]
        [TestCase(false, HeatFluxModelType.TransportOnly, true, false, false, "01100.tim")]
        [TestCase(false, HeatFluxModelType.TransportOnly, true, false, true, "01101.tim")]
        [TestCase(false, HeatFluxModelType.TransportOnly, true, true, false, "01110.tim")]
        [TestCase(false, HeatFluxModelType.TransportOnly, true, true, true, "01111.tim")]
        [TestCase(true, HeatFluxModelType.None, false, false, false, "10000.tim")]
        [TestCase(true, HeatFluxModelType.None, false, false, true, "10001.tim")]
        [TestCase(true, HeatFluxModelType.None, false, true, false, "10010.tim")]
        [TestCase(true, HeatFluxModelType.None, false, true, true, "10011.tim")]
        [TestCase(true, HeatFluxModelType.None, true, false, false, "10100.tim")]
        [TestCase(true, HeatFluxModelType.None, true, false, true, "10101.tim")]
        [TestCase(true, HeatFluxModelType.None, true, true, false, "10110.tim")]
        [TestCase(true, HeatFluxModelType.None, true, true, true, "10111.tim")]
        [TestCase(true, HeatFluxModelType.TransportOnly, false, false, false, "11000.tim")]
        [TestCase(true, HeatFluxModelType.TransportOnly, false, false, true, "11001.tim")]
        [TestCase(true, HeatFluxModelType.TransportOnly, false, true, false, "11010.tim")]
        [TestCase(true, HeatFluxModelType.TransportOnly, false, true, true, "11011.tim")]
        [TestCase(true, HeatFluxModelType.TransportOnly, true, false, false, "11100.tim")]
        [TestCase(true, HeatFluxModelType.TransportOnly, true, false, true, "11101.tim")]
        [TestCase(true, HeatFluxModelType.TransportOnly, true, true, false, "11110.tim")]
        [TestCase(true, HeatFluxModelType.TransportOnly, true, true, true, "11111.tim")]
        [Category(TestCategory.DataAccess)]
        public void TestExport_SourceAndSinks(bool useSalinity, HeatFluxModelType temperature, bool useMorSed, bool useSecFlow, bool tracersPresent, string fileName)
        {
            string expectedFile = TestHelper.GetTestFilePath(@"timFiles\" + fileName);

            // setup
            var sourceAndSink = new SourceAndSink();
            WaterFlowFMModel fmModel = ConstructSourceAndSinkFlowFMModel(sourceAndSink, useSalinity, temperature, useMorSed, useSecFlow, tracersPresent);

            // do the export
            string exportedFile = Path.Combine(tempDir, fileName);
            FileUtils.DeleteIfExists(exportedFile);
            exporter.GetModelForSourceAndSink = input => fmModel;

            exporter.Export(sourceAndSink, exportedFile);

            // check results
            Assert.IsTrue(FileUtils.FilesAreEqual(expectedFile, exportedFile));

            // final cleanup
            FileUtils.DeleteIfExists(exportedFile);
        }

        private static void AddVariableWithRange(SourceAndSink ss, string name, int n1, int n2, int n3)
        {
            IFunction function = ss.Function;
            IVariable variable = function.Components.FirstOrDefault(c => c.Name == name);
            Assert.NotNull(variable);
            variable.Values.Clear();
            variable.Values.AddRange(new List<double>
            {
                n1,
                n2,
                n3
            });
        }

        [TestCase(false, HeatFluxModelType.None, false, false, false)]
        public void GivenAnTimFileExporterAndASourceAndSinkItem_WhenExportIsCalled_ThenTrueIsReturned(bool useSalinity, HeatFluxModelType temperature, bool useMorSed, bool useSecFlow, bool tracersPresent)
        {
            string path = Path.Combine(tempDir, "myFile.tmp");

            // Construct complete fmModel / SourceAndSink due to use of static methods
            var sourceAndSink = new SourceAndSink();
            WaterFlowFMModel fmModel = ConstructSourceAndSinkFlowFMModel(sourceAndSink, useSalinity, temperature, useMorSed, useSecFlow, tracersPresent);

            exporter.GetModelForSourceAndSink = input => fmModel;

            try
            {
                Assert.That(exporter.Export(sourceAndSink, path), Is.True);
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        private static WaterFlowFMModel ConstructSourceAndSinkFlowFMModel(SourceAndSink sourceAndSink,
                                                                          bool useSalinity,
                                                                          HeatFluxModelType temperature,
                                                                          bool useMorSed,
                                                                          bool useSecFlow,
                                                                          bool tracersPresent)
        {
            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            var fractionList = new List<SedimentFraction>
            {
                new SedimentFraction {Name = "Fraction_1"},
                new SedimentFraction {Name = "Fraction_2"}
            };

            var tracerList = new List<string>()
            {
                "Tracer_1",
                "Tracer_2"
            };
            var tracerBoundaryConditions = new List<FlowBoundaryCondition>();
            foreach (string tracer in tracerList)
            {
                tracerBoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty) {TracerName = tracer});
            }

            var boundarySet = new BoundaryConditionSet();

            var model = new WaterFlowFMModel
            {
                SourcesAndSinks = {sourceAndSink},
                BoundaryConditionSets = {boundarySet}
            };

            model.SedimentFractions.AddRange(fractionList);
            if (tracersPresent)
            {
                model.TracerDefinitions.AddRange(tracerList);
                boundarySet.BoundaryConditions.AddRange(tracerBoundaryConditions);
            }

            WaterFlowFMModelDefinition modelDefinition = fmModel.ModelDefinition;

            var temperatureString = ((int) temperature).ToString();
            modelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = useSalinity;
            modelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString(temperatureString);
            modelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = useMorSed;
            modelDefinition.GetModelProperty(KnownProperties.SecondaryFlow).Value = useSecFlow;

            IVariable timeVariable = sourceAndSink.Function.Arguments.FirstOrDefault(c => c.Name == SourceSinkVariableInfo.TimeVariableName);
            Assert.NotNull(timeVariable);
            var timeIndex0 = new DateTime(2018, 07, 11, 00, 00, 00, DateTimeKind.Utc);
            timeVariable.Values.AddRange(new List<DateTime>
            {
                timeIndex0,
                timeIndex0.AddYears(1),
                timeIndex0.AddYears(2)
            });

            AddVariableWithRange(sourceAndSink, SourceSinkVariableInfo.DischargeVariableName, 1, 2, 3);
            AddVariableWithRange(sourceAndSink, SourceSinkVariableInfo.SalinityVariableName, 2, 3, 4);
            AddVariableWithRange(sourceAndSink, SourceSinkVariableInfo.TemperatureVariableName, 3, 4, 5);
            AddVariableWithRange(sourceAndSink, "Fraction_1", 4, 5, 6);
            AddVariableWithRange(sourceAndSink, "Fraction_2", 44, 55, 66);
            AddVariableWithRange(sourceAndSink, SourceSinkVariableInfo.SecondaryFlowVariableName, 5, 6, 7);
            if (tracersPresent)
            {
                AddVariableWithRange(sourceAndSink, "Tracer_1", 6, 7, 8);
                AddVariableWithRange(sourceAndSink, "Tracer_2", 66, 77, 88);
            }

            return fmModel;
        }
    }
}