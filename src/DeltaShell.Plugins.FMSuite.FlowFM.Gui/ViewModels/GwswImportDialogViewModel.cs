using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.ViewModels
{
    [Entity]
    public class GwswImportDialogViewModel
    {
        public GwswFileImporter Importer { get; set; }

        public WaterFlowFMModel Model { get; set; }

        public bool IsDefinitionFileLoaded { get; set; }

        public bool ImportCustomFeature { get; set; }
        
        public ICommand OnSelectDefinitionFile
        {
            get { return new RelayCommand(param => SelectDefinitionFile()); }
        }

        public ICommand OnLoadDefinitionFile
        {
            get { return new RelayCommand( param => LoadDefinitionFile()); }
        }

        public ICommand OnSelectCustomFeatureFile
        {
            get { return new RelayCommand(param => SelectFeatureFile()); }
        }

        public ICommand OnImportSelectedFeatures
        {
            get { return new RelayCommand(param => ImportFeatures()); }
        }

        public string CustomFeatureFilePath { get; set; }

        public string DefinitionFilePath { get; set; }

        public ObservableCollection<KeyValuePair<string, string>> GwswFeatureFiles { get; set; }
        public IList<KeyValuePair<string, string>> SelectedItems { get; set; }

        private void LoadDefinitionFile()
        {
            if (!String.IsNullOrEmpty(DefinitionFilePath))
            {
                //Missing to add the network to this view model. How?
                Importer.ImportDefinitionFile(DefinitionFilePath);
                GwswFeatureFiles = new ObservableCollection<KeyValuePair<string, string>>();
                SelectedItems = new List<KeyValuePair<string, string>>();
                Importer.GwswDefaultFeatures.ForEach( kv => GwswFeatureFiles.Add(new KeyValuePair<string, string>(kv.Key, kv.Value)));
                IsDefinitionFileLoaded = GwswFeatureFiles.Any();
            }
        }

        private void SelectDefinitionFile()
        {
            var dialog = new OpenFileDialog{Filter = Importer.FileFilter};
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                //Missing to add the network to this view model. How?
                DefinitionFilePath = dialog.FileName;
            }
        }
        private void SelectFeatureFile()
        {
            var dialog = new OpenFileDialog{ Filter = Importer.FileFilter };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                CustomFeatureFilePath = dialog.FileName;
            }
        }

        private RelayCommand closeCommand;
        private void ImportFeatures()
        {
            if (Importer.GwswAttributesDefinition == null || !Importer.GwswAttributesDefinition.Any()) return;
            var pathList = new List<string>();
            var directoryName = Path.GetDirectoryName(DefinitionFilePath);

            //Get the items to import
            SelectedItems.ForEach( it => pathList.Add(Path.Combine(directoryName, it.Key)));
            if (ImportCustomFeature && !String.IsNullOrEmpty(CustomFeatureFilePath))
                pathList.Add(CustomFeatureFilePath);
            
            //Import action
            Importer.ImportFeatureFileList(pathList, Model);
            CloseAction();
        }
        public Action CloseAction { get; set; }
    }
}