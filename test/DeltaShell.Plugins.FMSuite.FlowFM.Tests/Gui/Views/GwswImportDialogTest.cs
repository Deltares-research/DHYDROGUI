using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.ViewModels;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Views
{
    [TestFixture]
    public class GwswImportDialogTest
    {
        [Category(TestCategory.WindowsForms)]
        [Test]
        public void ShowUserControl()
        {
            var dialog = new GwswImportDialog();
            dialog.Data = new GwswFileImporter();
            WpfTestHelper.ShowModal(dialog);
        }

        [Test]
        public void CreateGwswFeatureViewItem()
        {
            var viewItem = new GwswFeatureViewItem();
            Assert.IsNotNull(viewItem);
        }

        [Test]
        public void GivenViewItemList_OnSelectAllItems_SetsAttributeSelectedToTrue()
        {
            var viewModel = new GwswImportDialogViewModel();
            Assert.IsNotNull(viewModel);

            var item1 = new GwswFeatureViewItem() {Selected = false};
            var item2 = new GwswFeatureViewItem() { Selected = false };

            viewModel.GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem>{item1, item2};
            viewModel.GwswFeatureFiles.ForEach( ff => Assert.IsFalse(ff.Selected));

            viewModel.OnSelectAllItems.Execute(null);
            viewModel.GwswFeatureFiles.ForEach(ff => Assert.IsTrue(ff.Selected));
        }

        [Test]
        public void GivenViewItemList_OnClearSelectedList_SetsAttributeSelectedToFalse()
        {
            var viewModel = new GwswImportDialogViewModel();
            Assert.IsNotNull(viewModel);

            var item1 = new GwswFeatureViewItem() { Selected = true };
            var item2 = new GwswFeatureViewItem() { Selected = true };

            viewModel.GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem> { item1, item2 };
            viewModel.GwswFeatureFiles.ForEach(ff => Assert.IsTrue(ff.Selected));

            viewModel.OnClearSelectedList.Execute(null);
            viewModel.GwswFeatureFiles.ForEach(ff => Assert.IsFalse(ff.Selected));
        }

        [Test]
        public void GivenGwswImporterAndGwswFeatureFilesList_OnImportSelectedFeatures_UpdatesListInImporter_WithSelectedItems()
        {
            var viewModel = new GwswImportDialogViewModel(){ Importer = new GwswFileImporter() };
            Assert.IsNotNull(viewModel);
            var item1 = new GwswFeatureViewItem() { FullPath = "test1", Selected = true };
            var item2 = new GwswFeatureViewItem() { FullPath = "test2", Selected = true };
            var item3 = new GwswFeatureViewItem() { FullPath = "test3", Selected = false };

            viewModel.GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem> { item1, item2, item3 };
            
            var filePath = TestHelper.GetTestFilePath(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsNotNull(filePath);
            var value = viewModel.Importer.LoadDefinitionFile(filePath);
            Assert.IsNotNull(value);

            viewModel.OnImportSelectedFeatures.Execute(null);
            var importerFilesToImport = viewModel.Importer.FilesToImport;

            Assert.IsTrue(importerFilesToImport.Contains(item1.FullPath));
            Assert.IsTrue(importerFilesToImport.Contains(item2.FullPath));
            Assert.IsFalse(importerFilesToImport.Contains(item3.FullPath));
        }

    }
}