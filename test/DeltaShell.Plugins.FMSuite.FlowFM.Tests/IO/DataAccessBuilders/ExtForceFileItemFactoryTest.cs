using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.DataAccessBuilders
{
    [TestFixture]
    public class ExtForceFileItemFactoryTest
    {
        [Test]
        public void GetBoundaryConditionsItems_ModelDefinitionNull_ThrowsArgumentNullException()
        {
            // Setup
            var mocks = new MockRepository();
            var polyLineForeFileItems = mocks.Stub<IDictionary<IFeatureData, ExtForceFileItem>>();
            mocks.ReplayAll();

            // Call
            void Call() => ExtForceFileItemFactory.GetBoundaryConditionsItems(null, polyLineForeFileItems);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("modelDefinition"));
            mocks.VerifyAll();
        }

        [Test]
        public void GetBoundaryConditionsItems_PolyLineForceFileItemsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ExtForceFileItemFactory.GetBoundaryConditionsItems(new WaterFlowFMModelDefinition(), null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("polyLineForceFileItems"));
        }

        [Test]
        public void GetBoundaryConditionsItems_WithData_ReturnsExpectedItems()
        {
            // Setup
            var modelDefinition = new WaterFlowFMModelDefinition();
            var feature = new Feature2D
            {
                Name = "boundary",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
            };

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents) {Feature = feature};
            AddBoundaryCondition(modelDefinition, bc1);

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.NormalVelocity, BoundaryConditionDataType.AstroComponents) {Feature = feature};
            AddBoundaryCondition(modelDefinition, bc2);

            var polyLineForceFileItems = new Dictionary<IFeatureData, ExtForceFileItem>
            {
                {bc1, new ExtForceFileItem(ExtForceQuantNames.GetQuantityString(bc1))},
                {bc2, new ExtForceFileItem(ExtForceQuantNames.GetQuantityString(bc2))}
            };

            // Call
            IDictionary<FlowBoundaryCondition, ExtForceFileItem> boundariesExtForceFileItems =
                ExtForceFileItemFactory.GetBoundaryConditionsItems(modelDefinition, polyLineForceFileItems);

            // Assert
            Assert.That(boundariesExtForceFileItems.Count, Is.EqualTo(2));
            Assert.That(boundariesExtForceFileItems, Is.Not.SameAs(polyLineForceFileItems));

            foreach (KeyValuePair<IFeatureData, ExtForceFileItem> polyLineForceFileItem in polyLineForceFileItems)
            {
                var flowBoundaryCondition = (FlowBoundaryCondition) polyLineForceFileItem.Key;
                Assert.That(boundariesExtForceFileItems.ContainsKey(flowBoundaryCondition), Is.True);
                Assert.That(boundariesExtForceFileItems[flowBoundaryCondition], Is.SameAs(polyLineForceFileItem.Value));
            }
        }

        [Test]
        public void GivenMultipleItemsAndInitialVelocities_WhenGetVelocityItem_ThenRetrieveExpectedForceFileItem()
        {
            //Arrange
            var extForceFileItem = new ExtForceFileItem("quantity");
            var initialVelocity = Substitute.For<IPointCloud>();
            IDictionary<ExtForceFileItem, object> existingForceFileItems = new Dictionary<ExtForceFileItem, object>();
            existingForceFileItems[extForceFileItem] = initialVelocity;
            existingForceFileItems[new ExtForceFileItem("otherquantityone")] = Substitute.For<IPointCloud>();
            existingForceFileItems[new ExtForceFileItem("otherquantitytwo")] = Substitute.For<IPointCloud>();

            //Act
            ExtForceFileItem retrievedExtForceFileItem = ExtForceFileItemFactory.GetVelocityItem(initialVelocity, existingForceFileItems);
            
            //Assert
            Assert.That(retrievedExtForceFileItem, Is.EqualTo(extForceFileItem));
        }

        private static void AddBoundaryCondition(WaterFlowFMModelDefinition modelDefinition, FlowBoundaryCondition bc)
        {
            BoundaryConditionSet set = modelDefinition.BoundaryConditionSets.FirstOrDefault(bcs => bcs.Feature == ((IBoundaryCondition) bc).Feature);

            if (set != null)
            {
                set.BoundaryConditions.Add(bc);
            }
            else
            {
                modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet
                {
                    Feature = ((IBoundaryCondition) bc).Feature as Feature2D,
                    BoundaryConditions = new EventedList<IBoundaryCondition> {bc}
                });
            }
        }
    }
}