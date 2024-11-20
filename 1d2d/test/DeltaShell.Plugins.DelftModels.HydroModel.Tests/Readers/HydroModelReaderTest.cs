using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Shell.Core.Services;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Readers
{
    [TestFixture]
    public class HydroModelReaderTest
    {
        private IFileImportService fileImportService;

        [SetUp]
        public void SetUp()
        {
            fileImportService = Substitute.For<IFileImportService>();
        }

        [Test]
        public void ConstructEmptyHydroModel()
        {
            string dimrPath = TestHelper.GetTestFilePath(Path.Combine("FileReader", "dimr.xml"));

            HydroModelReader reader = CreateReader();

            HydroModel hydroModel = reader.Read(dimrPath);

            Assert.NotNull(hydroModel);
            Assert.That(hydroModel.Activities.Count, Is.EqualTo(0));
        }

        [Test]
        [TestCaseSource(nameof(DimrXmlData))]
        public void GivenIncorrectDimrXml_WhenHydroModelReaderRead_ThenReturnExpectedLoggingMessage(string fileContent, string validationLoggingMessage)
        {
            HydroModelReader reader = CreateReader();

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CreateFile("dimr.xml", fileContent);

                void Call() => reader.Read(filePath);

                IEnumerable<string> errorMessages = TestHelper.GetAllRenderedMessages(Call, Level.Error);
                string firstErrorMessage = errorMessages.First();
                Assert.That(firstErrorMessage.Contains(validationLoggingMessage), Is.True);
            }
        }

        [Test]
        [TestCaseSource(nameof(DimrXmlData))]
        public void GivenIncorrectDimrXml_WhenHydroModelReaderRead_ThenReturnNull(string fileContent, string validationLoggingMessage)
        {
            HydroModelReader reader = CreateReader();

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CreateFile("dimr.xml", fileContent);
                HydroModel Call() => reader.Read(filePath);

                Assert.That(Call, Is.Null);
            }
        }

        private HydroModelReader CreateReader()
        {
            return new HydroModelReader(fileImportService);
        }

        private static IEnumerable<TestCaseData> DimrXmlData()
        {
            var fileContent = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
                                <dimrConfig xmlns=""http://schemas.deltares.nl/dimr"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://schemas.deltares.nl/dimr http://content.oss.deltares.nl/schemas/dimr-1.2.xsd"">
                                  <documentation>
                                    <fileVersion>1.2</fileVersion>
                                    <createdBy>Deltares, Coupling Team</createdBy>
                                    <creationDate>2018-09-06T12:56:45.1506771Z</creationDate>
                                  </documentation>
                                  <control>
                                    <parallel>
                                      <startGroup>
                                        <time>0 600 2592000</time>
                                        <coupler name=""flow1d_to_rtc"" />
                                        <start name=""real-time control"" />
                                        <coupler name=""rtc_to_flow1d"" />
                                      </startGroup>
                                      <start name=""rijn-flow-model"" />
                                    </parallel>
                                  </control>
                                  <component name=""real-time control"">
                                    <library>FBCTools_BMI</library>
                                    <workingDir>rtc</workingDir>
                                    <inputFile>.</inputFile>
                                  </component>
                                  <component name=""rijn-flow-model"">
                                    <library>cf_dll</library>
                                    <workingDir>dflow1d</workingDir>
                                    <inputFile>rijn-flow-model.md1d</inputFile>
                                  </component>
                                  <coupler name=""rtc_to_flow1d"">
                                    <sourceComponent>real-time control</sourceComponent>
                                    <targetComponent>rijn-flow-model</targetComponent>
                                    <item>
                                      <sourceName>output_ST_Driel_zom_Crest level (s)</sourceName>
                                      <targetName>weirs/ST_Driel_zom/structure_crest_level</targetName>
                                    </item>
                                  </coupler>
                                  <coupler name=""flow1d_to_rtc"">
                                    <sourceComponent>rijn-flow-model</sourceComponent>
                                    <targetComponent>real-time control</targetComponent>
                                    <item>
                                      <sourceName>weirs/ST_Driel_zom/structure_crest_level</sourceName>
                                      <targetName>input_ST_Driel_zom_Crest level (s)</targetName>
                                    </item>
                                  </coupler>
                                </dimrConfig>";

            XDocument xDocument = XDocument.Parse(fileContent);
            RemoveElement(xDocument, "coupler");
            var logMessage = "It is missing the following element(s): Coupler";
            yield return new TestCaseData(xDocument.ToString(), logMessage).SetName("Coupler missing");

            xDocument = XDocument.Parse(fileContent);
            RemoveElement(xDocument, "control");
            logMessage = "It is missing the following element(s): Control";
            yield return new TestCaseData(xDocument.ToString(), logMessage).SetName("Control missing");

            xDocument = XDocument.Parse(fileContent);
            ClearElement(xDocument, "control");
            logMessage = "It is missing the following element(s): Control";
            yield return new TestCaseData(xDocument.ToString(), logMessage).SetName("Control empty");

            xDocument = XDocument.Parse(fileContent);
            RemoveElement(xDocument, "documentation");
            logMessage = "It is missing the following element(s): Documentation";
            yield return new TestCaseData(xDocument.ToString(), logMessage).SetName("Documentation missing");

            xDocument = XDocument.Parse(fileContent);
            RemoveElement(xDocument, "component");
            logMessage = "It is missing the following element(s): Component";
            yield return new TestCaseData(xDocument.ToString(), logMessage).SetName("Component missing");
        }

        private static void RemoveElement(XContainer xDocument, string item)
        {
            XNamespace ns = "http://schemas.deltares.nl/dimr";
            foreach (XElement element in xDocument.Descendants(ns + item).ToList())
            {
                element.Remove();
            }
        }

        private static void ClearElement(XContainer xDocument, string item)
        {
            XNamespace ns = "http://schemas.deltares.nl/dimr";
            foreach (XElement element in xDocument.Descendants(ns + item).ToList())
            {
                element.RemoveNodes();
            }
        }
    }
}