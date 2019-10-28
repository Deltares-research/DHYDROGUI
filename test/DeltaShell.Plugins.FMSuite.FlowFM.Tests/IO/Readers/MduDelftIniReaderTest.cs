using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Readers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Readers
{
    [TestFixture]
    public class MduDelftIniReaderTest
    {
        [Test]
        [TestCaseSource(nameof(MyStrings))]
        public void ReadDelftIniFile_WithMultipleValuedPropertyMultilineDefined_ThenPropertyIsReadCorrectly(string fileContent)
        {
            // Setup
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));
            var reader = new MduDelftIniReader();

            // Call
            DelftIniCategory category = reader.ReadDelftIniFile(stream, string.Empty).Single();

            // Assert
            Assert.That(category.Name, Is.EqualTo("output"));

            DelftIniProperty property = category.Properties.Single();
            Assert.That(property.Name, Is.EqualTo("ObsFile"));
            Assert.That(property.Value, Is.EqualTo("obs_1_obs.xyn obs_2_obs.xyn obs_3_obs.xyn"));
            Assert.That(property.Comment, Is.EqualTo("My comment"));
        }

        [Test]
        public void ReadDelftIniFile_WithMultipleValuedPropertyThatHasInvalidCommentFormat_ThrowsFormatException()
        {
            // Setup
            string fileContent = "[output]"
                                 + Environment.NewLine
                                 + @"ObsFile  = obs_1_obs.xyn \ # Invalid place for comment"
                                 + Environment.NewLine
                                 + "obs_3_obs.xyn  # My comment";

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));
            var reader = new MduDelftIniReader();

            // Call
            const string fileName = "myFile.mdu";
            void Call() => reader.ReadDelftIniFile(stream, fileName);

            // Assert
            var exception = Assert.Throws<FormatException>(Call);
            Assert.That(exception.Message, Is.EqualTo($"Invalid comment placed on line 2 in file '{fileName}'"));
        }

        [Test]
        public void ReadDelftIniFile_WithMultipleValuedPropertyThatHasInvalidCommentFormat2_ThrowsFormatException()
        {
            // Setup
            string fileContent = "[output]"
                                 + Environment.NewLine
                                 + @"ObsFile  = obs_1_obs.xyn \"
                                 + Environment.NewLine
                                 + @"obs_2_obs.xyn \ # Invalid place for comment"
                                 + Environment.NewLine
                                 + "obs_3_obs.xyn  # My comment";

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));
            var reader = new MduDelftIniReader();

            // Call
            const string fileName = "myFile.mdu";
            void Call() => reader.ReadDelftIniFile(stream, fileName);

            // Assert
            var exception = Assert.Throws<FormatException>(Call);
            Assert.That(exception.Message, Is.EqualTo($"Invalid comment placed on line 3 in file '{fileName}'"));
        }

        private IEnumerable<string> MyStrings()
        {
            yield return "[output]"
                         + Environment.NewLine
                         + @"ObsFile  = obs_1_obs.xyn obs_2_obs.xyn \"
                         + Environment.NewLine
                         + "obs_3_obs.xyn  # My comment";

            yield return "[output]"
                         + Environment.NewLine
                         + @"ObsFile  = obs_1_obs.xyn \"
                         + Environment.NewLine
                         + @"obs_2_obs.xyn \"
                         + Environment.NewLine
                         + "obs_3_obs.xyn  # My comment";

            yield return "[output]"
                         + Environment.NewLine
                         + @"ObsFile  = \"
                         + Environment.NewLine
                         + @"obs_1_obs.xyn \"
                         + Environment.NewLine
                         + @"obs_2_obs.xyn \"
                         + Environment.NewLine
                         + "obs_3_obs.xyn  # My comment";
        }
    }
}