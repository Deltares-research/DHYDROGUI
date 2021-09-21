using System.Threading;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class PipeViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Test()
        {
            var view = new SewerConnectionView();

            var manhole1 = new Manhole("1");
            var compartment1 = new Compartment("cmp1") {SurfaceLevel = 2.12345, BottomLevel = 1.23456};
            manhole1.Compartments.Add(compartment1);

            var manhole2 = new Manhole("2");
            var compartment2 = new Compartment("cmp2") {SurfaceLevel = 2.12345, BottomLevel = 1.23456};
            manhole2.Compartments.Add(compartment2);

            view.Data = new Pipe
            {
                Source = manhole1, SourceCompartment = compartment1, Target = manhole2, TargetCompartment = compartment2,
                LevelSource = 1.3, LevelTarget = 1.3, 
                Length = 10
            };
            WpfTestHelper.ShowModal(view);
        }
    }
}