using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using DelftTools.Utils.Collections;
using Deltares.Infrastructure.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructureFileValidatorTest
    {
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
        }
        [Test]
        public void StructureMustHaveType()
        {
            var structureDataAccessObject = new StructureDAO(null);
            var idValue = "Test";
            structureDataAccessObject.AddProperty("id", typeof(string), idValue);
            structureDataAccessObject.AddProperty("type", typeof(string), "weir");

            StructureFileValidator validator = CreateValidator();

            Assert.AreEqual(string.Format("Structure '{0}' cannot have null as type.", idValue), validator.Validate(structureDataAccessObject));

            structureDataAccessObject = new StructureDAO("weir");
            structureDataAccessObject.AddProperty("id", typeof(string), idValue);
            Assert.AreEqual(string.Format("Structure '{0}' does not have a type specified.", idValue), validator.Validate(structureDataAccessObject));

            var emptyType = "";
            structureDataAccessObject = new StructureDAO(emptyType);
            structureDataAccessObject.AddProperty("id", typeof(string), idValue);
            structureDataAccessObject.AddProperty("type", typeof(string), emptyType);
            Assert.AreEqual(string.Format("Structure '{0}' has unsupported type ({1}) specified.", idValue, emptyType), validator.Validate(structureDataAccessObject));
        }

        [Test]
        public void StructureTypeMustBeConsistent()
        {
            var structureDataAccessObject = new StructureDAO("pump");
            structureDataAccessObject.AddProperty("id", typeof(string), "Test");
            structureDataAccessObject.AddProperty("type", typeof(string), "weir");

            StructureFileValidator validator = CreateValidator();
            
            Assert.AreEqual("Structure 'Test' has conflicting types: 'pump' and 'weir' are stated.",
                            validator.Validate(structureDataAccessObject));
        }

        [Test]
        public void StructureTypeMustBeSupported()
        {
            var structureDataAccessObject = new StructureDAO("test");
            structureDataAccessObject.AddProperty("id", typeof(string), "Test");
            structureDataAccessObject.AddProperty("type", typeof(string), "test");

            StructureFileValidator validator = CreateValidator();
            
            Assert.AreEqual("Structure 'Test' has unsupported type (test) specified.",
                            validator.Validate(structureDataAccessObject));
        }

        [Test]
        public void StructureMustHaveId()
        {
            StructureFileValidator validator = CreateValidator();
            
            var structureDataAccessObject = new StructureDAO("weir");
            structureDataAccessObject.AddProperty("type", typeof(string), "weir");
            Assert.AreEqual("Id of structure must be specified.", validator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("id", typeof(string), "");
            Assert.AreEqual("Id of structure must be specified.", validator.Validate(structureDataAccessObject));
        }

        [Test]
        public void StructureMustHaveValidGeometry()
        {
            var structureDataAccessObject = new StructureDAO("weir");
            structureDataAccessObject.AddProperty("type", typeof(string), "weir");
            structureDataAccessObject.AddProperty("id", typeof(string), "Test");

            StructureFileValidator validator = CreateValidator();

            Assert.AreEqual("Structure 'Test' must have geometry specified.", validator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("x", typeof(double), "5");
            Assert.AreEqual("Structure 'Test' has property 'x' specified, but 'y' is missing.", validator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("y", typeof(double), "5");
            Assert.AreEqual("", validator.Validate(structureDataAccessObject));

            structureDataAccessObject.Properties.RemoveAllWhere(p => p.PropertyDefinition.FilePropertyKey == "x");
            Assert.AreEqual("Structure 'Test' has property 'y' specified, but 'x' is missing.", validator.Validate(structureDataAccessObject));

            structureDataAccessObject.Properties.RemoveAllWhere(p => p.PropertyDefinition.FilePropertyKey == "y");
            structureDataAccessObject.AddProperty("polylinefile", typeof(string), "");
            Assert.AreEqual("Structure 'Test' does not have a filename specified for property 'polylinefile'.", validator.Validate(structureDataAccessObject));
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

            StructureFileValidator validator = CreateValidator();

            Assert.AreEqual("Structure 'Test' cannot have point geometry and polyline geometry.", validator.Validate(structureDataAccessObject));
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

            StructureFileValidator validator = CreateValidator();

            Assert.AreEqual("Structure 'Test' with constant reduction factor does not have factor defined.", validator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("reduction_factor", typeof(IList<double>), "1");
            Assert.AreEqual("", validator.Validate(structureDataAccessObject));
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

            StructureFileValidator validator = CreateValidator();

            Assert.AreEqual("Structure 'Test' with multiple reduction factors does not have reference levels defined.", validator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("head", typeof(IList<double>), "1 2 3");
            Assert.AreEqual("Structure 'Test' with multiple reduction factors does not have factors defined.", validator.Validate(structureDataAccessObject));

            structureDataAccessObject.AddProperty("reduction_factor", typeof(IList<double>), "4 5 6");
            Assert.AreEqual("", validator.Validate(structureDataAccessObject));
        }

        [Test]
        public void Validate_ValidStructure_ReturnsEmptyString()
        {
            const string structureFilePath = "C:\\structures.ini";
            var pliFile = "Pump01.pli";
            var timFile = "Pump01_capacity.tim";

            fileSystem.AddEmptyFile(fileSystem.GetAbsolutePath(structureFilePath, pliFile));
            fileSystem.AddEmptyFile(fileSystem.GetAbsolutePath(structureFilePath, timFile));

            StructureFileValidator validator = CreateValidator(structureFilePath);

            StructureDAO structureDao = CreatePump(pliFile, timFile);

            string result = validator.Validate(structureDao);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Validate_PolyLineFileWithInvalidCharacters_ReturnsError()
        {
            const string structureFilePath = "C:\\structures.ini";
            var pliFile = "Pump01?.pli";
            var timFile = "Pump01_capacity.tim";

            fileSystem.AddEmptyFile(fileSystem.GetAbsolutePath(structureFilePath, timFile));

            StructureFileValidator validator = CreateValidator(structureFilePath);

            StructureDAO structureDao = CreatePump(pliFile, timFile);

            string result = validator.Validate(structureDao);
            Assert.That(result, Is.EqualTo(GetExpectedMessageInvalidCharacters(pliFile, structureFilePath, "polylineFile", 3)));
        }

        [Test]
        public void Validate_MissingPolyLineFile_ReturnsError()
        {
            const string structureFilePath = "C:\\structures.ini";
            var pliFile = "Pump01.pli";
            var timFile = "Pump01_capacity.tim";

            fileSystem.AddEmptyFile(fileSystem.GetAbsolutePath(structureFilePath, timFile));

            StructureFileValidator validator = CreateValidator(structureFilePath);

            StructureDAO structureDao = CreatePump(pliFile, timFile);

            string result = validator.Validate(structureDao);
            string filePath = fileSystem.GetAbsolutePath(structureFilePath, pliFile);
            Assert.That(result, Is.EqualTo(GetExpectedMessageMissingFile(filePath, structureFilePath, "polylineFile", 3)));
        }

        [Test]
        public void Validate_MissingTimFile_ReturnsError()
        {
            const string structureFilePath = "C:\\structures.ini";
            var pliFile = "Pump01.pli";
            var timFile = "Pump01_capacity.tim";

            fileSystem.AddEmptyFile(fileSystem.GetAbsolutePath(structureFilePath, pliFile));

            StructureFileValidator validator = CreateValidator(structureFilePath);

            StructureDAO structureDao = CreatePump(pliFile, timFile);

            string result = validator.Validate(structureDao);
            string filePath = fileSystem.GetAbsolutePath(structureFilePath, timFile);
            Assert.That(result, Is.EqualTo(GetExpectedMessageMissingFile(filePath, structureFilePath, "capacity", 4)));
        }
        

        [Test]
        public void ValidatingNullDoesNotThrow()
        {
            StructureFileValidator validator = CreateValidator();
            Assert.AreEqual("", validator.Validate(null));
        }

        private StructureFileValidator CreateValidator(string structureFilePath = "structures.ini")
        {
            return new StructureFileValidator(structureFilePath, structureFilePath, fileSystem);
        }

        private static StructureDAO CreatePump(string pliFile, string timFile)
        {
            var structureDao = new StructureDAO("pump");
            structureDao.AddProperty("type", typeof(string), "pump");
            structureDao.AddProperty("id", typeof(string), "Pump01");
            structureDao.AddProperty("polylineFile", typeof(string), pliFile);
            structureDao.AddProperty("capacity", typeof(Steerable), timFile);

            var lineNumber = 1;
            structureDao.Properties.ForEach(p =>
            {
                p.LineNumber = lineNumber;
                lineNumber++;
            });

            return structureDao;
        }

        private static string GetExpectedMessageInvalidCharacters(string fileName, string parentFilePath, string propertyName, int lineNumber)
        {
            return string.Format(Resources.File_reference_0_contains_invalid_characters_but_is_defined_in_1_, fileName, parentFilePath) + Environment.NewLine +
                   string.Format(Resources.See_property_0_line_1_, propertyName, lineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private static string GetExpectedMessageMissingFile(string filePath, string parentFilePath, string propertyName, int lineNumber)
        {
            return string.Format(Resources.File_at_location_0_does_not_exist_but_is_defined_in_1_, filePath, parentFilePath) + "\r\n" +
                   string.Format(Resources.See_property_0_line_1_, propertyName, lineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }
    }
}