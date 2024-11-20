using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes
{
    [TestFixture]
    public class GaussViewShapeTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const double expectedGaussianValue = 5.5;
            var modelShape = new GaussShape() {GaussianSpread = expectedGaussianValue};

            // Call
            var viewShape = new GaussViewShape(modelShape);

            // Assert
            Assert.That(viewShape, Is.InstanceOf<IViewShape>());
            Assert.That(viewShape.GaussianSpread, Is.EqualTo(expectedGaussianValue),
                        "Expected a different GaussianSpread:");
            Assert.That(viewShape.ObservedShape, Is.SameAs(modelShape),
                        "Expected a different ObservedShape:");
        }

        [Test]
        public void Constructor_GaussShapeNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new GaussViewShape(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("gaussShape"));
        }

        [Test]
        public void GivenAGaussShapeWith_WhenGaussianSpreadIsModified_ThenTheUnderlyingShapeIsModified()
        {
            // Setup
            const double expectedGaussianValue = 15.5;
            var modelShape = new GaussShape() {GaussianSpread = 0.1};
            var viewShape = new GaussViewShape(modelShape);

            // Call
            viewShape.GaussianSpread = expectedGaussianValue;

            // Assert
            Assert.That(modelShape.GaussianSpread, Is.EqualTo(expectedGaussianValue),
                        "Expected a different GaussianSpread:");
        }
    }
}