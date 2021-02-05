using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM;
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
        public void WriteAndCheckEmptyDocumentIsNotValid()
        {
            var model = new HydroModel();
            var configWriter = new DHydroConfigWriter();
            bool exportFailed = true;
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
            hydroModel.Activities.Add(new RealTimeControlModel());
            hydroModel.CurrentWorkflow = hydroModel.Workflows.First();
            var xmlDocument = new DHydroConfigWriter().CreateConfigDocument(hydroModel);
            var stringWriter = new StringWriter(new StringBuilder());
            xmlDocument.Save(stringWriter);
            var resultString = stringWriter.ToString();
            Assert.IsNotNull(resultString);
            ValidateXml(xmlDocument);
        }

        [Test]
        public void WriteDocumentWithCouplings()
        {
            var hydroModel = BuildCoupledDemoModel();
            var xmlDocument = new DHydroConfigWriter().CreateConfigDocument(hydroModel);
            var stringWriter = new StringWriter(new StringBuilder());
            xmlDocument.Save(stringWriter);
            var resultString = stringWriter.ToString();
            Assert.IsNotNull(resultString);
            ValidateXml(xmlDocument);
        }

        [Test]
        public void WriteDocument_RTC_FM_HasLoggerElement()
        {
            DimrConfigModelCouplerFactory.CouplerProviders.Add(new RealTimeControlDimrConfigModelCouplerProvider());

            var hydroModel = BuildCoupledDemoModel();
            CheckCouplerXml(hydroModel);
        }

        #region PrivateHelperMethods
        private static HydroModel BuildCoupledDemoModel()
        {
            var hydroModel = new HydroModel();
            var waterFlowFMModel = new WaterFlowFMModel();
            var realTimeControlModel = new RealTimeControlModel();

            hydroModel.Activities.Add(waterFlowFMModel);
            hydroModel.Activities.Add(realTimeControlModel);

            var pump = new Pump2D("pomp")
            {
                Geometry = new LineString(new[] { new Coordinate(-50, -100), new Coordinate(50, -100) })
            };
            var gate = new Gate2D("poort")
            {
                Geometry = new LineString(new[] { new Coordinate(-50, 100), new Coordinate(50, 100) })
            };
            var obserVationPoint = new GroupableFeature2DPoint
            {
                Name = "station",
                Geometry = new Point(0, 0)
            };

            waterFlowFMModel.Area.Pumps.Add(pump);
            waterFlowFMModel.Area.Gates.Add(gate);
            waterFlowFMModel.Area.ObservationPoints.Add(obserVationPoint);

            realTimeControlModel.ControlGroups.Add(RealTimeControlModelHelper.CreateStandardControlGroup("InvertorRule"));

            var waterDepth = waterFlowFMModel.GetChildDataItems(obserVationPoint).ElementAt(1);
            var pumpCapacity = waterFlowFMModel.GetChildDataItems(pump).First();

            var controlGroupDataItem = realTimeControlModel.DataItems.First(di => di.Value is ControlGroup);
            var rtcInput = controlGroupDataItem.Children.First(di => di.Role == DataItemRole.Input);
            var rtcOutput = controlGroupDataItem.Children.First(di => di.Role == DataItemRole.Output);

            rtcInput.LinkTo(waterDepth);
            pumpCapacity.LinkTo(rtcOutput);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.First();

            return hydroModel;
        }

        private static void CheckCouplerXml(HydroModel hydroModel)
        {
            var xml = new DHydroConfigWriter().CreateConfigDocument(hydroModel);
            var couplers = xml.Descendants().Where(p => p.Name.LocalName == "coupler" && p.HasElements).ToList();

            Assert.IsTrue(couplers.Any());
            foreach (var coupler in couplers)
            {
                var couplerName = coupler.Attributes().FirstOrDefault(attr => attr.Name.LocalName == "name");
                Assert.IsNotNull(couplerName);
                Assert.IsNotNull(couplerName.Value);

                var logger = coupler.Descendants().SingleOrDefault(c => c.Name.LocalName == "logger");
                Assert.IsNotNull(logger);
                Assert.IsTrue(logger.HasElements);

                var outputFileElement = logger.Descendants().SingleOrDefault(l => l.Name.LocalName == "outputFile");
                Assert.IsNotNull(outputFileElement);

                var couplerNameWithExtension = string.Concat(couplerName.Value, ".nc");
                Assert.AreEqual(couplerNameWithExtension, outputFileElement.Value);
            }
            ValidateXml(xml);
        }

        private static void ValidateXml(XDocument xmlDocument, bool expectedToFail = false, Action<string> assertFailMessage = null)
        {
            if (assertFailMessage == null)
                assertFailMessage = (s) => Assert.Fail("Couldn't validate dimr xml!!" + Environment.NewLine + s);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

            settings.ValidationEventHandler +=
                new ValidationEventHandler(
                    (s, e) =>
                    {
                        if (!expectedToFail) assertFailMessage(e.Message);
                    });
            try
            {
                FileUtils.DeleteIfExists("mytest.xml");
                xmlDocument.Save("mytest.xml");
                // Create the XmlReader object.
                using (var xmlReader = XmlReader.Create("mytest.xml", settings))
                {
                    // Parse the file. 
                    while (xmlReader.Read())
                    {
                    }
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