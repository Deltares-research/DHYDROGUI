using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
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
            /*For some reason it crashes (sometimes) when closing it. For what I could read online it's due to the way we call the modal.
             It should be .Show, instead of .ShowModal*/
            WpfTestHelper.ShowModal(dialog);
        }

        [Test]
        public void CreateGwswFeatureViewItem()
        {
            var viewItem = new GwswFeatureViewItem();
            Assert.IsNotNull(viewItem);
        }

        #region OnSelectAll

        [Test]
        [TestCase(false, false, false, true)]
        [TestCase(false, true, false, true)]
        [TestCase(true, false, false, true)]
        [TestCase(true, true, true, false)]
        public void OnSelectAll_AllFilesSelected_GetsExpectedValue(bool selItem1, bool selItem2, bool initialExpectedValue, bool finalExpectedValue)
        {
            var viewModel = new GwswImportDialogViewModel();
            Assert.IsNotNull(viewModel);

            var item1 = new GwswFeatureViewItem { Selected = selItem1 };
            var item2 = new GwswFeatureViewItem { Selected = selItem2 };

            viewModel.GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem> { item1, item2 };
            Assert.AreEqual(initialExpectedValue, viewModel.AllFilesSelected);

            viewModel.OnSelectAll.Execute(null);
            Assert.AreEqual(finalExpectedValue, viewModel.AllFilesSelected);

        }

        [Test]
        public void GivenGwswImporterAndGwswFeatureFilesList_OnImportSelectedFeatures_UpdatesListInImporter_WithSelectedItems()
        {
            var viewModel = new GwswImportDialogViewModel{ Importer = new GwswFileImporter() };
            Assert.IsNotNull(viewModel);
            var item1 = new GwswFeatureViewItem { FullPath = "test1", Selected = true };
            var item2 = new GwswFeatureViewItem { FullPath = "test2", Selected = true };
            var item3 = new GwswFeatureViewItem { FullPath = "test3", Selected = false };

            viewModel.GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem> { item1, item2, item3 };
            
            var filePath = GetValidDefinitionFile();
            var value = viewModel.Importer.LoadDefinitionFile(filePath);
            Assert.IsNotNull(value);

            viewModel.OnConfigureImporter.Execute(null);
            var importerFilesToImport = viewModel.Importer.FilesToImport;

            Assert.IsTrue(importerFilesToImport.Contains(item1.FullPath));
            Assert.IsTrue(importerFilesToImport.Contains(item2.FullPath));
            Assert.IsFalse(importerFilesToImport.Contains(item3.FullPath));
        }

        #endregion

        #region OnLoadDefinitionFile

        [Test]
        public void GivenNullImporter_OnLoadDefinitionFile_LogMessageIsGiven()
        {
            var viewModel = new GwswImportDialogViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            var validDefinitionFile = GetValidDefinitionFile();
            viewModel.SelectedDefinitionFilePath = validDefinitionFile;

            var logMessage = string.Format(Resources.GwswImportDialogViewModel_LoadDefinitionFile_Definition_file__0__could_not_be_imported__Path___1_, Path.GetFileName(validDefinitionFile), validDefinitionFile);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => viewModel.OnLoadDefinitionFile.Execute(null), logMessage);
            Assert.IsNullOrEmpty(viewModel.SelectedDefinitionFilePath);
        }

        [Test]
        public void GivenOverwriteGwswFeatureFiles_IsFalse_PreviousDefinitionAndFiles_Remain()
        {
            var viewModel = new GwswImportDialogViewModel { Importer = new GwswFileImporter() };
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            var validDefinitionFile = GetValidDefinitionFile();

            //We need to execute it to store correctly the private property 'CurrentDefinitionFilePath'.
            viewModel.SelectedDefinitionFilePath = validDefinitionFile;
            Assert.AreEqual(validDefinitionFile, viewModel.SelectedDefinitionFilePath);
            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());
            var firstLoad = viewModel.GwswFeatureFiles;

            //Second execution will do nothing because overwrite is set to false;
            viewModel.OverwriteGwswFeatureFiles = false;
            Assert.AreEqual(validDefinitionFile, viewModel.SelectedDefinitionFilePath);

            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());
            Assert.AreEqual(validDefinitionFile, viewModel.SelectedDefinitionFilePath);
            Assert.AreEqual(firstLoad, viewModel.GwswFeatureFiles);
        }

        [Test]
        public void GivenOverwriteGwswFeatureFiles_IsTrue_PreviousDefinitionAndFiles_AreReplaced()
        {
            var viewModel = new GwswImportDialogViewModel { Importer = new GwswFileImporter() };
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            var validDefinitionFile = GetValidDefinitionFile();
            var fileName = "TestFile";
            viewModel.GwswFeatureFiles.Add( new GwswFeatureViewItem{FileName = fileName});
            Assert.IsTrue(viewModel.GwswFeatureFiles.Any( f => f.FileName == fileName));

            viewModel.OverwriteGwswFeatureFiles = true;
            viewModel.SelectedDefinitionFilePath = validDefinitionFile;

            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());
            Assert.AreEqual(validDefinitionFile, viewModel.SelectedDefinitionFilePath);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any(f => f.FileName == fileName));
        }

        [Test]
        public void GivenACorrectImport_OnLoadDefinitionFile_DefinitionFilePathIsRestored_IfNextImportFails()
        {
            var viewModel = new GwswImportDialogViewModel { Importer = new GwswFileImporter() };
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            var validDefinitionFile = GetValidDefinitionFile();
            var invalidDefinitionFile = "testFilepath.csv";

            //We need to execute it to store correctly the private property 'CurrentDefinitionFilePath'.
            viewModel.SelectedDefinitionFilePath = validDefinitionFile;
            Assert.AreEqual(validDefinitionFile, viewModel.SelectedDefinitionFilePath);

            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());

            viewModel.SelectedDefinitionFilePath = invalidDefinitionFile;
            Assert.AreEqual(invalidDefinitionFile, viewModel.SelectedDefinitionFilePath);

            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());
            Assert.AreEqual(validDefinitionFile, viewModel.SelectedDefinitionFilePath);

        }

        [Test]
        public void GivenCorrectDefinitionFile_OnLoadDefinitionFile_Loads_GwswFeatureFiles_WithSelected_ToTrue()
        {
            var viewModel = new GwswImportDialogViewModel { Importer = new GwswFileImporter() };
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            viewModel.SelectedDefinitionFilePath = GetValidDefinitionFile();
            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());
            Assert.IsTrue(viewModel.GwswFeatureFiles.All(ff => ff.Selected));
        }

        [Test]
        public void GivenCorrectDefinitionFile_OnLoadDefinitionFile_LogMessageIsGiven()
        {
            var viewModel = new GwswImportDialogViewModel { Importer = new GwswFileImporter() };
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            viewModel.SelectedDefinitionFilePath = GetValidDefinitionFile();
            var logMessage = string.Format(Resources.GwswImportDialogViewModel_LoadDefinitionFile_Definition_file__0__was_imported_correctly__Path___1_, Path.GetFileName(viewModel.SelectedDefinitionFilePath), viewModel.SelectedDefinitionFilePath);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => viewModel.OnLoadDefinitionFile.Execute(null), logMessage);

            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());
        }

        [Test]
        public void GivenOverwriteGwswFeatureFiles_FailedBecauseBadDefinitionFile_GwswFeatureFilesAndDefinitionFile_AreCleaned()
        {
            var viewModel = new GwswImportDialogViewModel{ Importer = new GwswFileImporter()};
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            //First load a valid definition file.
            viewModel.SelectedDefinitionFilePath = GetValidDefinitionFile();
            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());

            //Loading a Feature File as Definition will, in most of the cases, result in a failed Loading.
            viewModel.OverwriteGwswFeatureFiles = true;
            var filePath = TestHelper.GetTestFilePath(@"gwswFiles\Knoppunten.csv");
            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsNotNull(filePath);
            viewModel.SelectedDefinitionFilePath = filePath;

            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());
            Assert.IsNullOrEmpty(viewModel.SelectedDefinitionFilePath);
        }

        #endregion

        #region OnAddCustomFeatureFile

        [Test]
        public void GivenInvalidFeatureFile_GwswFeatureFiles_RemainTheSame()
        {
            var viewModel = new GwswImportDialogViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());
            Assert.IsNullOrEmpty(viewModel.SelectedFeatureFilePath);

            viewModel.OnAddCustomFeatureFile.Execute(null);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());
        }

        [Test]
        public void GivenValidFeatureFile_GwswFeatureFiles_AddsNewItem_WithSelectedToTrue_AndLogMessageIsGiven()
        {
            var viewModel = new GwswImportDialogViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            var filePath = "validPath";
            viewModel.SelectedFeatureFilePath = filePath;

            var logMessage = string.Format(Resources.GwswImportDialogViewModel_AddFeatureFile_Feature_file__0__added_to_the_list_correctly__Path___1_, Path.GetFileName(filePath), filePath);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => viewModel.OnAddCustomFeatureFile.Execute(null), logMessage);

            Assert.IsTrue(viewModel.GwswFeatureFiles.Any(ff => ff.FullPath == filePath));
            Assert.IsTrue(viewModel.GwswFeatureFiles.All( ff => ff.Selected));
        }

        [Test]
        /*The path is not really used as it is not check for existence of file or not. Only used to map the type of feature.*/
        [TestCase(@"gwswFiles\sub\Kunstwerk.csv", "Kunstwerk.csv", "Structure", "Structure")]
        public void GivenValidFeatureFile_GwswFeatureFiles_AddsNewRepeatedItem_WithExpectedProperties(string path, string expectedFileName, string expectedElementName, string expectedFeatureType)
        {
            var viewModel = new GwswImportDialogViewModel{ Importer = new GwswFileImporter()};
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            //Load definition file and all it´s features
            var filePath = TestHelper.GetTestFilePath(path);
            viewModel.SelectedDefinitionFilePath = TestHelper.GetTestFilePath(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());
            var numberOfFeatures = viewModel.GwswFeatureFiles.Count;

            //Load given file, check the properties are as expected.
            viewModel.SelectedFeatureFilePath = filePath;
            viewModel.OnAddCustomFeatureFile.Execute(null);
            Assert.AreEqual(numberOfFeatures + 1, viewModel.GwswFeatureFiles.Count);


            Assert.IsTrue(viewModel.GwswFeatureFiles.Any(ff=> ff.FullPath == filePath));
            var featureFile = viewModel.GwswFeatureFiles.FirstOrDefault(ff => ff.FullPath == filePath);
            Assert.IsNotNull(featureFile);
            Assert.AreEqual(Path.GetFileName(filePath), featureFile.FileName);
            Assert.AreEqual(expectedElementName, featureFile.ElementName);
            Assert.AreEqual(expectedFeatureType, featureFile.FeatureType);
            Assert.AreEqual(filePath, featureFile.FullPath);
            Assert.AreEqual(true, featureFile.Selected);
        }

        [Test]
        public void GivenRepeatedFeatureFile_GwswFeatureFiles_RemainTheSame()
        {
            var viewModel = new GwswImportDialogViewModel();
            Assert.IsNotNull(viewModel);

            var validPath = "testPath";
            viewModel.GwswFeatureFiles.Add(new GwswFeatureViewItem{FullPath = validPath});
            Assert.AreEqual( 1, viewModel.GwswFeatureFiles.Count);

            viewModel.SelectedFeatureFilePath = validPath;
            viewModel.OnAddCustomFeatureFile.Execute(null);
            Assert.AreEqual(1, viewModel.GwswFeatureFiles.Count);
        }
        #endregion

        #region OnConfigureImporter

        [Test]
        public void GivenNullImporter_OnConfigureImporter_DoesNotCrash()
        {
            var viewModel = new GwswImportDialogViewModel();
            try
            {
                viewModel.OnConfigureImporter.Execute(null);
            }
            catch (Exception)
            {
                Assert.Fail("It crashed.");
            }

        }

        [Test]
        public void GivenImporter_WithoutGwswAttributesDefinition_OnConfigureImporter_DoesNotModify_Importer_FilesToImport()
        {
            var importer = new GwswFileImporter();
            var testfile = "TestFile";
            importer.FilesToImport.Add(testfile);
            Assert.IsTrue(importer.FilesToImport.Any());

            var viewModel = new GwswImportDialogViewModel{ Importer = importer};

            viewModel.OnConfigureImporter.Execute(null);
            Assert.IsTrue(importer.FilesToImport.Any( f => f == testfile));

        }

        [Test]
        public void GivenImporter_WithGwswAttributesDefinition_OnConfigureImporter_SetsEmptyList_To_Importer_FilesToImport_IfNoGwswFeatureFilesExist()
        {
            var importer = new GwswFileImporter();
            importer.GwswAttributesDefinition.Add(new GwswAttributeType());

            var testfile = "TestFile";
            importer.FilesToImport.Add(testfile);
            Assert.IsTrue(importer.FilesToImport.Any());

            var viewModel = new GwswImportDialogViewModel { Importer = importer };

            viewModel.OnConfigureImporter.Execute(null);
            Assert.IsFalse(importer.FilesToImport.Any());
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void GivenImporter_WithGwswAttributesDefinition_OnConfigureImporter_SetsSelectedFiles_Or_EmptyList_To_Importer_FilesToImport(bool selectedFiles)
        {
            var importer = new GwswFileImporter();
            importer.GwswAttributesDefinition.Add(new GwswAttributeType());

            var testfile = "TestFile";
            importer.FilesToImport.Add(testfile);
            Assert.IsTrue(importer.FilesToImport.Any());
            
            var viewModel = new GwswImportDialogViewModel { Importer = importer };

            viewModel.GwswFeatureFiles.Add(
                new GwswFeatureViewItem
                {
                    Selected = selectedFiles
                });

            viewModel.OnConfigureImporter.Execute(null);

            //in any case, testFile should no longer be in files to import
            Assert.IsFalse(importer.FilesToImport.Any(f => f == testfile));
            Assert.AreEqual(selectedFiles, importer.FilesToImport.Any());
        }

        [Test]
        public void GivenOverwriteGwswFeatureFiles_FailedBecauseBadDefinitionFile_NothingIsImported()
        {
            var viewModel = new GwswImportDialogViewModel { Importer = new GwswFileImporter() };
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            //First load a valid definition file.
            viewModel.SelectedDefinitionFilePath = GetValidDefinitionFile();
            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());

            //Loading a Feature File as Definition will, in most of the cases, result in a failed Loading.
            viewModel.OverwriteGwswFeatureFiles = true;
            var filePath = TestHelper.GetTestFilePath(@"gwswFiles\Knoppunten.csv");
            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsNotNull(filePath);
            viewModel.SelectedDefinitionFilePath = filePath;

            viewModel.OnLoadDefinitionFile.Execute(null);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());
            Assert.IsNullOrEmpty(viewModel.SelectedDefinitionFilePath);

            //Now Try to Configure the import and import.
            try
            {
                viewModel.OnConfigureImporter.Execute(null);

                Assert.IsFalse(viewModel.GwswFeatureFiles.Any());
                Assert.IsFalse(viewModel.Importer.FilesToImport.Any());
            }
            catch (Exception e)
            {
                Assert.Fail("Should not throw exception.");
            }
        }

        #endregion

        private static string GetValidDefinitionFile()
        {
            var filePath = TestHelper.GetTestFilePath(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsNotNull(filePath);
            return filePath;
        }
    }
}