﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using DelftTools.TestUtils;
using Deltares.Infrastructure.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
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
            IEnumerable<WaterFlowFMProperty> fileProperties = GetFileProperties().Where(p => !IsOutputFileProperty(p)).ToArray();
            fileProperties.ForEach(SetNotExistingFileReference);

            IEnumerable<string> expected = fileProperties.Select(GetNotExistingFileReferenceMessage).ToArray();

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.EqualTo(expected));
        }

        [Test]
        public void Validate_OutputFilePropertiesWithNotExistingFileReferences_LogsNoErrorMessages()
        {
            IEnumerable<WaterFlowFMProperty> fileProperties = GetFileProperties().Where(IsOutputFileProperty).ToArray();
            fileProperties.ForEach(SetNotExistingFileReference);

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.Empty);
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidFilePaths))]
        public void Validate_FilePropertiesWithInvalidCharactersInFileReference_LogsErrorMessages(string fileName)
        {
            IEnumerable<WaterFlowFMProperty> fileProperties = GetFileProperties().ToArray();
            fileProperties.ForEach(p => p.SetValueFromString(fileName));

            IEnumerable<string> expected = fileProperties.Select(GetInvalidCharactersInPathMessage).ToArray();

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                () => validator.Validate());

            Assert.That(messages, Is.EqualTo(expected));
        }

        [Test]
        public void Validate_FilePropertiesWithNotExistingFileReferences_ClearsPropertyValues()
        {
            IEnumerable<WaterFlowFMProperty> fileProperties = GetFileProperties().Where(p => !IsOutputFileProperty(p)).ToArray();
            fileProperties.ForEach(SetNotExistingFileReference);

            validator.Validate();

            IEnumerable<string> propertyValues = fileProperties.Select(p => p.GetValueAsString());

            Assert.That(propertyValues, Is.All.Empty);
        }
        
        [Test]
        public void Validate_OutputFilePropertiesWithNotExistingFileReferences_DoesNotClearPropertyValues()
        {
            IEnumerable<WaterFlowFMProperty> fileProperties = GetFileProperties().Where(IsOutputFileProperty).ToArray();
            fileProperties.ForEach(SetNotExistingFileReference);

            validator.Validate();

            IEnumerable<string> propertyValues = fileProperties.Select(p => p.GetValueAsString());

            Assert.That(propertyValues, Is.All.Not.Empty);
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidFilePaths))]
        public void Validate_FilePropertiesWithInvalidCharactersInFileReferences_ClearsPropertyValues(string fileName)
        {
            IEnumerable<WaterFlowFMProperty> fileProperties = GetFileProperties().ToArray();
            fileProperties.ForEach(p => p.SetValueFromString(fileName));

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
            string message = GetNotExistingFileReferenceMessage(propertyName, propertyValue, fileProperty.LineNumber);

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
            property.SetValueFromString(fileName);
            fileSystem.AddEmptyFile($"{mduDir}{fileName}");
        }

        private void SetMultipleFileReferencesInMduDir(WaterFlowFMProperty property)
        {
            string fileName1 = CreateFileName();
            string fileName2 = CreateFileName();

            property.SetValueFromString($"{fileName1} {fileName2}");

            fileSystem.AddEmptyFile($"{mduDir}{fileName1}");
            fileSystem.AddEmptyFile($"{mduDir}{fileName2}");
        }

        private void SetRelativeFileReference(WaterFlowFMProperty property)
        {
            var fileName = $@"..\data\{CreateFileName()}";

            property.SetValueFromString(fileName);
            fileSystem.AddEmptyFile($"{mduDir}{fileName}");
        }

        private void SetAbsoluteFileReference(WaterFlowFMProperty property)
        {
            var filePath = $@"c:\data\foo\{CreateFileName()}";

            property.SetValueFromString(filePath);
            fileSystem.AddEmptyFile(filePath);
        }

        private void SetNotExistingFileReference(WaterFlowFMProperty property)
        {
            property.SetValueFromString(CreateFileName());
        }

        private IEnumerable<WaterFlowFMProperty> GetFileProperties()
        {
            return modelDefinition.Properties.Where(MduFileHelper.IsFileValued);
        }

        private bool IsOutputFileProperty(WaterFlowFMProperty property)
        {
            string propertyName = property.PropertyDefinition.MduPropertyName;

            return propertyName.EqualsCaseInsensitive(KnownProperties.HisFile) ||
                   propertyName.EqualsCaseInsensitive(KnownProperties.MapFile);
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
            string propertyName = property.PropertyDefinition.MduPropertyName;
            string propertyValue = property.GetValueAsString();

            return string.Format(Common.Properties.Resources.File_reference_0_contains_invalid_characters_but_is_defined_in_1_, propertyValue, mduFilePath) + "\r\n" +
                   string.Format(Common.Properties.Resources.See_property_0_line_1_, propertyName, property.LineNumber) + " " + Common.Properties.Resources.Data_for_this_item_is_dropped;
        }

        private static string GetNotExistingFileReferenceMessage(WaterFlowFMProperty property)
        {
            return GetNotExistingFileReferenceMessage(property.PropertyDefinition.MduPropertyName, property.GetValueAsString(), property.LineNumber);
        }

        private static string GetNotExistingFileReferenceMessage(string propertyName, string propertyValue, int lineNumber)
        {
            string filePath = Path.Combine(mduDir, propertyValue);
            return string.Format(Common.Properties.Resources.File_at_location_0_does_not_exist_but_is_defined_in_1_, filePath, mduFilePath) + "\r\n" +
                   string.Format(Common.Properties.Resources.See_property_0_line_1_, propertyName, lineNumber) + " " + Common.Properties.Resources.Data_for_this_item_is_dropped;
        }
    }
}