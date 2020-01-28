using System;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Mediators
{
    [TestFixture]
    public class WaveBoundaryConditionEditorMediatorTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var supportPointEditorViewModel = Substitute.For<IRefreshIsEnabledOnDataComponentChanged>();
            var parametersSettingsViewModel = Substitute.For<IRefreshDataComponentViewModel>();

            // Call
            var mediator = new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel, 
                                                                   parametersSettingsViewModel);

            // Assert
            Assert.That(mediator, Is.InstanceOf<IAnnounceDataComponentChanged>());
        }

        [Test]
        public void Constructor_SupportPointEditorViewModelNull_ThrowsArgumentNullException()
        {
            var parametersSettingsViewModel = Substitute.For<IRefreshDataComponentViewModel>();
            
            void Call() => new WaveBoundaryConditionEditorMediator(null, parametersSettingsViewModel);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponentIsEnabledDependentViewModel"));
        }
        
        [Test]
        public void Constructor_SpecificParametersSettingsViewModelNull_ThrowsArgumentNullException()
        {
            var supportPointEditorViewModel = Substitute.For<IRefreshIsEnabledOnDataComponentChanged>();

            void Call() => new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponentViewModelDependentViewModel"));
        }

        [Test]
        public void GivenAMediator_WhenDataComponentChangedIsCalled_ThenTheAppropriateObjectsAreNotified()
        {
            // Given 
            var supportPointEditorViewModel = Substitute.For<IRefreshIsEnabledOnDataComponentChanged>();
            var parametersSettingsViewModel = Substitute.For<IRefreshDataComponentViewModel>();

            var mediator = new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel, 
                                                                   parametersSettingsViewModel);

            // When
            mediator.AnnounceDataComponentChanged();

            // Then
            supportPointEditorViewModel.Received(1).RefreshIsEnabled();
            parametersSettingsViewModel.Received(1).RefreshDataComponentViewModel();
        }
    }
}