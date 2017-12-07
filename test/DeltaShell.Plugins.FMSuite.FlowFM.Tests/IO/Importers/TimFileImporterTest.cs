using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture()]
    public class TimFileImporterTest
    {
        [TestCase(false, false)] // None
        [TestCase(true, false)] // Salinity only
        [TestCase(false, true)] // Temperature only
        [TestCase(true, true)] // Both
        public void TestImportItem_SourceAndSinks(bool useSalinity, bool useTemperature)
        {
            var testFilePath = TestHelper.GetTestFilePath(@"timFiles\testFile.tim");

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

    }
}
