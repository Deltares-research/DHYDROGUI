using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public enum HeatFluxModelType
    {
        None = 0,
        TransportOnly = 1,
        ExcessTemperature = 3,
        Composite = 5,
    }

    [Entity]
    public class HeatFluxModel
    {
        private HeatFluxModelType modelType;
        private IFunction meteoData;
        private bool containsSolarRadiation;

        public HeatFluxModelType Type
        {
            get { return modelType; }
            set { modelType = value;
                switch (modelType)
                {
                    case HeatFluxModelType.None:
                    case HeatFluxModelType.TransportOnly:
                    case HeatFluxModelType.ExcessTemperature:
                        {
                            meteoData = null;
                            containsSolarRadiation = false;
                        }
                        break;
                    case HeatFluxModelType.Composite:
                        {
                            meteoData = CreateTimeseriesMeteoData();
                            UpdateSolarRadiationInMeteoData();
                        }
                        break;
                    default:
                        throw new NotImplementedException("Type of heat flux model is not yet implemented.");
                }
            }
        }

        public bool CanHaveSolarRadiation
        {
            get { return Type == HeatFluxModelType.ExcessTemperature || Type == HeatFluxModelType.Composite; }
        }

        public bool ContainsSolarRadiation
        {
            get { return containsSolarRadiation; }
            set
            {
                containsSolarRadiation = value;
                UpdateSolarRadiationInMeteoData();
            }
        }

        private void UpdateSolarRadiationInMeteoData()
        {
            if (MeteoData == null) return;
            var solarVariable = MeteoData.Components.FirstOrDefault(v => v.Name.Equals("Solar radiation"));

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

        public IFunction MeteoData { get { return meteoData; } }

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
            result.Components.Add(new Variable<double>("Air temperature")
            {
                Unit = new Unit("degree celsius", "°C")
            });
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
