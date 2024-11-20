using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Validation;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.Validation
{
    [TestFixture]
    public class FilePathCharactersValidatorTest
    {
        private const string parentFilePath = "some_parent_file.txt";

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ParentFilePathIsNullOrEmpty_ThrowsArgumentException(string invalidParentFilePath)
        {
            Assert.That(() => _ = new FilePathCharactersValidator(invalidParentFilePath), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new FilePathCharactersValidator(parentFilePath, null), Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_WithFileWithOnlyValidCharacters_ReturnsSuccess()
        {
            FilePathCharactersValidator validator = CreateValidator();

            const string fileReference = "some_file_name.txt";
            FilePathInfo filePathInfo = CreateFilePathInfo(fileReference);

            ValidationResult result = validator.Validate(filePathInfo);

            Assert.That(result.Valid, Is.True);
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidChars))]
        public void Validate_WithFileWithInvalidCharacters_ReturnsFail(string fileReference)
        {
            FilePathCharactersValidator validator = CreateValidator();

            FilePathInfo filePathInfo = CreateFilePathInfo(fileReference);

            ValidationResult result = validator.Validate(filePathInfo);

            Assert.That(result.Valid, Is.False);
            Assert.That(result.Message, Is.EqualTo(GetExpectedMessage(filePathInfo)));
        }

        private static IEnumerable<string> GetInvalidChars()
        {
            IEnumerable<char> invalidChars = Path.GetInvalidPathChars().Concat(new[] { '*', '?' });
            foreach (char invalidChar in invalidChars)
            {
                yield return $"file{invalidChar}.txt";
            }
        }

        private static string GetExpectedMessage(FilePathInfo filePathInfo)
        {
            return string.Format(Resources.File_reference_0_contains_invalid_characters_but_is_defined_in_1_, filePathInfo.FileReference, parentFilePath) + Environment.NewLine +
                   string.Format(Resources.See_property_0_line_1_, filePathInfo.PropertyName, filePathInfo.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;
        }

        private static FilePathCharactersValidator CreateValidator()
        {
            return new FilePathCharactersValidator(parentFilePath, new MockFileSystem());
        }

        private static FilePathInfo CreateFilePathInfo(string fileReference)
        {
            return new FilePathInfo(fileReference, "some_property_name", 3);
        }
    }
}