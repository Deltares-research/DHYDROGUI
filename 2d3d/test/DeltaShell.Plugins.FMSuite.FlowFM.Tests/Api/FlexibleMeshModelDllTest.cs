using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Remoting;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class FlexibleMeshModelDllTest
    {
        [Test]
        public void TestCallInitialize()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);
            }
        }

        [Test]
        public void TestDimrRunLogIsRetrieved()
        {
            string mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel())
            {
                model.ImportFromMdu(localCopy);

                // In order for this test to succeed, we need to manually set the Crest Width to anything greater than 0.
                // This is due to the structures file (har_structures.ini) not containing values for Crest Width.
                // The Gui will initialize the Crest Width with a default value of 0.0, whilst the computational core will initialize with the default length of the structure.
                // Since this test is not meant to test the CrestWidth getting and setting, we place a hack here to set all the Crest Widths to any positive value.
                model.Area.Structures.Select(c =>
                {
                    c.CrestWidth = 1.0;
                    return c;
                }).ToList();
                model.Initialize();
                model.Execute();
                model.Finish();
                model.Cleanup();

                Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned));

                IDataItem dimrLogDataItem = model.DataItems.FirstOrDefault(di => di.Tag == DimrRunHelper.dimrRunLogfileDataItemTag);
                Assert.NotNull(dimrLogDataItem, "DimrRunLog not retrieved after model run, check DimrRunner.DIMR_RUN_LOGFILE_NAME");
                Assert.NotNull(dimrLogDataItem.Value, "DimrRunLog not retrieved after model run, check DimrRunner.DIMR_RUN_LOGFILE_NAME");
            }
        }

        [Test]
        public void TestCallGetVariableNames()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            using (IFlexibleMeshModelApi api = FlexibleMeshModelApiFactory.CreateNew())
            {
                api.Initialize(model.MduFilePath);

                string[] variableNames1 = api.VariableNames;
                Assert.NotNull(variableNames1, "FlexibleMeshModelApi should return an array of names");

                api.Update();

                string[] variableNames2 = api.VariableNames;
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
            string mduPath =
                TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            using (IFlexibleMeshModelApi api = FlexibleMeshModelApiFactory.CreateNew())
            {
                api.Initialize(model.MduFilePath);

                string[] variableNames = api.VariableNames;
                Assert.NotNull(variableNames, "FlexibleMeshModelApi should return an array of names");
                Assert.IsTrue(variableNames.Length > 0);

                var namesAndLocations = new Dictionary<string, string>();
                foreach (string variable in variableNames)
                {
                    string location = api.GetVariableLocation(variable);
                    Assert.NotNull(location, string.Format("FlexibleMeshModelApi should return a location for Variable {0}", variable));
                    if (location != "")
                    {
                        namesAndLocations.Add(variable, location); // ignore those variables without a location
                    }
                }

                api.Update();

                foreach (KeyValuePair<string, string> variableNameAndLocation in namesAndLocations)
                {
                    string location = api.GetVariableLocation(variableNameAndLocation.Key);
                    Assert.AreEqual(variableNameAndLocation.Value, location, "FlexibleMeshModelApi should return the same Variable location for each call");
                }
            }
        }

        [Test]
        public void TestCallGetValuePumpCapacity()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            try
            {
                // In order for this test to succeed, we need to manually set the Crest Width to anything greater than 0.
                // This is due to the structures file (har_structures.ini) not containing values for Crest Width.
                // The Gui will initialize the Crest Width with a default value of 0.0, whilst the computational core will initialize with the default length of the structure.
                // Since this test is not meant to test the CrestWidth getting and setting, we place a hack here to set all the Crest Widths to any positive value.
                model.Area.Structures.Select(c =>
                {
                    c.CrestWidth = 1.0;
                    return c;
                }).ToList();
                model.Initialize();
                IPump pump = model.Area.Pumps.First(o => o.Name == "pump01");

                string cat = model.GetFeatureCategory(pump);
                Array result = model.GetVar(cat, pump.Name, "capacity");

                Assert.AreEqual(100.0, ((double[])result)[0]);

                model.Execute();
                result = model.GetVar(cat, pump.Name, "capacity");
                Assert.AreEqual(94.999999979045242, ((double[])result)[0]);
            }
            finally
            {
                model.Cleanup();
                model.Dispose();
            }
        }

        [Test]
        public void TestCallSetValueWeirCrestLevel()
        {
            string mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            try
            {
                // In order for this test to succeed, we need to manually set the Crest Width to anything greater than 0.
                // This is due to the structures file (har_structures.ini) not containing values for Crest Width.
                // The Gui will initialize the Crest Width with a default value of 0.0, whilst the computational core will initialize with the default length of the structure.
                // Since this test is not meant to test the CrestWidth getting and setting, we place a hack here to set all the Crest Widths to any positive value.
                model.Area.Structures.Select(c =>
                {
                    c.CrestWidth = 1.0;
                    return c;
                }).ToList();
                model.Initialize();
                // get weir02
                IStructure weir = model.Area.Structures.First(o => o.Name == "weir02");

                string cat = model.GetFeatureCategory(weir);
                Array result = model.GetVar(cat, weir.Name, KnownStructureProperties.CrestLevel);

                Assert.AreEqual(3.0, ((double[])result)[0]);

                model.SetVar(new[]
                {
                    -3.0
                }, cat, weir.Name, KnownStructureProperties.CrestLevel);
                result = model.GetVar(cat, weir.Name, KnownStructureProperties.CrestLevel);
                Assert.AreEqual(-3.0, ((double[])result)[0]);
            }
            finally
            {
                model.Cleanup();
                model.Dispose();
            }
        }

        [Test]
        public void TestCallGetValuesWaterLevelsCount()
        {
            string mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            try
            {
                model.Initialize();

                var waterLevels = model.GetVar("s0") as double[];

                Assert.IsNotNull(waterLevels);

                Assert.AreEqual(1, waterLevels.Length); //dimr getvar can only get 1 value!
            }
            finally
            {
                model.Cleanup();
                model.Dispose();
            }
        }

        [Test]
        public void TestCallSetValuesWaterLevels()
        {
            string mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            try
            {
                model.Initialize();

                var waterLevels = model.GetVar("s0") as double[];

                Assert.IsNotNull(waterLevels);

                double[] newWaterLevels = waterLevels.Select(s => 1.5 * (s + 1)).ToArray();

                model.SetVar(newWaterLevels, "s0");

                waterLevels = model.GetVar("s0") as double[];

                Assert.IsNotNull(waterLevels);

                Assert.AreEqual(waterLevels, newWaterLevels);
            }
            finally
            {
                model.Cleanup();
                model.Dispose();
            }
        }

        [Test]
        public void TestGetObservationPointWaterLevel()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            try
            {
                model.Initialize();
                // get 13
                GroupableFeature2DPoint pump = model.Area.ObservationPoints.First(o => o.Name == "13");

                string cat = model.GetFeatureCategory(pump);
                Array result = model.GetVar(cat, pump.Name, "water_level");

                // the water level should be found on an observation point, so it is not NaN
                Assert.AreEqual(0.0, ((double[])result)[0]);
            }
            finally
            {
                model.Cleanup();
                model.Dispose();
            }
        }

        [Test]
        public void TestWriteNetGeomFileHarlingen()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);
                remoteApi.WriteNetGeometry("netgeom.nc");

                Assert.IsTrue(File.Exists(Path.Combine(model.GetMduDirectory(), "netgeom.nc")));
            }
        }

        [Test]
        public void TestGetSnappedFeatures()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                Envelope gridExtent = model.GridExtent;

                Coordinate center = gridExtent.Centre;
                IGeometry snappedPoint = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ObsPoint, new Point(center));
                IGeometry snappedThinDam = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ThinDams,
                                                                        new LineString(new[]
                                                                        {
                                                                            center.CoordinateValue,
                                                                            new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                        }));
                IGeometry snappedFixedWeir = model.GetGridSnappedGeometry(UnstrucGridOperationApi.FixedWeir,
                                                                          new LineString(new[]
                                                                          {
                                                                              center.CoordinateValue,
                                                                              new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                          }));
                IGeometry snappedCrossSection = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ObsCrossSection,
                                                                             new LineString(new[]
                                                                             {
                                                                                 center.CoordinateValue,
                                                                                 new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                             }));
                IGeometry snappedWeir = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Weir,
                                                                     new LineString(new[]
                                                                     {
                                                                         center.CoordinateValue,
                                                                         new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                     }));
                IGeometry snappedGate = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Gate,
                                                                     new LineString(new[]
                                                                     {
                                                                         center.CoordinateValue,
                                                                         new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                     }));
                IGeometry snappedPump = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Pump,
                                                                     new LineString(new[]
                                                                     {
                                                                         center.CoordinateValue,
                                                                         new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                     }));

                IGeometry snappedWaterLevelBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.WaterLevelBnd,
                                                 model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                                      .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);
                IGeometry snappedVelocityBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.VelocityBnd,
                                                 model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                                      .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);
                IGeometry snappedDischargeBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.DischargeBnd,
                                                 model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                                      .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);

                Assert.IsTrue(snappedPoint.SnapsToFlowFmGrid(model.GridExtent));
                Assert.IsTrue(snappedThinDam.SnapsToFlowFmGrid(model.GridExtent));
                Assert.IsTrue(snappedFixedWeir.SnapsToFlowFmGrid(model.GridExtent));
                Assert.IsTrue(snappedCrossSection.SnapsToFlowFmGrid(model.GridExtent));
                Assert.IsTrue(snappedWeir.SnapsToFlowFmGrid(model.GridExtent));
                Assert.IsTrue(snappedGate.SnapsToFlowFmGrid(model.GridExtent));
                Assert.IsTrue(snappedPump.SnapsToFlowFmGrid(model.GridExtent));
                Assert.IsTrue(snappedWaterLevelBnd.SnapsToFlowFmGrid(model.GridExtent));
                Assert.IsTrue(snappedVelocityBnd.SnapsToFlowFmGrid(model.GridExtent));
                Assert.IsTrue(snappedDischargeBnd.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedFeaturesWorksAfterFailure()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);
            var mocks = new MockRepository();

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            try
            {
                Envelope gridExtent = model.GridExtent;
                Coordinate center = gridExtent.Centre;
                // Defined test geometries
                var thinDamGeom1 = new LineString(new[]
                {
                    center.CoordinateValue,
                    new Coordinate(center.X + 100.0, center.Y + 100.0)
                });
                var thinDamGeom2 = new LineString(new[]
                {
                    center.CoordinateValue,
                    new Coordinate(center.X + 10.0, center.Y + 10.0)
                });

                // Snap a feature first to ensure it works.
                /* Preparation for mocking */
                var xin = new List<double>();
                var yin = new List<double>();
                double[] xout = new double[0], yout = new double[0];
                double MissingValue = -999.0;
                var featureIds = new int[0];
                //Set the mockup so the first geometry will return an error but not the second one.
                foreach (Coordinate coord in thinDamGeom1.Coordinates)
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

                var fmModelApi = mocks.StrictMock<FlexibleMeshModelApi>();
                fmModelApi
                    .Expect(
                        fma => fma.GetSnappedFeature(UnstrucGridOperationApi.ThinDams, xin.ToArray(), yin.ToArray(),
                                                     ref xout, ref yout, ref featureIds))
                    .Return(false).Repeat.Any();
                //The inner calls will trigger the mocked api
                var mockedUgridApi = mocks.StrictMock<UnstrucGridOperationApi>(fmModelApi);
                mocks.ReplayAll();

                // Try to snap with a 'failed' mocked process.
                List<IGeometry> snappedThinDamGeometries = mockedUgridApi
                                                           .GetGridSnappedGeometry(UnstrucGridOperationApi.ThinDams, new[]
                                                           {
                                                               thinDamGeom1,
                                                               thinDamGeom2
                                                           })
                                                           .ToList();

                //If it returns the same geometry means nothing has actually been snapped.
                Assert.AreEqual(thinDamGeom1, snappedThinDamGeometries.First());
                Assert.AreNotEqual(thinDamGeom1, snappedThinDamGeometries.Last());
            }
            finally
            {
                model.Cleanup();
                model.Dispose();
            }
        }

        [Test]
        public void TestGetSnappedThinDamFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                Envelope gridExtent = model.GridExtent;

                Coordinate center = gridExtent.Centre;
                IGeometry snappedThinDam = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ThinDams,
                                                                        new LineString(new[]
                                                                        {
                                                                            center.CoordinateValue,
                                                                            new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                        }));

                Assert.IsTrue(snappedThinDam.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedFixedWeirFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                Envelope gridExtent = model.GridExtent;

                Coordinate center = gridExtent.Centre;
                IGeometry snappedFixedWeir = model.GetGridSnappedGeometry(UnstrucGridOperationApi.FixedWeir,
                                                                          new LineString(new[]
                                                                          {
                                                                              center.CoordinateValue,
                                                                              new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                          }));

                Assert.IsTrue(snappedFixedWeir.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedCrossSectionFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                Envelope gridExtent = model.GridExtent;

                Coordinate center = gridExtent.Centre;
                IGeometry snappedCrossSection = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ObsCrossSection,
                                                                             new LineString(new[]
                                                                             {
                                                                                 center.CoordinateValue,
                                                                                 new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                             }));

                Assert.IsTrue(snappedCrossSection.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedWeirFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                Envelope gridExtent = model.GridExtent;

                Coordinate center = gridExtent.Centre;
                IGeometry snappedWeir = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Weir,
                                                                     new LineString(new[]
                                                                     {
                                                                         center.CoordinateValue,
                                                                         new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                     }));

                Assert.IsTrue(snappedWeir.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedGateFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                Envelope gridExtent = model.GridExtent;

                Coordinate center = gridExtent.Centre;
                IGeometry snappedGate = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Gate,
                                                                     new LineString(new[]
                                                                     {
                                                                         center.CoordinateValue,
                                                                         new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                     }));

                Assert.IsTrue(snappedGate.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedPumpFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                Envelope gridExtent = model.GridExtent;

                Coordinate center = gridExtent.Centre;
                IGeometry snappedPump = model.GetGridSnappedGeometry(UnstrucGridOperationApi.Pump,
                                                                     new LineString(new[]
                                                                     {
                                                                         center.CoordinateValue,
                                                                         new Coordinate(center.X + 100.0, center.Y + 100.0)
                                                                     }));

                Assert.IsTrue(snappedPump.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedObservationPointFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                Envelope gridExtent = model.GridExtent;

                Coordinate center = gridExtent.Centre;
                IGeometry snappedPoint = model.GetGridSnappedGeometry(UnstrucGridOperationApi.ObsPoint, new Point(center));

                Assert.IsTrue(snappedPoint.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedWaterLevelBndFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                IGeometry snappedWaterLevelBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.WaterLevelBnd,
                                                 model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                                      .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);

                Assert.IsTrue(snappedWaterLevelBnd.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedVelocityBndFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                IGeometry snappedVelocityBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.VelocityBnd,
                                                 model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                                      .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);

                Assert.IsTrue(snappedVelocityBnd.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedDischargeBndFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                IGeometry snappedDischargeBnd =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.DischargeBnd,
                                                 model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                                      .First(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel).Feature.Geometry);

                Assert.IsTrue(snappedDischargeBnd.SnapsToFlowFmGrid(model.GridExtent));
            }
        }

        [Test]
        public void TestGetSnappedSourceSinkFeature()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");

            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            IFlexibleMeshModelApi api = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>();
            using (var remoteApi = new RemoteFlexibleMeshModelApi(api))
            {
                remoteApi.Initialize(model.MduFilePath);

                Assert.True(model.Grid.Cells.Count > 1);
                var geometry = new LineString(new[]
                {
                    model.Grid.Cells.First().Center,
                    model.Grid.Cells.Last().Center
                });

                model.SourcesAndSinks.Add(new SourceAndSink { Feature = new Feature2D { Geometry = geometry } });

                IGeometry snappedSourceAndSink =
                    model.GetGridSnappedGeometry(UnstrucGridOperationApi.SourceSink,
                                                 model.SourcesAndSinks.First().Feature.Geometry);

                Assert.IsTrue(snappedSourceAndSink.SnapsToFlowFmGrid(model.GridExtent));
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
            string mduPath = TestHelper.GetTestFilePath(@"structures_all_types\har.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            try
            {
                // In order for this test to succeed, we need to manually set the Crest Width to anything greater than 0.
                // This is due to the structures file (har_structures.ini) not containing values for Crest Width.
                // The Gui will initialize the Crest Width with a default value of 0.0, whilst the computational core will initialize with the default length of the structure.
                // Since this test is not meant to test the CrestWidth getting and setting, we place a hack here to set all the Crest Widths to any positive value.
                model.Area.Structures.Select(c =>
                {
                    c.CrestWidth = 1.0;
                    return c;
                }).ToList();
                GroupableFeature2DPoint obsSeg9 = model.Area.ObservationPoints.First(o => o.Name == "9_040.seg_9");
                ObservationCrossSection2D obCrWeir02 = model.Area.ObservationCrossSections.First(o => o.Name == "weir02");

                string obsPointCat = model.GetFeatureCategory(obsSeg9);
                string obsCrossCat = model.GetFeatureCategory(obCrWeir02);

                ValidationReport report = model.Validate();

                var errorReport = report.ToString();

                Assert.AreEqual(0, report.ErrorCount, errorReport);
                model.Initialize();

                DateTime startTime = model.BMIEngine.StartTime;
                DateTime currentTime = model.BMIEngine.CurrentTime;

                TimeSpan diffTime = currentTime - startTime;

                var threeMinutesChecked = false;

                // keep updating until 5 minutes are reached
                while (diffTime.TotalMinutes < 5)
                {
                    model.Execute();

                    if (diffTime.TotalMinutes >= 4.5)
                    {
                        Assert.GreaterOrEqual(0.01,
                                              ((double[])model.GetVar(obsPointCat, obsSeg9.Name, "water_level"))[0]);
                        Assert.GreaterOrEqual(70,
                                              ((double[])model.GetVar(obsCrossCat, obCrWeir02.Name, "discharge"))[0]);
                        break;
                    }
                    else if (!threeMinutesChecked && diffTime.TotalMinutes >= 3)
                    {
                        double waterLevel = ((double[])model.GetVar(obsPointCat, obsSeg9.Name, "water_level"))[0];
                        Assert.AreEqual(0.0, waterLevel);
                        double discharge = ((double[])model.GetVar(obsCrossCat, obCrWeir02.Name, "discharge"))[0];
                        Assert.AreEqual(0.0, discharge);

                        threeMinutesChecked = true;
                    }

                    currentTime = model.BMIEngine.CurrentTime;
                    diffTime = currentTime - startTime;
                }
            }
            finally
            {
                model.Cleanup();
                model.Dispose();
            }
        }
    }
}