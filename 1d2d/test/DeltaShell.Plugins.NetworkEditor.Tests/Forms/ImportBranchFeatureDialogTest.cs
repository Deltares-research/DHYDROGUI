using System.Collections;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms
{
    [TestFixture]
    public class ImportBranchFeatureDialogTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowDialog()
        {
            var layerList = new ArrayList
                                {
                                    new { Name = "Pumps" },
                                    new { Name = "Weirs" },
                                    new { Name = "Bridges" },
                                    new { Name = "Culverts" }
                                };
            ImportBranchFeatureDialog importBranchFeatureDialog = new ImportBranchFeatureDialog
                                                                      {
                                                                          DataSource = layerList,
                                                                          DisplayMember = "Name"
                                                                      };

            WindowsFormsTestHelper.ShowModal(importBranchFeatureDialog);
        }
    }
}
