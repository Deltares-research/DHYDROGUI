using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.RgfGrid
{
    [Category(TestCategory.WindowsForms)]
    public class RgfGridEditorTest
    {
        private const int MaxTimeOut = 120000; // 2 minutes

        // TODO: tried to un-mute these tests, still having trouble when running on build server (better luck next time)

        [Test]
        [Apartment(ApartmentState.MTA)]
        [Timeout(MaxTimeOut)]
        [Category(TestCategory.VerySlow)]
        [Ignore("Times-out on Build Server, needs to be run manually :(")]
        public void ShowWithData()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            PerformActionWithCancellationThread(MaxTimeOut, () =>
                                                    RgfGridEditor.OpenGrid(model.NetFilePath));
        }

        [Test]
        [Apartment(ApartmentState.MTA)]
        [Timeout(MaxTimeOut)]
        [Category(TestCategory.VerySlow)]
        [Ignore("Times-out on Build Server, needs to be run manually :(")]
        public void ShowWithEmptyGrid()
        {
            var model = new WaterFlowFMModel();
            ((IFileBased) model).CreateNew(Path.Combine(Path.GetTempPath(), "model"));
            model.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                 .SetValueFromString(model.Name + "_net.nc");

            PerformActionWithCancellationThread(MaxTimeOut, () =>
                                                    RgfGridEditor.OpenGrid(model.NetFilePath, true, new string[0]));
        }

        [Test]
        [Apartment(ApartmentState.MTA)]
        [Timeout(MaxTimeOut)]
        [Category(TestCategory.VerySlow)]
        [Ignore("Times-out on Build Server, needs to be run manually :(")]
        public void ShowWithDataAndLandBoundary()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            PerformActionWithCancellationThread(MaxTimeOut, () =>
                                                    RgfGridEditor.OpenGrid(model.NetFilePath, false, new[]
                                                    {
                                                        TestHelper.GetTestFilePath(@"harlingen\Harlingen_haven.ldb")
                                                    }));
        }

        [Test]
        [Apartment(ApartmentState.MTA)]
        [Category(TestCategory.Slow)]
        public void GeneratePolygonsForEmbankments()
        {
            var pointList = new[]
            {
                new Coordinate
                {
                    X = 10,
                    Y = 10
                },
                new Coordinate
                {
                    X = 30,
                    Y = 10
                },
                new Coordinate
                {
                    X = 50,
                    Y = 20
                },
                new Coordinate
                {
                    X = 40,
                    Y = 40
                },
                new Coordinate
                {
                    X = 20,
                    Y = 50
                },
                new Coordinate
                {
                    X = 0,
                    Y = 30
                },
                new Coordinate
                {
                    X = 10,
                    Y = 10
                }
            };

            var polygons = new List<IPolygon> {new Polygon(new LinearRing(pointList))};

            TestHelper.PerformActionInTemporaryDirectory(temporaryDir =>
            {
                string gridPath = Path.Combine(temporaryDir, "empty_grid.nc");
                UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile(gridPath);

                // perform operation
                RgfGridEditor.OpenGrid(gridPath, false, polygons, "polygon.pol");

                Assert.IsTrue(new FileInfo(gridPath).Length > 0, "Generated grid file is empty, RGFGrid generation failed.");

                using (var uGrid = new UGrid(gridPath))
                {
                    int numEdges = uGrid.GetNumberOfEdgesForMeshId(1);
                    Assert.AreEqual(12, numEdges); // 12 new rows. 
                }
            });
        }

        [Test]
        [Apartment(ApartmentState.MTA)]
        [Category(TestCategory.Slow)]
        public void GenerateAnExtraGrid()
        {
            var pointList = new[]
            {
                new Coordinate
                {
                    X = 110,
                    Y = 10
                },
                new Coordinate
                {
                    X = 130,
                    Y = 10
                },
                new Coordinate
                {
                    X = 150,
                    Y = 20
                },
                new Coordinate
                {
                    X = 140,
                    Y = 40
                },
                new Coordinate
                {
                    X = 120,
                    Y = 50
                },
                new Coordinate
                {
                    X = 100,
                    Y = 30
                },
                new Coordinate
                {
                    X = 110,
                    Y = 10
                }
            };
            var polygons = new List<IPolygon> {new Polygon(new LinearRing(pointList))};
            string gridPath = TestHelper.GetTestFilePath(@"grid_generation\existing_grid.nc");
            gridPath = TestHelper.CreateLocalCopy(gridPath);

            // perform operation
            RgfGridEditor.OpenGrid(gridPath, false, polygons, "polygon.pol");

            Assert.IsTrue(new FileInfo(gridPath).Length > 0, "Generated grid file is empty, RGFGrid generation failed.");

            using (var uGrid = new UGrid(gridPath))
            {
                int numEdges = uGrid.GetNumberOfEdgesForMeshId(1);
                Assert.AreEqual(24, numEdges); // 12 existing + 12 new rows.
            }
        }

        private static void PerformActionWithCancellationThread(int timeout, Action action)
        {
            // Action waits for rgfgrid to close, we do this manually from another thread
            var cancellationThread = new Thread(() => CloseRgfGrid(timeout));
            cancellationThread.Start();

            // Invoke action
            action.Invoke();
        }

        private static void CloseRgfGrid(int maxTimeout)
        {
            Thread.Sleep(500); // Give action time to get started
            const int millisecondsToSleep = 100;

            // Get active rgfGrid processes (there should only be one)
            Process[] rgfGridProcesses = Process.GetProcessesByName(RgfGridEditor.MfeAppProcessName);
            while (!rgfGridProcesses.Any())
            {
                Thread.Sleep(millisecondsToSleep);
                rgfGridProcesses = Process.GetProcessesByName(RgfGridEditor.MfeAppProcessName);
            }

            foreach (Process process in rgfGridProcesses)
            {
                var totalTimeWaiting = 0;
                // attempt to close rgfGrid (may not be successful straight away)
                while (!process.CloseMainWindow())
                {
                    totalTimeWaiting += millisecondsToSleep;
                    Thread.Sleep(millisecondsToSleep);

                    if (totalTimeWaiting > maxTimeout)
                    {
                        return;
                    }
                }
            }
        }
    }
}