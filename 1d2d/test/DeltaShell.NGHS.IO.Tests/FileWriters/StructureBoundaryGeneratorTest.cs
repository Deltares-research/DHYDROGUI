using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters
{
    [TestFixture]
    public class StructureBoundaryGeneratorTest
    {
        private const string header = "forcing";
        private IStructureBoundaryGenerator structureBoundaryGenerator;
        private DateTime startTime;

        [SetUp]
        public void SetUp()
        {
            structureBoundaryGenerator = new StructureBoundaryGenerator();
            startTime = new DateTime(2022, 6, 21, 10,0,0);
        }
        
        [Test]
        [TestCaseSource(nameof(ArgNull))]
        public void WhenGenerateBoundaryArgumentsAreNull_ThenThrowArgumentNullException(string structureName, ITimeSeries timeSeries)
        {
            //Arrange
            var generator = new StructureBoundaryGenerator();
            
            //Act & Assert
            Assert.Throws<ArgumentNullException>(()=> generator.GenerateBoundary(structureName, timeSeries, new DateTime()));
        }

        [Test]
        public void WhenGenerateBoundariesArgumentsAreNull_ThenThrowArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(()=> structureBoundaryGenerator.GenerateBoundaries(null, new DateTime()));
        }

        [Test]
        public void WhenGettingSingleBoundary_ReturnSingleBoundaryWithExpectedData()
        {
            //Arrange
            ITimeSeries series = InitializeSubstituteTimeSeries();
            const string structureName = "structureName";

            //Act
            var singleBoundary = structureBoundaryGenerator.GenerateBoundary(structureName, series, startTime);
            
            //Assert
            var boundary = singleBoundary.First();
            Assert.That(boundary.Section.Name, Is.EqualTo(series.Name));
            Assert.That(boundary.Section.Properties.ElementAt(0).Value, Is.EqualTo(structureName));
            Assert.That(boundary.Section.Properties.ElementAt(1).Value, Is.EqualTo(BoundaryRegion.FunctionStrings.TimeSeries));
            Assert.That(boundary.Section.Properties.ElementAt(2).Value, Is.EqualTo(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate));
        }

        private static ITimeSeries InitializeSubstituteTimeSeries()
        {
            const string nameOfUnit = "nameOfTimeSeries";
            ITimeSeries series = Substitute.For<ITimeSeries>();
            IUnit unit = Substitute.For<IUnit>();
            IVariable variable = Substitute.For<IVariable>();
            unit.Name.Returns(nameOfUnit);
            series.Name.Returns(header);
            variable.Unit.Returns(unit);
            EventedList<IVariable> listWithUnit = new EventedList<IVariable> {variable};
            series.Components.Returns(listWithUnit);
            return series;
        }

        [Test]
        public void WhenGettingBoundary_ReturnBoundaryWithExpectedData()
        {
            //Arrange
            IEnumerable<IStructureTimeSeries> listOfData = InitializeListOfData();

            //Act
            var boundaries = structureBoundaryGenerator.GenerateBoundaries(listOfData, startTime);
            
            //Assert
            var boundary = boundaries.First();
            Assert.That(boundary.Section.Properties.ElementAt(0).Value, Is.EqualTo(header));
            Assert.That(boundary.Section.Properties.ElementAt(1).Value, Is.EqualTo(BoundaryRegion.FunctionStrings.TimeSeries));
            Assert.That(boundary.Section.Properties.ElementAt(2).Value, Is.EqualTo(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate));
        }

        private static IEnumerable<IStructureTimeSeries> InitializeListOfData()
        {
            var listOfData = new List<IStructureTimeSeries>();

            var structure = new Weir() { Name = header };

            var timeSeries = HydroTimeSeriesFactory.CreateTimeSeries("-", "-", "-");

            listOfData.Add(new StructureTimeSeries(structure, timeSeries));

            return listOfData;
        }
        
        private static IEnumerable<TestCaseData> ArgNull()
        {
            yield return new TestCaseData(null, Substitute.For<ITimeSeries>());
            yield return new TestCaseData(string.Empty, null);
        }
    }
}