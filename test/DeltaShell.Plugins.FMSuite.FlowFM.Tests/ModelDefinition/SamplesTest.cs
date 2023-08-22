using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ModelDefinition
{
    [TestFixture]
    public class SamplesTest
    {
        [Test]
        [TestCaseSource(nameof(GetNullOrWhiteSpaceTestCases))]
        public void Constructor_NameNullOrWhiteSpace_ThrowsException(string name)
        {
            // Call
            void Call() => new Samples(name);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        private static IEnumerable<TestCaseData> GetNullOrWhiteSpaceTestCases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData("");
            yield return new TestCaseData("   ");
            yield return new TestCaseData(Environment.NewLine);
        }
    }
}