using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class BoundaryDescriptionViewModelTest
    {
        private static IWaveBoundary GetConfiguredWaveBoundary(string name)
        {
            var boundary = Substitute.For<IWaveBoundary>();
            boundary.Name = name;

            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();
            boundary.ConditionDefinition.DataComponent = dataComponent;

            return boundary;
        }

        private static IViewDataComponentFactory GetConfiguredFactory(IBoundaryConditionDataComponent dataComponent,
                                                                      ForcingViewType forcingType, 
                                                                      SpatialDefinitionViewType spatialDefinition)
        {
            var factory = Substitute.For<IViewDataComponentFactory>();

            factory.GetForcingType(dataComponent).Returns(forcingType);
            factory.GetSpatialDefinition(dataComponent).Returns(spatialDefinition);

            return factory;
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const string expectedName = "aBoundaryName";
            const ForcingViewType expectedForcingType = ForcingViewType.Constant;
            const SpatialDefinitionViewType expectedSpatialDefinition = SpatialDefinitionViewType.SpatiallyVarying;

            IWaveBoundary boundary = GetConfiguredWaveBoundary(expectedName);

            IViewDataComponentFactory factory = GetConfiguredFactory(boundary.ConditionDefinition.DataComponent,
                                                                     expectedForcingType, 
                                                                     expectedSpatialDefinition);
            var announceDataComponentChanged = Substitute.For<IAnnounceDataComponentChanged>();

            // Call
            var viewModel = new BoundaryDescriptionViewModel(boundary, factory, announceDataComponentChanged);

            // Assert
            Assert.That(viewModel.Name, Is.EqualTo(expectedName), 
                        "Expected a different name:");
            Assert.That(viewModel.SpatialDefinition, Is.EqualTo(expectedSpatialDefinition),
                "Expected a different SpatialDefinition:");
            Assert.That(viewModel.ForcingType, Is.EqualTo(expectedForcingType),
                        "Expected a different ForcingType");
        }

        [Test]
        public void Constructor_ObservedBoundaryNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = Substitute.For<IViewDataComponentFactory>();
            var announceDataComponentChanged = Substitute.For<IAnnounceDataComponentChanged>();

            // Call
            void Call() => new BoundaryDescriptionViewModel(null, factory, announceDataComponentChanged);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("observedBoundary"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void Constructor_DataComponentFactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundary = Substitute.For<IWaveBoundary>();
            var announceDataComponentChanged = Substitute.For<IAnnounceDataComponentChanged>();

            // Call
            void Call() => new BoundaryDescriptionViewModel(boundary, null, announceDataComponentChanged);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("dataComponentFactory"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void Constructor_AnnounceDataComponentChangedNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundary = Substitute.For<IWaveBoundary>();
            var factory = Substitute.For<IViewDataComponentFactory>();

            // Call
            void Call() => new BoundaryDescriptionViewModel(boundary, factory, null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("announceDataComponentChanged"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void GivenABoundaryDescriptionViewModelWithABoundary_WhenNameIsSet_ThenTheNameInTheBoundaryIsAdjusted()
        {
            // Setup
            const string expectedName = "aBoundaryName";
            
            IWaveBoundary boundary = GetConfiguredWaveBoundary("someOtherName");
            IViewDataComponentFactory factory = GetConfiguredFactory(boundary.ConditionDefinition.DataComponent,
                                                                     ForcingViewType.Constant,
                                                                     SpatialDefinitionViewType.SpatiallyVarying);
            var announceDataComponentChanged = Substitute.For<IAnnounceDataComponentChanged>();

            var viewModel = new BoundaryDescriptionViewModel(boundary, factory, announceDataComponentChanged);

            // Call
            viewModel.Name = expectedName;


            // Assert
            boundary.Received(1).Name = expectedName;
        }
    }
}