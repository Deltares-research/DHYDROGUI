using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Converters.WellKnownText;
using SharpMap.Extensions.CoordinateSystems;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroNetworkExtensionsTest
    {
        private const int BuildServerFactor = 3;

        [Test]
        public void
            GivenNetworkWithCoordinateSystemWhenUpdateGeodeticDistancesOfChannelsIsCalledThenGeodeticDistanceOfChannelsIsSet()
        {
            var mocks = new MockRepository();
            var network = mocks.StrictMock<IHydroNetwork>();
            var channel = new Channel() {Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")};
            network.Expect(n => n.CoordinateSystem).Return(new OgrCoordinateSystemFactory().CreateFromEPSG(28992))
                .Repeat.Twice(); // Amersfoort / RD New
            network.Expect(n => n.Channels).Return(new EventedList<IChannel> {channel}).Repeat.Once();
            mocks.ReplayAll();
            if (Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();

            Assert.That(channel.Length, Is.EqualTo(100).Within(0.1));

            network.UpdateGeodeticDistancesOfChannels();

            Assert.That(channel.Length, Is.Not.EqualTo(100).Within(0.1));
            mocks.VerifyAll();
        }

        [Test]
        public void GivenNetworkWithoutCoordinateSystemWhenUpdateGeodeticDistancesOfChannelsIsCalledThenGeodeticDistanceOfChannelsIsCleared()
        {
            var mocks = new MockRepository();
            var network = mocks.StrictMock<IHydroNetwork>();
            var channel = new Channel() { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)") , GeodeticLength = 150};
            network.Expect(n => n.CoordinateSystem).Return(null).Repeat.Once();
            network.Expect(n => n.Channels).Return(new EventedList<IChannel> { channel }).Repeat.Once();
            mocks.ReplayAll();

            Assert.That(channel.Length, Is.EqualTo(150).Within(0.1));

            network.UpdateGeodeticDistancesOfChannels();

            Assert.That(channel.Length, Is.EqualTo(100).Within(0.1));
            Assert.That(channel.GeodeticLength, Is.NaN);

            mocks.VerifyAll();
        }

        [Test]
        public void TestEnsureCompositeBranchStructureNamesAreUnique()
        {
            var mocks = new MockRepository();
            var network = mocks.StrictMock<IHydroNetwork>();
            var compositeStructure1 = mocks.StrictMock<ICompositeBranchStructure>();
            var compositeStructure2 = mocks.Stub<ICompositeBranchStructure>();
            var compositeStructureList = new IBranchFeature[] {compositeStructure1, compositeStructure2};

            network.Expect(n => n.BranchFeatures).Return(compositeStructureList).Repeat.Any();
            compositeStructure1.Expect(c => c.Name).Return("Test");
            compositeStructure2.Name = "Test";

            mocks.ReplayAll();

            network.MakeNamesUnique<ICompositeBranchStructure>();

            Assert.AreEqual("Test_1",compositeStructure2.Name);

            mocks.VerifyAll();
        }

        [Test, Category(TestCategory.Integration)]
        public void TestEnsureCompositeBranchStructureNamesAreUniqueWithRealNetwork()
        {
            var network = (HydroNetwork)HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch);

            var weir1 = new Weir { Name = "Weir1", Chainage = branch.Length / 3 };
            var weir2 = new Weir { Name = "Weir2", Chainage = branch.Length / 3 };
            var weir3 = new Weir { Name = "Weir3", Chainage = branch.Length / 3 * 2 };
            var weir4 = new Weir { Name = "Weir5", Chainage = branch.Length / 3 * 2 };

            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir1, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir2, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir3, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir4, branch);

            network.CompositeBranchStructures.ForEach(cbs => cbs.Name = "CompositeBranchStructure");

            Assert.IsFalse(network.CompositeBranchStructures.Select(cbs => cbs.Name).HasUniqueValues());

            // Make unique and check messages
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => network.MakeNamesUnique<ICompositeBranchStructure>(),
                "Branch feature names must be unique, the following Branch features have been renamed:");

            Assert.IsTrue(network.CompositeBranchStructures.Select(cbs => cbs.Name).HasUniqueValues());
        }

        [Test, Category(TestCategory.Performance)]
        public void EnsureCompositeBranchStructureNamesAreUniqueShouldBeFast()
        {
            var mocks = new MockRepository();
            var network = mocks.StrictMock<IHydroNetwork>();

            var compositeStructureList = Enumerable.Range(1, 100000).Select(i => new CompositeBranchStructure("Test", 0)).ToList();

            network.Expect(n => n.BranchFeatures).Return(compositeStructureList).Repeat.Any();
            
            mocks.ReplayAll();
            
            TestHelper.AssertIsFasterThan(300 * BuildServerFactor, () => network.MakeNamesUnique<ICompositeBranchStructure>());

            Assert.IsTrue(compositeStructureList.HasUniqueValues());
        }
    }
}