using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
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
            var viewModel = new TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>(parameters, allParameters);

            // Assert
            Assert.That(viewModel.ObservedParameters, Is.SameAs(parameters));
            Assert.That(viewModel.TimeDependentParametersFunctions, Is.EquivalentTo(new[] {underlyingFunction}));
        }

        [Test]
        public void Constructor_ParametersNull_ThrowsArgumentNullException()
        {
            void Call() => new TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>(
                null,
                new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>());

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }

        [Test]
        public void Constructor_Null_ThrowsArgumentNullException()
        {
            void Call() => new TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>(
                null,
                new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>());

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }
    }
}