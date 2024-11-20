using System.Collections.Specialized;
using System.ComponentModel;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects.InitialConditions
{
    [TestFixture]
    public class SpatialChannelInitialConditionDefinitionTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var spatialChannelInitialConditionDefinition = new SpatialChannelInitialConditionDefinition();

            // Assert
            Assert.AreEqual((InitialConditionQuantity)0, spatialChannelInitialConditionDefinition.Quantity);
        }

        [Test] // Test related to marking model dirty
        public void Type_SetValue_NotifiesPropertyChanged()
        {
            // Setup
            var spatialChannelInitialConditionDefinition = new SpatialChannelInitialConditionDefinition();

            var counter = 0;
            ((INotifyPropertyChanged)spatialChannelInitialConditionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, spatialChannelInitialConditionDefinition) &&
                    args.PropertyName == nameof(SpatialChannelInitialConditionDefinition.Quantity))
                {
                    counter++;
                }
            };

            // Call
            spatialChannelInitialConditionDefinition.Quantity = InitialConditionQuantity.WaterDepth;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void ConstantChannelInitialConditionDefinition_ChangeCollection_NotifiesCollectionChanged()
        {
            // Setup
            var spatialChannelInitialConditionDefinition = new SpatialChannelInitialConditionDefinition();
            
            var counter = 0;
            ((INotifyCollectionChanged)spatialChannelInitialConditionDefinition).CollectionChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, spatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions))
                {
                    counter++;
                }
            };

            // Call
            spatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(new ConstantSpatialChannelInitialConditionDefinition());

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test]
        [TestCase(InitialConditionQuantity.WaterLevel)]
        [TestCase(InitialConditionQuantity.WaterDepth)]
        public void CopyFrom_CorrectlyCopiesQuantityAndConstantSpatialDefinitionToNewDefinition(InitialConditionQuantity quantity)
        {
            var definitionToCopyFrom = new SpatialChannelInitialConditionDefinition { Quantity = quantity };
            definitionToCopyFrom.ConstantSpatialChannelInitialConditionDefinitions.Add(new ConstantSpatialChannelInitialConditionDefinition{Chainage = 0.123, Value = 456});
            definitionToCopyFrom.ConstantSpatialChannelInitialConditionDefinitions.Add(new ConstantSpatialChannelInitialConditionDefinition{Chainage = 147, Value = 987});

            var newDefinition = new SpatialChannelInitialConditionDefinition();
            newDefinition.CopyFrom(definitionToCopyFrom);

            Assert.That(newDefinition.Quantity, Is.EqualTo(definitionToCopyFrom.Quantity));
            Assert.That(newDefinition.ConstantSpatialChannelInitialConditionDefinitions, Is.EquivalentTo(definitionToCopyFrom.ConstantSpatialChannelInitialConditionDefinitions));
        }

        [Test]
        [TestCase(InitialConditionQuantity.WaterLevel)]
        [TestCase(InitialConditionQuantity.WaterDepth)]
        public void CopyFrom_CorrectlyCopiesQuantityAndConstantSpatialDefinitionToExistingDefinition(InitialConditionQuantity quantity)
        {
            var existingDefinition = new SpatialChannelInitialConditionDefinition();
            existingDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(new ConstantSpatialChannelInitialConditionDefinition { Chainage = 0.123, Value = 456 });
            existingDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(new ConstantSpatialChannelInitialConditionDefinition { Chainage = 147, Value = 987 });

            var definitionToCopyFrom = new SpatialChannelInitialConditionDefinition { Quantity = quantity };
            definitionToCopyFrom.ConstantSpatialChannelInitialConditionDefinitions.Add(new ConstantSpatialChannelInitialConditionDefinition { Chainage = 80085, Value = 707 });

            existingDefinition.CopyFrom(definitionToCopyFrom);

            Assert.That(existingDefinition.Quantity, Is.EqualTo(definitionToCopyFrom.Quantity));
            Assert.That(existingDefinition.ConstantSpatialChannelInitialConditionDefinitions, Is.EquivalentTo(definitionToCopyFrom.ConstantSpatialChannelInitialConditionDefinitions));
        }

    }
}
