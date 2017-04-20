using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Editing;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public static class BoundaryDataConverter
    {
        private static readonly IDictionary<BoundaryConditionDataType, IList<BoundaryConditionDataType>> ConversionDictionary =
            new Dictionary<BoundaryConditionDataType, IList<BoundaryConditionDataType>>
            {
                {BoundaryConditionDataType.TimeSeries, new[] {BoundaryConditionDataType.TimeSeries}},
                {
                    BoundaryConditionDataType.AstroComponents,
                    new[] {BoundaryConditionDataType.AstroComponents, BoundaryConditionDataType.AstroCorrection}
                },
                {
                    BoundaryConditionDataType.AstroCorrection,
                    new[] {BoundaryConditionDataType.AstroComponents, BoundaryConditionDataType.AstroCorrection}
                },
                {
                    BoundaryConditionDataType.Harmonics,
                    new[] {BoundaryConditionDataType.Harmonics, BoundaryConditionDataType.HarmonicCorrection}
                },
                {
                    BoundaryConditionDataType.HarmonicCorrection,
                    new[] {BoundaryConditionDataType.Harmonics, BoundaryConditionDataType.HarmonicCorrection}
                },
                {BoundaryConditionDataType.Qh, new[] {BoundaryConditionDataType.Qh}}
            };

        public static bool CanConvert(BoundaryConditionDataType sourceDataType, BoundaryConditionDataType targetDataType)
        {
            return ConversionDictionary.ContainsKey(sourceDataType) &&
                   ConversionDictionary[sourceDataType].Contains(targetDataType);
        }

        public static IFunction ConvertDataType(IFunction function, BoundaryConditionDataType dataType, BoundaryConditionDataType newDataType, int dimensions)
        {
            if (dataType == newDataType)
            {
                return function;
            }
            if (dataType == BoundaryConditionDataType.AstroComponents)
            {
                if (newDataType == BoundaryConditionDataType.AstroCorrection)
                {
                    function.BeginEdit(new DefaultEditAction("Expanding astro function"));
                    for (int i = 0; i < dimensions; ++i)
                    {
                        var amplitudeName = function.Components[4*i].Name + " corr.";
                        var phaseName = function.Components[4*i + 1].Name + " corr.";

                        var amplitudeCorrection = new Variable<double>(amplitudeName,
                            new Unit("-")) {DefaultValue = 1.0};
                        var phaseCorrection = new Variable<double>(phaseName,
                            new Unit("degree", "deg")) {DefaultValue = 0.0};

                        function.Components.Insert(4*i + 2, amplitudeCorrection);
                        function.Components.Insert(4*i + 3, phaseCorrection);
                    }
                    function.EndEdit();
                    return function;
                }
            }
            if (dataType == BoundaryConditionDataType.AstroCorrection)
            {
                function.BeginEdit("Reducing astro correction function");
                if (newDataType == BoundaryConditionDataType.AstroComponents)
                {
                    for (int i = dimensions - 1; i >=0 ; --i)
                    {
                        function.Components.RemoveAt(4*i + 3);
                        function.Components.RemoveAt(4*i + 2);
                    }
                    function.EndEdit();
                    return function;
                }
            }
            if (dataType == BoundaryConditionDataType.Harmonics)
            {
                if (newDataType == BoundaryConditionDataType.HarmonicCorrection)
                {
                    function.BeginEdit(new DefaultEditAction("Expanding harmonic function"));
                    for (int i = 0; i < dimensions; ++i)
                    {
                        var amplitudeName = function.Components[4*i].Name + " corr.";
                        var phaseName = function.Components[4*i + 1].Name + " corr.";

                        var amplitudeCorrection = new Variable<double>(amplitudeName,
                            new Unit("-")) {DefaultValue = 1.0};
                        var phaseCorrection = new Variable<double>(phaseName,
                            new Unit("degree", "deg")) {DefaultValue = 0.0};

                        function.Components.Insert(4*i + 2, amplitudeCorrection);
                        function.Components.Insert(4*i + 3, phaseCorrection);
                    }
                    function.EndEdit();
                    return function;
                }
            }
            if (dataType == BoundaryConditionDataType.HarmonicCorrection)
            {
                if (newDataType == BoundaryConditionDataType.Harmonics)
                {
                    function.BeginEdit("Reducing harmonic correction function");
                    for (int i = dimensions - 1; i >= 0; --i)
                    {
                        function.Components.RemoveAt(4 * i + 3);
                        function.Components.RemoveAt(4 * i + 2);
                    }
                    function.EndEdit();
                    return function;
                }
            }
            return null;
        }
    }
}
