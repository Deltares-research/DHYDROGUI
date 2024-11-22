﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Dimr;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class FlexibleMeshModelDllTest
    {
        [OneTimeSetUp]
        public void SetMapCoordinateSystemFactory()
        {
            if(Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory =new OgrCoordinateSystemFactory(); 
        }

        private void DoWithLocalModelVersion(string mduPath, Action<WaterFlowFMModel> action)
        {
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(localCopy)
            {
                WorkingDirectoryPathFunc = () => TestHelper.GetTestWorkingDirectory(TestHelper.GetCurrentMethodName())
            })
            {
                action?.Invoke(model);
            }

            FileUtils.DeleteIfExists(localCopy);
        }

        [Test]
        public void TestCallInitialize()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");

            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);
                }
            });
        }

        [Test]
        public void TestDimrRunLogIsRetrieved()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                model.Initialize();
                model.Execute();
                model.Finish();
                model.Cleanup();

                var dimrLogDataItem = model.DataItems.FirstOrDefault(di => di.Tag == DimrRunHelper.DimrRunLogfileDataItemTag);
                Assert.NotNull(dimrLogDataItem, "DimrRunLog not retrieved after model run, check DimrRunner.DIMR_RUN_LOGFILE_NAME");
                Assert.NotNull(dimrLogDataItem.Value, "DimrRunLog not retrieved after model run, check DimrRunner.DIMR_RUN_LOGFILE_NAME");
            });
        }

        [Test]
        public void TestCallGetVariableNames()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = FlexibleMeshModelApiFactory.CreateNew())
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
            });
        }

        [Test]
        public void TestCallGetVariableLocations()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = FlexibleMeshModelApiFactory.CreateNew())
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
                        if (location != "") namesAndLocations.Add(variable, location); // ignore those variables without a location
                    }

                    api.Update();

                    foreach (var variableNameAndLocation in namesAndLocations)
                    {
                        var location = api.GetVariableLocation(variableNameAndLocation.Key);
                        Assert.AreEqual(variableNameAndLocation.Value, location, "FlexibleMeshModelApi should return the same Variable location for each call");
                    }
                }
            });
        }

        [Test]
        public void TestCallGetValuesWaterLevelsCount()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                model.Initialize();

                var waterLevels = model.GetVar("s0") as double[];

                Assert.IsNotNull(waterLevels);
                Assert.AreEqual(1, waterLevels.Length); //dimr getvar can only get 1 value!
            });
        }

        [Test]
        public void TestCallSetValuesWaterLevels()
        {
            var mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                model.Initialize();
                var waterLevels = model.GetVar("s0") as double[];

                Assert.IsNotNull(waterLevels);

                var newWaterLevels = waterLevels.Select(s => 1.5 * (s + 1)).ToArray();

                model.SetVar(newWaterLevels, "s0");

                waterLevels = model.GetVar("s0") as double[];

                Assert.IsNotNull(waterLevels);

                Assert.AreEqual(waterLevels, newWaterLevels);
            });
        }

        [Test]
        public void TestGetObservationPointWaterLevel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                model.Initialize();
                // get 13
                var pump = model.Area.ObservationPoints.First(o => o.Name == "13");

                var cat = model.GetFeatureCategory(pump);
                var result = model.GetVar(cat, pump.Name, "water_level");

                // the water level should be found on an observation point, so it is not NaN
                Assert.AreEqual(0.0, ((double[])result)[0]);
            });
        }

        [Test]
        public void TestWriteNetGeomFileHarlingen()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    api.WriteNetGeometry("netgeom.nc");

                    Assert.IsTrue(File.Exists(Path.Combine(Path.GetDirectoryName(model.MduFilePath), "netgeom.nc")));
                }
            });
        }
        
        [Test]
        public void TestGetSnappedFeaturesWorksAfterFailure()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                // Defined test geometries
                var thinDamGeom1 = new LineString(new[]
                    {center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0)});
                var thinDamGeom2 = new LineString(new[]
                    {center.CoordinateValue, new Coordinate(center.X + 10.0, center.Y + 10.0)});

                // Snap a feature first to ensure it works.
                /* Preparation for mocking */
                var xin = new List<double>();
                var yin = new List<double>();
                double[] xout = new double[0], yout = new double[0];
                double MissingValue = -999.0;
                int[] featureIds = new int[0];
                //Set the mockup so the first geometry will return an error but not the second one.
                foreach (var coord in thinDamGeom1.Coordinates)
                {
                    xin.Add(coord.X);
                    yin.Add(coord.Y);
                }

                // no separators for point geometries (obs points):
                if (thinDamGeom1.Coordinates.Length != 1)
                {
                    xin.Add(MissingValue);
                    yin.Add(MissingValue);
                }
                /**/
                var fmModelApi = Substitute.For<IFlexibleMeshModelApi>();
                fmModelApi.GetSnappedFeature(UnstrucGridOperationApi.ThinDams, xin.ToArray(), yin.ToArray(), ref xout, ref yout, ref featureIds).Returns(false);

                //The inner calls will trigger the mocked api
                var uGridApi = new UnstrucGridOperationApi(fmModelApi);

                // Try to snap with a 'failed' process.
                var snappedThinDamGeometries = uGridApi.GetGridSnappedGeometry(UnstrucGridOperationApi.ThinDams, new[] { thinDamGeom1, thinDamGeom2 }).ToList();

                //If it returns the same geometry means nothing has actually been snapped.
                Assert.AreEqual(thinDamGeom1, snappedThinDamGeometries.First());
                Assert.AreNotEqual(thinDamGeom1, snappedThinDamGeometries.Last());
            });
        }

        [Test]
        public void TestGetSnappedThinDamFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var gridExtent = model.GridExtent;

                    var center = gridExtent.Centre;
                    var snappedThinDam = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ThinDams,
                        new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                    Assert.IsTrue(model.SnapsToGrid(snappedThinDam));
                }
            });
        }

        [Test]
        public void TestGetSnappedFixedWeirFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var gridExtent = model.GridExtent;

                    var center = gridExtent.Centre;
                    var snappedFixedWeir = model.GetGridSnappedGeometry(UnstrucGridOperationApi.FixedWeir,
                        new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                    Assert.IsTrue(model.SnapsToGrid(snappedFixedWeir));
                }
            });
        }

        [Test]
        public void TestGetSnappedLeveeBreachkFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var gridExtent = model.GridExtent;

                    var center = gridExtent.Centre;
                    var snappedLeveeBreach = model.GetGridSnappedGeometry(
                        UnstrucGridOperationApi.LeveeBreach,
                        new List<IGeometry>() {
                            new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 1000.0, center.Y + 1000.0) }),
                            new Point(new Coordinate(center.X + 500.0, center.Y + 500.0))}
                    );
                    Assert.AreEqual(2, snappedLeveeBreach.Count());
                    var snappedLeveeGeometry = snappedLeveeBreach.First() as ILineString;
                    var snappedBreachGeometry = snappedLeveeBreach.Last() as IPoint;

                    Assert.IsNotNull(snappedLeveeGeometry);
                    Assert.IsNotNull(snappedBreachGeometry);
                }
            });
        }

        [Test]
        public void TestGetSnappedCrossSectionFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var gridExtent = model.GridExtent;

                    var center = gridExtent.Centre;
                    var snappedCrossSection = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ObsCrossSection,
                        new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                    Assert.IsTrue(model.SnapsToGrid(snappedCrossSection));
                }
            });
        }

        [Test]
        public void TestGetSnappedWeirFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var gridExtent = model.GridExtent;

                    var center = gridExtent.Centre;
                    var snappedWeir = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Weir,
                        new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                    Assert.IsTrue(model.SnapsToGrid(snappedWeir));
                }
            });
        }

        [Test]
        public void TestGetSnappedGateFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var gridExtent = model.GridExtent;

                    var center = gridExtent.Centre;
                    var snappedGate = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Gate,
                        new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                    Assert.IsTrue(model.SnapsToGrid(snappedGate));
                }
            });
        }

        [Test]
        public void TestGetSnappedPumpFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var gridExtent = model.GridExtent;

                    var center = gridExtent.Centre;
                    var snappedPump = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Pump,
                        new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) }));

                    Assert.IsTrue(model.SnapsToGrid(snappedPump));
                }
            });
        }
        
        [Test]
        public void TestGetSnappedObservationPointFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var gridExtent = model.GridExtent;

                    var center = gridExtent.Centre;
                    var snappedPoint = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ObsPoint, new Point(center));
                    Assert.IsTrue(model.SnapsToGrid(snappedPoint));
                }
            });
        }

        [Test]
        public void TestGetSnappedWaterLevelBndFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var snappedWaterLevelBnd =
                        model.GetGridSnappedGeometry(UnstrucGridOperationApi.WaterLevelBnd,
                            model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);

                    Assert.IsTrue(model.SnapsToGrid(snappedWaterLevelBnd));
                }
            });
        }

        [Test]
        public void TestGetSnappedVelocityBndFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var snappedVelocityBnd =
                        model.GetGridSnappedGeometry(UnstrucGridOperationApi.VelocityBnd,
                            model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);

                    Assert.IsTrue(model.SnapsToGrid(snappedVelocityBnd));
                }
            });
        }

        [Test]
        public void TestGetSnappedDischargeBndFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    var snappedDischargeBnd =
                        model.GetGridSnappedGeometry(UnstrucGridOperationApi.DischargeBnd,
                            model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);

                    Assert.IsTrue(model.SnapsToGrid(snappedDischargeBnd));
                }
            });
        }

        [Test]
        public void TestGetSnappedSourceSinkFeature()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            DoWithLocalModelVersion(mduPath, (model) =>
            {
                using (var api = new RemoteFlexibleMeshModelApi())
                {
                    api.Initialize(model.MduFilePath);

                    Assert.True(model.Grid.Cells.Count > 1);
                    var geometry = new LineString(new[]
                    {
                        model.Grid.Cells.First().Center,
                        model.Grid.Cells.Last().Center
                    });

                    model.SourcesAndSinks.Add(new SourceAndSink { Feature = new Feature2D { Geometry = geometry } });

                    var snappedSourceAndSink =
                        model.GetGridSnappedGeometry(UnstrucGridOperationApi.SourceSink,
                            model.SourcesAndSinks.First().Feature.Geometry);

                    Assert.IsTrue(model.SnapsToGrid(snappedSourceAndSink));
                }
            });
        }
    }
}