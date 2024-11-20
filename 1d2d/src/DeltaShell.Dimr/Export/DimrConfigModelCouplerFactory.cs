using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Dimr.Export
{
    public static class DimrConfigModelCouplerFactory
    {
        public const string COUPLER_NAME_COMBINER = "_to_";

        public static List<IDimrConfigModelCouplerProvider> CouplerProviders { get; } =
            new List<IDimrConfigModelCouplerProvider>();

        public static IDimrConfigModelCoupler GetCouplerForModels(IModel source, IModel target,
                                                                  ICompositeActivity sourceCoupler,
                                                                  ICompositeActivity targetCoupler)
        {
            IDimrConfigModelCoupler couplerConfig = CouplerProviders
                                                    .Select(p => p.CreateCoupler(source, target, sourceCoupler, targetCoupler))
                                                    .FirstOrDefault(c => c != null);
            return couplerConfig ?? new DimrConfigModelCoupler(source, target, sourceCoupler, targetCoupler);
        }
    }
}