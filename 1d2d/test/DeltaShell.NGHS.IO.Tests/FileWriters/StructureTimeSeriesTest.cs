using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters
{
    [TestFixture]
    public class StructureTimeSeriesTest
    {
        [Test]
        [TestCaseSource(nameof(Constructor_ArgumentNull_TestData))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(IStructure1D structure, TimeSeries timeSeries, string propertyName)
        {
            // Call
            void Action() => new StructureTimeSeries(structure, timeSeries);

            // Assert
            Assert.That(Action, Throws.ArgumentNullException
                                      .With.Property(nameof(ArgumentNullException.ParamName)).EqualTo(propertyName));
        }

        private static IEnumerable<TestCaseData> Constructor_ArgumentNull_TestData()
        {
            yield return new TestCaseData(null, new TimeSeries(), "structure").SetName("Structure null");
            yield return new TestCaseData(Substitute.For<IStructure1D>(), null, "timeSeries").SetName("Time series null");
        }
    }
}