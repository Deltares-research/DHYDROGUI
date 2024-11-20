using System.Collections.Specialized;
using System.ComponentModel;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects.Friction
{
    [TestFixture]
    public class ChannelFrictionDefinitionTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var channel = new Channel();

            // Call
            var channelFrictionDefinition = new ChannelFrictionDefinition(channel);

            // Assert
            Assert.AreSame(channel, channelFrictionDefinition.Channel);
            Assert.AreEqual(ChannelFrictionSpecificationType.ModelSettings, channelFrictionDefinition.SpecificationType);
            Assert.AreSame(channel.Geometry, channelFrictionDefinition.Geometry);
            Assert.AreSame(channel.Attributes, channelFrictionDefinition.Attributes);
            Assert.AreEqual("1D Roughness", channelFrictionDefinition.ToString());
        }

        [Test]
        public void SpecificationType_SetToConstantChannelFrictionDefinition_DefinitionsSynchronizedAccordingly()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel());

            // Call
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;

            // Assert
            Assert.IsNotNull(channelFrictionDefinition.ConstantChannelFrictionDefinition);
            Assert.IsNull(channelFrictionDefinition.SpatialChannelFrictionDefinition);
        }

        [Test]
        public void SpecificationType_SetToSpatialChannelFrictionDefinition_DefinitionsSynchronizedAccordingly()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel());

            // Call
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;

            // Assert
            Assert.IsNull(channelFrictionDefinition.ConstantChannelFrictionDefinition);
            Assert.IsNotNull(channelFrictionDefinition.SpatialChannelFrictionDefinition);
        }

        [Test]
        public void SpecificationType_SetToModelSettings_DefinitionsSynchronizedAccordingly()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel());

            // Call
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;

            // Assert
            Assert.IsNull(channelFrictionDefinition.ConstantChannelFrictionDefinition);
            Assert.IsNull(channelFrictionDefinition.SpatialChannelFrictionDefinition);
        }

        [Test]
        public void SpecificationType_SetToRoughnessSections_DefinitionsSynchronizedAccordingly()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel());

            // Call
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.RoughnessSections;

            // Assert
            Assert.IsNull(channelFrictionDefinition.ConstantChannelFrictionDefinition);
            Assert.IsNull(channelFrictionDefinition.SpatialChannelFrictionDefinition);
        }

        [Test]
        public void SpecificationType_SetToCrossSectionFrictionDefinitions_DefinitionsSynchronizedAccordingly()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel());

            // Call
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions;

            // Assert
            Assert.IsNull(channelFrictionDefinition.ConstantChannelFrictionDefinition);
            Assert.IsNull(channelFrictionDefinition.SpatialChannelFrictionDefinition);
        }

        [Test] // Test related to marking model dirty
        public void SpecificationType_SetValue_NotifiesPropertyChanged()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel());

            var counter = 0;
            ((INotifyPropertyChanged) channelFrictionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, channelFrictionDefinition) &&
                    args.PropertyName == nameof(ChannelFrictionDefinition.SpecificationType))
                {
                    counter++;
                }
            };

            // Call
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.RoughnessSections;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to view synchronization
        public void Channel_SetName_BubblesPropertyChanged()
        {
            // Setup
            var channel = new Channel();
            var channelFrictionDefinition = new ChannelFrictionDefinition(channel);

            var counter = 0;
            ((INotifyPropertyChanged) channelFrictionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, channel) && args.PropertyName == nameof(IChannel.Name))
                {
                    counter++;
                }
            };

            // Call
            channel.Name = "New name";

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void ConstantChannelFrictionDefinition_SetProperty_BubblesPropertyChanged()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel())
            {
                SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition
            };

            var counter = 0;
            ((INotifyPropertyChanged) channelFrictionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, channelFrictionDefinition.ConstantChannelFrictionDefinition))
                {
                    counter++;
                }
            };

            // Call
            channelFrictionDefinition.ConstantChannelFrictionDefinition.Value = 1.1;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void SpatialChannelFrictionDefinition_SetProperty_BubblesPropertyChanged()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel())
            {
                SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition
            };

            var counter = 0;
            ((INotifyPropertyChanged) channelFrictionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, channelFrictionDefinition.SpatialChannelFrictionDefinition))
                {
                    counter++;
                }
            };

            // Call
            channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = RoughnessType.DeBosBijkerk;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void SpatialChannelFrictionDefinition_ChangeCollection_BubblesCollectionChanged()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel())
            {
                SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition
            };

            var counter = 0;
            ((INotifyCollectionChanged) channelFrictionDefinition).CollectionChanged += (sender, args) =>
            { 
                counter++;
            };

            // Call
            channelFrictionDefinition.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(new ConstantSpatialChannelFrictionDefinition());

            // Assert
            Assert.AreEqual(1, counter);
        }
    }
}