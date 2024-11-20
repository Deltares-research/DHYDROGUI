using System.ComponentModel;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects.InitialConditions
{
    public class ConstantChannelInitialConditionDefinitionTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var constantChannelInitialConditionDefinition = new ConstantChannelInitialConditionDefinition();

            // Assert
            Assert.AreEqual((InitialConditionQuantity)0, constantChannelInitialConditionDefinition.Quantity);
            Assert.AreEqual(0.0, constantChannelInitialConditionDefinition.Value);
        }

        [Test] // Test related to marking model dirty
        public void Quantity_SetValue_NotifiesPropertyChanged()
        {
            // Setup
            var constantChannelInitialConditionDefinition = new ConstantChannelInitialConditionDefinition();

            var counter = 0;
            ((INotifyPropertyChanged)constantChannelInitialConditionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, constantChannelInitialConditionDefinition) &&
                    args.PropertyName == nameof(ConstantChannelInitialConditionDefinition.Quantity))
                {
                    counter++;
                }
            };

            // Call
            constantChannelInitialConditionDefinition.Quantity = InitialConditionQuantity.WaterDepth;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void Value_SetValue_NotifiesPropertyChanged()
        {
            // Setup
            var constantChannelInitialConditionDefinition = new ConstantChannelInitialConditionDefinition();

            var counter = 0;
            ((INotifyPropertyChanged)constantChannelInitialConditionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, constantChannelInitialConditionDefinition) &&
                    args.PropertyName == nameof(ConstantChannelInitialConditionDefinition.Value))
                {
                    counter++;
                }
            };

            // Call
            constantChannelInitialConditionDefinition.Value = 1.1;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test]
        [TestCase(InitialConditionQuantity.WaterLevel, 123)]
        [TestCase(InitialConditionQuantity.WaterDepth, 456)]
        public void CopyFrom_CorrectlyCopiesQuantityAndValue(InitialConditionQuantity quantity, double value)
        {
            var definitionToCopyFrom = new ConstantChannelInitialConditionDefinition { Quantity = quantity, Value = value};

            var newDefinition = new ConstantChannelInitialConditionDefinition();
            newDefinition.CopyFrom(definitionToCopyFrom);

            Assert.That(newDefinition.Quantity, Is.EqualTo(definitionToCopyFrom.Quantity));
            Assert.That(newDefinition.Value, Is.EqualTo(definitionToCopyFrom.Value));
        }
    }
}