using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain
{
    [TestFixture]
    public class AreaSumChangedEventArgsTest
    {
        [Test]
        public void GivenAreaSumChangedEventArgs_Sum_ShouldBeCorrectlySet()
        {
            //Arrange & Act
            var expectedSum = 2000;
            var areaSumChangedEventArgs = new AreaSumChangedEventArgs(2000);
            
            // Assert
            Assert.AreEqual(expectedSum, areaSumChangedEventArgs.Sum);
        }
    }
}