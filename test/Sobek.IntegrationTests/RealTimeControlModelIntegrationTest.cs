using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using SharpTestsEx;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class RealTimeControlModelIntegrationTest
    {
        [Test]
        public void CopyToXmlForEmptyRulesAndConditionsWorks()
        {
            var rtcDomainAssembly = typeof (RtcBaseObject).Assembly;
            
            var allTypes = rtcDomainAssembly.GetTypes();
            var rulesAndConditions = allTypes.Where(t => t.Implements(typeof (RtcBaseObject)) && (!t.IsAbstract));

            int numTypes = 0;

            foreach(var type in rulesAndConditions)
            {
                var instance = (RtcBaseObject) Activator.CreateInstance(type);
                instance.ToXml(XNamespace.Get(""), "");
                numTypes++;
            }

            Assert.GreaterOrEqual(numTypes, 9); //9 types at time of writing
        }

        [Test]
        public void DeepCloneDoesNotCloneReferencesToWeirOfOriginalModel()
        {
            var weir = new Weir("weir") { Geometry = new Point(new Coordinate(10, 0)) };
            var sourceNode = new HydroNode() { Geometry = new Point(new Coordinate(0, 0)) };
            var targetNode = new HydroNode() { Geometry = new Point(new Coordinate(100, 0)) };
            var network = new HydroNetwork
                {
                    Branches = { new Channel { BranchFeatures = { weir }, Source = sourceNode, Target = targetNode } }, 
                    Nodes = { sourceNode, targetNode }
                };
            var flowModel = new WaterFlowModel1D { Network = network };

            var input = new Input();
            var output = new Output();
            var controlGroup = new ControlGroup { Inputs = { input }, Outputs = { output } };
            var rtcmodel = new RealTimeControlModel { ControlGroups = { controlGroup }};
            var hydroModel = new HydroModel { Activities = { rtcmodel } };

            hydroModel.Region.SubRegions.Add(flowModel.Region);
            hydroModel.Activities.Add(flowModel);

            var weirInputDataItem = flowModel.GetChildDataItems(weir).First(dataItem => (dataItem.Role & DataItemRole.Input) == DataItemRole.Input);

            // link 2 sides
            rtcmodel.GetDataItemByValue(input).LinkTo(weirInputDataItem);
            weirInputDataItem.LinkTo(rtcmodel.GetDataItemByValue(output));

            // clone
            var clonedHydroModel = (HydroModel)hydroModel.DeepClone();
            var clonedFlow = clonedHydroModel.Models.OfType<WaterFlowModel1D>().First();
            var clonedWeir = clonedFlow.Network.Weirs.First();

            clonedWeir.Name += " (clone)";

            // find number of occurances of weir in the original and cloned models
            var hitsOriginal = TestReferenceHelper.SearchObjectInObjectGraph(weir, hydroModel);
            var hitsCloned = TestReferenceHelper.SearchObjectInObjectGraph(weir, clonedHydroModel);
            var hitsClonedWeir = TestReferenceHelper.SearchObjectInObjectGraph(clonedWeir, hydroModel);

            Console.WriteLine("References to the weir in the original model:");
            hitsOriginal.ForEach(Console.WriteLine);

            Console.WriteLine();
            Console.WriteLine("References to the weir in the cloned model:");
            hitsCloned.ForEach(Console.WriteLine);

            Console.WriteLine("References to the cloned weir in the original model:");
            hitsClonedWeir.ForEach(Console.WriteLine);

            // asserts
            Assert.GreaterOrEqual(hitsOriginal.Count, 1, "number of references to the weir in the original model");
            Assert.AreEqual(0, hitsCloned.Count, "number of references to the weir in the cloned model");
            Assert.AreEqual(0, hitsClonedWeir.Count, "number of references to the cloned weir in the original model");
        }

        [Test]
        public void DeepCloneCopiesValuesCorrectly()
        {
            var weir = new Weir { Name = "weir1", CrestWidth = 1.0, Geometry = new Point(new Coordinate(10,0))};
            var sourceNode = new HydroNode() { Geometry = new Point(new Coordinate(0, 0)) };
            var targetNode = new HydroNode() { Geometry = new Point(new Coordinate(100, 0)) };
            var network = new HydroNetwork
                {
                    Branches = { new Channel { BranchFeatures = { weir }, Source = sourceNode, Target = targetNode } },
                    Nodes = { sourceNode, targetNode }
                };
            var flowModel = new WaterFlowModel1D { Network = network };

            var input = new Input();
            var controlGroup = new ControlGroup { Inputs = { input } };
            var rtcmodel = new RealTimeControlModel { ControlGroups = { controlGroup } };
            var hydroModel = new HydroModel { Activities = { rtcmodel } };

            hydroModel.Region.SubRegions.Add(flowModel.Region);
            hydroModel.Activities.Add(flowModel);

            // link input to weir crest width
            var weirInputDataItem = flowModel.GetChildDataItems(weir).First(dataItem => (dataItem.Role & DataItemRole.Input) == DataItemRole.Input);
            rtcmodel.GetDataItemByValue(input).LinkTo(weirInputDataItem);

            // clone
            var clonedModel = (HydroModel)hydroModel.DeepClone();
            var clonedRtc = clonedModel.Models.OfType<RealTimeControlModel>().First();

            clonedRtc.ControlGroups[0].Inputs[0].Value
                .Should("Values of inputs are copied correctly during clone").Be.EqualTo(1.0);
        }

        [Test]
        public void DeepCloneCheckLateralSourcesReferenceDeleted()
        {
            var flowModel = new WaterFlowModel1D { Network = HydroNetworkHelper.GetSnakeHydroNetwork(2) };
            var rtcmodel = new RealTimeControlModel();
            var hydroModel = new HydroModel { Activities = { rtcmodel } };

            hydroModel.Region.SubRegions.Add(flowModel.Region);
            hydroModel.Activities.Add(flowModel);

            var network = hydroModel.Region.SubRegions.OfType<HydroNetwork>().FirstOrDefault();

            var controlGroup = new ControlGroup();
            rtcmodel.ControlGroups.Add(controlGroup);

            var lateral = new LateralSource {Name = "lat sour", Geometry = new Point(network.Branches[0].Geometry.Coordinates[0]) };
            network.Branches[0].BranchFeatures.Add(lateral);

            var weirDataItem = flowModel.GetChildDataItems(lateral).First(dataItem => (dataItem.Role & DataItemRole.Input) == DataItemRole.Input);

            var input = new Input();
            controlGroup.Inputs.Add(input);
            rtcmodel.GetDataItemByValue(input).LinkTo(weirDataItem);

            var output = new Output();
            controlGroup.Outputs.Add(output);
            weirDataItem.LinkTo(rtcmodel.GetDataItemByValue(output));

            var clonedHydro = (HydroModel)hydroModel.DeepClone();

            var wfmodelClone = clonedHydro.Models.OfType<WaterFlowModel1D>().First();
            wfmodelClone.Network.Name += " (clone)";
            wfmodelClone.Network.LateralSources.First().Name += " (clone)";

            //var hits = TestReferenceHelper.SearchObjectInObjectGraph(lateral, rtcmodel);
            var hitsCloned = TestReferenceHelper.SearchObjectInObjectGraph(lateral, clonedHydro);

            //hits.ForEach(Console.WriteLine);
            Console.WriteLine();
            Console.WriteLine("cloned: ");
            hitsCloned.ForEach(Console.WriteLine);

            //Assert.GreaterOrEqual(hits.Count, 1);
            Assert.AreEqual(0, hitsCloned.Count);
        }

        [Test]
        public void DeepCloneCheckOutputOutOfSync()
        {
            var rtcmodel = new RealTimeControlModel{ OutputOutOfSync =  true };
            var clonedRtcModel = (RealTimeControlModel) rtcmodel.DeepClone();

            Assert.AreEqual(rtcmodel.OutputOutOfSync, clonedRtcModel.OutputOutOfSync);
        }

        [Test]
        public void TestDeepCloneHandlesOutputFileFunctionStore()
        {
            // create flow1d model
            var observationPoint = new ObservationPoint() { Name = "Near pipe", Geometry = new Point(new Coordinate(10, 0)) };
            var from = new HydroNode() { Geometry = new Point(new Coordinate(0, 0)) };
            var to = new HydroNode() { Geometry = new Point(new Coordinate(100, 0)) };
            var network = new HydroNetwork { Branches = { new Channel { BranchFeatures = { observationPoint }, Source = from, Target = to } }, Nodes = { from, to } };
            var flowModel = new WaterFlowModel1D { Network = network };

            // create RTC model
            var input = new Input();
            var rtcModel = new RealTimeControlModel { ControlGroups = { new ControlGroup { Inputs = { input } } } };
            var hydroModel = new HydroModel { Activities = { rtcModel } };

            hydroModel.Region.SubRegions.Add(flowModel.Region);
            hydroModel.Activities.Add(flowModel);

            var workflow = hydroModel.Workflows.FirstOrDefault(wf => wf.Name == "(RTC + Flow1D)");
            Assert.NotNull(workflow);
            hydroModel.CurrentWorkflow = workflow;

            // attach models to each other
            var source = rtcModel.GetDataItemByValue(input);
            var target = flowModel.GetChildDataItems(observationPoint).First();

            // link
            target.LinkTo(source);

            // Connect output
            var testFilePath = TestHelper.GetTestFilePath(@"RtcOutput\" + rtcModel.OutputFileName);
            TypeUtils.CallPrivateMethod(rtcModel, "ReconnectOutputFiles", new[] { testFilePath });

            // Check initial conditions
            var outputFunctionStore = rtcModel.OutputFileFunctionStore;
            Assert.NotNull(outputFunctionStore);

            // Clone model
            var clonedRtcModel = (RealTimeControlModel)rtcModel.DeepClone();

            // Check results
            var clonedoutputFunctionStore = clonedRtcModel.OutputFileFunctionStore;
            Assert.NotNull(clonedoutputFunctionStore);
            Assert.AreEqual(outputFunctionStore.Path, clonedoutputFunctionStore.Path);
        }

        [Test]
        public void TestUpdateCoordinateSystemIsReflectedInOutputFileFunctionStore()
        {
            var function = new FeatureCoverage();
            var outputFileFunctionStore = new RealTimeControlOutputFileFunctionStore() {Functions = { function }};
            var rtcModel = new RealTimeControlModel() {OutputFileFunctionStore = outputFileFunctionStore};

            Assert.AreEqual(null, rtcModel.CoordinateSystem);
            Assert.AreEqual(null, outputFileFunctionStore.CoordinateSystem);
            Assert.AreEqual(null, function.CoordinateSystem);

            var exampleCoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);

            rtcModel.CoordinateSystem = exampleCoordinateSystem;
            Assert.AreEqual(exampleCoordinateSystem, outputFileFunctionStore.CoordinateSystem);
            Assert.AreEqual(exampleCoordinateSystem, function.CoordinateSystem);
        }

        [Test]
        [Category(TestCategory.VerySlow)]
        public void DeepCloneCheckFlowModelBeingRewired()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\JAMM2010.sbk\40\deftop.1");

            var modelImporter = new SobekHydroModelImporter(false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(pathToSobekNetwork);
            var wfmodel = hydroModel.Models.OfType<WaterFlowModel1D>().First();

            var clonedHydroModel = hydroModel.DeepClone() as HydroModel;
            
            var hitsCloned = TestReferenceHelper.SearchObjectInObjectGraph(wfmodel, clonedHydroModel);
            
            Console.WriteLine("cloned 1: ");
            hitsCloned.ForEach(Console.WriteLine);
            Assert.AreEqual(0, hitsCloned.Count);
        }

        [Test]
        [Category(TestCategory.VerySlow)]
        public void DeepCloneCheckNetworkIsReplaced()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\JAMM2010.sbk\40\deftop.1");

            var modelImporter = new SobekHydroModelImporter(false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(pathToSobekNetwork);
            var wfmodel = hydroModel.Models.OfType<WaterFlowModel1D>().First();
            var network = wfmodel.Network;

            var clonedHydroModel = hydroModel.DeepClone() as HydroModel;
            var hitsCloned = TestReferenceHelper.SearchObjectInObjectGraph(network, clonedHydroModel);

            Console.WriteLine("cloned 1: ");
            hitsCloned.ForEach(Console.WriteLine);
            Assert.AreEqual(0, hitsCloned.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RemovingBranchFeaturesUsedByControlGroupsResetsInputsAndOutputsToDefault()
        {
            var flowModel = new WaterFlowModel1D { Network = HydroNetworkHelper.GetSnakeHydroNetwork(2) };
            var rtcmodel = new RealTimeControlModel();
            var hydroModel = new HydroModel { Activities = { rtcmodel } };

            hydroModel.Region.SubRegions.Add(flowModel.Region);
            hydroModel.Activities.Add(flowModel);

            var network = hydroModel.Region.SubRegions.OfType<HydroNetwork>().FirstOrDefault();
           
            var weir = new Weir("weir") {Geometry = new Point(network.Branches[0].Geometry.Coordinates[0])};
            network.Branches[0].BranchFeatures.Add(weir);

            var dataItemsForWeir = flowModel.GetChildDataItems(weir).Where(ei => (ei.Role & DataItemRole.Input) == DataItemRole.Input);
            Assert.AreNotEqual(0, dataItemsForWeir.Count());

            var dataItem = dataItemsForWeir.First();

            var controlGroup = new ControlGroup();
            rtcmodel.ControlGroups.Add(controlGroup);

            var input = new Input();
            controlGroup.Inputs.Add(input);
            rtcmodel.GetDataItemByValue(input).LinkTo(dataItem);
            
            var output = new Output();
            controlGroup.Outputs.Add(output);
            dataItem.LinkTo(rtcmodel.GetDataItemByValue(output));
            
            Assert.IsTrue(input.Name.StartsWith("weir"));
            Assert.IsTrue(output.Name.StartsWith("weir"));

            network.Branches[0].BranchFeatures.Remove(weir);

            Assert.IsFalse(input.Name.StartsWith("weir"));
            Assert.IsFalse(output.Name.StartsWith("weir"));
        }



        /// <summary>
        /// object is removed that has side effect of removing linked object. Removal of branch
        /// will also delete weir but no notification of weir removal is sent.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void RemovingBranchResetsInputLinkedToWeirDataItemToDefault()
        {
            var flowModel = new WaterFlowModel1D { Network = HydroNetworkHelper.GetSnakeHydroNetwork(2) };
            var rtcmodel = new RealTimeControlModel();
            var hydroModel = new HydroModel { Activities = { rtcmodel } };

            hydroModel.Region.SubRegions.Add(flowModel.Region);
            hydroModel.Activities.Add(flowModel);

            var network = hydroModel.Region.SubRegions.OfType<HydroNetwork>().FirstOrDefault();

            var weir = new Weir("weir") {Geometry = new Point(network.Branches[0].Geometry.Coordinates[0])};
            network.Branches[0].BranchFeatures.Add(weir);

            var controlGroup = new ControlGroup();
            rtcmodel.ControlGroups.Add(controlGroup);

            // link weir => input
            var input = new Input();
            controlGroup.Inputs.Add(input);
            
            var outputDataItem = flowModel.GetChildDataItems(weir).First(i => (i.Role & DataItemRole.Output) == DataItemRole.Output);
            rtcmodel.GetDataItemByValue(input).LinkTo(outputDataItem);
            
            // linke output => weir
            var output = new Output();
            controlGroup.Outputs.Add(output);

            var inputDataItem = flowModel.GetChildDataItems(weir).First(i => (i.Role & DataItemRole.Input) == DataItemRole.Input);
            inputDataItem.LinkTo(rtcmodel.GetDataItemByValue(output));

            // asserts
            Assert.IsTrue(input.Name.StartsWith("weir"));
            Assert.IsTrue(output.Name.StartsWith("weir"));

            network.Branches.Remove(network.Branches[0]);

            Assert.IsFalse(input.Name.StartsWith("weir"));
            Assert.IsFalse(output.Name.StartsWith("weir"));
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void CopyRtcModelAndReplaceFlow1DModelShouldNotGiveAnError()
        {
            var flowModel = new WaterFlowModel1D { Network = HydroNetworkHelper.GetSnakeHydroNetwork(2) };
            var rtcmodel = new RealTimeControlModel();
            var hydroModel = new HydroModel{ Activities = {rtcmodel} };

            hydroModel.Region.SubRegions.Add(flowModel.Region);
            hydroModel.Activities.Add(flowModel);

            var network = hydroModel.Region.SubRegions.OfType<HydroNetwork>().First();
            
            var weir = new Weir("weir") { Geometry = new Point(network.Branches[0].Geometry.Coordinates[0]) } ;
            network.Branches[0].BranchFeatures.Add(weir);

            var controlGroup = new ControlGroup();
            rtcmodel.ControlGroups.Add(controlGroup);

            // link weir => input
            var input = new Input();
            controlGroup.Inputs.Add(input);

            var outputDataItem = flowModel.GetChildDataItems(weir).First(i => (i.Role & DataItemRole.Output) == DataItemRole.Output);
            rtcmodel.GetDataItemByValue(input).LinkTo(outputDataItem);

            // linke output => weir
            var output = new Output();
            controlGroup.Outputs.Add(output);

            var inputDataItem = flowModel.GetChildDataItems(weir).First(i => (i.Role & DataItemRole.Input) == DataItemRole.Input);
            inputDataItem.LinkTo(rtcmodel.GetDataItemByValue(output));
            
            //Clone rtc-model
            var clonedHydroModel = (HydroModel)hydroModel.DeepClone();

            //Remove Flow1DModel
            clonedHydroModel.Activities.Remove(clonedHydroModel.Activities.OfType<WaterFlowModel1D>().First());

            //Add Flow1DModel
            clonedHydroModel.Activities.Add((WaterFlowModel1D)rtcmodel.ControlledModels.First().DeepClone());
        }


        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)] // test fails frequently on build server, find out why it happens before removing this category
        public void InitializeAndStartModelRunForNDBModelWithRTCToolsLimitedMemoryOptionTools7224()
        {
            // this requires field 'useLimitedMemory' in RealTimeControlXmlGenerator to be true
            // TODO: make this field, and field 'runEngineRemote' in RealTimeControlModel configurable
            // TODO: fix the original issue as described in TOOLS-7224 (in RTCTools code)

            string pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"030_NDB_zout_grotere_DX.lit\3\NETWORK.TP");

            var modelImporter = new SobekHydroModelImporter(false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(pathToSobekNetwork);
            var rtcModel = hydroModel.Models.OfType<RealTimeControlModel>().First();
            
            rtcModel.LimitMemory = true;
            
            rtcModel.Initialize();
            if (hydroModel.Status != ActivityStatus.Initialized)
            {
                Assert.Fail("Model initialization has failed");
            }

            rtcModel.Execute();
            if (hydroModel.Status == ActivityStatus.Failed)
            {
                Assert.Fail("Model run statup has failed");
            }
            rtcModel.Cleanup();
        }

        [Test]
        public void ExportRtcModelWillRefreshInitialValues()
        {
            // SOBEK3-633: When doing a DIMR export, not all changes to the model are percolated to the input data items for RTC. 
            // That means that the initial state of the model can be out-of-date. 
            // This all went alright when doing the model run. That is because the RealTimeControl.RefreshInitialState was called during initialize. 
            // This test checks whether the initial state is written correctly, when the user changes the input of an RTC rule. In this case, 
            // we chose to use the crest level to be changed by the user. 

            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.SobekModels);
            var flowModel1D = hydroModel.Activities.FirstOrDefault(m => m is WaterFlowModel1D) as WaterFlowModel1D;
            Assert.That(flowModel1D, Is.Not.Null);
            var rtcModel = hydroModel.Activities.FirstOrDefault(m => m is RealTimeControlModel) as RealTimeControlModel;
            Assert.That(rtcModel, Is.Not.Null);

            var channel = new Channel() {Geometry = new LineString(new [] {new Coordinate(0,0), new Coordinate(100,0)})};
            flowModel1D.Network.Branches.Add(channel);

            var weir1 = new Weir("weir1") { Geometry = new Point(flowModel1D.Network.Branches[0].Geometry.Coordinates[0]) };
            var weir2 = new Weir("weir2") { Geometry = new Point(flowModel1D.Network.Branches[0].Geometry.Coordinates[1]) };
            channel.BranchFeatures.AddRange(new [] {weir1, weir2});

            var input = new Input { ParameterName = "parameter", Feature = weir1 };
            var output = new Output { ParameterName = "parameter", Feature = weir2 };
            var rule = new TimeRule{ Name = "noot", Inputs = { input }, Outputs = { output } };
            var controlGroup = new ControlGroup
            {
                Name = "test",
                Rules = { rule },
                Inputs = { input },
                Outputs = { output },
            };

            rtcModel.ControlGroups.Add(controlGroup) ;
            
            // attach models to each other
            var rtcInputdataItem = rtcModel.AllDataItems
                .FirstOrDefault(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, input));
            Assert.That(rtcInputdataItem, Is.Not.Null); 
            var rtcOutputDataItem = rtcModel.AllDataItems
                .FirstOrDefault(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, output));
            Assert.That(rtcOutputDataItem, Is.Not.Null); 

            var flowOutputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(weir1).FirstOrDefault(di => di.Name.Contains("Crest level"));
            Assert.That(flowOutputDataItem, Is.Not.Null); 
            var flowInputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(weir2).FirstOrDefault(di => di.Name.Contains("Crest level"));
            Assert.That(flowInputDataItem, Is.Not.Null);

            // link
            rtcInputdataItem.LinkTo(flowOutputDataItem);
            flowInputDataItem.LinkTo(rtcOutputDataItem);

            // Action: change of crestlevel by the user!
            weir2.CrestLevel = 3.45; 

            // Test
            var exporter = new RealTimeControlModelExporter();

            var path = Path.GetFullPath("./testexportrtcmodel");
            try
            {
                FileUtils.DeleteIfExists(path);
                FileUtils.CreateDirectoryIfNotExists(path);
                Assert.That(exporter.Export(rtcModel, path),Is.True);

                var pathStateImport = Path.Combine(path, "state_import.xml");
                var txt = File.ReadAllText(pathStateImport);
                Assert.That(txt.Contains("<vector>3.45</vector>"));
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }
    }
}
