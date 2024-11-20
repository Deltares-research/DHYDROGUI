using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class FunctionExtentionsTest
    {
        [Test]
        public void TestRemoveComponentByName()
        {
            // setup
            var variable1 = new Variable<double>("Var1");
            var variable2 = new Variable<double>("Var2");
            var variable3 = new Variable<double>("Var3");

            var function = new Function();
            function.Components.AddRange(new List<IVariable>()
            {
                variable1,
                variable2,
                variable3
            });

            // verification
            Assert.IsTrue(function.Components.Contains(variable1));
            Assert.IsTrue(function.Components.Contains(variable2));
            Assert.IsTrue(function.Components.Contains(variable3));

            // remove existing component
            function.RemoveComponentByName(variable1.Name);

            // validate result
            Assert.IsFalse(function.Components.Contains(variable1));
            Assert.IsTrue(function.Components.Contains(variable2));
            Assert.IsTrue(function.Components.Contains(variable3));

            // remove non-existing component
            function.RemoveComponentByName("Var4");

            // validate result
            Assert.IsFalse(function.Components.Contains(variable1));
            Assert.IsTrue(function.Components.Contains(variable2));
            Assert.IsTrue(function.Components.Contains(variable3));
        }
    }
}