using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;


namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMArea2DValidator
    {
        /// <summary>
        /// Validate all entities that can occur in an Area2D of a WaterFlow Model. The anomalies are returned as messages in the ValidationReport.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>ValidationReport that contains the validation messages which can be Info, Warning or Error</returns>
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var area = model.Area;
            
            var issues = ThinDamValidator.Validate(model)
                .Concat(SourceAndSinkValidator.Validate(model, model.SourcesAndSinks))
                .Concat(FixedWeirValidator.Validate(model, area.FixedWeirs))
                .Concat(WeirValidator.Validate(model, area.Weirs))
                .Concat(PumpValidator.ValidatePumps(model, area.Pumps));

            return new ValidationReport("Structures", issues);
        }
    }
}
