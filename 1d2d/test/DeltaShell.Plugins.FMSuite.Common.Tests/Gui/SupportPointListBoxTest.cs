using System.Linq;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Gui
{
    [TestFixture]
    public class SupportPointListBoxTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Show()
        {
            var supportPointListBox = new SupportPointListBox();

            supportPointListBox.Items.AddRange(
                Enumerable.Range(0, 20).Select(i => i.ToString()).Cast<object>().ToArray());

            WindowsFormsTestHelper.ShowModal(supportPointListBox);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithContextMenu()
        {
            var supportPointListBox = new SupportPointListBox();

            supportPointListBox.Items.AddRange(
                Enumerable.Range(0, 20).Select(i => i.ToString()).Cast<object>().ToArray());

            supportPointListBox.ContextMenuItems = new[] { new ToolStripMenuItem("Edit..."), new ToolStripMenuItem("Properties...") };

            WindowsFormsTestHelper.ShowModal(supportPointListBox);
        }
    }
}