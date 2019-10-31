using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DelftIniReaders;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Readers
{
    [TestFixture]
    public class MduDelftIniReaderTest
    {
        [Test]
        [TestCaseSource(nameof(GetMultiValuedPropertiesFileContents))]
        public void ReadDelftIniFile_WithMultipleValuedProperty_ThenPropertyIsReadCorrectly(string fileContent)
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

        private static IEnumerable<string> GetMultiValuedPropertiesFileContents()
        {
            yield return "[output]"
                         + Environment.NewLine
                         + @"ObsFile  = obs_1_obs.xyn obs_2_obs.xyn obs_3_obs.xyn  # My comment";

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

            yield return "[output]"
                         + Environment.NewLine
                         + @"ObsFile  = obs_1_obs.xyn \ # This comment should be ignored"
                         + Environment.NewLine
                         + "obs_2_obs.xyn obs_3_obs.xyn  # My comment";

            yield return "[output]"
                         + Environment.NewLine
                         + @"ObsFile  = obs_1_obs.xyn \"
                         + Environment.NewLine
                         + @"obs_2_obs.xyn \ # This comment should be ignored"
                         + Environment.NewLine
                         + "obs_3_obs.xyn  # My comment";
        }
    }
}