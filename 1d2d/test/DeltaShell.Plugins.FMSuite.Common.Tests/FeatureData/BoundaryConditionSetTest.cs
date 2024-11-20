using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.FeatureData
{
    [TestFixture]
    public class BoundaryConditionSetTest
    {
        [Test]
        public void BoundaryConditionSetShouldBubbleEvents()
        {
            var set = new BoundaryConditionSet();

            var count = 0;
            set.CollectionChanged += (sender, args) => count++;

            set.BoundaryConditions.Add(new TestBoundaryCondition(BoundaryConditionDataType.Empty,false,false));

            Assert.AreEqual(1, count);
        }
    }
}