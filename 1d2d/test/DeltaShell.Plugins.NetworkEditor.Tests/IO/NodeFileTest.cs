using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.Network;
using NetTopologySuite.Extensions.Networks;
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
        [Category(TestCategory.DataAccess)]
        public void WhenWritingRetentionFile_ThenRetentionFileIsExisting()
        {
            var branch = new Branch { Source = new Node() };
            NodeFile.Write(filePath, new List<Compartment>(), new List<IRetention> { new Retention { Branch = branch } });
            Assert.IsTrue(File.Exists(filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
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
            var compartments = new List<Compartment> { compartment };

            NodeFile.Write(filePath, compartments, null);
            var propertiesPerCompartment = NodeFile.Read(filePath);

            Assert.That(propertiesPerCompartment.Count, Is.EqualTo(1));

            var compartmentProperties = propertiesPerCompartment[0];
            Assert.That(compartmentProperties.CompartmentId, Is.EqualTo(compartment.Name));
            Assert.That(compartmentProperties.ManholeId, Is.EqualTo(compartment.ParentManhole.Name));
            Assert.That(compartmentProperties.BedLevel, Is.EqualTo(0.33));

            var area = compartment.ManholeLength * compartment.ManholeWidth;
            Assert.That(compartmentProperties.Area, Is.EqualTo(area));
            Assert.That(compartmentProperties.StreetLevel, Is.EqualTo(2.75));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenManholeWithStorageTableAndUseTableTrue_StorageTableIsWrittenAndRead()
        {
            var compartment = new Compartment("manholeCompartment")
            {
                ParentManhole = new Manhole("manhole"),
                ManholeLength = 1.2,
                ManholeWidth = 3.4,
                BottomLevel = 5.6,
                SurfaceLevel = 7.8,
                UseTable = true
            };
            compartment.Storage.Arguments[0].SetValues( new []{ -6.5, -4.3, -2.1 } );
            compartment.Storage.Components[0].SetValues( new []{ 19.0, 18.0, 12.0 } );
            var compartments = new List<Compartment> { compartment };

            NodeFile.Write(filePath, compartments, null);
            
            var listOfCompartmentProperties = NodeFile.Read(filePath);

            Assert.That(listOfCompartmentProperties.Count, Is.EqualTo(1));
            var compartmentProperties = listOfCompartmentProperties[0];
            Assert.That(compartmentProperties.UseTable,Is.True);
            Assert.That(compartmentProperties.NumberOfLevels,Is.EqualTo(3));
            Assert.That(compartmentProperties.Levels,Is.EqualTo(new []{ -6.5, -4.3, -2.1 }));
            Assert.That(compartmentProperties.StorageAreas,Is.EqualTo(new []{ 19.0, 18.0, 12.0 }));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenManholeWithStorageTableAndUseTableFalse_StorageTableIsNotWrittenAndRead()
        {
            var compartment = new Compartment("manholeCompartment")
            {
                ParentManhole = new Manhole("manhole"),
                ManholeLength = 1.2,
                ManholeWidth = 3.4,
                BottomLevel = 5.6,
                SurfaceLevel = 7.8,
                UseTable = false
            };
            compartment.Storage.Arguments[0].SetValues( new []{ -6.5, -4.3, -2.1 } );
            compartment.Storage.Components[0].SetValues( new []{ 19.0, 18.0, 12.0 } );
            var compartments = new List<Compartment> { compartment };

            NodeFile.Write(filePath, compartments, null);
            
            var compartmentsRead = NodeFile.Read(filePath);

            Assert.That(compartmentsRead.Count, Is.EqualTo(1));
            var compartmentRead = compartmentsRead[0];
            Assert.That(compartmentRead.UseTable,Is.False);
            Assert.That(compartmentRead.NumberOfLevels,Is.EqualTo(0));
            Assert.That(compartmentRead.Levels,Is.Null);
            Assert.That(compartmentRead.StorageAreas,Is.Null);
        }
    }
}