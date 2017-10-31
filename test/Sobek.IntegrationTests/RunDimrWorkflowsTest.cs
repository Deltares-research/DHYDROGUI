using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.NetworkEditor;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Model1D2DBuilder = DeltaShell.Plugins.DeveloperTools.Commands.IntegratedDemoModels.Model1D2DBuilder;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class RunDimrWorkflowsTest
    {
        [Test]
        public void TestDimrConfigurationExport1D2DWorkflow() // Flow1D + FlowFM
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                app.Run();

                Model1D2DBuilder.Create1d2dModel(app);

                var hydroModel = app.GetAllModelsInProject().OfType<HydroModel>().First();
                var fmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().First();
                var f1DModel = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

                var observationPointFm = new GroupableFeature2DPoint { Name = "ObservationFM" };
                fmModel.Area.ObservationPoints.Add(observationPointFm);

                var firstBranch = f1DModel.Network.Branches[0];
                var firstBranchGeometry = firstBranch.Geometry;

                var observationPointF1D = new ObservationPoint
                    {
                        Name = "ObservationF1D",
                        Geometry = new Point(firstBranchGeometry.Coordinate.X + 10, firstBranchGeometry.Coordinate.Y)
                    };

                var weir = new Weir
                    {
                        Name = "Weir1",
                        Geometry = new Point(firstBranchGeometry.Coordinate.X + 20, firstBranchGeometry.Coordinate.Y)
                    };

                firstBranch.BranchFeatures.Add(observationPointF1D);
                firstBranch.BranchFeatures.Add(weir);

                //(FlowFM + Flow1D)
                var workflow = hydroModel.Workflows.FirstOrDefault(wf => wf.Name == "(FlowFM + Flow1D)");
                Assert.NotNull(workflow);
                hydroModel.CurrentWorkflow = workflow;

                FileUtils.DeleteIfExists(Path.GetFullPath("fmf1d"));

                var dirInfo = Directory.CreateDirectory("fmf1d");
                var exportFilePath = Path.Combine(dirInfo.FullName, "dimrWorkflowFmF1d.xml");
                var exporter = new DHydroConfigXmlExporter
                {
                    ExportFilePath = exportFilePath
                };

                Assert.IsTrue(exporter.Export(hydroModel, null));
                Assert.IsTrue(File.Exists(exportFilePath));

                var directoryPath = Path.GetDirectoryName(exportFilePath) ?? "";
                Assert.IsTrue(Directory.Exists(Path.Combine(directoryPath, f1DModel.DirectoryName)));
                Assert.IsTrue(Directory.Exists(Path.Combine(directoryPath, fmModel.DirectoryName)));
                Assert.IsTrue(Directory.Exists(Path.Combine(directoryPath, new Iterative1D2DCoupler().DirectoryName)));
                
                // Open File
                var exportedDocument = XDocument.Load(exportFilePath);
                ValidateXml(exportFilePath);

                Assert.IsNotNull(exportedDocument.Root);
                var nameSpace = exportedDocument.Root.Name.Namespace;

                // Check if controls are written as we want
                var rootElement = exportedDocument.Root;
                Assert.AreEqual(rootElement.Elements(nameSpace + "control").Count(), 1); //Only one control node    

                //Check control node
                var elementsInOrder = new List<KeyValuePair<string, string>>();
                var controlStartElements = new List<string> { "1d2d" };
                var parallelStartElements = new List<string>();

                XmlDimrCheckControl(rootElement, false, "0 20 3600", elementsInOrder, controlStartElements, parallelStartElements);

                //Check component nodes
                XmlDimrCheckComponents(rootElement, new List<List<string>>
                    {
                        new List<string> {"1d2d", "flow1d2d", "0", "1d2dcoupler", "1d2d.ini"}
                    });
            }
        }

        [Test]
        public void TestDimrConfigurationExport1DrtcWorkflow() // Flow1D + RTC
        {
            using (var app = new DeltaShellApplication {IsProjectCreatedInTemporaryDirectory = true})
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                app.Run();

                Model1D2DBuilder.Create1d2dModel(app);

                var hydroModel = app.GetAllModelsInProject().OfType<HydroModel>().First();
                var f1DModel = hydroModel.Activities.OfType<WaterFlowModel1D>().First();
                var rtcModel = new RealTimeControlModel();

                hydroModel.Activities.Add(rtcModel);

                var firstBranch = f1DModel.Network.Branches[0];

                /* Structures definition */
                var weir1 = new Weir("Weir1") {Geometry = new Point(firstBranch.Geometry.Coordinates[0])};
                var weir2 = new Weir("Weir2")
                {
                    Geometry = new Point(firstBranch.Geometry.Coordinates[1]),
                    CrestLevel = 33.00
                };

                var obsNearPipe = new ObservationPoint
                {
                    Name = "Obs. point",
                    Description = "O3",
                    Geometry = new Point(firstBranch.Length, 0.0)
                };

                /* Adding to the branch */
                firstBranch.BranchFeatures.AddRange(new IBranchFeature[] {weir1, weir2, obsNearPipe});

                /*  Control groups  */
                var pidWaterLevelController = new PIDRule
                {
                    Name = "Weir controller",
                    LongName = "Steers weir 6 such that water level at 'near pipe' does not go over 0",
                    PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant,
                    ConstantValue = 0.0,
                    Kp = 1.0,
                    Ki = 0.0,
                    Kd = 0.0,
                    Setting = {Max = 1.0, MaxSpeed = 1.0, Min = -0.9}
                };

                var hydroCondition = new StandardCondition
                {
                    Name = "Weir controller activator",
                    LongName = "Activates weir controller in case water level at 'near pipe' is (about to go) above 0",
                    Operation = Operation.Greater,
                    Value = -0.05
                };

                var inputRtc = new Input() {ParameterName = "rtcinputObsPoint", Feature = obsNearPipe};
                var outputRtC = new Output() { ParameterName = "rtcoutputWeir1", Feature = weir1 };
                var input = new Input() { ParameterName = "rtcinputWeir1", Feature = weir1 };
                var output = new Output() { ParameterName = "rtcoutputWeir2", Feature = weir2 };

                pidWaterLevelController.Inputs.Add(inputRtc);
                pidWaterLevelController.Outputs.Add(outputRtC);

                hydroCondition.Input = input;
                hydroCondition.TrueOutputs.Add(pidWaterLevelController);
                
                var controlGroup1 = new ControlGroup { Name = "Water level controller" };
                
                controlGroup1.Inputs.AddRange(new [] { inputRtc, input });
                controlGroup1.Outputs.AddRange(new [] { outputRtC, output });
                controlGroup1.Rules.Add(pidWaterLevelController);
                controlGroup1.Conditions.Add(hydroCondition);

                rtcModel.ControlGroups.Add(controlGroup1);

                // link weir2 => ouput
                var weirDataItemAsInput = f1DModel.GetChildDataItems(weir2).First(di => (di.Role & DataItemRole.Input) == DataItemRole.Input);
                var outputWeirDataItem = rtcModel.GetDataItemByValue(outputRtC);

                weirDataItemAsInput.LinkTo(outputWeirDataItem);

                /* Coupert 1dToRtc */
                // link weir => input
                var outputDataItem1 = f1DModel.GetChildDataItems(weir1).First(i => (i.Role & DataItemRole.Output) == DataItemRole.Output);
                rtcModel.GetDataItemByValue(input).LinkTo(outputDataItem1);

                // link output => weir
                var inputDataItem = f1DModel.GetChildDataItems(weir1).First(i => (i.Role & DataItemRole.Input) == DataItemRole.Input);
                inputDataItem.LinkTo(f1DModel.GetDataItemByValue(output));

                //(RTC + Flow1D)
                var workflow = hydroModel.Workflows.FirstOrDefault(wf => wf.Name == "(RTC + Flow1D)");
                Assert.NotNull(workflow);

                hydroModel.CurrentWorkflow = workflow;

                var dirPath = Path.GetFullPath("rtcf1d");
                FileUtils.DeleteIfExists(dirPath);

                var dirInfo = Directory.CreateDirectory("rtcf1d");
                var exportFilePath = Path.Combine(dirInfo.FullName, "dimrWorkflowrtcf1d.xml");

                var exporter = new DHydroConfigXmlExporter
                    {
                        ExportFilePath = exportFilePath
                    };

                Assert.IsTrue(exporter.Export(hydroModel, null));
                Assert.IsTrue(File.Exists(exportFilePath));

                var exportDirectory = Path.GetDirectoryName(exportFilePath) ?? "";
                Assert.IsTrue(Directory.Exists(Path.Combine(exportDirectory, f1DModel.DirectoryName)));
                Assert.IsTrue(Directory.Exists(Path.Combine(exportDirectory, rtcModel.DirectoryName)));

                // Open File
                var exportedDocument = XDocument.Load(exportFilePath);
                ValidateXml(exportFilePath);
                Assert.IsNotNull(exportedDocument.Root);

                var rootElement = exportedDocument.Root;

                //Check control node
                var elementsInOrder = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("coupler", "flow1d_to_rtc"),
                        new KeyValuePair<string, string>("start", "RTC Model"),
                        new KeyValuePair<string, string>("coupler", "rtc_to_flow1d")
                    };

                var controlStartElements = new List<string>();
                var parallelStartElements = new List<string> { "Flow1D" };
                XmlDimrCheckControl(rootElement, true, "0 20 3600", elementsInOrder, controlStartElements, parallelStartElements);
                
                //Check component nodes
                var componentValuesList = new List<List<string>>
                    {
                        new List<string> {"RTC Model", "FBCTools_BMI", "0", "rtc", "."},
                        new List<string> {"Flow1D", "cf_dll", "0", "dflow1d", "Flow1D.md1d"}
                    };

                XmlDimrCheckComponents(rootElement, componentValuesList);
                
                //Check coupler nodes
                var couplersList = new Dictionary<List<string>, Dictionary<string, string>>
                {
                    {
                      new List<string>(new[] { "rtc_to_flow1d", "RTC Model", "Flow1D" }),
                      new Dictionary<string, string> { { "output_Weir2_Crest level (s)", "weirs/Weir2/structure_crest_level" } }
                    },
                    {
                        new List<string> {"flow1d_to_rtc", "Flow1D", "RTC Model"},
                        new Dictionary<string, string> { { "weirs/Weir1/water_discharge", "input_Weir1_Discharge (s)" } }
                    }
                };
                
                XmlDimrCheckCouplers(rootElement, couplersList);
            }
        }

        [Test]
        public void TestDimrConfigurationExport2DrtcWorkflow() // FlowFM + RTC
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                app.Run();

                Model1D2DBuilder.Create1d2dModel(app);

                var hydroModel = app.GetAllModelsInProject().OfType<HydroModel>().First();
                var fmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().First();

                var observationPointFm = new GroupableFeature2DPoint { Name = "ObservationFM" };
                var weirFm = new Weir2D { Name = "Weir1", Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) }) };

                fmModel.Area.ObservationPoints.Add(observationPointFm);
                fmModel.Area.Weirs.Add(weirFm);
                
                /* Structures definition */
                var input = new Input() {ParameterName = "rtcObsInput", Feature = observationPointFm};
                var output = new Output() {ParameterName = "rtcWeir1Output", Feature = weirFm};
                var rule  = new PIDRule { Name = "noot", Inputs = { input }, Outputs = { output } };
                /*  Control groups  */
                var controlGroup = new ControlGroup{Name = "test", Rules = { rule }, };
                controlGroup.Inputs.Add(input);
                controlGroup.Outputs.Add(output);

                var rtcModel = new RealTimeControlModel { ControlGroups = { controlGroup } };
                hydroModel.Activities.Add(rtcModel);

                /* Coupert 2dToRtc */
                var flowObservationFmOutputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(observationPointFm).First();
                var rtcInputdataItemFm = rtcModel.AllDataItems.First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, input));
                
                // link
                rtcInputdataItemFm.LinkTo(flowObservationFmOutputDataItem);

                //(RTC + Flow1D)
                var workflow = hydroModel.Workflows.FirstOrDefault(wf => wf.Name == "(RTC + FlowFM)");
                Assert.NotNull(workflow);
                hydroModel.CurrentWorkflow = workflow;

                var dirPath = Path.GetFullPath("rtcfm");
                FileUtils.DeleteIfExists(dirPath);

                var dirInfo = Directory.CreateDirectory("rtcfm");
                var exportFilePath = Path.Combine(dirInfo.FullName, "dimrWorkflowRtcFm.xml");
                var exporter = new DHydroConfigXmlExporter
                    {
                        ExportFilePath = exportFilePath
                    };

                Assert.IsTrue(exporter.Export(hydroModel, null));
                Assert.IsTrue(File.Exists(exportFilePath));

                var exportDirectory = Path.GetDirectoryName(exportFilePath) ?? "";
                Assert.IsTrue(Directory.Exists(Path.Combine(exportDirectory, fmModel.DirectoryName)));
                Assert.IsTrue(Directory.Exists(Path.Combine(exportDirectory, rtcModel.DirectoryName)));

                // Open File
                var exportedDocument = XDocument.Load(exportFilePath);
                ValidateXml(exportFilePath);

                var rootElement = exportedDocument.Root;
                Assert.IsNotNull(rootElement);

                //Check control node
                var elementsInOrder = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("coupler", "flow_to_rtc"),
                        new KeyValuePair<string, string>("start", "RTC Model")
                    };

                var controlStartElements = new List<string>();
                var parallelStartElements = new List<string> { "FlowFM" };
                XmlDimrCheckControl(rootElement, true, "0 20 3600", elementsInOrder, controlStartElements, parallelStartElements);
                
                //Check component nodes
                XmlDimrCheckComponents(rootElement, new List<List<string>>
                    {
                        new List<string> {"RTC Model", "FBCTools_BMI", "0", "rtc", "."},
                        new List<string> {"FlowFM", "dflowfm", "0", "dflowfm", "FlowFM.mdu"}
                    });

                XmlDimrCheckCouplers(rootElement, new Dictionary<List<string>, Dictionary<string, string>>
                    {
                        {
                            new List<string>(new[] { "flow_to_rtc", "FlowFM", "RTC Model" }),
                            new Dictionary<string, string> { {"observations/ObservationFM/water_level", "input_ObservationFM_water_level" } }
                        }
                    });
            }
        }

        [Test]
        public void TestDimrConfigurationExport1D2DrtcWorkflow() //(RTC + (FlowFM + Flow1D))
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                app.Run();

                Model1D2DBuilder.Create1d2dModel(app);

                ICompositeActivity hydroModel = app.Project.RootFolder.Models.Cast<ICompositeActivity>().First();

                var fmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().First();
                var observationPointFm = new GroupableFeature2DPoint { Name = "ObservationFM" }; 
                fmModel.Area.ObservationPoints.Add(observationPointFm);

                var f1DModel = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

                var observationPointF1D = new ObservationPoint { Name = "ObservationF1D", Geometry = new Point(f1DModel.Network.Branches[0].Geometry.Coordinate.X + 10, f1DModel.Network.Branches[0].Geometry.Coordinate.Y) };
                var weir = new Weir { Name = "Weir1", Geometry = new Point(f1DModel.Network.Branches[0].Geometry.Coordinate.X + 20, f1DModel.Network.Branches[0].Geometry.Coordinate.Y) };

                f1DModel.Network.Branches[0].BranchFeatures.Add(observationPointF1D);
                f1DModel.Network.Branches[0].BranchFeatures.Add(weir);

                var inputF1D = new Input { ParameterName = "parameterObsF1d", Feature = observationPointF1D };
                var inputFm = new Input { ParameterName = "parameterObsFM", Feature = observationPointFm };
                var outputF1D = new Output { ParameterName = "parameterWeir", Feature = weir };
                var rule = new PIDRule { Name = "noot", Inputs = { inputF1D }, Outputs = { outputF1D } };
                var condition = new StandardCondition { Name = "aap", Input = inputFm, TrueOutputs = { rule }, FalseOutputs = { rule } };
                var tableFunction = LookupSignal.DefineFunction();
                tableFunction[8.65] = 8.20;
                tableFunction[9.10] = 8.05;
                tableFunction[9.60] = 7.60;
                tableFunction[10.0] = 7.40;
                var signal = new LookupSignal { Name = "signal", Inputs = { inputFm }, RuleBases = { rule }, Function = tableFunction};
                var controlGroup = new ControlGroup
                {
                    Name = "test",
                    Conditions = { condition },
                    Rules = { rule },
                    Inputs = { inputFm, inputF1D },
                    Outputs = { outputF1D },
                    Signals = { signal }
                };

                var rtcModel = new RealTimeControlModel { ControlGroups = { controlGroup } };

                hydroModel.Activities.Add(rtcModel);

                // attach models to eacht other
                var rtcInputdataItemF1D = rtcModel.AllDataItems
                    .First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, rtcModel.ControlGroups[0].Inputs[0]));

                var rtcInputdataItemFm = rtcModel.AllDataItems
                    .First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, rtcModel.ControlGroups[0].Inputs[1]));

                var rtcOutputDataItem = rtcModel.AllDataItems
                    .First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, rtcModel.ControlGroups[0].Outputs[0]));

                var flowObservationFmOutputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(observationPointFm).First();
                var flowObservationF1DOutputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(observationPointF1D).First();
                var flowWeirInputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(weir).First();

                // link
                rtcInputdataItemFm.LinkTo(flowObservationFmOutputDataItem);
                rtcInputdataItemF1D.LinkTo(flowObservationF1DOutputDataItem);
                flowWeirInputDataItem.LinkTo(rtcOutputDataItem);

                //(RTC + (FlowFM + Flow1D))

                var workflow = ((HydroModel)hydroModel).Workflows.FirstOrDefault(wf => wf.Name == "(RTC + (FlowFM + Flow1D))");
                Assert.That(workflow, Is.Not.Null);
                ((HydroModel)hydroModel).CurrentWorkflow = workflow;

                var dirPath = Path.GetFullPath("fmf1drtc");
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }
                var dirInfo = Directory.CreateDirectory("fmf1drtc");
                var exportFilePath = Path.Combine(dirInfo.FullName, "dimrWorkflowfmf1drtc.xml");
                var exporter = new DHydroConfigXmlExporter
                {
                    ExportFilePath = exportFilePath
                };

                Assert.IsTrue(exporter.Export(hydroModel, null));
                Assert.IsTrue(File.Exists(exportFilePath));
                var exportDirectory = Path.GetDirectoryName(exportFilePath) ?? "";
                Assert.IsTrue(Directory.Exists(Path.Combine(exportDirectory, f1DModel.DirectoryName)));
                Assert.IsTrue(Directory.Exists(Path.Combine(exportDirectory, fmModel.DirectoryName)));
                Assert.IsTrue(Directory.Exists(Path.Combine(exportDirectory, rtcModel.DirectoryName)));
                Assert.IsTrue(Directory.Exists(Path.Combine(exportDirectory, new Iterative1D2DCoupler().DirectoryName)));
                
                // Open File
                XDocument exportedDocument = XDocument.Load(exportFilePath);
                ValidateXml(exportFilePath);

                var rootElement = exportedDocument.Root;
                Assert.IsNotNull(rootElement);

                //Check control node
                var elementsInOrder = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("coupler", "1d2d_to_rtc"),
                        new KeyValuePair<string, string>("start", "RTC Model")
                    };
                var controlStartElements = new List<string>();
                var parallelStartElements = new List<string> { "1d2d" };
                XmlDimrCheckControl(rootElement, true, "0 20 3600", elementsInOrder, controlStartElements, parallelStartElements);
                
                //Check component nodes
                var componentValuesList = new List<List<string>>
                    {
                        new List<string> {"RTC Model", "FBCTools_BMI", "0", "rtc", "."},
                        new List<string> {"1d2d", "flow1d2d", "0", "1d2dcoupler", "1d2d.ini"}
                    };
                XmlDimrCheckComponents(rootElement, componentValuesList);
                
                var couplersList = new Dictionary<List<string>, Dictionary<string, string>>
                {
                    {
                      new List<string>(new[] {"1d2d_to_rtc", "1d2d", "RTC Model"}),
                      new Dictionary<string, string>
                      {
                          { "FlowFM/observations/ObservationFM/water_level", "input_ObservationFM_water_level" },
                          { "Flow1D/observations/ObservationF1D/water_level","input_ObservationF1D_Water level (op)" }
                      }
                    }
                };

                XmlDimrCheckCouplers(rootElement, couplersList);
            }
        }

        private static void ValidateXml(string exportFilePath)
        {
            var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema |
                                      XmlSchemaValidationFlags.ProcessSchemaLocation |
                                      XmlSchemaValidationFlags.ReportValidationWarnings
                };

            try
            {
                //Check file is opened and the xml complies to the XSD.
                using (var xmlReader = XmlReader.Create(exportFilePath, settings))
                {
                    // Parse the file. 
                    while (xmlReader.Read())
                    {
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Couldn't validate dimr xml!!" + Environment.NewLine + e.Message);
            }
        }

        private static void XmlDimrCheckControl(XElement rootElement, bool isParallel, string timeStep, IList<KeyValuePair<string, string>> elementsInOrder, IList<string> controlStarters, IList<string> parallelStarters)
        {
            var nameSpace = rootElement.Name.Namespace;
            var controlElements = rootElement.Elements(nameSpace + "control").ToList();
            Assert.AreEqual(controlElements.Count, 1, "There should only be one control node");

            var controlElement = controlElements.FirstOrDefault();
            Assert.NotNull(controlElement);

            //Fetch parallel children
            if (isParallel)
            {
                var parallelElement = controlElement.Element(nameSpace + "parallel");
                Assert.NotNull(parallelElement);
                Assert.AreEqual(parallelStarters.Count + 1, parallelElement.Elements().Count()); //We might have more than one start node.

                //Checking startGroup order
                XmlDimrCheckStartGroup(parallelElement, nameSpace, timeStep, elementsInOrder, parallelStarters);
            }

            //Check start component (it can also be outside the parallel group)
            XmlDimrCheckStartElement(controlElement, nameSpace, controlStarters);
        }

        private static void XmlDimrCheckStartGroup(XElement parallelElement, XNamespace nameSpace, string time, IList<KeyValuePair<string, string>> elementsInOrder, IList<string> parallelStarters)
        {
            var startGroupElement = parallelElement.Element(nameSpace + "startGroup");
            Assert.NotNull(startGroupElement);

            var elements = startGroupElement.Elements().ToList();
            Assert.AreEqual(elementsInOrder.Count + 1, elements.Count); //timestep is not included in the list

            elements[0].CheckElement(nameSpace,"time", time);

            var startGroupElements = elements.Skip(1).ToList();
            Assert.AreEqual(startGroupElements.Count, elementsInOrder.Count);

            var zip = startGroupElements.Zip(elementsInOrder, (element, pair) => new Tuple<XElement, string, string>(element, pair.Key, pair.Value));

            foreach (var tuple in zip)
            {
                tuple.Item1.CheckElement(nameSpace, tuple.Item2);
                tuple.Item1.CheckElementAttribute("name", tuple.Item3);
            }

            //Check start component
            XmlDimrCheckStartElement(parallelElement, nameSpace, parallelStarters);
        }

        private static void XmlDimrCheckStartElement(XElement parentElement, XNamespace nameSpace, IList<string> startersList)
        {
            var startElementList = parentElement.Elements(nameSpace + "start").ToList();
            Assert.AreEqual(startersList.Count, startElementList.Count);

            var startElementExpectedValuesList = startElementList.Zip(startersList, (element, s) => new Tuple<XElement, string>(element, s));

            foreach (var tuple in startElementExpectedValuesList)
            {
                tuple.Item1.CheckElement(nameSpace, "start");
                tuple.Item1.CheckElementAttribute("name", tuple.Item2);
            }
        }

        private static void XmlDimrCheckComponents(XElement rootElement, IList<List<string>> componentValuesList)
        {
            var nameSpace = rootElement.Name.Namespace;
            var componentElements = rootElement.Elements(nameSpace + "component").ToList();

            Assert.AreEqual(componentValuesList.Count, componentElements.Count);

            var componentComponentValuesList = componentElements.Zip(componentValuesList,(element, list) => new Tuple<XElement, List<string>> (element, list));

            foreach (var tuple in componentComponentValuesList)
            {
                XmlDimrCheckComponentChildren(tuple.Item1, nameSpace, tuple.Item2);
            }
        }

        private static void XmlDimrCheckComponentChildren(XElement component, XNamespace nameSpace, IList<string> componentValuesList)
        {
            //First check component
            component.CheckElementAttribute("name", componentValuesList[0]);

            //Fetch children in expected order;
            var elements = component.Elements().ToList();
            Assert.AreEqual(3, elements.Count);

            elements[0].CheckElement(nameSpace, "library", componentValuesList[1]);
            elements[1].CheckElement(nameSpace, "workingDir", componentValuesList[3]);
            elements[2].CheckElement(nameSpace, "inputFile", componentValuesList[4]);
        }

        private static void XmlDimrCheckCouplers(XElement rootElement, IDictionary<List<string>, Dictionary<string, string>> couplersValuesAndItemsList)
        {
            var nameSpace = rootElement.Name.Namespace;

            //Check coupler nodes
            var couplerElements = rootElement.Elements(nameSpace + "coupler").ToList();
            Assert.AreEqual(couplerElements.Count, couplersValuesAndItemsList.Count);

            var couplersValuesItemsListZip = couplerElements.Zip(couplersValuesAndItemsList,
                (element, pair) => new Tuple<XElement, List<string>, Dictionary<string, string>>(element, pair.Key, pair.Value));

            foreach (var tuple in couplersValuesItemsListZip)
            {
                XmlDimrCheckCoupler(tuple.Item1, nameSpace, tuple.Item2, tuple.Item3);
            }
        }

        private static void XmlDimrCheckCoupler(XElement couplerElement, XNamespace nameSpace, IList<string> valuesList, IDictionary<string, string> itemsList)
        {
            Assert.AreEqual(3, valuesList.Count);

            couplerElement.CheckElementAttribute("name", valuesList[0]);

            var elements = couplerElement.Elements().ToList();
            Assert.Greater(elements.Count, 2);

            elements[0].CheckElement(nameSpace, "sourceComponent", valuesList[1]);
            elements[1].CheckElement(nameSpace, "targetComponent", valuesList[2]);

            //Fetch coupler component items.
            var itemElementsList = elements.Skip(2).ToList();
            Assert.AreEqual(itemsList.Count, itemElementsList.Count);

            // Check coupler nodes
            var couplerSourceTargetList = itemElementsList.Zip(itemsList, (element, pair) => new Tuple<XElement, string, string>(element, pair.Key, pair.Value));

            foreach (var tuple in couplerSourceTargetList)
            {
                XmlDimrCheckCouplerItemNode(tuple.Item1, nameSpace, tuple.Item2, tuple.Item3);
            }
        }

        private static void XmlDimrCheckCouplerItemNode(XElement couplerItem, XNamespace nameSpace, string sourceNameValue, string targetNameValue)
        {
            couplerItem.CheckElement(nameSpace, "item");
            
            var itemSourceAndTarget = couplerItem.Elements().ToList();
            Assert.AreEqual(2, itemSourceAndTarget.Count);

            itemSourceAndTarget[0].CheckElement(nameSpace, "sourceName", sourceNameValue);
            itemSourceAndTarget[1].CheckElement(nameSpace, "targetName", targetNameValue);
        }
    }

    public static class XElementTestExtensions
    {
        public static void CheckElement(this XElement element, XNamespace nameSpace, string elementName)
        {
            Assert.AreEqual(nameSpace, element.Name.Namespace);
            Assert.AreEqual(elementName, element.Name.LocalName);
        }

        public static void CheckElement(this XElement element, XNamespace nameSpace, string elementName, string elementValue)
        {
            element.CheckElement(nameSpace, elementName);
            Assert.AreEqual(elementValue, element.Value);
        }

        public static void CheckElementAttribute(this XElement element, string attributeName, string attributeValue)
        {
            Assert.AreEqual(attributeValue, element.Attribute(attributeName).Value);
        }
    }

}