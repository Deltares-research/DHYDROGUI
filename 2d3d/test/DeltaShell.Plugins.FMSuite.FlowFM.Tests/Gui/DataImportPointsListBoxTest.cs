using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    internal class DataImportPointsListBoxTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithSomeItems()
        {
            var control = new DataImportPointsListBox();
            for (var i = 1; i < 11; ++i)
            {
                control.Items.Add("item_" + i);
                if (i % 2 == 0)
                {
                    control.DataPointIndices.Add(i - 1);
                }
            }

            WindowsFormsTestHelper.ShowModal(control);
        }
    }
}