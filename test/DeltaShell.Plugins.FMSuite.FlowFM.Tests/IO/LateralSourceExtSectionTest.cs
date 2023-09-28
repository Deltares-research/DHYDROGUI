using System;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    public class LateralSourceExtSectionTest
    {
        [Test]
        public void Constructor_IniSectionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new LateralSourceExtSection(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("iniSection"));
        }

        [Test]
        public void Constructor_NotLateralIniSection_ThrowsArgumentException()
        {
            // Call
            void Call() => new LateralSourceExtSection(new IniSection("some_name"));

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("iniSection"));
        }

        [Test]
        [TestCase("FM_model_lateral_sources.bc", double.NaN, "FM_model_lateral_sources.bc")]
        [TestCase("4.56", 4.56, null)]
        public void Constructor_InitializesInstanceCorrectly(string discharge, double expDischarge, string expDischargeFile)
        {
            // Setup
            var iniSection = new IniSection("Lateral");
            iniSection.AddProperty(CreateProperty("id", "some_id"));
            iniSection.AddProperty(CreateProperty("name", "some_name"));
            iniSection.AddProperty(CreateProperty("nodeId", "some_node_id"));
            iniSection.AddProperty(CreateProperty("branchId", "some_branch_id"));
            iniSection.AddProperty(CreateProperty("chainage", "1.23"));
            iniSection.AddProperty(CreateProperty("discharge", discharge));

            // Call
            var section = new LateralSourceExtSection(iniSection);

            // Assert
            Assert.That(section.Id, Is.EqualTo("some_id"));
            Assert.That(section.Name, Is.EqualTo("some_name"));
            Assert.That(section.NodeName, Is.EqualTo("some_node_id"));
            Assert.That(section.BranchName, Is.EqualTo("some_branch_id"));
            Assert.That(section.Chainage, Is.EqualTo(1.23));
            Assert.That(section.Discharge, Is.EqualTo(expDischarge));
            Assert.That(section.DischargeFile, Is.EqualTo(expDischargeFile));
        }

        [Test]
        public void Constructor_CannotParseDischarge_ReportsError()
        {
            // Setup
            var iniSection = new IniSection("Lateral") {LineNumber = 7};
            iniSection.AddProperty(CreateProperty("id", "some_id"));
            iniSection.AddProperty(CreateProperty("name", "some_name"));
            iniSection.AddProperty(CreateProperty("nodeId", "some_node_id"));
            iniSection.AddProperty(CreateProperty("branchId", "some_branch_id"));
            iniSection.AddProperty(CreateProperty("chainage", "1.23"));
            iniSection.AddProperty(CreateProperty("discharge", "a lot"));

            var logHandler = Substitute.For<ILogHandler>();

            // Call
            var section = new LateralSourceExtSection(iniSection, logHandler);

            // Assert
            logHandler.Received(1).ReportError("Cannot parse 'a lot' to a double, see section on line 7.");
            Assert.That(section.Id, Is.EqualTo("some_id"));
            Assert.That(section.Name, Is.EqualTo("some_name"));
            Assert.That(section.NodeName, Is.EqualTo("some_node_id"));
            Assert.That(section.BranchName, Is.EqualTo("some_branch_id"));
            Assert.That(section.Chainage, Is.EqualTo(1.23));
            Assert.That(section.Discharge, Is.EqualTo(double.NaN));
            Assert.That(section.DischargeFile, Is.EqualTo(null));
        }

        private static IniProperty CreateProperty(string name, string value)
        {
            return new IniProperty
            (
                name,
                value,
                string.Empty
            );
        }
    }
}