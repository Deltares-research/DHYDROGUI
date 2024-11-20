using System.ComponentModel;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects.Friction
{
    [TestFixture]
    public class ConstantChannelFrictionDefinitionTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var constantChannelFrictionDefinition = new ConstantChannelFrictionDefinition();

            // Assert
            Assert.AreEqual((RoughnessType) 0, constantChannelFrictionDefinition.Type);
            Assert.AreEqual(0.0, constantChannelFrictionDefinition.Value);
        }

        [Test] // Test related to marking model dirty
        public void Type_SetValue_NotifiesPropertyChanged()
        {
            // Setup
            var constantChannelFrictionDefinition = new ConstantChannelFrictionDefinition();

            var counter = 0;
            ((INotifyPropertyChanged) constantChannelFrictionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, constantChannelFrictionDefinition) &&
                    args.PropertyName == nameof(ConstantChannelFrictionDefinition.Type))
                {
                    counter++;
                }
            };

            // Call
            constantChannelFrictionDefinition.Type = RoughnessType.DeBosBijkerk;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void Value_SetValue_NotifiesPropertyChanged()
        {
            // Setup
            var constantChannelFrictionDefinition = new ConstantChannelFrictionDefinition();

            var counter = 0;
            ((INotifyPropertyChanged) constantChannelFrictionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, constantChannelFrictionDefinition) &&
                    args.PropertyName == nameof(ConstantChannelFrictionDefinition.Value))
                {
                    counter++;
                }
            };

            // Call
            constantChannelFrictionDefinition.Value = 1.1;

            // Assert
            Assert.AreEqual(1, counter);
        }
    }
}
