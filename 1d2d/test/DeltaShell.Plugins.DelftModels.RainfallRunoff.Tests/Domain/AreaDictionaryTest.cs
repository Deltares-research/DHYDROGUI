using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain
{
    [TestFixture]
    public class AreaDictionaryTest
    {
        [Test]
        public void GivenAreaDictionary_ChangingTotalArea_ShouldInvokeSumChangedEvent()
        {
            //Arrange
            var areaDictionary = new AreaDictionary<TestEnum>();

            var lastSum = 0.0;
            areaDictionary.SumChanged += (s, e) => lastSum = e.Sum;

            // Act & Assert
            Assert.AreEqual(0, lastSum);

            areaDictionary.Add(TestEnum.Test1, 10);
            Assert.AreEqual(10, lastSum);

            areaDictionary.Add(TestEnum.Test2, 4.82);
            Assert.AreEqual(14.82, lastSum);

            areaDictionary[TestEnum.Test2] = 5;
            Assert.AreEqual(15, lastSum);

            areaDictionary[TestEnum.Test3] = 0.5;
            Assert.AreEqual(15.5, lastSum);

            areaDictionary.Remove(TestEnum.Test2);
            Assert.AreEqual(10.5, lastSum);

            areaDictionary.Clear();
            Assert.AreEqual(0, lastSum);
        }

        private enum TestEnum
        {
            Test1,
            Test2,
            Test3
        }
    }
}