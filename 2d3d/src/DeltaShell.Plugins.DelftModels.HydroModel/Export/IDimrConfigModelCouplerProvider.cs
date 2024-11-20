using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Export
{
    public interface IDimrConfigModelCouplerProvider
    {
        IDimrConfigModelCoupler CreateCoupler(IModel source, IModel target, ICompositeActivity sourceCoupler, ICompositeActivity targetCoupler);
    }
}