using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.GWSW.Views;
using log4net;

namespace DeltaShell.Plugins.ImportExport.GWSW.ViewModels
{
    /// <summary>
    /// GwswImportDialogViewModel is a view model for the GwswImport dialog
    /// </summary>
    [Entity]
    public class GwswImportControlViewModel
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswImportControlViewModel));
        private static string selectedDirectoryPath;
        private char otherChar = '-';

        /// <summary>
        /// Initializes a new instance of the <see cref="GwswImportControlViewModel"/> class.
        /// </summary>
        public GwswImportControlViewModel()
        {
            GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem>();
            SelectedDirectoryPath = GetPreviousDirectoryPath();
            SelectedSeparatorType = SeparatorType.Semicolon;
        }
        
        /// <summary>
        /// Gets or sets the importer.
        /// </summary>
        public GwswFileImporter Importer { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public IWaterFlowFMModel Model { get; set; }

        /// <summary>
        /// Gets or sets the delimeter for the feature csv files.
        /// </summary>
        public SeparatorType SelectedSeparatorType { get; set; }

        public string OtherChar
        {
            get
            {
                return new string(new []{otherChar});
            }
            set
            {
                if (value.Length == 1)
                {
                    otherChar = value[0];
                    return;
                }

                throw new InvalidCastException("Could not convert to valid character");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is directory selected, a state of the viewand the user process.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is directory selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectorySelected { get; set; }

        /// <summary>
        /// Gets or sets the selected directory path.
        /// </summary>
        /// <value>
        /// The selected directory path.
        /// </value>
        public string SelectedDirectoryPath
        {
            get { return selectedDirectoryPath; }
            set
            {
                if (string.IsNullOrEmpty(value) || !Directory.Exists(value)) return;
                selectedDirectoryPath = value;
                IsDirectorySelected = true;
                SaveLastDirectoryPath(selectedDirectoryPath);
            }
        }

        /// <summary>
        /// Gets or sets the selected feature file path.
        /// </summary>
        /// <value>
        /// The selected feature file path.
        /// </value>
        public string SelectedFeatureFilePath { get; set; }

        public bool AllFilesSelected
        {
            get { return GwswFeatureFiles != null && GwswFeatureFiles.All( ff => ff.Selected); }
            set { } //The setter triggers the getter in the View.
        }

        /// <summary>
        /// Gets or sets the GWSW feature files, based on the GWSW definition.
        /// </summary>
        /// <value>
        /// The GWSW feature files.
        /// </value>
        public ObservableCollection<GwswFeatureViewItem> GwswFeatureFiles { get; set; }

        /// <summary>
        /// Gets or sets the close action.
        /// </summary>
        /// <value>
        /// The close action.
        /// </value>
        public Action<bool> CloseAction { get; set; }

        /// <summary>
        /// Gets or sets the message action.
        /// </summary>
        /// <value>
        /// The message action.
        /// </value>
        public Action<string, string> ShowInformationMessage { get; set; }

        #region Commands

        /// <summary>
        /// Gets the on select all command.
        /// </summary>
        /// <value>
        /// The on select all.
        /// </value>
        public ICommand OnSelectAll
        {
            get { return new RelayCommand( param => SelectAll());}
        }

        /// <summary>
        /// Gets the on directory selected command.
        /// </summary>
        /// <value>
        /// The on directory selected.
        /// </value>
        public ICommand OnDirectorySelected
        {
            get { return new RelayCommand( param => LoadFeatureFiles()); }
        }


        /// <summary>
        /// Gets the on add custom feature file command.
        /// </summary>
        /// <value>
        /// The on add custom feature file.
        /// </value>
        public ICommand OnAddCustomFeatureFile
        {
            get { return new RelayCommand(param => AddFeatureFile()); }
        }

        /// <summary>
        /// Gets the on configure importer command.
        /// </summary>
        /// <value>
        /// The on configure importer.
        /// </value>
        public ICommand OnConfigureImporter
        {
            get { return new RelayCommand(param => ConfigureImporter()); }
        }

        /// <summary>
        /// Gets the on cancel import features command.
        /// </summary>
        /// <value>
        /// The on cancel import features.
        /// </value>
        public ICommand OnCancelImportFeatures
        {
            get { return new RelayCommand(param => CloseAction(false)); }
        }

        #endregion

        #region Commands Private methods

        private void LoadFeatureFiles()
        {
            if (Importer == null) return;

            Importer.LoadFeatureFiles(SelectedDirectoryPath);

            GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem>();
            Importer.GwswDefaultFeatures.ForEach(
                kv => GwswFeatureFiles.Add(
                    new GwswFeatureViewItem
                    {
                        Selected = Importer.FilesToImport != null && Importer.FilesToImport.Contains(kv.Value[2]),
                        FileName = kv.Key,
                        FileExists = File.Exists(kv.Value[2]),
                        ElementName = kv.Value[0],
                        FeatureType = kv.Value[1],
                        FullPath = kv.Value[2],
                        AfterSelected = () => TriggerSelectedFilesCheckbox() // trigger event
                    }));

            TriggerSelectedFilesCheckbox();
        }
        
        private void SelectAll()
        {
            var currentValue = GwswFeatureFiles.All(ff => ff.Selected);
            GwswFeatureFiles.ForEach(ff =>
            {
                /* Set the value, but disable the after selection effect first.*/
                var action = ff.AfterSelected;
                ff.AfterSelected = null;

                ff.Selected = !currentValue;

                /*Set again the after selection event.*/
                ff.AfterSelected = action;
            });

            TriggerSelectedFilesCheckbox();
        }

        private void AddFeatureFile()
        {
            if (String.IsNullOrEmpty(SelectedFeatureFilePath)) return;
            var fileName = Path.GetFileName(SelectedFeatureFilePath);

            if (GwswFeatureFiles.Any(ff => ff.FullPath.Equals(SelectedFeatureFilePath)))
            {
                var message = $"The file {fileName} with path {SelectedFeatureFilePath}, already exists in the Feature List.";
                ShowInformationMessage?.Invoke("Feature File already exists.", message);
                return;
            }

            var foundFeature = GwswFeatureFiles?.FirstOrDefault(def => def.FileName.Equals(fileName));
            var newItem = new GwswFeatureViewItem
            {
                FileName = fileName,
                FileExists = File.Exists(SelectedFeatureFilePath),
                ElementName = foundFeature?.ElementName,
                FeatureType = foundFeature?.FeatureType,
                FullPath = SelectedFeatureFilePath,
                Selected = true,
                AfterSelected = () => TriggerSelectedFilesCheckbox()
            };
            GwswFeatureFiles?.Insert(0, newItem);
            if(GwswFeatureFiles != null && GwswFeatureFiles.Contains(newItem))
                Log.InfoFormat(Properties.Resources.GwswImportDialogViewModel_AddFeatureFile_Feature_file__0__added_to_the_list_correctly__Path___1_, fileName, SelectedFeatureFilePath);

            TriggerSelectedFilesCheckbox();
        }

        private void ConfigureImporter()
        {
            var pathList = new List<string>();

            //Get the items to import
            GwswFeatureFiles.Where( it => it.Selected ).ForEach( it => pathList.Add(it.FullPath));
            
            //Add the files to import to the importer property, close the window and then launch the importer.
            Importer.FilesToImport = new EventedList<string>(pathList);
            
            //Set delimeter.
            Importer.CsvDelimeter = SelectedSeparatorType.GetChar(otherChar);
            CloseAction?.Invoke(true);
        }

        #endregion

        private void TriggerSelectedFilesCheckbox()
        {
            AllFilesSelected = true; // trigger event
        }

        private static string GetPreviousDirectoryPath()
        {
            return Properties.Settings.Default.Last_GwswImport_FolderPath;
        }

        private static void SaveLastDirectoryPath(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath)) return;
            Properties.Settings.Default.Last_GwswImport_FolderPath = folderPath;
            Properties.Settings.Default.Save();
        }
    }
}