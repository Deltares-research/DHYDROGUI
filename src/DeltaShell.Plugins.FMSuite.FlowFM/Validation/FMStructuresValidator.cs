using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class FMStructuresValidator
    {
        /// <summary>
        /// Validate entities that can occur in an Area of a WaterFlowFMModel. The anomalies are returned as messages in the
        /// ValidationReport.
        /// </summary>
        /// <param name="model"> The <see cref="WaterFlowFMModel" /> object of which the structures are to be validated. </param>
        /// <returns> ValidationReport that contains the validation messages which can be Info, Warning or Error </returns>
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            HydroArea area = model.Area;

            IEnumerable<ValidationIssue> issues = area.ThinDams.Validate(model.GridExtent)
                                                      .Concat(model.SourcesAndSinks.Validate(
                                                                  model.GridExtent, model.StartTime,
                                                                  model.StopTime))
                                                      .Concat(area.FixedWeirs.Validate(model.GridExtent,
                                                                                       model.FixedWeirsProperties))
                                                      .Concat(area.Weirs.Validate(model.GridExtent, model.StartTime,
                                                                                  model.StopTime))
                                                      .Concat(area.Pumps.Validate(model.GridExtent, model.StartTime,
                                                                                  model.StopTime));

            return new ValidationReport("Structures", issues);
        }
    }
}