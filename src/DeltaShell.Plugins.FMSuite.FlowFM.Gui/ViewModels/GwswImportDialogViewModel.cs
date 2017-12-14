using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.ViewModels
{
    [Entity]
    public class GwswImportDialogViewModel
    {
        public GwswFileImporter Importer { get; set; }

        public WaterFlowFMModel Model { get; set; }

        public bool IsDefinitionFileLoaded { get; set; }

        public string DefinitionFilePath { get; set; }

        public ObservableCollection<GwswFeatureViewItem> GwswFeatureFiles { get; set; }

        public Action<bool> CloseAction { get; set; }

        public string LogMessage { get; set; }

        #region Commands

        public ICommand OnSelectAllItems
        {
            get { return new RelayCommand( param => SelectAllItems());}
        }

        public ICommand OnClearSelectedList
        {
            get { return new RelayCommand(param => ClearSelectedList());}
        }

        public ICommand OnLoadDefinitionFile
        {
            get { return new RelayCommand( param => LoadDefinitionFile()); }
        }

        public ICommand OnAddCustomFeatureFile
        {
            get { return new RelayCommand(param => AddFeatureFile()); }
        }

        public ICommand OnImportSelectedFeatures
        {
            get { return new RelayCommand(param => ConfigureImporter()); }
        }

        public ICommand OnCancelImportFeatures
        {
            get { return new RelayCommand(param => CloseAction(false)); }
        }

        #endregion

        private void SelectAllItems()
        {
            GwswFeatureFiles.ForEach(ff => ff.Selected = true);
        }

        private void ClearSelectedList()
        {
            GwswFeatureFiles.ForEach( ff => ff.Selected = false);
        }

        private void LoadDefinitionFile()
        {
            var dialog = new OpenFileDialog { Filter = Importer.FileFilter };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var selectedFilePath = dialog.FileName;
                try
                {
                    Importer.LoadDefinitionFile(selectedFilePath);
                }
                catch (Exception)
                {

                    LogMessage =
                        string.Format(
                            "Definition file {0} could not be imported. Please make sure it is in the correct format. " +
                            "\nPath: {1}",
                            Path.GetFileName(selectedFilePath), selectedFilePath);
                    return;
                }

                DefinitionFilePath = selectedFilePath;
                LogMessage = string.Format("Definition file {0} was imported correctly." +
                                           "\nPath: {1}", Path.GetFileName(DefinitionFilePath), DefinitionFilePath);

                GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem>();
                Importer.GwswDefaultFeatures.ForEach(
                    kv => GwswFeatureFiles.Add(
                        new GwswFeatureViewItem
                        {
                            Selected = Importer.FilesToImport != null && Importer.FilesToImport.Contains(kv.Value[2]),
                            FileName = kv.Key,
                            ElementName = kv.Value[0],
                            FeatureType = kv.Value[1],
                            FullPath = kv.Value[2]
                        }));
                IsDefinitionFileLoaded = GwswFeatureFiles.Any();
            }
        }
        private void AddFeatureFile()
        {
            var dialog = new OpenFileDialog{ Filter = Importer.FileFilter };
            var result = dialog.ShowDialog();

            if (result != DialogResult.OK) return;

            var dialogFullPath = dialog.FileName;
            var fileName = Path.GetFileName(dialogFullPath);

            if (String.IsNullOrEmpty(dialogFullPath)) return;
            if (GwswFeatureFiles.Any(ff => ff.FullPath.Equals(dialogFullPath)))
            {
                LogMessage = string.Format("Feature file {0} is already in the list.\nPath: {1}", fileName, dialogFullPath);
                return;
            }

            var foundFeature = GwswFeatureFiles?.FirstOrDefault(def => def.FileName.Equals(fileName));
            var newItem = new GwswFeatureViewItem
            {
                FileName = fileName,
                ElementName = foundFeature?.ElementName,
                FeatureType = foundFeature?.FeatureType,
                FullPath = dialogFullPath,
                Selected = true
            };
            GwswFeatureFiles?.Insert(0, newItem);
            if( GwswFeatureFiles.Contains(newItem))
                LogMessage = string.Format("Feature file {0} added to the list correctly.\nPath: {1}", fileName, dialogFullPath);
        }
        private void ConfigureImporter()
        {
            if (Importer.GwswAttributesDefinition == null || !Importer.GwswAttributesDefinition.Any()) return;
            var pathList = new List<string>();
            var directoryName = Path.GetDirectoryName(DefinitionFilePath);

            //Get the items to import
            GwswFeatureFiles.Where( it => it.Selected ).ForEach( it => pathList.Add(Path.Combine(directoryName, it.FileName)));
            
            //Add the files to import to the importer property, close the window and then launch the importer.
            Importer.FilesToImport = new EventedList<string>(pathList);
            CloseAction(true);
        }
    }

    [Entity]
    public class GwswFeatureViewItem
    {
        public bool Selected { get; set; }
        public string FileName { get; set; }
        public string ElementName { get; set; }
        public string FeatureType { get; set; }
        public string FullPath { get; set; }
    }
}