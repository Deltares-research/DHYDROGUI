using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.TestUtils
{
    [TestFixture]
    public class ToDictionaryExtensionsTest
    {
        [Test]
        [TestCaseSource(nameof(GetConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(
            IEnumerable<string> source,
            string context,
            Func<string, string> keySelector,
            Func<string, string> valueSelector,
            string parameterName)
        {
            // Call
            Dictionary<string, string> Call() => source.ToDictionaryWithErrorDetails(context, keySelector, valueSelector);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException
                                    .With.Property(nameof(ArgumentNullException.ParamName)).EqualTo(parameterName));
        }

        private static IEnumerable<TestCaseData> GetConstructorArgumentNullCases()
        {
            IEnumerable<string> source = Enumerable.Empty<string>();
            var context = string.Empty;
            var keySelector = new Func<string, string>(s => s);
            var valueSelector = new Func<string, string>(s => s);

            yield return new TestCaseData(null, context, keySelector, valueSelector, "source").SetName("Source null");
            yield return new TestCaseData(source, null, keySelector, valueSelector, "context").SetName("Context null");
            yield return new TestCaseData(source, context, null, valueSelector, "keySelector").SetName("KeySelector null");
            yield return new TestCaseData(source, context, keySelector, null, "valueSelector").SetName("ValueSelector null");
        }

        [Test]
        public void ToDictionaryWithErrorDetails_CollectionWithDuplicateValues_ThrowsArgumentExceptionAndReportsDuplicatesInErrorMessage()
        {
            // Setup
            int[] items = Enumerable.Range(0, 1000000).Select(i => i).ToArray();

            // create duplicates
            items[4928] = 2;
            items[728] = 2;
            items[3428] = 4;
            items[4258] = 6;

            // Call
            void Call() => items.ToDictionaryWithErrorDetails("Nodes", i => i);

            // Assert
            string expectedMessage = "The following entries were not unique in Nodes: \r\n" +
                                     "2 at indices (2, 728, 4928)\r\n" +
                                     "4 at indices (4, 3428)\r\n" +
                                     "6 at indices (6, 4258)";

            Assert.That(Call, Throws.ArgumentException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        public void ToDictionaryWithErrorDetails_CollectionWithoutDuplicateValues_DoesNotThrowException()
        {
            // Setup
            int[] items = Enumerable.Range(0, 1000000).Select(i => i).ToArray();

            // Call
            void Call() => items.ToDictionaryWithErrorDetails("Nodes", i => i);

            // Assert
            Assert.That(Call, Throws.Nothing);
        }
    }
}