using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    internal class SupportPointSelectionFormTest
    {
        [Test]
        public void ShowWithAstroControl()
        {
            var form = new SupportPointSelectionForm();
            WindowsFormsTestHelper.ShowModal(form);
        }
    }
}