using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class FMStructuresValidator
    {
        /// <summary>
        /// Validate entities that can occur in an Area of a WaterFlowFMModel. The anomalies are returned as messages in the ValidationReport.
        /// </summary>
        /// <param name="model">The <see cref="WaterFlowFMModel"/> object of which the structures are to be validated. </param>
        /// <returns>ValidationReport that contains the validation messages which can be Info, Warning or Error</returns>
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var area = model.Area;
            
            var issues = ThinDamValidator.Validate(model.Area.ThinDams, model.GridExtent)
                .Concat(SourceAndSinkValidator.Validate(model.SourcesAndSinks, model.GridExtent, model.StartTime, model.StopTime))
                .Concat(FixedWeirValidator.Validate(model.Area.FixedWeirs, model.GridExtent, model.FixedWeirsProperties))
                .Concat(WeirValidator.Validate(model.Area.Weirs, model.GridExtent, model.StartTime, model.StopTime))
                .Concat(PumpValidator.ValidatePumps(model, area.Pumps));

            return new ValidationReport("Structures", issues);
        }
    }
}
