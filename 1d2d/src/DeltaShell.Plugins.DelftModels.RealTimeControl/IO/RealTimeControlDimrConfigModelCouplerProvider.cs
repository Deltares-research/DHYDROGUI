using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr.Export;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    public class RealTimeControlDimrConfigModelCouplerProvider : IDimrConfigModelCouplerProvider
    {
        public IDimrConfigModelCoupler CreateCoupler(IModel source, IModel target, ICompositeActivity sourceCoupler,
                                                     ICompositeActivity targetCoupler)
        {
            if (source is RealTimeControlModel sourceRtcModel)
            {
                var coupler = new DimrConfigModelCoupler(source, target, sourceCoupler, targetCoupler) {AddCouplerLoggerInfo = true};
                sourceRtcModel.CommunicationRtcToFmFileName = coupler.Name;
                return coupler;
            }
            
            if (target is RealTimeControlModel targetRtcModel)
            {
                var coupler = new DimrConfigModelCoupler(source, target, sourceCoupler, targetCoupler) { AddCouplerLoggerInfo = true };
                targetRtcModel.CommunicationFmToRtcFileName = coupler.Name;
                return coupler;
            }

            return null;
        }
    }
}