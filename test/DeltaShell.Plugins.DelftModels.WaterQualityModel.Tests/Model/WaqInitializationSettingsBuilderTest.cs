using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaqInitializationSettingsBuilderTest
    {
        [Test]
        public void BuildSettingsForModelWithoutHydFile()
        {
            var model = new WaterQualityModel();

            var notSupportedException = Assert.Throws<NotSupportedException>(() => WaqInitializationSettingsBuilder.BuildWaqInitializationSettings(model));
            Assert.AreEqual(notSupportedException.Message, "Can not create initialization settings : no hydro dynamica specified.");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void BuildSettingsForModelWithHydFile()
        {
            string commonPath = TestHelper.GetTestDataDirectory();

            var hydFileData = new HydFileData
            {
                AttributesRelativePath = Path.Combine("IO", "attribute files", "allActive_4x4.atr"),
                AreasRelativePath = "AreasRelativePath",
                VolumesRelativePath = "VolumesRelativePath",
                PointersRelativePath = "PointersRelativePath",
                FlowsRelativePath = "FlowsRelativePath",
                LengthsRelativePath = "LengthsRelativePath",
                SurfacesRelativePath = "SurfacesRelativePath",
                VerticalDiffusionRelativePath = "VerticalDiffusionRelativePath",
                GridRelativePath = "GridFilePath",
                BoundariesRelativePath = "",
                NumberOfHydrodynamicLayers = 4,
                HydrodynamicLayerThicknesses = new[]
                {
                    0.25,
                    0.25,
                    0.25,
                    0.25
                },
                NumberOfWaqSegmentLayers = 4,
                NumberOfHydrodynamicLayersPerWaqSegmentLayer = new[]
                {
                    1,
                    1,
                    1,
                    1
                }
            };

            var hydroData = new TestHydroDataStub(hydFileData)
            {
                LayerType = LayerType.Sigma,
                Zbot = 1.0,
                Ztop = 0.0
            };

            var model = new WaterQualityModel
            {
                StartTime = DateTime.Now,
                StopTime = DateTime.Now.AddDays(2),
                TimeStep = new TimeSpan(0, 1, 0, 0),
                UseRestart = true
            };

            model.ImportHydroData(hydroData);

            model.SubstanceProcessLibrary.Substances.AddRange(new[]
            {
                new WaterQualitySubstance
                {
                    Name = "B",
                    ConcentrationUnit = "b/c",
                    Active = false
                },
                new WaterQualitySubstance
                {
                    Name = "A",
                    ConcentrationUnit = "a/c",
                    Active = true
                }
            });
            model.Loads.AddRange(new[]
            {
                new WaterQualityLoad
                {
                    Name = "load 1",
                    LoadType = "Test",
                    X = 1.1,
                    Y = 2.2,
                    Z = (model.ZBot + model.ZTop) / 2.0,
                    LocationAliases = "measurePoint 1, measurePoint 2 , measurePoint 3"
                },
                new WaterQualityLoad
                {
                    Name = "load 2",
                    LoadType = "Test",
                    X = 9.8,
                    Y = 4.6,
                    Z = (model.ZBot + model.ZTop) / 2.0,
                    LocationAliases = ", measurePoint 2 , measurePoint 3"
                },
                new WaterQualityLoad
                {
                    Name = "load 3",
                    LoadType = "Test",
                    X = 15.6,
                    Y = 12.56,
                    Z = (model.ZBot + model.ZTop) / 2.0,
                    LocationAliases = "measurePoint 1, , measurePoint 3"
                }
            });
            model.LoadsDataManager.CreateNewDataTable("A", "b", "c", "d"); // required to output aliases
            model.ObservationAreas.SetValuesAsLabels(new[]
            {
                WaterQualityObservationAreaCoverage.NoDataLabel,
                "One",
                "Two",
                "Two"
            });
            model.ObservationPoints.AddRange(new[]
            {
                new WaterQualityObservationPoint()
                {
                    Name = "obspoint1",
                    ObservationPointType = ObservationPointType.SinglePoint,
                    X = 1.1,
                    Y = 2.2,
                    Z = (model.ZBot + model.ZTop) / 2.0
                },
                new WaterQualityObservationPoint()
                {
                    Name = "obspoint2",
                    ObservationPointType = ObservationPointType.Average,
                    X = 9.8,
                    Y = 4.6
                },
                new WaterQualityObservationPoint()
                {
                    Name = "obspoint3",
                    ObservationPointType = ObservationPointType.OneOnEachLayer,
                    X = 15.6,
                    Y = 12.56
                }
            });

            // call
            WaqInitializationSettings settings = WaqInitializationSettingsBuilder.BuildWaqInitializationSettings(model);

            // assert
            Assert.AreEqual(Path.Combine(commonPath, hydroData.AttributesRelativePath), settings.AttributesFile);
            Assert.AreEqual(Path.Combine(commonPath, hydFileData.AreasRelativePath), settings.AreasFile);
            Assert.AreEqual(Path.Combine(commonPath, hydFileData.VolumesRelativePath), settings.VolumesFile);
            Assert.AreEqual(Path.Combine(commonPath, hydFileData.PointersRelativePath), settings.PointersFile);
            Assert.AreEqual(Path.Combine(commonPath, hydFileData.FlowsRelativePath), settings.FlowsFile);
            Assert.AreEqual(Path.Combine(commonPath, hydFileData.LengthsRelativePath), settings.LengthsFile);
            Assert.AreEqual(Path.Combine(commonPath, hydFileData.SurfacesRelativePath), settings.SurfacesFile);
            Assert.AreEqual(Path.Combine(commonPath, hydFileData.VerticalDiffusionRelativePath), settings.VerticalDiffusionFile);
            Assert.AreEqual(Path.Combine(commonPath, hydFileData.GridRelativePath), settings.GridFile);

            Assert.AreEqual(hydroData.NumberOfWaqSegmentLayers, settings.NumberOfLayers);
            Assert.AreEqual(hydroData.NumberOfDelwaqSegmentsPerHydrodynamicLayer, settings.SegmentsPerLayer);
            Assert.AreEqual(hydroData.NumberOfHorizontalExchanges, settings.HorizontalExchanges);
            Assert.AreEqual(hydroData.NumberOfVerticalExchanges, settings.VerticalExchanges);
            Assert.IsTrue(settings.UseAdditionalVerticalDiffusion);

            Assert.AreEqual(model.ModelSettings.WorkDirectory, settings.ModelWorkDirectory);
            Assert.AreSame(model.BoundaryDataManager, settings.BoundaryDataManager);
            Assert.AreSame(model.LoadsDataManager, settings.LoadsDataManager);
            Assert.AreEqual(model.Loads.Count, settings.LoadAndIds.Count);
            for (var i = 0; i < model.Loads.Count; i++)
            {
                WaterQualityLoad load = model.Loads[i];
                Assert.IsTrue(settings.LoadAndIds.Keys.Contains(load));
                Assert.AreEqual(model.GetSegmentIndexForLocation(load.Geometry.Coordinate), settings.LoadAndIds[load]);
            }

            var expectedObservationPoints = 2; // Two observation areas
            for (var i = 0; i < model.ObservationPoints.Count; i++)
            {
                WaterQualityObservationPoint observationPoint = model.ObservationPoints[i];

                switch (observationPoint.ObservationPointType)
                {
                    case ObservationPointType.SinglePoint:
                    {
                        Assert.IsTrue(settings.OutputLocations.ContainsKey(observationPoint.Name));
                        Assert.AreEqual(model.GetSegmentIndexForLocation(observationPoint.Geometry.Coordinate), settings.OutputLocations[observationPoint.Name][0]);

                        expectedObservationPoints++;
                    }
                        break;
                    case ObservationPointType.Average:
                    {
                        Assert.IsTrue(settings.OutputLocations.ContainsKey(observationPoint.Name));
                        Assert.AreEqual(model.NumberOfWaqSegmentLayers, settings.OutputLocations[observationPoint.Name].Count);
                        IEnumerable<int> expectedIDs = Enumerable.Range(0, model.NumberOfWaqSegmentLayers)
                                                                 .Select(layerNr => 1 + (layerNr * model.NumberOfDelwaqSegmentsPerHydrodynamicLayer));
                        CollectionAssert.AreEquivalent(expectedIDs, settings.OutputLocations[observationPoint.Name]);

                        expectedObservationPoints++;
                    }
                        break;
                    case ObservationPointType.OneOnEachLayer:
                    {
                        int[] expectedIDs = Enumerable.Range(0, model.NumberOfWaqSegmentLayers)
                                                      .Select(layerNr => 4 + (layerNr * model.NumberOfDelwaqSegmentsPerHydrodynamicLayer)).ToArray();
                        for (var l = 0; l < model.NumberOfWaqSegmentLayers; l++)
                        {
                            string obsName = string.Format("{0}_L{1}", observationPoint.Name, l + 1);
                            CollectionAssert.Contains(settings.OutputLocations.Keys, obsName,
                                                      "Couldn't find " + obsName);
                            Assert.AreEqual(expectedIDs[l], settings.OutputLocations[obsName][0]);
                        }

                        expectedObservationPoints += model.NumberOfWaqSegmentLayers;
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Assert.AreEqual(expectedObservationPoints, settings.OutputLocations.Count);
            CollectionAssert.AreEqual(new[]
            {
                2,
                6,
                10,
                14
            }, settings.OutputLocations["one"], "Observation Area 'One' should be defined for all layers.");
            CollectionAssert.AreEqual(new[]
            {
                3,
                4,
                7,
                8,
                11,
                12,
                15,
                16
            }, settings.OutputLocations["two"], "Observation Area 'Two' should be defined for all layers.");

            Assert.IsTrue(settings.LoadsAliases.ContainsKey("measurePoint 1"));
            Assert.IsTrue(settings.LoadsAliases.ContainsKey("measurePoint 2"));
            Assert.IsTrue(settings.LoadsAliases.ContainsKey("measurePoint 3"));

            CollectionAssert.AreEquivalent(new[]
            {
                "load 1",
                "load 3"
            }, settings.LoadsAliases["measurePoint 1"]);
            CollectionAssert.AreEquivalent(new[]
            {
                "load 1",
                "load 2"
            }, settings.LoadsAliases["measurePoint 2"]);
            CollectionAssert.AreEquivalent(new[]
            {
                "load 1",
                "load 2",
                "load 3"
            }, settings.LoadsAliases["measurePoint 3"]);
        }

        [Test]
        public void BuildLoadsWithoutAliases()
        {
            var hydFileData = new HydFileData
            {
                AttributesRelativePath = Path.Combine("IO", "attribute files", "allActive_4x4.atr"),
                AreasRelativePath = "AreasRelativePath",
                VolumesRelativePath = "VolumesRelativePath",
                PointersRelativePath = "PointersRelativePath",
                FlowsRelativePath = "FlowsRelativePath",
                LengthsRelativePath = "LengthsRelativePath",
                SurfacesRelativePath = "SurfacesRelativePath",
                GridRelativePath = "GridRelativePath",
                BoundariesRelativePath = "",
                NumberOfHydrodynamicLayers = 4,
                HydrodynamicLayerThicknesses = new[]
                {
                    0.25,
                    0.25,
                    0.25,
                    0.25
                },
                NumberOfWaqSegmentLayers = 4,
                NumberOfHydrodynamicLayersPerWaqSegmentLayer = new[]
                {
                    1,
                    1,
                    1,
                    1
                }
            };

            var hydroData = new TestHydroDataStub(hydFileData)
            {
                LayerType = LayerType.Sigma,
                Zbot = 1.0,
                Ztop = 0.0
            };

            var model = new WaterQualityModel
            {
                StartTime = DateTime.Now,
                StopTime = DateTime.Now.AddDays(2),
                TimeStep = new TimeSpan(0, 1, 0, 0),
                UseRestart = true
            };
            //model.ModelSettings.WorkDirectory = Path.Combine(Directory.GetCurrentDirectory(), "mdl");

            model.ImportHydroData(hydroData);

            model.SubstanceProcessLibrary.Substances.AddRange(new[]
            {
                new WaterQualitySubstance
                {
                    Name = "B",
                    Active = false
                },
                new WaterQualitySubstance
                {
                    Name = "A",
                    Active = true
                }
            });
            model.Loads.AddRange(new[]
            {
                new WaterQualityLoad
                {
                    Name = "load 1",
                    LoadType = "Test",
                    X = 1.1,
                    Y = 2.2,
                    Z = (model.ZBot + model.ZTop) / 2.0
                }
            });
            model.LoadsDataManager.CreateNewDataTable("myloads", "A", "b.usefor", "use");

            WaqInitializationSettings settings = WaqInitializationSettingsBuilder.BuildWaqInitializationSettings(model);

            Assert.IsTrue(settings.LoadsAliases.ContainsKey("load 1"));
            Assert.AreEqual(1, settings.LoadsAliases["load 1"].Count);
            Assert.AreEqual("load 1", settings.LoadsAliases["load 1"][0]);

            // aliases are only written when there is a datatable specified. Else, delwaq will crash.
            Assert.AreEqual(0, settings.BoundaryAliases.Count);
        }
    }
}