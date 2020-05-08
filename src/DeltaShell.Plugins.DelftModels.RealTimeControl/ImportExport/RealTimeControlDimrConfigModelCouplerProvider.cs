using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public class RealTimeControlDimrConfigModelCouplerProvider : IDimrConfigModelCouplerProvider
    {
        public IDimrConfigModelCoupler CreateCoupler(IModel source, IModel target, ICompositeActivity sourceCoupler,
                                                     ICompositeActivity targetCoupler)
        {
            var sourceRtcModel = source as IRealTimeControlModel;
            var targetRtcModel = target as IRealTimeControlModel;
            if (sourceRtcModel != null || targetRtcModel != null)
            {
                var coupler = new DimrConfigModelCoupler(source, target, sourceCoupler, targetCoupler) {AddCouplerLoggerInfo = true};

                var rtcModel = sourceRtcModel as RealTimeControlModel;
                if (rtcModel != null)
                {
                    rtcModel.OutputFileName = coupler.Name;
                }

                return coupler;
            }

            return null;
        }
    }
}