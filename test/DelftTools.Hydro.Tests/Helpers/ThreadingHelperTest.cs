using System.Linq;
using NUnit.Framework;
using DelftTools.Hydro.Helpers;

namespace DelftTools.Hydro.Tests.Helpers
{
    [TestFixture]
    public class ThreadingHelperTest
    {
        [Test]
        [RequiresMTA]
        public void ConvertMultiThreaded()
        {
            var numbers = Enumerable.Range(0, 5000).ToList();

            var oneAndAHalfTimes = numbers.ConvertMultiThreaded(i => i * 1.5);

            Assert.AreEqual(numbers[0], oneAndAHalfTimes[0]);
            Assert.AreEqual(numbers[2000] * 1.5, oneAndAHalfTimes[2000]);
            Assert.AreEqual(numbers[4999] * 1.5, oneAndAHalfTimes[4999]);
        }
    }
}