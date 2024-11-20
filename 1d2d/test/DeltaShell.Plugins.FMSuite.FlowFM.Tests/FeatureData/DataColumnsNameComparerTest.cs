using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    public class DataColumnsNameComparerTest
    {
        [Test]
        public void GivenTwoDataColumnsWithTheSameNames_WhenCheckingTheNames_ThenTrueShouldBeReturned ()
        {
            var comparer = new DataColumnsNameComparer();

            var dataColumn1 = new DataColumn<double>("Crest Level");
            var dataColumn2 = new DataColumn<double>("Crest Level");

            var answer = comparer.Equals(dataColumn1, dataColumn2);

            Assert.IsTrue(answer);
        }

        [Test]
        public void GivenTwoDataColumnsWithDifferentNames_WhenCheckingTheNames_ThenFalseShouldBeReturned()
        {
            var comparer = new DataColumnsNameComparer();

            var dataColumn1 = new DataColumn<double>("Crest Level");
            var dataColumn2 = new DataColumn<double>("CrestLevel");

            var answer = comparer.Equals(dataColumn1, dataColumn2);

            Assert.IsFalse(answer);
        }
    }
}