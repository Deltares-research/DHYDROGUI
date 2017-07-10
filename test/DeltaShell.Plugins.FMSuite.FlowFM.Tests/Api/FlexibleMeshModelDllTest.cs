using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class FlexibleMeshModelDllTest
    {
        [Test]
        public void AssertUnstrucDllIsXpCompatible()
        {
            // The problem is that platform toolsets (vc++ runtime dependencies) of 110 & higher (eg above 100) 
            // aren't xp compatible. This appears the default on VS2012 and up. If you encounter this problem, 
            // rebuild the dll using a toolset compatible with xp (eg 100, or 110_xp).
            
            // We use a hacky but effective way to check if the current dll is xp compatible, namely we check
            // for the occurance of 'GetTickCount64' in the dll imports. This method is only available on Vista
            // and above.
            var fmDllPath = FlexibleMeshModelDll.DllPath;
            var otherfmDllPath = fmDllPath.Contains("x86")
                ? fmDllPath.Replace("x86", "x64")
                : fmDllPath.Replace("x64", "x86");

            foreach (var dllVersion in new[] { Path.Combine(fmDllPath, FlexibleMeshModelDll.DFLOWFM_DLL_NAME), Path.Combine(otherfmDllPath, FlexibleMeshModelDll.DFLOWFM_DLL_NAME) })
            {
                foreach (var line in File.ReadLines(dllVersion))
                {
                    if (line.Contains("GetTickCount64"))
                        Assert.Fail("Current " + FlexibleMeshModelDll.DFLOWFM_DLL_NAME + " is not compatible with XP: " + dllVersion);
                }
            }
        }

        [Test]
        public void TestCallInitialize()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);
            }
        }

        [Test]
        public void TestCallGetVariableNames()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var variableNames1 = api.VariableNames;
                Assert.NotNull(variableNames1, "FlexibleMeshModelApi should return an array of names");

                api.Update();

                var variableNames2 = api.VariableNames;
                Assert.NotNull(variableNames2, "FlexibleMeshModelApi should return an array of names");

                Assert.AreEqual(variableNames1.Length, variableNames2.Length, "FlexibleMeshModelApi should return the same array of names for each call");
                for (var i = 0; i < variableNames1.Length; i++)
                {
                    Assert.AreEqual(variableNames1[i], variableNames2[i], "FlexibleMeshModelApi should return the same array of names for each call");
                }
            }
        }

        [Test]
        public void TestCallGetVariableLocations()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var variableNames = api.VariableNames;
                Assert.NotNull(variableNames, "FlexibleMeshModelApi should return an array of names");
                Assert.IsTrue(variableNames.Length > 0);

                Dictionary<string, string> namesAndLocations = new Dictionary<string, string>();
                foreach (var variable in variableNames)
                {
                    var location = api.GetVariableLocation(variable);
                    Assert.NotNull(location, string.Format("FlexibleMeshModelApi should return a location for Variable {0}", variable));
                    if(location != "") namesAndLocations.Add(variable, location); // ignore those variables without a location
                }

                api.Update();

                foreach (var variableNameAndLocation in namesAndLocations)
                {
                    var location = api.GetVariableLocation(variableNameAndLocation.Key);
                    Assert.AreEqual(variableNameAndLocation.Value, location, "FlexibleMeshModelApi should return the same Variable location for each call");
                }
            }
        }

        [Test]
        public void TestCallGetValuePumpCapacity()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(localCopy))
            {
                model.Initialize();
                var pump = model.Area.Pumps.First(o => o.Name == "pump01");

                var cat = model.GetFeatureCategory(pump);
                var result = model.GetVar(cat, pump.Name, "capacity");

                Assert.AreEqual(100.0, ((double[]) result)[0]);

                model.Execute();
                result = model.GetVar(cat, pump.Name, "capacity");
                Assert.AreEqual(95.0, ((double[]) result)[0]);
            }

        }

        [Test]
        public void TestCallSetValueWeirCrestLevel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(localCopy))
            {
                model.Initialize();
                // get weir02
                var weir = model.Area.Weirs.First(o => o.Name == "weir02");

                var cat = model.GetFeatureCategory(weir);
                var result = model.GetVar(cat, weir.Name, "crest_level");

                Assert.AreEqual(3.0, ((double[])result)[0]);

                model.SetVar(new[] { -3.0 }, cat, weir.Name, "crest_level");
                result = model.GetVar(cat, weir.Name, "crest_level");
                Assert.AreEqual(-3.0, ((double[])result)[0]);
                
            }
        }

        [Test]
        public void TestCallGetValuesWaterLevelsCount()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(localCopy))
            {
                model.Initialize();

                var waterLevels = model.GetVar("s0") as double[];

                Assert.IsNotNull(waterLevels);

                //Assert.AreEqual(waterLevels.Length, model.Grid.Cells.Count);
                Assert.AreEqual(1, waterLevels.Length); //dimr getvar can only get 1 value!
            }
        }

        [Test]
        public void TestCallSetValuesWaterLevels()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(localCopy))
            {
                model.Initialize();
                var waterLevels = model.GetVar("s0") as double[];

                Assert.IsNotNull(waterLevels);

                var newWaterLevels = waterLevels.Select(s => 1.5*(s+1)).ToArray();

                model.SetVar(newWaterLevels, "s0");

                waterLevels = model.GetVar("s0") as double[];

                Assert.IsNotNull(waterLevels);

                Assert.AreEqual(waterLevels, newWaterLevels);
            }
        }

        [Test]
        public void TestCallGetNonExistingValues()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(localCopy))
            {
                model.Initialize();
                Assert.AreEqual(Dimr.DimrApiDataSet.DIMR_FILL_VALUE, ((double[])model.GetVar("party", "at", "myplace"))[0], 0.01d);
            }
        }

        [Test]
        public void TestGetObservationPointWaterLevel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            using (var model = new WaterFlowFMModel(localCopy))
            {
                model.Initialize();
                // get 13
                var pump = model.Area.ObservationPoints.First(o => o.Name == "13");

                var cat = model.GetFeatureCategory(pump);
                var result = model.GetVar(cat, pump.Name, "water_level");

                // the water level should be found on an observation point, so it is not NaN
                Assert.AreEqual(0.0, ((double[]) result)[0]);
            }
        }

        [Test]
        public void TestWriteNetGeomFileHarlingen()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                api.WriteNetGeometry("netgeom.nc");

                Assert.IsTrue(File.Exists(Path.Combine(Path.GetDirectoryName(model.MduFilePath), "netgeom.nc")));
            }
        }

        [Test]
        public void TestGetSnappedFeatures()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var gridExtent = model.GridExtent;

                var center = gridExtent.Centre;
                var snappedPoint = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ObsPoint, new Point(center));
                var snappedThinDam = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ThinDams,
                    new LineString(new[] {center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0)}));
                var snappedFixedWeir = model.GetGridSnappedGeometry(UnstrucGridOperationApi.FixedWeir,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));
                var snappedCrossSection = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ObsCrossSection,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));
                var snappedWeir = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Weir,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));
                var snappedGate = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Gate,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));
                var snappedPump = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Pump,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));
                var snappedWaterLevelBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.WaterLevelBnd,
                        model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                            .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);
                var snappedVelocityBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.VelocityBnd,
                        model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                            .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);
                var snappedDischargeBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.DischargeBnd,
                        model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                            .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);

                Assert.IsTrue(model.SnapsToGrid(snappedPoint));
                Assert.IsTrue(model.SnapsToGrid(snappedThinDam));
                Assert.IsTrue(model.SnapsToGrid(snappedFixedWeir));
                Assert.IsTrue(model.SnapsToGrid(snappedCrossSection));
                Assert.IsTrue(model.SnapsToGrid(snappedWeir));
                Assert.IsTrue(model.SnapsToGrid(snappedGate));
                Assert.IsTrue(model.SnapsToGrid(snappedPump));
                Assert.IsTrue(model.SnapsToGrid(snappedWaterLevelBnd));
                Assert.IsTrue(model.SnapsToGrid(snappedVelocityBnd));
                Assert.IsTrue(model.SnapsToGrid(snappedDischargeBnd));
            }
        }

        [Test]
        public void TestGetSnappedThinDamFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var gridExtent = model.GridExtent;

                var center = gridExtent.Centre;
                var snappedThinDam = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ThinDams,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));
                
                Assert.IsTrue(model.SnapsToGrid(snappedThinDam));
            }
        }

        [Test]
        public void TestGetSnappedFixedWeirFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var gridExtent = model.GridExtent;

                var center = gridExtent.Centre;
                var snappedFixedWeir = model.GetGridSnappedGeometry(UnstrucGridOperationApi.FixedWeir,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                Assert.IsTrue(model.SnapsToGrid(snappedFixedWeir));
            }
        }

        [Test]
        public void TestGetSnappedCrossSectionFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var gridExtent = model.GridExtent;

                var center = gridExtent.Centre;
                var snappedCrossSection = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ObsCrossSection,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                Assert.IsTrue(model.SnapsToGrid(snappedCrossSection));
            }
        }

        [Test]
        public void TestGetSnappedWeirFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var gridExtent = model.GridExtent;

                var center = gridExtent.Centre;
                var snappedWeir = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Weir,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                Assert.IsTrue(model.SnapsToGrid(snappedWeir));
            }
        }

        [Test]
        public void TestGetSnappedGateFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var gridExtent = model.GridExtent;

                var center = gridExtent.Centre;
                var snappedGate = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Gate,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                Assert.IsTrue(model.SnapsToGrid(snappedGate));
            }
        }

        [Test]
        public void TestGetSnappedPumpFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var gridExtent = model.GridExtent;

                var center = gridExtent.Centre;
                var snappedPump = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Pump,
                    new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                Assert.IsTrue(model.SnapsToGrid(snappedPump));
            }
        }

        [Test]
        public void TestGetSnappedObservationPointFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var gridExtent = model.GridExtent;

                var center = gridExtent.Centre;
                var snappedPoint = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ObsPoint, new Point(center));
                Assert.IsTrue(model.SnapsToGrid(snappedPoint));
            }
        }

        [Test]
        public void TestGetSnappedWaterLevelBndFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);
                
                var snappedWaterLevelBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.WaterLevelBnd,
                        model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                            .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);
                
                Assert.IsTrue(model.SnapsToGrid(snappedWaterLevelBnd));
            }
        }

        [Test]
        public void TestGetSnappedVelocityBndFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);

                var snappedVelocityBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.VelocityBnd,
                        model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                            .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);

                 Assert.IsTrue(model.SnapsToGrid(snappedVelocityBnd));
            }
        }

        [Test]
        public void TestGetSnappedDischargeBndFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            var localCopy = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localCopy);

            using (var api = new RemoteFlexibleMeshModelApi())
            {
                api.Initialize(model.MduFilePath);
                
                var snappedDischargeBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.DischargeBnd,
                        model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                            .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);

                 Assert.IsTrue(model.SnapsToGrid(snappedDischargeBnd));
            }
        }

        /// <summary>
        /// E-mail from dam_ar:
        /// Na 3 minuten moet station ‘9_040.seg_9’ waterstand 0.0 hebben
        /// Na 3 minuten moet cross sectie ‘weir02’ een discharge van 0.0 hebben
        /// Na 4:100 moet station ‘9_040.seg_9’ waterstand > 0.01 hebben
        /// Na 4:100 moet cross sectie ‘weir02’ een discharge >70 hebben
        /// </summary>
        [Test]
        public void TestRunHarlingen()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(localCopy))
            {
                var obsSeg9 = model.Area.ObservationPoints.First(o => o.Name == "9_040.seg_9");
                var obCrWeir02 = model.Area.ObservationCrossSections.First(o => o.Name == "weir02");

                var obsPointCat = model.GetFeatureCategory(obsSeg9);
                var obsCrossCat = model.GetFeatureCategory(obCrWeir02);

                var report = model.Validate();

                var errorReport = report.ToString();

                Assert.AreEqual(0, report.ErrorCount, errorReport);
                model.Initialize();
                
                var startTime = model.BMIEngine.StartTime;
                var currentTime = model.BMIEngine.CurrentTime;

                var diffTime = currentTime - startTime;

                var threeMinutesChecked = false;

                // keep updating until 5 minutes are reached
                while (diffTime.TotalMinutes < 5)
                {
                    model.Execute();

                    if (diffTime.TotalMinutes >= 4.5)
                    {
                        Assert.GreaterOrEqual(0.01, ((double[])model.GetVar(obsPointCat, obsSeg9.Name, "water_level"))[0]);
                        Assert.GreaterOrEqual(70, ((double[])model.GetVar(obsCrossCat, obCrWeir02.Name, "discharge"))[0]);
                        break;
                    }
                    else if (!threeMinutesChecked && diffTime.TotalMinutes >= 3)
                    {
                        var waterLevel = ((double[])model.GetVar(obsPointCat, obsSeg9.Name, "water_level"))[0];
                        Assert.AreEqual(0.0, waterLevel);
                        var discharge = ((double[])model.GetVar(obsCrossCat, obCrWeir02.Name, "discharge"))[0];
                        Assert.AreEqual(0.0, discharge);

                        threeMinutesChecked = true;
                    }

                    currentTime = model.BMIEngine.CurrentTime;
                    diffTime = currentTime - startTime;
                }
            }
        }
    }
}