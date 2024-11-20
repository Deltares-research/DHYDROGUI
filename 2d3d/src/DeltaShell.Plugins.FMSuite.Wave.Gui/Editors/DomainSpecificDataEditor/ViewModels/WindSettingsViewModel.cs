using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeltaShell.Plugins.FMSuite.Common.Wind;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    /// <summary>
    /// View model for the wind settings.
    /// </summary>
    public class WindSettingsViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The meteo data
        /// </summary>
        private readonly WaveMeteoData meteoData;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindSettingsViewModel"/> class.
        /// </summary>
        /// <param name="meteoData">The meteo data.</param>
        public WindSettingsViewModel(WaveMeteoData meteoData)
        {
            this.meteoData = meteoData;
        }

        /// <summary>
        /// Gets or sets the type of the input for wind.
        /// </summary>
        /// <value>
        /// The type of the input.
        /// </value>
        public WindInputType InputType
        {
            get => ConvertToWindInputType(meteoData.FileType);
            set
            {
                if (ConvertToWindInputType(meteoData.FileType) != value)
                {
                    meteoData.FileType = ConvertToWindDefinitionType(value);

                    if (InputType == WindInputType.SpiderWebGrid)
                    {
                        UseSpiderWebGrid = true;
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the X component file path.
        /// </summary>
        /// <value>
        /// The X component file path.
        /// </value>
        public string XComponentFilePath
        {
            get => meteoData.XComponentFilePath;
            set
            {
                if (meteoData.XComponentFilePath != value)
                {
                    meteoData.XComponentFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Y component file path.
        /// </summary>
        /// <value>
        /// The Y component file path.
        /// </value>
        public string YComponentFilePath
        {
            get => meteoData.YComponentFilePath;
            set
            {
                if (meteoData.YComponentFilePath != value)
                {
                    meteoData.YComponentFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the spider web file path.
        /// </summary>
        /// <value>
        /// The spider web file path.
        /// </value>
        public string SpiderWebFilePath
        {
            get => meteoData.SpiderWebFilePath;
            set
            {
                if (meteoData.SpiderWebFilePath != value)
                {
                    meteoData.SpiderWebFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the wind velocity file path.
        /// </summary>
        /// <value>
        /// The wind velocity file path.
        /// </value>
        public string WindVelocityFilePath
        {
            get => meteoData.XYVectorFilePath;
            set
            {
                if (meteoData.XYVectorFilePath != value)
                {
                    meteoData.XYVectorFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the spider web grid.
        /// </summary>
        /// <value>
        /// <c>true</c> if the spider web grid should be used; otherwise, <c>false</c>.
        /// </value>
        public bool UseSpiderWebGrid
        {
            get => meteoData.HasSpiderWeb;
            set
            {
                if (meteoData.HasSpiderWeb != value)
                {
                    meteoData.HasSpiderWeb = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static WindDefinitionType ConvertToWindDefinitionType(WindInputType inputType)
        {
            switch (inputType)
            {
                case WindInputType.SpiderWebGrid:
                    return WindDefinitionType.SpiderWebGrid;
                case WindInputType.WindVector:
                    return WindDefinitionType.WindXY;
                case WindInputType.XYComponents:
                    return WindDefinitionType.WindXWindY;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null);
            }
        }

        private static WindInputType ConvertToWindInputType(WindDefinitionType meteoDataFileType)
        {
            switch (meteoDataFileType)
            {
                case WindDefinitionType.SpiderWebGrid:
                    return WindInputType.SpiderWebGrid;
                case WindDefinitionType.WindXWindY:
                    return WindInputType.XYComponents;
                case WindDefinitionType.WindXY:
                case WindDefinitionType.WindXYP:
                    return WindInputType.WindVector;
                default:
                    throw new ArgumentOutOfRangeException(nameof(meteoDataFileType), meteoDataFileType, null);
            }
        }
    }
}