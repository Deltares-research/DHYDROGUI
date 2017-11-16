using System.ComponentModel;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface ISewerConnection: IBranch
    {
        string ConnectionId { get; set; }
        double Length { get; set; }
        double LevelSource { get; set; }
        double LevelTarget { get; set; }
        SewerConnectionType SewerConnectionType { get; set; }
        Manhole SourceCompartment { get; set; }
        Manhole TargetCompartment { get; set; }
    }
    public enum SewerConnectionType
    {
        [Description("DRL")] Orifice,
        [Description("GSL")] ClosedConnection,
        [Description("ITR")] InfiltrationPipe,
        [Description("OPN")] Open,
        [Description("DRP")] Crest,
        [Description("PMP")] Pump
    }
}