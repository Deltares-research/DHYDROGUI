using System;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructuresFileWriterTest
    {
        [Test]
        public void GivenFmModelWithPump_WhenWritingStructures_ThenTheCorrectFilesAreWritten()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");

            var pumpName = "myPump";
            var pliFileName = pumpName + ".pli";
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, pliFileName);

            var timFileName = pumpName + ".tim";
            var timFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, timFileName);

            var pump2D = new Pump2D(pumpName)
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(2, 2), new Coordinate(10, -2) })
            };
            var area = new HydroArea();
            area.Pumps.Add(pump2D);

            try
            {
                var referenceDateTime = DateTime.Now;
                StructureFileWriter.WriteFile(structuresFilePath, new HydroNetwork(), area, referenceDateTime);
                Assert.IsTrue(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.IsTrue(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
                Assert.IsFalse(File.Exists(timFilePath),
                    $"Time series file has been written to location {timFilePath}, while it the pump does not have a time series for the capacity");
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithPumpThatHasATimeSeriesForCapacity_WhenWritingStructures_ThenTheCorrectFilesAreWritten()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");

            var pumpName = "myPump";
            var pliFileName = pumpName + ".pli";
            var timFileName = $"{pumpName}_{StructureRegion.Capacity.Key}.tim";
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, pliFileName);
            var timFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, timFileName);

            var pump2D = new Pump2D(pumpName)
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(2, 2), new Coordinate(10, -2)}),
                CanBeTimedependent = true,
                UseCapacityTimeSeries = true,
                CapacityTimeSeries =
                {
                    [new DateTime(2013, 1, 2, 3, 4, 0)] = 2.1,
                    [new DateTime(2013, 7, 8, 9, 10, 0)] = 3.34
                }
            };

            var area = new HydroArea();
            area.Pumps.Add(pump2D);

            try
            {
                var referenceDateTime = new DateTime(2013, 1, 2);
                StructureFileWriter.WriteFile(structuresFilePath, new HydroNetwork(), area, referenceDateTime);
                Assert.That(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.That(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
                Assert.That(File.Exists(timFilePath), $"Time series file has not been written to location {timFilePath}");
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }
    }
}