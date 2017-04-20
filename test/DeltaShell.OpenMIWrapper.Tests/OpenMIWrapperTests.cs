using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using Deltares.OpenMI.Oatc.Sdk.Backbone;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;
using OpenMI.Standard;
using TimeSpan = System.TimeSpan;

namespace DeltaShell.OpenMIWrapper.Tests
{
    [TestFixture]
    public class OpenMIWrapperTests
    {
        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        public void CheckBoundaryConditions()
        {
            try
            {
                DeltaShellOpenMIEngine.GetAdditionalPlugins = GetAdditionalPlugins;

                WaterFlowModel1D modelWithDemoNetwork = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                modelWithDemoNetwork.TimeStep = new TimeSpan(0, 1, 0);
                modelWithDemoNetwork.StartTime = new DateTime(2000, 1, 1);
                modelWithDemoNetwork.StopTime = new DateTime(2000, 1, 1, 0, 30, 0);
                modelWithDemoNetwork.OutputSettings.GridOutputTimeStep = new TimeSpan(0, 30, 0);
                modelWithDemoNetwork.OutputSettings.StructureOutputTimeStep = new TimeSpan(0, 30, 0);
                ILinkableComponent linkableComponent = CreateLinkableComponent(modelWithDemoNetwork);
                Assert.AreEqual(2, linkableComponent.InputExchangeItemCount, "#Inputs");
                Assert.AreEqual(35, linkableComponent.OutputExchangeItemCount, "#Outputs");

                // establish links
                ILinkableComponent providingModel = new DummyConnectedModel();
                ILinkableComponent consumingModel = new DummyConnectedModel();

                ILink boundaryInflowLink = EstablishLink("sinus",
                    providingModel, consumingModel.GetOutputExchangeItem(0),
                    linkableComponent, FindInputItem(linkableComponent, "Node1", "Discharge")
                    );
                IValueSet valueSet = providingModel.GetValues(new TimeStamp(0), boundaryInflowLink.ID);

                ILink dischargeLink = EstablishLink("dischargeLink",
                    linkableComponent, FindOutputItem(linkableComponent, "reach_segment", "Discharge"),
                    consumingModel, consumingModel.GetInputExchangeItem(2));

                linkableComponent.Prepare();

                // check value(s) after initialization
                valueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.Start, dischargeLink.ID);
                Assert.AreEqual(25, valueSet.Count, "#computed values");
                Assert.AreEqual(0.1d, ((IScalarSet) valueSet).GetScalar(1), 1e-6, "start value for discharge in river");

                // check value(s) at end of computation
                valueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.End, dischargeLink.ID);
                Assert.AreEqual(25, valueSet.Count, "#computed values");
                Assert.AreEqual(9.6024d, ((IScalarSet) valueSet).GetScalar(1), 1e-4, "end value for discharge in river");

