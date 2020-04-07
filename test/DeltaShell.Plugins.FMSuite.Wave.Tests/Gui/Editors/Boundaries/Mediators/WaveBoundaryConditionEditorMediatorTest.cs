using System;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Mediators
{
    [TestFixture]
    public class WaveBoundaryConditionEditorMediatorTest
    {
        private IRefreshIsEnabledOnDataComponentChanged supportPointEditorViewModel;
        private IRefreshDataComponentViewModel parametersSettingsViewModel;
        private IRefreshViewModel refreshViewModel;

        [SetUp]
        public void SetUp()
        {
            supportPointEditorViewModel = Substitute.For<IRefreshIsEnabledOnDataComponentChanged>();
            parametersSettingsViewModel = Substitute.For<IRefreshDataComponentViewModel>();
            refreshViewModel = Substitute.For<IRefreshViewModel>();
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var mediator = new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel,
                                                                   parametersSettingsViewModel,
                                                                   refreshViewModel);

            // Assert
            Assert.That(mediator, Is.InstanceOf<IAnnounceDataComponentChanged>());
        }

        [Test]
        public void Constructor_SupportPointEditorViewModelNull_ThrowsArgumentNullException()
        {
            void Call() => new WaveBoundaryConditionEditorMediator(null, parametersSettingsViewModel, refreshViewModel);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponentIsEnabledDependentViewModel"));
        }

        [Test]
        public void Constructor_SpecificParametersSettingsViewModelNull_ThrowsArgumentNullException()
        {
            void Call() => new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel, null, refreshViewModel);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponentViewModelDependentViewModel"));
        }

        [Test]
        public void Constructor_RefreshViewModelNull_ThrowsArgumentNullException()
        {
            void Call() => new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel, parametersSettingsViewModel, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("refreshViewModel"));
        }

        [Test]
        public void GivenAMediator_WhenDataComponentChangedIsCalled_ThenTheAppropriateObjectsAreNotified()
        {
            // Given 
            var mediator = new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel,
                                                                   parametersSettingsViewModel,
                                                                   refreshViewModel);

            // When
            mediator.AnnounceDataComponentChanged();

            // Then
            Received.InOrder(() =>
            {
                parametersSettingsViewModel.RefreshDataComponentViewModel();
                supportPointEditorViewModel.RefreshIsEnabled();
                refreshViewModel.RefreshViewModel();
            });
        }
    }
}