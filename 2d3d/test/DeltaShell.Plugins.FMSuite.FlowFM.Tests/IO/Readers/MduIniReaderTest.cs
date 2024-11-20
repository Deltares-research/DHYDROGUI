using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.IniReaders;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Readers
{
    [TestFixture]
    public class MduIniReaderTest
    {
        [Test]
        [TestCaseSource(nameof(GetMultiValuedPropertiesFileContents))]
        public void ReadIniFile_WithMultipleValuedProperty_ThenPropertyIsReadCorrectly(string fileContent, string expectedComment)
        {
            // Setup
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));
            var reader = new MduIniReader();

            // Call
            IniData iniData = reader.ReadIniFile(stream, string.Empty);
            IniSection iniSection = iniData.Sections.Single();

            // Assert
            Assert.That(iniSection.Name, Is.EqualTo("output"));

            IniProperty property = iniSection.Properties.Single();
            Assert.That(property.Key, Is.EqualTo("ObsFile"));
            Assert.That(property.Value, Is.EqualTo("obs_1_obs.xyn obs_2_obs.xyn obs_3_obs.xyn"));
            Assert.That(property.Comment, Is.EqualTo(expectedComment));
        }

        private static IEnumerable<object> GetMultiValuedPropertiesFileContents()
        {
            yield return new object[]
            {
                "[output]"
                + Environment.NewLine
                + @"ObsFile  = obs_1_obs.xyn obs_2_obs.xyn obs_3_obs.xyn  # My comment",
                "My comment"
            };

            yield return new object[]
            {
                "[output]"
                + Environment.NewLine
                + @"ObsFile  = obs_1_obs.xyn obs_2_obs.xyn \"
                + Environment.NewLine
                + "obs_3_obs.xyn  # My comment",
                "My comment"
            };

            yield return new object[]
            {
                "[output]"
                + Environment.NewLine
                + @"ObsFile  = obs_1_obs.xyn \"
                + Environment.NewLine
                + @"obs_2_obs.xyn \"
                + Environment.NewLine
                + "obs_3_obs.xyn  # My comment",
                "My comment"
            };

            yield return new object[]
            {
                "[output]"
                + Environment.NewLine
                + @"ObsFile  = \"
                + Environment.NewLine
                + @"obs_1_obs.xyn \"
                + Environment.NewLine
                + @"obs_2_obs.xyn \"
                + Environment.NewLine
                + "obs_3_obs.xyn  # My comment",
                "My comment"
            };

            yield return new object[]
            {
                "[output]"
                + Environment.NewLine
                + @"ObsFile  = obs_1_obs.xyn \"
                + Environment.NewLine
                + "obs_2_obs.xyn obs_3_obs.xyn",
                string.Empty
            };

            yield return new object[]
            {
                "[output]"
                + Environment.NewLine
                + @"ObsFile  = obs_1_obs.xyn \ # This comment should be ignored"
                + Environment.NewLine
                + "obs_2_obs.xyn obs_3_obs.xyn  # My comment",
                "My comment"
            };

            yield return new object[]
            {
                "[output]"
                + Environment.NewLine
                + @"ObsFile  = obs_1_obs.xyn \"
                + Environment.NewLine
                + @"obs_2_obs.xyn \ # This comment should be ignored"
                + Environment.NewLine
                + "obs_3_obs.xyn  # My comment",
                "My comment"
            };
        }
    }
}