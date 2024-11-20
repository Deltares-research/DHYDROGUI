using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes
{
    [TestFixture]
    public class JonswapViewShapeTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const double expectedPeakEnhancementFactor = 5.5;
            var modelShape = new JonswapShape() {PeakEnhancementFactor = expectedPeakEnhancementFactor};

            // Call
            var viewShape = new JonswapViewShape(modelShape);

            // Assert
            Assert.That(viewShape, Is.InstanceOf<IViewShape>());
            Assert.That(viewShape.PeakEnhancementFactor, Is.EqualTo(expectedPeakEnhancementFactor),
                        "Expected a different PeakEnhancementFactor:");
            Assert.That(viewShape.ObservedShape, Is.SameAs(modelShape),
                        "Expected a different ObservedShape:");
        }

        [Test]
        public void Constructor_JonswapShapeNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new JonswapViewShape(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("jonswapShape"));
        }

        [Test]
        public void GivenAGaussShapeWith_WhenGaussianSpreadIsModified_ThenTheUnderlyingShapeIsModified()
        {
            // Setup
            const double expectedPeakEnhancementFactor = 15.5;
            var modelShape = new JonswapShape() {PeakEnhancementFactor = 0.1};
            var viewShape = new JonswapViewShape(modelShape);

            // Call
            viewShape.PeakEnhancementFactor = expectedPeakEnhancementFactor;

            // Assert
            Assert.That(modelShape.PeakEnhancementFactor, Is.EqualTo(expectedPeakEnhancementFactor),
                        "Expected a different PeakEnhancementFactor:");
        }
    }
}