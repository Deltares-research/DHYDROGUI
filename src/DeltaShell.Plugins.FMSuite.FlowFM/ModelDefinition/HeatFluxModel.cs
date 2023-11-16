using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    [Entity]
    public class HeatFluxModel
    {
        private HeatFluxModelType modelType;
        private IFunction meteoData;
        private bool containsSolarRadiation;
        public string GridFilePath { get; set; }
        public string GriddedHeatFluxFilePath { get; set; }
        public IFunction MeteoData => meteoData;

        public HeatFluxModelType Type
        {
            get => modelType;
            set
            {
                modelType = value;
                switch (modelType)
                {
                    case HeatFluxModelType.None:
                    case HeatFluxModelType.TransportOnly:
                    case HeatFluxModelType.ExcessTemperature:
                    {
                        meteoData = null;
                        containsSolarRadiation = false;
                        GridFilePath = null;
                        GriddedHeatFluxFilePath = null;
                    }
                        break;
                    case HeatFluxModelType.Composite:
                    {
                        GridFilePath = null;
                        GriddedHeatFluxFilePath = null;
                        meteoData = CreateTimeseriesMeteoData();
                        UpdateSolarRadiationInMeteoData();
                    }
                        break;
                    default:
                        throw new NotImplementedException("Type of heat flux model is not yet implemented.");
                }
            }
        }

        public bool CanHaveSolarRadiation =>
            Type == HeatFluxModelType.ExcessTemperature || Type == HeatFluxModelType.Composite;

        public bool ContainsSolarRadiation
        {
            get => containsSolarRadiation;
            set
            {
                containsSolarRadiation = value;
                UpdateSolarRadiationInMeteoData();
            }
        }

        /// <summary>
        /// CopyTo method copies file-based model data to save location. switchTo is true for saving and false for exporting.
        /// </summary>
        /// <param name="destinationPath"></param>
        /// <param name="switchTo"></param>
        public void CopyTo(string destinationPath, bool switchTo = true)
        {
            if (!File.Exists(GriddedHeatFluxFilePath))
            {
                throw new FileNotFoundException($"Could not find heat flux data file {GriddedHeatFluxFilePath}");
            }

            if (!File.Exists(GridFilePath))
            {
                throw new FileNotFoundException($"Could not find heat flux grid file {GridFilePath}");
            }

            string sourceGriddedHeatFluxFile = Path.GetFullPath(GriddedHeatFluxFilePath);
            string sourceGridFile = Path.GetFullPath(GridFilePath);
            destinationPath = Path.GetFullPath(destinationPath);

            string targetDirectory = Path.GetDirectoryName(destinationPath);

            FileUtils.CreateDirectoryIfNotExists(targetDirectory);

            if (sourceGriddedHeatFluxFile != destinationPath)
            {
                File.Copy(GriddedHeatFluxFilePath, destinationPath, true);
                GriddedHeatFluxFilePath = switchTo ? destinationPath : GriddedHeatFluxFilePath;
            }

            string destGridFilePath = GetCorrespondingGridFilePath(destinationPath);
            if (sourceGridFile != Path.GetFullPath(destGridFilePath))
            {
                File.Copy(GridFilePath, destGridFilePath, true);
                GridFilePath = switchTo ? destGridFilePath : GridFilePath;
            }
        }

        public static string GetCorrespondingGridFilePath(string filePath)
        {
            return HtcFile.GetCorrespondingGridFilePath(filePath);
        }

        private void UpdateSolarRadiationInMeteoData()
        {
            if (MeteoData == null)
            {
                return;
            }

            IVariable solarVariable = MeteoData.Components.FirstOrDefault(v => v.Name.Equals("Solar radiation"));

            if (containsSolarRadiation)
            {
                if (solarVariable == null && CanHaveSolarRadiation)
                {
                    MeteoData.Components.Add(new Variable<double>("Solar radiation")
                    {
                        Unit = new Unit("Irradiance", "W/m2"),
                        DefaultValue = 0
                    });
                }
            }
            else
            {
                if (solarVariable != null)
                {
                    MeteoData.Components.Remove(solarVariable);
                }
            }
        }

        private IFunction CreateTimeseriesMeteoData()
        {
            IFunction result = new Function("Meteo data");
            result.Arguments.Add(new Variable<DateTime>("Time"));
            result.Components.Add(new Variable<double>("Humidity")
            {
                Unit = new Unit("percent", "%"),
                MinValidValue = 0d,
                MaxValidValue = 100d
            });
            result.Components.Add(new Variable<double>("Air temperature") {Unit = new Unit("degree celsius", "°C")});
            result.Components.Add(new Variable<double>("Cloud coverage")
            {
                Unit = new Unit("percent", "%"),
                MinValidValue = 0d,
                MaxValidValue = 100d
            });

            return result;
        }
    }
}