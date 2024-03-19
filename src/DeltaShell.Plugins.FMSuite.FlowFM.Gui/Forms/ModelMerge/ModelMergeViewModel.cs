using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Controls.Wpf.Services;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelMerge;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Clipboard = DelftTools.Controls.Clipboard;
using ICommand = System.Windows.Input.ICommand;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.ModelMerge
{
    /// <summary>
    /// ViewModel for the <see cref="ModelMergeView"/>.
    /// </summary>
    public class ModelMergeViewModel : INotifyPropertyChanged, IDisposable
    {
        private WaterFlowFMModel newModel;
        private bool modelCanBeMerged;
        private readonly ModelMergeViewLogAppender logAppender;
        private string buttonText;
        private string validationProgressText;
        private string importProgressText;
        private string mergeProgressText;
        private string selectedPath;

        public ModelMergeViewModel()
        {
            buttonText = "Cancel";
            
            ImportModelCommand = new RelayCommand(async (o) => await ImportModel());
            MergeModelsCommand = new RelayCommand(MergeModels);
            CopyConflictsCommand = new RelayCommand(CopyConflictsToClipboard, o => DuplicateNames.Any());
            
            LogEvents = new ObservableCollection<LoggingEvent>();
            DuplicateNames = new ObservableCollection<string>();

            logAppender = new ModelMergeViewLogAppender()
            {
                AddLogAction = (logEvent) =>
                {
                    Application.Current.Dispatcher.Invoke(() => LogEvents.Add(logEvent));
                }
            };
        }
        
        private void CopyConflictsToClipboard(object obj)
        {
            Clipboard.SetText(string.Join("\n", DuplicateNames));
        }

        /// <summary>
        /// Gets or sets the existing model to be merged with a new model.
        /// </summary>
        public WaterFlowFMModel OriginalModel { get; set; }

        /// <summary>
        /// Gets the new model to be merged with the original model.
        /// </summary>
        private WaterFlowFMModel NewModel
        {
            get
            {
                if (newModel == null)
                {
                    newModel = new WaterFlowFMModel();
                }

                return newModel;
            }
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="LogEvents"/>.
        /// </summary>
        public ObservableCollection<LoggingEvent> LogEvents { get; }
        
        public ObservableCollection<string> DuplicateNames { get; }

        /// <summary>
        /// Gets or set the import progress text.
        /// </summary>
        public string ImportProgressText
        {
            get
            {
                return importProgressText;
            }
            private set
            {
                importProgressText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or set the validation progress text.
        /// </summary>
        public string ValidationProgressText
        {
            get
            {
                return validationProgressText;
            }
            private set
            {
                validationProgressText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or set the merge progress text.
        /// </summary>
        public string MergeProgressText
        {
            get
            {
                return mergeProgressText;
            }
            private set
            {
                mergeProgressText = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Gets or sets the button text.
        /// </summary>
        public string ButtonText
        {
            get
            {
                return buttonText;
            }

            private set
            {
                buttonText = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Whether or not the model can be merged.
        /// </summary>
        public bool ModelCanBeMerged
        {
            get
            {
                return modelCanBeMerged;
            }
            set
            {
                modelCanBeMerged = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Command for selecting and importing a model.
        /// </summary>
        public ICommand ImportModelCommand { get; }
        
        /// <summary>
        /// Command to start the merging of models.
        /// </summary>
        public ICommand MergeModelsCommand { get; }
        
        /// <summary>
        /// Command for copying the duplicate names.
        /// </summary>
        public ICommand CopyConflictsCommand { get; }

        /// <summary>
        /// Selected path to import
        /// </summary>
        public string SelectedPath
        {
            get
            {
                return selectedPath;
            }
            private set
            {
                selectedPath = value;
                OnPropertyChanged();
            }
        }

        private async Task ImportModel()
        {
            var fileDialogService = new FileDialogService();
            var fileDialogOptions = new FileDialogOptions { FileFilter = "Mdu file (*.mdu)|*.mdu" };
            
            string filePath = fileDialogService.ShowOpenFileDialog(fileDialogOptions);

            SelectedPath = filePath;

            if (filePath == null || !File.Exists(filePath))
            {
                return;
            }
            
            ClearExistingMessages();
            ModelCanBeMerged = false;

            AddLogAppender();
            await RunImport(filePath);
            RemoveLogAppender();
            
            ValidateMerge();
        }

        private void ClearExistingMessages()
        {
            DuplicateNames.Clear();
            
            LogEvents.Clear();
        }

        private void ValidateMerge()
        {
            ValidationProgressText = "Validating merge...";
            
            var modelMergeValidator = new ModelMergeValidator();
            bool canMerge = modelMergeValidator.Validate(OriginalModel, NewModel);
            
            DuplicateNames.AddRange(modelMergeValidator.DuplicateNames);

            if (canMerge)
            {
                ModelCanBeMerged = true;
            }
            
            string message = $"Done validating. Found {DuplicateNames.Count} merge conflicts.";
            if (DuplicateNames.Any())
            {
                message += " Please clean the models so they no longer contain duplicate ids.";
            }

            ValidationProgressText = message;
        }

        private Task RunImport(string fileName)
        {
            return Task.Run(() =>
            {
                var importer = new WaterFlowFMFileImporter(null)
                {
                    ProgressChanged = (text, current, total) =>
                    {
                        Application.Current.Dispatcher.Invoke(() => ImportProgressText = text);
                    }
                };
                newModel = importer.ImportItem(fileName) as WaterFlowFMModel;
            });
        }

        private void MergeModels(object o)
        {
            MergeProgressText = "Merging models...";
            if (!ModelCanBeMerged)
            {
                MergeProgressText = "Models cannot be merged because there are merge conflicts.";
                return;
            }

            ModelMerger.Merge(OriginalModel, NewModel);
            MergeProgressText = "Done merging.";
            ModelCanBeMerged = false;
            ButtonText = "Close";
        }
        
        private void AddLogAppender()
        {
            Logger rootLogger = ((Hierarchy) LogManager.GetRepository()).Root;
            rootLogger.AddAppender(logAppender);
        }

        private void RemoveLogAppender()
        {
            Logger rootLogger = ((Hierarchy) LogManager.GetRepository()).Root;
            if (rootLogger.Appenders.Contains(logAppender))
            {
                rootLogger.RemoveAppender(logAppender);
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveLogAppender();
                newModel?.Dispose();
            }
        }
    }
}