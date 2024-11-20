using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Export
{
    public interface IDimrConfigModelCoupler
    {
        string Source { get; }
        string Target { get; }
        bool SourceIsMasterTimeStep { get; }
        IEnumerable<DimrCoupleInfo> CoupleInfos { get; }
        string Name { get; set; }
        bool AddCouplerLoggerInfo { get; set; }
        void UpdateModel(IModel sourceModel, IModel targetModel, ICompositeActivity sourceCoupler, ICompositeActivity targetCoupler);
    }
}