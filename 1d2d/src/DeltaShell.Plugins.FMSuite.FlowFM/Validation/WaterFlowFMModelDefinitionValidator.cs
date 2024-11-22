﻿using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMModelDefinitionValidator
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var modelDefinition = model.ModelDefinition;
            var groupReports = new List<ValidationReport>();
            var timerCategory = modelDefinition.GetModelProperty(GuiProperties.StartTime).PropertyDefinition.Category;
            var solverProperty = modelDefinition.GetModelProperty(KnownProperties.SolverType);
            var bedLevelTypeProperty = modelDefinition.GetModelProperty(KnownProperties.BedlevType);
            var conveyanceTypeProperty = modelDefinition.GetModelProperty(KnownProperties.Conveyance2d);
            foreach (var propertyGroup in modelDefinition.Properties.GroupBy(p => p.PropertyDefinition.Category))
            {
                var issues = new List<ValidationIssue>();
                foreach (var waterFlowFmProperty in propertyGroup)
                {
                    if (waterFlowFmProperty.IsVisible(modelDefinition.Properties) &&
                        waterFlowFmProperty.IsEnabled(modelDefinition.Properties) && !waterFlowFmProperty.Validate())
                    {
                        issues.Add(new ValidationIssue(propertyGroup.Key, ValidationSeverity.Error,
                            "Parameter " + waterFlowFmProperty.PropertyDefinition.Caption +
                            " outside validity range" +
                            RangeToString(waterFlowFmProperty.MinValue,
                                waterFlowFmProperty.MaxValue) + ".", model));
                    }
                    if (solverProperty != null && waterFlowFmProperty == solverProperty)
                    {
                        var solver = int.Parse(waterFlowFmProperty.GetValueAsString());
                        if (solver > 4)
                        {
                            issues.Add(new ValidationIssue(propertyGroup.Key, ValidationSeverity.Error,
                                "Solver type selected for parallel run; this is currently not possible in GUI."));
                        }
                    }

                    // Whenever morphology is active, give an error in the validation report in case the bed level locations is not set to 'cells' (BedlevType.val0)
                    //if (waterFlowFmProperty.PropertyDefinition.MduPropertyName.Equals(KnownPropertyMduNames.BedlevType))
                    if (bedLevelTypeProperty != null && waterFlowFmProperty == bedLevelTypeProperty)
                    {
                        int bedLevelTypeNumber;
                        var useMorSed = modelDefinition.UseMorphologySediment;
                        if (useMorSed
                            && int.TryParse(waterFlowFmProperty.GetValueAsString(), out bedLevelTypeNumber) &&
                            !bedLevelTypeNumber.Equals((int) BedLevelLocation.Faces))
                            issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                "Bed level locations should be set to 'cells' when morphology is active."));
                    }

                    // Whenever morphology is active, give an error in the validation report 
                    // when if conveyance 2d type is not set to:
                    // * R=HU 
                    // * R=H  
                    // * R=A/P
                    if (conveyanceTypeProperty != null && waterFlowFmProperty == conveyanceTypeProperty)
                    {
                        var useMorSed = modelDefinition.UseMorphologySediment;
                        if (useMorSed && Enum.TryParse(waterFlowFmProperty.GetValueAsString(),
                            out Conveyance2DType currentConveyanceType) && currentConveyanceType != Conveyance2DType.RisHU && currentConveyanceType != Conveyance2DType.RisH && currentConveyanceType != Conveyance2DType.RisAperP)
                        {
                            issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                Resources.WaterFlowFMModelDefinitionValidator_Validate_));
                        }
                    }
                }
                if (propertyGroup.Key.Equals(timerCategory))
                {
                    var validator = new WaterFlowFMModelTimersValidator();
                    issues.AddRange(validator.ValidateModelTimers(model, model.OutputTimeStep, model));
                }
                groupReports.Add(new ValidationReport(propertyGroup.Key, issues));
            }
            return new ValidationReport("WaterFlow FM model definition", groupReports);
        }

        private static string RangeToString(object min, object max)
        {
            return " [" + (min == null ? "-inf" : Convert.ToDouble(min).ToString()) + "," + (max == null ? "+inf" : Convert.ToDouble(max).ToString()) + "]";
        }
    }
}