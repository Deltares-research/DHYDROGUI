using System;
using DelftTools.Units;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.WaveEnergyFunctions
{
    [TestFixture]
    [TestFixture(typeof(DegreesDefinedSpreading), typeof(PowerDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading), typeof(DegreesDefinedSpreading))]
    public class WaveEnergyFunctionConversionTest<TOld, TNew> where TOld : class, IBoundaryConditionSpreading, new()
                                                              where TNew : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void ConvertSpreadingType_ExpectedResults()
        {
            // Setup
            var oldWaveEnergyFunction = new WaveEnergyFunction<TOld>();
            var generator = new TimeSeriesGenerator();
            generator.GenerateTimeSeries(oldWaveEnergyFunction.UnderlyingFunction, DateTime.Today, DateTime.Today + TimeSpan.FromDays(1), TimeSpan.FromHours(1));

            // Call
            IWaveEnergyFunction<TNew> newWaveFunction = WaveEnergyFunction<TNew>.ConvertSpreadingType(oldWaveEnergyFunction);

            // Assert
            Unit expectedUnit = SpreadingConversion.GetSpreadingUnit<TNew>();
            Assert.That(newWaveFunction.SpreadingComponent.Unit.Name, Is.EqualTo(expectedUnit.Name));
            Assert.That(newWaveFunction.SpreadingComponent.Unit.Symbol, Is.EqualTo(expectedUnit.Symbol));
            Assert.That(newWaveFunction.SpreadingComponent.DefaultValue, Is.EqualTo(SpreadingConversion.GetSpreadingDefaultValue<TNew>()));
            Assert.That(newWaveFunction.SpreadingComponent.AllValues, Has.All.EqualTo(SpreadingConversion.GetSpreadingDefaultValue<TNew>()));
        }
    }
}