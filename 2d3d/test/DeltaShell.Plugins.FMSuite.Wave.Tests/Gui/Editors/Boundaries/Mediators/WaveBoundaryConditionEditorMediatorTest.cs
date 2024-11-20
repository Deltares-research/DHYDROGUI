using System;
using System.Collections.Generic;
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
        private IRefreshGeometryView refreshGeometryView;

        [SetUp]
        public void SetUp()
        {
            supportPointEditorViewModel = Substitute.For<IRefreshIsEnabledOnDataComponentChanged>();
            parametersSettingsViewModel = Substitute.For<IRefreshDataComponentViewModel>();
            refreshViewModel = Substitute.For<IRefreshViewModel>();
            refreshGeometryView = Substitute.For<IRefreshGeometryView>();
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var mediator = new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel,
                                                                   parametersSettingsViewModel,
                                                                   refreshViewModel,
                                                                   refreshGeometryView);

            // Assert
            Assert.That(mediator, Is.InstanceOf<IAnnounceDataComponentChanged>());
        }

        [Test]
        [TestCaseSource(nameof(GetConstructorNullArgumentTestData))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(IRefreshIsEnabledOnDataComponentChanged dataComponentIsEnabledDependentViewModel,
                                                                         IRefreshDataComponentViewModel dataComponentViewModelDependentViewModel,
                                                                         IRefreshViewModel refreshViewModelParam,
                                                                         IRefreshGeometryView refreshGeometryViewParam,
                                                                         string expectedParamName)
        {
            void Call() => new WaveBoundaryConditionEditorMediator(dataComponentIsEnabledDependentViewModel, dataComponentViewModelDependentViewModel, refreshViewModelParam, refreshGeometryViewParam);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        [Test]
        public void GivenAMediator_WhenDataComponentChangedIsCalled_ThenTheAppropriateObjectsAreNotified()
        {
            // Given 
            var mediator = new WaveBoundaryConditionEditorMediator(supportPointEditorViewModel,
                                                                   parametersSettingsViewModel,
                                                                   refreshViewModel,
                                                                   refreshGeometryView);

            // When
            mediator.AnnounceDataComponentChanged();

            // Then
            Received.InOrder(() =>
            {
                parametersSettingsViewModel.RefreshDataComponentViewModel();
                supportPointEditorViewModel.RefreshIsEnabled();
                refreshViewModel.RefreshViewModel();
                refreshGeometryView.RefreshGeometryView();
            });
        }

        private static IEnumerable<TestCaseData> GetConstructorNullArgumentTestData()
        {
            var supportPointEditorViewModel = Substitute.For<IRefreshIsEnabledOnDataComponentChanged>();
            var parametersSettingsViewModel = Substitute.For<IRefreshDataComponentViewModel>();
            var refreshViewModel = Substitute.For<IRefreshViewModel>();
            var refreshGeometryView = Substitute.For<IRefreshGeometryView>();

            yield return new TestCaseData(null, parametersSettingsViewModel, refreshViewModel, refreshGeometryView, "dataComponentIsEnabledDependentViewModel");
            yield return new TestCaseData(supportPointEditorViewModel, null, refreshViewModel, refreshGeometryView, "dataComponentViewModelDependentViewModel");
            yield return new TestCaseData(supportPointEditorViewModel, parametersSettingsViewModel, null, refreshGeometryView, "refreshViewModel");
            yield return new TestCaseData(supportPointEditorViewModel, parametersSettingsViewModel, refreshViewModel, null, "refreshGeometryView");
        }
    }
}