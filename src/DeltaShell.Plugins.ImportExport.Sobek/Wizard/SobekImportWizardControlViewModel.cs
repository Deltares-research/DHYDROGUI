using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;

namespace DeltaShell.Plugins.ImportExport.Sobek.Wizard
{
    public class SobekImportWizardControlViewModel : INotifyPropertyChanged
    {
        private string filePath;
        private string[] cases;
        private string selectedCase;
        private string progressText;
        private int progressTotalSteps;
        private int progressCurrentStep;
        private bool isRunning;
        private bool canImportFlowRtc;
        private bool canImportRR;
        private IEnumerable<IPartialSobekImporter> importersRtc;
        private IEnumerable<IPartialSobekImporter> importersWaterFlow1d;
        private IEnumerable<IPartialSobekImporter> importersRainfallRunoff;

        public SobekImportWizardControlViewModel()
        {
            GetFilepathCommand = new RelayCommand(o=> FilePath = GetFilePath?.Invoke());
            ExecuteCommand = new RelayCommand(async o =>
            {
                StartingImport?.Invoke();
                IsRunning = true;
                try
                {
                    var model = await Task.Run(() =>
                    {
                        SobekImporter.ImportItem(SobekImporter.PathSobek);
                        return SobekImporter.TargetObject;
                    });
                    ExecuteProjectTemplate?.Invoke(model);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    FinishedImport?.Invoke();
                    IsRunning = false;
                }

            }, o => (ImportersWaterFlow1d?.Any() ?? false) ||
                    (ImportersRainfallRunoff?.Any() ?? false) ||
                    (ImportersRtc?.Any() ?? false));

            CancelCommand = new RelayCommand(o => CancelProjectTemplate?.Invoke());

            SobekImporter = new SobekHydroModelImporter
            {
                ProgressChanged = (currentStepName, currentStep, totalSteps) =>
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        ProgressText = currentStepName;
                        ProgressCurrentStep = currentStep;
                        ProgressTotalTotalSteps = totalSteps;
                    });
                }
            };
        }

        public IApplication Application
        {
            get { return SobekImporter.Application;}
            set { SobekImporter.Application = value; }
        }

        public SobekHydroModelImporter SobekImporter { get; }

        public Func<string> GetFilePath { get; set; }

        public bool IsCaseList
        {
            get { return filePath != null && Path.GetExtension(filePath).ToLower() == ".cmt"; }
        }

        public bool HasFileSet
        {
            get { return !string.IsNullOrEmpty(FilePath); }
        }

        public string[] Cases
        {
            get { return cases; }
            set
            {
                cases = value?
                    .Where(c => !string.IsNullOrEmpty(GetCasePath(c)))
                    .ToArray();

                OnPropertyChanged();
            }
        }

        public IEnumerable<IPartialSobekImporter> ImportersWaterFlow1d
        {
            get { return importersWaterFlow1d; }
            set
            {
                importersWaterFlow1d = value; 
                OnPropertyChanged();
            }
        }

        public IEnumerable<IPartialSobekImporter> ImportersRtc
        {
            get { return importersRtc; }
            set
            {
                importersRtc = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<IPartialSobekImporter> ImportersRainfallRunoff
        {
            get { return importersRainfallRunoff; }
            set
            {
                importersRainfallRunoff = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCase
        {
            get { return selectedCase; }
            set
            {
                selectedCase = value;
                if (!string.IsNullOrEmpty(selectedCase))
                {
                    SetSelectedPath(GetCasePath(selectedCase));
                }

                OnPropertyChanged();
            }
        }

        public ICommand GetFilepathCommand { get; set; }
        
        public ICommand ExecuteCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;

                Cases = IsCaseList 
                    ? File.ReadAllLines(filePath) 
                    : new string[0];

                SelectedCase = Cases.FirstOrDefault();

                SetSelectedPath(IsCaseList
                    ? GetCasePath(Cases.FirstOrDefault())
                    : filePath);

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCaseList));
                OnPropertyChanged(nameof(HasFileSet));
            }
        }

        public Action<object> ExecuteProjectTemplate { get; set; }

        public Action CancelProjectTemplate { get; set; }

        public bool CanImportFlowRtc
        {
            get { return canImportFlowRtc; }
            set
            {
                canImportFlowRtc = value;
                OnPropertyChanged();
            }
        }

        public bool CanImportRR
        {
            get { return canImportRR; }
            set
            {
                canImportRR = value;
                OnPropertyChanged();
            }
        }
        public bool ImportRR
        {
            get { return SobekImporter.UseRR; }
            set
            {
                SobekImporter.UseRR = value;
                OnPropertyChanged();
                RefreshImporters();
            }
        }

        public bool ImportRtc
        {
            get { return SobekImporter.UseRTC; }
            set
            {
                SobekImporter.UseRTC = value;
                OnPropertyChanged();
                RefreshImporters();
            }
        }
        public bool ImportFlow
        {
            get { return SobekImporter.UseFm; }
            set
            {
                SobekImporter.UseFm = value;
                OnPropertyChanged();
                RefreshImporters();
            }
        }

        public string ProgressText
        {
            get { return progressText; }
            set
            {
                progressText = value;
                OnPropertyChanged();
            }
        }

        public int ProgressTotalTotalSteps
        {
            get { return progressTotalSteps; }
            set
            {
                progressTotalSteps = value;
                OnPropertyChanged();
            }
        }

        public int ProgressCurrentStep
        {
            get { return progressCurrentStep; }
            set
            {
                progressCurrentStep = value;
                OnPropertyChanged();
            }
        }

        public bool IsRunning
        {
            get { return isRunning; }
            set
            {
                isRunning = value;
                OnPropertyChanged();
            }
        }

        public Action StartingImport { get; set; }

        public Action FinishedImport { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RefreshImporters()
        {
            var importers = GetImporters(SobekImporter).Reverse().ToList();
            ImportersWaterFlow1d = importers.Where(i => i.Category == SobekImporterCategories.WaterFlow1D);
            ImportersRainfallRunoff = importers.Where(i => i.Category == SobekImporterCategories.RainfallRunoff);
            ImportersRtc = importers.Where(i => i.Category == SobekImporterCategories.Rtc);
        }

        private void SetSelectedPath(string path)
        {
            SobekImporter.PathSobek = path;

            if (path?.ToLower().EndsWith("deftop.1") ?? false)
            {
                // This is a SobekRE model. 
                SobekImporter.UseFm = true;
                SobekImporter.UseRTC = true;
                SobekImporter.UseRR = false;
                CanImportFlowRtc = true;
                CanImportRR = false;
            }
            else if (path?.ToLower().EndsWith("network.tp") ?? false)
            {
                string pathSettingsDat = Path.Combine(Path.GetDirectoryName(path)?? "", "settings.dat");
                var settingsDat = "";
                if (File.Exists(pathSettingsDat))
                {
                    settingsDat = File.ReadAllText(pathSettingsDat).ToLower();
                    var indexRestart = settingsDat.IndexOf("[restart]",StringComparison.InvariantCultureIgnoreCase);
                    settingsDat = settingsDat.Substring(0, indexRestart);
                }

                // Add RTC in case that a flow model is detected. In case no controls are detected afterwards, this will be deleted. 
                var hasFlowRtc = settingsDat.Contains("channel=-1") || settingsDat.Contains("river=-1") || settingsDat.Contains("sewer=-1");
                var hasRR = settingsDat.Contains("3b=-1");

                SobekImporter.UseFm = hasFlowRtc;
                SobekImporter.UseRTC = hasFlowRtc;
                SobekImporter.UseRR = hasRR;
                CanImportFlowRtc = hasFlowRtc;
                CanImportRR = hasRR;
            }
            else
            {
                SobekImporter.UseFm = false;
                SobekImporter.UseRTC = false;
                SobekImporter.UseRR = false;
                CanImportFlowRtc = false;
                CanImportRR = false;
            }

            RefreshImporters();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private static IEnumerable<IPartialSobekImporter> GetImporters(IPartialSobekImporter partialImporter)
        {
            while (partialImporter != null)
            {
                if (partialImporter.IsVisible)
                {
                    yield return partialImporter;
                }
                
                partialImporter = partialImporter.PartialSobekImporter;
            }
        }

        private string GetCasePath(string caseName)
        {
            var caseDescription = caseName;
            var caseId = caseDescription.Split(' ')[0];
            
            var caseDirectory = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, caseId);

            var networkFile = Path.Combine(caseDirectory,"network.tp");
            if (File.Exists(networkFile))
            {
                return networkFile;
            }

            var reFile = Path.Combine(caseDirectory, "deftop.1");
            return File.Exists(reFile) 
                    ? reFile 
                    : "";
        }
    }
}