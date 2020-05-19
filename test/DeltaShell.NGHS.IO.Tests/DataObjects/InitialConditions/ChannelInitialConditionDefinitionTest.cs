using System.Collections.Specialized;
using System.ComponentModel;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects.InitialConditions
{
    [TestFixture]
    public class ChannelInitialConditionDefinitionTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var channel = new Channel();

            // Call
            var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(channel);

            // Assert
            Assert.AreSame(channel, channelInitialConditionDefinition.Channel);
            Assert.AreEqual(ChannelInitialConditionSpecificationType.ModelSettings, channelInitialConditionDefinition.SpecificationType);
            Assert.AreSame(channel.Geometry, channelInitialConditionDefinition.Geometry);
            Assert.AreSame(channel.Attributes, channelInitialConditionDefinition.Attributes);
            Assert.AreEqual("1D Initial Conditions", channelInitialConditionDefinition.ToString());
        }

        [Test]
        public void SpecificationType_SetToConstantChannelInitialConditionDefinition_DefinitionsSynchronizedAccordingly()
        {
            // Setup
            var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(new Channel());

            // Call
            channelInitialConditionDefinition.SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;

            // Assert
            Assert.IsNotNull(channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition);
            Assert.IsNull(channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition);
        }

        [Test]
        public void SpecificationType_SetToSpatialChannelInitialConditionDefinition_DefinitionsSynchronizedAccordingly()
        {
            // Setup
            var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(new Channel());

            // Call
            channelInitialConditionDefinition.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;

            // Assert
            Assert.IsNull(channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition);
            Assert.IsNotNull(channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition);
        }

        [Test]
        public void SpecificationType_SetToModelSettings_DefinitionsSynchronizedAccordingly()
        {
            // Setup
            var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(new Channel());

            // Call
            channelInitialConditionDefinition.SpecificationType = ChannelInitialConditionSpecificationType.ModelSettings;

            // Assert
            Assert.IsNull(channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition);
            Assert.IsNull(channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition);
        }


        [Test] // Test related to marking model dirty
        public void SpecificationType_SetValue_NotifiesPropertyChanged()
        {
            // Setup
            var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(new Channel());

            var counter = 0;
            ((INotifyPropertyChanged)channelInitialConditionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, channelInitialConditionDefinition) &&
                    args.PropertyName == nameof(ChannelInitialConditionDefinition.SpecificationType))
                {
                    counter++;
                }
            };

            // Call
            channelInitialConditionDefinition.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to view synchronization
        public void Channel_SetName_BubblesPropertyChanged()
        {
            // Setup
            var channel = new Channel();
            var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(channel);

            var counter = 0;
            ((INotifyPropertyChanged)channelInitialConditionDefinition).PropertyChanged += (sender, args) =>
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
        public void ConstantChannelInitialConditionDefinition_SetProperty_BubblesPropertyChanged()
        {
            // Setup
            var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(new Channel())
            {
                SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition
            };

            var counter = 0;
            ((INotifyPropertyChanged)channelInitialConditionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition))
                {
                    counter++;
                }
            };

            // Call
            channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Value = 1.1;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void SpatialChannelInitialConditionDefinition_SetProperty_BubblesPropertyChanged()
        {
            // Setup
            var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(new Channel())
            {
                SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition
            };

            var counter = 0;
            ((INotifyPropertyChanged)channelInitialConditionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition))
                {
                    counter++;
                }
            };

            // Call
            channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition.Quantity = InitialConditionQuantity.WaterDepth;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void SpatialChannelInitialConditionDefinition_ChangeCollection_BubblesCollectionChanged()
        {
            // Setup
            var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(new Channel())
            {
                SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition
            };

            var counter = 0;
            ((INotifyCollectionChanged)channelInitialConditionDefinition).CollectionChanged += (sender, args) =>
            {
                counter++;
            };

            // Call
            channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(new ConstantSpatialChannelInitialConditionDefinition());

            // Assert
            Assert.AreEqual(1, counter);
        }
    }
}