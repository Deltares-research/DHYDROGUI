using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Editing;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public static class BoundaryDataConverter
    {
        private static readonly IDictionary<BoundaryConditionDataType, IList<BoundaryConditionDataType>>
            conversionDictionary = new Dictionary<BoundaryConditionDataType, IList<BoundaryConditionDataType>>
            {
                {
                    BoundaryConditionDataType.TimeSeries, new[]
                    {
                        BoundaryConditionDataType.TimeSeries
                    }
                },
                {
                    BoundaryConditionDataType.AstroComponents, new[]
                    {
                        BoundaryConditionDataType.AstroComponents,
                        BoundaryConditionDataType.AstroCorrection
                    }
                },
                {
                    BoundaryConditionDataType.AstroCorrection, new[]
                    {
                        BoundaryConditionDataType.AstroComponents,
                        BoundaryConditionDataType.AstroCorrection
                    }
                },
                {
                    BoundaryConditionDataType.Harmonics, new[]
                    {
                        BoundaryConditionDataType.Harmonics,
                        BoundaryConditionDataType.HarmonicCorrection
                    }
                },
                {
                    BoundaryConditionDataType.HarmonicCorrection, new[]
                    {
                        BoundaryConditionDataType.Harmonics,
                        BoundaryConditionDataType.HarmonicCorrection
                    }
                },
                {
                    BoundaryConditionDataType.Qh, new[]
                    {
                        BoundaryConditionDataType.Qh
                    }
                }
            };

        public static bool CanConvert(BoundaryConditionDataType sourceDataType,
                                      BoundaryConditionDataType targetDataType)
        {
            return conversionDictionary.ContainsKey(sourceDataType) &&
                   conversionDictionary[sourceDataType].Contains(targetDataType);
        }

        public static IFunction ConvertDataType(IFunction function, BoundaryConditionDataType dataType,
                                                BoundaryConditionDataType newDataType, int dimensions)
        {
            if (dataType == newDataType)
            {
                return function;
            }

            switch (dataType)
            {
                case BoundaryConditionDataType.AstroComponents when newDataType == BoundaryConditionDataType.AstroCorrection:
                    return ExpandAstroFunction(function, dimensions);
                case BoundaryConditionDataType.AstroCorrection when newDataType == BoundaryConditionDataType.AstroComponents:
                    return ReduceAstroFunction(function, dimensions);
                case BoundaryConditionDataType.Harmonics when newDataType == BoundaryConditionDataType.HarmonicCorrection:
                    return ExpandHarmonicFunction(function, dimensions);
                case BoundaryConditionDataType.HarmonicCorrection when newDataType == BoundaryConditionDataType.Harmonics:
                    return ReduceHarmonicFunction(function, dimensions);
                default:
                    return null;
            }
        }

        private static IFunction ReduceHarmonicFunction(IFunction function, int dimensions)
        {
            function.BeginEdit("Reducing harmonic correction function");
            for (int i = dimensions - 1; i >= 0; --i)
            {
                function.Components.RemoveAt((4 * i) + 3);
                function.Components.RemoveAt((4 * i) + 2);
            }

            function.EndEdit();
            return function;
        }

        private static IFunction ExpandHarmonicFunction(IFunction function, int dimensions)
        {
            function.BeginEdit("Expanding harmonic function");
            for (var i = 0; i < dimensions; ++i)
            {
                string amplitudeName = function.Components[4 * i].Name + " corr.";
                string phaseName = function.Components[(4 * i) + 1].Name + " corr.";

                var amplitudeCorrection = new Variable<double>(amplitudeName,
                                                               new Unit("-")) {DefaultValue = 1.0};
                var phaseCorrection = new Variable<double>(phaseName,
                                                           new Unit("degree", "deg")) {DefaultValue = 0.0};

                function.Components.Insert((4 * i) + 2, amplitudeCorrection);
                function.Components.Insert((4 * i) + 3, phaseCorrection);
            }

            function.EndEdit();
            return function;
        }

        private static IFunction ReduceAstroFunction(IFunction function, int dimensions)
        {
            function.BeginEdit("Reducing astro correction function");
            for (int i = dimensions - 1; i >= 0; --i)
            {
                function.Components.RemoveAt((4 * i) + 3);
                function.Components.RemoveAt((4 * i) + 2);
            }

            function.EndEdit();
            return function;
        }

        private static IFunction ExpandAstroFunction(IFunction function, int dimensions)
        {
            function.BeginEdit("Expanding astro function");
            for (var i = 0; i < dimensions; ++i)
            {
                string amplitudeName = function.Components[4 * i].Name + " corr.";
                string phaseName = function.Components[(4 * i) + 1].Name + " corr.";

                var amplitudeCorrection = new Variable<double>(amplitudeName,
                                                               new Unit("-")) {DefaultValue = 1.0};
                var phaseCorrection = new Variable<double>(phaseName,
                                                           new Unit("degree", "deg")) {DefaultValue = 0.0};

                function.Components.Insert((4 * i) + 2, amplitudeCorrection);
                function.Components.Insert((4 * i) + 3, phaseCorrection);
            }

            function.EndEdit();
            return function;
        }
    }
}