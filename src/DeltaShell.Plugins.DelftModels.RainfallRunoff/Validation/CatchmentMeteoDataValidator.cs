using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class CatchmentMeteoDataValidator : IValidator<RainfallRunoffModel, IEnumerable<CatchmentModelData>>
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, IEnumerable<CatchmentModelData> target)
        {
            if (rootObject.Precipitation.DataDistributionType == MeteoDataDistributionType.PerStation)
            {
                var issues = new List<ValidationIssue>();

                foreach (var cad in target)
                {
                    if (string.IsNullOrEmpty(cad.MeteoStationName) ||
                        !rootObject.MeteoStations.Contains(cad.MeteoStationName))
                    {
                        issues.Add(new ValidationIssue(cad, ValidationSeverity.Warning,
                                                       "Unknown meteo station id; the default (first) meteo station will be used"));
                    }
                }

                return new ValidationReport("Meteo Stations", issues);
            }
            if (rootObject.Temperature.DataDistributionType == MeteoDataDistributionType.PerStation)
            {
                var issues = new List<ValidationIssue>();

                foreach (var cad in target)
                {
                    if (string.IsNullOrEmpty(cad.TemperatureStationName) ||
                        !rootObject.TemperatureStations.Contains(cad.TemperatureStationName))
                    {
                        issues.Add(new ValidationIssue(cad, ValidationSeverity.Warning,
                                                       "Unknown temperature station id; the default (first) temperature station will be used"));
                    }
                }

                return new ValidationReport("Temperature Stations", issues);                
            }
            return null;
        }
    }
}