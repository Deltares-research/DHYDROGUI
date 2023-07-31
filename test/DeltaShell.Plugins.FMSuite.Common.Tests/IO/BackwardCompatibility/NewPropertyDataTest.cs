using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility
{
    [TestFixture]
    public class NewPropertyDataTest
    {
        [Test]
        [TestCaseSource(nameof(GetArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(string name, IPropertyUpdater converter)
        {
            // Call
            void Call() => _ = new NewPropertyData(name, converter);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_ExpectedArguments()
        {
            // Setup
            const string name = "random name";
            var updater = Substitute.For<IPropertyUpdater>();

            // Call
            var data = new NewPropertyData(name, updater);

            // Assert
            Assert.That(data.Name, Is.EqualTo(name));
            Assert.That(data.Updater, Is.SameAs(updater));
        }

        private static IEnumerable<TestCaseData> GetArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IPropertyUpdater>());
            yield return new TestCaseData(string.Empty, null);
        }
    }
}