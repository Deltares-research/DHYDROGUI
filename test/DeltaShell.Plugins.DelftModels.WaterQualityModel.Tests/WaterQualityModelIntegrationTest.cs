using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class WaterQualityModelIntegrationTest
    {
        [Test]
        public void ImportSobekHydFileAndRun()
        {
            string dataDir = TestHelper.GetDataDir();
            string hydFile = Path.Combine(dataDir, "IntegrationTests", "Flow1D", "sobek.hyd");

            WaterQualityModel model = new WaterQualityModel();

            new HydFileImporter().ImportItem(hydFile, model);

            var subFilePath = Path.Combine(dataDir, "IntegrationTests", "Eutrof_simple.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            // Send the model to delwaq
            ActivityRunner.RunActivity(model);

            Assert.IsTrue(model.Status == ActivityStatus.Cleaned);
            Assert.IsTrue(model.OutputSubstancesDataItemSet.DataItems.Any());
            var oxygenDataItem = model.OutputSubstancesDataItemSet.DataItems.FirstOrDefault(d => d.Name.Equals("OXY"));
            Assert.NotNull(oxygenDataItem, "OXY dataitem not found.");
            var oxygen = (UnstructuredGridCellCoverage)oxygenDataItem.Value;
            var firstFeature = oxygen.GetTimeSeries(oxygen.GetCoordinatesForGrid(oxygen.Grid).First());
            Assert.NotNull(firstFeature, "First feature in oxygen data item not found.");
            var firstComponent = firstFeature.Components.FirstOrDefault();
            Assert.NotNull(firstComponent, "first feature component invalid.");
            for (int i = 1; i < firstComponent.Values.Count; i++)
            {
                Assert.IsTrue((double)firstComponent.Values[i] > 0d);
            }
        }

        [Test]
        public void ImportFMHydFileAndRun()
        {
            string dataDir = TestHelper.GetDataDir();
            string hydFile = Path.Combine(dataDir, "IntegrationTests", "FM", "FlowFM.hyd");

            WaterQualityModel model = new WaterQualityModel();

            new HydFileImporter().ImportItem(hydFile, model);

            var subFilePath = Path.Combine(dataDir, "IntegrationTests", "coli_04.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);
            
            // Send the model to delwaq
            ActivityRunner.RunActivity(model);

            Assert.IsTrue(model.Status == ActivityStatus.Cleaned);
            var dataItems = model.OutputSubstancesDataItemSet.DataItems.ToList();
            Assert.IsTrue(dataItems.Any());
            var substanceDataItem = dataItems.FirstOrDefault(d => d.Name.Equals("Salinity"));
            Assert.NotNull(substanceDataItem, "Substance data item for Salinity not found.");
            var substance = substanceDataItem.Value as UnstructuredGridCellCoverage;
            Assert.NotNull(substance, "Substance not of type UnstructuredGridCellCoverage.");
            Assert.NotNull(substance.Grid, "Substance.Grid undefined.");
            var coordinate = substance.GetCoordinatesForGrid(substance.Grid).ToList().FirstOrDefault();
            Assert.NotNull(coordinate, "Coordinate not found.");
            var firstFeature = substance.GetTimeSeries(coordinate);
            Assert.NotNull(firstFeature, "First feature in substance data item not found.");
            var firstComponent = firstFeature.Components.FirstOrDefault();
            Assert.AreEqual(181, firstComponent.Values.Count);
        }

        [Test]
        public void ImportUgridHydFileAndRun()
        {
            string dataDir = TestHelper.GetDataDir();
            string hydFile = Path.Combine(dataDir, "IntegrationTests", "UGrid", "f34.hyd");

            WaterQualityModel model = new WaterQualityModel();

            new HydFileImporter().ImportItem(hydFile, model);

            var subFilePath = Path.Combine(dataDir, "IntegrationTests", "Eutrof_simple.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            // Send the model to delwaq
            ActivityRunner.RunActivity(model);

            Assert.IsTrue(model.Status == ActivityStatus.Cleaned);
            Assert.IsTrue(model.OutputSubstancesDataItemSet.DataItems.Any());
            var oxygenDataItem = model.OutputSubstancesDataItemSet.DataItems.FirstOrDefault(d => d.Name.Equals("OXY"));
            Assert.NotNull(oxygenDataItem, "OXY dataitem not found.");
            var oxygen = (UnstructuredGridCellCoverage)oxygenDataItem.Value;
            var firstFeature = oxygen.GetTimeSeries(oxygen.GetCoordinatesForGrid(oxygen.Grid).First());
            Assert.NotNull(firstFeature, "First feature in oxygen data item not found.");
            var firstComponent = firstFeature.Components.FirstOrDefault();
            Assert.NotNull(firstComponent, "first feature component invalid.");
            for (int i = 1; i < firstComponent.Values.Count; i++)
            {
                Assert.IsTrue((double)firstComponent.Values[i] > 0d);
            }
        }

    }
}