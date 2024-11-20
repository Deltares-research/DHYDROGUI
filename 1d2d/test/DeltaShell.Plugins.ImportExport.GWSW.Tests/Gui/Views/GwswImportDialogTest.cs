using System.Threading;
using DeltaShell.Plugins.ImportExport.GWSW.Views;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests.Gui.Views
{
    [TestFixture]
    public class GwswImportDialogTest
    {
        [Test, Apartment(ApartmentState.STA)]
        public void GivenGwswImportDialog_SettingData_ShouldSetViewModelImporter()
        {
            //Arrange
            var dialog = new GwswImportDialog();

            // Act
            var gwswFileImporter = new GwswFileImporter(new DefinitionsProvider());
            dialog.Data = gwswFileImporter;

            // Assert
            Assert.AreEqual(gwswFileImporter,dialog.GwswImportControl.Importer);
        }
    }
}