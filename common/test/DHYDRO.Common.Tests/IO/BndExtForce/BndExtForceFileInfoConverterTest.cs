using Deltares.Infrastructure.IO.Ini;
using DHYDRO.Common.IO.BndExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceFileInfoConverterTest
    {
        [Test]
        public void ToFileInfo_IniSectionWithFileVersionAndFileType_ReturnsCorrectFileInfo()
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.General);
            section.AddProperty(BndExtForceFileConstants.Keys.FileVersion, "1.0");
            section.AddProperty(BndExtForceFileConstants.Keys.FileType, "extForce");

            BndExtForceFileInfo fileInfo = section.ToFileInfo();

            Assert.That(fileInfo, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(fileInfo.FileVersion, Is.EqualTo("1.0"));
                Assert.That(fileInfo.FileType, Is.EqualTo("extForce"));
            });
        }

        [Test]
        public void ToFileInfo_IniSectionWithoutFileVersionAndFileType_ReturnsFileInfoWithNullValues()
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.General);

            BndExtForceFileInfo fileInfo = section.ToFileInfo();

            Assert.That(fileInfo, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(fileInfo.FileVersion, Is.Null);
                Assert.That(fileInfo.FileType, Is.Null);
            });
        }

        [Test]
        public void ToIniSection_FileInfoWithFileVersionAndFileType_ReturnsCorrectIniSection()
        {
            var fileInfo = new BndExtForceFileInfo
            {
                FileVersion = "2.01",
                FileType = "extForce"
            };

            var section = fileInfo.ToIniSection();

            Assert.That(section, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.FileVersion), Is.EqualTo("2.01"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.FileType), Is.EqualTo("extForce"));
            });
        }

        [Test]
        public void ToIniSection_FileInfoWithNullFileVersionAndFileType_ReturnsIniSectionWithEmptyValues()
        {
            var fileInfo = new BndExtForceFileInfo
            {
                FileVersion = null,
                FileType = null
            };

            var section = fileInfo.ToIniSection();

            Assert.That(section, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.FileVersion), Is.Empty);
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.FileType), Is.Empty);
            });
        }
    }
}