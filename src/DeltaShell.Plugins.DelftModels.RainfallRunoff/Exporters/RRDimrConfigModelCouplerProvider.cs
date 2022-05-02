using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr.Export;

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
            if (source is IRainfallRunoffModel && target is IHydroModel || 
                target is IRainfallRunoffModel && source is IHydroModel)
            {
                return new RRFlowDimrConfigModelCoupler(source, target, sourceCoupler, targetCoupler)
                {
                    AddCouplerLoggerInfo = true
                };
            }

            var sourceRRModel = source as IRainfallRunoffModel;
            var targetRtcModel = target as IRainfallRunoffModel;
            if (sourceRRModel != null || targetRtcModel != null)
            {
                return new DimrConfigModelCoupler(source, target, sourceCoupler, targetCoupler)
                {
                    AddCouplerLoggerInfo = true
                };
            }

            return null;
        }

        #endregion
    }
}