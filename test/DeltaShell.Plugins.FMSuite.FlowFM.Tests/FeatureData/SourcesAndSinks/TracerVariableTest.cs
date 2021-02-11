using System;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData.SourcesAndSinks
{
    [TestFixture]
    public class TracerVariableTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var variable = new TracerVariable("Some name");

            // Assert
            Assert.That(variable, Is.InstanceOf<Variable<double>>());
            Assert.That(variable.Name, Is.EqualTo("Some name"));
            Assert.That(variable.Unit.Name, Is.EqualTo("kilograms per cubic meter"));
            Assert.That(variable.Unit.Symbol, Is.EqualTo("kg/m3"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Constructor_ArgumentNullOrEmpty_ThrowsArgumentException(string name)
        {
            // Call
            void Call() => new TracerVariable(name);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("name"));
        }
    }
}