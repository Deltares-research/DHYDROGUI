using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Extensions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Helpers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor
{
    /// <summary>
    /// <see cref="WindFilesViewModel"/> implements the view model for the
    /// <see cref="Views.TimeFrameEditor.WindFilesView"/>.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    /// <seealso cref="IDisposable" />
    public sealed class WindFilesViewModel : INotifyPropertyChanged,
                                             IDisposable
    {
        private readonly IReadOnlyDictionary<string, string> propertyMapping = new Dictionary<string, string>
        {
            {nameof(WaveMeteoData.FileType), nameof(WindFileType)},
            {nameof(WaveMeteoData.XYVectorFilePath), nameof(WindVelocityPath)},
            {nameof(WaveMeteoData.XComponentFilePath), nameof(XComponentPath)},
            {nameof(WaveMeteoData.YComponentFilePath), nameof(YComponentPath)},
            {nameof(WaveMeteoData.HasSpiderWeb), nameof(UseSpiderWeb)},
            {nameof(WaveMeteoData.SpiderWebFilePath), nameof(SpiderWebPath)},
        };

        private readonly WaveMeteoData waveMeteoData;

        // Note this object takes care of the event propagation for this class.
        private readonly NotifyPropertyChangedEventPropagator eventPropagator;
        private readonly ITimeFrameEditorFileImportHelper importHelper;

        /// <summary>
        /// Creates a new <see cref="WindFilesViewModel"/>.
        /// </summary>
        /// <param name="waveMeteoData">The wave meteo data.</param>
        /// <param name="importHelper">Helper to handle importing of files.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveMeteoData"/> or <paramref name="importHelper"/>
        /// is <c>null</c>.
        /// </exception>
        public WindFilesViewModel(WaveMeteoData waveMeteoData,
                                  ITimeFrameEditorFileImportHelper importHelper)
        {
            Ensure.NotNull(waveMeteoData, nameof(waveMeteoData));
            Ensure.NotNull(importHelper, nameof(importHelper));

            this.waveMeteoData = waveMeteoData;
            this.importHelper = importHelper;

            eventPropagator = new NotifyPropertyChangedEventPropagator((INotifyPropertyChanged)waveMeteoData,
                                                                       OnPropertyChanged,
                                                                       propertyMapping);

            void SetWindVelocityWithUserInput(WindFilesViewModel vm) =>
                vm.WindVelocityPath = vm.importHelper.HandleInputFileImport(TimeFrameFileFilters.WindVelocity) ?? vm.WindVelocityPath;
            WindVelocitySelectPathCommand = new RelayCommand<WindFilesViewModel>(SetWindVelocityWithUserInput);

            void SetXComponentWithUserInput(WindFilesViewModel vm) =>
                vm.XComponentPath = vm.importHelper.HandleInputFileImport(TimeFrameFileFilters.UniformXSeries) ?? vm.XComponentPath;
            XComponentSelectPathCommand = new RelayCommand<WindFilesViewModel>(SetXComponentWithUserInput);


            void SetYComponentWithUserInput(WindFilesViewModel vm) =>
                vm.YComponentPath = vm.importHelper.HandleInputFileImport(TimeFrameFileFilters.UniformYSeries) ?? vm.YComponentPath;
            YComponentSelectPathCommand = new RelayCommand<WindFilesViewModel>(SetYComponentWithUserInput);

            void SetSpiderWebWithUserInput(WindFilesViewModel vm) =>
                vm.SpiderWebPath = vm.importHelper.HandleInputFileImport(TimeFrameFileFilters.SpiderWeb) ?? vm.SpiderWebPath;
            SpiderWebSelectPathCommand = new RelayCommand<WindFilesViewModel>(SetSpiderWebWithUserInput);
        }

        /// <summary>
        /// Gets or sets the type of the wind file.
        /// </summary>
        public WindInputType WindFileType
        {
            get => waveMeteoData.FileType.ConvertToWindInputType();
            set
            {
                if (value != WindFileType)
                {
                    waveMeteoData.FileType = value.ConvertToWindDefinitionType();
                }
            }
        }

        /// <summary>
        /// Gets or sets the wind velocity path.
        /// </summary>
        public string WindVelocityPath
        {
            get => waveMeteoData.XYVectorFilePath;
            set
            {
                if (value != WindVelocityPath)
                {
                    waveMeteoData.XYVectorFilePath = value;
                }
            }
        }

        /// <summary>
        /// Gets the command to select the <see cref="WindVelocityPath"/>.
        /// </summary>
        public System.Windows.Input.ICommand WindVelocitySelectPathCommand { get; }

        /// <summary>
        /// Gets or sets the x component path.
        /// </summary>
        public string XComponentPath
        {
            get => waveMeteoData.XComponentFilePath;
            set
            {
                if (value != XComponentPath)
                {
                    waveMeteoData.XComponentFilePath = value;
                }
            }
        }

        /// <summary>
        /// Gets the command to select the <see cref="XComponentPath"/>.
        /// </summary>
        public System.Windows.Input.ICommand XComponentSelectPathCommand { get; }

        /// <summary>
        /// Gets or sets the y component path.
        /// </summary>
        public string YComponentPath
        {
            get => waveMeteoData.YComponentFilePath;
            set
            {
                if (value != YComponentPath)
                {
                    waveMeteoData.YComponentFilePath = value;
                }
            }
        }

        /// <summary>
        /// Gets the command to select the <see cref="YComponentPath"/>.
        /// </summary>
        public System.Windows.Input.ICommand YComponentSelectPathCommand { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the spider web.
        /// </summary>
        public bool UseSpiderWeb
        {
            get => waveMeteoData.HasSpiderWeb;
            set
            {
                if (value != UseSpiderWeb)
                {
                    waveMeteoData.HasSpiderWeb = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the spider web path.
        /// </summary>
        public string SpiderWebPath
        {
            get => waveMeteoData.SpiderWebFilePath;
            set
            {
                if (value != SpiderWebPath)
                {
                    waveMeteoData.SpiderWebFilePath = value;
                }
            }
        }

        /// <summary>
        /// Gets the command to select the <see cref="SpiderWebPath"/>.
        /// </summary>
        public System.Windows.Input.ICommand SpiderWebSelectPathCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void Dispose()
        {
            if (hasDisposed)
            {
                return;
            }

            eventPropagator?.Dispose();
            hasDisposed = true;
        }

        private bool hasDisposed = false;
    }
}