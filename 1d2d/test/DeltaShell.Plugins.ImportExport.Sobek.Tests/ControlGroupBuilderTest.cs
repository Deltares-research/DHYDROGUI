using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class ControlGroupBuilderTest
    {
        private string controllerId = "controllerId";
        private string controllerName = "controllerName";
        private InterpolationType interpolation = InterpolationType.Constant;
        private ExtrapolationType extrapolation = ExtrapolationType.Periodic;
        private IDictionary<string, SobekController> dictionarySobekControllers;
        private IDictionary<string, SobekTrigger> dictionarySobekTriggers;

        private MockRepository mockRepository;
        private IModel modelMock;
        private string parameterName = "Crest level";
        private IList<IFeature> availableLocations;
        private List<IDataItem> availableDataItems;
        private RealTimeControlModel rtcModel;

        [SetUp]
        public void SetUp()
        {
            rtcModel = new RealTimeControlModel();
            dictionarySobekControllers = new Dictionary<string, SobekController>();
            dictionarySobekTriggers = new Dictionary<string, SobekTrigger>();
        }


        [Test]
        public void CreateControlGroupForStructureWithOneRule()
        {
            var structureID = "structureID";

            var timeController = new SobekController
            {
                ControllerType = SobekControllerType.TimeController,
                Id = controllerId,
                Name = controllerName,
                SobekControllerParameterType = SobekControllerParameter.CrestLevel
            };
            timeController.TimeTable = SobekController.TimeTableStructure;
            dictionarySobekControllers[controllerId] = timeController;

            var structureMapping = new SobekStructureMapping
                                       {
                                           Name = "structureName",
                                           StructureId = structureID,
                                           ControllerIDs = new List<string>()
                                       };
            structureMapping.ControllerIDs.Add(controllerId);

            //Set Mocks
            InitModelMocks();
            AddDataItemToMockModel(structureID, parameterName, QuantityType.CrestLevel, ElementSet.Structures, DataItemRole.Input);

            ControlGroupBuilder.CreateControlGroupForStructureAndAddToRtcModel(structureMapping,
                                                                                  new Weir(),
                                                                                  modelMock,
                                                                                  rtcModel,
                                                                                  dictionarySobekControllers,
                                                                                  dictionarySobekTriggers);

            var controlGroup = rtcModel.ControlGroups.First();
            Assert.AreEqual(1, controlGroup.Outputs.Count);
            Assert.AreEqual(1, controlGroup.Rules.Count);

            mockRepository.VerifyAll();

        }

        [Test]
        public void CreateControlGroupForStructureWithOneRuleAndOneCondition()
        {
            var structureID = "structureID";
            var trigger1 = "trigger1";

            var timeController = new SobekController
            {
                ControllerType = SobekControllerType.TimeController,
                Id = controllerId,
                Name = controllerName,
                SobekControllerParameterType = SobekControllerParameter.CrestLevel,
                Triggers = new List<Trigger>
                               {
                                   new Trigger{Id = trigger1, Active = true, And = true}
                               }
            };
            timeController.TimeTable = SobekController.TimeTableStructure;
            dictionarySobekControllers[controllerId] = timeController;

            var structureMapping = new SobekStructureMapping
            {
                Name = "structureName",
                StructureId = structureID,
                ControllerIDs = new List<string>()
            };
            structureMapping.ControllerIDs.Add(controllerId);

            var triggerTable = SobekTrigger.TriggerTableStructure;
            var row = triggerTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 0;
            row[2] = 0;
            row[3] = 1;
            row[4] = 11.1d;
            triggerTable.Rows.Add(row);


            //add triggers
            dictionarySobekTriggers[trigger1] = new SobekTrigger
            {
                Id = trigger1,
                TriggerType = SobekTriggerType.Hydraulic,
                TriggerTable = triggerTable,
                MeasurementStationId = structureID
            };

            //Set Mocks
            InitModelMocks();
            AddDataItemToMockModel(structureID, parameterName,
                QuantityType.CrestLevel, ElementSet.Structures, DataItemRole.Input);
            AddDataItemToMockModel(structureID, "",
                QuantityType.WaterLevel, ElementSet.Observations, DataItemRole.Output);

            ControlGroupBuilder.CreateControlGroupForStructureAndAddToRtcModel(structureMapping, new Weir(),
                                                                                  modelMock,
                                                                                  rtcModel,
                                                                                  dictionarySobekControllers,
                                                                                  dictionarySobekTriggers);

            var controlGroup = rtcModel.ControlGroups.First(); 
            Assert.AreEqual(1, controlGroup.Outputs.Count);
            Assert.AreEqual(1, controlGroup.Rules.Count);
            Assert.AreEqual(2, controlGroup.Conditions.Count); //time condition and standard condition
            Assert.AreEqual(1, controlGroup.Inputs.Count);
            mockRepository.VerifyAll();

        }

        [Test]
        public void CreateControlGroupOfStructure24OfNDBModel()
        {
           // CNTL id '69' nm 'HY_OPEN_CONTR' ta 1 1 0 0 gi '5' '20771' '-1' '-1' ao 1 0 1 1 ct 0 ac 1 ca 2 cf 1 cb '-1' '-1' '-1' '-1' '-1' cl 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 ti tv 'Time Controller' PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE 
           // '1991/01/01;00:00:00' 9 < 
           // tble
           // mp 500000 mc 0.005 sp tc 0 9.9999e+009 9.9999e+009 ui 9.9999e+009 ua 9.9999e+009 u0 9.9999e+009 pf 9.9999e+009 if 9.9999e+009 df 9.9999e+009 va 9.9999e+009 si '-1' hc ht 5 9.9999e+009 9.9999e+009 bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl

            var structureID = "structureID";
            var trigger1 = "5";
            var trigger2 = "20771";

            //add controller
            var timeController = new SobekController
            {
                ControllerType = SobekControllerType.TimeController,
                Id = controllerId,
                Name = controllerName,
                SobekControllerParameterType = SobekControllerParameter.CrestLevel,
                Triggers = new List<Trigger>
                               {
                                   new Trigger{Id = trigger1, Active = true, And = true},
                                   new Trigger{Id = trigger2, Active = true, And = false}
                               }
            };
            timeController.TimeTable = SobekController.TimeTableStructure;
            dictionarySobekControllers[controllerId] = timeController;

            var triggerTable = SobekTrigger.TriggerTableStructure;
            var row = triggerTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 0;
            row[2] = 0;
            row[3] = 1;
            row[4] = 11.1d;
            triggerTable.Rows.Add(row);


            //add triggers
            dictionarySobekTriggers[trigger1] = new SobekTrigger
            {
                Id = trigger1,
                TriggerType = SobekTriggerType.Hydraulic,
                TriggerTable = triggerTable
            };

            dictionarySobekTriggers[trigger2] = new SobekTrigger
            {
                Id = trigger2,
                TriggerType = SobekTriggerType.Hydraulic,
                TriggerTable = triggerTable
            };

            var structureMapping = new SobekStructureMapping
            {
                Name = "structureName",
                StructureId = structureID,
                ControllerIDs = new List<string>()
            };
            structureMapping.ControllerIDs.Add(controllerId);

            //Set Mocks
            InitModelMocks();
            AddDataItemToMockModel(structureID, parameterName, QuantityType.CrestLevel, ElementSet.Structures, DataItemRole.Input);

            ControlGroupBuilder.CreateControlGroupForStructureAndAddToRtcModel(structureMapping, new Weir(),
                                                                                  modelMock,
                                                                                  rtcModel,
                                                                                  dictionarySobekControllers,
                                                                                  dictionarySobekTriggers);

            var controlGroup = rtcModel.ControlGroups.First(); 
            Assert.AreEqual(1, controlGroup.Outputs.Count);
            Assert.AreEqual(1, controlGroup.Rules.Count);
            Assert.AreEqual(4, controlGroup.Conditions.Count);

            var condition1Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger1);
            var condition1Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger1 + "_1");
            var condition2Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2);
            var condition2Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2 + "_1");
            Assert.IsNotNull(condition1Time);
            Assert.IsNotNull(condition1Hydraulic);
            Assert.IsNotNull(condition2Time);
            Assert.IsNotNull(condition2Hydraulic);
            Assert.AreSame(condition1Hydraulic, condition1Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition2Time, condition1Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition2Hydraulic, condition2Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(controlGroup.Rules.FirstOrDefault(), condition2Hydraulic.TrueOutputs.FirstOrDefault());

            Assert.AreEqual(0, condition1Time.FalseOutputs.Count());
            Assert.AreEqual(0, condition1Hydraulic.FalseOutputs.Count());
            Assert.AreEqual(0, condition2Time.FalseOutputs.Count());
            Assert.AreEqual(0, condition2Hydraulic.FalseOutputs.Count());

            mockRepository.VerifyAll();

        }

       [Test]
       public void CreateControlGroupOfStructureWithTwoControllersForTheSameOutputParameter()
       {

           var structureID = "structureID";
           var controller1 = "controller1";
           var controller2 = "controller2";
           var trigger1 = "trigger1";
           var trigger2 = "trigger2";
           var trigger3 = "trigger3";

           //add controller
           var timeController1 = new SobekController
           {
               ControllerType = SobekControllerType.TimeController,
               Id = controller1,
               Name = controller1,
               SobekControllerParameterType = SobekControllerParameter.CrestLevel,
               Triggers = new List<Trigger>
                               {
                                   new Trigger{Id = trigger1, Active = true, And = true},
                                   new Trigger{Id = trigger2, Active = true, And = true}
                               }
           };
           timeController1.TimeTable = SobekController.TimeTableStructure;
           dictionarySobekControllers[controller1] = timeController1;

           var timeController2 = new SobekController
           {
               ControllerType = SobekControllerType.TimeController,
               Id = controller2,
               Name = controller2,
               SobekControllerParameterType = SobekControllerParameter.CrestLevel,
               Triggers = new List<Trigger>
                               {
                                   new Trigger{Id = trigger3, Active = true, And = false}
                               }
           };
           timeController2.TimeTable = SobekController.TimeTableStructure;
           dictionarySobekControllers[controller2] = timeController2;

           var triggerTable = SobekTrigger.TriggerTableStructure;
           var row = triggerTable.NewRow();
           row[0] = DateTime.Now;
           row[1] = 0;
           row[2] = 0;
           row[3] = 1;
           row[4] = 11.1d;
           triggerTable.Rows.Add(row);


           //add triggers
           dictionarySobekTriggers[trigger1] = new SobekTrigger
           {
               Id = trigger1,
               TriggerType = SobekTriggerType.Hydraulic,
               TriggerTable = triggerTable
           };

           dictionarySobekTriggers[trigger2] = new SobekTrigger
           {
               Id = trigger2,
               TriggerType = SobekTriggerType.Hydraulic,
               TriggerTable = triggerTable
           };

           dictionarySobekTriggers[trigger3] = new SobekTrigger
           {
               Id = trigger3,
               TriggerType = SobekTriggerType.Hydraulic,
               TriggerTable = triggerTable
           };

           var structureMapping = new SobekStructureMapping
           {
               Name = "structureName",
               StructureId = structureID,
               ControllerIDs = new List<string>()
           };
           structureMapping.ControllerIDs.Add(controller1);
           structureMapping.ControllerIDs.Add(controller2);

           //Set Mocks
           InitModelMocks();
           AddDataItemToMockModel(structureID, parameterName, QuantityType.CrestLevel, ElementSet.Structures, DataItemRole.Input);

           ControlGroupBuilder.CreateControlGroupForStructureAndAddToRtcModel(structureMapping, new Weir(),
                                                                                 modelMock,
                                                                                 rtcModel,
                                                                                 dictionarySobekControllers,
                                                                                 dictionarySobekTriggers);

           var controlGroup = rtcModel.ControlGroups.First(); 
           Assert.AreEqual(1, controlGroup.Outputs.Count);
           Assert.AreEqual(2, controlGroup.Rules.Count);
           Assert.AreEqual(6, controlGroup.Conditions.Count);

           var condition1Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger1);
           var condition1Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger1 + "_1");
           var condition2Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2);
           var condition2Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2 + "_1");
           var condition3Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger3);
           var condition3Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger3 + "_1");

           Assert.IsNotNull(condition1Time);
           Assert.IsNotNull(condition1Hydraulic);
           Assert.IsNotNull(condition2Time);
           Assert.IsNotNull(condition2Hydraulic);
           Assert.IsNotNull(condition3Time);
           Assert.IsNotNull(condition3Hydraulic);

           Assert.AreSame(condition1Hydraulic, condition1Time.TrueOutputs.FirstOrDefault());
           Assert.AreSame(condition2Time, condition1Hydraulic.TrueOutputs.FirstOrDefault());
           Assert.AreSame(condition2Hydraulic, condition2Time.TrueOutputs.FirstOrDefault());
           Assert.AreSame(controlGroup.Rules.First(), condition2Hydraulic.TrueOutputs.FirstOrDefault());
           Assert.AreSame(condition3Hydraulic, condition3Time.TrueOutputs.FirstOrDefault());
           Assert.AreSame(controlGroup.Rules.Last(), condition3Hydraulic.TrueOutputs.FirstOrDefault());

           Assert.AreSame(condition3Time, condition1Time.FalseOutputs.FirstOrDefault());
           Assert.AreSame(condition3Time, condition1Hydraulic.FalseOutputs.FirstOrDefault());
           Assert.AreSame(condition3Time, condition2Time.FalseOutputs.FirstOrDefault());
           Assert.AreSame(condition3Time, condition2Hydraulic.FalseOutputs.FirstOrDefault());

           mockRepository.VerifyAll();
           
       }

        [Test]
        public void CreateControlGroupWithConditionsWithAndRelation()
        {
            var structureID = "structureID";
            var trigger1 = "trigger1";
            var trigger2 = "trigger2";

            //add controller
            var timeController = new SobekController
            {
                ControllerType = SobekControllerType.TimeController,
                Id = controllerId,
                Name = controllerName,
                SobekControllerParameterType = SobekControllerParameter.CrestLevel,
                Triggers = new List<Trigger>
                               {
                                   new Trigger{Id = trigger1, Active = true, And = true},
                                   new Trigger{Id = trigger2, Active = true, And = true}
                               }
            };
            timeController.TimeTable = SobekController.TimeTableStructure;
            dictionarySobekControllers[controllerId] = timeController;

            var triggerTable = SobekTrigger.TriggerTableStructure;
            var row = triggerTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 0;
            row[2] = 0;
            row[3] = 1;
            row[4] = 11.1d;
            triggerTable.Rows.Add(row);


            //add triggers
            dictionarySobekTriggers[trigger1] = new SobekTrigger
                                                    {
                                                        Id = trigger1,
                                                        TriggerType = SobekTriggerType.Hydraulic,
                                                        TriggerTable = triggerTable
                                                    };

            dictionarySobekTriggers[trigger2] = new SobekTrigger
                                                    {
                                                        Id = trigger2,
                                                        TriggerType = SobekTriggerType.Hydraulic,
                                                        TriggerTable = triggerTable
                                                    };

            var structureMapping = new SobekStructureMapping
            {
                Name = "structureName",
                StructureId = structureID,
                ControllerIDs = new List<string>()
            };
            structureMapping.ControllerIDs.Add(controllerId);

            //Set Mocks
            InitModelMocks();
            AddDataItemToMockModel(structureID, parameterName, QuantityType.CrestLevel, ElementSet.Structures, DataItemRole.Input);

            ControlGroupBuilder.CreateControlGroupForStructureAndAddToRtcModel(structureMapping, new Weir(),
                                                                                  modelMock,
                                                                                  rtcModel,
                                                                                  dictionarySobekControllers,
                                                                                  dictionarySobekTriggers);

            var controlGroup = rtcModel.ControlGroups.First();
            Assert.AreEqual(1, controlGroup.Outputs.Count);
            Assert.AreEqual(1, controlGroup.Rules.Count);
            Assert.AreEqual(4, controlGroup.Conditions.Count);

            var condition1Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger1);
            var condition1Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger1 + "_1");
            var condition2Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2);
            var condition2Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2 + "_1");
            Assert.IsNotNull(condition1Time);
            Assert.IsNotNull(condition1Hydraulic);
            Assert.IsNotNull(condition2Time);
            Assert.IsNotNull(condition2Hydraulic);
            Assert.AreSame(condition1Hydraulic, condition1Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition2Time, condition1Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreSame(controlGroup.Rules.FirstOrDefault(), condition2Hydraulic.TrueOutputs.FirstOrDefault());

            mockRepository.VerifyAll();

        }


        [Test]
        public void CreateControlGroupWithTwoTimeConditionsAndOneHydraulicConditionWithAndRelations()
        {
            var structureID = "structureID";
            var trigger1 = "trigger1";
            var trigger2 = "trigger2";
            var trigger3 = "trigger3";

            //add controller
            var timeController = new SobekController
            {
                ControllerType = SobekControllerType.TimeController,
                Id = controllerId,
                Name = controllerName,
                SobekControllerParameterType = SobekControllerParameter.CrestLevel,
                Triggers = new List<Trigger>
                               {
                                   new Trigger{Id = trigger1, Active = true, And = true},
                                   new Trigger{Id = trigger2, Active = true, And = true},
                                   new Trigger{Id = trigger3, Active = true, And = true}
                               }
            };
            timeController.TimeTable = SobekController.TimeTableStructure;
            dictionarySobekControllers[controllerId] = timeController;

            var triggerTableOneRow = SobekTrigger.TriggerTableStructure;
            var row = triggerTableOneRow.NewRow();
            row[0] = DateTime.Now;
            row[1] = 0;
            row[2] = 0;
            row[3] = 1;
            row[4] = 11.1d;
            triggerTableOneRow.Rows.Add(row);


            var triggerTable = SobekTrigger.TriggerTableStructure;
            row = triggerTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 0;
            row[2] = 0;
            row[3] = 1;
            row[4] = 11.1d;
            triggerTable.Rows.Add(row);
            row = triggerTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 0;
            row[2] = 0;
            row[3] = 0;
            row[4] = 22.2d;
            triggerTable.Rows.Add(row);
            row = triggerTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 0;
            row[2] = 0;
            row[3] = 0;
            row[4] = 33.3d;
            triggerTable.Rows.Add(row);


            //add triggers
            dictionarySobekTriggers[trigger1] = new SobekTrigger
            {
                Id = trigger1,
                TriggerType = SobekTriggerType.Time,
                TriggerTable = triggerTableOneRow
            };

            dictionarySobekTriggers[trigger2] = new SobekTrigger
            {
                Id = trigger2,
                TriggerType = SobekTriggerType.Hydraulic,
                TriggerTable = triggerTable
            };

            dictionarySobekTriggers[trigger3] = new SobekTrigger
            {
                Id = trigger3,
                TriggerType = SobekTriggerType.Time,
                TriggerTable = triggerTableOneRow
            };

            var structureMapping = new SobekStructureMapping
            {
                Name = "structureName",
                StructureId = structureID,
                ControllerIDs = new List<string>()
            };
            structureMapping.ControllerIDs.Add(controllerId);

            //Set Mocks
            InitModelMocks();
            AddDataItemToMockModel(structureID, parameterName, QuantityType.CrestLevel, ElementSet.Structures, DataItemRole.Input);

            ControlGroupBuilder.CreateControlGroupForStructureAndAddToRtcModel(structureMapping, new Weir(),
                                                                                  modelMock,
                                                                                  rtcModel,
                                                                                  dictionarySobekControllers,
                                                                                  dictionarySobekTriggers);

            var controlGroup = rtcModel.ControlGroups.First(); 
            Assert.AreEqual(1, controlGroup.Outputs.Count);
            Assert.AreEqual(1, controlGroup.Rules.Count);
            Assert.AreEqual(8, controlGroup.Conditions.Count);

            var firstCondition = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger1);

            var condition1Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2);
            var condition1Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2 + "_1");
            var condition2Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2 + "_2");
            var condition2Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2 + "_3");
            var condition3Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2 + "_4");
            var condition3Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2 + "_5");

            var lastCondition = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger3);

            Assert.IsNotNull(firstCondition);
            Assert.IsNotNull(condition1Time);
            Assert.IsNotNull(condition1Hydraulic);
            Assert.IsNotNull(condition2Time);
            Assert.IsNotNull(condition2Hydraulic);
            Assert.IsNotNull(condition2Time);
            Assert.IsNotNull(condition3Hydraulic);
            Assert.IsNotNull(condition3Time);
            Assert.IsNotNull(lastCondition);

            Assert.AreSame(condition1Time, firstCondition.TrueOutputs.FirstOrDefault());

            Assert.AreSame(condition1Hydraulic, condition1Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition2Time, condition1Time.FalseOutputs.FirstOrDefault());
            Assert.AreSame(lastCondition, condition1Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreSame(null, condition1Hydraulic.FalseOutputs.FirstOrDefault());

            Assert.AreSame(condition2Hydraulic, condition2Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition3Time, condition2Time.FalseOutputs.FirstOrDefault());
            Assert.AreSame(lastCondition, condition2Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreSame(null, condition2Hydraulic.FalseOutputs.FirstOrDefault());

            Assert.AreSame(condition3Hydraulic, condition3Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(null, condition3Time.FalseOutputs.FirstOrDefault());
            Assert.AreSame(lastCondition, condition3Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreSame(null, condition3Hydraulic.FalseOutputs.FirstOrDefault());

            Assert.AreSame(controlGroup.Rules.FirstOrDefault(), lastCondition.TrueOutputs.FirstOrDefault());

            mockRepository.VerifyAll();

        }


        [Test]
        public void CreateControlGroupWithConditions_C1AndC2_Or_C3AndC4()
        {
            var structureID = "structureID";
            var trigger1 = "trigger1";
            var trigger2 = "trigger2";
            var trigger3 = "trigger3";
            var trigger4 = "trigger4";

            //add controller
            var timeController = new SobekController
            {
                ControllerType = SobekControllerType.TimeController,
                Id = controllerId,
                Name = controllerName,
                Triggers = new List<Trigger>
                               {
                                   new Trigger{Id = trigger1, Active = true, And = true},
                                   new Trigger{Id = trigger2, Active = true, And = false},
                                   new Trigger{Id = trigger3, Active = true, And = true},
                                   new Trigger{Id = trigger4, Active = true, And = true}
                               }
            };
            timeController.TimeTable = SobekController.TimeTableStructure;
            dictionarySobekControllers[controllerId] = timeController;

            var triggerTable = SobekTrigger.TriggerTableStructure;
            var row = triggerTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 0;
            row[2] = 0;
            row[3] = 1;
            row[4] = 11.1d;
            triggerTable.Rows.Add(row);


            //add triggers
            dictionarySobekTriggers[trigger1] = new SobekTrigger
            {
                Id = trigger1,
                TriggerType = SobekTriggerType.Hydraulic,
                TriggerTable = triggerTable
            };

            dictionarySobekTriggers[trigger2] = new SobekTrigger
            {
                Id = trigger2,
                TriggerType = SobekTriggerType.Hydraulic,
                TriggerTable = triggerTable
            };

            dictionarySobekTriggers[trigger3] = new SobekTrigger
            {
                Id = trigger3,
                TriggerType = SobekTriggerType.Hydraulic,
                TriggerTable = triggerTable
            };

            dictionarySobekTriggers[trigger4] = new SobekTrigger
            {
                Id = trigger4,
                TriggerType = SobekTriggerType.Hydraulic,
                TriggerTable = triggerTable
            };

            var structureMapping = new SobekStructureMapping
            {
                Name = "structureName",
                StructureId = structureID,
                ControllerIDs = new List<string>()
            };
            structureMapping.ControllerIDs.Add(controllerId);

            //Set Mocks
            InitModelMocks();
            AddDataItemToMockModel(structureID, parameterName, QuantityType.CrestLevel, ElementSet.Structures, DataItemRole.Input);

            ControlGroupBuilder.CreateControlGroupForStructureAndAddToRtcModel(structureMapping, new Weir(),
                                                                                  modelMock,
                                                                                  rtcModel,
                                                                                  dictionarySobekControllers,
                                                                                  dictionarySobekTriggers);

            var controlGroup = rtcModel.ControlGroups.First(); 
            Assert.AreEqual(1, controlGroup.Outputs.Count);
            Assert.AreEqual(1, controlGroup.Rules.Count);
            Assert.AreEqual(8, controlGroup.Conditions.Count);

            var condition1Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger1);
            var condition1Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger1 + "_1");
            var condition2Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2);
            var condition2Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger2 + "_1");
            var condition3Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger3);
            var condition3Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger3 + "_1");
            var condition4Time = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger4);
            var condition4Hydraulic = controlGroup.Conditions.FirstOrDefault(c => c.Name == trigger4 + "_1");
            var rule = controlGroup.Rules.FirstOrDefault();

            Assert.IsNotNull(condition1Time);
            Assert.IsNotNull(condition1Hydraulic);
            Assert.IsNotNull(condition2Time);
            Assert.IsNotNull(condition2Hydraulic);
            Assert.IsNotNull(condition3Time);
            Assert.IsNotNull(condition3Hydraulic);
            Assert.IsNotNull(condition4Time);
            Assert.IsNotNull(condition4Hydraulic);

            //and
            Assert.AreSame(condition1Hydraulic, condition1Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition2Time, condition1Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition2Hydraulic, condition2Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(rule, condition2Hydraulic.TrueOutputs.FirstOrDefault());

            //or
            Assert.AreSame(condition1Hydraulic, condition1Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition2Time, condition1Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition2Hydraulic, condition2Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(rule, condition2Hydraulic.TrueOutputs.FirstOrDefault());

            //and
            Assert.AreSame(condition3Hydraulic, condition3Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition4Time, condition3Hydraulic.TrueOutputs.FirstOrDefault());
            Assert.AreSame(condition4Hydraulic, condition4Time.TrueOutputs.FirstOrDefault());
            Assert.AreSame(rule, condition4Hydraulic.TrueOutputs.FirstOrDefault());

            mockRepository.VerifyAll();

        }

        [Test]
        public void ObservationPointParameterMappingNamesWaterFlowModel1D()
        {
            var waterFlowFmModel = new WaterFlowFMModel();

            // Assert.AreEqual(waterFlowFmModel.OutputSettings.EngineParameters.Where(ep => ep.QuantityType == QuantityType.Discharge).FirstOrDefault().QuantityType,
            //     ControlGroupBuilder.GetWaterFlowModelQuantityType(SobekMeasurementLocationParameter.Discharge));
            // Assert.AreEqual(waterFlowFmModel.OutputSettings.EngineParameters.Where(ep => ep.QuantityType == QuantityType.WaterLevel).FirstOrDefault().QuantityType,
            //     ControlGroupBuilder.GetWaterFlowModelQuantityType(SobekMeasurementLocationParameter.WaterLevel));
        }

        [Test]
        public void ConvertHydraulicTrigger()
        {
            var triggerId = "triggerId";
            var triggerName = "triggerName";

            var hydraulicTrigger = new SobekTrigger
            {
                Id = triggerId,
                Name = triggerName,
                TriggerType = SobekTriggerType.Hydraulic
            };
            hydraulicTrigger.TriggerTable = SobekTrigger.TriggerTableStructure;

            var row = hydraulicTrigger.TriggerTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 0;
            row[2] = 0;
            row[3] = 1;
            row[4] = 11.1d;
            hydraulicTrigger.TriggerTable.Rows.Add(row);

            //first one is a time condition, the second a hydraulic rule
            var condition = ControlGroupBuilder.GetConditions(hydraulicTrigger).LastOrDefault() as StandardCondition;

            Assert.IsNotNull(condition);
            Assert.AreEqual(triggerId, condition.Name);
            Assert.AreEqual(triggerName, condition.LongName);
            Assert.AreEqual(Operation.Greater, condition.Operation);
            Assert.AreEqual(11.1d, condition.Value);

        }

        [Test]
        public void ConvertTimeController()
        {
            var timeController = new SobekController
                                     {
                                         ControllerType = SobekControllerType.TimeController,
                                         Id = controllerId,
                                         Name = controllerName,
                                         InterpolationType = interpolation,
                                         ExtrapolationType = extrapolation,
                                         MeasurementStationId = "MeasurementStationId"
                                     };
            timeController.TimeTable = SobekController.TimeTableStructure;

            var row = timeController.TimeTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 100;
            timeController.TimeTable.Rows.Add(row);
            row = timeController.TimeTable.NewRow();
            row[0] = DateTime.Now + new TimeSpan(1,0,0);
            row[1] = 200;
            timeController.TimeTable.Rows.Add(row);

            var rule = ControlGroupBuilder.GetRule(timeController) as TimeRule;

            Assert.IsNotNull(rule);
            Assert.AreEqual(controllerId,rule.Name);
            Assert.AreEqual(controllerName,rule.LongName);
            Assert.AreEqual(interpolation,rule.InterpolationOptionsTime);
            Assert.AreEqual(extrapolation,rule.Periodicity);
            Assert.AreEqual(2,rule.TimeSeries.Time.Values.Count);
            Assert.AreEqual(0, rule.Inputs.Count,"Time controller can not have an input item");

        }

        [Test]
        public void ConvertHydraulicController()
        {
            var hydraulicController = new SobekController
            {
                ControllerType = SobekControllerType.HydraulicController,
                Id = controllerId,
                Name = controllerName,
                InterpolationType = interpolation,
                ExtrapolationType = extrapolation
            };
            hydraulicController.SpecificProperties = new SobekHydraulicControllerProperties { TimeLag = 123 };
            hydraulicController.LookUpTable = SobekController.LookUpTableStructure;

            var row = hydraulicController.LookUpTable.NewRow();
            row[0] = 1;
            row[1] = 100;
            hydraulicController.LookUpTable.Rows.Add(row);
            row = hydraulicController.LookUpTable.NewRow();
            row[0] = 2;
            row[1] = 200;
            hydraulicController.LookUpTable.Rows.Add(row);

            var rule = ControlGroupBuilder.GetRule(hydraulicController) as HydraulicRule;

            Assert.IsNotNull(rule);
            Assert.AreEqual(controllerId, rule.Name);
            Assert.AreEqual(controllerName, rule.LongName);
            Assert.AreEqual(123, rule.TimeLag);
            Assert.AreEqual(interpolation, rule.Interpolation);
            Assert.AreEqual(2, rule.Function.Arguments[0].Values.Count);

        }

        [Test]
        public void ConvertRelativeTimeController()
        {
            var realiveTimeController = new SobekController
            {
                ControllerType = SobekControllerType.RelativeTimeController,
                Id = controllerId,
                Name = controllerName,
                InterpolationType = interpolation,
                ExtrapolationType = extrapolation
            };
            realiveTimeController.LookUpTable = SobekController.LookUpTableStructure;

            var row = realiveTimeController.LookUpTable.NewRow();
            row[0] = 1;
            row[1] = 100;
            realiveTimeController.LookUpTable.Rows.Add(row);
            row = realiveTimeController.LookUpTable.NewRow();
            row[0] = 2;
            row[1] = 200;
            realiveTimeController.LookUpTable.Rows.Add(row);

            var rule = ControlGroupBuilder.GetRule(realiveTimeController) as RelativeTimeRule;

            Assert.IsNotNull(rule);
            Assert.AreEqual(controllerId, rule.Name);
            Assert.AreEqual(controllerName, rule.LongName);
            Assert.AreEqual(interpolation, rule.Interpolation);
            Assert.AreEqual(2, rule.Function.Arguments[0].Values.Count);

        }

        [Test]
        public void ConvertRelativeTimeFromValueController()
        {
            var realiveTimeController = new SobekController
            {
                ControllerType = SobekControllerType.RelativeFromValueController,
                Id = controllerId,
                Name = controllerName,
                InterpolationType = interpolation,
                ExtrapolationType = extrapolation
            };
            realiveTimeController.LookUpTable = SobekController.LookUpTableStructure;

            var row = realiveTimeController.LookUpTable.NewRow();
            row[0] = 1;
            row[1] = 100;
            realiveTimeController.LookUpTable.Rows.Add(row);
            row = realiveTimeController.LookUpTable.NewRow();
            row[0] = 2;
            row[1] = 200;
            realiveTimeController.LookUpTable.Rows.Add(row);

            var rule = ControlGroupBuilder.GetRule(realiveTimeController) as RelativeTimeRule; //RelativeFromValueRule not available yet

            Assert.IsNotNull(rule);
            Assert.AreEqual(controllerId, rule.Name);
            Assert.AreEqual(controllerName, rule.LongName);
            Assert.AreEqual(interpolation, rule.Interpolation);
            Assert.AreEqual(2, rule.Function.Arguments[0].Values.Count);

        }

        [Test]
        public void ConvertPIDControllerWithTimeSeries()
        {
            var pidController = new SobekController
            {
                ControllerType = SobekControllerType.PIDController,
                Id = controllerId,
                Name = controllerName,
                InterpolationType = interpolation,
                ExtrapolationType = extrapolation
            };
            pidController.TimeTable = SobekController.TimeTableStructure;

            var row = pidController.TimeTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 100;
            pidController.TimeTable.Rows.Add(row);
            row = pidController.TimeTable.NewRow();
            row[0] = DateTime.Now + new TimeSpan(1, 0, 0);
            row[1] = 200;
            pidController.TimeTable.Rows.Add(row);

            var specificProperties = new SobekPidControllerProperties();
            pidController.SpecificProperties = specificProperties;

            specificProperties.KFactorDifferential = 1.0;
            specificProperties.KFactorIntegral = 2.0;
            specificProperties.KFactorProportional = 3.0;

            var rule = ControlGroupBuilder.GetRule(pidController) as PIDRule;

            Assert.IsNotNull(rule);
            Assert.AreEqual(controllerId, rule.Name);
            Assert.AreEqual(controllerName, rule.LongName);
            Assert.AreEqual(2, rule.TimeSeries.Time.Values.Count);

            Assert.AreEqual(1.0, rule.Kd);
            Assert.AreEqual(2.0,rule.Ki);
            Assert.AreEqual(3.0,rule.Kp);

        }

        [Test]
        public void ConvertPIDControllerWithConstantValue() //No conversion anymore
        {
            var pidController = new SobekController
            {
                ControllerType = SobekControllerType.PIDController,
                Id = controllerId,
                Name = controllerName,
                InterpolationType = interpolation,
                ExtrapolationType = extrapolation
            };
            pidController.TimeTable = SobekController.TimeTableStructure;

            var row = pidController.TimeTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 100;
            pidController.TimeTable.Rows.Add(row);
            row = pidController.TimeTable.NewRow();
            row[0] = DateTime.Now + new TimeSpan(1, 0, 0);
            row[1] = 200;
            pidController.TimeTable.Rows.Add(row);

            var specificProperties = new SobekPidControllerProperties();
            pidController.SpecificProperties = specificProperties;

            specificProperties.KFactorDifferential = 1.0;
            specificProperties.KFactorIntegral = 2.0;
            specificProperties.KFactorProportional = 3.0;
            specificProperties.ConstantSetPoint = 4.0;

            specificProperties.FromSobekType = SobekType.Unknown;

            var rule = ControlGroupBuilder.GetRule(pidController) as PIDRule;

            Assert.IsNotNull(rule);
            Assert.AreEqual(controllerId, rule.Name);
            Assert.AreEqual(controllerName, rule.LongName);

            Assert.AreEqual(1.0, rule.Kd);
            Assert.AreEqual(2.0, rule.Ki);
            Assert.AreEqual(3.0, rule.Kp);
            Assert.IsTrue(rule.PidRuleSetpointType == PIDRule.PIDRuleSetpointType.Constant);
            Assert.AreEqual(4.0, rule.ConstantValue);

        }

        [Test]
        public void ConvertPIDControllerFromRE() //No conversion anymore
        {
            var timeStepModel = new TimeSpan(0, 0, 30);

            var pidController = new SobekController
            {
                ControllerType = SobekControllerType.PIDController,
                Id = controllerId,
                Name = controllerName,
                InterpolationType = interpolation,
                ExtrapolationType = extrapolation
            };
            pidController.TimeTable = SobekController.TimeTableStructure;

            var specificProperties = new SobekPidControllerProperties();
            pidController.SpecificProperties = specificProperties;

            specificProperties.KFactorDifferential = 1.0;
            specificProperties.KFactorIntegral = 2.0;
            specificProperties.KFactorProportional = 3.0;
            specificProperties.ConstantSetPoint = 4.0;

            specificProperties.FromSobekType = SobekType.SobekRE;
            specificProperties.TimeStepModel = timeStepModel;


            var rule = ControlGroupBuilder.GetRule(pidController) as PIDRule;

            Assert.IsNotNull(rule);
            Assert.AreEqual(controllerId, rule.Name);
            Assert.AreEqual(controllerName, rule.LongName);

            Assert.AreEqual(1.0, rule.Kd);
            Assert.AreEqual(2.0, rule.Ki);
            Assert.AreEqual(3.0, rule.Kp);
            Assert.IsTrue(rule.PidRuleSetpointType == PIDRule.PIDRuleSetpointType.Constant);
            Assert.AreEqual(4.0, rule.ConstantValue);

        }

 
        [Test]
        public void ConvertPIDControllerFrom212()
        {
            var timeStepModel =  new TimeSpan(0, 0, 40);

            var pidController = new SobekController
            {
                ControllerType = SobekControllerType.PIDController,
                Id = controllerId,
                Name = controllerName,
                InterpolationType = interpolation,
                ExtrapolationType = extrapolation
            };
            pidController.TimeTable = SobekController.TimeTableStructure;

            var row = pidController.TimeTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 100;
            pidController.TimeTable.Rows.Add(row);
            row = pidController.TimeTable.NewRow();
            row[0] = DateTime.Now + new TimeSpan(1, 0, 0);
            row[1] = 200;
            pidController.TimeTable.Rows.Add(row);

            var specificProperties = new SobekPidControllerProperties();
            pidController.SpecificProperties = specificProperties;

            specificProperties.KFactorDifferential = 1.0;
            specificProperties.KFactorIntegral = 2.0;
            specificProperties.KFactorProportional = 3.0;
            specificProperties.ConstantSetPoint = 4.0;

            specificProperties.FromSobekType = SobekType.Sobek212;
            specificProperties.TimeStepModel = timeStepModel;

            var rule = ControlGroupBuilder.GetRule(pidController) as PIDRule;

            Assert.IsNotNull(rule);
            Assert.AreEqual(controllerId, rule.Name);
            Assert.AreEqual(controllerName, rule.LongName);

            Assert.AreEqual(1.0, rule.Kd);
            Assert.AreEqual(2.0, rule.Ki);
            Assert.AreEqual(3.0, rule.Kp);
            Assert.IsTrue(rule.PidRuleSetpointType == PIDRule.PIDRuleSetpointType.Constant);
            Assert.AreEqual(4.0, rule.ConstantValue);

        }

        [Test]
        public void ConvertIntervalControllerFixed()
        {
            var intervalController = new SobekController
            {
                ControllerType = SobekControllerType.IntervalController,
                Id = controllerId,
                Name = controllerName,
                InterpolationType = interpolation,
                ExtrapolationType = extrapolation
            };
            intervalController.TimeTable = SobekController.TimeTableStructure;

            var row = intervalController.TimeTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 100;
            intervalController.TimeTable.Rows.Add(row);
            row = intervalController.TimeTable.NewRow();
            row[0] = DateTime.Now + new TimeSpan(1, 0, 0);
            row[1] = 200;
            intervalController.TimeTable.Rows.Add(row);

            var specificProperties = new SobekIntervalControllerProperties();
            intervalController.SpecificProperties = specificProperties;

            specificProperties.DeadBandType = IntervalControllerDeadBandType.Fixed;
            specificProperties.DeadBandMin = 1.0;
            specificProperties.DeadBandMax = 2.0;
            specificProperties.ControlVelocity = 3.0;
            specificProperties.USminimum = 4.0;
            specificProperties.USmaximum = 5.0;
            specificProperties.DeadBandFixedSize = 6.0;
 
            var rule = ControlGroupBuilder.GetRule(intervalController) as IntervalRule;

            Assert.IsNotNull(rule);
            Assert.AreEqual(controllerId, rule.Name);
            Assert.AreEqual(controllerName, rule.LongName);
            Assert.AreEqual(interpolation, rule.InterpolationOptionsTime);
            Assert.AreEqual(2, rule.TimeSeries.Time.Values.Count);

            Assert.AreEqual(1.0, rule.Setting.Min);
            Assert.AreEqual(2.0, rule.Setting.Max);
            Assert.AreEqual(3.0, rule.Setting.MaxSpeed);
            Assert.AreEqual(4.0, rule.Setting.Below);
            Assert.AreEqual(5.0, rule.Setting.Above);
            Assert.AreEqual(6.0, rule.DeadbandAroundSetpoint);

        }

        [Test]
        public void ConvertIntervalControllerPercentage()
        {
            var intervalController = new SobekController
            {
                ControllerType = SobekControllerType.IntervalController,
                Id = controllerId,
                Name = controllerName,
                InterpolationType = interpolation,
                ExtrapolationType = extrapolation
            };
            intervalController.TimeTable = SobekController.TimeTableStructure;

            var row = intervalController.TimeTable.NewRow();
            row[0] = DateTime.Now;
            row[1] = 100;
            intervalController.TimeTable.Rows.Add(row);
            row = intervalController.TimeTable.NewRow();
            row[0] = DateTime.Now + new TimeSpan(1, 0, 0);
            row[1] = 200;
            intervalController.TimeTable.Rows.Add(row);

            var specificProperties = new SobekIntervalControllerProperties();
            intervalController.SpecificProperties = specificProperties;


            specificProperties.DeadBandType = IntervalControllerDeadBandType.PercentageDischarge;
            specificProperties.DeadBandMin = 1.0;
            specificProperties.DeadBandMax = 2.0;
            specificProperties.ControlVelocity = 3.0;
            specificProperties.USminimum = 4.0;
            specificProperties.USmaximum = 5.0;
            specificProperties.DeadBandPecentage = 0.5;

            var rule = ControlGroupBuilder.GetRule(intervalController) as IntervalRule;

            Assert.IsNotNull(rule);
            Assert.AreEqual(controllerId, rule.Name);
            Assert.AreEqual(controllerName, rule.LongName);
            Assert.AreEqual(interpolation, rule.InterpolationOptionsTime);
            Assert.AreEqual(2, rule.TimeSeries.Time.Values.Count);

            Assert.AreEqual(3.0, rule.Setting.MaxSpeed);
            Assert.AreEqual(4.0, rule.Setting.Below);
            Assert.AreEqual(5.0, rule.Setting.Above);
            Assert.AreEqual(0.5, rule.DeadbandAroundSetpoint);

        }

        [Test]
        public void ConvertIntervalControllerToIntervalRuleWithPeriodicExtrapolation()
        {
            var intervalController = new SobekController
                                         {
                                             ControllerType = SobekControllerType.IntervalController,
                                             Id = controllerId,
                                             Name = controllerName,
                                             InterpolationType = interpolation,
                                             ExtrapolationType = ExtrapolationType.Periodic,
                                             ExtrapolationPeriod = "'7;00:00:00'"
                                         };
            intervalController.TimeTable = SobekController.TimeTableStructure;

            var now = DateTime.Now;

            var row = intervalController.TimeTable.NewRow();
            row[0] = now;
            row[1] = 100;
            intervalController.TimeTable.Rows.Add(row);
            row = intervalController.TimeTable.NewRow();
            row[0] = now + new TimeSpan(3, 12, 0, 0);
            row[1] = 200;
            intervalController.TimeTable.Rows.Add(row);

            var specificProperties = new SobekIntervalControllerProperties();
            intervalController.SpecificProperties = specificProperties;

            var rule = ControlGroupBuilder.GetRule(intervalController) as IntervalRule;

            Assert.IsNotNull(rule);
            Assert.AreEqual(ExtrapolationType.Periodic, rule.Extrapolation);

        }


        private void InitModelMocks()
        {
            availableLocations = new List<IFeature>();
            availableDataItems = new List<IDataItem>();

            mockRepository = new MockRepository();
            modelMock = mockRepository.Stub<IModel>();
            modelMock.Stub(p => p.GetChildDataItems(null)).IgnoreArguments().Return(availableDataItems);
            modelMock.Stub(p => p.GetChildDataItemLocations(DataItemRole.Output)).IgnoreArguments().Return(availableLocations);

            mockRepository.ReplayAll();

        }

        private void AddDataItemToMockModel(string name, string parameterName, QuantityType quantityType, ElementSet elementSet, DataItemRole role)
        {
            var hydroNode = new HydroNode(name); // IFeature and INamable
            availableLocations.Add(hydroNode);
            availableDataItems.Add(new DataItem
            {
                Role = role,
                ValueType = typeof(double),
                ValueConverter = new Model1DBranchFeatureValueConverter(modelMock, hydroNode, parameterName, quantityType, elementSet, role, "")
            });
        }

        [Test]
        public void WaterFlowModelQuantityTypeControllerParameterTest()
        {
            Assert.AreEqual(QuantityType.CrestLevel, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekControllerParameter.CrestLevel));
            Assert.AreEqual(QuantityType.CrestWidth, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekControllerParameter.CrestWidth));
            Assert.AreEqual(QuantityType.GateLowerEdgeLevel, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekControllerParameter.GateHeight));
            Assert.AreEqual(QuantityType.ValveOpening, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Culvert(), SobekControllerParameter.GateHeight));
            Assert.AreEqual(QuantityType.PumpCapacity, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekControllerParameter.PumpCapacity));
        }

        [Test]
        public void WaterFlowModelQuantityTypeTriggerParameterTest()
        {
            Assert.AreEqual(QuantityType.WaterLevel, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekTriggerParameterType.WaterLevelBranchLocation));
            Assert.AreEqual(QuantityType.Head, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekTriggerParameterType.HeadDifferenceStructure));
            Assert.AreEqual(QuantityType.Discharge, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekTriggerParameterType.DischargeBranchLocation));
            Assert.AreEqual(QuantityType.GateLowerEdgeLevel, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekTriggerParameterType.GateHeightStructure));
            Assert.AreEqual(QuantityType.ValveOpening, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Culvert(), SobekTriggerParameterType.GateHeightStructure));
            Assert.AreEqual(QuantityType.CrestLevel, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekTriggerParameterType.CrestLevelStructure));
            Assert.AreEqual(QuantityType.CrestWidth, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekTriggerParameterType.CrestWidthStructure));
            Assert.AreEqual(QuantityType.WaterLevel, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekTriggerParameterType.WaterlevelRetentionArea));
            Assert.AreEqual(QuantityType.PressureDifference, ControlGroupBuilder.GetWaterFlowModelQuantityType(new Weir(), SobekTriggerParameterType.PressureDifferenceStructure));
        }

        [Test]
        public void WaterFlowModelQuantityTypemeasurementLocationParameterTest()
        {
            Assert.AreEqual(QuantityType.WaterLevel, ControlGroupBuilder.GetWaterFlowModelQuantityType(SobekMeasurementLocationParameter.WaterLevel));
            Assert.AreEqual(QuantityType.Discharge, ControlGroupBuilder.GetWaterFlowModelQuantityType(SobekMeasurementLocationParameter.Discharge));
        }


        [Test]
        public void CreateControlGroupWithTimeCondition()
        {
            var triggerId = "triggerId";
            var triggerName = "triggerName";

            var timeCondition = new SobekTrigger
            {
                Id = triggerId,
                Name = triggerName,
                TriggerType = SobekTriggerType.Time,
            };
            timeCondition.TriggerTable = SobekTrigger.TriggerTableStructure;
            timeCondition.PeriodicExtrapolationPeriod = "3600"; // > 0

            var now = DateTime.Now;
            var row = timeCondition.TriggerTable.NewRow();
            row[0] = now;
            row[1] = 0; //off
            timeCondition.TriggerTable.Rows.Add(row);

             row = timeCondition.TriggerTable.NewRow();
            row[0] = now.AddMinutes(30);
            row[1] = 1; //on
            timeCondition.TriggerTable.Rows.Add(row);

            var condition = ControlGroupBuilder.GetConditions(timeCondition).OfType<TimeCondition>().FirstOrDefault();

            Assert.IsNotNull(condition);
            Assert.AreEqual(triggerId, condition.Name);
            Assert.AreEqual(triggerName, condition.LongName);
            Assert.AreEqual(2, condition.TimeSeries.Time.Values.Count);
            Assert.IsFalse((bool)condition.TimeSeries[condition.TimeSeries.Time.Values[0]]);
            Assert.IsTrue((bool)condition.TimeSeries[condition.TimeSeries.Time.Values[1]]);

             Assert.AreEqual(ExtrapolationType.Periodic, condition.Extrapolation);
            Assert.AreEqual(ExtrapolationType.Periodic,condition.TimeSeries.Time.ExtrapolationType);

        }

        [Test]
        public void CreateControlGroupWithPIDRuleWithConstantValue()
        {
            //CNTL id '5577940' nm 'Lith_WKC' ta 1 0 0 0 gi '5577719' '-1' '-1' '-1' ao 1 1 1 1 ct 3 ac 1 ca 0 cf 1 cb '016' '-1' '-1' '-1' '-1' cl 34249 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 mp 0 mc 0 sp tc 0 4.9 9.9999e+009 ui 3 ua 4.85 u0 4.5 pf 0.25 if 1.5 df 0 va 0.03 si '-1' hc ht 5 9.9999e+009 9.9999e+009 bl 0 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl

            var constantValue = 4.9;

            var pidController = new SobekController
            {
                ControllerType = SobekControllerType.PIDController,
                Id = controllerId,
                Name = controllerName,
                InterpolationType = interpolation,
                ExtrapolationType = extrapolation,
                SpecificProperties = new SobekPidControllerProperties { ConstantSetPoint = constantValue }

            };

            var pidRule = ControlGroupBuilder.GetRule(pidController) as PIDRule;

            Assert.IsNotNull(pidRule);
            Assert.AreEqual(constantValue, pidRule.ConstantValue);

        }

        [Test]
        public void CreateControlGroupWithRelativeTimeFromValueRule()
        {

            var relativeTimeFromValueController = new SobekController
            {
                ControllerType = SobekControllerType.RelativeFromValueController,
                Id = controllerId,
                Name = controllerName,
                LookUpTable = SobekController.LookUpTableStructure
            };

            var relativeTimeFromValueRule = ControlGroupBuilder.GetRule(relativeTimeFromValueController) as RelativeTimeRule;

            Assert.IsNotNull(relativeTimeFromValueRule);
            Assert.IsTrue(relativeTimeFromValueRule.FromValue);

        }

        [Test]
        public void CreateControlGroupWithHydraulicRuleOnFlowDirection()
        {
            //@"CNTL id '##4' nm 'Flow direction Cntrl 1' ct 1 ca 1 ac 1 cf 1 ta 0 0 0 0 gi '-1' '-1' '-1' '-1' ao 1 1 1 1 cp 4 ml '7' '-1' '-1' '-1' '-1' ps 40 ns 50 cntl" + Environment.NewLine +
            // the ps 40 ns 50 combination is a setpoint of 50 for negative direction and 40 for positive direction.
            // The import will convert this to a lookup table with the following values
            // -  - something 50
            // -  - small 50
            // -  + small 40
            // -  + something 40
            var hydraulicControllerOnFlowDirection = new SobekController
            {
                ControllerType = SobekControllerType.HydraulicController,
                Id = controllerId,
                Name = controllerName,
                PositiveStream = 40,
                NegativeStream = 50
            };

            var hydraulicRuleOnFlowDirection = ControlGroupBuilder.GetRule(hydraulicControllerOnFlowDirection) as HydraulicRule;

            Assert.IsNotNull(hydraulicRuleOnFlowDirection);
            Assert.AreEqual(1, hydraulicRuleOnFlowDirection.Function.Arguments.Count);
            Assert.AreEqual(1, hydraulicRuleOnFlowDirection.Function.Components.Count);
            Assert.AreEqual(3, hydraulicRuleOnFlowDirection.Function.Arguments[0].Values.Count);
            Assert.AreEqual(3, hydraulicRuleOnFlowDirection.Function.Components[0].Values.Count);
            Assert.AreEqual(-9999.0, (double)hydraulicRuleOnFlowDirection.Function.Arguments[0].Values[0], 1.0e-6);
            Assert.AreEqual(0.0, (double)hydraulicRuleOnFlowDirection.Function.Arguments[0].Values[1], 1.0e-6);
            Assert.AreEqual(9999.0, (double)hydraulicRuleOnFlowDirection.Function.Arguments[0].Values[2], 1.0e-6);
            Assert.AreEqual(hydraulicControllerOnFlowDirection.NegativeStream, (double)hydraulicRuleOnFlowDirection.Function.Components[0].Values[0], 1.0e-6);
            Assert.AreEqual(hydraulicControllerOnFlowDirection.PositiveStream, (double)hydraulicRuleOnFlowDirection.Function.Components[0].Values[1], 1.0e-6);
            Assert.AreEqual(hydraulicControllerOnFlowDirection.PositiveStream, (double)hydraulicRuleOnFlowDirection.Function.Components[0].Values[2], 1.0e-6);
        }


        [Test]
        public void CreateControlGroupWithHydraulicTriggerAndPeriodicExtrapolation()
        {
            var triggerId = "triggerId";
            var triggerName = "triggerName";
            var t0 = DateTime.Now;
            var t1 = t0.Add(new TimeSpan(1, 0, 0));
            var t2 = t0.Add(new TimeSpan(2, 0, 0));

            var hydraulicTriggerPeriodic = new SobekTrigger
                                               {
                                                   Id = triggerId,
                                                   Name = triggerName,
                                                   TriggerType = SobekTriggerType.Hydraulic,
                                                   PeriodicExtrapolationPeriod = "10800",
                                                   TriggerTable = SobekTrigger.TriggerTableStructure
                                               };

            var row = hydraulicTriggerPeriodic.TriggerTable.NewRow();
            row[0] = t0;
            row[1] = 0;
            row[2] = 0;
            row[3] = 1;
            row[4] = 11.1d;
            hydraulicTriggerPeriodic.TriggerTable.Rows.Add(row);

            row = hydraulicTriggerPeriodic.TriggerTable.NewRow();
            row[0] = t1;
            row[1] = 0;
            row[2] = 0;
            row[3] = 2;
            row[4] = 22.2d;
            hydraulicTriggerPeriodic.TriggerTable.Rows.Add(row);

            row = hydraulicTriggerPeriodic.TriggerTable.NewRow();
            row[0] = t2;
            row[1] = 0;
            row[2] = 0;
            row[3] = 1;
            row[4] = 33.3d;
            hydraulicTriggerPeriodic.TriggerTable.Rows.Add(row);

            var conditions = ControlGroupBuilder.GetConditions(hydraulicTriggerPeriodic).ToList();

            Assert.AreEqual(6, conditions.Count);

            var timeCondition1 = conditions[0] as TimeCondition;
            var timeCondition2 = conditions[2] as TimeCondition;
            var timeCondition3 = conditions[4] as TimeCondition;

            Assert.IsNotNull(timeCondition1);
            Assert.IsNotNull(timeCondition2);
            Assert.IsNotNull(timeCondition3);

            Assert.AreEqual(ExtrapolationType.Periodic, timeCondition1.Extrapolation);
            Assert.AreEqual(ExtrapolationType.Periodic, timeCondition2.Extrapolation);
            Assert.AreEqual(ExtrapolationType.Periodic, timeCondition3.Extrapolation);

            //timeCondition1
            Assert.AreEqual(3, timeCondition1.TimeSeries.Time.Values.Count);
            Assert.AreEqual(t0, timeCondition1.TimeSeries.Time.Values[0]);
            Assert.AreEqual(true, (bool)timeCondition1.TimeSeries[timeCondition1.TimeSeries.Time.Values[0]]);
            Assert.AreEqual(t1, timeCondition1.TimeSeries.Time.Values[1]);
            Assert.AreEqual(false, (bool)timeCondition1.TimeSeries[timeCondition1.TimeSeries.Time.Values[1]]);
            Assert.AreEqual(true, (bool)timeCondition1.TimeSeries[timeCondition1.TimeSeries.Time.Values[2]]);

            //timeCondition2
            Assert.AreEqual(3, timeCondition2.TimeSeries.Time.Values.Count);
            Assert.AreEqual(t0, timeCondition2.TimeSeries.Time.Values[0]);
            Assert.AreEqual(false, (bool)timeCondition2.TimeSeries[timeCondition2.TimeSeries.Time.Values[0]]);
            Assert.AreEqual(t1, timeCondition2.TimeSeries.Time.Values[1]);
            Assert.AreEqual(true, (bool)timeCondition2.TimeSeries[timeCondition2.TimeSeries.Time.Values[1]]);
            Assert.AreEqual(t2, timeCondition2.TimeSeries.Time.Values[2]);
            Assert.AreEqual(false, (bool)timeCondition2.TimeSeries[timeCondition2.TimeSeries.Time.Values[2]]);

            //timeCondition3
            Assert.AreEqual(2, timeCondition3.TimeSeries.Time.Values.Count);
            Assert.AreEqual(t0, timeCondition3.TimeSeries.Time.Values[0]);
            Assert.AreEqual(false, (bool)timeCondition3.TimeSeries[timeCondition3.TimeSeries.Time.Values[0]]);
            Assert.AreEqual(t2, timeCondition3.TimeSeries.Time.Values[1]);
            Assert.AreEqual(true, (bool)timeCondition3.TimeSeries[timeCondition3.TimeSeries.Time.Values[1]]);
        }

    }
}

