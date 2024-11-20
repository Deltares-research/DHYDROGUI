using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Unpaved;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Concepts
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class ErnstZeeuwHellingaDrainageControlTest
    {
        [Test]
        public void ShowEmpty()
        {
            var control = new ErnstZeeuwHellingaDrainageControl();
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        public void ShowZeeuw()
        {
            var zeeuw = new DeZeeuwHellingaDrainageFormula();
            var control = new ErnstZeeuwHellingaDrainageControl();
            control.Data = zeeuw;
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        public void ShowErnst()
        {
            var ernst = new ErnstDrainageFormula();
            ernst.LevelOneEnabled = true;
            ernst.LevelTwoEnabled = true;
            ernst.LevelThreeEnabled = true;
            ernst.SurfaceRunoff = 1;
            ernst.LevelOneValue = 2;
            ernst.LevelTwoValue = 3;
            ernst.LevelThreeValue = 4;
            ernst.InfiniteDrainageLevelRunoff = 5;
            ernst.HorizontalInflow = 6;

            var control = new ErnstZeeuwHellingaDrainageControl();
            control.Data = ernst;
            WindowsFormsTestHelper.ShowModal(control);
        }
    }
}
