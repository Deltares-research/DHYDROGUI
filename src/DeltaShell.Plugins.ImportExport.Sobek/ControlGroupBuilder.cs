using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public static class ControlGroupBuilder
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ControlGroupBuilder));

        public static void CreateControlGroupForStructureAndAddToRtcModel(SobekStructureMapping sobekStructureMapping,
                                                                          IStructure1D structure,
                                                                          IModel model,
                                                                          RealTimeControlModel rtcModel,
                                                                          IDictionary<string, SobekController> sobekControllers,
                                                                          IDictionary<string, SobekTrigger> sobekTriggers)
        {
            IEnumerable<IDataItem> structureDataItems = GetInputDataItemsByLocationName(sobekStructureMapping.StructureId, model);

            if (structureDataItems.Count() == 0)
            {
                log.ErrorFormat("Item with Id '{0}' has no data item. Controller and triggers of {0} have not been imported.", sobekStructureMapping.StructureId);
                return;
            }

            var controlGroup = new ControlGroup {Name = "Control group of " + sobekStructureMapping.StructureId};

            IList<SobekController> orderedControllers = GetControllersOfStructureOrderedByOutputType(sobekStructureMapping.StructureId, sobekStructureMapping.ControllerIDs, sobekControllers);

            if (orderedControllers.Any())
            {
                // add control group only when there are inputs/outputs
                rtcModel.ControlGroups.Add(controlGroup);
            }

            foreach (SobekController sobekController in orderedControllers)
            {
                IEnumerable<ConditionBase> conditionsOfLastRuleWithSameOutput = new List<ConditionBase>();

                Output output = GetOutputItemFromControlGroup(sobekStructureMapping, structure, controlGroup, structureDataItems, sobekController);

                if (output != null)
                {
                    //last rule has same output, so get conditions for merging later on
                    RuleBase lastRule = controlGroup.Rules.Last();
                    conditionsOfLastRuleWithSameOutput = ControlGroupHelper.ConditionsOfRule(controlGroup, lastRule);
                }

                output = AddNewOutputItemToControlGroup(sobekStructureMapping, structure, controlGroup, structureDataItems, sobekController, rtcModel);

                if (output == null)
                {
                    continue; //no output for rule available
                }

                RuleBase rule = GetRule(sobekController);

                rule.Outputs.Add(output);
                SetUniqueName(rule, controlGroup);
                controlGroup.Rules.Add(rule);

                Input input = AddInputItemToControlGroup(structure, sobekController, controlGroup, model, rtcModel);

                if (input != null)
                {
                    rule.Inputs.Add(input);
                }

                AddConditionsToControlGroup(structure, sobekController.Triggers, sobekTriggers, model, rtcModel, rule, controlGroup);

                if (conditionsOfLastRuleWithSameOutput.Count() > 0)
                {
                    MergeLastRowWithSameOutput(controlGroup, rule, conditionsOfLastRuleWithSameOutput);
                }
            }
        }

        public static IEnumerable<ConditionBase> ConditionsOfRule(ControlGroup controlGroup, RuleBase rule)
        {
            var conditions = new HashSet<ConditionBase>();
            foreach (ConditionBase condition in controlGroup.Conditions)
            {
                if (condition.TrueOutputs.Contains(rule))
                {
                    conditions.Add(condition);
                    ConditionsOfCondition(controlGroup, condition, conditions);
                }

                if (condition.FalseOutputs.Contains(rule))
                {
                    conditions.Add(condition);
                    ConditionsOfCondition(controlGroup, condition, conditions);
                }
            }

            return conditions;
        }

        public static RuleBase GetRule(SobekController controller)
        {
            switch (controller.ControllerType)
            {
                case SobekControllerType.TimeController:
                    return GetTimeRule(controller);
                case SobekControllerType.HydraulicController:
                    return GetHydraulicRule(controller);
                case SobekControllerType.IntervalController:
                    return GetIntervalRule(controller);
                case SobekControllerType.PIDController:
                    return GetPIDRule(controller);
                case SobekControllerType.RelativeTimeController:
                    return GetRelativeTimeRule(controller);
                case SobekControllerType.RelativeFromValueController:
                    return GetFromValueRule(controller);
                default:
                    throw new NotSupportedException(string.Format("SobekController of type {0} has not been supported.", controller.ControllerType));
            }
        }

        public static IEnumerable<ConditionBase> GetConditions(SobekTrigger sobekTrigger)
        {
            switch (sobekTrigger.TriggerType)
            {
                case SobekTriggerType.Time:
                    yield return GetTimeCondition(sobekTrigger);
                    break;
                case SobekTriggerType.TimeAndHydraulic:
                    foreach (ConditionBase condition in GetCombinedConditions(sobekTrigger))
                    {
                        yield return condition;
                    }

                    break;
                case SobekTriggerType.Hydraulic:
                    foreach (ConditionBase condition in GetHydraulicConditions(sobekTrigger))
                    {
                        yield return condition;
                    }

                    break;
                default:
                    throw new NotSupportedException(string.Format("SobekTrigger of type {0} has not been supported.", sobekTrigger.TriggerType));
            }
        }

        public static QuantityType GetWaterFlowModelQuantityType(IStructure1D structure, SobekControllerParameter sobekControllerParameter)
        {
            Type type = structure.GetType();
            switch (sobekControllerParameter)
            {
                case SobekControllerParameter.BottomLevel2DGridCell:
                    // todo do not throw exception
                    throw new ArgumentException("Unsupported type", "measurementLocationParameter");
                case SobekControllerParameter.CrestLevel:
                    return QuantityType.CrestLevel;
                case SobekControllerParameter.CrestWidth:
                    return QuantityType.CrestWidth;
                case SobekControllerParameter.GateHeight:
                    // if Sobek212 set GateHeight is in ModelApi Lower Edge
                    if (typeof(ICulvert).IsAssignableFrom(type))
                    {
                        return QuantityType.ValveOpening;
                    }

                    return QuantityType.GateLowerEdgeLevel;
                case SobekControllerParameter.PumpCapacity:
                    return QuantityType.PumpCapacity;
            }

            throw new ArgumentException("Unsupported type", "measurementLocationParameter");
        }

        public static QuantityType GetWaterFlowModelQuantityType(IStructure1D structure, SobekTriggerParameterType triggerParameterType)
        {
            Type type = structure.GetType();
            switch (triggerParameterType)
            {
                case SobekTriggerParameterType.WaterLevelBranchLocation:
                    return QuantityType.WaterLevel;
                case SobekTriggerParameterType.HeadDifferenceStructure:
                    return QuantityType.Head;
                case SobekTriggerParameterType.DischargeBranchLocation:
                    return QuantityType.Discharge;
                case SobekTriggerParameterType.GateHeightStructure:
                    // if Sobek212 set GateHeight is in ModelApi Lower Edge or ValveOpening
                    if (typeof(ICulvert).IsAssignableFrom(type))
                    {
                        return QuantityType.ValveOpening;
                    }

                    return QuantityType.GateLowerEdgeLevel;
                case SobekTriggerParameterType.CrestLevelStructure:
                    return QuantityType.CrestLevel;
                case SobekTriggerParameterType.CrestWidthStructure:
                    return QuantityType.CrestWidth;
                case SobekTriggerParameterType.WaterlevelRetentionArea:
                    return QuantityType.WaterLevel;
                case SobekTriggerParameterType.PressureDifferenceStructure:
                    return QuantityType.PressureDifference;
            }

            throw new ArgumentException("Unsupported type", "measurementLocationParameter");
        }

        public static QuantityType GetWaterFlowModelQuantityType(SobekMeasurementLocationParameter measurementLocationParameter)
        {
            switch (measurementLocationParameter)
            {
                case SobekMeasurementLocationParameter.WaterLevel:
                    return QuantityType.WaterLevel;
                case SobekMeasurementLocationParameter.Discharge:
                    return QuantityType.Discharge;
                case SobekMeasurementLocationParameter.HeadDifference: //on structure
                    return QuantityType.Head;
                case SobekMeasurementLocationParameter.Velocity: //on structure
                    return QuantityType.Velocity;
                case SobekMeasurementLocationParameter.FlowDirection:      //on structure
                    return QuantityType.Discharge;                         //FlowDirection will be checked on pos or neg discharge
                case SobekMeasurementLocationParameter.PressureDifference: //on structure
                    return QuantityType.PressureDifference;
            }

            throw new ArgumentException("Unsupported type", "measurementLocationParameter");
        }

        private static void MergeLastRowWithSameOutput(ControlGroup controlGroup, RuleBase rule, IEnumerable<ConditionBase> conditionsOfLastRuleWithSameOutput)
        {
            var startOfNewRow = ControlGroupHelper.StartObjectOfARule(controlGroup, rule);

            if (startOfNewRow == null)
            {
                return;
            }

            foreach (ConditionBase condition in conditionsOfLastRuleWithSameOutput)
            {
                if (condition.TrueOutputs.Count == 0)
                {
                    condition.TrueOutputs.Add(startOfNewRow);
                }

                if (condition.FalseOutputs.Count == 0)
                {
                    condition.FalseOutputs.Add(startOfNewRow);
                }
            }
        }

        private static IList<SobekController> GetControllersOfStructureOrderedByOutputType(string structureId, IList<string> controllerIDs, IDictionary<string, SobekController> sobekControllers)
        {
            var returnList = new List<SobekController>();
            foreach (string controllerID in controllerIDs)
            {
                if (!sobekControllers.ContainsKey(controllerID))
                {
                    log.ErrorFormat("Controller Id {0} of structure {1} has not been found.", controllerID, structureId);
                    continue;
                }

                SobekController controller = sobekControllers[controllerID];
                SobekControllerParameter sobekControllerParameterType = controller.SobekControllerParameterType;
                int index = -1;

                for (int i = returnList.Count - 1; i >= 0; i--)
                {
                    if (returnList[i].SobekControllerParameterType == sobekControllerParameterType)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1 || index >= returnList.Count - 2)
                {
                    returnList.Add(controller);
                }
                else
                {
                    returnList.Insert(index + 1, controller); //insert after same type
                }
            }

            return returnList;
        }

        private static void ConditionsOfCondition(ControlGroup controlGroup, ConditionBase currentCondition, HashSet<ConditionBase> conditions)
        {
            foreach (ConditionBase condition in controlGroup.Conditions)
            {
                if (conditions.Contains(condition))
                {
                    continue; //breaks the recursive method
                }

                if (condition.TrueOutputs.Contains(currentCondition))
                {
                    conditions.Add(condition);
                    ConditionsOfCondition(controlGroup, condition, conditions);
                }

                if (condition.FalseOutputs.Contains(currentCondition))
                {
                    conditions.Add(condition);
                    ConditionsOfCondition(controlGroup, currentCondition, conditions);
                }
            }
        }

        private static void AddConditionsToControlGroup(IStructure1D structure, IEnumerable<Trigger> sobekControllerTriggers, IDictionary<string, SobekTrigger> sobekTriggers, IModel model, RealTimeControlModel rtcModel, RuleBase rule, ControlGroup controlGroup)
        {
            if (sobekControllerTriggers == null)
            {
                return;
            }

            IList<ConditionBase> conditions = null;
            var previousConditions = new List<ConditionBase>();
            var bNewOrRow = false;

            foreach (Trigger trigger in sobekControllerTriggers)
            {
                if (!trigger.Active)
                {
                    continue;
                }

                if (!sobekTriggers.ContainsKey(trigger.Id))
                {
                    log.ErrorFormat("Adding conditions to control group: trigger with id {0} can not be found", trigger.Id);
                    continue;
                }

                SobekTrigger sobekTrigger = sobekTriggers[trigger.Id];

                //get conditions and add to control group
                conditions = GetConditions(sobekTrigger).ToList();
                foreach (ConditionBase condition in conditions)
                {
                    SetUniqueName(condition, controlGroup);
                    controlGroup.Conditions.Add(condition);
                }

                //set input
                Input input = AddInputItemToControlGroup(structure, sobekTrigger, controlGroup, model, rtcModel);
                if (input != null)
                {
                    AddInputToNonTimeConditions(conditions, input);
                    HydraukicRuleBasedOnDownUpHack(conditions, input);
                }

                //Have to start a new (or) row
                if (bNewOrRow)
                {
                    StartNew_OrRow_AndConnectPreviousConditions(conditions.First(), previousConditions);
                    bNewOrRow = false;
                }
                else
                {
                    foreach (ConditionBase p in previousConditions.Where(pc => pc.TrueOutputs.Count == 0))
                    {
                        p.TrueOutputs.Add(conditions.First());
                    }
                }

                previousConditions.AddRange(conditions);

                if (!trigger.And)
                {
                    bNewOrRow = true;
                    AddRuleToEmptyTrueOutputs(conditions, rule);
                    conditions = null;
                }
            }

            //last conditions (with 'and' property) should be connected to rule
            AddRuleToEmptyTrueOutputs(conditions, rule);
        }

        private static void HydraukicRuleBasedOnDownUpHack(IList<ConditionBase> conditions, Input input)
        {
            if (input.ParameterName == "haha")
            {
                foreach (ConditionBase hydraulicCondition in conditions.Where(c => c.Input == input))
                {
                    hydraulicCondition.Value = 0;
                }
            }
        }

        private static void AddInputToNonTimeConditions(IEnumerable<ConditionBase> conditions, Input input)
        {
            foreach (ConditionBase condition in conditions)
            {
                if (condition is TimeCondition)
                {
                    continue;
                }

                condition.Input = input;
            }
        }

        private static void AddRuleToEmptyTrueOutputs(IEnumerable<ConditionBase> conditions, RuleBase rule)
        {
            if (conditions == null)
            {
                return;
            }

            foreach (ConditionBase condition in conditions)
            {
                if (condition.TrueOutputs.Count == 0)
                {
                    condition.TrueOutputs.Add(rule);
                }
            }
        }

        private static void StartNew_OrRow_AndConnectPreviousConditions(ConditionBase condition, List<ConditionBase> previousConditions)
        {
            foreach (ConditionBase previousCondition in previousConditions)
            {
                if (previousCondition.FalseOutputs.Count == 0)
                {
                    previousCondition.FalseOutputs.Add(condition);
                }
            }

            previousConditions.Clear();
        }

        private static void SetUniqueName(RtcBaseObject rtcBaseObject, ControlGroup controlGroup)
        {
            var index = 0;
            if (rtcBaseObject is ConditionBase)
            {
                index = controlGroup.Conditions.Count(c => c.Name == rtcBaseObject.Name || c.Name.StartsWith(rtcBaseObject.Name + "_"));
                if (index > 0)
                {
                    rtcBaseObject.Name = rtcBaseObject.Name + "_" + index;
                }
            }
            else if (rtcBaseObject is RuleBase)
            {
                index = controlGroup.Rules.Count(c => c.Name.StartsWith(rtcBaseObject.Name + "_"));
                if (index > 0)
                {
                    rtcBaseObject.Name = rtcBaseObject.Name + "_" + index;
                }
            }
        }

        private static SobekLocationType GetLocationType(SobekTrigger sobekTrigger)
        {
            switch (sobekTrigger.TriggerParameterType)
            {
                case SobekTriggerParameterType.WaterLevelBranchLocation:
                    return SobekLocationType.ObservationPointLocation;
                case SobekTriggerParameterType.HeadDifferenceStructure:
                    return SobekLocationType.StructureLocation;
                case SobekTriggerParameterType.DischargeBranchLocation:
                    return SobekLocationType.ObservationPointLocation;
                case SobekTriggerParameterType.GateHeightStructure:
                    return SobekLocationType.StructureLocation;
                case SobekTriggerParameterType.CrestLevelStructure:
                    return SobekLocationType.StructureLocation;
                case SobekTriggerParameterType.CrestWidthStructure:
                    return SobekLocationType.StructureLocation;
                case SobekTriggerParameterType.WaterlevelRetentionArea:
                    return SobekLocationType.RetentionAreaLocation;
                case SobekTriggerParameterType.PressureDifferenceStructure:
                    return SobekLocationType.StructureLocation;
            }

            throw new ArgumentException(
                string.Format("Unsupported triggertype {0}", sobekTrigger.TriggerParameterType), "sobekTrigger");
        }

        private static Input AddInputItemToControlGroup(IStructure1D structure, SobekTrigger sobekTrigger, ControlGroup controlGroup, IModel model, RealTimeControlModel rtcModel)
        {
            IDataItem inputDataItem;

            if (sobekTrigger.TriggerType == SobekTriggerType.Time)
            {
                return null; // time trigger does not have input
            }

            SobekLocationType location = GetLocationType(sobekTrigger);

            switch (location)
            {
                case SobekLocationType.ObservationPointLocation:
                    if (string.IsNullOrEmpty(sobekTrigger.MeasurementStationId))
                    {
                        return null; //condition without input. Probably a time condition
                    }

                    inputDataItem = GetDataItem(model,
                                                sobekTrigger.MeasurementStationId,
                                                GetWaterFlowModelQuantityType(structure, sobekTrigger.TriggerParameterType),
                                                ElementSet.Observations);

                    if (inputDataItem == null)
                    {
                        log.ErrorFormat("Parameter {1} of observation point {0} is not supported by the data item provider.", sobekTrigger.MeasurementStationId, sobekTrigger.TriggerParameterType);
                        return null;
                    }

                    break;

                case SobekLocationType.StructureLocation:
                    if (string.IsNullOrEmpty(sobekTrigger.StructureId) || sobekTrigger.StructureId == "-1")
                    {
                        return null; //condition without input. Probably a time condition
                    }

                    inputDataItem = GetDataItem(model,
                                                sobekTrigger.StructureId,
                                                GetWaterFlowModelQuantityType(structure, sobekTrigger.TriggerParameterType),
                                                ElementSet.Structures);

                    if (inputDataItem == null)
                    {
                        log.ErrorFormat("Parameter {1} of structure {0} is not supported by the data item provider.", sobekTrigger.StructureId, sobekTrigger.TriggerParameterType);
                        return null;
                    }

                    break;

                case SobekLocationType.RetentionAreaLocation:
                    log.ErrorFormat("Parameter {0} of RetentionAreaLocation is not supported yet.", sobekTrigger.TriggerParameterType);
                    return null; // not supported

                default:
                    throw new ArgumentException(string.Format("Unsupported triggerlocatriontype {0}", location), "sobekTrigger");
            }

            Input input = controlGroup.Inputs.FirstOrDefault(o => o.Name == inputDataItem.GetFeature() + "_" + inputDataItem.GetParameterName());

            if (input != null)
            {
                return input; //input already exists and returns existing input
            }

            input = new Input();
            controlGroup.Inputs.Add(input);
            rtcModel.GetDataItemByValue(input).LinkTo(inputDataItem);

            return input;
        }

        private static Input AddInputItemToControlGroup(IStructure1D structure, SobekController sobekController, ControlGroup controlGroup, IModel model, RealTimeControlModel rtcModel)
        {
            IDataItem outputDataItem;
            if (!string.IsNullOrEmpty(sobekController.MeasurementStationId))
            {
                outputDataItem = GetDataItem(model,
                                             sobekController.MeasurementStationId,
                                             GetWaterFlowModelQuantityType(sobekController.MeasurementLocationParameter),
                                             ElementSet.Observations);

                if (outputDataItem == null)
                {
                    log.ErrorFormat("Parameter {1} of observation point {0} is not supported by the controlled model.", sobekController.MeasurementStationId, sobekController.SobekControllerParameterType);
                    return null;
                }
            }
            else if (!string.IsNullOrEmpty(sobekController.StructureId))
            {
                // if no measure station is given the input is a structure
                // this only applies to head difference (2) and pressure difference (5)
                outputDataItem = GetDataItem(model,
                                             sobekController.StructureId,
                                             GetWaterFlowModelQuantityType(sobekController.MeasurementLocationParameter),
                                             ElementSet.Structures);

                if (outputDataItem == null)
                {
                    log.ErrorFormat("Parameter {1} of structure {0} is not supported by the controlled model.", sobekController.StructureId, sobekController.SobekControllerParameterType);
                    return null;
                }
            }
            else
            {
                return null; //rule without input
            }

            Input input = controlGroup.Inputs.FirstOrDefault(o => o.Name == outputDataItem.GetFeature() + "_" + outputDataItem.GetParameterName());

            if (input != null)
            {
                return input; //input already exists and returns existing input
            }

            input = new Input();
            controlGroup.Inputs.Add(input);
            rtcModel.GetDataItemByValue(input).LinkTo(outputDataItem);

            return input;
        }

        private static IDataItem GetDataItem(IModel model, string locationName, QuantityType quantityType, ElementSet elementSet)
        {
            IEnumerable<IDataItem> inputDataItems = GetOutputDataItemsByLocationName(locationName, model);
            IDataItem inputDataItem = inputDataItems.FirstOrDefault(
                item =>
                {
                    var flowValueConverter = item.ValueConverter as Model1DBranchFeatureValueConverter;
                    if (flowValueConverter != null)
                    {
                        return flowValueConverter.QuantityType == quantityType
                               && flowValueConverter.ElementSet == elementSet;
                    }

                    return false;
                });
            return inputDataItem;
        }

        private static IEnumerable<IDataItem> GetInputDataItemsByLocationName(string locationName, IModel model)
        {
            var location = (IFeature)model.GetChildDataItemLocations(DataItemRole.Input)
                                          .OfType<INameable>()
                                          .FirstOrDefault(l => string.Equals(l.Name, locationName, StringComparison.InvariantCultureIgnoreCase));

            return location != null 
                       ? model.GetChildDataItems(location) 
                       : new List<IDataItem>();
        }

        private static IEnumerable<IDataItem> GetOutputDataItemsByLocationName(string locationName, IModel model)
        {
            var location = (IFeature)model.GetChildDataItemLocations(DataItemRole.Output)
                                          .OfType<INameable>()
                                          .FirstOrDefault(l => string.Equals(l.Name, locationName, StringComparison.InvariantCultureIgnoreCase));

            return location != null 
                       ? model.GetChildDataItems(location) 
                       : new List<IDataItem>();
        }

        /// <summary>
        /// The sobekController has knowledge about the controlled parameter of the structure needed for the output
        /// </summary>
        /// <param name="sobekStructureMapping"></param>
        /// <param name="controlGroup"></param>
        /// <param name="structureDataItems"></param>
        /// <param name="controller"></param>
        /// <returns>Returns null if not exists</returns>
        private static Output GetOutputItemFromControlGroup(SobekStructureMapping sobekStructureMapping, IStructure1D structure, ControlGroup controlGroup,
                                                            IEnumerable<IDataItem> structureDataItems, SobekController controller)
        {
            IDataItem structureDataItem =
                structureDataItems.FirstOrDefault(
                    item =>
                    {
                        var converter = (Model1DBranchFeatureValueConverter) item.ValueConverter;
                        return converter.QuantityType == GetWaterFlowModelQuantityType(structure, controller.SobekControllerParameterType)
                               && (converter.ElementSet == ElementSet.Structures || converter.ElementSet == ElementSet.Pumps && controller.SobekControllerParameterType == SobekControllerParameter.PumpCapacity);
                    });

            if (structureDataItem == null)
            {
                log.ErrorFormat("Parameter {1} of structure {0} is not supported by the controlled model.", sobekStructureMapping.StructureId, controller.SobekControllerParameterType);
                return null;
            }

            Output output = controlGroup.Outputs.FirstOrDefault(o => o.Name == structureDataItem.GetFeature() + "_" + structureDataItem.GetParameterName());

            return output;
        }

        /// <summary>
        /// The sobekController has knowledge about the controlled parameter of the structure needed for the output
        /// </summary>
        /// <param name="sobekStructureMapping"></param>
        /// <param name="controlGroup"></param>
        /// <param name="structureDataItems"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        private static Output AddNewOutputItemToControlGroup(SobekStructureMapping sobekStructureMapping, IStructure1D structure, ControlGroup controlGroup,
                                                             IEnumerable<IDataItem> structureDataItems, SobekController controller, RealTimeControlModel rtcModel)
        {
            IDataItem structureDataItem = structureDataItems.FirstOrDefault(
                item =>
                {
                    var flowValueConverter = item.ValueConverter as Model1DBranchFeatureValueConverter;
                    if (flowValueConverter != null)
                    {
                        return flowValueConverter.QuantityType
                               ==
                               GetWaterFlowModelQuantityType(
                                   structure, controller.SobekControllerParameterType)
                               && (flowValueConverter.ElementSet == ElementSet.Structures || flowValueConverter.ElementSet == ElementSet.Pumps && controller.SobekControllerParameterType == SobekControllerParameter.PumpCapacity);
                    }

                    return false;
                });

            if (structureDataItem == null)
            {
                log.ErrorFormat("Parameter {1} of structure {0} is not supported by the model.", sobekStructureMapping.StructureId, controller.SobekControllerParameterType);
                return null;
            }

            Output output = controlGroup.Outputs.FirstOrDefault(o => o.Name == structureDataItem.GetFeature() + "_" + structureDataItem.GetParameterName());

            if (output != null)
            {
                return output; //output already exists and returns existing output
            }

            output = new Output();
            controlGroup.Outputs.Add(output);
            structureDataItem.LinkTo(rtcModel.GetDataItemByValue(output));

            return output;
        }

        private static IEnumerable<ConditionBase> GetHydraulicConditions(SobekTrigger sobekTrigger)
        {
            DataColumn columnTime = sobekTrigger.TriggerTable.Columns["Time"];
            DataColumn columnOperator = sobekTrigger.TriggerTable.Columns["Operation"];
            DataColumn columnValue = sobekTrigger.TriggerTable.Columns["Value"];
            TimeCondition previousTimeCondition = null;
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now;
            int nRows = sobekTrigger.TriggerTable.Rows.Count;

            if (nRows > 0)
            {
                startTime = (DateTime) sobekTrigger.TriggerTable.Rows[0][columnTime];
                endTime = (DateTime) sobekTrigger.TriggerTable.Rows[nRows - 1][columnTime];
            }

            for (var i = 0; i < nRows; i++)
            {
                DataRow nextRow = null;
                DataRow row = sobekTrigger.TriggerTable.Rows[i];
                Operation operation = (bool) row[columnOperator] ? Operation.Greater : Operation.Less;
                var value = Convert.ToDouble(row[columnValue]);

                if (i < sobekTrigger.TriggerTable.Rows.Count - 1)
                {
                    nextRow = sobekTrigger.TriggerTable.Rows[i + 1];
                }

                var timeCondition = new TimeCondition
                {
                    Name = sobekTrigger.Id,
                    LongName = sobekTrigger.Name,
                    InterpolationOptionsTime = InterpolationType.Constant,
                    Extrapolation = ExtrapolationType.Constant
                };

                if ((DateTime) row[columnTime] != startTime)
                {
                    timeCondition.TimeSeries[startTime] = false;
                }

                timeCondition.TimeSeries[row[columnTime]] = true;

                if (nextRow != null)
                {
                    timeCondition.TimeSeries[nextRow[columnTime]] = false;
                }

                if (endTime > timeCondition.TimeSeries.Time.Values.Last())
                {
                    timeCondition.TimeSeries[endTime] = timeCondition.TimeSeries[startTime];
                }

                TimeSeriesHelper.SetPeriodicExtrapolationRtc(timeCondition.TimeSeries, sobekTrigger.PeriodicExtrapolationPeriod);

                StandardCondition hydraulicCondition;

                if (sobekTrigger.CheckOn == SobekTriggerCheckOn.Direction)
                {
                    hydraulicCondition = new DirectionalCondition
                    {
                        Name = sobekTrigger.Id,
                        LongName = sobekTrigger.Name,
                        Operation = operation,
                    };
                }
                else
                {
                    hydraulicCondition = new StandardCondition
                    {
                        Name = sobekTrigger.Id,
                        LongName = sobekTrigger.Name,
                        Operation = operation,
                        Value = value
                    };
                }

                timeCondition.TrueOutputs.Add(hydraulicCondition);

                if (previousTimeCondition != null)
                {
                    previousTimeCondition.FalseOutputs.Add(timeCondition);
                }

                previousTimeCondition = timeCondition;

                yield return timeCondition;
                yield return hydraulicCondition;
            }
        }

        private static IEnumerable<ConditionBase> GetCombinedConditions(SobekTrigger sobekTrigger)
        {
            log.WarnFormat("Combined triggers are not supported. Trigger {0} - {1} has not been imported!",
                           sobekTrigger.Id, sobekTrigger.Name);
            yield return null;
        }

        private static ConditionBase GetTimeCondition(SobekTrigger sobekTrigger)
        {
            var timeCondition = new TimeCondition
            {
                Name = sobekTrigger.Id,
                LongName = sobekTrigger.Name,
                InterpolationOptionsTime = InterpolationType.Constant,
                Extrapolation = ExtrapolationType.Constant
            };

            foreach (DataRow row in sobekTrigger.TriggerTable.Rows)
            {
                timeCondition.TimeSeries[(DateTime) row[sobekTrigger.TriggerTable.Columns["Time"]]] = (bool) row[sobekTrigger.TriggerTable.Columns["OnOff"]];
            }

            if (sobekTrigger.PeriodicExtrapolationPeriod != "")
            {
                timeCondition.Extrapolation = ExtrapolationType.Periodic;
                TimeSeriesHelper.SetPeriodicExtrapolationRtc(timeCondition.TimeSeries, sobekTrigger.PeriodicExtrapolationPeriod);
            }

            return timeCondition;
        }

        private static TimeRule GetTimeRule(SobekController controller)
        {
            var timeRule = new TimeRule
            {
                Name = controller.Id,
                LongName = controller.Name,
                InterpolationOptionsTime = controller.InterpolationType,
                Periodicity = controller.ExtrapolationType // setter will check for validity
            };

            DataTableHelper.SetTableToFunction(controller.TimeTable, timeRule.TimeSeries);
            TimeSeriesHelper.SetPeriodicExtrapolationRtc(timeRule.TimeSeries, controller.ExtrapolationPeriod);

            return timeRule;
        }

        private static IntervalRule GetIntervalRule(SobekController controller)
        {
            var intervalRule = new IntervalRule
            {
                Name = controller.Id,
                LongName = controller.Name,
                InterpolationOptionsTime = controller.InterpolationType,
                Extrapolation = controller.ExtrapolationType
            };

            var specificProperties = (SobekIntervalControllerProperties) controller.SpecificProperties;
            intervalRule.Setting.Min = specificProperties.DeadBandMin;
            intervalRule.Setting.Max = specificProperties.DeadBandMax;
            intervalRule.Setting.Below = specificProperties.USminimum;
            intervalRule.Setting.Above = specificProperties.USmaximum;
            intervalRule.Setting.MaxSpeed = specificProperties.ControlVelocity;
            intervalRule.DeadBandType = (IntervalRule.IntervalRuleDeadBandType) specificProperties.DeadBandType;

            if (intervalRule.DeadBandType == IntervalRule.IntervalRuleDeadBandType.Fixed)
            {
                intervalRule.DeadbandAroundSetpoint = specificProperties.DeadBandFixedSize;
            }
            else
            {
                intervalRule.DeadbandAroundSetpoint = specificProperties.DeadBandPecentage;
            }

            intervalRule.IntervalType = (IntervalRule.IntervalRuleIntervalType) specificProperties.IntervalType;
            intervalRule.FixedInterval = specificProperties.FixedInterval;

            if (controller.TimeTable != null)
            {
                DataTableHelper.SetTableToFunction(controller.TimeTable, intervalRule.TimeSeries);
                TimeSeriesHelper.SetPeriodicExtrapolationRtc(intervalRule.TimeSeries, controller.ExtrapolationPeriod);
            }

            intervalRule.TimeSeries.Components[0].DefaultValue = specificProperties.ConstantSetPoint;

            return intervalRule;
        }

        private static HydraulicRule GetHydraulicRule(SobekController controller)
        {
            var hydraulicRule = new HydraulicRule
            {
                Name = controller.Id,
                LongName = controller.Name,
                Interpolation = controller.InterpolationType
            };

            var specificProperties = controller.SpecificProperties as SobekHydraulicControllerProperties;
            if (specificProperties != null)
            {
                hydraulicRule.TimeLag = specificProperties.TimeLag;
            }

            if (controller.LookUpTable != null)
            {
                DataTableHelper.SetTableToFunction(controller.LookUpTable, hydraulicRule.Function);
            }
            else if (controller.PositiveStream != 0 || controller.NegativeStream != 0)
            {
                MakeTableForFlowDirection(controller, hydraulicRule);
            }

            return hydraulicRule;
        }

        private static PIDRule GetPIDRule(SobekController controller)
        {
            var pidRule = new PIDRule
            {
                Name = controller.Id,
                LongName = controller.Name
            };

            var specificProperties = (SobekPidControllerProperties) controller.SpecificProperties;

            pidRule.Kd = specificProperties.KFactorDifferential;
            pidRule.Ki = specificProperties.KFactorIntegral;
            pidRule.Kp = specificProperties.KFactorProportional;

            pidRule.Setting.Min = specificProperties.USminimum;
            pidRule.Setting.Max = specificProperties.USmaximum;
            pidRule.Setting.MaxSpeed = specificProperties.MaximumSpeed;

            if (!double.IsNaN(specificProperties.ConstantSetPoint))
            {
                pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;
                pidRule.TimeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
                pidRule.TimeSeries.Components[0].DefaultValue = specificProperties.ConstantSetPoint;
            }
            else
            {
                pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
                if (controller.TimeTable == null)
                {
                    log.ErrorFormat("Time table for setpoint of {0} not set; rule correctly initialized", controller.Id);
                    return pidRule;
                }

                DataTableHelper.SetTableToFunction(controller.TimeTable, pidRule.TimeSeries);

                pidRule.ExtrapolationOptionsTime = controller.ExtrapolationType;
                pidRule.InterpolationOptionsTime = controller.InterpolationType;

                TimeSeriesHelper.SetPeriodicExtrapolationRtc(pidRule.TimeSeries, controller.ExtrapolationPeriod);
            }

            log.WarnFormat("The initial value {1} of PID controller {0} will not be used. The defined structure dimension is the initial value", pidRule.Name, specificProperties.USinitial);

            return pidRule;
        }

        private static RelativeTimeRule GetRelativeTimeRule(SobekController controller)
        {
            var relativeTimeRule = new RelativeTimeRule
            {
                Name = controller.Id,
                LongName = controller.Name,
                Interpolation = controller.InterpolationType,
                MinimumPeriod = controller.MinimumPeriod
            };

            DataTableHelper.SetTableToFunction(controller.LookUpTable, relativeTimeRule.Function);

            return relativeTimeRule;
        }

        private static RuleBase GetFromValueRule(SobekController controller)
        {
            RelativeTimeRule fromValueRule = GetRelativeTimeRule(controller);
            fromValueRule.FromValue = true;
            return fromValueRule;
        }

        /// <summary>
        /// MakeTableForFlowDirection
        /// No possibility to set extrapolation,interpolation in rtcTools lookupTable yet (both are linear now...)
        /// So a hack to solve this problem for the moment
        /// do not use double.MinValue and double.MaxValue: Teechart will crash on these values
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="hydraulicRule"></param>
        private static void MakeTableForFlowDirection(SobekController controller, HydraulicRule hydraulicRule)
        {
            hydraulicRule.Function[-9999.0] = controller.NegativeStream;
            hydraulicRule.Function[0.0] = controller.PositiveStream;
            hydraulicRule.Function[9999.0] = controller.PositiveStream;
            hydraulicRule.Interpolation = InterpolationType.Constant;
        }
    }
}