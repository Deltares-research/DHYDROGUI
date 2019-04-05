using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Wizard;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import.Wizard
{
    [TestFixture]
    [Category(TestCategory.X86)]
    public class SelectDataWizardPageRelatedTablesPopUpTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSelectDataWizardPageRelatedTablesPopUp()
        {
            var schemaReader = new OleDbSchemaReader();
            var page = new SelectDataWizardPageRelatedTablesPopUp();

            string path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            schemaReader.Path = path;
            schemaReader.OpenConnection();

            page.SchemaReader = schemaReader;
            page.TableName = "Cross_section_definition";

            //var form = new Form();
            //form.Size = new Size(750, 500);
            //page.Dock = DockStyle.Fill;
            //form.Controls.Add(page);
            //form.ShowDialog();

            WindowsFormsTestHelper.ShowModal(page);

            if (page.DialogResult == DialogResult.OK)
            {
                var columnNameID = page.GetColumnNameID();
                var relatedTables = page.GetRelatedTables();
            }

            schemaReader.CloseConnection();
        }
    }
}
