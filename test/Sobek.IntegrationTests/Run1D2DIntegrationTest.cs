using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DeveloperTools.Commands.IntegratedDemoModels;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class Run1D2DIntegrationTest
    {
        [Test]
        [Category(TestCategory.Slow)]
        public void VerifyThat1D2DModelRunsCorrectly()
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                app.Run();
                Model1D2DBuilder.Create1d2dModel(app);

                ICompositeActivity hydroModel = app.Project.RootFolder.Models.Cast<ICompositeActivity>().First();

                ActivityRunner.RunActivity(hydroModel);
                Assert.AreEqual(ActivityStatus.Cleaned, hydroModel.Status);

                // Waterlevel should decrease from 0.5 meter to a certain value. 
                var waterlevel1d = hydroModel.Activities.OfType<WaterFlowModel1D>().First().OutputWaterLevel;
                Assert.That((double)waterlevel1d.Components[0].MinValue < 0.2);  
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void VerifyThat1D2DModelRunsCorrectly_AfterRenamingModels()
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                app.Run();
                Model1D2DBuilder.Create1d2dModel(app);

                ICompositeActivity hydroModel = app.Project.RootFolder.Models.Cast<ICompositeActivity>().First();

                var flow1DModel = hydroModel.Activities.OfType<WaterFlowModel1D>().FirstOrDefault();
                Assert.NotNull(flow1DModel);
                flow1DModel.Name = flow1DModel.Name + "_renamed";

                var flowFMModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.NotNull(flowFMModel);
                flowFMModel.Name = flowFMModel.Name + "_renamed";

                ActivityRunner.RunActivity(hydroModel);
                Assert.AreEqual(ActivityStatus.Cleaned, hydroModel.Status);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void VerifyExpectedOutputLinkCoverages()
        {
            var expectedOutputLinkCoverageNames = new List<string>
            {
                "1d2d_zeta",
                "1d2d_crest_level",
                "1d2d_b_2di",
                "1d2d_b_2dv",
                "1d2d_d_2dv",
                "1d2d_qzeta",
                "1d2d_q_lat",
                "1d2d_cfl",
                "1d2d_sb",
                "1d2d_s0_2d",
                "1d2d_s1_2d",
            };

            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                app.Run();
                Model1D2DBuilder.Create1d2dModel(app);

                ICompositeActivity hydroModel = app.Project.RootFolder.Models.Cast<ICompositeActivity>().First();

                ActivityRunner.RunActivity(hydroModel);

                var coupler = hydroModel.CurrentWorkflow as Iterative1D2DCoupler;
                Assert.NotNull(coupler);

                Assert.AreEqual(expectedOutputLinkCoverageNames.Count, coupler.LinkCoverages.Count,
                    "Expected number of 1D2D output link coverages differs from actual, is this an intentional change? Verify this with FM kernel");

                foreach (var coverageName in expectedOutputLinkCoverageNames)
                {
                    Assert.IsTrue(coupler.LinkCoverages.Select(lc => lc.Name).Contains(coverageName),
                        string.Format("Could not find expected 1D2D output link coverage: {0}, was this renamed? Verify this with FM kernel", coverageName));
                }

            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void VerifyThatClearingOutputOfHydroModelWith1D2DModelRemovesOutputLinkCoverages()
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                app.Run();
                Model1D2DBuilder.Create1d2dModel(app);

                ICompositeActivity hydroModel = app.Project.RootFolder.Models.Cast<ICompositeActivity>().First();

                ActivityRunner.RunActivity(hydroModel);

                var coupler = hydroModel.CurrentWorkflow as Iterative1D2DCoupler;
                Assert.NotNull(coupler);

                Assert.IsTrue(coupler.LinkCoverages.Any());
                Assert.IsTrue(coupler.Data.OutputDataItems.Any());

                coupler.ClearOutput();

                Assert.AreEqual(0, coupler.LinkCoverages.Count);
                Assert.AreEqual(0, coupler.Data.OutputDataItems.Count());
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Ignore("CF DLL is not in dimr set anymore")]
        public void Given1d2dCoupledModelWhenDimrExportThenDimrExportedFile()
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                app.Run();
                Model1D2DBuilder.Create1d2dModel(app);
               
                ICompositeActivity hydroModel = app.Project.RootFolder.Models.Cast<ICompositeActivity>().First();

                var dirPath = Path.GetFullPath("fmf1d");
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }
                var dirInfo = Directory.CreateDirectory("fmf1d");
                var exporter = new DHydroConfigXmlExporter
                {
                    ExportFilePath = Path.Combine(dirInfo.FullName, "dimr.xml")
                };
                Assert.That(exporter.Export(hydroModel, null), Is.True);

                var dHydroActivity = hydroModel.CurrentWorkflow as IDimrModel;
                Assert.That(dHydroActivity, Is.Not.Null);
                var coupler1d2dFile = Path.Combine(dirInfo.FullName, dHydroActivity.DirectoryName.ToLower(),dHydroActivity.ShortName +".ini");
                var categories1d2dFile = new DelftIniReader().ReadDelftIniFile(coupler1d2dFile);
                var generalCategory = categories1d2dFile.FirstOrDefault(category => category.Name == "General");
                Assert.That(generalCategory, Is.Not.Null);
                var filetype = generalCategory.GetPropertyValue("fileType");
                Assert.That(filetype, Is.EqualTo(GeneralRegion.FileTypeName.Iterative1D2DCoupler));

                var f1dModelCategory = categories1d2dFile.FirstOrDefault(category => category.Name == "Model" && category.GetPropertyValue("type") == "Flow1D");
                Assert.That(f1dModelCategory, Is.Not.Null);
                var modelType = f1dModelCategory.GetPropertyValue("type");
                Assert.That(modelType, Is.EqualTo("Flow1D"));
                var modelName = f1dModelCategory.GetPropertyValue("name");
                Assert.That(modelName, Is.EqualTo(hydroModel.Activities.OfType<WaterFlowModel1D>().First().Name));
                var directory = f1dModelCategory.GetPropertyValue("directory");
                Assert.That(directory, Is.EqualTo(@"..\"+hydroModel.Activities.OfType<WaterFlowModel1D>().First().DirectoryName));
                var modelDefinitionFile = f1dModelCategory.GetPropertyValue("modelDefinitionFile");
                Assert.That(modelDefinitionFile, Is.EqualTo(hydroModel.Activities.OfType<WaterFlowModel1D>().First().Name + ModelFileNames.ModelFilenameExtension));

                var f2dModelCategory = categories1d2dFile.FirstOrDefault(category => category.Name == "Model" && category.GetPropertyValue("type") == "FlowFM");
                Assert.That(f2dModelCategory, Is.Not.Null);
                modelType = f2dModelCategory.GetPropertyValue("type");
                Assert.That(modelType, Is.EqualTo("FlowFM"));
                modelName = f2dModelCategory.GetPropertyValue("name");
                Assert.That(modelName, Is.EqualTo(hydroModel.Activities.OfType<WaterFlowFMModel>().First().Name));
                directory = f2dModelCategory.GetPropertyValue("directory");
                Assert.That(directory, Is.EqualTo(@"..\" + hydroModel.Activities.OfType<WaterFlowFMModel>().First().DirectoryName));
                modelDefinitionFile = f2dModelCategory.GetPropertyValue("modelDefinitionFile");
                Assert.That(modelDefinitionFile, Is.EqualTo(hydroModel.Activities.OfType<WaterFlowFMModel>().First().Name + ".mdu"));

                var filesCategory = categories1d2dFile.FirstOrDefault(category => category.Name == "Files");
                Assert.That(filesCategory, Is.Not.Null);
                var mappingFile = filesCategory.GetPropertyValue("mappingFile");
                Assert.That(mappingFile, Is.EqualTo(dHydroActivity.ShortName + "_mapping.ini"));
                var logFile = filesCategory.GetPropertyValue("logFile");
                Assert.That(logFile, Is.EqualTo("1d2d.log"));
                
                var parametersCategory = categories1d2dFile.FirstOrDefault(category => category.Name == "Parameters");
                Assert.That(parametersCategory, Is.Not.Null);
                var maximumIterations = parametersCategory.ReadProperty<int>("maximumIterations");
                var iterative1D2DCouplerData = ((Iterative1D2DCouplerData)((Iterative1D2DCoupler)hydroModel.CurrentWorkflow).Data);
                Assert.That(maximumIterations, Is.EqualTo(iterative1D2DCouplerData.MaxIteration));
                var maximumError = parametersCategory.ReadProperty<double>("maximumError");
                Assert.That(Math.Abs(maximumError-iterative1D2DCouplerData.MaxError), Is.LessThanOrEqualTo(double.Epsilon));
                

                var coupler1d2dMappingFile = Path.Combine(dirInfo.FullName, dHydroActivity.DirectoryName.ToLower(), dHydroActivity.ShortName +"_mapping.ini");
                var categories1d2dMappingFile = new DelftIniReader().ReadDelftIniFile(coupler1d2dMappingFile);
                generalCategory = categories1d2dMappingFile.FirstOrDefault(category => category.Name == "General");
                Assert.That(generalCategory, Is.Not.Null);
                filetype = generalCategory.GetPropertyValue("fileType");
                Assert.That(filetype, Is.EqualTo(GeneralRegion.FileTypeName.Iterative1D2DCouplerMapping));

                var featureCollection = new FeatureCollection((IList) ((Iterative1D2DCoupler) hydroModel.CurrentWorkflow).Features, typeof (Iterative1D2DCouplerLink));
                var iterative1D2DCouplerLinkCount = featureCollection.GetFeatureCount();
            
                var coordinateCount = categories1d2dMappingFile.Count(category => category.Name == "1d2dLink");
                Assert.That(coordinateCount, Is.EqualTo(iterative1D2DCouplerLinkCount));

                var first1d2dLinkCategory = categories1d2dMappingFile.FirstOrDefault(category => category.Name == "1d2dLink");
                Assert.That(first1d2dLinkCategory, Is.Not.Null);
                var coordinates1D = first1d2dLinkCategory.ReadPropertiesToListOfType<double>("XY_1D");
                var firstFeature = featureCollection.Features.Cast<Feature>().FirstOrDefault();
                Assert.That(firstFeature, Is.Not.Null);
              // Assert.NotNull(firstFeature);
                
                //getting the coordinates, stole this from RefreshMappings method in Iterative1D2DCoupler class
                //firstFeature.Geometry.Coordinates[1] = 1d coordinates of the feature
                Assert.That(Math.Abs(coordinates1D[0] - firstFeature.Geometry.Coordinates[1].X), Is.LessThanOrEqualTo(0.0001));
                Assert.That(Math.Abs(coordinates1D[1] - firstFeature.Geometry.Coordinates[1].Y), Is.LessThanOrEqualTo(0.0001));

                //getting the coordinates, stole this from RefreshMappings method in Iterative1D2DCoupler class
                //firstFeature.Geometry.Coordinates[0] = 2d coordinates of the feature
                var coordinates2D = first1d2dLinkCategory.ReadPropertiesToListOfType<double>("XY_2D");
                Assert.That(Math.Abs(coordinates2D[0] - firstFeature.Geometry.Coordinates[0].X), Is.LessThanOrEqualTo(0.0001));
                Assert.That(Math.Abs(coordinates2D[1] - firstFeature.Geometry.Coordinates[0].Y), Is.LessThanOrEqualTo(0.0001));
                
            }
        }
        
        [Test]
        [Category(TestCategory.Slow)]
        public void Given1d2dCoupledModelWithRTCWhenDimrExportThenDimrExportedFile()
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                
                app.Run();
                
                AddIntegratedModel1d2d.Create1d2dModel(app);
                
                ICompositeActivity hydroModel = app.Project.RootFolder.Models.Cast<ICompositeActivity>().First();

                var fmModel = hydroModel.Activities.OfType<WaterFlowFMModel>().First();
                var observationPointFM = new GroupableFeature2DPoint
                {
                    Name = "ObservationFM",
                    Geometry =
                        new LineString(new[]
                        {
                            new Coordinate(10,10),
                            new Coordinate(20,10)
                        })
                }; 
                fmModel.Area.ObservationPoints.Add(observationPointFM);

                var f1dModel = hydroModel.Activities.OfType<WaterFlowModel1D>().First();
                ModelTestHelper.RefreshCrossSectionDefinitionSectionWidths(f1dModel.Network);

                var observationPointF1D = new ObservationPoint { Name = "ObservationF1D", Geometry = new Point(f1dModel.Network.Branches[0].Geometry.Coordinate.X + 10, f1dModel.Network.Branches[0].Geometry.Coordinate.Y) };
                var weir = new Weir { Name = "Weir1", Geometry = new Point(f1dModel.Network.Branches[0].Geometry.Coordinate.X + 20, f1dModel.Network.Branches[0].Geometry.Coordinate.Y) };

                f1dModel.Network.Branches[0].BranchFeatures.Add(observationPointF1D);
                f1dModel.Network.Branches[0].BranchFeatures.Add(weir);

                var inputF1d = new Input { ParameterName = "parameterObsF1d", Feature = observationPointF1D };
                var inputFm = new Input { ParameterName = "parameterObsFM", Feature = observationPointFM };
                var outputF1d = new Output { ParameterName = "parameterWeir", Feature = weir};
                var rule = new PIDRule { Name = "noot", Inputs = { inputF1d }, Outputs = { outputF1d } };
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
                    Inputs = { inputFm, inputF1d },
                    Outputs = { outputF1d },
                    Signals = { signal }
                };

                var rtcModel = new RealTimeControlModel { ControlGroups = { controlGroup } }; 
                
                hydroModel.Activities.Add(rtcModel);
                
                // attach models to eacht other
                var rtcInputdataItemF1D = rtcModel.AllDataItems
                    .First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, rtcModel.ControlGroups[0].Inputs[0]));
                
                var rtcInputdataItemFM = rtcModel.AllDataItems
                    .First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, rtcModel.ControlGroups[0].Inputs[1]));

                var rtcOutputDataItem = rtcModel.AllDataItems
                    .First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, rtcModel.ControlGroups[0].Outputs[0]));

                var flowObservationFMOutputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(observationPointFM).First();
                var flowObservationF1DOutputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(observationPointF1D).First();
                var flowWeirInputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(weir).First();

                // link
                rtcInputdataItemFM.LinkTo(flowObservationFMOutputDataItem);
                rtcInputdataItemF1D.LinkTo(flowObservationF1DOutputDataItem);
                flowWeirInputDataItem.LinkTo(rtcOutputDataItem);
            
                //(RTC + (FlowFM + Flow1D))

                var workflow = ((HydroModel) hydroModel).Workflows.FirstOrDefault(wf => wf.Name == "(RTC + (FlowFM + Flow1D))");
                Assert.That(workflow, Is.Not.Null);
                ((HydroModel) hydroModel).CurrentWorkflow = workflow;
                
                var dirPath = Path.GetFullPath("fmf1drtc");
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }
                var dirInfo = Directory.CreateDirectory("fmf1drtc");
                var exportFilePath = Path.Combine(dirInfo.FullName, "dimr.xml");
                var exporter = new DHydroConfigXmlExporter
                {
                    ExportFilePath = exportFilePath
                };
                
                Assert.IsTrue(exporter.Export(hydroModel, null));
                Assert.IsTrue(File.Exists(exportFilePath));
                Assert.IsTrue(Directory.Exists(Path.Combine(Path.GetDirectoryName(exportFilePath) ?? string.Empty, f1dModel.DirectoryName)));
                Assert.IsTrue(Directory.Exists(Path.Combine(Path.GetDirectoryName(exportFilePath) ?? string.Empty, fmModel.DirectoryName)));
                Assert.IsTrue(Directory.Exists(Path.Combine(Path.GetDirectoryName(exportFilePath) ?? string.Empty, rtcModel.DirectoryName)));
                Assert.IsTrue(Directory.Exists(Path.Combine(Path.GetDirectoryName(exportFilePath) ?? string.Empty, new Iterative1D2DCoupler().DirectoryName)));
                // Open File
                XDocument exportedDocument = XDocument.Load(exportFilePath);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                try
                {
                    //Check file is opened and the xml complies to the XSD.
                    using (var xmlReader = XmlReader.Create(exportFilePath, settings))
                    {
                        // Parse the file. 
                        while (xmlReader.Read())
                        { }
                    }
                }
                catch (Exception e)
                {
                    Assert.Fail("Couldn't validate dimr xml!!" + Environment.NewLine + e.Message);
                }
                

                Assert.IsNotNull(exportedDocument.Root);
                var nameSpace = exportedDocument.Root.Name.Namespace;
                // Check if controls are written as we want
                var rootElement = exportedDocument.Root;
                Assert.AreEqual(rootElement.Elements(nameSpace+"control").Count(), 1); //Only one control node    
                //Fetch control node
                XElement controlNode = rootElement.Elements(nameSpace + "control").FirstOrDefault();
                Assert.NotNull(controlNode);
                //Fetch parallel children
                XElement parallelNode = controlNode.Element(nameSpace + "parallel");
                Assert.NotNull(parallelNode);
                Assert.That(parallelNode.Elements().Count(), Is.EqualTo(2) );
                XElement startGroupNode = parallelNode.Element(nameSpace + "startGroup");
                Assert.NotNull(startGroupNode);
                Assert.That(startGroupNode.Elements().Count(), Is.EqualTo(3));
                //Checking children order
                XElement firstGroupFirstChildNode = startGroupNode.Elements().First();
                    Assert.NotNull(firstGroupFirstChildNode);
                    Assert.That(firstGroupFirstChildNode.Name, Is.EqualTo(nameSpace + "time"));
                    Assert.That(firstGroupFirstChildNode.Value, Is.EqualTo("0 20 3600"));
                XElement secondStartGroupChildNode = firstGroupFirstChildNode.ElementsAfterSelf().First();
                    Assert.NotNull(secondStartGroupChildNode);
                    Assert.That(secondStartGroupChildNode.Name, Is.EqualTo(nameSpace + "coupler"));
                    Assert.That(secondStartGroupChildNode.Attribute("name").Value, Is.EqualTo("1d2d_to_rtc"));
                XElement thirdStartGroupChildNode = secondStartGroupChildNode.ElementsAfterSelf().First();
                    Assert.NotNull(thirdStartGroupChildNode);
                    Assert.That(thirdStartGroupChildNode.Name, Is.EqualTo(nameSpace + "start"));
                    Assert.That(thirdStartGroupChildNode.Attribute("name").Value, Is.EqualTo("RTC Model"));
                //Fetch component nodes
                var componentNodes = rootElement.Elements(nameSpace + "component").ToList();
                Assert.That(componentNodes.Count(), Is.EqualTo(2));
                XElement firstComponent = componentNodes.First();
                xmlDimrCheckComponentChildren(firstComponent, nameSpace, "RTC Model", "FBCTools_BMI", "0", "rtc", ".");
                XElement secondComponent = componentNodes.Last();
                xmlDimrCheckComponentChildren(secondComponent, nameSpace, "1d2d", "flow1d2d", "0", "1d2dcoupler", "1d2d.ini");
                //Fetch coupler node
                var couplerNodes = rootElement.Elements(nameSpace + "coupler").ToList();
                Assert.That(couplerNodes.Count, Is.EqualTo(1));
                XElement sourceComponent = couplerNodes.Elements().First();
                Assert.NotNull(sourceComponent);
                Assert.That(sourceComponent.Name, Is.EqualTo(nameSpace + "sourceComponent"));
                Assert.That(sourceComponent.Value, Is.EqualTo("1d2d"));
                XElement targetComponent = sourceComponent.ElementsAfterSelf().First();
                Assert.That(targetComponent.Name, Is.EqualTo(nameSpace +"targetComponent"));
                Assert.That(targetComponent.Value, Is.EqualTo("RTC Model"));
                //Fetch coupler component items.
                    var componentItems = targetComponent.ElementsAfterSelf().ToList();
                    Assert.That(componentItems.Count(), Is.EqualTo(3));
                    XElement firstComponentItem = componentItems.First();
                    xmlDimrCheckCouplerItemNode(firstComponentItem, nameSpace, "FlowFM/observations/ObservationFM/water_level", "input_ObservationFM_water_level");
                    XElement secondComponentItem = firstComponentItem.ElementsAfterSelf().First();
                    xmlDimrCheckCouplerItemNode(secondComponentItem, nameSpace, "Flow1D/observations/ObservationF1D/water_level", "input_ObservationF1D_Water level (op)");
                    XElement thirdComponentCoupler = secondComponentItem.ElementsAfterSelf().First();
                    /*Logger for the coupler, the '.' will be replaced in the future by a relative path. So the test is expected to fail here.*/
                    xmlDimrCheckCouplerLoggerNode(thirdComponentCoupler, nameSpace, ".", "1d2d_to_rtc.nc");  
            }
        }

        private void xmlDimrCheckCouplerLoggerNode(XElement couplerItem, XNamespace nameSpace, string workngDirValue, string outputFileValue)
        {
            Assert.NotNull(couplerItem);
            Assert.That(couplerItem.Name, Is.EqualTo(nameSpace + "logger"));
            var couplerWorkingDirAndOutputFile = couplerItem.Elements().ToList();
            Assert.That(couplerWorkingDirAndOutputFile.Count(), Is.EqualTo(2));
            XElement workingDir = couplerWorkingDirAndOutputFile.First();
            Assert.NotNull(workingDir);
            Assert.That(workingDir.Name, Is.EqualTo(nameSpace + "workingDir"));
            Assert.That(workingDir.Value, Is.EqualTo(workngDirValue));
            XElement outputFile = workingDir.ElementsAfterSelf().First();
            Assert.NotNull(outputFile);
            Assert.That(outputFile.Name, Is.EqualTo(nameSpace + "outputFile"));
            Assert.That(outputFile.Value, Is.EqualTo(outputFileValue));
        }

        private void xmlDimrCheckCouplerItemNode(XElement couplerItem, XNamespace nameSpace, string sourceNameValue, string targetNameValue)
        {
            Assert.NotNull(couplerItem);
            Assert.That(couplerItem.Name, Is.EqualTo(nameSpace + "item"));
            var itemSourceAndTarget = couplerItem.Elements().ToList();
            Assert.That(itemSourceAndTarget.Count(), Is.EqualTo(2));
            XElement sourceName = itemSourceAndTarget.First();
                Assert.NotNull(sourceName);
                Assert.That(sourceName.Name, Is.EqualTo(nameSpace + "sourceName"));
                Assert.That(sourceName.Value, Is.EqualTo(sourceNameValue));
            XElement targetName = sourceName.ElementsAfterSelf().First();
                Assert.NotNull(targetName);
                Assert.That(targetName.Name, Is.EqualTo(nameSpace + "targetName"));
                Assert.That(targetName.Value, Is.EqualTo(targetNameValue));
        }

        private void xmlDimrCheckComponentChildren(XElement component, XNamespace nameSpace, string componentAttributeName, string libraryValue, string processValue, string workingDirValue, string inputFileValue)
        {
            //First check component
            Assert.NotNull(component);
            Assert.That(component.Attribute("name").Value, Is.EqualTo(componentAttributeName));
            //Fetch children in expected order;
            XElement firstChildFirstComponentNode = component.Elements().First();
                Assert.NotNull(firstChildFirstComponentNode);
                Assert.That(firstChildFirstComponentNode.Name, Is.EqualTo(nameSpace + "library"));
                Assert.That(firstChildFirstComponentNode.Value, Is.EqualTo(libraryValue));
            XElement secondChildFirstComponentNode = firstChildFirstComponentNode.ElementsAfterSelf().First();
                Assert.NotNull(secondChildFirstComponentNode);
                Assert.That(secondChildFirstComponentNode.Name, Is.EqualTo(nameSpace + "workingDir"));
                Assert.That(secondChildFirstComponentNode.Value, Is.EqualTo(workingDirValue));
            XElement thirdChildFirstComponentNode = secondChildFirstComponentNode.ElementsAfterSelf().First();
                Assert.NotNull(thirdChildFirstComponentNode);
                Assert.That(thirdChildFirstComponentNode.Name, Is.EqualTo(nameSpace + "inputFile"));
                Assert.That(thirdChildFirstComponentNode.Value, Is.EqualTo(inputFileValue));
        }
    }
}