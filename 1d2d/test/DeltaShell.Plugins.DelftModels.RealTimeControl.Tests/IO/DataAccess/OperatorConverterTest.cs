using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    public class OperatorConverterTest
    {
        [TestCase(Operator.Add, "({0} + {1})")]
        [TestCase(Operator.Subtract, "({0} - {1})")]
        [TestCase(Operator.Multiply, "{0} * {1}")]
        [TestCase(Operator.Divide, "{0} / {1}")]
        [TestCase(Operator.Min, "min({0}, {1})")]
        [TestCase(Operator.Max, "max({0}, {1})")]
        public void ToFormatString_ReturnsCorrectResult(Operator @operator, string expectedResult)
        {
            Assert.That(@operator.ToFormatString(), Is.EqualTo(expectedResult));
        }

        [Test]
        public void ToFormatString_OperatorUndefined_ThrowsInvalidEnumArgumentException()
        {
            // Call
            void Call() => ((Operator) 6).ToFormatString();

            // Assert
            var exception = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo("operator"));
        }
    }
}