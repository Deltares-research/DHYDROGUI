using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class MdwBoundaryCategoryGeometryExtenderTest
    {
        [Test]
        public void AddNewProperties_ForBoundaryGeometryDefinitionWithUnsortedSupportPoints()
        {
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            
            var supportPoint1 = new SupportPoint(0, geometryDefinition);
            var supportPoint2 = new SupportPoint(33, geometryDefinition);
            
            var supportPoints = new EventedList<SupportPoint>
            {
                supportPoint2,
                supportPoint1
            };
            
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();

            var coordinate1 = Substitute.For<Coordinate>();
            coordinate1.X = 1.5;
            coordinate1.Y = 2.5;

            var coordinate2 = Substitute.For<Coordinate>();
            coordinate2.X = 3.5;
            coordinate2.Y = 4.5;

            boundaryContainer.GetBoundarySnappingCalculator().Returns(boundarySnappingCalculator);
            boundarySnappingCalculator.CalculateCoordinateFromSupportPoint(supportPoint1).Returns(coordinate1);
            boundarySnappingCalculator.CalculateCoordinateFromSupportPoint(supportPoint2).Returns(coordinate2);

            MdwBoundaryCategoryGeometryExtender.AddNewProperties(category, boundaryContainer, supportPoints);

            List<DelftIniProperty> properties = category.Properties.ToList();
            Assert.AreEqual(5, properties.Count);
            Assert.AreEqual(KnownWaveProperties.Definition, properties[0].Name);
            Assert.AreEqual("xy-coordinates", properties[0].Value);
            Assert.AreEqual(KnownWaveProperties.StartCoordinateX, properties[1].Name);
            Assert.AreEqual(GetStringValue(coordinate1.X), properties[1].Value);
            Assert.AreEqual(KnownWaveProperties.EndCoordinateX, properties[2].Name);
            Assert.AreEqual(GetStringValue(coordinate2.X), properties[2].Value);
            Assert.AreEqual(KnownWaveProperties.StartCoordinateY, properties[3].Name);
            Assert.AreEqual(GetStringValue(coordinate1.Y), properties[3].Value);
            Assert.AreEqual(KnownWaveProperties.EndCoordinateY, properties[4].Name);
            Assert.AreEqual(GetStringValue(coordinate2.Y), properties[4].Value);
        }

        [Test]
        public void AddNewProperties_CategoryNull_ThrowsArgumentNullException()
        {
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var supportPoints = new List<SupportPoint>();

            // Act
            void Call() => MdwBoundaryCategoryGeometryExtender.AddNewProperties(null, boundaryContainer, supportPoints);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryCategory"));
        }

        [Test]
        public void AddNewProperties_BoundaryContainerNull_ThrowsArgumentNullException()
        {
            var delftIniCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            var supportPoints = new List<SupportPoint>();

            // Act
            void Call() => MdwBoundaryCategoryGeometryExtender.AddNewProperties(delftIniCategory, null, supportPoints);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryContainer"));
        }

        [Test]
        public void AddNewProperties_SupportPointsNull_ThrowsArgumentNullException()
        {
            var delftIniCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            var boundaryContainer = Substitute.For<IBoundaryContainer>();

            // Act
            void Call() => MdwBoundaryCategoryGeometryExtender.AddNewProperties(delftIniCategory, boundaryContainer, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPoints"));
        }

        private static string GetStringValue(double value)
        {
            return value.ToString("F7", CultureInfo.InvariantCulture);
        }
    }
}