using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum BoundaryConditionDataType
    {
        [Description("Time Series")]
        TimeSeries,
        [Description("Astronomical")]
        AstroComponents,
        [Description("Astronomical with correction")]
        AstroCorrection,
        [Description("Harmonic")]
        Harmonics,
        [Description("Harmonic with correction")]
        HarmonicCorrection,
        [Description("Q-h relation")]
        Qh,
        [Description("Constant")]
        Constant,
        [Description("Empty")]
        Empty,
        [Description("Parametrized (Constant)")]
        ParametrizedSpectrumConstant,
        [Description("Parametrized (Timeseries)")]
        ParametrizedSpectrumTimeseries,
        [Description("Filebased Spectrum")]
        SpectrumFromFile
    }
}