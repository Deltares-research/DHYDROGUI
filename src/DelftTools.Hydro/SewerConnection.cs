using DelftTools.Hydro.CrossSections;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    public class SewerConnection : Branch, ISewerConnection
    {
        public string ConnectionId { get; set; }
        public double LevelSource { get; set; }
        public double LevelTarget { get; set; }
        public SewerConnectionType SewerConnectionType { get; set; }
        public Manhole SourceCompartment { get; set; }
        public Manhole TargetCompartment { get; set; }

        public override bool IsLengthCustom
        {
            get { return true; }
        }
    }
}