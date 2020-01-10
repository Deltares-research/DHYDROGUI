using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class RTCModelImporterTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ImportREModel_NDB()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_NDB.sbk\6\deftop.1";
            var model = ImportModelFromSobek(pathToSobekNetwork);

            Assert.IsTrue(model is ICompositeActivity);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ImportREModel_Maas()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\deftop.1";
            var model = ImportModelFromSobek(pathToSobekNetwork);

            Assert.IsTrue(model is ICompositeActivity);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ImportNoControllerTriggersModelReturnsAWaterFlowModel()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\SW_max_1.lit\3\Network.TP";
            var model = ImportModelFromSobek(pathToSobekNetwork);

            Assert.IsTrue(model is WaterFlowModel1D);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void ImportDeltaModel()
        {
            LogHelper.ConfigureLogging();
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Delta_M.lit\30\Network.TP";
            var model = ImportModelFromSobek(pathToSobekNetwork);

            Assert.IsTrue(model is ICompositeActivity);
        }


        private IModel ImportModelFromSobek(string pathToSobekNetwork)
        {
            var modelImporter = new SobekModelToIntegratedModelImporter();
            return (IModel)modelImporter.ImportItem(pathToSobekNetwork);
        }

    }
}
