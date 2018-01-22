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
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.Wave;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
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

        private static HydroModel CreateSimpleCoupledModelWithOneCatchment(CatchmentType catchmentType, double bedlevel = 1.3)
        {
            // create full hydro model
            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.All);

            // remove non Flow/RR activities
            foreach (var activity in hydroModel.Activities.ToList())
            {
                if (!(activity is WaterFlowModel1D) && !(activity is RainfallRunoffModel))
                {
                    hydroModel.Activities.Remove(activity);
                }
            }

            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

            // add catchment to rr
            var c1 = new Catchment
            {
                Name = "C1",
                CatchmentType = catchmentType,
                Geometry = new Point(100, 500),
                IsGeometryDerivedFromAreaSize = true
            };
            c1.SetAreaSize(1000.0);
            rr.Basin.Catchments.Add(c1);

            // add channel to flow
            var n1 = new HydroNode("N1") { Geometry = new Point(0, 0) };
            var n2 = new HydroNode("N2") { Geometry = new Point(200, 0) };
            flow.Network.Nodes.Add(n1);
            flow.Network.Nodes.Add(n2);
            var channel = new Channel(n1, n2)
            {
                Name = "B1",
                Geometry = new LineString(new[] { n1.Geometry.Coordinate, n2.Geometry.Coordinate })
            };
            flow.Network.Branches.Add(channel);

            // add simple cross section to channel
            CrossSectionHelper.AddCrossSection(channel, 50, bedlevel);

            // add lateral to flow
            var l1 = new LateralSource { Name = "L1", Geometry = new Point(100, 0) };
            channel.BranchFeatures.Add(l1);

            // link catchment c1 to lateral l1
            c1.LinkTo(l1);

            return hydroModel;
        }

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

        [Test]
        public void WriteDocument_RTC_1D_HasLoggerElement()
        {
            var hydroModel = BuildCoupledDemo1DModel();
            CheckCouplerXml(hydroModel);
        }

        [Test]
        public void WriteDocument_RTC_FM_HasLoggerElement()
        {
            var hydroModel = BuildCoupledDemoModel();
            CheckCouplerXml(hydroModel);
        }

        [Test]
        public void WriteDocument_RR_1D_HasLoggerElement()
        {
            var hydroModel = CreateSimpleCoupledModelWithOneCatchment(CatchmentType.Paved);
            Assert.IsNotNull(hydroModel);

            hydroModel.CurrentWorkflow =
                hydroModel.Workflows.FirstOrDefault(w => w is ParallelActivity && w.Activities.Count == 2);

            CheckCouplerXml(hydroModel);
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