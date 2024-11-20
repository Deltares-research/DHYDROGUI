using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Forms
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class MultipleDataEditorTest
    {
        [Test]
        public void ShowEmpty()
        {
            var mde = new MultipleDataEditor();
            WindowsFormsTestHelper.ShowModal(mde);
        }

        [Test]
        public void ShowWithGreenhouseAndUnpavedAndPavedAndOpenWater()
        {
            var mde = new MultipleDataEditor();

            var rrmodel = new RainfallRunoffModel();
            rrmodel.Basin.Catchments.Add(new Catchment {Name = "c01", CatchmentType = CatchmentType.Unpaved});
            rrmodel.Basin.Catchments.Add(new Catchment { Name = "c02", CatchmentType = CatchmentType.GreenHouse });
            rrmodel.Basin.Catchments.Add(new Catchment { Name = "c03", CatchmentType = CatchmentType.Paved });
            rrmodel.Basin.Catchments.Add(new Catchment { Name = "c04", CatchmentType = CatchmentType.OpenWater });

            rrmodel.Basin.WasteWaterTreatmentPlants.Add(new WasteWaterTreatmentPlant { Name = "wwtp1" });
            
            var providerOne = new ConceptDataRowProvider<UnpavedData, UnpavedDataRow>(rrmodel, "Unpaved");
            var providerTwo = new ConceptDataRowProvider<GreenhouseData, GreenhouseDataRow>(rrmodel, "Greenhouse");
            var providerThree = new ConceptDataRowProvider<PavedData, PavedDataRow>(rrmodel, "Paved");
            var providerFour = new ConceptDataRowProvider<OpenWaterData,OpenWaterDataRow>(rrmodel, "Openwater");

            mde.Data = new IDataRowProvider[] {providerOne, providerTwo, providerThree, providerFour};

            WindowsFormsTestHelper.ShowModal(mde);
        }
    }
}
