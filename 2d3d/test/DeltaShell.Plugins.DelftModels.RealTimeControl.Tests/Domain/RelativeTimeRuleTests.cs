using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class RelativeTimeRuleTests
    {
        private Function tableFunction;

        [SetUp]
        public void SetUp()
        {
            tableFunction = RelativeTimeRule.DefineFunction();
            tableFunction[0.0] = 1.2;
            tableFunction[60.0] = 3.4;
            tableFunction[120.0] = 5.6;
            tableFunction[180.0] = 7.8;
        }

        [Test]
        public void CopyFromAndClone()
        {
            var source = new RelativeTimeRule
            {
                Name = "test",
                FromValue = false,
                Interpolation = InterpolationType.Linear
            };

            var newRule = new RelativeTimeRule();
            double[] argumentValues = new[]
            {
                60,
                120.0,
                360.0
            };
            var componentValues = new[]
            {
                8.0,
                9.0,
                10.0
            };
            for (var i = 0; i < argumentValues.Count(); i++)
            {
                source.Function[argumentValues[i]] = componentValues[i];
            }

            newRule.CopyFrom(source);

            Assert.AreEqual(source.Name, newRule.Name);
            for (var i = 0; i < source.Function.Arguments[0].Values.Count; i++)
            {
                Assert.AreEqual(source.Function.Arguments[0].Values[i], newRule.Function.Arguments[0].Values[i]);
                Assert.AreEqual(source.Function.Components[0].Values[i], newRule.Function.Components[0].Values[i]);
            }

            Assert.AreEqual(source.FromValue, newRule.FromValue);
            Assert.AreEqual(source.Interpolation, newRule.Interpolation);

            var clone = (RelativeTimeRule) source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }
    }
}