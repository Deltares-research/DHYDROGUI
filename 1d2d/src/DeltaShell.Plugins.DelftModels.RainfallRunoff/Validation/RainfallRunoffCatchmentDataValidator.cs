using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class RainfallRunoffCatchmentDataValidator : IValidator<RainfallRunoffModel, IEnumerable<CatchmentModelData>>
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, IEnumerable<CatchmentModelData> target)
        {
            return new ValidationReport("Concept Data", new List<ValidationIssue>(), GetSubReports(rootObject));
        }

        private IEnumerable<ValidationReport> GetSubReports(RainfallRunoffModel rootObject)
        {
            var reports = new List<ValidationReport>();
            reports.Add(CreateGeneralReport(rootObject.GetAllModelData()));
            reports.Add(new UnpavedDataValidator().Validate(rootObject, rootObject.GetAllModelData().OfType<UnpavedData>()));
            reports.Add(new PavedDataValidator().Validate(rootObject, rootObject.GetAllModelData().OfType<PavedData>()));
            reports.Add(new GreenhouseDataValidator().Validate(rootObject, rootObject.GetAllModelData().OfType<GreenhouseData>()));
            reports.Add(new OpenWaterDataValidator().Validate(rootObject, rootObject.GetAllModelData().OfType<OpenWaterData>()));
            reports.Add(new SacramentoDataValidator().Validate(rootObject, rootObject.GetAllModelData().OfType<SacramentoData>()));
            reports.Add(new HbvDataValidator().Validate(rootObject, rootObject.GetAllModelData().OfType<HbvData>()));

            var meteoReport = new CatchmentMeteoDataValidator().Validate(rootObject, rootObject.GetAllModelData());
            if (meteoReport != null)
                reports.Add(meteoReport);
            return reports;
        }

        private static ValidationReport CreateGeneralReport(IEnumerable<CatchmentModelData> allCatchmentData)
        {
            var issues = new List<ValidationIssue>();

            foreach (var catchmentData in allCatchmentData)
            {
                var calculationArea = catchmentData.CalculationArea;

                if (Math.Abs(calculationArea - 0.0) < 0.0001) //if area == 0.0
                {
                    issues.Add(new ValidationIssue(catchmentData, ValidationSeverity.Warning, "Calculation area is zero"));
                }
                else
                {
                    if (catchmentData.Catchment.Geometry is IPoint)
                        continue; // don't check for point catchments

                    if (calculationArea > 2.0*catchmentData.Catchment.GeometryArea)
                    {
                        issues.Add(new ValidationIssue(catchmentData, ValidationSeverity.Info,
                                                       "Calculation area significantly larger than area of map geometry"));
                    }
                    else if (calculationArea < 0.5*catchmentData.Catchment.GeometryArea)
                    {
                        issues.Add(new ValidationIssue(catchmentData, ValidationSeverity.Info,
                                                       "Calculation area significantly smaller than area of map geometry"));
                    }
                }
            }

            return new ValidationReport("General", issues, new ValidationReport[0]);
        }
    }
}