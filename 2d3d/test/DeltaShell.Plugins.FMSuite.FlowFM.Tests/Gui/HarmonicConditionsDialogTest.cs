using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.Wpf)]
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