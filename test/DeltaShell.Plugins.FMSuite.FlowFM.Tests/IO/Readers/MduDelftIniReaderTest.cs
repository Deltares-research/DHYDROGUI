using System;
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
        public void ReadDelftIniFile_WithMultipleValuedPropertyDefinedOnTwoLines_ThenPropertyIsReadCorrectly()
        {
            // Setup
            string fileContent = "[output]"
                                 + Environment.NewLine
                                 + @"ObsFile  = obs_1_obs.xyn obs_2_obs.xyn \"
                                 + Environment.NewLine
                                 + "obs_3_obs.xyn  # My comment";

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));
            var reader = new MduDelftIniReader();

            // Call
            DelftIniCategory category = reader.ReadDelftIniFile(stream, "myFilePath").Single();

            // Assert
            Assert.That(category.Name, Is.EqualTo("output"));

            DelftIniProperty property = category.Properties.Single();
            Assert.That(property.Name, Is.EqualTo("ObsFile"));
            Assert.That(property.Value, Is.EqualTo("obs_1_obs.xyn obs_2_obs.xyn obs_3_obs.xyn"));
            Assert.That(property.Comment, Is.EqualTo("My comment"));
        }

        [Test]
        public void ReadDelftIniFile_WithMultipleValuedPropertyDefinedOnThreeLines_ThenPropertyIsReadCorrectly()
        {
            // Setup
            string fileContent = "[output]"
                                 + Environment.NewLine
                                 + @"ObsFile  = obs_1_obs.xyn \"
                                 + Environment.NewLine
                                 + @"obs_2_obs.xyn \"
                                 + Environment.NewLine
                                 + "obs_3_obs.xyn  # My comment";

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));
            var reader = new MduDelftIniReader();

            // Call
            DelftIniCategory category = reader.ReadDelftIniFile(stream, "myFilePath").Single();

            // Assert
            Assert.That(category.Name, Is.EqualTo("output"));

            DelftIniProperty property = category.Properties.Single();
            Assert.That(property.Name, Is.EqualTo("ObsFile"));
            Assert.That(property.Value, Is.EqualTo("obs_1_obs.xyn obs_2_obs.xyn obs_3_obs.xyn"));
            Assert.That(property.Comment, Is.EqualTo("My comment"));
        }

        [Test]
        public void ReadDelftIniFile_WithMultipleValuedPropertyDefinedOnNextLines_ThenPropertyIsReadCorrectly()
        {
            // Setup
            string fileContent = "[output]"
                                 + Environment.NewLine
                                 + @"ObsFile  = \"
                                 + Environment.NewLine
                                 + @"obs_1_obs.xyn \"
                                 + Environment.NewLine
                                 + @"obs_2_obs.xyn \"
                                 + Environment.NewLine
                                 + "obs_3_obs.xyn  # My comment";

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent));
            var reader = new MduDelftIniReader();

            // Call
            DelftIniCategory category = reader.ReadDelftIniFile(stream, "myFilePath").Single();

            // Assert
            Assert.That(category.Name, Is.EqualTo("output"));

            DelftIniProperty property = category.Properties.Single();
            Assert.That(property.Name, Is.EqualTo("ObsFile"));
            Assert.That(property.Value, Is.EqualTo("obs_1_obs.xyn obs_2_obs.xyn obs_3_obs.xyn"));
            Assert.That(property.Comment, Is.EqualTo("My comment"));
        }
    }
}