using System.Collections.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructureFactoryValidatorTest
    {
        [Test]
        public void StructureMustHaveType()
        {
            var structure = new Structure2D(null);
            var idValue = "Test";
            structure.AddProperty("id", typeof(string), idValue);
            structure.AddProperty("type", typeof(string), "weir");

            Assert.AreEqual(string.Format("Structure '{0}' cannot have null as type.", idValue), StructureFactoryValidator.Validate(structure));

            structure = new Structure2D("weir");
            structure.AddProperty("id", typeof(string), idValue);
            Assert.AreEqual(string.Format("Structure '{0}' does not have a type specified.", idValue), StructureFactoryValidator.Validate(structure));

            var emptyType = "";
            structure = new Structure2D(emptyType);
            structure.AddProperty("id", typeof(string), idValue);
            structure.AddProperty("type", typeof(string), emptyType);
            Assert.AreEqual(string.Format("Structure '{0}' has unsupported type ({1}) specified.", idValue, emptyType), StructureFactoryValidator.Validate(structure));
        }

        [Test]
        public void StructureTypeMustBeConsistent()
        {
            var structure = new Structure2D("pump");
            structure.AddProperty("id", typeof(string), "Test");
            structure.AddProperty("type", typeof(string), "weir");

            Assert.AreEqual("Structure 'Test' has conflicting types: 'pump' and 'weir' are stated.", 
                StructureFactoryValidator.Validate(structure));
        }

        [Test]
        public void StructureTypeMustBeSupported()
        {
            var structure = new Structure2D("test");
            structure.AddProperty("id", typeof(string), "Test");
            structure.AddProperty("type", typeof(string), "test");

            Assert.AreEqual("Structure 'Test' has unsupported type (test) specified.",
                StructureFactoryValidator.Validate(structure));
        }

        [Test]
        public void StructureMustHaveId()
        {
            var structure = new Structure2D("weir");
            structure.AddProperty("type", typeof(string), "weir");
            Assert.AreEqual("Id of structure must be specified.", StructureFactoryValidator.Validate(structure));

            structure.AddProperty("id", typeof(string), "");
            Assert.AreEqual("Id of structure must be specified.", StructureFactoryValidator.Validate(structure));
        }

        [Test]
        public void StructureMustHaveValidGeometry()
        {
            var structure = new Structure2D("weir");
            structure.AddProperty("type", typeof(string), "weir");
            structure.AddProperty("id", typeof(string), "Test");

            Assert.AreEqual("Structure 'Test' must have geometry specified.", StructureFactoryValidator.Validate(structure));

            structure.AddProperty(StructureRegion.XCoordinates.Key, typeof(double), "5");
            Assert.AreEqual("Structure 'Test' has property 'x' specified, but 'y' is missing.", StructureFactoryValidator.Validate(structure));

            structure.AddProperty(StructureRegion.YCoordinates.Key, typeof(double), "5");
            Assert.AreEqual("", StructureFactoryValidator.Validate(structure));

            structure.Properties.RemoveAllWhere(p => p.PropertyDefinition.FilePropertyKey == StructureRegion.XCoordinates.Key);
            Assert.AreEqual("Structure 'Test' has property 'y' specified, but 'x' is missing.", StructureFactoryValidator.Validate(structure));

            structure.Properties.RemoveAllWhere(p => p.PropertyDefinition.FilePropertyKey == StructureRegion.YCoordinates.Key);
            structure.AddProperty("polylinefile", typeof(string), "");
            Assert.AreEqual("Structure 'Test' does not have a filename specified for property 'polylinefile'.", StructureFactoryValidator.Validate(structure));
        }

        [Test]
        public void StructureMustHaveOnlyOneTypeOfGeometryDefinition()
        {
            var structure = new Structure2D("weir");
            structure.AddProperty("type", typeof(string), "weir");
            structure.AddProperty("id", typeof(string), "Test");
            structure.AddProperty(StructureRegion.XCoordinates.Key, typeof(double), "5");
            structure.AddProperty(StructureRegion.YCoordinates.Key, typeof(double), "5");
            structure.AddProperty("polylinefile", typeof(string), "testing.pli");

            Assert.AreEqual("Structure 'Test' cannot have point geometry and polyline geometry.", StructureFactoryValidator.Validate(structure));
        }

        [Test]
        public void PumpWithConstantReductionFactorMustHaveFactor()
        {
            var structure = new Structure2D("pump");
            structure.AddProperty("type", typeof(string), "pump");
            structure.AddProperty("id", typeof(string), "Test");
            structure.AddProperty(StructureRegion.XCoordinates.Key, typeof(double), "5");
            structure.AddProperty(StructureRegion.YCoordinates.Key, typeof(double), "5");
            structure.AddProperty("reduction_factor_no_levels", typeof(int), "1");

            Assert.AreEqual("Structure 'Test' with constant reduction factor does not have factor defined.", StructureFactoryValidator.Validate(structure));

            structure.AddProperty("reduction_factor", typeof(IList<double>), "1");
            Assert.AreEqual("", StructureFactoryValidator.Validate(structure));
        }

        [Test]
        public void PumpWithMultipleLevelsMustHaveHeadAndFactorDefined()
        {
            var structure = new Structure2D("pump");
            structure.AddProperty("type", typeof(string), "pump");
            structure.AddProperty("id", typeof(string), "Test");
            structure.AddProperty(StructureRegion.XCoordinates.Key, typeof(double), "5");
            structure.AddProperty(StructureRegion.YCoordinates.Key, typeof(double), "5");
            structure.AddProperty("reduction_factor_no_levels", typeof(int), "3");

            Assert.AreEqual("Structure 'Test' with multiple reduction factors does not have reference levels defined.", StructureFactoryValidator.Validate(structure));

            structure.AddProperty("head", typeof(IList<double>), "1 2 3");
            Assert.AreEqual("Structure 'Test' with multiple reduction factors does not have factors defined.", StructureFactoryValidator.Validate(structure));

            structure.AddProperty("reduction_factor", typeof(IList<double>), "4 5 6");
            Assert.AreEqual("", StructureFactoryValidator.Validate(structure));
        }

        [Test]
        public void ValidatingNullDoesNotThrow()
        {
            Assert.AreEqual("", StructureFactoryValidator.Validate(null));
        }
    }
}