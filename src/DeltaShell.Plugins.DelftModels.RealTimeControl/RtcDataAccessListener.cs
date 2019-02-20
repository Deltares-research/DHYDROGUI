using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public class RtcDataAccessListener : DataAccessListenerBase
    {
        private const string PreviousDischargeAtLateralDataItemTag = "Discharge (l)";
        private const string ControlGroupsPropertyName = "ControlGroups";

        private bool firstRtcModel = true;

        public override void OnPreLoad(object entity, object[] loadedState, string[] propertyNames)
        {
            if (entity is Project)
            {
                firstRtcModel = true;
            }
            else if (firstRtcModel && entity is RealTimeControlModel)
            {
                ProjectRepository.PreLoad<ControlGroup>(cg => cg.Inputs);
                ProjectRepository.PreLoad<ControlGroup>(cg => cg.Outputs);
                ProjectRepository.PreLoad<ControlGroup>(cg => cg.Conditions);
                ProjectRepository.PreLoad<ControlGroup>(cg => cg.Rules);

                ProjectRepository.PreLoad<RuleBase>(r => r.Inputs);
                ProjectRepository.PreLoad<RuleBase>(r => r.Outputs);

                ProjectRepository.PreLoad<Input>(i => i.Feature);
                ProjectRepository.PreLoad<Output>(i => i.Feature);

                firstRtcModel = false;
            }

            RemovingInterpolationNoneForTimeRulesIfSetInDatabase(entity, loadedState);
        }


        private static void RemovingInterpolationNoneForTimeRulesIfSetInDatabase(object entity, object[] loadedState)
        {
            if (entity is TimeRule)
            {
                var timeSeries = loadedState.OfType<TimeSeries>().FirstOrDefault();

                if (timeSeries != null)
                {
                    if (timeSeries.Time.InterpolationType == InterpolationType.None)
                    {
                        timeSeries.Time.InterpolationType = InterpolationType.Linear;
                    }
                }
            }
        }

        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            var rtcModel = entity as IRealTimeControlModel;
            if (rtcModel == null) return;

            var propertyNamesList = propertyNames.ToList();

            var controlGroupIndex = propertyNamesList.IndexOf(ControlGroupsPropertyName);
            if (controlGroupIndex < 0) return;

            // state and propertyNames will always be the same length
            var stateControlGroups = state[controlGroupIndex] as IEnumerable<ControlGroup>;
            if (stateControlGroups == null) return;

            foreach (var controlGroup in stateControlGroups)
            {
                // SOBEK3-115: Existing projects can have ControlGroups with locations at the deprecated output parameter 'Discharge (l)'
                controlGroup.Inputs.Where(i => i.ParameterName == PreviousDischargeAtLateralDataItemTag).ForEach(i => i.Reset());
                controlGroup.Outputs.Where(o => o.ParameterName == PreviousDischargeAtLateralDataItemTag).ForEach(o => o.Reset());
                    
                // SOBEK3-562: Existing projects can have ControlGroups with locations at inputs/outputs but no underlying dataitem links
                rtcModel.ResetOrphanedControlGroupInputsAndOutputs(controlGroup);
            }
        }

        public override object Clone()
        {
            return new RtcDataAccessListener {ProjectRepository = ProjectRepository};
        }
    }
}