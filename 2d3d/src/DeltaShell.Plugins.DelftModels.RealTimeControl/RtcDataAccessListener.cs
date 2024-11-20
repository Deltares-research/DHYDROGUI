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
    public class RtcDataAccessListener : IDataAccessListener
    {
        private const string previousDischargeAtLateralDataItemTag = "Discharge (l)";
        private const string controlGroupsPropertyName = "ControlGroups";

        private bool firstRtcModel = true;
        private IProjectRepository projectRepository;

        public RtcDataAccessListener(IProjectRepository repository)
        {
            projectRepository = repository;
        }

        public void SetProjectRepository(IProjectRepository repository)
        {
            projectRepository = repository;
        }

        public void OnPreLoad(object entity, object[] loadedState, string[] propertyNames)
        {
            if (entity is Project)
            {
                firstRtcModel = true;
            }
            else if (firstRtcModel && entity is RealTimeControlModel)
            {
                projectRepository.PreLoad<ControlGroup>(cg => cg.Inputs);
                projectRepository.PreLoad<ControlGroup>(cg => cg.Outputs);
                projectRepository.PreLoad<ControlGroup>(cg => cg.Conditions);
                projectRepository.PreLoad<ControlGroup>(cg => cg.Rules);
                projectRepository.PreLoad<ControlGroup>(cg => cg.MathematicalExpressions);

                projectRepository.PreLoad<RuleBase>(r => r.Inputs);
                projectRepository.PreLoad<RuleBase>(r => r.Outputs);

                projectRepository.PreLoad<Input>(i => i.Feature);
                projectRepository.PreLoad<Output>(i => i.Feature);

                firstRtcModel = false;
            }

            RemovingInterpolationNoneForTimeRulesIfSetInDatabase(entity, loadedState);
        }

        public void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            var rtcModel = entity as IRealTimeControlModel;
            if (rtcModel == null)
            {
                return;
            }

            List<string> propertyNamesList = propertyNames.ToList();

            int controlGroupIndex = propertyNamesList.IndexOf(controlGroupsPropertyName);
            if (controlGroupIndex < 0)
            {
                return;
            }

            // state and propertyNames will always be the same length
            var stateControlGroups = state[controlGroupIndex] as IEnumerable<ControlGroup>;
            if (stateControlGroups == null)
            {
                return;
            }

            foreach (ControlGroup controlGroup in stateControlGroups)
            {
                // SOBEK3-115: Existing projects can have ControlGroups with locations at the deprecated output parameter 'Discharge (l)'
                controlGroup.Inputs.Where(i => i.ParameterName == previousDischargeAtLateralDataItemTag).ForEach(i => i.Reset());
                controlGroup.Outputs.Where(o => o.ParameterName == previousDischargeAtLateralDataItemTag).ForEach(o => o.Reset());

                // SOBEK3-562: Existing projects can have ControlGroups with locations at inputs/outputs but no underlying dataitem links
                rtcModel.ResetOrphanedControlGroupInputsAndOutputs(controlGroup);
            }
        }

        public bool OnPreUpdate(object entity, object[] state, string[] propertyNames)
        {
            return false;
        }

        public bool OnPreInsert(object entity, object[] state, string[] propertyNames)
        {
            return false;
        }

        public void OnPostUpdate(object entity, object[] state, string[] propertyNames)
        {
        }

        public void OnPostInsert(object entity, object[] state, string[] propertyNames)
        {
        }

        public bool OnPreDelete(object entity, object[] deletedState, string[] propertyNames)
        {
            return false;
        }

        public void OnPostDelete(object entity, object[] deletedState, string[] propertyNames)
        {
        }

        public IDataAccessListener Clone()
        {
            return new RtcDataAccessListener(projectRepository);
        }

        private static void RemovingInterpolationNoneForTimeRulesIfSetInDatabase(object entity, object[] loadedState)
        {
            if (entity is TimeRule || entity is RelativeTimeRule)
            {
                Function timeSeries = loadedState.OfType<Function>().FirstOrDefault();

                if (timeSeries != null && timeSeries.Arguments.First().InterpolationType == InterpolationType.None)
                {
                    timeSeries.Arguments.First().InterpolationType = InterpolationType.Linear;
                }
            }
        }
    }
}