                linkableComponent.Finish();
            }
            finally
            {
                DeltaShellOpenMIEngine.GetAdditionalPlugins = null;
            }
        }

        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        [Category(TestCategory.Integration)]
        public void CheckOutputSimpleModelA()
        {
            try
            {
                DeltaShellOpenMIEngine.GetAdditionalPlugins = GetAdditionalPlugins;

                const string testDataName = "SimpleModelA";
                var testDataDir = Path.Combine(TestHelper.GetDataDir(), testDataName);
                string testRunDir = testDataDir + "-Out";

                ILinkableComponent linkableComponent = CreateLinkableComponent(testDataDir, testRunDir);

                Assert.AreEqual(4, linkableComponent.InputExchangeItemCount, "#Inputs");
                Assert.AreEqual(201, linkableComponent.OutputExchangeItemCount, "#Outputs");
                IInputExchangeItem inputExchangeItem3 = linkableComponent.GetInputExchangeItem(3);
                IOutputExchangeItem outputExchangeItem172 = linkableComponent.GetOutputExchangeItem(172);
                IOutputExchangeItem outputExchangeItem195 = linkableComponent.GetOutputExchangeItem(195);
                Assert.AreEqual("Node003", inputExchangeItem3.ElementSet.ID);
                Assert.AreEqual("Water level", inputExchangeItem3.Quantity.ID);
                Assert.AreEqual("grid_point.Channel2_963.630", outputExchangeItem172.ElementSet.ID);
                Assert.AreEqual("Number of iterations", outputExchangeItem172.Quantity.ID);
                Assert.AreEqual("reach_segment", outputExchangeItem195.ElementSet.ID);
                Assert.AreEqual("FloodPlain2 Chezy values", outputExchangeItem195.Quantity.ID);

                // establish links
                ILinkableComponent consumingModel = new DummyConnectedModel();

                ILink lateralLink = EstablishLink("lateralLink",
                    linkableComponent, FindOutputItem(linkableComponent, "LatDisch_a", "Discharge"),
                    consumingModel, consumingModel.GetInputExchangeItem(0));

                ILink obsWaterLevelLink = EstablishLink("obsPointLink",
                    linkableComponent, FindOutputItem(linkableComponent, "ObsLoc_II", "Water level"),
                    consumingModel, consumingModel.GetInputExchangeItem(1));

                ILink waterLevelLink = EstablishLink("waterLevelLink",
                    linkableComponent, FindOutputItem(linkableComponent, "grid_point", "Water level"),
                    consumingModel, consumingModel.GetInputExchangeItem(2));

                linkableComponent.Prepare();

                // check value(s) after initialization
                double val;
                IValueSet valueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.Start, lateralLink.ID);
                Assert.AreEqual(1, valueSet.Count, "#computed values");
                Assert.AreEqual(-9.0d, ((IScalarSet)valueSet).GetScalar(0), 1e-6,
                                "start: realized value for lateral_A");

                valueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.Start, obsWaterLevelLink.ID);
                Assert.AreEqual(1, valueSet.Count, "#computed values");
                Assert.AreEqual(0.0d, ((IScalarSet)valueSet).GetScalar(0), 1e-6,
                                "start: correct value for obs_II water_level");

                valueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.Start, waterLevelLink.ID);
                Assert.AreEqual(19, valueSet.Count, "#computed values");
                Assert.AreEqual(0.0d, ((IScalarSet)valueSet).GetScalar(0), 1e-6,
                                "start: correct value for network water_level");

                // check value(s) at end of computation
                valueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.End, lateralLink.ID);
                Assert.AreEqual(1, valueSet.Count, "#computed values");
                Assert.AreEqual(-8.0d, ((IScalarSet)valueSet).GetScalar(0), 1e-6,
                                "end: realized value for lateral_A");

                valueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.End, obsWaterLevelLink.ID);
                Assert.AreEqual(1, valueSet.Count, "#computed values");
                val = ((IScalarSet)valueSet).GetScalar(0);
                Assert.AreEqual(-0.19998404583106111, val, 1e-6,
                                "end: correct value for obs_II water_level");

                valueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.End, waterLevelLink.ID);
                Assert.AreEqual(19, valueSet.Count, "#computed values");
                val = ((IScalarSet)valueSet).GetScalar(0);
                Assert.AreEqual(-0.1984988891979651, val, 1e-6,
                                "end: correct value for network water_level");

                linkableComponent.Finish();
            }
            finally
            {
                DeltaShellOpenMIEngine.GetAdditionalPlugins = null;
            }
        }

        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        [Category(TestCategory.Integration)]
        public void CheckOutputSimpleModelATwice()
        {
            // TOOLS-9043
            CheckOutputSimpleModelA();
            CheckOutputSimpleModelA();
        }

        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        [Category(TestCategory.Integration)]
        public void CheckInputSimpleModelA()
        {
            const string testDataName = "SimpleModelA";
            var testDataDir = Path.Combine(TestHelper.GetDataDir(), testDataName);
            const string testRunDir = testDataName + "-In";
            try
            {
                DeltaShellOpenMIEngine.GetAdditionalPlugins = GetAdditionalPlugins;
                ILinkableComponent linkableComponent = CreateLinkableComponent(testDataDir, testRunDir);

                // establish links
                ILinkableComponent providingModel = new DummyConnectedModel();
                ILinkableComponent consumingModel = new DummyConnectedModel();

                ILink lateralInputLink = EstablishLink("linear",
                    providingModel, consumingModel.GetOutputExchangeItem(0),
                    linkableComponent, FindInputItem(linkableComponent, "LatDisch_a", "Discharge")
                    );
                ILink waterlevelInputLink = EstablishLink("sinus2",
                    providingModel, consumingModel.GetOutputExchangeItem(0),
                    linkableComponent, FindInputItem(linkableComponent, "Node003", "Water level")
                    );
                IValueSet latDischargeValueSet = providingModel.GetValues(new TimeStamp(0), lateralInputLink.ID);
                Assert.AreEqual(-90d, ((IScalarSet) latDischargeValueSet).GetScalar(0), 1e-10, "input value for lateral_A");
                IValueSet waterLevelValueSet = providingModel.GetValues(new TimeStamp(0), waterlevelInputLink.ID);
                Assert.AreEqual(0.39359438d, ((IScalarSet)waterLevelValueSet).GetScalar(0), 1e-8, "input value for waterlevel Node003");

                ILink lateralOutputLink = EstablishLink("lateralOutLink",
                    linkableComponent, FindOutputItem(linkableComponent, "LatDisch_a", "Discharge"),
                    consumingModel, consumingModel.GetInputExchangeItem(0));

                ILink waterLevelOutputLink = EstablishLink("waterLevelLink",
                    linkableComponent, FindOutputItem(linkableComponent, "grid_point", "Water level"),
                    consumingModel, consumingModel.GetInputExchangeItem(2));

                linkableComponent.Prepare();

                // check value(s) after initialization
                latDischargeValueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.Start, lateralOutputLink.ID);
                Assert.AreEqual(1, latDischargeValueSet.Count, "#computed values");
                Assert.AreEqual(-9.0d, ((IScalarSet) latDischargeValueSet).GetScalar(0), 1e-6,
                    "realized value for lateral_A");

                latDischargeValueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.Start, waterLevelOutputLink.ID);
                Assert.AreEqual(19, latDischargeValueSet.Count, "#computed values");
                Assert.AreEqual(0.0d, ((IScalarSet) latDischargeValueSet).GetScalar(0), 1e-6,
                    "correct value for network water_level");

                // check value(s) at end of computation
                latDischargeValueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.End, lateralOutputLink.ID);
                Assert.AreEqual(1, latDischargeValueSet.Count, "#computed values");
                Assert.AreEqual(-346.0932617d, ((IScalarSet)latDischargeValueSet).GetScalar(0), 1e-6,
                    "realized value for lateral_A");

                latDischargeValueSet = linkableComponent.GetValues(linkableComponent.TimeHorizon.End, waterLevelOutputLink.ID);
                Assert.AreEqual(19, latDischargeValueSet.Count, "#computed values");
                Assert.AreEqual(-1.6911776d, ((IScalarSet)latDischargeValueSet).GetScalar(1), 1e-6,
                    "correct value for network water_level");

                linkableComponent.Finish();
            }
            finally
            {
                DeltaShellOpenMIEngine.GetAdditionalPlugins = null;
            }
        }

        private IEnumerable<ApplicationPlugin> GetAdditionalPlugins()
        {
            return new ApplicationPlugin[]
                {
                    new NHibernateDaoApplicationPlugin(),
                    new NetCdfApplicationPlugin(),
                    new CommonToolsApplicationPlugin(),
                    new ScriptingApplicationPlugin(),
                    new SharpMapGisApplicationPlugin(),
                    new NetworkEditorApplicationPlugin(),
                    new HydroModelApplicationPlugin(),
                    new RainfallRunoffApplicationPlugin(),
                    new WaterFlowModel1DApplicationPlugin(),
                    new RealTimeControlApplicationPlugin()
                };
        }
        
        private static ILinkableComponent CreateLinkableComponent(ITimeDependentModel rootModel)
        {
            ILinkableComponent linkableComponent = new DeltaShellOpenMILinkableComponent(rootModel);
            IArgument[] arguments = new IArgument[0];
            linkableComponent.Initialize(arguments);
            return linkableComponent;
        }

        private static ILinkableComponent CreateLinkableComponent(string sourcePath, string testRunDir)
        {
            PrepareTestDirectory(sourcePath, testRunDir);

            ILinkableComponent linkableComponent = new DeltaShellOpenMILinkableComponent();
            IArgument[] arguments =
            {
                new Argument("DsProjFilePath", Path.Combine(testRunDir, "SimpleModelA.dsproj"), true, ""),
                new Argument("ModelName", "simpleFlowModel", true, ""),
                new Argument("SplitSpecificElementSets", "grid_point", true, "")
            };
            using (CultureUtils.SwitchToCulture("nl-NL"))
            {
                linkableComponent.Initialize(arguments);
            }

            return linkableComponent;
        }

        private static void PrepareTestDirectory(string sourcePath, string testRunDir)
        {
            FileUtils.DeleteIfExists(testRunDir);
            Directory.CreateDirectory(testRunDir);
            FileUtils.CopyDirectory(sourcePath, testRunDir, ".svn");
        }

        private static ILink EstablishLink(string linkId,
                                           ILinkableComponent sourceComponent, IOutputExchangeItem outputItem,
                                           ILinkableComponent targetComponent, IInputExchangeItem inputItem)
        {
            ILink link = new Link(sourceComponent, outputItem.ElementSet, outputItem.Quantity,
                                         targetComponent, inputItem.ElementSet, inputItem.Quantity,
                                         linkId);
            sourceComponent.AddLink(link);
            targetComponent.AddLink(link);
            return link;
        }

        private IInputExchangeItem FindInputItem(ILinkableComponent linkableComponent, string elementsetId,
                                                 string quantityId)
        {
            string avaiableItems = "";
            for (int i = 0; i < linkableComponent.InputExchangeItemCount; i++)
            {
                IInputExchangeItem inputExchangeItem = linkableComponent.GetInputExchangeItem(i);
                var esId = inputExchangeItem.ElementSet.ID;
                var qId = inputExchangeItem.Quantity.ID;
                avaiableItems += "\n" + esId + "/" + qId;
                if (esId.Equals(elementsetId) &&
                    qId.Equals(quantityId))
                {
                    return inputExchangeItem;
                }
            }
            throw new Exception(String.Format("Input item not found {0}/{1}, available:{2}",
                elementsetId, quantityId, avaiableItems));
        }

        private IOutputExchangeItem FindOutputItem(ILinkableComponent linkableComponent, string elementsetId,
                                                   string quantityId)
        {
            string avaiableItems = "";
            for (int i = 0; i < linkableComponent.OutputExchangeItemCount; i++)
            {
                IOutputExchangeItem outputExchangeItem = linkableComponent.GetOutputExchangeItem(i);
                var esId = outputExchangeItem.ElementSet.ID;
                var qId = outputExchangeItem.Quantity.ID;
                avaiableItems += "\n" + esId + "/" + qId;
                if (esId.Equals(elementsetId) &&
                    qId.Equals(quantityId))
                {
                    return outputExchangeItem;
                }
            }
            throw new Exception(String.Format("Output item not found {0}/{1}, available:{2}",
                elementsetId, quantityId, avaiableItems));
        }
    }

    public class DummyConnectedModel : ILinkableComponent
    {
        private string modelId;

        private int timeStep;

        public DummyConnectedModel()
        {
            modelId = GetType().Name;
        }

        public int InputExchangeItemCount
        {
            get { throw new NotImplementedException(); }
        }

        public IInputExchangeItem GetInputExchangeItem(int inputExchangeItemIndex)
        {
            return new InputExchangeItem
            {
                ElementSet = new ElementSet("inputES", "inputES", ElementType.IDBased, null),
                Quantity = new Quantity("outputQuant" + inputExchangeItemIndex)
            };
        }

        public void Subscribe(IListener listener, EventType eventType)
        {
            throw new NotImplementedException();
        }

        public void UnSubscribe(IListener listener, EventType eventType)
        {
            throw new NotImplementedException();
        }

        public void SendEvent(IEvent Event)
        {
            throw new NotImplementedException();
        }

        public int GetPublishedEventTypeCount()
        {
            throw new NotImplementedException();
        }

        public EventType GetPublishedEventType(int providedEventTypeIndex)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IArgument[] properties)
        {
            throw new NotImplementedException();
        }

        public int OutputExchangeItemCount
        {
            get { throw new NotImplementedException(); }
        }

        public IOutputExchangeItem GetOutputExchangeItem(int outputExchangeItemIndex)
        {
            if (outputExchangeItemIndex == 0)
            {
                return new OutputExchangeItem
                {
                    ElementSet = new ElementSet("outputES1", "outputES1", ElementType.IDBased, null),
                    Quantity = new Quantity("outputQuant1" + outputExchangeItemIndex)
                };
            }
            else if (outputExchangeItemIndex == 1)
            {
                return new OutputExchangeItem
                {
                    ElementSet = new ElementSet("outputES2", "outputES2", ElementType.IDBased, null),
                    Quantity = new Quantity("outputQuant2" + outputExchangeItemIndex)
                };
            }
            throw new Exception("Invalid input index");
        }

        public void AddLink(ILink link)
        {
            // no action needed
        }

        public void RemoveLink(string linkID)
        {
            throw new NotImplementedException();
        }

        public string Validate()
        {
            // no action needed
            return "";
        }

        public void Prepare()
        {
            // no action needed
        }

        public IValueSet GetValues(ITime time, string linkID)
        {
            timeStep++;
            double value;
            if (linkID.Equals("sinus"))
            {
                value = 10 + Math.Sin(timeStep);
            }
            else if (linkID.Equals("sinus2"))
            {
                value = 0 + 0.4 * Math.Sin(timeStep-.25d);
            }
            else if (linkID.Equals("linear"))
            {
                value = -80.0d - 10.0d*timeStep;
            }
            else
            {
                throw new Exception("Unknown link id " + linkID);
            }
            return new ScalarSet(new double[] { value });
        }

        public void Finish()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string ComponentID
        {
            get { return modelId; }
        }

        public string ComponentDescription
        {
            get { return modelId; }
        }

        public string ModelID
        {
            get { return modelId; }
        }

        public string ModelDescription
        {
            get { return modelId; }
        }

        public ITimeSpan TimeHorizon
        {
            get { throw new NotImplementedException(); }
        }

        public ITimeStamp EarliestInputTime
        {
            get { return new TimeStamp(0); }
        }
    }
}