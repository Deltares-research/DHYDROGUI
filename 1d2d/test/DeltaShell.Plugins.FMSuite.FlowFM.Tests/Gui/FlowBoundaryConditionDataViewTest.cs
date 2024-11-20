using System;
using System.Reflection;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class FlowBoundaryConditionDataViewTest
    {
        [Test]
        public void HarmonicNoArgumentsTest()
        {
            var function = new Function { Name = "HarmonicTestFunction" };
            Assert.AreEqual("HarmonicTestFunction", function.Name);

            var expectedA1 = 30.0;
            var error = Assert.Throws<NotSupportedException>(() =>
            {
                try
                {
                    var result = (bool)TypeUtils.CallPrivateStaticMethod(typeof(FlowBoundaryConditionDataView),
                        "ApplyHarmonicComponentValues", new object[] { new[] { expectedA1 }, function });
                }

                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }
                    Assert.Fail(ex.Message);
                }
            });
            Assert.AreEqual("Function has no arguments", error.Message);
        }

        [Test]
        public void ApplyThreeHarmonicValuesTest()
        {

            var function = new Function { Name = "HarmonicTestFunction" };
            Assert.AreEqual("HarmonicTestFunction", function.Name);

            function.Arguments.Add(new Variable<double>("Arg1", new Unit("Arg1", "a1")));
            function.Arguments[0].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp1", new Unit("Comp1", "c1")));
            function.Components[0].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp2", new Unit("Comp2", "c2")));
            function.Components[1].SetValues(new[] { 0.0 });

            var expectedA1 = 30.0;
            var expectedC1 = 20.0;
            var expectedC2 = 10.0;

            var runOne = (bool)TypeUtils.CallPrivateStaticMethod(typeof(FlowBoundaryConditionDataView),
                "ApplyHarmonicComponentValues", new object[] { new[] { expectedA1, expectedC1, expectedC2 }, function });
            Assert.IsTrue(runOne);

            double resultA1 = (double)function.Arguments[0].Values[0];
            double resultC1 = (double)function.Components[0].Values[0];
            double resultC2 = (double)function.Components[1].Values[0];

            Assert.AreEqual(expectedA1, resultA1);
            Assert.AreEqual(expectedC1, resultC1);
            Assert.AreEqual(expectedC2, resultC2);
        }

        [Test]
        public void ApplyFiveHarmonicValuesTest()
        {
            var function = new Function { Name = "HarmonicTestFunction" };
            Assert.AreEqual("HarmonicTestFunction", function.Name);

            function.Arguments.Add(new Variable<double>("Arg1", new Unit("Arg1", "a1")));
            function.Arguments[0].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp1", new Unit("Comp1", "c1")));
            function.Components[0].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp2", new Unit("Comp2", "c2")));
            function.Components[1].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp3", new Unit("Comp3", "c3")));
            function.Components[2].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp4", new Unit("Comp4", "c4")));
            function.Components[3].SetValues(new[] { 0.0 });

            var expectedA1 = 30.0;
            var expectedC1 = 20.0;
            var expectedC2 = 10.0;
            var expectedC3 = 5.0;
            var expectedC4 = 2.5;

            var runOne = (bool)TypeUtils.CallPrivateStaticMethod(typeof(FlowBoundaryConditionDataView),
                "ApplyHarmonicComponentValues", new object[] { new[] { expectedA1, expectedC1, expectedC2, expectedC3, expectedC4 }, function });
            Assert.IsTrue(runOne);

            double resultA1 = (double)function.Arguments[0].Values[0];
            double resultC1 = (double)function.Components[0].Values[0];
            double resultC2 = (double)function.Components[1].Values[0];
            double resultC3 = (double)function.Components[2].Values[0];
            double resultC4 = (double)function.Components[3].Values[0];

            Assert.AreEqual(expectedA1, resultA1);
            Assert.AreEqual(expectedC1, resultC1);
            Assert.AreEqual(expectedC2, resultC2);
            Assert.AreEqual(expectedC3, resultC3);
            Assert.AreEqual(expectedC4, resultC4);
        }

        [Test]
        public void ApplyIncorrectFourHarmonicValuesTest()
        {
            var function = new Function { Name = "HarmonicTestFunction" };
            Assert.AreEqual("HarmonicTestFunction", function.Name);

            function.Arguments.Add(new Variable<double>("Arg1", new Unit("Arg1", "a1")));
            function.Arguments[0].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp1", new Unit("Comp1", "c1")));
            function.Components[0].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp2", new Unit("Comp2", "c2")));
            function.Components[1].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp3", new Unit("Comp3", "c3")));
            function.Components[2].SetValues(new[] { 0.0 });

            var expectedA1 = 30.0;
            var expectedC1 = 20.0;
            var expectedC2 = 10.0;
            var expectedC3 = 5.0;

            var error = Assert.Throws<NotSupportedException>(() =>
            {
                try
                {
                    var result = (bool)TypeUtils.CallPrivateStaticMethod(typeof(FlowBoundaryConditionDataView),
                        "ApplyHarmonicComponentValues",
                        new object[] { new[] { expectedA1, expectedC1, expectedC2, expectedC3 }, function });
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }
                    Assert.Fail(ex.Message);
                }
            });
            Assert.AreEqual("Incorrect number of components", error.Message);
        }

        [Test]
        public void ApplyIncorrectSixHarmonicValuesTest()
        {
            var function = new Function { Name = "HarmonicTestFunction" };
            Assert.AreEqual("HarmonicTestFunction", function.Name);

            function.Arguments.Add(new Variable<double>("Arg1", new Unit("Arg1", "a1")));
            function.Arguments[0].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp1", new Unit("Comp1", "c1")));
            function.Components[0].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp2", new Unit("Comp2", "c2")));
            function.Components[1].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp3", new Unit("Comp3", "c3")));
            function.Components[2].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp4", new Unit("Comp4", "c4")));
            function.Components[3].SetValues(new[] { 0.0 });
            function.Components.Add(new Variable<double>("TestComp5", new Unit("Comp5", "c5")));
            function.Components[4].SetValues(new[] { 0.0 });

            var expectedA1 = 30.0;
            var expectedC1 = 20.0;
            var expectedC2 = 10.0;
            var expectedC3 = 5.0;
            var expectedC4 = 2.5;
            var expectedC5 = 1.25;
            try
            {
                var result = (bool)TypeUtils.CallPrivateStaticMethod(typeof(FlowBoundaryConditionDataView),
                    "ApplyHarmonicComponentValues", new object[] { new[] { expectedA1, expectedC1, expectedC2, expectedC3, expectedC4, expectedC5 }, function });
            }

            catch (Exception ex)
            {
                Assert.NotNull(ex.InnerException);
                Assert.AreEqual("Incorrect number of components", ex.InnerException.Message);
            }
        }

        [Test]
        public void ApplyAstroComponentTest()
        {
            var dataView = new FlowBoundaryConditionDataView();

            MethodInfo applyAstroComponentMethod = dataView.GetType().GetMethod("ApplyAstroComponentSelection", BindingFlags.Static | BindingFlags.NonPublic);
            if (applyAstroComponentMethod == null)
            {
                Assert.Fail(string.Format("Could not find method '{0}'", "ApplyAstroComponentSelection"));
            }

            var function = new Function { Name = "AstroTestFunction" };
            Assert.AreEqual("AstroTestFunction", function.Name);

            function.Arguments.Add(new Variable<string>("Component"));

            var expected = "Q1";
            var success = (bool)applyAstroComponentMethod.Invoke(typeof(bool), new object[] { new[] { expected }, function });
            Assert.IsTrue(success);
            string result = (string)function.Arguments[0].Values[0];
            Assert.AreEqual(expected, result);

            string secondExpected = "AnotherComponent";
            var secondSucess = (bool)applyAstroComponentMethod.Invoke(typeof(bool), new object[] { new[] { secondExpected }, function });
            Assert.IsTrue(secondSucess);
            string secondResult = (string)function.Arguments[0].Values[0];
            Assert.AreEqual(secondExpected, secondResult);
            Assert.IsTrue(function.Arguments[0].Values.Count == 1);
        }

    }
}