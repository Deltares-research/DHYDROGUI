using System.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Modals.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.Modals.Helpers
{
    [TestFixture]
    public class RequestUserInputServiceTest
    {
        private enum TestEnum { }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var service = new RequestUserInputService<TestEnum>();

            // Assert
            Assert.That(service, Is.InstanceOf<IRequestUserInputService<TestEnum>>());
        }

        private static IEnumerable<TestCaseData> GetRequestUserInputParameterNullData()
        {
            yield return new TestCaseData(null, "someText", "title");
            yield return new TestCaseData("someTitle", null, "text");
        }

        [Test]
        [TestCaseSource(nameof(GetRequestUserInputParameterNullData))]
        public void RequestUserInput_ParameterNull_ThrowsArgumentNullException(string title,
                                                                               string text,
                                                                               string expectedParameterName)
        {
            // Setup
            var service = new RequestUserInputService<TestEnum>();

            // Call | Assert
            void Call() => service.RequestUserInput(title, text);
            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }
    }
}