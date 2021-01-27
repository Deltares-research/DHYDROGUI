using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructuresObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.Wave;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class DHydroConfigWriterTest
    {
        [Test]
        [Category(TestCategory.Slow)]
        public void WriteAndCheckEmptyDocumentIsNotValid()
        {
            var model = new HydroModel();
            var configWriter = new DHydroConfigWriter();
            var exportFailed = true;
            try
            {
                configWriter.CreateConfigDocument(model);
                exportFailed = false;
            }
            catch
            {
                Assert.That(exportFailed, Is.True);
            }
        }

        [Test]
        public void WriteDocumentWithComponents()
        {
            var hydroModel = new HydroModel();
            hydroModel.Activities.Add(new WaterFlowFMModel());
            hydroModel.Activities.Add(new WaveModel());
            hydroModel.Activities.Add(new RealTimeControlModel());
            hydroModel.CurrentWorkflow = hydroModel.Workflows.First();
            XDocument xmlDocument = new DHydroConfigWriter().CreateConfigDocument(hydroModel);
            var stringWriter = new StringWriter(new StringBuilder());
            xmlDocument.Save(stringWriter);
            var resultString = stringWriter.ToString();
            Assert.IsNotNull(resultString);
            ValidateXml(xmlDocument);
        }

        [Test]
        public void WriteDocumentWithCouplings()
        {
            HydroModel hydroModel = BuildCoupledDemoModel();
            XDocument xmlDocument = new DHydroConfigWriter().CreateConfigDocument(hydroModel);
            var stringWriter = new StringWriter(new StringBuilder());
            xmlDocument.Save(stringWriter);
            var resultString = stringWriter.ToString();
            Assert.IsNotNull(resultString);
            ValidateXml(xmlDocument);
        }

        [Test]
        public void WriteDocument_RTC_FM_HasLoggerElement()
        {
            HydroModel hydroModel = BuildCoupledDemoModel();
            CheckCouplerXml(hydroModel);
        }

        #region PrivateHelperMethods

        private static HydroModel BuildCoupledDemoModel()
        {
            var hydroModel = new HydroModel();
            var waterFlowFMModel = new WaterFlowFMModel();
            var realTimeControlModel = new RealTimeControlModel();

            hydroModel.Activities.Add(waterFlowFMModel);
            hydroModel.Activities.Add(new WaveModel());
            hydroModel.Activities.Add(realTimeControlModel);

            var pump = new Pump()
            {
                Name = "pomp",
                Geometry = new LineString(new[]
                {
                    new Coordinate(-50, -100),
                    new Coordinate(50, -100)
                })
            };
            var gate = new Structure()
            {
                Name = "poort",
                Geometry = new LineString(new[]
                {
                    new Coordinate(-50, 100),
                    new Coordinate(50, 100)
                })
            };
            var obserVationPoint = new GroupableFeature2DPoint
            {
                Name = "station",
                Geometry = new Point(0, 0)
            };

            waterFlowFMModel.Area.Pumps.Add(pump);
            waterFlowFMModel.Area.Weirs.Add(gate);
            waterFlowFMModel.Area.ObservationPoints.Add(obserVationPoint);

            realTimeControlModel.ControlGroups.Add(RealTimeControlModelHelper.CreateStandardControlGroup("InvertorRule"));

            IDataItem waterDepth = waterFlowFMModel.GetChildDataItems(obserVationPoint).ElementAt(1);
            IDataItem pumpCapacity = waterFlowFMModel.GetChildDataItems(pump).First();

            IDataItem controlGroupDataItem = realTimeControlModel.DataItems.First(di => di.Value is ControlGroup);
            IDataItem rtcInput = controlGroupDataItem.Children.First(di => di.Role == DataItemRole.Input);
            IDataItem rtcOutput = controlGroupDataItem.Children.First(di => di.Role == DataItemRole.Output);

            rtcInput.LinkTo(waterDepth);
            pumpCapacity.LinkTo(rtcOutput);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.First();

            return hydroModel;
        }

        /*  **** DIMR **** 15/8/2016
         * XSD for Dimr does not consider an empty model. Thus for any model is expecting
         * the nodes control (with its children) and component (with its attributes / children).
         * In our opinion this should not be the case, however, if this changes in the future
         * we will be able to detect it, (discuss it) and fix it.
         */

        private static void CheckCouplerXml(HydroModel hydroModel)
        {
            XDocument xml = new DHydroConfigWriter().CreateConfigDocument(hydroModel);
            List<XElement> couplers = xml.Descendants().Where(p => p.Name.LocalName == "coupler" && p.HasElements).ToList();

            Assert.IsTrue(couplers.Any());
            foreach (XElement coupler in couplers)
            {
                XAttribute couplerName = coupler.Attributes().FirstOrDefault(attr => attr.Name.LocalName == "name");
                Assert.IsNotNull(couplerName);
                Assert.IsNotNull(couplerName.Value);

                XElement logger = coupler.Descendants().SingleOrDefault(c => c.Name.LocalName == "logger");
                Assert.IsNotNull(logger);
                Assert.IsTrue(logger.HasElements);

                XElement outputFileElement = logger.Descendants().SingleOrDefault(l => l.Name.LocalName == "outputFile");
                Assert.IsNotNull(outputFileElement);

                string couplerNameWithExtension = string.Concat(couplerName.Value, ".nc");
                Assert.AreEqual(couplerNameWithExtension, outputFileElement.Value);
            }

            ValidateXml(xml);
        }

        private static void ValidateXml(XDocument xmlDocument, bool expectedToFail = false, Action<string> assertFailMessage = null)
        {
            if (assertFailMessage == null)
            {
                assertFailMessage = (s) => Assert.Fail("Couldn't validate dimr xml!!" + Environment.NewLine + s);
            }

            var settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

            settings.ValidationEventHandler +=
                new ValidationEventHandler(
                    (s, e) =>
                    {
                        if (!expectedToFail)
                        {
                            assertFailMessage(e.Message);
                        }
                    });
            try
            {
                FileUtils.DeleteIfExists("mytest.xml");
                xmlDocument.Save("mytest.xml");
                // Create the XmlReader object.
                using (var xmlReader = XmlReader.Create("mytest.xml", settings))
                {
                    // Parse the file. 
                    while (xmlReader.Read()) {}
                }
            }
            finally
            {
                FileUtils.DeleteIfExists("mytest.xml");
            }
        }

        #endregion
    }
}