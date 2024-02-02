using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Extensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class MduFileValidatorTest
    {
        private const string mduDir = @"c:\models\foo\";
        private const string mduFilePath = mduDir + @"fm.mdu";

        private MockFileSystem fileSystem;
        private WaterFlowFMModelDefinition modelDefinition;
        private MduFileValidator validator;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            modelDefinition = new WaterFlowFMModelDefinition();
            validator = new MduFileValidator(mduFilePath, modelDefinition) { FileSystem = fileSystem };
        }

        [Test]
        public void Validate_FilePropertiesAreEmpty_LogsNoMessages()
        {
            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.Empty);
        }

        [Test]
        [TestCase(@"/MyObservationPoints_obs.xyn")]
        [TestCase(@"\MyObservationPoints_obs.xyn")]
        [TestCase(@"\/MyObservationPoints_obs.xyn")]
        public void Validate_FilePropertyWithLeadingSlashFilePath_CleansFilePath(string path)
        {
            WaterFlowFMProperty fileProperty = GetFileProperties().First();
            SetFileReferenceInMduDir(fileProperty, path);

            validator.Validate();

            string actualPath = fileProperty.GetValueAsString();

            Assert.That(actualPath, Is.EqualTo("MyObservationPoints_obs.xyn"));
        }

        [Test]
        public void Validate_FilePropertyWithFileReferenceInMduDir_LogsNoMessages()
        {
            WaterFlowFMProperty fileProperty = GetFileProperties().First();
            SetFileReferenceInMduDir(fileProperty);

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public void Validate_FilePropertyWithMultipleFileReferencesInMduDir_LogsNoMessages()
        {
            WaterFlowFMProperty fileProperty = GetFileProperties().First(p => p.PropertyDefinition.IsMultipleFile);
            SetMultipleFileReferencesInMduDir(fileProperty);

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public void Validate_FilePropertyWithRelativeFileReference_LogsNoMessages()
        {
            WaterFlowFMProperty fileProperty = GetFileProperties().First();
            SetRelativeFileReference(fileProperty);

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public void Validate_FilePropertyWithAbsoluteFileReference_LogsNoMessages()
        {
            WaterFlowFMProperty fileProperty = GetFileProperties().Last();
            SetAbsoluteFileReference(fileProperty);

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public void Validate_FilePropertyWithWhiteSpaceInPath_LogsNoMessages()
        {
            WaterFlowFMProperty fileProperty = GetFileProperties().First(p => !p.PropertyDefinition.IsMultipleFile);
            SetFileReferenceInMduDir(fileProperty, "file with spaces.txt");

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public void Validate_FilePropertiesWithNotExistingFileReferences_LogsErrorMessages()
        {
            IEnumerable<WaterFlowFMProperty> fileProperties = GetFileProperties().ToArray();
            fileProperties.ForEach(SetNotExistingFileReference);

            IEnumerable<string> expected = fileProperties.Select(GetNotExistingFileReferenceMessage).ToArray();

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.EqualTo(expected));
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidFilePaths))]
        public void Validate_FilePropertiesWithInvalidCharactersInFileReference_LogsErrorMessages(string fileName)
        {
            IEnumerable<WaterFlowFMProperty> fileProperties = GetFileProperties().ToArray();
            fileProperties.ForEach(p => p.SetValueAsString(fileName));

            IEnumerable<string> expected = fileProperties.Select(GetInvalidCharactersInPathMessage).ToArray();

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.EqualTo(expected));
        }

        [Test]
        public void Validate_FilePropertiesWithNotExistingFileReferences_ClearsPropertyValues()
        {
            IEnumerable<WaterFlowFMProperty> fileProperties = GetFileProperties().ToArray();
            fileProperties.ForEach(SetNotExistingFileReference);

            validator.Validate();

            IEnumerable<string> propertyValues = fileProperties.Select(p => p.GetValueAsString());

            Assert.That(propertyValues, Is.All.Empty);
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidFilePaths))]
        public void Validate_FilePropertiesWithInvalidCharactersInFileReferences_ClearsPropertyValues(string fileName)
        {
            IEnumerable<WaterFlowFMProperty> fileProperties = GetFileProperties().ToArray();
            fileProperties.ForEach(p => p.SetValueAsString(fileName));

            validator.Validate();

            IEnumerable<string> propertyValues = fileProperties.Select(p => p.GetValueAsString());

            Assert.That(propertyValues, Is.All.Empty);
        }

        [Test]
        public void Validate_FilePropertyWithExistingAndNotExistingFileReferences_LogsErrorMessage()
        {
            WaterFlowFMProperty fileProperty = GetFileProperties().First(p => p.PropertyDefinition.IsMultipleFile);
            SetMultipleFileReferencesInMduDir(fileProperty);

            string removedFile = fileSystem.AllFiles.Last();
            fileSystem.RemoveFile(removedFile);

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            string propertyName = fileProperty.PropertyDefinition.MduPropertyName;
            string propertyValue = fileSystem.Path.GetFileName(removedFile);
            string message = GetNotExistingFileReferenceMessage(propertyName, propertyValue);

            Assert.That(messages, Is.EqualTo(new[] { message }));
        }

        [Test]
        public void Validate_FilePropertyWithExistingAndNotExistingFileReference_UpdatesPropertyValues()
        {
            WaterFlowFMProperty fileProperty = GetFileProperties().First(p => p.PropertyDefinition.IsMultipleFile);
            SetMultipleFileReferencesInMduDir(fileProperty);

            string removedFile = fileSystem.AllFiles.Last();
            fileSystem.RemoveFile(removedFile);

            validator.Validate();

            string propertyValue = fileProperty.GetValueAsString();
            string remainingFile = fileSystem.Path.GetFileName(fileSystem.AllFiles.First());

            Assert.That(propertyValue, Is.EqualTo(remainingFile));
        }

        private void SetFileReferenceInMduDir(WaterFlowFMProperty property)
        {
            SetFileReferenceInMduDir(property, CreateFileName());
        }

        private void SetFileReferenceInMduDir(WaterFlowFMProperty property, string fileName)
        {
            property.SetValueAsString(fileName);
            fileSystem.AddEmptyFile($"{mduDir}{fileName}");
        }

        private void SetMultipleFileReferencesInMduDir(WaterFlowFMProperty property)
        {
            string fileName1 = CreateFileName();
            string fileName2 = CreateFileName();

            property.SetValueAsString($"{fileName1} {fileName2}");

            fileSystem.AddEmptyFile($"{mduDir}{fileName1}");
            fileSystem.AddEmptyFile($"{mduDir}{fileName2}");
        }

        private void SetRelativeFileReference(WaterFlowFMProperty property)
        {
            var fileName = $@"..\data\{CreateFileName()}";

            property.SetValueAsString(fileName);
            fileSystem.AddEmptyFile($"{mduDir}{fileName}");
        }

        private void SetAbsoluteFileReference(WaterFlowFMProperty property)
        {
            var filePath = $@"c:\data\foo\{CreateFileName()}";

            property.SetValueAsString(filePath);
            fileSystem.AddEmptyFile(filePath);
        }

        private void SetNotExistingFileReference(WaterFlowFMProperty property)
        {
            property.SetValueAsString(CreateFileName());
        }

        private IEnumerable<WaterFlowFMProperty> GetFileProperties()
        {
            return modelDefinition.Properties.Where(MduFileHelper.IsFileValued);
        }

        private string CreateFileName()
        {
            return $"{Guid.NewGuid().ToString()}.txt";
        }

        private static IEnumerable<string> GetInvalidFilePaths()
        {
            return new[]
            {
                "invalid|file.txt",
                "invalid<file.txt",
                "invalid?.txt",
                "*.txt"
            };
        }

        private string GetInvalidCharactersInPathMessage(WaterFlowFMProperty property)
        {
            string message = Resources.MduFileReferencePathContainsInvalidCharacters;
            string propertyName = property.PropertyDefinition.MduPropertyName;
            string propertyValue = property.GetValueAsString();
            string modelName = modelDefinition.ModelName;

            return string.Format(message, propertyValue, mduFilePath, propertyName, modelName);
        }

        private string GetNotExistingFileReferenceMessage(WaterFlowFMProperty property)
        {
            return GetNotExistingFileReferenceMessage(property.PropertyDefinition.MduPropertyName, property.GetValueAsString());
        }

        private string GetNotExistingFileReferenceMessage(string propertyName, string propertyValue)
        {
            string message = Resources.MduFileReferenceDoesNotExist;
            string modelName = modelDefinition.ModelName;

            return string.Format(message, propertyValue, mduFilePath, propertyName, modelName);
        }
    }
}