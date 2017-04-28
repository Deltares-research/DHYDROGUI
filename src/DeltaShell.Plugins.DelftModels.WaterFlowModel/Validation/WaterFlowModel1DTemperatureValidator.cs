using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public class WaterFlowModel1DTemperatureValidator
    {
        public static ValidationReport Validate(WaterFlowModel1D model)
        {
            return new ValidationReport("Temperature", new[]
            {
                ValidateBoundaryConditions(model),
                ValidateLateralData(model),
                ValidateModelParameters(model),
                ValidateInitialTemperature(model)

            });
        }

        private static ValidationReport ValidateBoundaryConditions(WaterFlowModel1D model)
        {
            /*  If a model has temperature enabled and a boundary with flow data, temperature data MUST be set too
                (much like salt currently works, see ValidateBoundaryConditions)*/
            var issues = new List<ValidationIssue>();
            if (model.UseTemperature)
            {
                var invalidTemperatureBoundaryConditions = model.BoundaryConditions
                    .Where(bc => bc.DataType != WaterFlowModel1DBoundaryNodeDataType.None
                                 && bc.TemperatureConditionType == TemperatureBoundaryConditionType.None);

                foreach (var bc in invalidTemperatureBoundaryConditions)
                {
                    issues.Add(new ValidationIssue(bc, ValidationSeverity.Error, string.Format(
                       Resources.WaterFlowModel1DTemperatureValidator_ValidateBoundaryConditions_The_boundary_condition__0__has_a_temperature_type_of_None__All_open_boundaries_must_specify_temperature_values_,
                        bc.Name)));
                }

                var tempBoundaryConditions =
                    model.BoundaryConditions.Where(
                        bc => bc.TemperatureConditionType != TemperatureBoundaryConditionType.None);
                foreach (var bc in tempBoundaryConditions)
                {
                    if (bc.TemperatureConditionType == TemperatureBoundaryConditionType.Constant)
                    {
                        if (bc.TemperatureConstant > model.backgroundTemperatureMax
                            || bc.TemperatureConstant < model.backgroundTemperatureMin)
                        {
                            issues.Add(new ValidationIssue(bc, ValidationSeverity.Error, GenerateExtendedErrorMessage("Boundary Condition temperature", bc.TemperatureConstant, model.backgroundTemperatureMin, model.backgroundTemperatureMax, model.backgroundTemperatureDefault)));
                        }
                    }
                    else if (bc.TemperatureConditionType == TemperatureBoundaryConditionType.TimeDependent)
                    {
                        var invalidTempBCTimeSeries = bc.TemperatureTimeSeries.GetValues();
                        foreach (var invalidBC in invalidTempBCTimeSeries)
                        {
                            double doubleBC;
                            if (Double.TryParse(invalidBC.ToString(), out doubleBC))
                            {
                                if (doubleBC > model.backgroundTemperatureMax
                                    || doubleBC < model.backgroundTemperatureMin)
                                {
                                    issues.Add(new ValidationIssue(bc, ValidationSeverity.Error, GenerateExtendedErrorMessage("Boundary Condition temperature", doubleBC, model.backgroundTemperatureMin, model.backgroundTemperatureMax, model.backgroundTemperatureDefault)));
                                }
                            }
                            else
                            {
                                issues.Add(new ValidationIssue(bc, ValidationSeverity.Error, Resources.WaterFlowModel1DTemperatureValidator_ValidateModelParameters_Values_should_be_in_double_format_));
                            }
                        }
                    }
                }
            }

            return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_Boundary_conditions, issues);

        }

        private static ValidationReport ValidateLateralData(WaterFlowModel1D model)
        {
            var issues = new List<ValidationIssue>();
            if (model.UseTemperature)
            {
                var tempLateralData =
                    model.LateralSourceData.Where(ls => ls.TemperatureLateralDischargeType != TemperatureLateralDischargeType.None);
                foreach (var ld in tempLateralData)
                {
                    if (ld.TemperatureLateralDischargeType == TemperatureLateralDischargeType.Constant)
                    {
                        if (ld.TemperatureConstant > model.backgroundTemperatureMax
                            || ld.TemperatureConstant < model.backgroundTemperatureMin)
                        {
                            issues.Add(new ValidationIssue(ld, ValidationSeverity.Error, GenerateExtendedErrorMessage("Lateral data temperature", ld.TemperatureConstant, model.backgroundTemperatureMin, model.backgroundTemperatureMax, model.backgroundTemperatureDefault)));
                        }
                    }
                    else if (ld.TemperatureLateralDischargeType == TemperatureLateralDischargeType.TimeDependent)
                    {
                        var invalidTempBCTimeSeries = ld.TemperatureTimeSeries.GetValues();
                        foreach (var invalidLD in invalidTempBCTimeSeries)
                        {
                            double doubleLD;
                            if (Double.TryParse(invalidLD.ToString(), out doubleLD))
                            {
                                if (doubleLD > model.backgroundTemperatureMax
                                    || doubleLD < model.backgroundTemperatureMin)
                                {
                                    issues.Add(new ValidationIssue(ld, ValidationSeverity.Error, GenerateExtendedErrorMessage("Lateral data temperature", doubleLD, model.backgroundTemperatureMin, model.backgroundTemperatureMax, model.backgroundTemperatureDefault)));
                                }
                            }
                            else
                            {
                                issues.Add(new ValidationIssue(ld, ValidationSeverity.Error, Resources.WaterFlowModel1DTemperatureValidator_ValidateModelParameters_Values_should_be_in_double_format_));
                            }
                        }
                    }
                }
            }

            return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_Boundary_conditions, issues);
        }

        private static ValidationReport ValidateModelParameters(WaterFlowModel1D model)
        {
            var issues = new List<ValidationIssue>();
            if (model.UseTemperature && model.TemperatureModelType == TemperatureModelType.Composite)
            {
                //Dalton number 0.0 - 1.0 - 0.00130 - ?
                if (model.DaltonNumber < model.daltonNumberMin
                    || model.DaltonNumber > model.daltonNumberMax)
                    issues.Add(new ValidationIssue(model.DaltonNumber, ValidationSeverity.Error, GenerateExtendedErrorMessage("Dalton Number", model.DaltonNumber, model.daltonNumberMin, model.daltonNumberMax, model.daltonNumberDefault)));
                //Stanton number 0.0 - 1.0 - 0.00130 - ?
                if (model.StantonNumber < model.stantonNumberMin
                    || model.StantonNumber > model.stantonNumberMax)
                    issues.Add(new ValidationIssue(model.StantonNumber, ValidationSeverity.Error, GenerateExtendedErrorMessage("Stanton Number", model.StantonNumber, model.stantonNumberMin, model.stantonNumberMax, model.stantonNumberDefault)));
                //Temperature 0.0 - 60.0 - 0.0 - C
                if(model.BackgroundTemperature < model.backgroundTemperatureMin
                    || model.BackgroundTemperature > model.backgroundTemperatureMax)
                    issues.Add(new ValidationIssue(model.BackgroundTemperature, ValidationSeverity.Error, GenerateExtendedErrorMessage("Background Temperature", model.BackgroundTemperature, model.backgroundTemperatureMin, model.backgroundTemperatureMax, model.backgroundTemperatureDefault)));
                //Water surface 0.0 - max - 0.0 - m2
                if (model.SurfaceArea < model.surfaceAreaMin)
                    issues.Add(new ValidationIssue(model.SurfaceArea, ValidationSeverity.Error, GenerateExtendedErrorMessage("Surface Area", model.SurfaceArea, model.surfaceAreaMin, null, model.surfaceAreaDefault)));

                //Solar radiation 0.0 - max - 0.0 - W/m2
                foreach (var airValue in model.MeteoData.AirTemperature.Values)
                {
                    double airValueDouble;
                    if (Double.TryParse(airValue.ToString(), out airValueDouble))
                    {
                        if (airValueDouble < model.backgroundTemperatureMin || airValueDouble > model.backgroundTemperatureMax)
                        {
                            issues.Add(new ValidationIssue(model.MeteoData, ValidationSeverity.Error, GenerateExtendedErrorMessage("Air Temperature", airValueDouble, model.backgroundTemperatureMin, model.backgroundTemperatureMax, null)));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(model.MeteoData, ValidationSeverity.Error, Resources.WaterFlowModel1DTemperatureValidator_ValidateModelParameters_Values_should_be_in_double_format_));
                    }
                }

                foreach (var cloudValue in model.MeteoData.Cloudiness.Values)
                {
                    double cloudValueDouble;
                    if (Double.TryParse(cloudValue.ToString(), out cloudValueDouble))
                    {
                        if (cloudValueDouble < 0.0 || cloudValueDouble > 100)
                        {
                            issues.Add(new ValidationIssue(model.MeteoData, ValidationSeverity.Error, GenerateExtendedErrorMessage("Cloudiness", cloudValueDouble, 0.0, 100, null)));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(model.MeteoData, ValidationSeverity.Error, Resources.WaterFlowModel1DTemperatureValidator_ValidateModelParameters_Values_should_be_in_double_format_));
                    }
                }
                foreach (var humidityValue in model.MeteoData.RelativeHumidity.Values)
                {
                    double humidityValueDouble;
                    if (Double.TryParse(humidityValue.ToString(), out humidityValueDouble))
                    {
                        if (humidityValueDouble < 0.0 || humidityValueDouble > 100)
                        {
                            issues.Add(new ValidationIssue(model.MeteoData, ValidationSeverity.Error, GenerateExtendedErrorMessage("Humidity", humidityValueDouble, 0.0, 100, null)));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(model.MeteoData, ValidationSeverity.Error, Resources.WaterFlowModel1DTemperatureValidator_ValidateModelParameters_Values_should_be_in_double_format_));
                    }
                }
            }

            return new ValidationReport(Resources.WaterFlowModel1DModelTemperatureValidator_ValidateModelParameters_Temperature_parameters, issues);
        }

        private static string GenerateExtendedErrorMessage(string parameterName, double currentValue, double parameterMin, double? parameterMax, double? parameterDefault)
        {
            var defVal = parameterDefault != null ? string.Format("Default value: {0}.", parameterDefault) : "";
            var maxVal = parameterMax != null ? string.Format("Upper limit: {0}.", parameterMax) : "";

            return string.Format("{0} value ({1}) out of range. {2} Lower limit: {3}. {4}",
                parameterName, currentValue, defVal, parameterMin, maxVal);
        }


        private static ValidationReport ValidateInitialTemperature(WaterFlowModel1D model)
        {
            var issues = new List<ValidationIssue>();
            if (model.UseTemperature && model.InitialTemperature != null)
            {
                foreach (var tempValue in model.InitialTemperature.GetValues())
                {
                    try
                    {
                        var result = Convert.ToDouble(tempValue);
                        if (result > 60.0 || result < 0)
                        {
                            issues.Add(new ValidationIssue(model.InitialTemperature, ValidationSeverity.Error, GenerateExtendedErrorMessage("Initial Temperature", result, 0.0, 60.0, model.backgroundTemperatureDefault)));
                        }
                    }
                    catch (Exception)
                    {
                        issues.Add(new ValidationIssue(model.InitialTemperature, ValidationSeverity.Error, Resources.WaterFlowModel1DTemperatureValidator_ValidateModelParameters_Values_should_be_in_double_format_));
                    }

                }
            }

            return new ValidationReport(Resources.WaterFlowModel1DTemperatureValidator_ValidateInitialTemperature_Initial_temperature_, issues);
        }
    }
}