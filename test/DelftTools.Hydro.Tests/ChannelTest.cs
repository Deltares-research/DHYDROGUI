using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
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
            var channel = new Channel {LongName = "Long"};
            var clone = (Channel) channel.Clone();

            //todo expand to cover functionality
            Assert.AreEqual(channel.LongName, clone.LongName);
        }

        [Test]
        public void GivenChannel_WhenSettingLengthToNonPositiveValue_ThenLengthIsUnchanged()
        {
            // Given
            const double initialChannelLength = 10.0;
            var channel = new Channel {Length = initialChannelLength};

            // When
            channel.Length = 0.0;

            // Then
            Assert.That(channel.Length, Is.EqualTo(initialChannelLength));
        }

        [Test]
        public void GivenChannel_WhenSettingLengthToNonPositiveValue_ThenMessageIsLogged()
        {
            // Given
            const double initialChannelLength = 10.0;
            var channel = new Channel {Length = initialChannelLength};

            // When - Then
            string expectedMessage = $"Channel length must be positive. Length of channel '{channel.Name}' remains {initialChannelLength}.";
            TestHelper.AssertLogMessageIsGenerated(() => channel.Length = 0.0, expectedMessage, 1);
        }

        [Test]
        public void CloneWithWeir()
        {
            var weir = new Weir {Name = "weir"};
            var compositeStructure = new CompositeBranchStructure {Structures = {weir}};
            weir.ParentStructure = compositeStructure;
            var channel = new Channel
            {
                BranchFeatures =
                {
                    compositeStructure,
                    weir
                }
            };

            var channelClone = (Channel) channel.Clone();
            var compositeStructureClone = (CompositeBranchStructure) channelClone.BranchFeatures[0];

            channelClone.BranchFeatures[1].Should().Be.SameInstanceAs(compositeStructureClone.Structures[0]);
        }

        [Test]
        public void CloneWithWeirDoesNotChangeItemOrder()
        {
            var weir1 = new Weir {Name = "weir1"};
            var weir2 = new Weir {Name = "weir2"};
            var compositeStructure = new CompositeBranchStructure
            {
                Structures =
                {
                    weir1,
                    weir2
                }
            };
            weir1.ParentStructure = compositeStructure;
            weir2.ParentStructure = compositeStructure;
            var channel = new Channel
            {
                BranchFeatures =
                {
                    compositeStructure,
                    weir1,
                    weir2
                }
            };

            var channelClone = (Channel) channel.Clone();
            var compositeStructureClone = (CompositeBranchStructure) channelClone.BranchFeatures[0];

            channelClone.BranchFeatures[1].Should("cloned weir1").Be.SameInstanceAs(compositeStructureClone.Structures[0]);
            channelClone.BranchFeatures[2].Should("cloned weir2").Be.SameInstanceAs(compositeStructureClone.Structures[1]);
        }
    }
}
