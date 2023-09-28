using System.Collections.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructureFactoryValidatorTest
    {
        [Test]
        public void StructureMustHaveType()
        {
            var structureDataAccessObject = new StructureDAO(null);
            var idValue = "Test";
            structureDataAccessObject.AddProperty("id", typeof(string), idValue);
            structureDataAccessObject.AddProperty("type", typeof(string), "weir");

            Assert.AreEqual(string.Format("Structure '{0}' cannot have null as type.", idValue), StructureFactoryValidator.Validate(structureDataAccessObject));

            structureDataAccessObject = new StructureDAO("weir");
            structureDataAccessObject.AddProperty("id", typeof(string), idValue);
            Assert.AreEqual(string.Format("Structure '{0}' does not have a type specified.", idValue), StructureFactoryValidator.Validate(structureDataAccessObject));

            var emptyType = "";
            structureDataAccessObject = new StructureDAO(emptyType);
            structureDataAccessObject.AddProperty("id", typeof(string), idValue);
            structureDataAccessObject.AddProperty("type", typeof(string), emptyType);
            Assert.AreEqual(string.Format("Structure '{0}' has unsupported type ({1}) specified.", idValue, emptyType), StructureFactoryValidator.Validate(structureDataAccessObject));
        }

        [Test]
        public void StructureTypeMustBeConsistent()
        {
            var structureDataAccessObject = new StructureDAO("pump");
            structureDataAccessObject.AddProperty("id", typeof(string), "Test");
            structureDataAccessObject.AddProperty("type", typeof(string), "weir");

            Assert.AreEqual("Structure 'Test' has conflicting types: 'pump' and 'weir' are stated.",
                            StructureFactoryValidator.Validate(structureDataAccessObject));
        }

        [Test]
        public void StructureTypeMustBeSupported()
        {
            var structureDataAccessObject = new StructureDAO("test");
            structureDataAccessObject.AddProperty("id", typeof(string), "Test");
            structureDataAccessObject.AddProperty("type", typeof(string), "test");

            Assert.AreEqual("Structure 'Test' has unsupported type (test) specified.",
                            StructureFactoryValidator.Validate(structureDataAccessObject));
        }

        [Test]
        public void StructureMustHaveId()
        {
            var structureDataAccessObject = new StructureDAO("weir");
            structureDataAccessObject.AddProperty("type", typeof(string), "weir");
            Assert.AreEqual("Id of structure must be specified.", StructureFactoryValidator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("id", typeof(string), "");
            Assert.AreEqual("Id of structure must be specified.", StructureFactoryValidator.Validate(structureDataAccessObject));
        }

        [Test]
        public void StructureMustHaveValidGeometry()
        {
            var structureDataAccessObject = new StructureDAO("weir");
            structureDataAccessObject.AddProperty("type", typeof(string), "weir");
            structureDataAccessObject.AddProperty("id", typeof(string), "Test");

            Assert.AreEqual("Structure 'Test' must have geometry specified.", StructureFactoryValidator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("x", typeof(double), "5");
            Assert.AreEqual("Structure 'Test' has property 'x' specified, but 'y' is missing.", StructureFactoryValidator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("y", typeof(double), "5");
            Assert.AreEqual("", StructureFactoryValidator.Validate(structureDataAccessObject));

            structureDataAccessObject.Properties.RemoveAllWhere(p => p.PropertyDefinition.FilePropertyKey == "x");
            Assert.AreEqual("Structure 'Test' has property 'y' specified, but 'x' is missing.", StructureFactoryValidator.Validate(structureDataAccessObject));

            structureDataAccessObject.Properties.RemoveAllWhere(p => p.PropertyDefinition.FilePropertyKey == "y");
            structureDataAccessObject.AddProperty("polylinefile", typeof(string), "");
            Assert.AreEqual("Structure 'Test' does not have a filename specified for property 'polylinefile'.", StructureFactoryValidator.Validate(structureDataAccessObject));
        }

        [Test]
        public void StructureMustHaveOnlyOneTypeOfGeometryDefinition()
        {
            var structureDataAccessObject = new StructureDAO("weir");
            structureDataAccessObject.AddProperty("type", typeof(string), "weir");
            structureDataAccessObject.AddProperty("id", typeof(string), "Test");
            structureDataAccessObject.AddProperty("x", typeof(double), "5");
            structureDataAccessObject.AddProperty("y", typeof(double), "5");
            structureDataAccessObject.AddProperty("polylinefile", typeof(string), "testing.pli");

            Assert.AreEqual("Structure 'Test' cannot have point geometry and polyline geometry.", StructureFactoryValidator.Validate(structureDataAccessObject));
        }

        [Test]
        public void PumpWithConstantReductionFactorMustHaveFactor()
        {
            var structureDataAccessObject = new StructureDAO("pump");
            structureDataAccessObject.AddProperty("type", typeof(string), "pump");
            structureDataAccessObject.AddProperty("id", typeof(string), "Test");
            structureDataAccessObject.AddProperty("x", typeof(double), "5");
            structureDataAccessObject.AddProperty("y", typeof(double), "5");
            structureDataAccessObject.AddProperty("reduction_factor_no_levels", typeof(int), "1");

            Assert.AreEqual("Structure 'Test' with constant reduction factor does not have factor defined.", StructureFactoryValidator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("reduction_factor", typeof(IList<double>), "1");
            Assert.AreEqual("", StructureFactoryValidator.Validate(structureDataAccessObject));
        }

        [Test]
        public void PumpWithMultipleLevelsMustHaveHeadAndFactorDefined()
        {
            var structureDataAccessObject = new StructureDAO("pump");
            structureDataAccessObject.AddProperty("type", typeof(string), "pump");
            structureDataAccessObject.AddProperty("id", typeof(string), "Test");
            structureDataAccessObject.AddProperty("x", typeof(double), "5");
            structureDataAccessObject.AddProperty("y", typeof(double), "5");
            structureDataAccessObject.AddProperty("reduction_factor_no_levels", typeof(int), "3");

            Assert.AreEqual("Structure 'Test' with multiple reduction factors does not have reference levels defined.", StructureFactoryValidator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("head", typeof(IList<double>), "1 2 3");
            Assert.AreEqual("Structure 'Test' with multiple reduction factors does not have factors defined.", StructureFactoryValidator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("reduction_factor", typeof(IList<double>), "4 5 6");
            Assert.AreEqual("", StructureFactoryValidator.Validate(structureDataAccessObject));
        }

        [Test]
        public void ValidatingNullDoesNotThrow()
        {
            Assert.AreEqual("", StructureFactoryValidator.Validate(null));
        }
    }
}