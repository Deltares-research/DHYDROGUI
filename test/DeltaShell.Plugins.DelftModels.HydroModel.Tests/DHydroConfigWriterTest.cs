using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.Wave;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class DHydroConfigWriterTest
    {
        private static HydroModel BuildCoupledDemoModel()
        {
            var hydroModel = new HydroModel();
            var waterFlowFMModel = new WaterFlowFMModel();
            var realTimeControlModel = new RealTimeControlModel();

            hydroModel.Activities.Add(waterFlowFMModel);
            hydroModel.Activities.Add(new WaveModel());
            hydroModel.Activities.Add(realTimeControlModel);

            var pump = new Pump2D("pomp")
            {
                Geometry = new LineString(new[] {new Coordinate(-50, -100), new Coordinate(50, -100)})
            };
            var gate = new Gate2D("poort")
            {
                Geometry = new LineString(new[] {new Coordinate(-50, 100), new Coordinate(50, 100)})
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

        private static HydroModel BuildCoupledDemo1DModel()
        {
            var hydroModel = new HydroModel();
            var waterFlow1DModel = new WaterFlowModel1D
            {
                ExplicitWorkingDirectory = Path.GetTempPath(),
                Network = HydroNetworkHelper.GetSnakeHydroNetwork(2)
            };
            var realTimeControlModel = new RealTimeControlModel();
            var pump = new Pump("pomp")
            {
                Geometry = new Point(new Coordinate(-50, -100))
            };

            waterFlow1DModel.Network.Branches.First().BranchFeatures.Add(pump);

            var observationPoint = new ObservationPoint()
            {
                Name = "obsPoint",
                Geometry = new Point(0, 0)
            };
            waterFlow1DModel.Network.Branches.Last().BranchFeatures.Add(observationPoint);

            realTimeControlModel.ControlGroups.Add(RealTimeControlModelHelper.CreateStandardControlGroup("InvertorRule"));
            hydroModel.Activities.Add(waterFlow1DModel);
            hydroModel.Activities.Add(realTimeControlModel);

            var waterDepth = waterFlow1DModel.GetChildDataItems(observationPoint).ElementAt(1);
            var pumpSetpoint = waterFlow1DModel.GetChildDataItems(pump).Last();

            var controlGroupDataItem = realTimeControlModel.DataItems.First(di => di.Value is ControlGroup);
            var rtcInput = controlGroupDataItem.Children.First(di => di.Role == DataItemRole.Input);
            var rtcOutput = controlGroupDataItem.Children.First(di => di.Role == DataItemRole.Output);

            rtcInput.LinkTo(waterDepth);
            pumpSetpoint.LinkTo(rtcOutput);


            hydroModel.CurrentWorkflow = hydroModel.Workflows.First();

            return hydroModel;
        }

        /*  **** DIMR **** 15/8/2016
         * XSD for Dimr does not consider an empty model. Thus for any model is expecting
         * the nodes control (with its children) and component (with its attributes / children).
         * In our opinion this should not be the case, however, if this changes in the future
         * we will be able to detect it, (discuss it) and fix it.
         */

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
            hydroModel.Activities.Add(new WaveModel());
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
        public void WriteDocument1DWithCouplings()
        {
            var hydroModel = BuildCoupledDemo1DModel();
            var xmlDocument = new DHydroConfigWriter().CreateConfigDocument(hydroModel);
            var stringWriter = new StringWriter(new StringBuilder());
            xmlDocument.Save(stringWriter);
            var resultString = stringWriter.ToString();
            Assert.IsNotNull(resultString);
            ValidateXml(xmlDocument);
        }
 
        private static void ValidateXml(XDocument xmlDocument, bool expectedToFail = false, Action<string> assertFailMessage = null)
        {
            if(assertFailMessage == null)
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
                        if(!expectedToFail) assertFailMessage(e.Message);
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
       
    }

}