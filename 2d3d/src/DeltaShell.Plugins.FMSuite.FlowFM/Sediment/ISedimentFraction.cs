using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Sediment
{
    public interface ISedimentFraction
    {
        string Name { get; set; }
        ISedimentType CurrentSedimentType { get; set; }
        ISedimentFormulaType CurrentFormulaType { get; set; }
        List<ISedimentType> AvailableSedimentTypes { get; set; }
        List<ISedimentFormulaType> SupportedFormulaTypes { get; }
        List<string> GetAllSpatiallyVaryingPropertyNames();
        List<string> GetAllActiveSpatiallyVaryingPropertyNames();
    }
}