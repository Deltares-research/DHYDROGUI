using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class IniFileWriterTest
    {
        private string testDirectory;
        private string testFilePath;

        [SetUp]
        public void Setup()
        {
            testDirectory = FileUtils.CreateTempDirectory();
            testFilePath = Path.Combine(testDirectory, "myFile.ini");
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(testDirectory);
        }

        [Test]
        public void GivenIniSections_WhenAppendingToIniFile_ThenFileWriterShouldAppend()
        {
            var iniSection = new IniSection("mySection");
            iniSection.AddProperty("haha", "lala");
            var iniSections = new List<IniSection> { iniSection };

            var fileWriter = new IniFileWriter();
            fileWriter.WriteIniFile(iniSections, testFilePath);

            iniSections.Clear();
            var newIniSection = new IniSection("myNewSection");
            iniSection.AddProperty("newKey", "newValue");
            iniSections.Add(newIniSection);
            fileWriter.WriteIniFile(iniSections, testFilePath, false, true);

            var readIniSections = new IniReader().ReadIniFile(testFilePath);
            Assert.That(readIniSections.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenIniSections_WhenNotAppendingToIniFile_ThenFileWriterShouldNotAppend()
        {
            var iniSection = new IniSection("mySection");
            iniSection.AddProperty("haha", "lala");
            var iniSections = new List<IniSection> { iniSection };

            var fileWriter = new IniFileWriter();
            fileWriter.WriteIniFile(iniSections, testFilePath);

            iniSections.Clear();
            var newIniSection = new IniSection("myNewSection");
            iniSection.AddProperty("newKey", "newValue");
            iniSections.Add(newIniSection);
            fileWriter.WriteIniFile(iniSections, testFilePath, false, false);

            var readSections = new IniReader().ReadIniFile(testFilePath);
            Assert.That(readSections.Count, Is.EqualTo(1));
        }
    }
}