using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NUnit.Framework;
using Rhino.Mocks;
using Assert = NUnit.Framework.Assert;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class TimFileImporterTest
    {
        private MockRepository mocks;
        private TimFileImporter importer;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            importer = new TimFileImporter();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void GivenTimFileImporterWhenBoundaryConditionHasCorrectDataTypeThenValidate()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result);
        }

        [Test]
        public void GivenTimFileImporterWhenBoundaryConditionHasInCorrectDataTypeThenValidate()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

        [TestCase(@"timFiles\NoSalinityOrTemperature.tim", false, false)] // None
        [TestCase(@"timFiles\SalinityOnly.tim", true, false)] // Salinity only
        [TestCase(@"timFiles\TemperatureOnly.tim", false, true)] // Temperature only
        [TestCase(@"timFiles\BothSalinityAndTemperature.tim", true, true)] // Both
        [TestCase(@"timFiles\testFile.tim", true, true)] // Both

        public void TestImportItem_SourceAndSinks(string testFile, bool useSalinity, bool useTemperature)
        {
            var testFilePath = TestHelper.GetTestFilePath(testFile);

            // setup
            var sourceAndSink = new SourceAndSink();
            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            var modelDefinition = fmModel.ModelDefinition;
            var salinityProperty = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            salinityProperty.Value = useSalinity;

            var tempertureProperty = modelDefinition.GetModelProperty(GuiProperties.UseTemperature);
            tempertureProperty.Value = useTemperature;

            // do the import
            var importer = new TimFileImporter()
            {
                GetModelForSourceAndSink = input => fmModel
            };
            importer.ImportItem(testFilePath, sourceAndSink);

            // check results
            var function = sourceAndSink.Function;
            var dischargeVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.DischargeVariableName);
            Assert.NotNull(dischargeVariable);

            var dischargeValues = ((MultiDimensionalArray<double>) dischargeVariable.Values).ToList();
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

        
        [TestCase(@"timFiles\testFile.tim", false, false)] // None
        [TestCase(@"timFiles\testFile.tim", true, false)] // Salinity only
        [TestCase(@"timFiles\testFile.tim", false, true)] // Temperature only
        public void TestImportItem_SourceAndSinks_LogsErrorMessageForAdditionalValuesInFile(string testFile, bool useSalinity, bool useTemperature)
        {
            var testFilePath = TestHelper.GetTestFilePath(testFile);

            // setup
            var sourceAndSink = new SourceAndSink();
            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            var modelDefinition = fmModel.ModelDefinition;
            var salinityProperty = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            salinityProperty.Value = useSalinity;

            var tempertureProperty = modelDefinition.GetModelProperty(GuiProperties.UseTemperature);
            tempertureProperty.Value = useTemperature;

            // do the import
            var importer = new TimFileImporter()
            {
                GetModelForSourceAndSink = input => fmModel
            };

            var expectedLogMessage = Resources.SourceAndSinkImporterHelper_TryAdjustSalinityAndTemperatureComponents_Additional_values_detected_for_one_or_more_physical_processes;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(testFilePath, sourceAndSink), expectedLogMessage);
        }

        [Test]
        public void TestImportItem_SourceAndSinks_LogsErrorMessageForNullModel()
        {
            // setup
            var sourceAndSink = new SourceAndSink();

            // do the import & check results
            var importer = new TimFileImporter()
            {
                GetModelForSourceAndSink = input => null
            };

            var expectedLogMessage = string.Format(Resources.Tim_file_import_failed__could_not_retrieve_model_for_SourceAndSink___0_, sourceAndSink.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(string.Empty, sourceAndSink), expectedLogMessage);
        }

        [Test]
        public void TestImportItem_SourceAndSinks_LogsErrorMessageForNullFunction()
        {
            // setup
            var sourceAndSink = new SourceAndSink() { Data = null };
            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            // do the import & check results
            var importer = new TimFileImporter()
            {
                GetModelForSourceAndSink = input => fmModel
            };

            var expectedLogMessage = string.Format(Resources.Tim_file_import_failed__could_not_retrieve_function_for_SourceAndSink___0_, sourceAndSink.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(string.Empty, sourceAndSink), expectedLogMessage);
        }

        [Test]
        public void TestImportItem_SourceAndSinks_LogsErrorMessageForNullValues()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"timFiles\SalinityOnly.tim");

            // setup
            var sourceAndSink = new SourceAndSink();
            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            // Remove temperatureComponent
            sourceAndSink.Function.RemoveComponentByName(SourceAndSink.TemperatureVariableName);

            var modelDefinition = fmModel.ModelDefinition;
            var salinityProperty = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            salinityProperty.Value = true;

            var tempertureProperty = modelDefinition.GetModelProperty(GuiProperties.UseTemperature);
            tempertureProperty.Value = false;

            // do the import & check results
            var importer = new TimFileImporter()
            {
                GetModelForSourceAndSink = input => fmModel
            };

            var expectedLogMessage = string.Format(Resources.Tim_file_import_failed__could_not_determine_physical_processes_for_imported_SourceAndSink__0_, sourceAndSink.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(testFilePath, sourceAndSink), expectedLogMessage);
        }

    }
}
