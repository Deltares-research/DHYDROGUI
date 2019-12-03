using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.NWRW
{
    public class GwswNwRWData : NWRWData
    {
        public GwswNwRWData() : base()
        {
        }

        public double SurfaceStorage { get; set; }
        public double InfiltrationCapacityMax { get; set; }
        public double InfiltrationCapacityMin { get; set; }
        public double InfiltrationCapacityReduction { get; set; }
        public double InfiltrationCapacityRecovery { get; set; }
        public double RunoffDelay { get; set; }
        //public double RunoffLength { get; set; }
        //public double RunoffSlope { get; set; }
        //public double TerrainRoughness { get; set; }

    }
    [Entity(FireOnCollectionChange = false)]
    public class NWRWData : CatchmentModelData, ISewerFeature
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
        public int UnpavedFlatStretched { get; set; }
        public string MeteoStationId { get; set; }
        public string DryWeatherFlowId { get; set; }
        public int NumberOfPeople { get; set; }
        public int NumberOfSpecialAreas { get; set; }
        public IList<NWRWSpecialArea> SpecialAreas { get; set; }
        public double AreaAdjustmentFactor { get; set; }


        public void AddToHydroNetwork(IHydroNetwork network)
        {
            //
        }
    }

}
