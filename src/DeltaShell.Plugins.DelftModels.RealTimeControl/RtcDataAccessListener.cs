using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public class RtcDataAccessListener : DataAccessListenerBase
    {
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
        }

        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            var rtcModel = entity as IRealTimeControlModel;
            if (rtcModel != null)
            {
                // SOBEK3-562: Existing projects can have ControlGroups with locations at inputs/outputs but no underlying dataitem links
                rtcModel.ControlGroups.ForEach(rtcModel.ResetOrphanedControlGroupInputsAndOutputs);
            }
        }

        public override object Clone()
        {
            return new RtcDataAccessListener {ProjectRepository = ProjectRepository};
        }
    }
}