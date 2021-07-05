using System;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    public class LateralSourceExtCategoryTest
    {
        [Test]
        public void Constructor_CategoryNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new LateralSourceExtCategory(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("category"));
        }

        [Test]
        public void Constructor_NotLateralCategory_ThrowsArgumentException()
        {
            // Call
            void Call() => new LateralSourceExtCategory(new DelftIniCategory("some_name"));

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("category"));
        }

        [Test]
        [TestCase("FM_model_lateral_sources.bc", double.NaN, "FM_model_lateral_sources.bc")]
        [TestCase("4.56", 4.56, null)]
        public void Constructor_InitializesInstanceCorrectly(string discharge, double expDischarge, string expDischargeFile)
        {
            // Setup
            var delftIniCategory = new DelftIniCategory("Lateral");
            delftIniCategory.Properties.Add(CreateProperty("id", "some_id"));
            delftIniCategory.Properties.Add(CreateProperty("name", "some_name"));
            delftIniCategory.Properties.Add(CreateProperty("nodeId", "some_node_id"));
            delftIniCategory.Properties.Add(CreateProperty("branchId", "some_branch_id"));
            delftIniCategory.Properties.Add(CreateProperty("chainage", "1.23"));
            delftIniCategory.Properties.Add(CreateProperty("discharge", discharge));

            // Call
            var category = new LateralSourceExtCategory(delftIniCategory);

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
            var delftIniCategory = new DelftIniCategory("Lateral") {LineNumber = 7};
            delftIniCategory.Properties.Add(CreateProperty("id", "some_id"));
            delftIniCategory.Properties.Add(CreateProperty("name", "some_name"));
            delftIniCategory.Properties.Add(CreateProperty("nodeId", "some_node_id"));
            delftIniCategory.Properties.Add(CreateProperty("branchId", "some_branch_id"));
            delftIniCategory.Properties.Add(CreateProperty("chainage", "1.23"));
            delftIniCategory.Properties.Add(CreateProperty("discharge", "a lot"));

            var logHandler = Substitute.For<ILogHandler>();

            // Call
            var category = new LateralSourceExtCategory(delftIniCategory, logHandler);

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

        private static DelftIniProperty CreateProperty(string name, string value)
        {
            return new DelftIniProperty
            {
                Name = name,
                Value = value,
                Comment = string.Empty
            };
        }
    }
}