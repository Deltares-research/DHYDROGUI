using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes
{
    [TestFixture]
    public class PiersonMoskowitzViewShapeTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var modelShape = new PiersonMoskowitzShape();

            // Call
            var viewShape = new PiersonMoskowitzViewShape(modelShape);

            // Assert
            Assert.That(viewShape, Is.InstanceOf<IViewShape>());
            Assert.That(viewShape.ObservedShape, Is.SameAs(modelShape),
                        "Expected a different ObservedShape:");
        }

        [Test]
        public void Constructor_JonswapShapeNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new PiersonMoskowitzViewShape(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("piersonMoskowitzShape"));
        }
    }
}