using System.Threading;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class HarmonicConditionsDialogTest
    {
        [Test]
        public void ShowDialog()
        {
            var dialog = new HarmonicConditionsDialog();
            WpfTestHelper.ShowModal(dialog);
        }
    }
}