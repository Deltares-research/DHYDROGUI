using DelftTools.Functions;
using DelftTools.Functions.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Coverages
{
    [TestFixture]
    public class VariableExtensionsTests
    {
        [Test]
        public void VariableExtensionsDoNotCalRemoveEventTest()
        {
            // arrange
            var coverage = new Variable<double>();
            var coverage2 = new Variable<double>();
            var coverage3 = new Variable<double>();
            int i = 0;
            int j = 0;
            int k = 0;
            int count = 100000;
            double value = 80.1;
            coverage.Values.IsAutoSorted = true;
            coverage2.Values.IsAutoSorted = true;
            
            coverage.Values.FireEvents = true;
            coverage2.Values.FireEvents = true;

            coverage.ValuesChanged += (sender, args) =>  i++;
            coverage2.ValuesChanged += (sender, args) =>  j++;
            coverage3.ValuesChanged += (sender, args) =>  k++;

            // act
            FunctionHelper.SetValuesRaw(coverage, Enumerable.Repeat(value, count));
            coverage.ClearWithoutEventing();
            
            FunctionHelper.SetValuesRaw(coverage2, Enumerable.Repeat(value, count));
            coverage2.Clear();
            
            coverage3.ClearWithoutEventing();

            // assert
            Assert.That(coverage.Values.Count, Is.EqualTo(0));
            Assert.That(i, Is.EqualTo(0));
            
            Assert.That(coverage2.Values.Count, Is.EqualTo(0));
            Assert.That(j, Is.Not.EqualTo(0));
            Assert.That(j, Is.EqualTo(count));

            Assert.That(coverage.Values.IsAutoSorted, Is.True);
            Assert.That(coverage2.Values.IsAutoSorted, Is.True);

            Assert.That(coverage.Values.FireEvents, Is.True);
            Assert.That(coverage2.Values.FireEvents, Is.True);

            Assert.That(coverage3.Values.Count, Is.EqualTo(0));
            Assert.That(k, Is.EqualTo(0));
        }

    }
}