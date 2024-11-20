using System.Threading;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class LeveeBreachViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void WhenOpeningDamBreakView_ShouldNotGiveError()
        {
            var view = new LeveeBreachView { Data = new LeveeBreach() };

            WpfTestHelper.ShowModal(view);
        }
    }
}