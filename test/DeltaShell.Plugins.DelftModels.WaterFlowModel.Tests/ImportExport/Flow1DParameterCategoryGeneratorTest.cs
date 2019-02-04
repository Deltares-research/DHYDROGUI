using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class Flow1DParameterCategoryGeneratorTest
    {
        [Test]
        public void
            GivenAWaterFlowModel1D_WhenGenerateSpecialsValuesIsCalledWithThisModel_ThenACorrectGenerateSpecialsValuesIsGenerated() { }

        [Test]
        public void
            GivenAWaterFlowModel1DWithSedimentValues_WhenGeneratingSedimentProperties_ThenADelftIniCategoryIsReturned()
        {
            var model = new WaterFlowModel1D("TestModel")
            {
                D50 = 0.0005,
                D90 = 0.001,
                DepthUsedForSediment = 0.1
            };

            var category = Flow1DParameterCategoryGenerator.GenerateSedimentValues(model);

            Assert.That(category.Properties.Count, Is.EqualTo(3));
        }

        [Test]
        public void
            GivenAWaterFlowModel1DWithoutSedimentValues_WhenGeneratingSedimentProperties_ThenNoPropertiesAreReturnedAndValuesAreNotSetOnModel()
        {
            var model = new WaterFlowModel1D("TestModel");

            var category = Flow1DParameterCategoryGenerator.GenerateSedimentValues(model);

            Assert.That(category.Properties.Count, Is.EqualTo(0));
            Assert.That(model.D50, Is.EqualTo(null));
            Assert.That(model.D90, Is.EqualTo(null));
            Assert.That(model.DepthUsedForSediment, Is.EqualTo(null));
        }
    }
}