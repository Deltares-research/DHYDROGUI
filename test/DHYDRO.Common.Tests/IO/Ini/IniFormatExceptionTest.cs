using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini
{
    [TestFixture]
    public class IniFormatExceptionTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            var exception = new IniFormatException("message", "line", 10);

            Assert.That(exception.Message, Is.EqualTo("message"));
            Assert.That(exception.Line, Is.EqualTo("line"));
            Assert.That(exception.LineNumber, Is.EqualTo(10));
        }
    }
}