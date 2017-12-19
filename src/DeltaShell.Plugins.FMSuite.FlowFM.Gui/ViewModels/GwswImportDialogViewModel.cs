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
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.ViewModels
{
    [Entity]
    public class GwswImportDialogViewModel
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswImportDialogViewModel));
        private static string definitionFilePath;

        public GwswFileImporter Importer { get; set; }

        public WaterFlowFMModel Model { get; set; }

        public bool IsDefinitionFileLoaded { get; set; }

        public char SelectedDelimeter { get; set; }

        public char DefinitionDelimeter { get; set; }
        public char FeatureDelimeter { get; set; }

        public string SelectedDefinitionFilePath { get; set; }

        private string DefinitionFilePath
        {
            get { return definitionFilePath; }
            set
            {
                if (!String.IsNullOrEmpty(value) && File.Exists(value))
                    definitionFilePath = value;
            }
        }

        public string SelectedFeatureFilePath { get; set; }

        public bool AllFilesSelected
        {
            get { return GwswFeatureFiles != null && GwswFeatureFiles.All( ff => ff.Selected); }
            set { } //The setter triggers the getter in the View.
        }

        public ObservableCollection<GwswFeatureViewItem> GwswFeatureFiles { get; set; }
        public bool OverwriteGwswFeatureFiles;

        public Action<bool> CloseAction { get; set; }

        public Func<char, char> GetDelimeter { get; set; }

        public Func<string, string, MessageBoxButtons, MessageBoxIcon, bool> MessageAction { get; set; }

        public GwswImportDialogViewModel()
        {
            GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem>();
            definitionFilePath = null;
            //Default Delimeters
            DefinitionDelimeter = ',';
            FeatureDelimeter = ';';
        }

        #region Commands

        public ICommand OnSelectAll
        {
            get { return new RelayCommand( param => SelectAll());}
        }

        public ICommand OnLoadDefinitionFile
        {
            get { return new RelayCommand( param => LoadDefinitionFile()); }
        }

        public ICommand OnAddCustomFeatureFile
        {
            get { return new RelayCommand(param => AddFeatureFile()); }
        }

        public ICommand OnConfigureImporter
        {
            get { return new RelayCommand(param => ConfigureImporter()); }
        }

        public ICommand OnCancelImportFeatures
        {
            get { return new RelayCommand(param => CloseAction(false)); }
        }

        public ICommand OnSetDefinitionDelimeter
        {
            get { return new RelayCommand(param => SetDefinitionDelimeter()); }
        }


        public ICommand OnSetFeatureDelimeter
        {
            get { return new RelayCommand(param => SetFeatureDelimeter()); }
        }


        #endregion

        #region Commands Private methods

        private void SetDefinitionDelimeter()
        {
            DefinitionDelimeter = GetDelimeter?.Invoke(DefinitionDelimeter) ?? DefinitionDelimeter;
        }

        private void SetFeatureDelimeter()
        {
            FeatureDelimeter = GetDelimeter?.Invoke(FeatureDelimeter) ?? FeatureDelimeter;
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

        private void LoadDefinitionFile()
        {
            if (String.IsNullOrEmpty(SelectedDefinitionFilePath)
                || !OverwriteGwswFeatureFiles 
                    && GwswFeatureFiles != null 
                    && GwswFeatureFiles.Any())
            {
                SelectedDefinitionFilePath = DefinitionFilePath ?? string.Empty;
                return;
            }

            //Set delimeter.
            if (Importer != null)
                Importer.CsvDelimeter = DefinitionDelimeter;

            //Load definition file if possible.
            var loadResult = Importer?.LoadDefinitionFile(SelectedDefinitionFilePath);
            if (loadResult == null || Importer.GwswDefaultFeatures == null )
            {
                //Log message in DeltaShell
                Log.ErrorFormat(Resources.GwswImportDialogViewModel_LoadDefinitionFile_Definition_file__0__could_not_be_imported__Path___1_, Path.GetFileName(SelectedDefinitionFilePath), SelectedDefinitionFilePath);
                
                //Display a message window informing the user
                var message = "A problem was found while importing the Definition File, please check the log messages for further details.";
                MessageAction?.Invoke("Error importing Definition File.", message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Clean Definition file path, GwswFeatureFiles and Importer mappings.
                SelectedDefinitionFilePath = string.Empty;
                GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem>();
                IsDefinitionFileLoaded = false;
                return;
            }

            DefinitionFilePath = SelectedDefinitionFilePath;
            Log.InfoFormat(Resources.GwswImportDialogViewModel_LoadDefinitionFile_Definition_file__0__was_imported_correctly__Path___1_, Path.GetFileName(DefinitionFilePath), DefinitionFilePath);

            GwswFeatureFiles = new ObservableCollection<GwswFeatureViewItem>();
            Importer.GwswDefaultFeatures.ForEach(
                kv => GwswFeatureFiles.Add(
                    new GwswFeatureViewItem
                    {
                        Selected = Importer.FilesToImport != null && Importer.FilesToImport.Contains(kv.Value[2]),
                        FileName = kv.Key,
                        ElementName = kv.Value[0],
                        FeatureType = kv.Value[1],
                        FullPath = kv.Value[2],
                        AfterSelected = () => TriggerSelectedFilesCheckbox() // trigger event
                    }));

            IsDefinitionFileLoaded = GwswFeatureFiles.Any();
            TriggerSelectedFilesCheckbox();
        }

        private void AddFeatureFile()
        {
            if (String.IsNullOrEmpty(SelectedFeatureFilePath)) return;
            var fileName = Path.GetFileName(SelectedFeatureFilePath);

            if (GwswFeatureFiles.Any(ff => ff.FullPath.Equals(SelectedFeatureFilePath)))
            {
                var message = string.Format("The file {0} with path {1}, already exists in the Feature List.", fileName, SelectedFeatureFilePath);
                MessageAction?.Invoke("Feature File already exists.", message, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var foundFeature = GwswFeatureFiles?.FirstOrDefault(def => def.FileName.Equals(fileName));
            var newItem = new GwswFeatureViewItem
            {
                FileName = fileName,
                ElementName = foundFeature?.ElementName,
                FeatureType = foundFeature?.FeatureType,
                FullPath = SelectedFeatureFilePath,
                Selected = true,
                AfterSelected = () => TriggerSelectedFilesCheckbox()
            };
            GwswFeatureFiles?.Insert(0, newItem);
            if( GwswFeatureFiles.Contains(newItem))
                Log.InfoFormat(Resources.GwswImportDialogViewModel_AddFeatureFile_Feature_file__0__added_to_the_list_correctly__Path___1_, fileName, SelectedFeatureFilePath);

            TriggerSelectedFilesCheckbox();
        }

        private void ConfigureImporter()
        {
            if (Importer?.GwswAttributesDefinition == null || !Importer.GwswAttributesDefinition.Any()) return;
            var pathList = new List<string>();

            //Get the items to import
            GwswFeatureFiles.Where( it => it.Selected ).ForEach( it => pathList.Add(it.FullPath));
            
            //Add the files to import to the importer property, close the window and then launch the importer.
            Importer.FilesToImport = new EventedList<string>(pathList);
            //Set delimeter.
            Importer.CsvDelimeter = FeatureDelimeter;
            CloseAction?.Invoke(true);
        }

        #endregion

        private void TriggerSelectedFilesCheckbox()
        {
            AllFilesSelected = true; // trigger event
        }
    }

    [Entity]
    public class GwswFeatureViewItem
    {
        private bool selected;

        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                AfterSelected?.Invoke();
            }
        }

        public string FileName { get; set; }

        public string ElementName { get; set; }

        public string FeatureType { get; set; }

        public string FullPath { get; set; }

        public Action AfterSelected { get; set; }
    }
}