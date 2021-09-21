using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class MultipleFunctionChartTest
    {
        [Test]
        public void ShowEmpty()
        {
            var view = new MultipleFunctionChart();
            WindowsFormsTestHelper.ShowModal(view);
        }

        private static IEnumerable<IFunction> Create1()
        {
            var result = new Function("f1");
            result.Arguments.Add(new Variable<double>("x"));
            result.Components.Add(new Variable<double>("y"));
            result.BeginEdit(new DefaultEditAction("Filling variables"));
            result.Arguments[0].Values.AddRange(Enumerable.Range(0, 10).Select(i => 0.1*i).ToList());
            result.Components[0].SetValues(Enumerable.Range(0, 10).Select(i => 0.1*i).ToList());
            yield return result;
        }

        private static IEnumerable<IFunction> Create2()
        {
            var result = new Function("f2");
            result.Arguments.Add(new Variable<double>("x"));
            result.Components.Add(new Variable<double>("y"));
            result.Arguments[0].Values.AddRange(Enumerable.Range(0, 10).Select(i => 0.1*i).ToList());
            result.Components[0].SetValues(Enumerable.Range(0, 10).Select(i => 0.01*i*i).ToList());
            yield return result;
        }

        [Test]
        public void ShowWithData()
        {
            var view = new MultipleFunctionChart
                {
                    AvailableFunctions =
                        new Dictionary<string, Func<IEnumerable<IFunction>>> {{"linear", Create1}, {"quadratic", Create2}}
                };
            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}
