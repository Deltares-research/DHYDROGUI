using DelftTools.Hydro.Structures;
using NUnit.Framework;
using SharpTestsEx;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class ChannelTest
    {
        [Test]
        public void Clone()
        {
            var channel = new Channel { LongName = "Long" };
            var clone = (Channel)channel.Clone();

            //todo expand to cover functionality
            Assert.AreEqual(channel.LongName, clone.LongName);
        }

        [Test]
        public void CloneWithWeir()
        {
            var weir = new Weir { Name = "weir" };
            var compositeStructure = new CompositeBranchStructure { Structures = { weir } };
            weir.ParentStructure = compositeStructure; // TODO: bug in implementation of CompositeBranchStructure, should not be required
            var channel = new Channel { BranchFeatures = { compositeStructure, weir } };
            
            var channelClone = (Channel)channel.Clone();
            var compositeStructureClone = (CompositeBranchStructure)channelClone.BranchFeatures[0];

            channelClone.BranchFeatures[1].Should().Be.SameInstanceAs(compositeStructureClone.Structures[0]);
        }

        [Test]
        public void CloneWithGate()
        {
            var gate = new Gate() {Name = "gate"};
            var compositeStructure = new CompositeBranchStructure() {Structures = {gate}};
            gate.ParentStructure = compositeStructure;
            var channel = new Channel() {BranchFeatures = {compositeStructure, gate}};

            var channelClone = (Channel) channel.Clone();
            var compositeStructureClone = (CompositeBranchStructure) channelClone.BranchFeatures[0];

            channelClone.BranchFeatures[1].Should().Be.SameInstanceAs(compositeStructureClone.Structures[0]);
        }

        [Test]
        public void CloneWithWeirDoesNotChangeItemOrder()
        {
            var weir1 = new Weir { Name = "weir1" };
            var weir2 = new Weir { Name = "weir2" };
            var compositeStructure = new CompositeBranchStructure { Structures = { weir1, weir2 } };
            weir1.ParentStructure = compositeStructure; // TODO: bug in implementation of CompositeBranchStructure, should not be required
            weir2.ParentStructure = compositeStructure; // TODO: bug in implementation of CompositeBranchStructure, should not be required
            var channel = new Channel { BranchFeatures = { compositeStructure, weir1, weir2 } };

            var channelClone = (Channel)channel.Clone();
            var compositeStructureClone = (CompositeBranchStructure)channelClone.BranchFeatures[0];

            channelClone.BranchFeatures[1].Should("cloned weir1").Be.SameInstanceAs(compositeStructureClone.Structures[0]);
            channelClone.BranchFeatures[2].Should("cloned weir2").Be.SameInstanceAs(compositeStructureClone.Structures[1]);

        }

        [Test]
        public void CloneWithGateDoesNotChangeItemOrder()
        {
            var gate1 = new Gate {Name = "gate1"};
            var gate2 = new Gate() {Name = "gate2"};
            var compositeStructure = new CompositeBranchStructure { Structures = { gate1, gate2 } };
            gate1.ParentStructure = compositeStructure; // TODO: bug in implementation of CompositeBranchStructure, should not be required
            gate2.ParentStructure = compositeStructure; // TODO: bug in implementation of CompositeBranchStructure, should not be required
            var channel = new Channel { BranchFeatures = { compositeStructure, gate1, gate2 } };

            var channelClone = (Channel)channel.Clone();
            var compositeStructureClone = (CompositeBranchStructure)channelClone.BranchFeatures[0];

            channelClone.BranchFeatures[1].Should("cloned gate1").Be.SameInstanceAs(compositeStructureClone.Structures[0]);
            channelClone.BranchFeatures[2].Should("cloned gate2").Be.SameInstanceAs(compositeStructureClone.Structures[1]);
        }
    }
}