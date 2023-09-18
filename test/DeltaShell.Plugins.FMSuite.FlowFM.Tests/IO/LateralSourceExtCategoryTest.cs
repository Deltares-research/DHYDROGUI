using System;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    public class LateralSourceExtCategoryTest
    {
        [Test]
        public void Constructor_IniSectionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new LateralSourceExtCategory(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("iniSection"));
        }

        [Test]
        public void Constructor_NotLateralIniSection_ThrowsArgumentException()
        {
            // Call
            void Call() => new LateralSourceExtCategory(new IniSection("some_name"));

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
            var category = new LateralSourceExtCategory(iniSection);

            // Assert
            Assert.That(category.Id, Is.EqualTo("some_id"));
            Assert.That(category.Name, Is.EqualTo("some_name"));
            Assert.That(category.NodeName, Is.EqualTo("some_node_id"));
            Assert.That(category.BranchName, Is.EqualTo("some_branch_id"));
            Assert.That(category.Chainage, Is.EqualTo(1.23));
            Assert.That(category.Discharge, Is.EqualTo(expDischarge));
            Assert.That(category.DischargeFile, Is.EqualTo(expDischargeFile));
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
            var category = new LateralSourceExtCategory(iniSection, logHandler);

            // Assert
            logHandler.Received(1).ReportError("Cannot parse 'a lot' to a double, see category on line 7.");
            Assert.That(category.Id, Is.EqualTo("some_id"));
            Assert.That(category.Name, Is.EqualTo("some_name"));
            Assert.That(category.NodeName, Is.EqualTo("some_node_id"));
            Assert.That(category.BranchName, Is.EqualTo("some_branch_id"));
            Assert.That(category.Chainage, Is.EqualTo(1.23));
            Assert.That(category.Discharge, Is.EqualTo(double.NaN));
            Assert.That(category.DischargeFile, Is.EqualTo(null));
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