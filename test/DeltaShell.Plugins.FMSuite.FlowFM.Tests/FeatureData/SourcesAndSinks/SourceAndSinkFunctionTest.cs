using System;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData.SourcesAndSinks
{
    [TestFixture]
    public class SourceAndSinkFunctionTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var function = new SourceAndSinkFunction();

            // Assert
            IEventedList<IVariable> arguments = function.Arguments;
            Assert.That(arguments, Has.Count.EqualTo(1));
            Assert.That(arguments[0].Name, Is.EqualTo("Time"));
            Assert.That(arguments[0].DefaultValue, Is.EqualTo(DateTime.Today));

            IEventedList<IVariable> components = function.Components;
            Assert.That(components, Has.Count.EqualTo(4));
            AssertComponent(components[0], "Discharge", "cubic meters per second", "m3/s");
            AssertComponent(components[1], "Salinity", "parts per trillion", "ppt");
            AssertComponent(components[2], "Temperature", "degree celsius", "°C");
            AssertComponent(components[3], "Secondary Flow", "meters per second", "m/s");
        }

        [Test]
        public void AddTracer_AddsCorrectVariable()
        {
            // Setup
            var function = new SourceAndSinkFunction();

            // Call
            function.AddTracer("Some Tracer 1");
            function.AddTracer("Some Tracer 2");
            function.AddTracer("Some Tracer 3");

            // Assert
            Assert.That(function.Components, Has.Count.EqualTo(7));
            AssertComponent(function.Components[4], "Some Tracer 1", "kilograms per cubic meter", "kg/m3");
            AssertComponent(function.Components[5], "Some Tracer 2", "kilograms per cubic meter", "kg/m3");
            AssertComponent(function.Components[6], "Some Tracer 3", "kilograms per cubic meter", "kg/m3");
        }

        [Test]
        public void AddSedimentFraction_AddsCorrectVariable()
        {
            // Setup
            var function = new SourceAndSinkFunction();

            // Call
            function.AddSedimentFraction("Some Sediment Fraction 1");
            function.AddSedimentFraction("Some Sediment Fraction 2");
            function.AddSedimentFraction("Some Sediment Fraction 3");

            // Assert
            Assert.That(function.Components, Has.Count.EqualTo(7));
            AssertComponent(function.Components[3], "Some Sediment Fraction 1", "", "");
            AssertComponent(function.Components[4], "Some Sediment Fraction 2", "", "");
            AssertComponent(function.Components[5], "Some Sediment Fraction 3", "", "");
        }

        [Test]
        public void RemoveTracer_RemovesCorrectVariable()
        {
            // Setup
            var function = new SourceAndSinkFunction();
            function.AddTracer("Some Tracer 1");
            function.AddTracer("Some Tracer 2");
            function.AddTracer("Some Tracer 3");

            // Call
            function.RemoveTracer("Some Tracer 2");

            // Assert
            Assert.That(function.Components, Has.Count.EqualTo(6));
            AssertComponent(function.Components[4], "Some Tracer 1", "kilograms per cubic meter", "kg/m3");
            AssertComponent(function.Components[5], "Some Tracer 3", "kilograms per cubic meter", "kg/m3");
        }

        [Test]
        public void RemoveSedimentFraction_RemovesCorrectVariable()
        {
            // Setup
            var function = new SourceAndSinkFunction();
            function.AddSedimentFraction("Some Sediment Fraction 1");
            function.AddSedimentFraction("Some Sediment Fraction 2");
            function.AddSedimentFraction("Some Sediment Fraction 3");

            // Call
            function.RemoveSedimentFraction("Some Sediment Fraction 2");

            // Assert
            Assert.That(function.Components, Has.Count.EqualTo(6));
            AssertComponent(function.Components[3], "Some Sediment Fraction 1", "", "");
            AssertComponent(function.Components[4], "Some Sediment Fraction 3", "", "");
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddTracer_ArgumentNullOrEmpty_ThrowsArgumentException(string name)
        {
            // Setup
            var function = new SourceAndSinkFunction();

            // Call
            void Call() => function.AddTracer(name);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("name"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddSedimentFraction_ArgumentNullOrEmpty_ThrowsArgumentException(string name)
        {
            // Setup
            var function = new SourceAndSinkFunction();

            // Call
            void Call() => function.AddSedimentFraction(name);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("name"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void RemoveTracer_ArgumentNullOrEmpty_ThrowsArgumentException(string name)
        {
            // Setup
            var function = new SourceAndSinkFunction();

            // Call
            void Call() => function.RemoveTracer(name);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("name"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void RemoveSedimentFraction_ArgumentNullOrEmpty_ThrowsArgumentException(string name)
        {
            // Setup
            var function = new SourceAndSinkFunction();

            // Call
            void Call() => function.RemoveSedimentFraction(name);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("name"));
        }

        private static void AssertComponent(IVariable component, string name, string unitDescription, string unitSymbol)
        {
            Assert.That(component.Name, Is.EqualTo(name));
            Assert.That(component.Unit.Name, Is.EqualTo(unitDescription));
            Assert.That(component.Unit.Symbol, Is.EqualTo(unitSymbol));
        }
    }
}