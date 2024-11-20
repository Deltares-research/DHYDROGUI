using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.ModelApiControllers
{
    [TestFixture]
    public class AggregationBufferTest
    {
        private AggregationBuffer buffer;
        private object itemOne;
        private object itemTwo;
        private double[] values;
        private double[] values2;

        [SetUp]
        public void SetUp()
        {
            buffer = new AggregationBuffer();
            itemOne = new object();
            itemTwo = new object();
            values = new double[] { 1, 2, 3, 4, 5, 6 };
            values2 = new double[] { 6, 5, 4, 3, 2, 1 };
        }

        [Test]
        public void TestAverage()
        {

            buffer.AddToBuffer(itemOne, AggregationBuffer.AggregationType.Average, values);
            buffer.AddToBuffer(itemOne, AggregationBuffer.AggregationType.Average, values2);
            buffer.AddToBuffer(itemOne, AggregationBuffer.AggregationType.Average, values2);

            Assert.That(buffer.GetOutputAndClearBuffer(itemOne), Is.EqualTo(new[] { 4.333, 4, 3.666, 3.333, 3, 2.666 }).Within(0.001));

            buffer.AddToBuffer(itemOne, AggregationBuffer.AggregationType.Average, values);
            Assert.AreEqual(values, buffer.GetOutputAndClearBuffer(itemOne));

        }

        [Test]
        public void TestMinimum()
        {
            buffer.AddToBuffer(itemTwo, AggregationBuffer.AggregationType.Minimum, values2);
            buffer.AddToBuffer(itemTwo, AggregationBuffer.AggregationType.Minimum, values);
            buffer.AddToBuffer(itemTwo, AggregationBuffer.AggregationType.Minimum, values);
            Assert.AreEqual(new double[] { 1, 2, 3, 3, 2, 1 }, buffer.GetOutputAndClearBuffer(itemTwo));
        }

        [Test]
        public void TestSum()
        {
            buffer.AddToBuffer(itemTwo, AggregationBuffer.AggregationType.Sum, values2);
            buffer.AddToBuffer(itemTwo, AggregationBuffer.AggregationType.Sum, values);
            buffer.AddToBuffer(itemTwo, AggregationBuffer.AggregationType.Sum, values);
            Assert.AreEqual(new double[] {8, 9, 10, 11, 12, 13}, buffer.GetOutputAndClearBuffer(itemTwo));
        }

        [Test]
        public void TestLast()
        {
            buffer.AddToBuffer(itemOne, AggregationBuffer.AggregationType.Last, values2);
            buffer.AddToBuffer(itemOne, AggregationBuffer.AggregationType.Last, values);
            buffer.AddToBuffer(itemOne, AggregationBuffer.AggregationType.Last, values);
            Assert.AreEqual(values, buffer.GetOutputAndClearBuffer(itemOne));
        }
    }
}