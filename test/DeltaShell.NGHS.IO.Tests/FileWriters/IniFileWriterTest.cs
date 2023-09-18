using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters;
using DHYDRO.Common.IO.Ini;
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
            var iniSection = new IniSection("myCategory");
            iniSection.AddProperty("haha", "lala");
            var categories = new List<IniSection> { iniSection };

            var fileWriter = new IniFileWriter();
            fileWriter.WriteIniFile(categories, testFilePath);

            categories.Clear();
            var newIniSection = new IniSection("myNewCategory");
            iniSection.AddProperty("newKey", "newValue");
            categories.Add(newIniSection);
            fileWriter.WriteIniFile(categories, testFilePath, false, true);

            var readCategories = new DelftIniReader().ReadDelftIniFile(testFilePath);
            Assert.That(readCategories.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenIniSections_WhenNotAppendingToIniFile_ThenFileWriterShouldNotAppend()
        {
            var iniSection = new IniSection("myCategory");
            iniSection.AddProperty("haha", "lala");
            var categories = new List<IniSection> { iniSection };

            var fileWriter = new IniFileWriter();
            fileWriter.WriteIniFile(categories, testFilePath);

            categories.Clear();
            var newIniSection = new IniSection("myNewCategory");
            iniSection.AddProperty("newKey", "newValue");
            categories.Add(newIniSection);
            fileWriter.WriteIniFile(categories, testFilePath, false, false);

            var readCategories = new DelftIniReader().ReadDelftIniFile(testFilePath);
            Assert.That(readCategories.Count, Is.EqualTo(1));
        }
    }
}