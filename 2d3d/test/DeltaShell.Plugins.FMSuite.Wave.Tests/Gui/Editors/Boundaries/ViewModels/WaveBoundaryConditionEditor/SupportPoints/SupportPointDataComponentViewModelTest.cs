using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints
{
    [TestFixture(typeof(PowerDefinedSpreading))]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    public class SupportPointDataComponentViewModelTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();

            // Call
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Assert
            Assert.That(viewModel.ObservedDataComponent, Is.SameAs(conditionDefinition.DataComponent));
        }

        [Test]
        public void Constructor_ConditionDefinitionNull_ThrowsArgumentNullException()
        {
            // Setup
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();

            // Call | Assert
            void Call() => new SupportPointDataComponentViewModel(null, parametersFactory, mediator);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("conditionDefinition"));
        }

        [Test]
        public void Constructor_ParametersFactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();

            // Call | Assert
            void Call() => new SupportPointDataComponentViewModel(conditionDefinition, null, mediator);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("parametersFactory"));
        }

        [Test]
        public void Constructor_AnnounceSelectedSupportPointDataChangedNull_ThrowsArgumentNullException()
        {
            // Setup
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            // Call | Assert
            void Call() => new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("announceSupportPointDataChanged"));
        }

        [Test]
        [TestCaseSource(nameof(GetIsEnabledData))]
        public void IsEnabled_ExpectedResults(IWaveBoundaryConditionDefinition conditionDefinition, bool expectedResults)
        {
            // Setup
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call
            bool result = viewModel.IsEnabled();

            // Assert
            Assert.That(result, Is.EqualTo(expectedResults));
        }

        [Test]
        [TestCaseSource(nameof(GetIsEnabledSupportPointData))]
        public void IsEnabledSupportPoint_ExpectedResults(IWaveBoundaryConditionDefinition conditionDefinition,
                                                          SupportPoint supportPoint,
                                                          bool expectedIsEnabled)
        {
            // Setup
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call
            bool result = viewModel.IsEnabledSupportPoint(supportPoint);

            // Assert
            Assert.That(result, Is.EqualTo(expectedIsEnabled));
        }

        [Test]
        public void IsEnabledSupportPoint_SupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call | Assert
            void Call() => viewModel.IsEnabledSupportPoint(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }

        [Test]
        public void AddDefaultParameters_ConstantParameters_ExpectedResults()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            parametersFactory.ConstructDefaultConstantParameters<TSpreading>().Returns(parameters);
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();

            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);
            SupportPoint supportPoint = GetDefaultSupportPoint();

            // Call
            viewModel.AddDefaultParameters(supportPoint);

            // Assert
            Assert.That(dataComponent.Data.ContainsKey(supportPoint),
                        "The data component should contain the newly added SupportPoint, but did not:");
            Assert.That(dataComponent.Data[supportPoint], Is.SameAs(parameters));
            parametersFactory.Received(1).ConstructDefaultConstantParameters<TSpreading>();
        }

        [Test]
        public void AddDefaultParameters_TimeDependentParameters_ExpectedResults()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponent =
                new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var parameters = new TimeDependentParameters<TSpreading>(Substitute.For<IWaveEnergyFunction<TSpreading>>());
            parametersFactory.ConstructDefaultTimeDependentParameters<TSpreading>().Returns(parameters);
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();

            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);
            SupportPoint supportPoint = GetDefaultSupportPoint();

            // Call
            viewModel.AddDefaultParameters(supportPoint);

            // Assert
            Assert.That(dataComponent.Data.ContainsKey(supportPoint),
                        "The data component should contain the newly added SupportPoint, but did not:");
            Assert.That(dataComponent.Data[supportPoint], Is.SameAs(parameters));
            parametersFactory.Received(1).ConstructDefaultTimeDependentParameters<TSpreading>();
        }

        [Test]
        public void AddDefaultParameters_FileBasedParameters_ExpectedResults()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponent =
                new SpatiallyVaryingDataComponent<FileBasedParameters>();
            conditionDefinition.DataComponent = dataComponent;

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var parameters = new FileBasedParameters("path");
            parametersFactory.ConstructDefaultFileBasedParameters().Returns(parameters);
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();

            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);
            SupportPoint supportPoint = GetDefaultSupportPoint();

            // Call
            viewModel.AddDefaultParameters(supportPoint);

            // Assert
            Assert.That(dataComponent.Data.ContainsKey(supportPoint),
                        "The data component should contain the newly added SupportPoint, but did not:");
            Assert.That(dataComponent.Data[supportPoint], Is.SameAs(parameters));
            parametersFactory.Received(1).ConstructDefaultFileBasedParameters();
        }

        [Test]
        public void AddDefaultParameters_SupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call | Assert
            void Call() => viewModel.AddDefaultParameters(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }

        [Test]
        public void AddDefaultParameters_InvalidDataComponent_ThrowsInvalidOperationException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);
            SupportPoint supportPoint = GetDefaultSupportPoint();

            // Call | Assert
            void Call() => viewModel.AddDefaultParameters(supportPoint);
            Assert.Throws<InvalidOperationException>(Call);
        }

        [Test]
        public void RemoveParameters_ConstantParameters_ExpectedResults()
        {
            // Setup
            SupportPoint supportPoint = GetDefaultSupportPoint();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var dataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            dataComponent.AddParameters(supportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call
            viewModel.RemoveParameters(supportPoint);

            // Assert
            Assert.That(dataComponent.Data.ContainsKey(supportPoint), Is.False,
                        "The data component should not contain the removed SupportPoint:");
        }

        [Test]
        public void RemoveParameters_TimeDependentParameters_ExpectedResults()
        {
            // Setup
            SupportPoint supportPoint = GetDefaultSupportPoint();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var dataComponent =
                new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parameters = new TimeDependentParameters<TSpreading>(Substitute.For<IWaveEnergyFunction<TSpreading>>());
            dataComponent.AddParameters(supportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call
            viewModel.RemoveParameters(supportPoint);

            // Assert
            Assert.That(dataComponent.Data.ContainsKey(supportPoint), Is.False,
                        "The data component should not contain the removed SupportPoint:");
        }

        [Test]
        public void RemoveParameters_FileBasedParameters_ExpectedResults()
        {
            // Setup
            SupportPoint supportPoint = GetDefaultSupportPoint();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var dataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            conditionDefinition.DataComponent = dataComponent;

            var parameters = new FileBasedParameters("path");
            dataComponent.AddParameters(supportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call
            viewModel.RemoveParameters(supportPoint);

            // Assert
            Assert.That(dataComponent.Data.ContainsKey(supportPoint), Is.False,
                        "The data component should not contain the removed SupportPoint:");
        }

        [Test]
        public void RemoveParameters_SupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call | Assert
            void Call() => viewModel.RemoveParameters(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }

        [Test]
        public void RemoveParameters_InvalidDataComponent_ThrowsInvalidOperationException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);
            SupportPoint supportPoint = GetDefaultSupportPoint();

            // Call | Assert
            void Call() => viewModel.RemoveParameters(supportPoint);
            Assert.Throws<InvalidOperationException>(Call);
        }

        [Test]
        public void ReplaceSupportPoint_ConstantParameters_ExpectedResults()
        {
            // Setup
            SupportPoint oldSupportPoint = GetDefaultSupportPoint();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var dataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            dataComponent.AddParameters(oldSupportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            SupportPoint newSupportPoint = GetDefaultSupportPoint();

            // Call
            viewModel.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);

            // Assert
            Assert.That(dataComponent.Data.ContainsKey(oldSupportPoint), Is.False,
                        "The data component should not contain the old SupportPoint:");
            Assert.That(dataComponent.Data.ContainsKey(newSupportPoint), Is.True,
                        "The data component should contain the new SupportPoint:");
            Assert.That(dataComponent.Data[newSupportPoint], Is.SameAs(parameters));
        }

        [Test]
        public void ReplaceSupportPoint_TimeDependentParameters_ExpectedResults()
        {
            // Setup
            SupportPoint oldSupportPoint = GetDefaultSupportPoint();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var dataComponent =
                new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parameters = new TimeDependentParameters<TSpreading>(Substitute.For<IWaveEnergyFunction<TSpreading>>());
            dataComponent.AddParameters(oldSupportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            SupportPoint newSupportPoint = GetDefaultSupportPoint();

            // Call
            viewModel.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);

            // Assert
            Assert.That(dataComponent.Data.ContainsKey(oldSupportPoint), Is.False,
                        "The data component should not contain the old SupportPoint:");
            Assert.That(dataComponent.Data.ContainsKey(newSupportPoint), Is.True,
                        "The data component should contain the new SupportPoint:");
            Assert.That(dataComponent.Data[newSupportPoint], Is.SameAs(parameters));
        }

        [Test]
        public void ReplaceSupportPoint_FileBasedParameters_ExpectedResults()
        {
            // Setup
            SupportPoint oldSupportPoint = GetDefaultSupportPoint();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var dataComponent =
                new SpatiallyVaryingDataComponent<FileBasedParameters>();
            conditionDefinition.DataComponent = dataComponent;

            var parameters = new FileBasedParameters("path");
            dataComponent.AddParameters(oldSupportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            SupportPoint newSupportPoint = GetDefaultSupportPoint();

            // Call
            viewModel.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);

            // Assert
            Assert.That(dataComponent.Data.ContainsKey(oldSupportPoint), Is.False,
                        "The data component should not contain the old SupportPoint:");
            Assert.That(dataComponent.Data.ContainsKey(newSupportPoint), Is.True,
                        "The data component should contain the new SupportPoint:");
            Assert.That(dataComponent.Data[newSupportPoint], Is.SameAs(parameters));
        }

        [Test]
        public void ReplaceSupportPoint_OldSupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call | Assert
            void Call() => viewModel.ReplaceSupportPoint(null, GetDefaultSupportPoint());
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("oldSupportPoint"));
        }

        [Test]
        public void ReplaceSupportPoint_NewSupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call | Assert
            void Call() => viewModel.ReplaceSupportPoint(GetDefaultSupportPoint(), null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("newSupportPoint"));
        }

        [Test]
        public void ReplaceSupportPoint_NewSupportPointAlreadyExists_ThrowsInvalidArgumentException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            SupportPoint newSupportPoint = GetDefaultSupportPoint();
            SupportPoint oldSupportPoint = GetDefaultSupportPoint();
            dataComponent.AddParameters(oldSupportPoint, new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading()));
            dataComponent.AddParameters(newSupportPoint, new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading()));

            // Call | Assert
            void Call() => viewModel.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);
            Assert.Throws<InvalidOperationException>(Call);
        }

        [Test]
        public void ReplaceSupportPoint_OldSupportPointDoesNotExist_ThrowsInvalidArgumentException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call | Assert
            void Call() => viewModel.ReplaceSupportPoint(GetDefaultSupportPoint(),
                                                         GetDefaultSupportPoint());

            Assert.Throws<InvalidOperationException>(Call);
        }

        [Test]
        public void ReplaceSupportPoint_InvalidDataComponent_ThrowsInvalidOperationException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            // Call | Assert
            void Call() => viewModel.ReplaceSupportPoint(GetDefaultSupportPoint(),
                                                         GetDefaultSupportPoint());

            Assert.Throws<InvalidOperationException>(Call);
        }

        [Test]
        public void SelectedSupportPoint_Set_CallsAnnounceSelectedSupportPointDataChanged()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(15.0);
            var supportPoint = new SupportPoint(10.0, geometricDefinition);

            // Call
            viewModel.SelectedSupportPoint = supportPoint;

            // Assert
            mediator.Received(1).AnnounceSupportPointsChanged();
            mediator.Received(1).AnnounceSelectedSupportPointDataChanged(supportPoint);
            Assert.That(viewModel.SelectedSupportPoint, Is.SameAs(supportPoint));
        }

        [Test]
        public void AddDefaultParameters_SupportPointIsSelected_AnnounceSelectedSupportPointDataChanged()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            parametersFactory.ConstructDefaultConstantParameters<TSpreading>().Returns(parameters);
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();

            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);
            SupportPoint supportPoint = GetDefaultSupportPoint();
            viewModel.SelectedSupportPoint = supportPoint;

            mediator.ClearReceivedCalls();

            // Call
            viewModel.AddDefaultParameters(supportPoint);

            // Assert
            mediator.Received(1).AnnounceSupportPointsChanged();
            mediator.Received(1).AnnounceSelectedSupportPointDataChanged(supportPoint);
        }

        [Test]
        public void RemoveParameters_SupportPointIsSelected_AnnounceSelectedSupportPointDataChanged()
        {
            // Setup
            SupportPoint supportPoint = GetDefaultSupportPoint();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var dataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            dataComponent.AddParameters(supportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            viewModel.SelectedSupportPoint = supportPoint;
            mediator.ClearReceivedCalls();
            // Call
            viewModel.RemoveParameters(supportPoint);

            // Assert
            mediator.Received(1).AnnounceSupportPointsChanged();
            mediator.Received(1).AnnounceSelectedSupportPointDataChanged(supportPoint);
        }

        [Test]
        public void AddDefaultParameters_SupportPointNotSelected_OnlyAnnounceSupportPointsDataChanged()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            parametersFactory.ConstructDefaultConstantParameters<TSpreading>().Returns(parameters);
            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();

            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);
            SupportPoint supportPoint = GetDefaultSupportPoint();

            mediator.ClearReceivedCalls();

            // Call
            viewModel.AddDefaultParameters(supportPoint);

            // Assert
            mediator.Received(1).AnnounceSupportPointsChanged();
            mediator.DidNotReceiveWithAnyArgs().AnnounceSelectedSupportPointDataChanged(null);
        }

        [Test]
        public void RemoveParameters_SupportPointNotSelected_OnlyAnnounceSupportPointsDataChanged()
        {
            // Setup
            SupportPoint supportPoint = GetDefaultSupportPoint();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var dataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            dataComponent.AddParameters(supportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            mediator.ClearReceivedCalls();
            // Call
            viewModel.RemoveParameters(supportPoint);

            // Assert
            mediator.Received(1).AnnounceSupportPointsChanged();
            mediator.DidNotReceiveWithAnyArgs().AnnounceSelectedSupportPointDataChanged(null);
        }

        [Test]
        public void ReplaceParameters_AnnounceSupportPointsDataChanged()
        {
            // Setup
            SupportPoint oldSupportPoint = GetDefaultSupportPoint();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var dataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            dataComponent.AddParameters(oldSupportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            mediator.ClearReceivedCalls();

            // Call
            viewModel.ReplaceSupportPoint(oldSupportPoint,
                                          GetDefaultSupportPoint());

            // Assert
            mediator.Received(1).AnnounceSupportPointsChanged();
        }

        private static IEnumerable<TestCaseData> GetIsEnabledData()
        {
            var waveBoundaryIsEnabledConstant = Substitute.For<IWaveBoundaryConditionDefinition>();
            waveBoundaryIsEnabledConstant.DataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            yield return new TestCaseData(waveBoundaryIsEnabledConstant, true);

            var waveBoundaryIsEnabledTimeDependent = Substitute.For<IWaveBoundaryConditionDefinition>();
            waveBoundaryIsEnabledTimeDependent.DataComponent =
                new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();
            yield return new TestCaseData(waveBoundaryIsEnabledTimeDependent, true);

            var waveBoundaryIsEnabledFileBased = Substitute.For<IWaveBoundaryConditionDefinition>();
            waveBoundaryIsEnabledFileBased.DataComponent =
                new SpatiallyVaryingDataComponent<FileBasedParameters>();
            yield return new TestCaseData(waveBoundaryIsEnabledFileBased, true);

            var waveBoundaryIsNotEnabledConstant = Substitute.For<IWaveBoundaryConditionDefinition>();
            waveBoundaryIsNotEnabledConstant.DataComponent =
                new UniformDataComponent<ConstantParameters<TSpreading>>(new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading()));
            yield return new TestCaseData(waveBoundaryIsNotEnabledConstant, false);

            var waveBoundaryIsNotEnabledTimeDependent = Substitute.For<IWaveBoundaryConditionDefinition>();
            waveBoundaryIsNotEnabledTimeDependent.DataComponent =
                new UniformDataComponent<TimeDependentParameters<TSpreading>>(new TimeDependentParameters<TSpreading>(Substitute.For<IWaveEnergyFunction<TSpreading>>()));
            yield return new TestCaseData(waveBoundaryIsNotEnabledTimeDependent, false);

            var waveBoundaryIsNotEnabledFileBased = Substitute.For<IWaveBoundaryConditionDefinition>();
            waveBoundaryIsNotEnabledFileBased.DataComponent =
                new UniformDataComponent<FileBasedParameters>(new FileBasedParameters("path"));
            yield return new TestCaseData(waveBoundaryIsNotEnabledFileBased, false);
        }

        private static SupportPoint GetDefaultSupportPoint()
        {
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(10.0);

            return new SupportPoint(0.0, geometricDefinition);
        }

        private static IEnumerable<TestCaseData> GetIsEnabledSupportPointData()
        {
            SupportPoint supportPoint = GetDefaultSupportPoint();

            var disabledSpatiallyVaryingConstantDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            disabledSpatiallyVaryingConstantDefinition.DataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            yield return new TestCaseData(disabledSpatiallyVaryingConstantDefinition, supportPoint, false);

            var disabledSpatiallyVaryingTimeDependentDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            disabledSpatiallyVaryingTimeDependentDefinition.DataComponent =
                new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();
            yield return new TestCaseData(disabledSpatiallyVaryingTimeDependentDefinition, supportPoint, false);

            var disabledUniformConstantDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            disabledUniformConstantDefinition.DataComponent =
                new UniformDataComponent<ConstantParameters<TSpreading>>(new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading()));
            yield return new TestCaseData(disabledUniformConstantDefinition, supportPoint, false);

            var disabledUniformTimeDependentDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            disabledUniformTimeDependentDefinition.DataComponent =
                new UniformDataComponent<TimeDependentParameters<TSpreading>>(new TimeDependentParameters<TSpreading>(Substitute.For<IWaveEnergyFunction<TSpreading>>()));
            yield return new TestCaseData(disabledUniformTimeDependentDefinition, supportPoint, false);

            var disabledUniformFileBasedDependentDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            disabledUniformFileBasedDependentDefinition.DataComponent =
                new UniformDataComponent<FileBasedParameters>(new FileBasedParameters("path"));
            yield return new TestCaseData(disabledUniformFileBasedDependentDefinition, supportPoint, false);

            var enabledSpatiallyVaryingConstantDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponentConstant =
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            dataComponentConstant.AddParameters(supportPoint, new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading()));
            enabledSpatiallyVaryingConstantDefinition.DataComponent = dataComponentConstant;
            yield return new TestCaseData(enabledSpatiallyVaryingConstantDefinition, supportPoint, true);

            var enabledSpatiallyVaryingTimeDependentDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponentTimeDependent =
                new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();
            dataComponentTimeDependent.AddParameters(supportPoint, new TimeDependentParameters<TSpreading>(Substitute.For<IWaveEnergyFunction<TSpreading>>()));
            enabledSpatiallyVaryingTimeDependentDefinition.DataComponent = dataComponentTimeDependent;
            yield return new TestCaseData(enabledSpatiallyVaryingTimeDependentDefinition, supportPoint, true);

            var enabledSpatiallyVaryingFileBasedDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponentFileBased =
                new SpatiallyVaryingDataComponent<FileBasedParameters>();
            dataComponentFileBased.AddParameters(supportPoint, new FileBasedParameters("path"));
            enabledSpatiallyVaryingFileBasedDefinition.DataComponent = dataComponentFileBased;
            yield return new TestCaseData(enabledSpatiallyVaryingFileBasedDefinition, supportPoint, true);
        }
    }
}