using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class DepthLayerDialogTest
    {
        [Test]
        public void ShowSingleLayer()
        {
            var editor = new DepthLayerDialog();
            WindowsFormsTestHelper.ShowModal(editor);
        }
    }
}
