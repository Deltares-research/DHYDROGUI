using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Helpers
{
    [TestFixture]
    public class ThreadingHelperTest
    {
        [Test]
        [RequiresMTA]
        public void ConvertMultiThreaded()
        {
            List<int> numbers = Enumerable.Range(0, 5000).ToList();

            IList<double> oneAndAHalfTimes = numbers.ConvertMultiThreaded(i => i * 1.5);

            Assert.AreEqual(numbers[0], oneAndAHalfTimes[0]);
            Assert.AreEqual(numbers[2000] * 1.5, oneAndAHalfTimes[2000]);
            Assert.AreEqual(numbers[4999] * 1.5, oneAndAHalfTimes[4999]);
        }
    }
}