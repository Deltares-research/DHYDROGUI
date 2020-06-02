using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.UndoRedo;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.IO;
using NUnit.Framework;
using SharpTestsEx;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroRegionTest
    {
        private static readonly WKTReader wktReader = new WKTReader();

        [Test]
        [Category(TestCategory.Integration)]
        public void LinkCatchmentToHydroNode()
        {
            var node1 = new HydroNode
            {
                Name = "node1",
                Geometry = wktReader.Read("POINT(10 0)")
            };
            var node2 = new HydroNode
            {
                Name = "node2",
                Geometry = wktReader.Read("POINT(0 0)")
            };
            var branch1 = new Channel
            {
                Name = "branch1",
                Source = node1,
                Target = node2,
                Geometry = wktReader.Read("LINESTRING(0 0, 10 0)")
            };
            var network = new HydroNetwork
            {
                Branches = {branch1},
                Nodes =
                {
                    node1,
                    node2
                }
            };

            var catchment = Catchment.CreateDefault();
            var basin = new DrainageBasin {Catchments = {catchment}};

            var region = new HydroRegion
            {
                SubRegions =
                {
                    network,
                    basin
                }
            };

            // catchment -> node1
            catchment.LinkTo(node1);

            // checks
            region.Links.Count
                  .Should().Be.EqualTo(1);

            region.Links[0].Source
                  .Should().Be.SameInstanceAs(catchment);

            region.Links[0].Target
                  .Should().Be.SameInstanceAs(node1);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneWithInternalAndExternalLinks()
        {
            var node1 = new HydroNode {Name = "node1"};
            var node2 = new HydroNode {Name = "node2"};
            var lateralSource = new LateralSource {Name = "lateral1"};
            var channel1 = new Channel
            {
                Name = "channel1",
                Source = node1,
                Target = node2,
                BranchFeatures = {lateralSource}
            };
            var network = new HydroNetwork
            {
                Nodes =
                {
                    node1,
                    node2
                },
                Branches = {channel1}
            };

            var catchment = new Catchment();
            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            var basin = new DrainageBasin
            {
                Catchments = {catchment},
                WasteWaterTreatmentPlants = {wasteWaterTreatmentPlant}
            };

            catchment.LinkTo(wasteWaterTreatmentPlant); // internal link in basin

            var region = new HydroRegion
            {
                SubRegions =
                {
                    network,
                    basin
                }
            };

            catchment.LinkTo(lateralSource); // external link between basin and network

            var regionClone = (HydroRegion) region.Clone();

            var basinClone = (DrainageBasin) region.SubRegions[1];

            basinClone.Links.Count
                      .Should().Be.EqualTo(1);

            regionClone.Links.Count
                       .Should().Be.EqualTo(1);
        }

        [Test]
        public void GetAllRegions()
        {
            var subRegion2 = new HydroRegion();

            var subRegion1 = new HydroRegion {SubRegions = {subRegion2}};
            var subRegion3 = new HydroNetwork();
            var region = new HydroRegion
            {
                SubRegions =
                {
                    subRegion1,
                    subRegion3
                }
            };

            region.AllRegions
                  .Should().Have.SameSequenceAs(new IHydroRegion[]
                  {
                      region,
                      subRegion1,
                      subRegion2,
                      subRegion3
                  });
        }

        [Test]
        public void UnsubscribeFromSubRegions()
        {
            var headRegion = new HydroRegion();
            var newRegionList = new EventedList<IRegion>();
            var newSubRegion = new HydroRegion();

            IEventedList<IRegion> oldSubRegions = headRegion.SubRegions;

            headRegion.SubRegions = newRegionList;

            // asserts
            ((INotifyCollectionChange) headRegion).CollectionChanged += (sender, args) => Assert.Fail("unsubscription failed");

            oldSubRegions.Add(newSubRegion);
        }

        [Test]
        public void RemoveLink()
        {
            var node1 = new HydroNode {Name = "node1"};
            var network = new HydroNetwork {Nodes = {node1}};

            var catchment = new Catchment();
            var basin = new DrainageBasin {Catchments = {catchment}};

            IHydroRegion region = new HydroRegion
            {
                SubRegions =
                {
                    network,
                    basin
                }
            };

            HydroLink link = catchment.LinkTo(node1); // external link between basin and network

            region.RemoveLink(link.Source, link.Target);

            region.Links
                  .Should().Be.Empty();

            catchment.Links
                     .Should().Be.Empty();

            node1.Links
                 .Should().Be.Empty();
        }

        [Test]
        public void RemoveLinkOnFeatureRemove()
        {
            var node1 = new HydroNode {Name = "node1"};
            var network = new HydroNetwork {Nodes = {node1}};

            var catchment = new Catchment();
            var basin = new DrainageBasin {Catchments = {catchment}};

            IHydroRegion region = new HydroRegion
            {
                SubRegions =
                {
                    network,
                    basin
                }
            };

            catchment.LinkTo(node1); // external link between basin and network

            basin.Catchments.Clear(); // should trigger link remove

            region.Links
                  .Should().Be.Empty();

            catchment.Links
                     .Should().Be.Empty();

            node1.Links
                 .Should().Be.Empty();
        }

        [Test]
        public void CanNotLinkItemsOfTwoIndependentRegions()
        {
            var catchment = new Catchment();
            var basin1 = new DrainageBasin {Catchments = {catchment}};

            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            var basin2 = new DrainageBasin {WasteWaterTreatmentPlants = {wasteWaterTreatmentPlant}};

            HydroRegion.CanLinkTo(catchment, wasteWaterTreatmentPlant)
                       .Should().Be.False();
        }

        [Test]
        public void LinkCatchmentToPlantShouldCreateLinkInDrainageBasin()
        {
            var catchment = new Catchment();
            var plant = new WasteWaterTreatmentPlant();
            var basin = new DrainageBasin
            {
                Catchments = {catchment},
                WasteWaterTreatmentPlants = {plant}
            };
            var region = new HydroRegion {SubRegions = {basin}}; // note that basin is wrapped in hydro region 

            HydroLink link = catchment.LinkTo(plant);

            region.Links
                  .Should().Be.Empty();

            basin.Links
                 .Should().Have.SameSequenceAs(new[]
                 {
                     link
                 });
        }

        [Test]
        [Category(TestCategory.UndoRedo)]
        public void UndoRemoveLink()
        {
            var lateralSource = new LateralSource();
            var node1 = new HydroNode {Name = "node1"};
            var branch1 = new Channel
            {
                Name = "channel1",
                Source = node1,
                Target = node1,
                BranchFeatures = {lateralSource}
            };
            var network = new HydroNetwork
            {
                Nodes = {node1},
                Branches = {branch1}
            };

            var catchment = new Catchment();
            var basin = new DrainageBasin {Catchments = {catchment}};

            IHydroRegion region = new HydroRegion
            {
                SubRegions =
                {
                    network,
                    basin
                }
            };

            HydroLink link = catchment.LinkTo(lateralSource); // external link between basin and network

            using (var undoRedo = new UndoRedoManager(region))
            {
                region.RemoveLink(link.Source, link.Target);

                undoRedo.Undo();

                region.Links.Should().Contain(link);

                catchment.Links.Should().Contain(link);

                lateralSource.Links.Should().Contain(link);
            }
        }

        [TestFixture]
        public class CanLinkHydroNetworkToDrainageBasin
        {
            private IHydroRegion region;

            private HydroNode node1;
            private HydroNode node2;
            private LateralSource lateralSource;
            private Channel branch1;

            private Catchment catchment;
            private WasteWaterTreatmentPlant wasteWaterTreatmentPlant;

            [SetUp]
            public void SetUp()
            {
                node1 = new HydroNode();
                node2 = new HydroNode();
                lateralSource = new LateralSource();
                branch1 = new Channel
                {
                    Source = node1,
                    Target = node2,
                    BranchFeatures = {lateralSource}
                };
                var network = new HydroNetwork
                {
                    Branches = {branch1},
                    Nodes =
                    {
                        node1,
                        node2
                    }
                };

                catchment = new Catchment();
                wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
                var basin = new DrainageBasin
                {
                    Catchments = {catchment},
                    WasteWaterTreatmentPlants = {wasteWaterTreatmentPlant}
                };

                region = new HydroRegion
                {
                    SubRegions =
                    {
                        network,
                        basin
                    }
                };
            }

            [Test]
            public void CanLinkCatchmentToNode()
            {
                region.CanLinkTo(catchment, node1).Should().Be.True();
            }

            [Test]
            public void CanLinkCatchmentToWasteWaterTreatmentPlant()
            {
                region.CanLinkTo(catchment, wasteWaterTreatmentPlant).Should().Be.True();
            }

            [Test]
            public void CanLinkCatchmentToLateralSource()
            {
                region.CanLinkTo(catchment, lateralSource).Should().Be.True();
            }

            [Test]
            public void CanNotLinkCatchmentToBranch()
            {
                region.CanLinkTo(catchment, branch1).Should().Be.False();
            }

            [Test]
            public void CanNotLinkCatchmentToItself()
            {
                region.CanLinkTo(catchment, catchment).Should().Be.False();
            }
        }
    }
}