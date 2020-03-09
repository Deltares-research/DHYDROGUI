using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekRRBoundaryConditionsImporterTest
    {
        IPartialSobekImporter importer;
        RainfallRunoffModel rrModel;

        private void SetImporterForFile(string filePath)
        {
            rrModel = new RainfallRunoffModel();
            importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(filePath, rrModel);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        [Category("Quarantine")]
        public void ImportTholenCheckBoundaryConditions()
        {
            SetImporterForFile(TestHelper.GetTestDataDirectory() + @"\Tholen.lit\29\NETWORK.TP");
            importer.Import();
            // there are 302 bc's in the file but all are linked-to-flow
            Assert.AreEqual(0, rrModel.GetAllModelData()
                                      .OfType<UnpavedData>()
                                      .Count(up => up.BoundaryData.Data.Components[0].Values.Count > 0));
            // there is only one real boundary
            Assert.AreEqual(1, rrModel.BoundaryData.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category("Quarantine")]
        public void ImportRrMiniTestModelsUnpavedBoundaryConditions()
        {
            SetImporterForFile(TestHelper.GetTestDataDirectory() + @"\RRMiniTestModels\DRRSA.lit\2\NETWORK.TP");
            importer.Import();

            //BOUN id 'cnGFE1022' bl 0  -2.85 is 0 boun
            //BOUN id 'cnGFE1021' bl 0  -2.85 is 0 boun

            var unpavedDatas =
                rrModel.GetAllModelData().OfType<UnpavedData>().ToList();

            Assert.AreEqual("GFE1022", unpavedDatas[0].Name);
            Assert.AreEqual(-2.85, unpavedDatas[0].BoundaryData.Value, 1e-05);
            Assert.AreEqual("GFE1021", unpavedDatas[1].Name);
            Assert.AreEqual(-2.85, unpavedDatas[1].BoundaryData.Value, 1e-05);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category("Quarantine")]
        public void ImportRrMiniTestModelsUnpavedBoundaryConditionsTable()
        {
            SetImporterForFile(TestHelper.GetTestDataDirectory() + @"\RRMiniTestModels\DRRSA.lit\6\NETWORK.TP");
            importer.Import();

            //BOUN id 'cnGFE1022' bl 1 '1' is  0 boun
            //BOUN id 'cnGFE1021' bl 1 '2' is  0 boun

            var unpavedDatas =
                rrModel.GetAllModelData().OfType<UnpavedData>().ToList();

            Assert.AreEqual(2, unpavedDatas.Count);
            Assert.AreEqual("GFE1022", unpavedDatas[0].Name);
            Assert.AreEqual(577, unpavedDatas[0].BoundaryData.Data.Time.Values.Count);
            Assert.AreEqual("GFE1021", unpavedDatas[1].Name);
            Assert.AreEqual(577, unpavedDatas[1].BoundaryData.Data.Time.Values.Count);
        }
    }
}