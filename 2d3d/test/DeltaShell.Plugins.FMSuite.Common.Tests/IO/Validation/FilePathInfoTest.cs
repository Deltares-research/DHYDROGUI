using System;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.Validation
{
    [TestFixture]
    public class FilePathInfoTest
    {
        private const string fileReference = "some_file_reference";
        private const string propertyName = "some_property_name";

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_FileReferenceIsNullOrWhiteSpace_ThrowsArgumentException(string arg)
        {
            Assert.That(() => _ = new FilePathInfo(arg, propertyName, 0), Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_PropertyNameIsNullOrWhiteSpace_ThrowsArgumentException(string arg)
        {
            Assert.That(() => _ = new FilePathInfo(fileReference, arg, 1), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_FileReferenceIsNullOrWhiteSpace_ThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => _ = new FilePathInfo(fileReference, propertyName, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_InitializesInstance()
        {
            var info = new FilePathInfo(fileReference, propertyName, 3);

            Assert.That(info.FileReference, Is.EqualTo(fileReference));
            Assert.That(info.PropertyName, Is.EqualTo(propertyName));
            Assert.That(info.LineNumber, Is.EqualTo(3));
        }
    }
}