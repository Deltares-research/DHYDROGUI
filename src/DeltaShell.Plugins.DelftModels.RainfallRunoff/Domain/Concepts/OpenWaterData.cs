using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    [Entity(FireOnCollectionChange=false)]
    public class OpenWaterData : CatchmentModelData
    {        
        //nhib
        protected OpenWaterData()
            : base(null)
        {
        }

        public OpenWaterData(Catchment catchment) : base(catchment)
        {
            catchment.ModelData = this;
        }
    }
}