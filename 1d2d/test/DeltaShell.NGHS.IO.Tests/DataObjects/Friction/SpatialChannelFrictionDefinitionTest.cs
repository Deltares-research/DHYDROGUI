using System.Collections.Specialized;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects.Friction
{
    [TestFixture]
    public class SpatialChannelFrictionDefinitionTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var spatialChannelFrictionDefinition = new SpatialChannelFrictionDefinition();

            // Assert
            Assert.AreEqual((RoughnessType) 0, spatialChannelFrictionDefinition.Type);
            Assert.AreEqual(RoughnessFunction.Constant, spatialChannelFrictionDefinition.FunctionType);
        }

        [Test]
        public void FunctionType_SetToConstant_DefinitionsAndFunctionSynchronizedAccordingly()
        {
            // Setup
            var spatialChannelFrictionDefinition = new SpatialChannelFrictionDefinition();

            // Call
            spatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;

            // Assert
            Assert.IsNotNull(spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions);
            Assert.IsNull(spatialChannelFrictionDefinition.Function);
        }

        [Test]
        public void FunctionType_SetToFunctionOfQ_DefinitionsAndFunctionSynchronizedAccordingly()
        {
            // Setup
            var spatialChannelFrictionDefinition = new SpatialChannelFrictionDefinition();

            // Call
            spatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;

            // Assert
            Assert.IsNull(spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions);
            Assert.IsNotNull(spatialChannelFrictionDefinition.Function);
            Assert.AreEqual("Q", spatialChannelFrictionDefinition.Function.Arguments[1].Name);
        }

        [Test]
        public void FunctionType_SetToFunctionOfH_DefinitionsAndFunctionSynchronizedAccordingly()
        {
            // Setup
            var spatialChannelFrictionDefinition = new SpatialChannelFrictionDefinition();

            // Call
            spatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;

            // Assert
            Assert.IsNull(spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions);
            Assert.IsNotNull(spatialChannelFrictionDefinition.Function);
            Assert.AreEqual("H", spatialChannelFrictionDefinition.Function.Arguments[1].Name);
        }

        [Test] // Test related to marking model dirty
        public void Type_SetValue_NotifiesPropertyChanged()
        {
            // Setup
            var spatialChannelFrictionDefinition = new SpatialChannelFrictionDefinition();

            var counter = 0;
            ((INotifyPropertyChanged) spatialChannelFrictionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, spatialChannelFrictionDefinition) &&
                    args.PropertyName == nameof(SpatialChannelFrictionDefinition.Type))
                {
                    counter++;
                }
            };

            // Call
            spatialChannelFrictionDefinition.Type = RoughnessType.DeBosBijkerk;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void FunctionType_SetValue_NotifiesPropertyChanged()
        {
            // Setup
            var spatialChannelFrictionDefinition = new SpatialChannelFrictionDefinition();

            var counter = 0;
            ((INotifyPropertyChanged) spatialChannelFrictionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, spatialChannelFrictionDefinition) &&
                    args.PropertyName == nameof(SpatialChannelFrictionDefinition.FunctionType))
                {
                    counter++;
                }
            };

            // Call
            spatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void ConstantChannelFrictionDefinition_ChangeCollection_NotifiesCollectionChanged()
        {
            // Setup
            var spatialChannelFrictionDefinition = new SpatialChannelFrictionDefinition
            {
                FunctionType = RoughnessFunction.Constant
            };

            var counter = 0;
            ((INotifyCollectionChanged) spatialChannelFrictionDefinition).CollectionChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions))
                {
                    counter++;
                }
            };

            // Call
            spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(new ConstantSpatialChannelFrictionDefinition());

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void Function_ChangeValue_BubblesPropertyChanged()
        {
            // Setup
            var spatialChannelFrictionDefinition = new SpatialChannelFrictionDefinition
            {
                FunctionType = RoughnessFunction.FunctionOfQ
            };

            var counter = 0;
            ((INotifyPropertyChanged) spatialChannelFrictionDefinition).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, spatialChannelFrictionDefinition.Function) && !spatialChannelFrictionDefinition.Function.IsEditing)
                {
                    counter++;
                }
            };

            // Call
            spatialChannelFrictionDefinition.Function.BeginEdit("");
            spatialChannelFrictionDefinition.Function.EndEdit();

            // Assert
            Assert.AreEqual(1, counter);
        }
    }
}
