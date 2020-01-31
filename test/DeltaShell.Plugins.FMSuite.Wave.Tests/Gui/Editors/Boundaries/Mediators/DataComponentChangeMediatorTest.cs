using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Mediators
{
    [TestFixture]
    public class DataComponentChangeMediatorTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var selectedSupportPointDependentViewModel =
                Substitute.For<IRefreshDataComponentViewModel>();

            // Call
            var mediator = new DataComponentChangeMediator(selectedSupportPointDependentViewModel);

            // Assert
            Assert.That(mediator, Is.InstanceOf<IAnnounceSelectedSupportPointDataChanged>());
        }

        [Test]
        public void Constructor_SelectedSupportPointDependentViewModelNull_ThrowsArgumentNullException()
        {
            void Call() => new DataComponentChangeMediator(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("selectedSupportPointDependentViewModel"));
        }

        [Test]
        public void AnnounceSelectedSupportPointDataChanged()
        {
            // Setup
            var selectedSupportPointDependentViewModel =
                Substitute.For<IRefreshDataComponentViewModel>();
            var mediator = new DataComponentChangeMediator(selectedSupportPointDependentViewModel);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(10.0);

            var supportPoint = new SupportPoint(0.0, geometricDefinition);

            // Call
            mediator.AnnounceSelectedSupportPointDataChanged(supportPoint);

            // Assert
            selectedSupportPointDependentViewModel.Received(1).UpdateSelectedActiveParameters(supportPoint);
        }
    }
}