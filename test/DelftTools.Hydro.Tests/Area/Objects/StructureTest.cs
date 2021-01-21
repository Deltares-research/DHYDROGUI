using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Area.Objects
{
    [TestFixture]
    public class StructureTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var structure = new Structure();

            // Assert
            Assert.That(structure, Is.InstanceOf<Hydro.Area.Objects.IStructure>());
            Assert.That(structure, Is.InstanceOf<INotifyCollectionChange>());
            Assert.That(structure, Is.InstanceOf<Unique<long>>());

            Assert.That(structure.Name, Is.EqualTo("Structure"));

            Assert.That(structure.Formula, Is.Null);
            Assert.That(structure.FormulaName, Is.Null);

            Assert.That(structure.CrestLevelTimeSeries, Is.Not.Null);
            Assert.That(structure.UseCrestLevelTimeSeries, Is.False);
            Assert.That(structure.IsDefaultGroup, Is.False);
            Assert.That(structure.CrestLevel, Is.EqualTo(0.0));
            Assert.That(structure.CrestWidth, Is.EqualTo(0.0));
        }

        [Test]
        public void Clone_CreatesDeepCopy()
        {
            // Setup
            var structure = new Structure()
            {
                GroupName = "Groupie!",
                Geometry = Substitute.For<IGeometry>(),
                Attributes = Substitute.For<IFeatureAttributeCollection>(),
                Name = "Struclet",
                IsDefaultGroup = true,
                Formula = Substitute.For<IWeirFormula>(),
                CrestWidth = 25.32,
                CrestLevel = 125.32,
                UseCrestLevelTimeSeries = true,
            };

            var clonedGeometry = Substitute.For<IGeometry>();
            structure.Geometry.Clone().Returns(clonedGeometry);

            var clonedAttributes = Substitute.For<IFeatureAttributeCollection>();
            structure.Attributes.Clone().Returns(clonedAttributes);

            var clonedFormula = Substitute.For<IWeirFormula>();
            structure.Formula.Clone().Returns(clonedFormula);

            // Call
            object clonedStructureObject = structure.Clone();

            // Assert
            var clonedStructure = clonedStructureObject as Structure;
            Assert.That(clonedStructure, Is.Not.Null);

            Assert.That(clonedStructure.GroupName, Is.EqualTo(structure.GroupName));
            Assert.That(clonedStructure.Name, Is.EqualTo(structure.Name));
            Assert.That(clonedStructure.IsDefaultGroup, Is.EqualTo(structure.IsDefaultGroup));

            Assert.That(clonedStructure.CrestWidth, Is.EqualTo(structure.CrestWidth));
            Assert.That(clonedStructure.CrestLevel, Is.EqualTo(structure.CrestLevel));
            Assert.That(clonedStructure.UseCrestLevelTimeSeries, Is.EqualTo(structure.UseCrestLevelTimeSeries));

            Assert.That(clonedStructure.Geometry, Is.SameAs(clonedGeometry));
            Assert.That(clonedStructure.Attributes, Is.SameAs(clonedAttributes));
            Assert.That(clonedStructure.Formula, Is.SameAs(clonedFormula));

            Assert.That(clonedStructure.CrestLevelTimeSeries, Is.Not.Null);
            Assert.That(clonedStructure.CrestLevelTimeSeries, Is.Not.SameAs(structure.CrestLevelTimeSeries));
        }
    }
}