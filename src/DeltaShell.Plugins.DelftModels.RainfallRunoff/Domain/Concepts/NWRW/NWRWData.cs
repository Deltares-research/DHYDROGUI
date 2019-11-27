using DelftTools.Hydro;
using System.Collections.Generic;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.NWRW
{
    [Entity(FireOnCollectionChange = false)]
    public class NWRWData : CatchmentModelData
    {
        public NWRWData(Catchment catchment) : base(catchment)
        {
            //throw new NotImplementedException();
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
        public int UnpavedFlatStretched { get; set; }
        public string MeteoStationId { get; set; }
        public string DryWeatherFlowId { get; set; }
        public int NumberOfPeople { get; set; }
        public int NumberOfSpecialAreas { get; set; }
        public IList<SpecialArea> SpecialAreas { get; set; }
        public int AreaAdjustmentFactor { get; set; }


    }

}
