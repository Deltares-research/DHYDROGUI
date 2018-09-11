using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.NetworkEditor.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.IO
{
    [TestFixture]
    public class NodeFileTest
    {
        private string filePath;

        [SetUp]
        public void Setup()
        {
            filePath = Path.Combine(FileUtils.CreateTempDirectory(), "node.ini");
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(filePath);
        }

        [Test]
        public void WhenWritingRetentionFile_ThenRetentionFileIsExisting()
        {
            NodeFile.Write(filePath, new List<Compartment>());
            Assert.IsTrue(File.Exists(filePath));
        }

        [Test]
        public void GivenListOfManholes_WhenWritingAndReading_ThenRightInformationHasBeenRead()
        {
            var compartment = new Compartment("myCompartment")
            {
                ParentManhole = new Manhole("myManhole"),
                ManholeLength = 33,
                ManholeWidth = 28,
                BottomLevel = 0.33,
                SurfaceLevel = 2.75
            };
            var compartments = new List<Compartment>{ compartment };

            NodeFile.Write(filePath, compartments);
            var propertiesPerCompartment = NodeFile.Read(filePath);

            Assert.That(propertiesPerCompartment.Count, Is.EqualTo(1));

            var compartmentProperties = propertiesPerCompartment[0];
            Assert.That(compartmentProperties.CompartmentId, Is.EqualTo(compartment.Name));
            Assert.That(compartmentProperties.ManholeId, Is.EqualTo(compartment.ParentManhole.Name));
            Assert.That(compartmentProperties.BottomLevel, Is.EqualTo(0.33));

            var area = compartment.ManholeLength * compartment.ManholeWidth;
            Assert.That(compartmentProperties.Area, Is.EqualTo(area));
            Assert.That(compartmentProperties.StreetLevel, Is.EqualTo(2.75));
        }
    }
}