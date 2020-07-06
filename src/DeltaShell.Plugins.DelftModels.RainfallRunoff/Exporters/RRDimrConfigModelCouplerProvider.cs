using DelftTools.Hydro.Roughness;
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
            var targetfmModel = target as IModelWithNetwork;
            if (sourceRtcModel != null)
            {
                if (targetfmModel != null)

                {
                    return new RRFlowDimrConfigModelCoupler(source, target, sourceCoupler, targetCoupler)
                    {
                        AddCouplerLoggerInfo = true
                    };
                }
            }

            var targetRtcModel = target as IRainfallRunoffModel;
            if (sourceRtcModel != null || targetRtcModel != null)
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