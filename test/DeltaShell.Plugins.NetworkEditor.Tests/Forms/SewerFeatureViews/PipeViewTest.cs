using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class PipeViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Test()
        {
            var view = new PipeView();

            var pipe = new Pipe
            {
                Name = "",
                WaterType = SewerConnectionWaterType.Combined,
                Length = 40,
                SourceCompartment = new Compartment("cmp1"),
                TargetCompartment = new Compartment("cmp2"),
                LevelSource = 1,
                LevelTarget = 2,
            };

            view.Data = pipe;
            WpfTestHelper.ShowModal(view);
        }
    }
}