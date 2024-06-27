using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    /// <summary>
    /// This class generates the possible quantities for 2D <see cref="IFeature"/> objects, depending on
    /// the type of <see cref="IFeature"/>.
    /// </summary>
    public static class QuantityGenerator
    {
        /// <summary>
        /// Gets all possible quantities for the provided feature based on its type.
        /// </summary>
        /// <param name="feature">The feature to get the quantities for.</param>
        /// <param name="useSalinity">Whether or not salinity is used.</param>
        /// <param name="useTemperature">Whether or not temperature is used.</param>
        /// <param name="tracerDefinitions">Collection of tracer definitions.</param>
        /// <param name="sourceAndSinks">Collection of sources and sinks.</param>
        /// <returns>The collection of possible quantities for the given feature.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="feature"/>, <paramref name="tracerDefinitions"/> or
        /// <paramref name="sourceAndSinks"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<string> GetQuantitiesForFeature(
            IFeature feature,
            bool useSalinity,
            bool useTemperature,
            IEnumerable<string> tracerDefinitions,
            IEnumerable<SourceAndSink> sourceAndSinks)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(tracerDefinitions, nameof(tracerDefinitions));
            Ensure.NotNull(sourceAndSinks, nameof(sourceAndSinks));

            switch (feature)
            {
                case Pump2D _:
                    foreach (string quantity in GetPumpQuantities())
                    {
                        yield return quantity;
                    }

                    yield break;
                case Weir2D weir:
                    foreach (string quantity in GetWeirQuantities(weir.WeirFormula))
                    {
                        yield return quantity;
                    }

                    yield break;
                case Gate2D _:
                    foreach (string quantity in GetGateQuantities())
                    {
                        yield return quantity;
                    }

                    yield break;
                case LeveeBreach _:
                    foreach (string quantity in GetDamBreakQuantities())
                    {
                        yield return quantity;
                    }

                    yield break;
                case ObservationCrossSection2D _:
                    foreach (string quantity in GetObservationCrossSectionQuantities())
                    {
                        yield return quantity;
                    }

                    yield break;
                case ObservationPoint2D _:
                    foreach (string quantity in GetObservationPointQuantities(useSalinity, useTemperature, tracerDefinitions))
                    {
                        yield return quantity;
                    }

                    yield break;
                case Feature2D feature2D:
                    foreach (string sourceAndSinkQuantity in GetSourceAndSinkQuantities(feature2D, sourceAndSinks))
                    {
                        yield return sourceAndSinkQuantity;
                    }

                    yield break;
            }
        }

        private static IEnumerable<string> GetPumpQuantities()
        {
            yield return "Capacity";
        }

        private static IEnumerable<string> GetWeirQuantities(IWeirFormula formula)
        {
            switch (formula)
            {
                case SimpleWeirFormula _:
                    yield return "CrestLevel";
                    yield break;
                case GeneralStructureWeirFormula _:
                    yield return "CrestLevel";
                    yield return "GateHeight";
                    yield return "GateLowerEdgeLevel";
                    yield return "GateOpeningWidth";
                    yield break;
            }
        }

        private static IEnumerable<string> GetGateQuantities()
        {
            yield return "CrestLevel";
            yield return "GateHeight";
            yield return "GateLowerEdgeLevel";
            yield return "GateOpeningWidth";
        }

        private static IEnumerable<string> GetDamBreakQuantities()
        {
            yield return "DambreakS1up";
            yield return "DambreakS1dn";
            yield return "DambreakBreach_depth";
            yield return "DambreakBreach_depth";
            yield return "DambreakBreak_width";
            yield return "DambreakInstantaneous_discharge";
            yield return "DambreakCumulative_discharge";
        }

        private static IEnumerable<string> GetObservationCrossSectionQuantities()
        {
            yield return "discharge";
            yield return "velocity";
            yield return "water_level";
            yield return "water_depth";
        }

        private static IEnumerable<string> GetObservationPointQuantities(bool useSalinity, bool useTemperature, IEnumerable<string> tracerDefinitions)
        {
            yield return "water_level";
            if (useSalinity)
            {
                yield return "salinity";
            }

            if (useTemperature)
            {
                yield return "temperature";
            }

            yield return "water_depth";
            foreach (string tracerDefinition in tracerDefinitions)
            {
                yield return tracerDefinition;
            }
        }

        private static IEnumerable<string> GetSourceAndSinkQuantities(Feature2D feature, IEnumerable<SourceAndSink> sourceAndSinks)
        {
            if (!sourceAndSinks.Any(ss => ss.Feature.Equals(feature)))
            {
                yield break;
            }

            yield return "discharge";
            yield return "change_in_salinity";
            yield return "change_in_temperature";
        }
    }
}