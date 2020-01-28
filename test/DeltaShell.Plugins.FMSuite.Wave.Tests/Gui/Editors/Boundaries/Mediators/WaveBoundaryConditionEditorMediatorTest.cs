using System;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Mediators
{
    [TestFixture]
    public class WaveBoundaryConditionEditorMediatorTest
    {
        private static SupportPointEditorViewModel GetConfiguredSupportPointEditorViewModel()
        {
            const double maxDistance = 20.0;
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(maxDistance);
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>()
            {
                new SupportPoint(0.0, geometricDefinition),
                new SupportPoint(maxDistance, geometricDefinition),
            });

            var waveBoundary = Substitute.For<IWaveBoundary>();
            waveBoundary.GeometricDefinition.Returns(geometricDefinition);

            return new SupportPointEditorViewModel(waveBoundary);
        }

        private static BoundarySpecificParametersSettingsViewModel GetConfiguredParametersSettingsViewModel()
        {
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var factory = Substitute.For<IViewDataComponentFactory>();
            var dataComponentViewModel = Substitute.For<IParametersSettingsViewModel>();

            factory.ConstructParametersSettingsViewModel(conditionDefinition.DataComponent)
                   .Returns(dataComponentViewModel);

            // Call
            return new BoundarySpecificParametersSettingsViewModel(conditionDefinition, factory);
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            SupportPointEditorViewModel supportPointEditorViewModel = 
                GetConfiguredSupportPointEditorViewModel();
            BoundarySpecificParametersSettingsViewModel parametersSettingsViewModel =
                GetConfiguredParametersSettingsViewModel();

            // Call
            var mediator = new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel, 
                                                                   parametersSettingsViewModel);

            // Assert
            Assert.That(mediator, Is.InstanceOf<IAnnounceDataComponentChanged>());
        }

        [Test]
        public void Constructor_SupportPointEditorViewModelNull_ThrowsArgumentNullException()
        {
            BoundarySpecificParametersSettingsViewModel parametersSettingsViewModel =
                GetConfiguredParametersSettingsViewModel();
            
            void Call() => new WaveBoundaryConditionEditorMediator(null, parametersSettingsViewModel);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPointEditorViewModel"));
        }
        
        [Test]
        public void Constructor_SpecificParametersSettingsViewModelNull_ThrowsArgumentNullException()
        {
            SupportPointEditorViewModel supportPointEditorViewModel = 
                GetConfiguredSupportPointEditorViewModel();

            void Call() => new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("specificParametersSettingsViewModel"));
        }
    }
}