using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpTestsEx;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroNetworkTest
    {
        [Test]
        public void DeletingDefaultCsSharedDefinitionClearsDefault()
        {
            var hydroNetwork = new HydroNetwork();

            var csDef1 = new CrossSectionDefinitionYZ();
            var csDef2 = new CrossSectionDefinitionYZ();
            var csDef3 = new CrossSectionDefinitionYZ();
            hydroNetwork.SharedCrossSectionDefinitions.Add(csDef1);
            hydroNetwork.SharedCrossSectionDefinitions.Add(csDef2);
            hydroNetwork.SharedCrossSectionDefinitions.Add(csDef3);
            hydroNetwork.DefaultCrossSectionDefinition = csDef2;

            Assert.AreSame(csDef2, hydroNetwork.DefaultCrossSectionDefinition);
            hydroNetwork.SharedCrossSectionDefinitions.Remove(csDef1);
            Assert.AreSame(csDef2, hydroNetwork.DefaultCrossSectionDefinition);
            hydroNetwork.SharedCrossSectionDefinitions.Remove(csDef3);
            Assert.AreSame(csDef2, hydroNetwork.DefaultCrossSectionDefinition);
            hydroNetwork.SharedCrossSectionDefinitions.Remove(csDef2);
            Assert.AreSame(null, hydroNetwork.DefaultCrossSectionDefinition);
        }

        [Test]
        [Ignore("TODO: not working anymore due to refactoring; re-enable later")]
        public void AddCrossSectionToBranchUsingCollections()
        {
            var crossSection = new CrossSection(null);
            var branch = new Channel(new HydroNode("from"), new HydroNode("To"));

            //NetworkHelper.AddBranchFeatureToBranch(branch, crossSection, crossSection.Offset);
            branch.BranchFeatures.Add(crossSection);

            Assert.AreEqual(branch, crossSection.Branch);

            branch.BranchFeatures.Clear();
            Assert.IsNull(crossSection.Branch);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void AddManyBranchesWithCrossSections()
        {
            TestHelper.AssertIsFasterThan(2500,() =>
                                                     {
                                                         const int count = 10000;
                                                         var network = new HydroNetwork();
                                                         for (int i = 0; i < count; i++)
                                                         {
                                                             var from = new HydroNode();
                                                             var to = new HydroNode();

                                                             network.Nodes.Add(from);
                                                             network.Nodes.Add(to);

                                                             var channel = new Channel {Source = from, Target = to};
                                                             HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel,
                                                                                                    new CrossSectionDefinitionXYZ(),
                                                                                                    0);
                                                         }

                                                         // access all CrossSections should be also fast
                                                         network.CrossSections.ToArray();
                                                     });
        }

        [Test]
        [Category(TestCategory.Performance)] // TODO: test Add or change name
        public void AddManyBranchesWithSimpleBranchFeature()
        {
            const int count = 10000;
            int weirCount = 0;

            Action action = delegate // TODO: what are we testing here? Test only add.
                                {
                                    var network = new HydroNetwork();
                                    for (int i = 0; i < count; i++)
                                    {
                                        var from = new HydroNode();
                                        var to = new HydroNode();

                                        network.Nodes.Add(from);
                                        network.Nodes.Add(to);

                                        var channel = new Channel {Source = from, Target = to};

                                        var compositeBranchStructure = new CompositeBranchStructure();
                                        NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, channel, 0);
                                        HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, new Weir());

                                        network.Branches.Add(channel);
                                    }

                                    // access all Weirs should be also fast
                                    weirCount = network.Weirs.Count();
                                };

            TestHelper.AssertIsFasterThan(2750, string.Format("Added {0} branches with {1} weirs", count, weirCount), action);
        }

        [Test]
        public void BranchCrossSectionShouldRaiseCollectionChangedEvent()
        {
            var crossSection = new CrossSectionDefinitionXYZ();
            var branch = new Channel(new HydroNode("from"), new HydroNode("To"));

            int count = 0;
            ((INotifyCollectionChange)branch).CollectionChanged += delegate { count++; };

            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, crossSection, 0.0);
            Assert.AreEqual(1, count);

            branch.BranchFeatures.Clear();
            Assert.AreEqual(2, count);
        }

        [Test]
        public void BubbleBranchEventsViaBranchFeatures()
        {
            var branch = new Channel(new HydroNode("from"), new HydroNode("To"));
            branch.BranchFeatures.Add(new Bridge());
            branch.BranchFeatures[0].Branch = branch;

            var count = 0;
            ((INotifyPropertyChange)branch).PropertyChanged += delegate { count++; };

            branch.Name = "new name";
            branch.BranchFeatures[0].Branch = branch;
            count.Should().Be.EqualTo(2);
        }

        [Test]
        public void CloneRewiresProxyDefinitions()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var sharedDefinition = new CrossSectionDefinitionYZ();
            
            network.SharedCrossSectionDefinitions.Add(sharedDefinition);
            var crossSectionDefinitionProxy = new CrossSectionDefinitionProxy(sharedDefinition);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(network.Branches.First(),
                                                                 crossSectionDefinitionProxy, 10.0);
            
            var clonedNetwork = (HydroNetwork) network.Clone();
            Assert.AreEqual(1, clonedNetwork.SharedCrossSectionDefinitions.Count);
            //check the proxy got rewired
            var crossSectionClone = clonedNetwork.CrossSections.First();
            var clonedProxyDefinition = (CrossSectionDefinitionProxy)crossSectionClone.Definition;
            Assert.AreEqual(clonedProxyDefinition.InnerDefinition, clonedNetwork.SharedCrossSectionDefinitions.First());
        }

        
        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroNetworkWithProxyDefinitions()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var sharedDefinition = new CrossSectionDefinitionYZ();
            network.SharedCrossSectionDefinitions.Add(sharedDefinition);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(network.Channels.First(),
                                                                 new CrossSectionDefinitionProxy(sharedDefinition),
                                                                 10.0d);

            var clone = (HydroNetwork)network.Clone();
            TestReferenceHelper.AssertStringRepresentationOfGraphIsEqual(network, clone);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroNetworkWithData()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            ReflectionTestHelper.FillObjectListPropertiesWithRandomInstances(network);

            var clone = (HydroNetwork)network.Clone();
            
            TestReferenceHelper.AssertStringRepresentationOfGraphIsEqual(network, clone);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroNetworkWithRoutes()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            var route = new Route();
            var routeName = "RouteToBeCloned";
            route.Name = routeName;
            network.Routes.Add(route);

            var clone = (HydroNetwork)network.Clone();

            var lingeringReferences = TestReferenceHelper.SearchObjectInObjectGraph(route.Network, clone);
            lingeringReferences.ForEach(Console.WriteLine);
            Assert.AreEqual(0, lingeringReferences.Count);
            
            Assert.AreEqual(1, clone.Routes.Count);
            
            var clonedRoute = clone.Routes[0];
            Assert.AreEqual(clone, clonedRoute.Network);
            Assert.AreEqual(routeName, clonedRoute.Name);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroNetwork()
        {
            var network = new HydroNetwork();
            var from = new HydroNode();
            var to = new HydroNode();
            network.Nodes.Add(from);
            network.Nodes.Add(to);
            var channel = new Channel {Source = from, Target = to};
            network.Branches.Add(channel);
            network.CrossSectionSectionTypes.Add(new CrossSectionSectionType {Name = "JemigdePemig"});
            var crossSectionSectionTypesCount = network.CrossSectionSectionTypes.Count;
            // The default CrossSectionSectionType and JDP
            Assert.AreEqual(2, crossSectionSectionTypesCount);
            var clonedHydroNetwork = (IHydroNetwork) network.Clone();
            clonedHydroNetwork.GetType().Should().Be.EqualTo(typeof (HydroNetwork));

            clonedHydroNetwork.Branches.Count.Should().Be.EqualTo(1);
            clonedHydroNetwork.Nodes.Count.Should().Be.EqualTo(2);
            clonedHydroNetwork.CrossSectionSectionTypes.Count.Should().Be.EqualTo(crossSectionSectionTypesCount);
            Assert.AreEqual("JemigdePemig", clonedHydroNetwork.CrossSectionSectionTypes.Last().Name);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AutoCloneHydroNetwork()
        {
            var from = new HydroNode();
            var to = new HydroNode();
            var channel = new Channel {Source = from, Target = to};
            var network = new HydroNetwork
                {
                    Branches = {channel},
                    Nodes = {from, to},
                    CrossSectionSectionTypes = {new CrossSectionSectionType {Name = "newType"}}
                };

            var clonedNetwork = (HydroNetwork)network.Clone();

            clonedNetwork.GetType().Should().Be.EqualTo(typeof(HydroNetwork));

            clonedNetwork.Branches.Count.Should().Be.EqualTo(1);
            clonedNetwork.Nodes.Count.Should().Be.EqualTo(2);
            clonedNetwork.CrossSectionSectionTypes.Count.Should().Be.EqualTo(network.CrossSectionSectionTypes.Count);
            Assert.AreEqual("newType", clonedNetwork.CrossSectionSectionTypes.Last().Name);
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroNetworkWithCrossSectionSectionTypes()
        {
            var network = new HydroNetwork();
            var crossSectionSectionType = new CrossSectionSectionType{Name = "Jan"};
            network.CrossSectionSectionTypes.Add(crossSectionSectionType);
            crossSectionSectionType.Id = 666;//debug easy by idd
            var from = new HydroNode();
            var to = new HydroNode();
            network.Nodes.Add(from);
            network.Nodes.Add(to);
            var channel = new Channel { Source = from, Target = to };
            network.Branches.Add(channel);
            var crossSectionXYZ = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0, 0), new Coordinate(10, 0, 0) })
            };

            crossSectionXYZ.Sections.Add(new CrossSectionSection { SectionType = crossSectionSectionType });

            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSectionXYZ, 0);

            var clonedHydroNetwork = (IHydroNetwork)network.Clone();
            clonedHydroNetwork.CrossSections.Should().Have.Count.EqualTo(1);
            var cloneCrossSection = clonedHydroNetwork.CrossSections.FirstOrDefault();
            var clonedType = clonedHydroNetwork.CrossSectionSectionTypes.FirstOrDefault(t => t.Name == "Jan");
            
            //the type should be cloned
            Assert.AreNotEqual(clonedType, crossSectionSectionType);
            //the crosssection reference should be updated to use the cloned type
            Assert.AreEqual(clonedType, cloneCrossSection.Definition.Sections[0].SectionType);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroNetworkWithVariousBranchFeatures()
        {
            var network = new HydroNetwork();
            var from = new HydroNode();
            var to = new HydroNode();
            network.Nodes.Add(from);
            network.Nodes.Add(to);
            var channel = new Channel {Source = from, Target = to};
            network.Branches.Add(channel);
            var compositeBranchStructure = new CompositeBranchStructure();
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, channel, 0);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, new Weir());
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, new Gate());
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, new Pump());

            var crossSectionXYZ = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0, 0), new Coordinate(10, 0, 0) })
            };
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSectionXYZ, 0);

            var clonedHydroNetwork = (IHydroNetwork) network.Clone();
            clonedHydroNetwork.CrossSections.Should().Have.Count.EqualTo(1);
            clonedHydroNetwork.CompositeBranchStructures.Should().Have.Count.EqualTo(1);
            clonedHydroNetwork.Weirs.Should().Have.Count.EqualTo(1);
            clonedHydroNetwork.Pumps.Should().Have.Count.EqualTo(1);
            clonedHydroNetwork.Gates.Should().Have.Count.EqualTo(1);
            clonedHydroNetwork.Pumps.First().Should().Not.Be.SameInstanceAs(network.Pumps.First());
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void AutoCloneHydroNetworkWithVariousBranchFeatures()
        {
            var network = new HydroNetwork();
            var from = new HydroNode();
            var to = new HydroNode();
            network.Nodes.Add(from);
            network.Nodes.Add(to);
            var channel = new Channel { Source = from, Target = to };
            network.Branches.Add(channel);
            var compositeBranchStructure = new CompositeBranchStructure();
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, channel, 0);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, new Weir());
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, new Gate());
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, new Pump());

            var crossSectionXYZ = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0, 1), new Coordinate(10, 0, 1) })
            };
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSectionXYZ, 0);

            var clonedHydroNetwork = (HydroNetwork)network.Clone();

            var hits = TestReferenceHelper.SearchObjectInObjectGraph(clonedHydroNetwork, network);
            hits.ForEach(Console.WriteLine);
            Assert.AreEqual(0, hits.Count);

            clonedHydroNetwork.CrossSections.Should().Have.Count.EqualTo(1);
            clonedHydroNetwork.CompositeBranchStructures.Should().Have.Count.EqualTo(1);
            clonedHydroNetwork.Weirs.Should().Have.Count.EqualTo(1);
            clonedHydroNetwork.Gates.Should().Have.Count.EqualTo(1);
            clonedHydroNetwork.Pumps.Should().Have.Count.EqualTo(1);

            // failing asserts:
            var route = new Route();
            clonedHydroNetwork.Routes.Add(route);
            route.Network.Should().Be.EqualTo(clonedHydroNetwork); //due to missing event resubscription

            // due to no cloning of enumerables
            clonedHydroNetwork.Pumps.First().Should().Not.Be.SameInstanceAs(network.Pumps.First());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroNetworkAndAddBranch()
        {
            var network = new HydroNetwork();
            var from = new HydroNode();
            var to = new HydroNode();
            network.Nodes.Add(from);
            network.Nodes.Add(to);
            var channel = new Channel { Source = from, Target = to };
            network.Branches.Add(channel);

            var clonedNetwork = (IHydroNetwork)network.Clone();

            var from2 = new HydroNode("from2");
            var to2 = new HydroNode("to2");
            clonedNetwork.Nodes.Add(from2);
            clonedNetwork.Nodes.Add(to2);
            var channel2 = new Channel {Name = "channel2", Source = from2, Target = to2 };
            clonedNetwork.Branches.Add(channel2);

            Assert.AreEqual(1,network.Branches.Count);
            Assert.AreEqual(2, clonedNetwork.Branches.Count);
        }

        [Test]
        public void GetAllItemsRecursive()
        {
            //TODO: expand the asserts..
            var network = new HydroNetwork();
            var allItems = network.GetAllItemsRecursive().ToArray();
            Assert.AreEqual(new object[] { network, network.CrossSectionSectionTypes[0] }, allItems);
        }

        [Test]
        public void CannotRemoveSectionTypesThatAreUsedByCrossSections()
        {
            //setup a network with a crossection and a sectiontype that is used
            var channel = new Channel();
            var network = new HydroNetwork();
            var crossSectionZW = new CrossSectionDefinitionZW();
            var crossSectionSectionType = new CrossSectionSectionType();
            
            crossSectionZW.Sections.Add(new CrossSectionSection { SectionType = crossSectionSectionType });
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel,crossSectionZW,0.0);
            
            network.CrossSectionSectionTypes.Add(crossSectionSectionType);
            network.Branches.Add(channel);

            
            //action! remove the sectiontype
            network.CrossSectionSectionTypes.Remove(crossSectionSectionType);

            //still have 2. one plus a 'default'?
            Assert.AreEqual(2,network.CrossSectionSectionTypes.Count);

            Assert.IsTrue(network.CrossSectionSectionTypes.Contains(crossSectionSectionType));
        }
   }
}