using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Area.Objects.StructureObjects
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
            Assert.That(structure, Is.InstanceOf<IStructure>());
            Assert.That(structure, Is.InstanceOf<INotifyCollectionChange>());
            Assert.That(structure, Is.InstanceOf<Unique<long>>());

            Assert.That(structure.Name, Is.EqualTo("Structure"));

            Assert.That(structure.Formula, Is.InstanceOf<SimpleWeirFormula>());
            Assert.That(structure.FormulaName, Is.EqualTo("Simple Weir"));

            Assert.That(structure.CrestLevelTimeSeries, Is.Not.Null);
            Assert.That(structure.UseCrestLevelTimeSeries, Is.False);
            Assert.That(structure.IsDefaultGroup, Is.False);
            Assert.That(structure.CrestLevel, Is.EqualTo(0.0));
            Assert.That(structure.CrestWidth, Is.EqualTo(double.NaN));
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
                Formula = Substitute.For<IStructureFormula>(),
                CrestWidth = 25.32,
                CrestLevel = 125.32,
                UseCrestLevelTimeSeries = true,
            };

            var clonedGeometry = Substitute.For<IGeometry>();
            structure.Geometry.Clone().Returns(clonedGeometry);

            var clonedAttributes = Substitute.For<IFeatureAttributeCollection>();
            structure.Attributes.Clone().Returns(clonedAttributes);

            var clonedFormula = Substitute.For<IStructureFormula>();
            structure.Formula.Clone().Returns(clonedFormula);

            // Call
            object clonedStructureObject = structure.Clone();

            // Assert
            var clonedStructure = clonedStructureObject as Structure;
            Assert.That(clonedStructure, Is.Not.Null);
            Assert.That(clonedStructureObject, Is.Not.SameAs(structure));

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

        [Test]
        public void Clone_PropertyNull_DoesNotThrowException()
        {
            // Setup
            var structure = new Structure()
            {
                Geometry = null,
                Attributes = null,
                Formula = null,
            };

            // Call
            var clonedStructure = (IStructure) structure.Clone();

            // Assert
            Assert.That(clonedStructure, Is.Not.Null);
            Assert.That(clonedStructure, Is.Not.SameAs(structure));

            Assert.That(clonedStructure.Attributes, Is.Null);
            Assert.That(clonedStructure.Geometry, Is.Null);
            Assert.That(clonedStructure.Formula, Is.Null);
        }

        [Test]
        [TestCase("SomeName", "SomeName")]
        [TestCase( null, "Unnamed Structure")]
        public void ToString_ExpectedResults(string inputName, string expectedResult)
        {
            // Setup
            var structure = new Structure() {Name = inputName};

            // Call
            var result = structure.ToString();

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAStructureWithAGeneralStructure_WhenCrestLevelIsSet_ThenTheGeneralStructurePropertyAndStructurePropertyAreUpdatedCorrectly()
        {
            // Setup
            var formula = new GeneralStructureFormula();
            var structure = new Structure {Formula = formula};
            const double crestLevel = 123.456;

            // Call
            structure.CrestLevel = crestLevel;

            // Assert
            Assert.That(structure.CrestLevel, Is.EqualTo(crestLevel));
            Assert.That(formula.CrestLevel, Is.EqualTo(crestLevel));

            // Set formula to null to ensure we query the value on the Structure.
            structure.Formula = null;
            Assert.That(structure.CrestLevel, Is.EqualTo(crestLevel));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAStructureWithAGeneralStructure_WhenCrestWidthIsSet_ThenTheGeneralStructurePropertyAndStructurePropertyAreUpdatedCorrectly()
        {
            // Setup
            var formula = new GeneralStructureFormula();
            var structure = new Structure {Formula = formula};
            const double crestWidth = 123.456;

            // Call
            structure.CrestWidth = crestWidth;

            // Assert
            Assert.That(structure.CrestWidth, Is.EqualTo(crestWidth));
            Assert.That(formula.CrestWidth, Is.EqualTo(crestWidth));

            // Set formula to null to ensure we query the value on the Structure.
            structure.Formula = null;
            Assert.That(structure.CrestWidth, Is.EqualTo(crestWidth));
        }
    }
}