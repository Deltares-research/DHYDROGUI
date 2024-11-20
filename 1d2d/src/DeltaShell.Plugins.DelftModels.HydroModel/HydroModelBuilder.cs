using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.Scripting;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    /// <summary>
    /// Creates a new pre-configured HydroModel
    /// </summary>
    public class HydroModelBuilder : PythonClass
    {
        public bool CanBuildModel(ModelGroup modelGroup)
        {
            return Invoke<bool>("can_create_modelgroup", CompileAndExecute("ModelGroups." + modelGroup));
        }

        public HydroModel BuildModel(ModelGroup modelGroup)
        {
            return Invoke<HydroModel>("build_model", CompileAndExecute("ModelGroups." + modelGroup));
        }

        public void AutoAddRequiredLinks(IHydroModel hydroModel, IActivity childActivity, bool relinking)
        {
            Invoke("auto_add_required_model_links", hydroModel, childActivity, true, relinking);
        }

        public void RefreshDefaultModelWorkflows(IHydroModel hydroModel)
        {
            Invoke("refresh_default_model_workflows", hydroModel);
        }

        public void SetDefaultActivityName(IActivity activity)
        {
            Invoke("on_activity_added", activity);
        }

        public void OnActivityRemoving(IHydroModel hydroModel, IActivity activity)
        {
            Invoke("on_activity_removing", hydroModel, activity);
        }

        public void OnActivityRemoved(IHydroModel hydroModel, IActivity activity)
        {
            Invoke("on_activity_removed", hydroModel, activity);
        }

        public void RebuildAllModelLinks(IHydroModel hydroModel)
        {
            Invoke("rebuild_all_model_links", hydroModel);
        }
    }
}