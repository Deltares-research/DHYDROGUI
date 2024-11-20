using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class FactorRuleTests
    {
        [Test]
        public void CopyFromAndClone()
        {
            var source = new FactorRule
            {
                Name = "test",
                Factor = 25.0
            };

            var newRule = new FactorRule();

            newRule.CopyFrom(source);

            // test for base class data 
            Assert.AreEqual(source.Name, newRule.Name);
            for (var i = 0; i < source.Function.Arguments[0].Values.Count; i++)
            {
                Assert.AreEqual(source.Function.Arguments[0].Values[i], newRule.Function.Arguments[0].Values[i]);
                Assert.AreEqual(source.Function.Components[0].Values[i], newRule.Function.Components[0].Values[i]);
            }

            // factor rule data
            Assert.AreEqual(source.Factor, newRule.Factor);

            var clone = (HydraulicRule) source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }
    }
}