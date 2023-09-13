using System;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.DataAccessBuilders
{
    [TestFixture]
    public class IniSectionFactoryTest
    {
        [Test]
        public void CreateBoundaryBlock_CreatesCorrectSection()
        {
            const string quantity = "some_quantity";
            const string locationFilePath = "some_location_file_path";
            const string forcingFilePath = "some_forcing_file_path";
            var thatcherHarlemanTimeLag = new TimeSpan(1, 2, 3);

            // Call
            IniSection result = IniSectionFactory.CreateBoundaryBlock(quantity, locationFilePath, forcingFilePath, thatcherHarlemanTimeLag);

            // Assert
            Assert.That(result.Name, Is.EqualTo("boundary"));
            Assert.That(result.Properties, Has.Count.EqualTo(4));
            SectionContains(result, "quantity", quantity);
            SectionContains(result, "locationFile", locationFilePath);
            SectionContains(result, "forcingFile", forcingFilePath);
            SectionContains(result, "returnTime", "3.7230000e+003");
        }

        [Test]
        public void CreateBoundaryBlock_InvalidValues_CreatesCorrectSection()
        {
            // Call
            IniSection result = IniSectionFactory.CreateBoundaryBlock(null, null, null, TimeSpan.Zero);

            // Assert
            Assert.That(result.Name, Is.EqualTo("boundary"));
            Assert.That(result.Properties, Has.Count.EqualTo(0));
            SectionDoesNotContain(result, "quantity");
            SectionDoesNotContain(result, "locationFile");
            SectionDoesNotContain(result, "forcingFile");
            SectionDoesNotContain(result, "returnTime");
            SectionDoesNotContain(result, "OpenBoundaryTolerance");
        }

        private static void SectionContains(IniSection section, string propertyKey, object propertyValue)
        {
            IniProperty property = section.GetProperty(propertyKey);
            Assert.That(property, Is.Not.Null,
                        $"Section should contain property <{propertyKey}>.");
            Assert.That(property.Value, Is.EqualTo(propertyValue),
                        $"Section '{propertyKey}' has an incorrect value.");
        }

        private static void SectionDoesNotContain(IniSection section, string propertyKey)
        {
            IniProperty property = section.GetProperty(propertyKey);
            Assert.That(property, Is.Null, $"Section should not contain property <{propertyKey}>.");
        }
    }
}