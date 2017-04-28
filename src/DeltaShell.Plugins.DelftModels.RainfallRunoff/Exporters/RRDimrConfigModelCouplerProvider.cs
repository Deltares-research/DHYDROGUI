using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    /// <summary>
    /// RR dimr export provider
    /// </summary>
    public class RRDimrConfigModelCouplerProvider : IDimrConfigModelCouplerProvider
    {
        #region Implementation of IDimrConfigModelCouplerProvider
        public IDimrConfigModelCoupler CreateCoupler(IModel source, IModel target, ICompositeActivity sourceCoupler,
            ICompositeActivity targetCoupler)
        {
            if (source is IRainfallRunoffModel || target is IRainfallRunoffModel)
            {
                return new RRFlowDimrConfigModelCoupler(source, target, sourceCoupler, targetCoupler);
            }
            return null;
        }

        #endregion
    }
}