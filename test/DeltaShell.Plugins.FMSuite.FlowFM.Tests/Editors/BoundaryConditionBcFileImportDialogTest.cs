using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture]
    public class BoundaryConditionBcFileImportDialogTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowDialog()
        {
            var form = new BoundaryConditionBcFileImportDialog();
            WindowsFormsTestHelper.ShowModal(form);
        }
    }
}