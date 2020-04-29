using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    [TestFixture(typeof(PowerDefinedSpreading))]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    public class TimeDependentSpatiallyVaryingParametersViewModelTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var underlyingFunction = Substitute.For<IFunction>();
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            waveEnergyFunction.UnderlyingFunction.Returns(underlyingFunction);

            var supportPoint = new SupportPoint(10.0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);

            var allParameters = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>> {
                { supportPoint, parameters }
            };

            // Call
            var viewModel = new TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>(
                Substitute.For<IGenerateSeries>(),
                parameters, 
                allParameters);

            // Assert
            Assert.That(viewModel.ObservedParameters, Is.SameAs(parameters));
            Assert.That(viewModel.TimeDependentParametersFunctions, Is.EquivalentTo(new[] {underlyingFunction}));
        }

        [Test]
        public void Constructor_GenerateSeriesNull_ThrowsArgumentNullException()
        {
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var supportPoint = new SupportPoint(10.0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);

            var allParameters = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>> {

                { supportPoint, parameters }
            };

            void Call() => new TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>(
                null,
                parameters,
                allParameters);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("generateSeries"));
        }

        [Test]
        public void Constructor_ParametersNull_ThrowsArgumentNullException()
        {
            void Call() => new TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>(
                Substitute.For<IGenerateSeries>(),
                null,
                new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>());

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }

        [Test]
        public void Constructor_SupportPointToParametersMappingNull_ThrowsArgumentNullException()
        {
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);

            void Call() => new TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>(
                Substitute.For<IGenerateSeries>(),
                parameters,
                null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPointToParametersMapping"));
        }

        [Test]
        public void GenerateSeriesCommand_ExpectedResult()
        {
            // Setup
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();

            var supportPoint = new SupportPoint(10.0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);

            var otherFunctions = new List<IWaveEnergyFunction<TSpreading>>();

            var allParameters = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>> {

                { supportPoint, parameters }
            };

            for (var i = 0; i < 5; i++)
            {
                var otherWaveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
                var otherSupportPoint = new SupportPoint(10.0, Substitute.For<IWaveBoundaryGeometricDefinition>());
                var otherParameters = new TimeDependentParameters<TSpreading>(otherWaveEnergyFunction);

                allParameters.Add(otherSupportPoint, otherParameters);
                otherFunctions.Add(otherWaveEnergyFunction);
            }

            var generateSeries = Substitute.For<IGenerateSeries>();
            var viewModel = new TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>(generateSeries,
                                                                                             parameters, 
                                                                                             allParameters);

            var window = Substitute.For<IWin32Window>();

            // Call
            viewModel.GenerateTimeSeriesCommand.Execute(window);
            
            // Assert
            generateSeries.Received(1).Execute(Arg.Is(waveEnergyFunction), 
                                               Arg.Is<IEnumerable<IWaveEnergyFunction<TSpreading>>>(x => x.SequenceEqual(otherFunctions)));
        }
    }
}