using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms.PropertyGrid
{
    [TestFixture]
    public class ReverseRoughnessSectionPropertiesTest
    {
        [Test, Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new ReverseRoughnessSectionProperties{ Data = new ReverseRoughnessSection(new RoughnessSection(new CrossSectionSectionType(), new HydroNetwork())) });
        }
    }
}