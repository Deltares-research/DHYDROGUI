using System;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructuresFileWriterTest
    {
        #region Write Pump

        [Test]
        public void GivenFmModelWithPump_WhenWritingStructures_ThenTheCorrectFilesAreWritten()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var pumpName = "myPump";
            var pliFileName = pumpName + ".pli";
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, pliFileName);

            var timFileName = pumpName + ".tim";
            var timFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, timFileName);

            var pump2D = new Pump2D(pumpName)
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(2, 2), new Coordinate(10, -2)})
            };
            var area = new HydroArea();
            area.Pumps.Add(pump2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath,
                    new WaterFlowFMModel {Area = area, MduFilePath = mduFilePath},
                    StructureFile.Generate2DStructureCategoriesFromFMModel);

                Assert.IsTrue(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.IsTrue(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
                Assert.IsFalse(File.Exists(timFilePath),
                    $"Time series file has been written to location {timFilePath}, while the pump does not have a time series for the capacity");
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
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

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

            var fmModel = new WaterFlowFMModel
            {
                ReferenceTime = new DateTime(2013, 1, 2),
                MduFilePath = mduFilePath
            };
            fmModel.Area.Pumps.Add(pump2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, StructureFile.Generate2DStructureCategoriesFromFMModel);

                Assert.That(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.That(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
                Assert.That(File.Exists(timFilePath), $"Time series file has not been written to location {timFilePath}");
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        #endregion

        #region Write Weir

        [Test]
        public void GivenFmModelWithWeir_WhenWritingStructures_ThenTheCorrectFilesAreWritten()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var weirName = "myWeir";
            var pliFileName = weirName + ".pli";
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, pliFileName);

            var timFileName = weirName + "_crest_level.tim";
            var timFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, timFileName);

            var weir2D = new Weir2D(weirName)
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(2, 2), new Coordinate(10, -2)})
            };
            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                ReferenceTime = DateTime.Now
            };
            fmModel.Area.Weirs.Add(weir2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, StructureFile.Generate2DStructureCategoriesFromFMModel);
                Assert.IsTrue(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.IsTrue(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
                Assert.IsFalse(File.Exists(timFilePath),
                    $"Time series file has been written to location {timFilePath}, while the weir does not have a time series for the capacity");
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithWeirThatHasATimeSeriesForCapacity_WhenWritingStructures_ThenTheCorrectFilesAreWritten()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var weirName = "myWeir";
            var pliFileName = weirName + ".pli";
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, pliFileName);

            var timFileName = weirName + "_crest_level.tim";
            var timFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, timFileName);

            var weir2D = new Weir2D(weirName, true)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                CrestWidth = 2.58,
                WeirFormula = new SimpleWeirFormula { LateralContraction = 0.34 },
                UseCrestLevelTimeSeries = true
            };

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                ReferenceTime = DateTime.Now
            };
            fmModel.Area.Weirs.Add(weir2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, StructureFile.Generate2DStructureCategoriesFromFMModel);
                Assert.IsTrue(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.IsTrue(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
                Assert.IsTrue(File.Exists(timFilePath), $"Time series file has not been written to location {timFilePath}");
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        #endregion

        #region Write General Structure

        [Test]
        public void GivenFmModelWithGeneralStructure_WhenWritingStructures_ThenTheCorrectFilesAreWritten()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var generalStructureName = "myGeneralStructure";
            var pliFileName = generalStructureName + ".pli";
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, pliFileName);

            var generalStructure2D = new Weir2D(generalStructureName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2), new Coordinate(10, -2) }),
                WeirFormula = new GeneralStructureWeirFormula()
            };
            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                ReferenceTime = DateTime.Now
            };
            fmModel.Area.Weirs.Add(generalStructure2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, StructureFile.Generate2DStructureCategoriesFromFMModel);
                Assert.IsTrue(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.IsTrue(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        #endregion

        #region Write Gate

        [Test]
        public void GivenFmModelWithGateThatHasSillLevelTimeSeries_WhenWritingStructures_ThenTheCorrectFilesAreWritten()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");
            
            var gateName = "myGate";
            var pliFileName = gateName + ".pli";
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, pliFileName);

            var timeSeriesFileName = $"{gateName}_{KnownStructureProperties.GateSillLevel}.tim";
            var timeSeriesFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, timeSeriesFileName);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var gate2D = new Gate2D(gateName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                UseSillLevelTimeSeries = true
            };
            fmModel.Area.Gates.Add(gate2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, StructureFile.Generate2DStructureCategoriesFromFMModel);
                Assert.IsTrue(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.IsTrue(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
                Assert.IsTrue(File.Exists(timeSeriesFilePath), $"Time series file has not been written to location {timeSeriesFilePath}");
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithGateThatHasLowerEdgeLevelTimeSeries_WhenWritingStructures_ThenTheCorrectFilesAreWritten()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var gateName = "myGate";
            var pliFileName = gateName + ".pli";
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, pliFileName);

            var timeSeriesFileName = $"{gateName}_{KnownStructureProperties.GateLowerEdgeLevel}.tim";
            var timeSeriesFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, timeSeriesFileName);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var gate2D = new Gate2D(gateName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                UseLowerEdgeLevelTimeSeries = true
            };
            fmModel.Area.Gates.Add(gate2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, StructureFile.Generate2DStructureCategoriesFromFMModel);
                Assert.IsTrue(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.IsTrue(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
                Assert.IsTrue(File.Exists(timeSeriesFilePath), $"Time series file has not been written to location {timeSeriesFilePath}");
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithGateThatHasOpeningWidthTimeSeries_WhenWritingStructures_ThenTheCorrectFilesAreWritten()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var gateName = "myGate";
            var pliFileName = gateName + ".pli";
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, pliFileName);

            var timeSeriesFileName = $"{gateName}_{KnownStructureProperties.GateOpeningWidth}.tim";
            var timeSeriesFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, timeSeriesFileName);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var gate2D = new Gate2D(gateName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                UseOpeningWidthTimeSeries = true
            };
            fmModel.Area.Gates.Add(gate2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, StructureFile.Generate2DStructureCategoriesFromFMModel);
                Assert.IsTrue(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.IsTrue(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
                Assert.IsTrue(File.Exists(timeSeriesFilePath), $"Time series file has not been written to location {timeSeriesFilePath}");
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        #endregion

        #region Write Levee Breach

        [Test]
        public void GivenFmModelWithLeveeBreachThatHasUserDefinedFormula_WhenWritingStructures_ThenTheCorrectFilesAreWritten()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var leveeBreachName = "myBreach";
            var pliFileName = leveeBreachName + ".pli";
            var pliFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, pliFileName);

            var timeSeriesFileName = $"{leveeBreachName}_{KnownStructureProperties.TimeFilePath}.tim";
            var timeSeriesFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(structuresFilePath, timeSeriesFileName);

            var referenceTime = new DateTime(2018, 8, 25);
            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                ReferenceTime = referenceTime
            };
            var leveeBreach = new LeveeBreach
            {
                Name = leveeBreachName,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                LeveeBreachFormula = LeveeBreachGrowthFormula.UserDefinedBreach
            };
            leveeBreach.SetBaseLeveeBreachSettings(referenceTime.AddHours(2.0), true);
            var settings = leveeBreach.GetActiveLeveeBreachSettings() as UserDefinedBreachSettings;
            settings.ManualBreachGrowthSettings.Add(new BreachGrowthSetting
            {
                Width = 2.0,
                Height = 3.0,
                Area = 6.0,
                TimeSpan = new TimeSpan(0, 1, 0, 0)
            });

            fmModel.Area.LeveeBreaches.Add(leveeBreach);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, StructureFile.Generate2DStructureCategoriesFromFMModel);
                Assert.IsTrue(File.Exists(structuresFilePath), $"Structures file has not been written to location {structuresFilePath}");
                Assert.IsTrue(File.Exists(pliFilePath), $"Polyline file has not been written to location {pliFilePath}");
                Assert.IsTrue(File.Exists(timeSeriesFilePath), $"Time series file has not been written to location {timeSeriesFilePath}");
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        #endregion
    }
}