using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.CustomRenderers;
using DeltaShell.Plugins.NetworkEditor.Gui;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    class NetworkLayerStyleFactoryTest
    {
        private static IEnumerable<TestCaseData> TestStyles()
        {
            yield return new  TestCaseData(
                    Substitute.For<IEventedList<HydroLink>>(), 
                    false,
                    Color.FromArgb(80, Color.Chocolate),
                    null,
                    (Action<VectorStyle, Color, Bitmap>)ValidateHydroLink)
                .SetName("Test checking style of HydroLinks");

            yield return new  TestCaseData(
                    Substitute.For<IEventedList<HydroLink>>(), 
                    true,
                    Color.FromArgb(80, Color.FromArgb(80, Color.DarkCyan)),
                    null,
                    (Action<VectorStyle, Color, Bitmap>)ValidateHydroLink)
                .SetName("Test checking style of HydroLinks, alternate color");
            
            yield return new  TestCaseData(
                    Substitute.For<IEventedList<WasteWaterTreatmentPlant>>(), 
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.wwtp,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style of WasteWaterTreatmentPlant");

            yield return new  TestCaseData(
                    Substitute.For<IEventedList<RunoffBoundary>>(), 
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.runoff,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style of RunoffBoundary");

            yield return new TestCaseData(
                    Substitute.For<IEventedList<Catchment>>(),
                    null,
                    Color.FromArgb(50, Color.LightSkyBlue),
                    null,
                    (Action<VectorStyle, Color, Bitmap>)ValidateCatchmentStyle)
                .SetName("Test checking style of Catchments");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<ILateralSource>(), 10),
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.LateralSourceMap,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style of ILateralSource");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<IManhole>(), 10),
                    null,
                    Color.Orange,
                    null,
                    (Action<VectorStyle, Color, Bitmap>)ValidateManholeStyle)
                .SetName("Test checking style of IManhole");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<IRetention>(), 10),
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.Retention,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style of IRetention");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<IObservationPoint>(), 10),
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.Observation,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style of IObservationPoint");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<IPump>(), 10),
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.pump,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style of IPump");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<IWeir>(), 10),
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.WeirSmall,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style of IWeir");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<IOrifice>(), 10),
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.Gate,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style of IOrifice");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<OutletCompartment>(), 10),
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.Outlet,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style of OutletCompartment");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<ICulvert>(), 10),
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.CulvertSmall,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style ICulvert");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<IBridge>(), 10),
                    null,
                    null,
                    NetworkEditor.Gui.Properties.Resources.BridgeSmall,
                    (Action<VectorStyle, Color, Bitmap>)ValidateImage)
                .SetName("Test checking style IBridge");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<ICompositeBranchStructure>(), 10),
                    null,
                    Color.SpringGreen,
                    null,
                    (Action<VectorStyle, Color, Bitmap>)ValidateCompositeBranchStructure)
                .SetName("Test checking style of ICompositeBranchStructure");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<ICrossSection>(), 10),
                    null,
                    Color.Tomato,
                    null,
                    (Action<VectorStyle, Color, Bitmap>)ValidateCrossSectionStyle)
                .SetName("Test checking style of ICrossSection");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<IChannel>(), 10),
                    null,
                    Color.SteelBlue,
                    null,
                    (Action<VectorStyle, Color, Bitmap>)ValidateChannelStyle)
                .SetName("Test checking style of IChannel");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<IPipe>(), 10),
                    null,
                    Color.Black,
                    null,
                    (Action<VectorStyle, Color, Bitmap>)ValidatePipeStyle)
                .SetName("Test checking style of IPipe");

            yield return new TestCaseData(
                    Enumerable.Repeat(Substitute.For<ISewerConnection>(), 10),
                    null,
                    Color.CadetBlue,
                    null,
                    (Action<VectorStyle, Color, Bitmap>)ValidateSewerStyle)
                .SetName("Test checking style of ISewerConnection");

        }

        private static void ValidateSewerStyle(VectorStyle vectorStyle, Color color, Bitmap bitmap)
        {
            CheckVectorStyleLineColor(vectorStyle, color);
            Assert.That(vectorStyle.EnableOutline, Is.False);
            Assert.That(vectorStyle.GeometryType, Is.EqualTo(typeof(ILineString)));
        }

        private static void ValidatePipeStyle(VectorStyle vectorStyle, Color color, Bitmap bitmap)
        {
            CheckVectorStyleLineColor(vectorStyle, color);
            Assert.That(vectorStyle.GeometryType, Is.EqualTo(typeof(ILineString)));
        }

        private static void ValidateChannelStyle(VectorStyle vectorStyle, Color color, Bitmap bitmap)
        {
            CheckVectorStyleLineColor(vectorStyle, color);
            Assert.That(vectorStyle.EnableOutline, Is.False);
            var channelLineCustomEndCap = vectorStyle.Line.CustomEndCap as AdjustableArrowCap;
            Assert.That(channelLineCustomEndCap, Is.Not.Null);
            Assert.That(channelLineCustomEndCap.Filled, Is.True);
            Assert.That(channelLineCustomEndCap.BaseCap, Is.EqualTo(LineCap.Triangle));
            Assert.That(vectorStyle.GeometryType, Is.EqualTo(typeof(ILineString)));
        }

        private static void ValidateCrossSectionStyle(VectorStyle vectorStyle, Color color, Bitmap bitmap)
        {
            CheckVectorStyleFillColor(vectorStyle, color);
            CheckVectorStyleLineColor(vectorStyle, Color.Indigo);
            Assert.That(vectorStyle.GeometryType, Is.EqualTo(typeof(ILineString)));
        }

        private static void ValidateCompositeBranchStructure(VectorStyle vectorStyle, Color color, Bitmap bitmap)
        {
            CheckVectorStyleFillColor(vectorStyle, color);
            CheckVectorStyleLineColor(vectorStyle, Color.Black);
            Assert.That(vectorStyle.GeometryType, Is.EqualTo(typeof(IPolygon)));
        }

        private static void ValidateManholeStyle(VectorStyle vectorStyle, Color color, Bitmap bitmap)
        {
            CheckVectorStyleFillColor(vectorStyle, color);
            Assert.That(vectorStyle.GeometryType, Is.EqualTo(typeof(IPoint)));
            CheckVectorStyleOutlineColor(vectorStyle, Color.FromArgb(255, Color.Black));
            Assert.That(vectorStyle.Shape, Is.EqualTo(ShapeType.Ellipse));
            Assert.That(vectorStyle.ShapeSize, Is.EqualTo(11));
        }

        private static void ValidateCatchmentStyle(VectorStyle vectorStyle, Color color, Bitmap bitmap)
        {
            CheckVectorStyleFillColor(vectorStyle, color);
            CheckVectorStyleOutlineColor(vectorStyle, Color.FromArgb(100, Color.DarkBlue));
            Assert.That(vectorStyle.GeometryType, Is.EqualTo(typeof(IPolygon)));
        }

        private static void ValidateImage(VectorStyle vectorStyle, Color color, Bitmap bitmap)
        {
            Assert.That(NetworkLayerFactoryTestHelper.CompareImages(vectorStyle.Symbol, bitmap), Is.True);
            Assert.That(vectorStyle.GeometryType, Is.EqualTo(typeof(IPoint)));
        }

        private static void ValidateHydroLink(VectorStyle vectorStyle, Color color, Bitmap bitmap)
        {
            CheckVectorStyleLineColor(vectorStyle, color);
            Assert.That(vectorStyle.GeometryType, Is.EqualTo(typeof(ILineString)));
            var linkCap = vectorStyle.Line.CustomEndCap as AdjustableArrowCap;
            Assert.That(linkCap, Is.Not.Null);
            Assert.That(linkCap.Filled, Is.True);
            Assert.That(linkCap.BaseCap, Is.EqualTo(LineCap.Triangle));
            Assert.That(vectorStyle.Line.DashStyle, Is.EqualTo(DashStyle.Dash));
            Assert.That(vectorStyle.EnableOutline, Is.False);
        }

        private static void CheckVectorStyleLineColor(VectorStyle vectorStyle, Color color)
        {
            Assert.That(vectorStyle.Line.Color, Is.EqualTo(color), $"Expected vector style line color {color} but it is set to {vectorStyle.Line.Color}");
        }

        private static void CheckVectorStyleOutlineColor(VectorStyle vectorStyle, Color color)
        {
            Assert.That(vectorStyle.Outline.Color, Is.EqualTo(color), $"Expected vector style outline color {color} but it is set to {vectorStyle.Outline.Color}");
        }
        private static void CheckVectorStyleFillColor(VectorStyle vectorStyle, Color color)
        {
            Color actual = ((SolidBrush)vectorStyle.Fill).Color;
            Assert.That(actual, Is.EqualTo(color), $"Expected vector style fill color {color} but it is set to {actual}");
        }

        [TestCaseSource(nameof(TestStyles))]
        public void GivenAnEnumerableAndBoolForIndicatingAlternativeStyleWhenGeneratingThemeThenExpectColourForThisStyle(IEnumerable networkObjects, bool? useAlternativeStyle, Color color, Bitmap checkBitmap, Action<VectorStyle,Color,Bitmap> validate)
        {
            var style = useAlternativeStyle.HasValue 
                            ? NetworkLayerStyleFactory.CreateStyle(networkObjects, useAlternativeStyle.Value)
                            : NetworkLayerStyleFactory.CreateStyle(networkObjects);
            Assert.That(style, Is.Not.Null);
            validate(style, color, checkBitmap);
        }

        [Test]
        public void GivenAnEnumerableOfObjectWhenGeneratingThemeThenExpectNullObjectForTheme()
        {
            object o = Substitute.For<object>();
            IEnumerable<object> typeLessObjects = Enumerable.Repeat(o, 10);
            Assert.That(NetworkLayerStyleFactory.CreateStyle(typeLessObjects), Is.Null);
            Assert.That(NetworkLayerStyleFactory.CreateStyle(typeLessObjects, true), Is.Null);
        }
    }
}