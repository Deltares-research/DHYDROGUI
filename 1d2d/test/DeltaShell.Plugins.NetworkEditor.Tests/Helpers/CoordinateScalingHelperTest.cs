using System;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Helpers
{
    [TestFixture]
    public class CoordinateScalingHelperTest
    {
        [Test]
        [TestCase(0, 0, 10, 100, 0)]
        [TestCase(10, 0, 10, 100, 100)]
        [TestCase(5, -3, 7, 325, 260)]
        [TestCase(50, -30, 70, 3.25, 2.60)]

        public void WhenScalingXValue_GiveExpectedResult(double x, double minX, double maxX, double targetWidth, double expectedValue)
        {
            AssertDouble(() => CoordinateScalingHelper.ScaleX(x, minX, maxX, targetWidth), expectedValue);
        }

        [Test]
        [TestCase(0, 0, 10, 100, 100)]
        [TestCase(10, 0, 10, 100, 0)]
        [TestCase(5, -3, 7, 325, 65)]
        [TestCase(50, -30, 70, 3.25, 0.65)]
        public void WhenScalingYValue_GiveExpectedResult(double y, double minY, double maxY, double targetHeight, double expectedValue)
        {
            AssertDouble(() => CoordinateScalingHelper.ScaleY(y, minY, maxY, targetHeight), expectedValue);
        }

        [Test]
        [TestCase(0, 0, 10, 100, 0)]
        [TestCase(10, 0, 10, 100, 100)]
        [TestCase(5, -3, 7, 325, 162.5)]
        [TestCase(50, -30, 70, 3.25, 1.625)]
        public void WhenScalingWidth_GiveExpectedResult(double width, double minX, double maxX, double targetWidth, double expectedValue)
        {
            AssertDouble(() => CoordinateScalingHelper.ScaleWidth(width, minX, maxX, targetWidth), expectedValue);
        }

        [Test]
        [TestCase(0, 0, 10, 100, 0)]
        [TestCase(10, 0, 10, 100, 100)]
        [TestCase(5, -3, 7, 325, 162.5)]
        [TestCase(50, -30, 70, 3.25, 1.625)]
        public void WhenScalingHeight_GiveExpectedResult(double height, double minY, double maxY, double targetHeight, double expectedValue)
        {
            AssertDouble(() => CoordinateScalingHelper.ScaleHeight(height, minY, maxY, targetHeight), expectedValue);
        }

        private void AssertDouble(Func<double> f, double expectedResult, double tolerance = 1e-7)
        {
            Assert.AreEqual(expectedResult, f.Invoke(), tolerance);
        }
    }
}