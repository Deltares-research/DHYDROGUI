using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Enums
{
    [TestFixture]
    public class WaveBoundaryConditionEditorEnumExtensionsTest
    {
        [Test]
        [TestCase(BoundaryConditionPeriodType.Peak, PeriodViewType.Peak)]
        [TestCase(BoundaryConditionPeriodType.Mean, PeriodViewType.Mean)]
        public void ConvertToPeriodViewType_ReturnsCorrectValues(BoundaryConditionPeriodType input, PeriodViewType expectedOutput)
        {
            // Call
            PeriodViewType result = input.ConvertToPeriodViewType();

            // Assert
            Assert.That(result, Is.EqualTo(expectedOutput));
        }

        [Test]
        [TestCase(PeriodViewType.Peak, BoundaryConditionPeriodType.Peak)]
        [TestCase(PeriodViewType.Mean, BoundaryConditionPeriodType.Mean)]
        public void ConvertToBoundaryConditionPeriodType_ReturnsCorrectValues(PeriodViewType input, BoundaryConditionPeriodType expectedOutput)
        {
            // Call
            BoundaryConditionPeriodType result = input.ConvertToBoundaryConditionPeriodType();

            // Assert
            Assert.That(result, Is.EqualTo(expectedOutput));
        }
    }
}