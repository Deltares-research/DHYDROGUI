using System.ComponentModel;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface ISewerConnection : IBranch, IHydroNetworkFeature
    {
        string ConnectionId { get; set; }
        double Length { get; set; }
        double LevelSource { get; set; }
        double LevelTarget { get; set; }
        SewerConnectionType SewerConnectionType { get; set; }
        Compartment SourceCompartment { get; set; }
        Compartment TargetCompartment { get; set; }
    }
    public enum SewerConnectionType
    {
        [Description("DRL")] Orifice,
        [Description("GSL")] ClosedConnection /*Should be created as a pipe*/,
        [Description("ITR")] InfiltrationPipe /*Should be created as a pipe*/,
        [Description("OPL")] Open /*Should be created as a pipe*/,
        [Description("OVS")] Crest,
        [Description("PMP")] Pump
    }

    public enum SewerConnectionWaterType
    {
        [Description("NVT")] None,
        [Description("HWA")] FlowingRainWater,
        [Description("DWA")] DryWeatherRainage,
        [Description("GMD")] MixedWasteWater,
    }
}