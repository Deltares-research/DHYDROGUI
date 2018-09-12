using DelftTools.Hydro.Structures;
using log4net;

namespace DelftTools.Hydro.SewerFeatures
{
    public class GwswConnectionOrifice : Orifice
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GwswConnectionOrifice));
        public GwswConnectionOrifice(string name) : base(name)
        {
        }

        public string SourceCompartmentName { get; set; }
        public string TargetCompartmentName { get; set; }
        public double LevelSource { get; set; }
        public double LevelTarget { get; set; }
        public SewerConnectionWaterType WaterType { get; set; }

        protected override void SetSewerConnectionProperties(ISewerConnection sewerConnection)
        {
            sewerConnection.LevelSource = LevelSource;
            sewerConnection.LevelTarget = LevelTarget;
            sewerConnection.Length = Length;
            sewerConnection.WaterType = WaterType;
            sewerConnection.SourceCompartmentName = SourceCompartmentName;
            sewerConnection.TargetCompartmentName = TargetCompartmentName;
        }

        protected override void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
        }
    }
}
