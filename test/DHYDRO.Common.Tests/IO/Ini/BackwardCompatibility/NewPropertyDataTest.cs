using System.Collections.Generic;
using DHYDRO.Common.IO.Ini.BackwardCompatibility;
using NSubstitute;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini.BackwardCompatibility
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
            const string key = "random key";
            var updater = Substitute.For<IPropertyUpdater>();

            // Call
            var data = new NewPropertyData(key, updater);

            // Assert
            Assert.That(data.Key, Is.EqualTo(key));
            Assert.That(data.Updater, Is.SameAs(updater));
        }

        private static IEnumerable<TestCaseData> GetArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IPropertyUpdater>());
            yield return new TestCaseData(string.Empty, null);
        }
    }
}