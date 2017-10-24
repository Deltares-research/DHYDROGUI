using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Wizard;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import.Wizard
{
    [TestFixture]
    [Category("OleDbSchema_x86")]
    public class SelectDataWizardPageRelatedTablesPopUpTest
    {
        private SelectDataWizardPageRelatedTablesPopUp page;
        private ISchemaReader schemaReader;


        [SetUp]
        public void SetUp()
        {
            schemaReader = new OleDbSchemaReader();
            page = new SelectDataWizardPageRelatedTablesPopUp();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSelectDataWizardPageRelatedTablesPopUp()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
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
