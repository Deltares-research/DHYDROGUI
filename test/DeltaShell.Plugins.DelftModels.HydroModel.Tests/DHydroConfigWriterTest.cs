using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Dimr.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
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

        [TestCase(false)]
        [TestCase(true)]
        public void WriteDocument_RR_FM_WaterLevelBoundary(bool useLocalBoundaryData)
        {
            var hydroModel = new HydroModel();
            var waterFlowFMModel = new WaterFlowFMModel();
            var rainfallRunoffModel = new RainfallRunoffModel();
                        
            //set up workflow
            hydroModel.Activities.Add(waterFlowFMModel);
            hydroModel.Activities.Add(rainfallRunoffModel);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.First(wf => wf.Name == @"(RR + FlowFM)");

            //set up 1d network and lateral
            var node1 = new HydroNode(){ Geometry = new Point(0,0) };
            var node2 = new HydroNode(){ Geometry = new Point(100,0) };
            var lateralSource = new LateralSource() { Geometry = new Point(50,0), Chainage = 50};
            var branch1 = new Channel
            {
                Source = node1,
                Target = node2,
            };
            waterFlowFMModel.Network.Branches.Add(branch1);
            branch1.BranchFeatures.Add(lateralSource);
            
            //set up unpaved catchment
            var catchment = new Catchment()
            {
                CatchmentType = CatchmentType.Unpaved,
                Geometry = new LinearRing(new[]{new Coordinate(40,40),new Coordinate(40,60),new Coordinate(60,60),new Coordinate(60,40),new Coordinate(40,40)})
            };
            rainfallRunoffModel.Basin.Catchments.Add(catchment);
            var unpavedData = (UnpavedData)rainfallRunoffModel.ModelData.First();
            unpavedData.UseLocalBoundaryData = useLocalBoundaryData;
            
            //add link to region and set one way direction
            hydroModel.Region.AddNewLink(catchment,lateralSource);
            
            //write file
            DimrConfigModelCouplerFactory.CouplerProviders.Add(new RRDimrConfigModelCouplerProvider());
            DimrConfigModelCouplerFactory.CouplerProviders.Add(new RRDimrConfigModelCouplerProvider());
            var writer = new DHydroConfigWriter();
            var xmlDocument = writer.CreateConfigDocument(hydroModel.CurrentWorkflow);
            
            List<XElement> couplers = xmlDocument.Descendants().Where(p => p.Name.LocalName == "coupler" && p.HasElements).ToList();

            if (useLocalBoundaryData)
            {
                Assert.AreEqual(1,couplers.Count);
                Assert.AreEqual("rr_to_flow",couplers.First().FirstAttribute.Value);
            }
            else
            {
                Assert.AreEqual(2,couplers.Count);
                Assert.AreEqual("rr_to_flow",couplers.First().FirstAttribute.Value);   
                Assert.AreEqual("flow_to_rr",couplers.Last().FirstAttribute.Value);   
            }
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
                Geometry = new LineString(new[]
                {
                    new Coordinate(-50, -100),
                    new Coordinate(50, -100)
                })
            };
            var gate = new Weir2D("poort")
            {
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

        /// <summary>
        /// Validate the <paramref name="xmlDocument"/> with its schema
        /// </summary>
        /// <param name="xmlDocument">Document to check</param>
        private static void ValidateXml(XDocument xmlDocument)
        {
            if (!CheckIfSchemasCanBeReached(xmlDocument, out var errorMessages))
            {
                Assert.Fail($"Could not reach all namespaces{Environment.NewLine}{string.Join(Environment.NewLine, errorMessages)}");
            }
            
            var temporaryFilePath = "mytest.xml";

            try
            {
                FileUtils.DeleteIfExists(temporaryFilePath);
                xmlDocument.Save(temporaryFilePath);

                // setup settings for schema checking
                var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints
                                      | XmlSchemaValidationFlags.AllowXmlAttributes
                                      | XmlSchemaValidationFlags.ProcessInlineSchema
                                      | XmlSchemaValidationFlags.ProcessSchemaLocation
                                      | XmlSchemaValidationFlags.ReportValidationWarnings
                };

                settings.ValidationEventHandler += (s, e) =>
                {
                    Assert.Fail("Xsd validation failed: " + Environment.NewLine + e.Message);
                };

                // Create the XmlReader object.
                using (var xmlReader = XmlReader.Create(temporaryFilePath, settings))
                {
                    // Parse the file. 
                    while (xmlReader.Read()) {}
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(temporaryFilePath);
            }
        }

        /// <summary>
        /// Searches for namespace location attributes (schemaLocation) in <paramref name="xmlDocument"/> and checks
        /// if the referenced location can be reached
        /// </summary>
        /// <param name="xmlDocument">Document to search</param>
        /// <param name="errorMessages">If result is false, the error messages will be added to this parameter</param>
        /// <returns>true if all schemas can be reached</returns>
        private static bool CheckIfSchemasCanBeReached(XDocument xmlDocument, out ICollection<string> errorMessages)
        {
            errorMessages = new List<string>();
            var schemaLocationAttributes = xmlDocument.Root?.Attributes()
                                                     .Where(a => a.Name.LocalName == "schemaLocation")
                                                     ?? Enumerable.Empty<XAttribute>();

            foreach (var schemaLocationAttribute in schemaLocationAttributes)
            {
                var locationParts = schemaLocationAttribute.Value.Split();
                if (locationParts.Length != 2)
                {
                    continue;
                }

                var namespaceName = locationParts[0];
                var location = locationParts[1];

                if (CanReachLocation(location))
                {
                    continue;
                }

                errorMessages.Add($"Could not reach location for namespace {namespaceName} at {location}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if <paramref name="location"/> can be reached
        /// </summary>
        /// <param name="location">Web address to check</param>
        /// <returns>True if the response status code is <see cref="HttpStatusCode.OK"/></returns>
        private static bool CanReachLocation(string location)
        {
            var request = WebRequest.Create(location);
            request.Timeout = 5000;
            request.Method = "HEAD";

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        #endregion
    }
}