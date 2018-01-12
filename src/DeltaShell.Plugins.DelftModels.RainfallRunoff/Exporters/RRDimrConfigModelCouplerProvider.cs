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
            var sourceRtcModel = source as IRainfallRunoffModel;
            var targetRtcModel = target as IRainfallRunoffModel;
            if (sourceRtcModel != null || targetRtcModel != null)
            {
                return new RRFlowDimrConfigModelCoupler(source, target, sourceCoupler, targetCoupler)
                {
                    AddOptionalCouplerInfo = true
                };
            }
            return null;
        }

        #endregion
    }
}