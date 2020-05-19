using System.Collections.Specialized;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Editing;
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

    }
}
