using DelftTools.Functions;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    [Entity(FireOnCollectionChange = false)]
    public class Gate : BranchStructure, IGate
    {
        [FeatureAttribute]
        public virtual double SillLevel { get; set; }
        public virtual bool UseSillLevelTimeSeries { get; set; }
        public virtual TimeSeries SillLevelTimeSeries { get; set; }

        [FeatureAttribute]
        public virtual double LowerEdgeLevel { get; set; }
        public virtual bool UseLowerEdgeLevelTimeSeries { get; set; }
        public virtual TimeSeries LowerEdgeLevelTimeSeries { get; set; }

        [FeatureAttribute]
        public virtual double OpeningWidth { get; set; }
        public virtual bool UseOpeningWidthTimeSeries { get; set; }
        public virtual TimeSeries OpeningWidthTimeSeries { get; set; }

        [FeatureAttribute]
        public virtual double DoorHeight { get; set; }

        [FeatureAttribute]
        public virtual GateOpeningDirection HorizontalOpeningDirection { get; set; }

        [FeatureAttribute]
        public virtual double SillWidth { get; set; }

        public Gate() : this("Gate")
        {
        }

        public Gate(string name)
        {
            Name = name;
            SillLevel = 1;
            OffsetY = 0;

            SillLevelTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries("Sill level", "Sill level", "m");
            LowerEdgeLevelTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries("Lower edge level", "Lower edge level", "m");
            OpeningWidthTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries("Opening width", "Opening width", "m");
        }

        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            var copyFrom = (Gate)source;

            SillLevel = copyFrom.SillLevel;
            LowerEdgeLevel = copyFrom.LowerEdgeLevel;
            OpeningWidth = copyFrom.OpeningWidth;
            DoorHeight = copyFrom.DoorHeight;
            HorizontalOpeningDirection = copyFrom.HorizontalOpeningDirection;
            SillWidth = copyFrom.SillWidth;

            // Set time varying properties:
            UseSillLevelTimeSeries = copyFrom.UseSillLevelTimeSeries;
            if (copyFrom.SillLevelTimeSeries != null)
            {
                SillLevelTimeSeries = (TimeSeries) copyFrom.SillLevelTimeSeries.Clone(true);
            }
            
            UseLowerEdgeLevelTimeSeries = copyFrom.UseLowerEdgeLevelTimeSeries;
            if (copyFrom.LowerEdgeLevelTimeSeries != null)
            {
                LowerEdgeLevelTimeSeries = (TimeSeries) copyFrom.LowerEdgeLevelTimeSeries.Clone(true);
            }

            UseOpeningWidthTimeSeries = copyFrom.UseOpeningWidthTimeSeries;
            if(copyFrom.OpeningWidthTimeSeries != null)
            {
                OpeningWidthTimeSeries = (TimeSeries)copyFrom.OpeningWidthTimeSeries.Clone(true);
            }
        }

        public override StructureType GetStructureType()
        {
            return StructureType.Gate;
        }
    }
}