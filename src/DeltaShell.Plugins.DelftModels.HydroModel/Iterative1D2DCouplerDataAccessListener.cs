using DelftTools.Shell.Core.Dao;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public class Iterative1D2DCouplerDataAccessListener : DataAccessListenerBase
    {
        public override object Clone()
        {
            return new Iterative1D2DCouplerDataAccessListener { ProjectRepository = ProjectRepository};
        }

        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            base.OnPostLoad(entity, state, propertyNames);
            
            var integratedModel = entity as HydroModel;
            if (integratedModel == null) return;
            var wf = integratedModel.CurrentWorkflow as Iterative1D2DCoupler;
            if (wf == null) return;
            var dimrModel = wf.Flow2DModel as IDimrModel;
            if (dimrModel == null) return;
            dimrModel.SetVar(new[] {true}, Iterative1D2DCoupler.IsPartOf1D2DModelPropertyName);
            dimrModel.SetVar(new[] {true}, Iterative1D2DCoupler.DisableFlowNodeRenumberingPropertyName);
        }
    }
}