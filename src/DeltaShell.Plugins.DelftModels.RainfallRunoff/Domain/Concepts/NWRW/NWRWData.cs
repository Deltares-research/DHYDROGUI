using DelftTools.Hydro;
using System.Collections.Generic;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.NWRW
{
    [Entity(FireOnCollectionChange = false)]
    public class NWRWData : CatchmentModelData
    {
        //nhib
        protected NWRWData()
            : base(null) { }

        public NWRWData(Catchment catchment) : base(catchment)
        {
        }

        public string NWRWDataId { get; set; }
        public double SurfaceLevel { get; set; }
        public int ClosedPavedWithSlope { get; set; }
        public int ClosedPavedFlat { get; set; }
        public int ClosedPavedFlatStretched { get; set; }
        public int OpenPavedWithSlope { get; set; }
        public int OpenPavedFlat { get; set; }
        public int OpenPavedFlatStretched { get; set; }
        public int RoofWithSlope { get; set; }
        public int RoofFlat { get; set; }
        public int RoofFlatStretched { get; set; }
        public int UnpavedWithSlope { get; set; }
        public int UnpavedFlat { get; set; }
        public virtual int UnpavedFlatStretched { get; set; }
        public string MeteoStationId { get; set; }
        public string DryWeatherFlowId { get; set; }
        public int NumberOfPeople { get; set; }
        public int NumberOfSpecialAreas { get; set; }
        public IList<NWRWSpecialArea> SpecialAreas { get; set; }
        public double AreaAdjustmentFactor { get; set; }


    }

}
