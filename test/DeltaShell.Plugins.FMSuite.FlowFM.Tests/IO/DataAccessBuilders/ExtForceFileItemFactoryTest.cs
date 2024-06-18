using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.SpatialOperations;

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
                { bc1, new ExtForceFileItem(ExtForceQuantNames.GetQuantityString(bc1)) { FileName = "bc1.pli" } },
                { bc2, new ExtForceFileItem(ExtForceQuantNames.GetQuantityString(bc2)) { FileName = "bc2.pli" } },
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
        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void Constructor_ArgumentNullCases_ThrowsException(Samples samples, string quantity, 
                                                                  IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            // Call
            void Call() => ExtForceFileItemFactory.GetSamplesItem(samples, quantity, existingForceFileItems);
            
            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_QuantityNullOrWhitespace_ThrowsException(string quantity)
        {
            // Setup
            var samples = new Samples("randomName");
            var existingForceFileItems = Substitute.For<IDictionary<ExtForceFileItem, object>>();
            
            // Call
            void Call() => ExtForceFileItemFactory.GetSamplesItem(samples, quantity, existingForceFileItems);
            
            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void GetSamplesItem_SpatialInterpolationMethodAveraging_ReturnsExpectedExtForceFileItem()
        {
            // Setup
            const string fileName = "randomFileName";
            const double relativeSearchCellSize = 123.4;
            var samples = new Samples("randomName")
            {
                SourceFileName = fileName,
                InterpolationMethod = SpatialInterpolationMethod.Averaging,
                RelativeSearchCellSize = relativeSearchCellSize,
                AveragingMethod = GridCellAveragingMethod.ClosestPoint
            };
            
            
            const string quantity = "randomQuantity";
            var existingFiles = new Dictionary<ExtForceFileItem, object>();

            // Call
            ExtForceFileItem samplesItem = ExtForceFileItemFactory.GetSamplesItem(samples, quantity, existingFiles);

            // Assert
            Assert.That(samplesItem.Quantity, Is.EqualTo(quantity));
            Assert.That(samplesItem.FileName, Is.EqualTo(fileName));
            Assert.That(samplesItem.FileType, Is.EqualTo(7));
            Assert.That(samplesItem.Method, Is.EqualTo(6)); // Averaging => 6
            Assert.That(samplesItem.Operand, Is.EqualTo("O"));
            Assert.That(samplesItem.ModelData[ExtForceFileConstants.AveragingTypeKey], Is.EqualTo(2)); // ClosestPoint => 2
            Assert.That(samplesItem.ModelData[ExtForceFileConstants.RelSearchCellSizeKey], Is.EqualTo(relativeSearchCellSize));
        }
        
        [Test]
        public void GetSamplesItem_SpatialInterpolationMethodTriangulation_ReturnsExpectedExtForceFileItem()
        {
            // Setup
            const string fileName = "randomFileName";
            const double relativeSearchCellSize = 123.4;
            const double extraPolTol = 123.4;
            var samples = new Samples("randomName")
            {
                SourceFileName = fileName,
                InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                RelativeSearchCellSize = relativeSearchCellSize,
                AveragingMethod = GridCellAveragingMethod.ClosestPoint,
                ExtrapolationTolerance = extraPolTol
            };
            
            
            const string quantity = "randomQuantity";
            var existingFiles = new Dictionary<ExtForceFileItem, object>();

            // Call
            ExtForceFileItem samplesItem = ExtForceFileItemFactory.GetSamplesItem(samples, quantity, existingFiles);

            // Assert
            Assert.That(samplesItem.Quantity, Is.EqualTo(quantity));
            Assert.That(samplesItem.FileName, Is.EqualTo(fileName));
            Assert.That(samplesItem.FileType, Is.EqualTo(7));
            Assert.That(samplesItem.Method, Is.EqualTo(5)); // Triangulation => 5
            Assert.That(samplesItem.Operand, Is.EqualTo("O"));
            Assert.That(samplesItem.ExtraPolTol, Is.EqualTo(extraPolTol));
        }

        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            var samples = new Samples("randomName");
            const string quantity = "randomQuantity";
            var existingForceFileItems = Substitute.For<IDictionary<ExtForceFileItem, object>>();

            yield return new TestCaseData(null, quantity, existingForceFileItems);
            yield return new TestCaseData(samples, quantity, null);
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