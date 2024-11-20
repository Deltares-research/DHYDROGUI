using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroNodeTest
    {
        [Test]
        public void IsConnectedToMultipleBranches()
        {
            var mocks = new MockRepository();
            var branch1 = mocks.Stub<IBranch>();
            var branch2 = mocks.Stub<IBranch>();

            mocks.ReplayAll();

            var node = new HydroNode();
            Assert.IsFalse(node.IsConnectedToMultipleBranches);

            node.OutgoingBranches.Add(branch1);
            Assert.IsFalse(node.IsConnectedToMultipleBranches);

            node.OutgoingBranches.Add(branch2);
            Assert.IsTrue(node.IsConnectedToMultipleBranches);

            node.OutgoingBranches.Remove(branch2);
            Assert.IsFalse(node.IsConnectedToMultipleBranches);

            node.IncomingBranches.Add(branch2);
            Assert.IsTrue(node.IsConnectedToMultipleBranches);

            node.OutgoingBranches.Remove(branch1);
            Assert.IsFalse(node.IsConnectedToMultipleBranches);

            node.IncomingBranches.Add(branch2);
            Assert.IsTrue(node.IsConnectedToMultipleBranches);
        }

        [Test]
        public void IsOnSingleBranch()
        {
            var mocks = new MockRepository();
            var branch1 = mocks.Stub<IBranch>();
            var branch2 = mocks.Stub<IBranch>();

            mocks.ReplayAll();

            var node = new HydroNode();
            Assert.IsFalse(node.IsOnSingleBranch);

            node.OutgoingBranches.Add(branch1);
            Assert.IsTrue(node.IsOnSingleBranch);

            node.OutgoingBranches.Add(branch2);
            Assert.IsFalse(node.IsOnSingleBranch);

            node.OutgoingBranches.Remove(branch2);
            Assert.IsTrue(node.IsOnSingleBranch);

            node.IncomingBranches.Add(branch2);
            Assert.IsFalse(node.IsOnSingleBranch);

            node.OutgoingBranches.Remove(branch1);
            Assert.IsTrue(node.IsOnSingleBranch);

            node.IncomingBranches.Add(branch2);
            Assert.IsFalse(node.IsOnSingleBranch);
        }

        [Test]
        public void LinksAreRemovedOnBranchAdd()
        {
            var mocks = new MockRepository();

            var source = mocks.Stub<IHydroObject>();
            var links = new EventedList<HydroLink>();
            
            var branch1 = mocks.Stub<IBranch>();
            var branch2 = mocks.Stub<IBranch>();

            var node = new HydroNode();
            node.OutgoingBranches.Add(branch1);

            source.Links = links;

            source.Expect(s => s.UnlinkFrom(node)).Repeat.Once();

            mocks.ReplayAll();

            // normally done using node.LinkTo
            var link = new HydroLink(source, node);
            node.Links.Add(link);
            source.Links.Add(link);

            node.IncomingBranches.Add(branch2);

            Assert.AreEqual(0, node.Links.Count);
        }

        [Test]
        public void Clone()
        {
            var node = new HydroNode("Name") { LongName = "LongName" };
            var clone = (HydroNode) node.Clone();

            // TODO: Expand to cover functionality
            Assert.AreEqual(node.Name, clone.Name);
            Assert.AreEqual(node.LongName, clone.LongName);
        }
    }
}