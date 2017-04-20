using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DelftTools.Utils.Data;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class HydFileDataTest
    {
        [Test]
        public void InitialValuesAfterDefaultConstructor()
        {
            // setup

            // call
            var data = new HydFileData();

            // assert
            Assert.IsInstanceOf<IUnique<long>>(data);
            Assert.IsNull(data.Path);
            Assert.IsNull(data.Checksum);
            Assert.AreEqual(HydroDynamicModelType.Undefined, data.HydroDynamicModelType);
            Assert.AreEqual(DateTime.MinValue, data.ConversionReferenceTime);
            Assert.AreEqual(DateTime.MinValue, data.ConversionStartTime);
            Assert.AreEqual(DateTime.MinValue, data.ConversionStopTime);
            Assert.AreEqual(TimeSpan.FromTicks(0), data.ConversionTimeStep);
            Assert.AreEqual(0, data.NumberOfHorizontalExchanges);
            Assert.AreEqual(0, data.NumberOfVerticalExchanges);
            Assert.AreEqual(0, data.NumberOfDelwaqSegmentsPerHydrodynamicLayer);
            Assert.AreEqual(0, data.NumberOfWaqSegmentLayers);
            Assert.AreEqual(0, data.NumberOfHydrodynamicLayers);

            Assert.IsNull(data.BoundariesRelativePath);
            Assert.IsNull(data.SchematizationRelativePath);

            Assert.IsEmpty(data.HydrodynamicLayerThicknesses);
            Assert.IsEmpty(data.NumberOfHydrodynamicLayersPerWaqSegmentLayer);

            Assert.AreEqual(string.Empty, data.VolumesRelativePath);
            Assert.AreEqual(string.Empty, data.AreasRelativePath);
            Assert.AreEqual(string.Empty, data.FlowsRelativePath);
            Assert.AreEqual(string.Empty, data.PointersRelativePath);
            Assert.AreEqual(string.Empty, data.LengthsRelativePath);
            Assert.AreEqual(string.Empty, data.SalinityRelativePath);
            Assert.AreEqual(string.Empty, data.TemperatureRelativePath);
            Assert.AreEqual(string.Empty, data.VerticalDiffusionRelativePath);
            Assert.AreEqual(string.Empty, data.SurfacesRelativePath);
            Assert.AreEqual(string.Empty, data.ShearStressesRelativePath);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportAndChangeHydFileShouldGiveDataChangedEvent()
        {
            var hydFileName = "square.hyd";

            var targetDirectory = "square";

            try
            {
                FileUtils.CopyDirectory(Path.Combine(TestHelper.GetDataDir(), @"IO\square\"), targetDirectory);

                var newFilePath = Path.Combine(targetDirectory, hydFileName);
                
                var data = new HydFileReader(new FileInfo(newFilePath)).ReadAll();
                var count = 0;
                data.DataChanged += (s, e) => count++;

                using (var d = new StreamWriter(newFilePath, true))
                {
                    d.WriteLine("");
                    d.Close();
                }

                // needed for processing the async event
                Application.DoEvents();
                Thread.Sleep(200);

                Assert.AreEqual(1,count);

            }
            finally
            {
                if (Directory.Exists(targetDirectory))
                {
                    Directory.Delete(targetDirectory, true);
                }
            }
        }

        [Test]
        public void HasDataForReturnFalseIfNoReadHasBeenPerformed()
        {
            // setup
            var data = new HydFileData();

            // call
            var hasData = data.HasDataFor(null);

            // assert
            Assert.IsFalse(hasData);
        }

        [Test]
        public void HasSameSchematizationForItself()
        {
            // setup
            var data = new HydFileData
            {
                Checksum = "1234567890abcdfe1234567890abcdfe"
            };

            // call
            var isSame = data.HasSameSchematization(data);

            // assert
            Assert.IsTrue(isSame);
        }

        [Test]
        public void HasSameSchematizationForHydFileDataWithSameChecksum()
        {
            // setup
            const string checksumStub = "1234567890abcdfe1234567890abcdfe";
            var data = new HydFileData
            {
                Checksum = checksumStub
            };
            var data2 = new HydFileData
            {
                Checksum = checksumStub
            };

            // call & assert
            Assert.IsTrue(data.HasSameSchematization(data2));
            Assert.IsTrue(data2.HasSameSchematization(data));
        }

        [Test]
        public void HasSameSchematizationForDifferentHydFile()
        {
            // setup
            var data = new HydFileData
            {
                Checksum = "1234567890abcdfe1234567890abcdfe"
            };

            var data2 = new HydFileData
            {
                Checksum = "abcdef12345678900987654321fedcba"
            };

            // call & assert
            Assert.IsFalse(data.HasSameSchematization(data2));
            Assert.IsFalse(data2.HasSameSchematization(data));
        }

        [Test]
        public void HasSameSchematizationWithNonHydFileDataReturnsFalse()
        {
            // setup
            var data = new HydFileData
            {
                Checksum = "1234567890abcdfe1234567890abcdfe"
            };

            var dataStub = new TestHydroDataStub();

            // call & assert
            Assert.IsFalse(data.HasSameSchematization(dataStub));
            Assert.IsFalse(dataStub.HasSameSchematization(data));
        }
    }
}