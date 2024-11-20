using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Editors
{
    [TestFixture]
    public class BoundaryConditionExtensionsTest
    {
        [Test]
        public void ApplyForSupportPointModeTest()
        {
            var mocks = new MockRepository();
            var bcDataPointIndices = mocks.DynamicMock<IEventedList<int>>();
            bcDataPointIndices.Expect(bcdpi => bcdpi.Contains(Arg<int>.Is.Anything)).IgnoreArguments().Return(false).Repeat.Times(6);
            var bcFeatGeometry = mocks.DynamicMock<IGeometry>();
            bcFeatGeometry.Expect(bcfg => bcfg.Coordinates).Return(new Coordinate[3]);
            var bcFeature = mocks.DynamicMock<IFeature>();
            bcFeature.Expect(bcf => bcf.Geometry).Return(bcFeatGeometry).Repeat.Times(4);

            var boundaryCondition = mocks.DynamicMock<IBoundaryCondition>();
            boundaryCondition.Expect(bc => bc.BeginEdit("")).IgnoreArguments().Repeat.Once();
            boundaryCondition.Expect(bc => bc.EndEdit()).IgnoreArguments().Repeat.Once();
            boundaryCondition.Expect(bc => bc.Feature).Return(bcFeature).Repeat.Times(4);
            boundaryCondition.Expect(bc => bc.DataPointIndices).Return(bcDataPointIndices).Repeat.Times(6);

            double[] array = new[]
            {
                10.0,
                5,
                2.5
            };
            double[] array2 = new[]
            {
                20.0,
                10,
                5
            };
            double[] array3 = new[]
            {
                30.0,
                15,
                7.5
            };
            double[] array4 = new[]
            {
                40.0,
                20,
                10
            };

            var function1 = new Function {Name = "HarmonicTestFunction"};
            function1.Arguments.Add(new Variable<double>("Arg1", new Unit("Arg1", "a1")));
            function1.Arguments[0].SetValues(new[]
            {
                0.0
            });
            function1.Components.Add(new Variable<double>("TestComp1", new Unit("Comp1", "c1")));
            function1.Components[0].SetValues(new[]
            {
                0.0
            });
            function1.Components.Add(new Variable<double>("TestComp2", new Unit("Comp2", "c2")));
            function1.Components[1].SetValues(new[]
            {
                0.0
            });

            var function2 = new Function {Name = "HarmonicTestFunction2"};
            function2.Arguments.Add(new Variable<double>("Arg1", new Unit("Arg1", "a1")));
            function2.Arguments[0].SetValues(new[]
            {
                0.0
            });
            function2.Components.Add(new Variable<double>("TestComp1", new Unit("Comp1", "c1")));
            function2.Components[0].SetValues(new[]
            {
                0.0
            });
            function2.Components.Add(new Variable<double>("TestComp2", new Unit("Comp2", "c2")));
            function2.Components[1].SetValues(new[]
            {
                0.0
            });

            var function3 = new Function {Name = "HarmonicTestFunction3"};
            function3.Arguments.Add(new Variable<double>("Arg1", new Unit("Arg1", "a1")));
            function3.Arguments[0].SetValues(new[]
            {
                0.0
            });
            function3.Components.Add(new Variable<double>("TestComp1", new Unit("Comp1", "c1")));
            function3.Components[0].SetValues(new[]
            {
                0.0
            });
            function3.Components.Add(new Variable<double>("TestComp2", new Unit("Comp2", "c2")));
            function3.Components[1].SetValues(new[]
            {
                0.0
            });

            boundaryCondition.Expect(bc => bc.GetDataAtPoint(0)).Return(function1).Repeat.Times(4);
            boundaryCondition.Expect(bc => bc.GetDataAtPoint(1)).Return(function3).Repeat.Times(2);
            boundaryCondition.Expect(bc => bc.GetDataAtPoint(2)).Return(function3).Repeat.Times(2);
            boundaryCondition.Expect(bc => bc.PointData).Return(new EventedList<IFunction>()
            {
                function1,
                function3
            }).Repeat.Once();

            Func<IEnumerable<double>, IFunction, bool> applyToFunction1 = (a, f) =>
            {
                Assert.That(a.ElementAt(0), Is.EqualTo(10.0).Within(0.01));
                Assert.That(a.ElementAt(1), Is.EqualTo(5.0).Within(0.01));
                Assert.That(a.ElementAt(2), Is.EqualTo(2.5).Within(0.01));
                Assert.AreEqual("HarmonicTestFunction", f.Name);
                return true;
            };

            Func<IEnumerable<double>, IFunction, bool> applyToFunction2 = (a, f) =>
            {
                Assert.That(a.ElementAt(0), Is.EqualTo(20.0).Within(0.01));
                Assert.That(a.ElementAt(1), Is.EqualTo(10.0).Within(0.01));
                Assert.That(a.ElementAt(2), Is.EqualTo(5.0).Within(0.01));
                Assert.That(f.Name, Does.StartWith("HarmonicTestFunction"));

                return true;
            };

            Func<IEnumerable<double>, IFunction, bool> applyToFunction3 = (a, f) =>
            {
                Assert.That(a.ElementAt(0), Is.EqualTo(30.0).Within(0.01));
                Assert.That(a.ElementAt(1), Is.EqualTo(15.0).Within(0.01));
                Assert.That(a.ElementAt(2), Is.EqualTo(7.5).Within(0.01));
                Assert.That(f.Name, Does.StartWith("HarmonicTestFunction"));

                return true;
            };

            Func<IEnumerable<double>, IFunction, bool> applyToFunction4 = (a, f) =>
            {
                Assert.That(a.ElementAt(0), Is.EqualTo(40.0).Within(0.01));
                Assert.That(a.ElementAt(1), Is.EqualTo(20.0).Within(0.01));
                Assert.That(a.ElementAt(2), Is.EqualTo(10.0).Within(0.01));
                Assert.That(f.Name, Does.StartWith("HarmonicTestFunction"));

                return true;
            };
            mocks.ReplayAll();
            boundaryCondition.ApplyForSupportPointMode(SupportPointMode.SelectedPoint, array, applyToFunction1, "", 0);
            boundaryCondition.ApplyForSupportPointMode(SupportPointMode.ActivePoints, array2, applyToFunction2, "", 0);
            boundaryCondition.ApplyForSupportPointMode(SupportPointMode.InactivePoints, array3, applyToFunction3, "", 0);
            boundaryCondition.ApplyForSupportPointMode(SupportPointMode.AllPoints, array4, applyToFunction4, "", 0);
            mocks.VerifyAll();
        }
    }
}