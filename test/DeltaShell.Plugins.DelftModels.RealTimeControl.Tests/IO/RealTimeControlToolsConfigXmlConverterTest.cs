using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlToolsConfigXmlConverterTest
    {
        private const string ControlGroupName = "control_group_name";
        private const string ComponentName = "component_name";
        private static readonly Random random = new Random();
        private readonly ExpressionNodeEqualityComparer nodeComparer = new ExpressionNodeEqualityComparer();

        [Test]
        public void ConvertToDataAccessObjects_TimeRule_CorrectResultIsReturned()
        {
            // Setup
            RuleComplexType ruleElement = CreateTimeRuleElement(ControlGroupName);

            RuleComplexType[] ruleElements =
            {
                ruleElement
            };
            var triggerElements = new TriggerComplexType[]
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggerElements)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));
            var ruleObj = dataAccessObjects[0] as RuleDataAccessObject;
            Assert.That(ruleObj, Is.Not.Null);
            Assert.AreEqual(typeof(TimeRule), ruleObj.Object.GetType());
        }

        [Test]
        public void GivenSomeRuleElements_WhenCreateControlGroupsFromXmlElementIDsIsCalled_CorrectControlGroupsAreMade()
        {
            // Setup
            const string controlGroup1Name = "control_group1";
            const string controlGroup2Name = "control_group2";

            RuleComplexType[] ruleElements = new[]
            {
                CreateTimeRuleElement(controlGroup1Name),
                CreateIntervalRuleElement(controlGroup2Name, false),
                CreateLookupTableRuleElement(RtcXmlTag.HydraulicRule, controlGroup1Name),
                CreatePidRuleElement(controlGroup2Name),
                CreateRelativeTimeRuleElement(controlGroup1Name)
            };

            var triggerElements = new TriggerComplexType[]
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggerElements)
                                                                      .ToArray();

            string[] controlGroupNames = dataAccessObjects.Select(o => o.ControlGroupName).Distinct().ToArray();
            // Assert
            Assert.AreEqual(2, controlGroupNames.Length, "Number of control groups was expected to be 2.");
            Assert.AreEqual(controlGroup1Name, controlGroupNames.First());
            Assert.AreEqual(controlGroup2Name, controlGroupNames.Last());
        }

        [Test]
        public void ConvertToExpressionTrees_MultipleExpressions_OneControlGroup_ReturnsCorrectTree()
        {
            // Setup
            var leafInput = $"{RtcXmlTag.Input}some_input";

            const string nameA = "A";
            var idA = $"{ControlGroupName}/{nameA}";
            var operatorA = Operator.Subtract; // TODO random

            const string nameB = "B";
            var idB = $"{ControlGroupName}/{nameB}";
            var operatorB = Operator.Add; // TODO random

            ExpressionComplexType expressionA = ExpressionComplexTypeBuilder.Create(idA, operatorA, nameA)
                                                                            .WithInputAsFirstReference(nameB)
                                                                            .AndInputAsSecondReference(leafInput);
            ExpressionComplexType expressionB = ExpressionComplexTypeBuilder.Create(idB, operatorB, nameB)
                                                                            .WithInputAsFirstReference(leafInput)
                                                                            .AndInputAsSecondReference(leafInput);

            TriggerComplexType[] triggers = WrapTriggers(expressionB, expressionA);
            RuleComplexType[] ruleElements =
                {};

            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggers)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));

            ExpressionTree tree = AssertExpressionTree(dataAccessObjects.OfType<ExpressionTree>().ToArray(), idA,
                                                       ControlGroupName, nameA);
            BranchNode branchNode = AssertCorrectBranchNode(tree.RootNode, nameA, operatorA);
            AssertCorrectBranchNodeWithTwoLeafNodes(branchNode.FirstNode, nameB, operatorB, leafInput, leafInput);
            AssertCorrectLeafNode(branchNode.SecondNode, leafInput);
        }

        [Test]
        public void ConvertToExpressionTrees_MultipleExpressions_MultipleControlGroups_ReturnsCorrectResult()
        {
            // Setup
            var leafInput = $"{RtcXmlTag.Input}some_input";
            const string controlGroup1 = "controlgroup_A";
            const string controlGroup2 = "controlgroup_B";

            const string expressionNameA = "A";
            var operatorA = Operator.Subtract; // TODO random

            const string expressionNameB = "B";
            var operatorB = Operator.Add; // TODO random

            var idA1 = $"{controlGroup1}/{expressionNameA}";
            var idB1 = $"{controlGroup1}/{expressionNameB}";

            var idA2 = $"{controlGroup2}/{expressionNameA}";
            var idB2 = $"{controlGroup2}/{expressionNameB}";

            TriggerComplexType[] triggers = WrapTriggers(
                ExpressionComplexTypeBuilder.Create(idA1, operatorA, expressionNameA)
                                            .WithInputAsFirstReference(expressionNameB)
                                            .AndInputAsSecondReference(leafInput),
                ExpressionComplexTypeBuilder.Create(idB1, operatorB, expressionNameB)
                                            .WithInputAsFirstReference(leafInput)
                                            .AndInputAsSecondReference(leafInput),
                ExpressionComplexTypeBuilder.Create(idA2, operatorA, expressionNameA)
                                            .WithInputAsFirstReference(expressionNameB)
                                            .AndInputAsSecondReference(leafInput),
                ExpressionComplexTypeBuilder.Create(idB2, operatorB, expressionNameB)
                                            .WithInputAsFirstReference(leafInput)
                                            .AndInputAsSecondReference(leafInput));
            RuleComplexType[] rules =
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(rules, triggers)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(2));
            ExpressionTree[] trees = dataAccessObjects.OfType<ExpressionTree>().ToArray();

            ExpressionTree tree = AssertExpressionTree(trees, idA1, controlGroup1, expressionNameA);
            BranchNode branchNode = AssertCorrectBranchNode(tree.RootNode, expressionNameA, operatorA);
            AssertCorrectBranchNodeWithTwoLeafNodes(branchNode.FirstNode, expressionNameB, operatorB, leafInput,
                                                    leafInput);
            AssertCorrectLeafNode(branchNode.SecondNode, leafInput);

            tree = AssertExpressionTree(trees, idA2, controlGroup2, expressionNameA);
            branchNode = AssertCorrectBranchNode(tree.RootNode, expressionNameA, operatorA);
            AssertCorrectBranchNodeWithTwoLeafNodes(branchNode.FirstNode, expressionNameB, operatorB, leafInput,
                                                    leafInput);
            AssertCorrectLeafNode(branchNode.SecondNode, leafInput);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ConvertToExpressionTrees_WithComplexExample_ReturnsCorrectTree()
        {
            // Setup
            var leafInput = $"{RtcXmlTag.Input}some_input"; // TODO random : constant or param
            var @operator = Operator.Add;                   // TODO random

            const string nameA = "A";
            var idA = $"{ControlGroupName}/{nameA}";

            const string nameB = "B";
            var idB = $"{ControlGroupName}/{nameB}";

            const string nameC = "C";
            var idC = $"{ControlGroupName}/{nameC}";

            const string nameD = "D";
            var idD = $"{ControlGroupName}/{nameD}";

            const string nameE = "E";
            var idE = $"{ControlGroupName}/{nameE}";

            const string nameF = "F";
            var idF1 = $"{ControlGroupName}/{nameF}_1";
            var idF2 = $"{ControlGroupName}/{nameF}_2";

            const string nameG = "G";
            var idG = $"{ControlGroupName}/{nameG}";

            const string nameH = "H";
            var idH = $"{ControlGroupName}/{nameH}";

            var condition2 =
                (StandardTriggerComplexType) CreateStandardConditionElement(RtcXmlTag.StandardCondition, ControlGroupName,
                                                                            "condition2").Item;

            ConditionXmlBuilder.Start(condition2)
                               .WithTrueOutput(ExpressionComplexTypeBuilder.Create(idF1, @operator, nameF)
                                                                           .WithInputAsFirstReference(nameD)
                                                                           .AndInputAsSecondReference(nameG))
                               .WithTrueOutput(ExpressionComplexTypeBuilder.Create(idG, @operator, nameG)
                                                                           .WithInputAsFirstReference(leafInput)
                                                                           .AndInputAsSecondReference(leafInput))
                               .WithFalseOutput(ExpressionComplexTypeBuilder.Create(idF2, @operator, nameF)
                                                                            .WithInputAsFirstReference(nameD)
                                                                            .AndInputAsSecondReference(leafInput));

            var condition1 =
                (StandardTriggerComplexType) CreateStandardConditionElement(RtcXmlTag.StandardCondition, ControlGroupName,
                                                                            "condition1").Item;
            ConditionXmlBuilder.Start(condition1)
                               .WithTrueOutput(condition2)
                               .WithFalseOutput(ExpressionComplexTypeBuilder.Create(idH, @operator, nameH)
                                                                            .WithInputAsFirstReference(leafInput)
                                                                            .AndInputAsSecondReference(leafInput));

            TriggerComplexType[] triggers = WrapTriggers(
                ExpressionComplexTypeBuilder.Create(idA, @operator, nameA)
                                            .WithInputAsFirstReference(leafInput)
                                            .AndInputAsSecondReference(leafInput),
                ExpressionComplexTypeBuilder.Create(idB, @operator, nameB)
                                            .WithInputAsFirstReference(leafInput)
                                            .AndInputAsSecondReference(leafInput),
                ExpressionComplexTypeBuilder.Create(idC, @operator, nameC)
                                            .WithInputAsFirstReference(nameA)
                                            .AndInputAsSecondReference(nameB),
                ExpressionComplexTypeBuilder.Create(idD, @operator, nameD)
                                            .WithInputAsFirstReference(leafInput)
                                            .AndInputAsSecondReference(leafInput),
                ExpressionComplexTypeBuilder.Create(idE, @operator, nameE)
                                            .WithInputAsFirstReference(nameF)
                                            .AndInputAsSecondReference(leafInput),
                condition1);

            RuleComplexType[] rules =
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(rules, triggers)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(8));
            ExpressionTree[] trees = dataAccessObjects.OfType<ExpressionTree>().ToArray();

            ExpressionTree treeC = AssertExpressionTree(trees, idC, ControlGroupName, nameC);
            BranchNode rootNode = AssertCorrectBranchNode(treeC.RootNode, nameC, @operator);
            AssertCorrectBranchNodeWithTwoLeafNodes(rootNode.FirstNode, nameA, @operator, leafInput, leafInput);
            AssertCorrectBranchNodeWithTwoLeafNodes(rootNode.SecondNode, nameB, @operator, leafInput, leafInput);

            ExpressionTree treeD = AssertExpressionTree(trees, idD, ControlGroupName, nameD);
            AssertCorrectBranchNodeWithTwoLeafNodes(treeD.RootNode, nameD, @operator, leafInput, leafInput);

            ExpressionTree treeE = AssertExpressionTree(trees, idE, ControlGroupName, nameE);
            AssertCorrectBranchNodeWithTwoLeafNodes(treeE.RootNode, nameE, @operator, nameF, leafInput);

            ExpressionTree treeF1 = AssertExpressionTree(trees, idF1, ControlGroupName, nameF);
            rootNode = AssertCorrectBranchNode(treeF1.RootNode, nameF, @operator);
            AssertCorrectLeafNode(rootNode.FirstNode, nameD);
            AssertCorrectBranchNodeWithTwoLeafNodes(rootNode.SecondNode, nameG, @operator, leafInput, leafInput);

            ExpressionTree treeF2 = AssertExpressionTree(trees, idF2, ControlGroupName, nameF);
            AssertCorrectBranchNodeWithTwoLeafNodes(treeF2.RootNode, nameF, @operator, nameD, leafInput);

            ExpressionTree treeH = AssertExpressionTree(trees, idH, ControlGroupName, nameH);
            AssertCorrectBranchNodeWithTwoLeafNodes(treeH.RootNode, nameH, @operator, leafInput, leafInput);
        }

        [TestCase(timeRelativeEnumStringType.ABSOLUTE, false,
                  interpolationOptionEnumStringType.LINEAR, InterpolationType.Linear)]
        [TestCase(timeRelativeEnumStringType.RELATIVE, true,
                  interpolationOptionEnumStringType.BLOCK, InterpolationType.Constant)]
        public void ConvertToDataAccessObjects_RelativeTimeRule_CorrectResultIsReturned(
            timeRelativeEnumStringType reference, bool expectedFromValue,
            interpolationOptionEnumStringType interpolationOption, InterpolationType expectedInterpolation)
        {
            // Setup
            RuleComplexType ruleElement = CreateRelativeTimeRuleElement(ControlGroupName, interpolationOption, reference);

            RuleComplexType[] ruleElements =
            {
                ruleElement
            };
            var triggerElements = new TriggerComplexType[]
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggerElements)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));
            var ruleObj = dataAccessObjects[0] as RuleDataAccessObject;
            Assert.That(ruleObj, Is.Not.Null);

            AssertRelativeTimeRuleValidity(ruleObj.Object, expectedFromValue, expectedInterpolation);
        }

        [TestCase(PIDRule.PIDRuleSetpointType.Constant, 7d)]
        [TestCase(PIDRule.PIDRuleSetpointType.TimeSeries, 0d)]
        [TestCase(PIDRule.PIDRuleSetpointType.Signal, 0d)]
        public void ConvertToDataAccessObjects_PidRule_CorrectResultIsReturned(
            PIDRule.PIDRuleSetpointType expectedSetpointType,
            object expectedConstantValue)
        {
            // Setup
            RuleComplexType ruleElement = CreatePidRuleElement(ControlGroupName, expectedSetpointType);

            RuleComplexType[] ruleElements =
            {
                ruleElement
            };
            var triggerElements = new TriggerComplexType[]
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggerElements)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));
            var ruleObj = dataAccessObjects[0] as RuleDataAccessObject;
            Assert.That(ruleObj, Is.Not.Null);
            AssertPidRuleValidity(ruleObj.Object, expectedSetpointType, expectedConstantValue);
        }

        [TestCase(ItemChoiceType6.settingMaxStep, Item1ChoiceType3.deadbandSetpointAbsolute,
                  IntervalRule.IntervalRuleIntervalType.Fixed, IntervalRule.IntervalRuleDeadBandType.Fixed, false)]
        [TestCase(ItemChoiceType6.settingMaxSpeed, Item1ChoiceType3.deadbandSetpointRelative,
                  IntervalRule.IntervalRuleIntervalType.Variable, IntervalRule.IntervalRuleDeadBandType.PercentageDischarge,
                  false)]
        [TestCase(ItemChoiceType6.settingMaxSpeed, Item1ChoiceType3.deadbandSetpointRelative,
                  IntervalRule.IntervalRuleIntervalType.Signal, IntervalRule.IntervalRuleDeadBandType.PercentageDischarge,
                  true)]
        public void ConvertToDataAccessObjects_IntervalRule_CorrectResultIsReturned(
            ItemChoiceType6 intervalType,
            Item1ChoiceType3 deadBandType,
            IntervalRule.IntervalRuleIntervalType expectedIntervalType,
            IntervalRule.IntervalRuleDeadBandType expectedDeadBandType,
            bool signalAsSetpoint)
        {
            // Setup
            RuleComplexType ruleElement =
                CreateIntervalRuleElement(ControlGroupName, signalAsSetpoint, intervalType, deadBandType);
            RuleComplexType[] ruleElements =
            {
                ruleElement
            };
            var triggerElements = new TriggerComplexType[]
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggerElements)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));
            var ruleObj = dataAccessObjects[0] as RuleDataAccessObject;
            Assert.That(ruleObj, Is.Not.Null);
            AssertIntervalRuleValidity(ruleObj.Object, expectedIntervalType, expectedDeadBandType);
        }

        [TestCase(interpolationOptionEnumStringType.LINEAR, interpolationOptionEnumStringType.LINEAR,
                  InterpolationHydraulicType.Linear, ExtrapolationHydraulicType.Linear)]
        [TestCase(interpolationOptionEnumStringType.BLOCK, interpolationOptionEnumStringType.BLOCK,
                  InterpolationHydraulicType.Constant, ExtrapolationHydraulicType.Constant)]
        public void ConvertToDataAccessObjects_HydraulicRule_CorrectResultIsReturned(
            interpolationOptionEnumStringType interpolation,
            interpolationOptionEnumStringType extrapolation,
            InterpolationHydraulicType expectedInterpolation,
            ExtrapolationHydraulicType expectedExtrapolation)
        {
            // Setup
            RuleComplexType ruleElement = CreateLookupTableRuleElement(RtcXmlTag.HydraulicRule, ControlGroupName, interpolation,
                                                                       extrapolation);

            RuleComplexType[] ruleElements =
            {
                ruleElement
            };
            var triggerElements = new TriggerComplexType[]
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggerElements)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));
            var ruleObj = dataAccessObjects[0] as RuleDataAccessObject;
            Assert.That(ruleObj, Is.Not.Null);
            AssertHydraulicRuleValidity(ruleObj.Object, expectedInterpolation, expectedExtrapolation);
        }

        [TestCase(interpolationOptionEnumStringType.LINEAR, interpolationOptionEnumStringType.LINEAR,
                  InterpolationHydraulicType.Linear, ExtrapolationHydraulicType.Linear)]
        [TestCase(interpolationOptionEnumStringType.BLOCK, interpolationOptionEnumStringType.BLOCK,
                  InterpolationHydraulicType.Constant, ExtrapolationHydraulicType.Constant)]
        public void ConvertToDataAccessObjects_FactorRule_CorrectResultIsReturned(
            interpolationOptionEnumStringType interpolation,
            interpolationOptionEnumStringType extrapolation,
            InterpolationHydraulicType expectedInterpolation,
            ExtrapolationHydraulicType expectedExtrapolation)
        {
            // Setup
            RuleComplexType ruleElement = CreateLookupTableRuleElement(
                RtcXmlTag.FactorRule, ControlGroupName, interpolation, extrapolation);

            RuleComplexType[] ruleElements =
            {
                ruleElement
            };
            var triggerElements = new TriggerComplexType[]
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggerElements)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));
            var ruleObj = dataAccessObjects[0] as RuleDataAccessObject;
            Assert.That(ruleObj, Is.Not.Null);

            AssertFactorRuleValidity(ruleObj.Object, expectedInterpolation, expectedExtrapolation);
        }

        [TestCase(RtcXmlTag.StandardCondition, typeof(StandardCondition), inputReferenceEnumStringType.EXPLICIT,
                  StandardCondition.ReferenceType.Explicit, relationalOperatorEnumStringType.Equal, Operation.Equal, 3.3)]
        [TestCase(RtcXmlTag.StandardCondition, typeof(StandardCondition), inputReferenceEnumStringType.IMPLICIT,
                  StandardCondition.ReferenceType.Implicit, relationalOperatorEnumStringType.Greater, Operation.Greater, 3.3)]
        [TestCase(RtcXmlTag.TimeCondition, typeof(TimeCondition), inputReferenceEnumStringType.EXPLICIT,
                  StandardCondition.ReferenceType.Explicit, relationalOperatorEnumStringType.GreaterEqual,
                  Operation.GreaterEqual, 3.3)]
        [TestCase(RtcXmlTag.TimeCondition, typeof(TimeCondition), inputReferenceEnumStringType.IMPLICIT,
                  StandardCondition.ReferenceType.Implicit, relationalOperatorEnumStringType.Less, Operation.Less, 3.3)]
        [TestCase(RtcXmlTag.DirectionalCondition, typeof(DirectionalCondition), inputReferenceEnumStringType.EXPLICIT,
                  StandardCondition.ReferenceType.Explicit, relationalOperatorEnumStringType.LessEqual, Operation.LessEqual,
                  0.0)]
        [TestCase(RtcXmlTag.DirectionalCondition, typeof(DirectionalCondition), inputReferenceEnumStringType.IMPLICIT,
                  StandardCondition.ReferenceType.Implicit, relationalOperatorEnumStringType.Unequal, Operation.Unequal, 0.0)]
        public void ConvertToDataAccessObjects_StandardCondition_CorrectResultIsReturned(
            string tag, Type expectedConditionType,
            inputReferenceEnumStringType reference, string expectedReference,
            relationalOperatorEnumStringType operatorType, Operation expectedOperation,
            double expectedValue)
        {
            // Setup
            TriggerComplexType conditionElement = CreateStandardConditionElement(
                tag, ControlGroupName, ComponentName, reference, operatorType);

            var ruleElements = new RuleComplexType[]
                {};
            TriggerComplexType[] triggerElements =
            {
                conditionElement
            };

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggerElements)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));
            var conditionObj = dataAccessObjects[0] as ConditionDataAccessObject;
            Assert.That(conditionObj, Is.Not.Null);
            AssertStandardConditionValidity(conditionObj.Object, expectedConditionType, expectedReference,
                                            expectedOperation, expectedValue);
        }

        [TestCase(true, 2)]
        [TestCase(false, 1)]
        public void ConvertToDataAccessObjects_StandardConditionWithOutput_CorrectResultIsReturned(
            bool hasOutput, int expectedNumberOfConditions)
        {
            // Setup
            TriggerComplexType conditionElement = CreateStandardConditionElement(
                RtcXmlTag.StandardCondition, ControlGroupName, ComponentName, hasOutput: hasOutput);

            var ruleElements = new RuleComplexType[]
                {};
            TriggerComplexType[] triggerElements =
            {
                conditionElement
            };

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggerElements)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(expectedNumberOfConditions));
        }

        [TestCase(interpolationOptionEnumStringType.LINEAR, interpolationOptionEnumStringType.LINEAR,
                  InterpolationHydraulicType.Linear, ExtrapolationHydraulicType.Linear)]
        [TestCase(interpolationOptionEnumStringType.BLOCK, interpolationOptionEnumStringType.BLOCK,
                  InterpolationHydraulicType.Constant, ExtrapolationHydraulicType.Constant)]
        public void
            GivenASignalElementAndAControlGroup_WhenCreateSignalsFromXmlElementsAndAddToControlGroupIsCalled_CorrectSignalIsCreatedAndAddedToControlGroup(
                interpolationOptionEnumStringType interpolation,
                interpolationOptionEnumStringType extrapolation,
                InterpolationHydraulicType expectedInterpolation,
                ExtrapolationHydraulicType expectedExtrapolation)
        {
            // Setup
            RuleComplexType signalElement = CreateLookupTableRuleElement(RtcXmlTag.LookupSignal, ControlGroupName,
                                                                         interpolation,
                                                                         extrapolation);
            RuleComplexType[] ruleElements =
            {
                signalElement
            };
            var triggerElements = new TriggerComplexType[]
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggerElements)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));
            var signalObj = dataAccessObjects[0] as SignalDataAccessObject;
            Assert.That(signalObj, Is.Not.Null);
            AssertSignalValidity(signalObj.Object, expectedInterpolation, expectedExtrapolation);
        }

        private static RuleComplexType CreateTimeRuleElement(string controlGroupName)
        {
            var timeRuleElement = new TimeAbsoluteComplexType {id = RtcXmlTag.TimeRule + controlGroupName + "/" + ComponentName};

            var ruleElement = new RuleComplexType {Item = timeRuleElement};

            return ruleElement;
        }

        private static RuleComplexType CreateRelativeTimeRuleElement(string controlGroupName,
                                                                     interpolationOptionEnumStringType interpolationOption =
                                                                         interpolationOptionEnumStringType.BLOCK,
                                                                     timeRelativeEnumStringType reference =
                                                                         timeRelativeEnumStringType.ABSOLUTE)
        {
            var timeRelativeRuleElement = new TimeRelativeComplexType
            {
                id = RtcXmlTag.RelativeTimeRule + controlGroupName + "/" + ComponentName,
                mode = TimeRelativeComplexTypeMode.RETAINVALUEWHENINACTIVE,
                valueOption = reference,
                maximumPeriod = 1,
                interpolationOption = interpolationOption,
                controlTable = new[]
                {
                    new TimeRelativeControlTableRecordComplexType
                    {
                        time = 60,
                        value = 10
                    },
                    new TimeRelativeControlTableRecordComplexType
                    {
                        time = 600,
                        value = 100
                    }
                }
            };

            var ruleElement = new RuleComplexType {Item = timeRelativeRuleElement};

            return ruleElement;
        }

        private static RuleComplexType CreatePidRuleElement(string controlGroupName,
                                                            PIDRule.PIDRuleSetpointType expectedSetpointType =
                                                                PIDRule.PIDRuleSetpointType.Constant)
        {
            var pidRuleElement = new PidComplexType
            {
                id = RtcXmlTag.PIDRule + controlGroupName + "/" + ComponentName,
                mode = PidComplexTypeMode.PIDVEL,
                settingMin = 1,
                settingMax = 2,
                settingMaxSpeed = 3,
                kp = 4,
                ki = 5,
                kd = 6,
                input = new InputPidComplexType()
            };

            if (expectedSetpointType == PIDRule.PIDRuleSetpointType.Constant)
            {
                pidRuleElement.input.Item = 7.0d;
            }
            else if (expectedSetpointType == PIDRule.PIDRuleSetpointType.TimeSeries)
            {
                pidRuleElement.input.Item = RtcXmlTag.SP + "something";
            }
            else
            {
                pidRuleElement.input.Item = RtcXmlTag.Signal + "something";
            }

            var ruleElement = new RuleComplexType {Item = pidRuleElement};

            return ruleElement;
        }

        private static RuleComplexType CreateIntervalRuleElement(string controlGroupName, bool signalAsSetpoint,
                                                                 ItemChoiceType6 intervalType = ItemChoiceType6.settingMaxSpeed,
                                                                 Item1ChoiceType3 deadBandType =
                                                                     Item1ChoiceType3.deadbandSetpointAbsolute)
        {
            var intervalRuleElement = new IntervalComplexType
            {
                id = RtcXmlTag.IntervalRule + controlGroupName + "/" + ComponentName,
                settingBelow = 1,
                settingAbove = 2,
                Item = 3, // interval
                ItemElementName = intervalType,
                Item1 = 4,
                Item1ElementName = deadBandType,
                input = new IntervalInputComplexType() {setpoint = RtcXmlTag.SP + controlGroupName + ComponentName}
            };

            if (signalAsSetpoint)
            {
                intervalRuleElement.input.setpoint = RtcXmlTag.Signal + controlGroupName + "signal1";
            }

            var ruleElement = new RuleComplexType {Item = intervalRuleElement};

            return ruleElement;
        }

        private static RuleComplexType CreateLookupTableRuleElement(string tag, string controlGroupName,
                                                                    interpolationOptionEnumStringType interpolation =
                                                                        interpolationOptionEnumStringType.BLOCK,
                                                                    interpolationOptionEnumStringType extrapolation =
                                                                        interpolationOptionEnumStringType.BLOCK)
        {
            var lookupTableRuleElement = new LookupTableComplexType
            {
                id = tag + controlGroupName + "/" + ComponentName,
                Item = new TableLookupTableComplexType
                {
                    record = new[]
                    {
                        new DateRecord2DataComplexType
                        {
                            x = 1,
                            y = 5
                        },
                        new DateRecord2DataComplexType
                        {
                            x = 2,
                            y = 4
                        }
                    }
                },
                interpolationOption = interpolation,
                extrapolationOption = extrapolation
            };

            var ruleElement = new RuleComplexType {Item = lookupTableRuleElement};

            return ruleElement;
        }

        private static TriggerComplexType CreateStandardConditionElement(string tag, string controlGroupName,
                                                                         string conditionName,
                                                                         inputReferenceEnumStringType referenceType = inputReferenceEnumStringType.EXPLICIT,
                                                                         relationalOperatorEnumStringType operatorType = relationalOperatorEnumStringType.Equal,
                                                                         bool hasOutput = false)
        {
            object item1;

            if (tag == RtcXmlTag.DirectionalCondition)
            {
                item1 = new RelationalConditionComplexTypeX2Series {@ref = inputReferenceEnumStringType.EXPLICIT};
            }

            else
            {
                item1 = "3.3";
            }

            var standardConditionElement = new StandardTriggerComplexType
            {
                id = tag + controlGroupName + "/" + conditionName,
                condition = new RelationalConditionComplexType
                {
                    Item = new RelationalConditionComplexTypeX1Series {@ref = referenceType},
                    relationalOperator = operatorType,
                    Item1 = item1
                }
            };

            if (hasOutput)
            {
                standardConditionElement.@true = new[]
                {
                    CreateStandardConditionElement(tag, controlGroupName, conditionName + ":true_output")
                };
            }

            var conditionElement = new TriggerComplexType {Item = standardConditionElement};

            return conditionElement;
        }

        private void AssertPidRuleValidity(RuleBase rule, PIDRule.PIDRuleSetpointType expectedSetpointType,
                                           object expectedConstantValue)
        {
            var pidRule = rule as PIDRule;
            Assert.NotNull(pidRule);

            Setting setting = pidRule.Setting;
            Assert.AreEqual(1d, setting.Min,
                            $"Pid rule: minimum settings was expected to be {1d}.");
            Assert.AreEqual(2d, setting.Max,
                            $"Pid rule: maximum settings was expected to be {2d}.");
            Assert.AreEqual(3d, setting.MaxSpeed,
                            $"Pid rule: maximum speed was expected to be {3d}.");
            Assert.AreEqual(4d, pidRule.Kp,
                            $"Pid rule: Kp was expected to be {4d}.");
            Assert.AreEqual(5d, pidRule.Ki,
                            $"Pid rule: Ki was expected to be {5d}.");
            Assert.AreEqual(6d, pidRule.Kd,
                            $"Pid rule: Kd was expected to be {6d}.");
            Assert.AreEqual(expectedSetpointType, pidRule.PidRuleSetpointType,
                            $"Pid rule: setpoint type was expected to be {expectedSetpointType.ToString()}.");
            Assert.AreEqual(expectedConstantValue, pidRule.ConstantValue,
                            $"Pid rule: constant value was expected to be {expectedConstantValue}.");
        }

        private void AssertRelativeTimeRuleValidity(RuleBase rule, bool expectedFromValue,
                                                    InterpolationType expectedInterpolation)
        {
            var relativeTimeRule = rule as RelativeTimeRule;
            Assert.NotNull(relativeTimeRule);
            Assert.AreEqual(expectedFromValue, relativeTimeRule.FromValue,
                            $"Relative time rule: from value was expected to be {expectedFromValue.ToString()}.");
            Assert.AreEqual(1, relativeTimeRule.MinimumPeriod,
                            $"Relative time rule: minimum period was expected to be 1.");
            Assert.AreEqual(expectedInterpolation, relativeTimeRule.Interpolation,
                            $"Relative time rule: interpolation was expected to be {expectedInterpolation.ToString()}.");

            Function function = relativeTimeRule.Function;
            Assert.AreEqual(2, function.Arguments[0].Values.Count);
            Assert.AreEqual(function[60d], 10d);
            Assert.AreEqual(function[600d], 100d);
        }

        private void AssertIntervalRuleValidity(RuleBase rule,
                                                IntervalRule.IntervalRuleIntervalType expectedIntervalType,
                                                IntervalRule.IntervalRuleDeadBandType expectedDeadBandType)
        {
            var intervalRule = rule as IntervalRule;
            Assert.NotNull(intervalRule);

            Setting setting = intervalRule.Setting;
            Assert.AreEqual(1d, setting.Below,
                            $"Interval rule: settings below was expected to be {1d}.");
            Assert.AreEqual(2d, setting.Above,
                            $"Interval rule: settings above was expected to be {2d}.");
            Assert.AreEqual(expectedIntervalType, intervalRule.IntervalType,
                            $"Interval rule: interval type was expected to be {expectedIntervalType.ToString()}.");
            Assert.AreEqual(expectedDeadBandType, intervalRule.DeadBandType,
                            $"Interval rule: dead band type was expected to be {expectedDeadBandType.ToString()}.");
            Assert.AreEqual(4d, intervalRule.DeadbandAroundSetpoint,
                            $"Interval rule: dead band around set point was expected to be {4d}.");

            var expectedIntervalValue = 3d;
            if (expectedIntervalType == IntervalRule.IntervalRuleIntervalType.Fixed)
            {
                Assert.AreEqual(expectedIntervalValue, intervalRule.FixedInterval,
                                $"Interval rule: fixed interval was expected to be {expectedIntervalValue}.");
                Assert.AreEqual(0.0d, setting.MaxSpeed,
                                $"Interval rule: maximum speed was expected to be {0d}.");
            }
            else
            {
                Assert.AreEqual(0.0d, intervalRule.FixedInterval,
                                $"Interval rule: fixed interval was expected to be {0d}.");
                Assert.AreEqual(expectedIntervalValue, setting.MaxSpeed,
                                $"Interval rule: maximum speed was expected to be {expectedIntervalValue}.");
            }
        }

        private void AssertSignalValidity(SignalBase signal, InterpolationHydraulicType expectedInterpolation,
                                          ExtrapolationHydraulicType expectedExtrapolation)
        {
            var lookupSignal = signal as LookupSignal;
            Assert.NotNull(lookupSignal);

            Function function = lookupSignal.Function;
            Assert.AreEqual(2, function.Arguments[0].Values.Count);
            Assert.AreEqual(function[1d], 5d);
            Assert.AreEqual(function[2d], 4d);
            Assert.AreEqual(expectedInterpolation.ToString(), lookupSignal.Interpolation.ToString(),
                            $"Signal: interpolation was expected to be {expectedInterpolation.ToString()}.");
            Assert.AreEqual(expectedExtrapolation.ToString(), lookupSignal.Extrapolation.ToString(),
                            $"Signal: extrapolation was expected to be {expectedExtrapolation.ToString()}.");
        }

        private void AssertHydraulicRuleValidity(RuleBase rule, InterpolationHydraulicType expectedInterpolation,
                                                 ExtrapolationHydraulicType expectedExtrapolation)
        {
            var hydraulicRule = rule as HydraulicRule;
            Assert.NotNull(hydraulicRule);

            Function function = hydraulicRule.Function;
            Assert.AreEqual(2, function.Arguments[0].Values.Count);
            Assert.AreEqual(function[1d], 5d);
            Assert.AreEqual(function[2d], 4d);
            Assert.AreEqual(expectedInterpolation.ToString(), hydraulicRule.Interpolation.ToString(),
                            $"Hydraulic rule: interpolation was expected to be {expectedInterpolation.ToString()}.");
            Assert.AreEqual(expectedExtrapolation.ToString(), hydraulicRule.Extrapolation.ToString(),
                            $"Hydraulic rule: extrapolation was expected to be {expectedExtrapolation.ToString()}.");
        }

        private void AssertFactorRuleValidity(RuleBase rule, InterpolationHydraulicType expectedInterpolation,
                                              ExtrapolationHydraulicType expectedExtrapolation)
        {
            var factorRule = rule as FactorRule;
            Assert.NotNull(factorRule);

            Function function = factorRule.Function;
            Assert.AreEqual(2, function.Arguments[0].Values.Count);
            Assert.AreEqual(function[-1d], 5d);
            Assert.AreEqual(function[1d], -5d);
            Assert.AreEqual(-5d, factorRule.Factor);
            Assert.AreEqual(expectedInterpolation.ToString(), factorRule.Interpolation.ToString(),
                            $"Factor rule: interpolation was expected to be {expectedInterpolation.ToString()}.");
            Assert.AreEqual(expectedExtrapolation.ToString(), factorRule.Extrapolation.ToString(),
                            $"Factor rule: extrapolation was expected to be {expectedExtrapolation.ToString()}.");
        }

        private void AssertStandardConditionValidity(ConditionBase condition, Type expectedConditionType,
                                                     string expectedReference,
                                                     Operation expectedOperation, double expectedValue)
        {
            var standardCondition = condition as StandardCondition;
            Assert.NotNull(standardCondition);
            Assert.AreEqual(expectedConditionType, standardCondition.GetType(),
                            $"Standard condition: condition type was expected to be {expectedConditionType}.");
            Assert.AreEqual(expectedReference, standardCondition.Reference,
                            $"Standard condition: reference was expected to be {expectedReference}.");
            Assert.AreEqual(expectedOperation, standardCondition.Operation,
                            $"Standard condition: operation was expected to be {expectedOperation.ToString()}.");
            Assert.AreEqual(expectedValue, standardCondition.Value,
                            $"Standard condition: value was expected to be {expectedValue}.");
        }

        [TestCaseSource(nameof(ExpressionTestCases))]
        public void ConvertToExpressionTrees_SingleExpression_ReturnsCorrectTree(ExpressionComplexType expressionXml,
                                                                                 ILeafNode expectedFirstInput,
                                                                                 ILeafNode expectedSecondInput,
                                                                                 Operator expectedOperator)
        {
            // Setup
            TriggerComplexType[] triggers = WrapTriggers(expressionXml);
            var ruleElements = new RuleComplexType[]
                {};
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(ruleElements, triggers)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));
            var tree = dataAccessObjects[0] as ExpressionTree;
            Assert.That(tree, Is.Not.Null,
                        $"The returned data access object should be a {nameof(ExpressionTree)}");

            const string expectedName = "expression_name";
            var expectedId = $"{ControlGroupName}/{expectedName}";

            Assert.That(tree.ControlGroupName, Is.EqualTo(ControlGroupName));
            Assert.That(tree.Id, Is.EqualTo(expectedId));

            IBranchNode rootNode = tree.RootNode;
            Assert.That(rootNode, Is.Not.Null,
                        "The root node of the expression should not be null.");
            Assert.That(rootNode.OperatorValue, Is.EqualTo(expectedOperator));

            Assert.That(rootNode.FirstNode, Is.EqualTo(expectedFirstInput)
                                              .Using(nodeComparer));
            Assert.That(rootNode.SecondNode, Is.EqualTo(expectedSecondInput)
                                               .Using(nodeComparer));
        }

        [TestCaseSource(nameof(ExpressionGroupsTestCases))]
        public void ConvertToExpressionTrees_TriggerXmlsFromDifferentExpressionGroups_ReturnsExpectedTrees(
            TriggerComplexType[] triggers, int expectedCount, string[] expectedIds)
        {
            // Setup
            RuleComplexType[] rules =
                {};

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(rules, triggers)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(expectedCount));
            expectedIds.ForEach(id => Assert.NotNull(dataAccessObjects.SingleOrDefault(t => t.Id == id)));
        }

        [TestCaseSource(nameof(GetTriggerObjects))]
        [Category(TestCategory.Integration)]
        public void ConvertToDataAccessObjects_TriggerReferencedTwice_ReturnsCorrectResult(object trigger)
        {
            // Setup
            var conditionA = (StandardTriggerComplexType) CreateStandardConditionElement(RtcXmlTag.StandardCondition,
                                                                                         ControlGroupName,
                                                                                         "condition_A").Item;

            var conditionB = (StandardTriggerComplexType) CreateStandardConditionElement(RtcXmlTag.StandardCondition,
                                                                                         ControlGroupName,
                                                                                         "condition_B").Item;

            ConditionXmlBuilder.Start(conditionA)
                               .WithTrueOutput(conditionB)
                               .WithFalseOutput(trigger);

            ConditionXmlBuilder.Start(conditionB)
                               .WithFalseOutput(trigger);

            TriggerComplexType[] triggers = WrapTriggers(conditionA);

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(new RuleComplexType[0], triggers)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(3));
            IEnumerable<string> ids = dataAccessObjects.Select(o => o.Id);
            Assert.That(ids, Is.Unique);
        }

        [TestCaseSource(nameof(GetTriggerObjects))]
        public void ConvertToDataAccessObjects_TriggerElementsWithDuplicateIds_ReturnsCorrectResult(object obj)
        {
            // Setup
            TriggerComplexType[] triggers = WrapTriggers(obj, obj);

            // Call
            IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects = RealTimeControlToolsConfigXmlConverter
                                                                      .ConvertToDataAccessObjects(new RuleComplexType[0], triggers)
                                                                      .ToArray();

            // Assert
            Assert.That(dataAccessObjects, Has.Length.EqualTo(1));
        }

        private static IEnumerable<object> GetTriggerObjects()
        {
            yield return (StandardTriggerComplexType) CreateStandardConditionElement(RtcXmlTag.StandardCondition,
                                                                                     ControlGroupName,
                                                                                     "condition").Item;

            yield return ExpressionComplexTypeBuilder.Create("expression_id", Operator.Add, "y")
                                                     .WithConstantAsFirstReference("1")
                                                     .AndConstantAsSecondReference("2");
        }

        private static IEnumerable<TestCaseData> ExpressionTestCases()
        {
            const string yName = "expression_name";
            var id = $"{ControlGroupName}/{yName}";

            var constantValue = random.NextDouble().ToString();
            var constantLeafNode = new ConstantValueLeafNode(constantValue);

            var inputReference = $"{RtcXmlTag.Input}some_input";
            var inputLeafNode = new ParameterLeafNode(inputReference);

            const string expressionReference = "some_expression";
            var expressionLeafNode = new ParameterLeafNode(expressionReference);

            foreach (Operator @operator in Enum.GetValues(typeof(Operator)))
            {
                ExpressionComplexType expressionXml1 = ExpressionComplexTypeBuilder.Create(id, @operator, yName)
                                                                                   .WithConstantAsFirstReference(constantValue)
                                                                                   .AndConstantAsSecondReference(constantValue);

                yield return new TestCaseData(expressionXml1, constantLeafNode, constantLeafNode, @operator);

                ExpressionComplexType expressionXml2 = ExpressionComplexTypeBuilder.Create(id, @operator, yName)
                                                                                   .WithConstantAsFirstReference(constantValue)
                                                                                   .AndInputAsSecondReference(inputReference);

                yield return new TestCaseData(expressionXml2, constantLeafNode, inputLeafNode, @operator);

                ExpressionComplexType expressionXml3 = ExpressionComplexTypeBuilder.Create(id, @operator, yName)
                                                                                   .WithConstantAsFirstReference(constantValue)
                                                                                   .AndInputAsSecondReference(expressionReference);

                yield return new TestCaseData(expressionXml3, constantLeafNode, expressionLeafNode, @operator);

                ExpressionComplexType expressionXml4 = ExpressionComplexTypeBuilder.Create(id, @operator, yName)
                                                                                   .WithInputAsFirstReference(inputReference)
                                                                                   .AndConstantAsSecondReference(constantValue);

                yield return new TestCaseData(expressionXml4, inputLeafNode, constantLeafNode, @operator);

                ExpressionComplexType expressionXml5 = ExpressionComplexTypeBuilder.Create(id, @operator, yName)
                                                                                   .WithInputAsFirstReference(inputReference)
                                                                                   .AndInputAsSecondReference(inputReference);

                yield return new TestCaseData(expressionXml5, inputLeafNode, inputLeafNode, @operator);

                ExpressionComplexType expressionXml6 = ExpressionComplexTypeBuilder.Create(id, @operator, yName)
                                                                                   .WithInputAsFirstReference(inputReference)
                                                                                   .AndInputAsSecondReference(expressionReference);

                yield return new TestCaseData(expressionXml6, inputLeafNode, expressionLeafNode, @operator);

                ExpressionComplexType expressionXml7 = ExpressionComplexTypeBuilder.Create(id, @operator, yName)
                                                                                   .WithInputAsFirstReference(expressionReference)
                                                                                   .AndConstantAsSecondReference(constantValue);

                yield return new TestCaseData(expressionXml7, expressionLeafNode, constantLeafNode, @operator);

                ExpressionComplexType expressionXml8 = ExpressionComplexTypeBuilder.Create(id, @operator, yName)
                                                                                   .WithInputAsFirstReference(expressionReference)
                                                                                   .AndInputAsSecondReference(inputReference);

                yield return new TestCaseData(expressionXml8, expressionLeafNode, inputLeafNode, @operator);

                ExpressionComplexType expressionXml9 = ExpressionComplexTypeBuilder.Create(id, @operator, yName)
                                                                                   .WithInputAsFirstReference(expressionReference)
                                                                                   .AndInputAsSecondReference(expressionReference);

                yield return new TestCaseData(expressionXml9, expressionLeafNode, expressionLeafNode, @operator);
            }
        }

        private static IEnumerable<TestCaseData> ExpressionGroupsTestCases()
        {
            const string nameA = "A";
            const string nameB = "B";
            const string nameC = "C";
            const string nameD = "D";

            var idA = $"{ControlGroupName}/{nameA}";
            var idB = $"{ControlGroupName}/{nameB}";
            var idC = $"{ControlGroupName}/{nameC}";
            var idD = $"{ControlGroupName}/{nameD}";

            IList<ExpressionComplexType> expressions =
                RetrieveExampleSetExpressionElements(idA, nameA, idB, nameB, idC, nameC, idD, nameD);
            TriggerComplexType[] triggers = WrapTriggers(expressions);
            string[] expectedIds =
            {
                idA
            };

            //      O      ---> 1 root expressions
            //     / \   
            //    O   O   ---> 2 sub-expressions
            //   / \ / \ 
            //  *   O   * ---> 1 shared sub-expression for sub-expressions and 2 leaf inputs
            //     / \
            //    *  *    ---> 2 leaf inputs
            yield return new TestCaseData(triggers, 2, expectedIds).SetName("Same control group, same level.");

            var condition = (StandardTriggerComplexType) CreateStandardConditionElement(RtcXmlTag.StandardCondition,
                                                                                        ControlGroupName,
                                                                                        "condition1").Item;
            ConditionXmlBuilder.Start(condition)
                               .WithTrueOutput(expressions[1])
                               .WithFalseOutput(expressions[2]);

            triggers = WrapTriggers(expressions[0], expressions[3], condition);
            expectedIds = new[]
            {
                idA,
                idB,
                idC,
                idD
            };

            yield return new TestCaseData(triggers, 5, expectedIds).SetName("Same control group, different levels.");

            idA = $"Group1/{nameA}";
            idB = $"Group2/{nameB}";
            idC = $"Group3/{nameC}";
            idD = $"Group4/{nameD}";

            expressions = RetrieveExampleSetExpressionElements(idA, nameA, idB, nameB, idC, nameC, idD, nameD);
            triggers = WrapTriggers(expressions);
            expectedIds = new[]
            {
                idA,
                idB,
                idC,
                idD
            };

            yield return new TestCaseData(triggers, 4, expectedIds).SetName("Different control groups, same level.");
        }

        private static IList<ExpressionComplexType> RetrieveExampleSetExpressionElements(string idA, string nameA,
                                                                                         string idB, string nameB,
                                                                                         string idC, string nameC,
                                                                                         string idD, string nameD)
        {
            var leafInput = $"{RtcXmlTag.Input}some_input";
            var @operator = Operator.Add; // TODO random

            return new List<ExpressionComplexType>
            {
                ExpressionComplexTypeBuilder.Create(idA, @operator, nameA)
                                            .WithInputAsFirstReference(nameB)
                                            .AndInputAsSecondReference(nameC),
                ExpressionComplexTypeBuilder.Create(idB, @operator, nameB)
                                            .WithInputAsFirstReference(leafInput)
                                            .AndInputAsSecondReference(nameD),
                ExpressionComplexTypeBuilder.Create(idC, @operator, nameC)
                                            .WithInputAsFirstReference(nameD)
                                            .AndInputAsSecondReference(leafInput),
                ExpressionComplexTypeBuilder.Create(idD, @operator, nameD)
                                            .WithInputAsFirstReference(leafInput)
                                            .AndInputAsSecondReference(leafInput)
            };
        }

        private static void AssertCorrectBranchNodeWithTwoLeafNodes(IExpressionNode node, string branchNodeName,
                                                                    Operator @operator, string firstLeafValue,
                                                                    string secondLeafValue)
        {
            BranchNode branchNode = AssertCorrectBranchNode(node, branchNodeName, @operator);
            AssertCorrectLeafNode(branchNode.FirstNode, firstLeafValue);
            AssertCorrectLeafNode(branchNode.SecondNode, secondLeafValue);
        }

        private static ExpressionTree AssertExpressionTree(ExpressionTree[] result, string id, string controlGroup,
                                                           string name)
        {
            ExpressionTree tree = result.FirstOrDefault(t => t.Id == id);

            Assert.That(tree, Is.Not.Null);
            Assert.That(tree.ControlGroupName, Is.EqualTo(controlGroup));

            return tree;
        }

        private static BranchNode AssertCorrectBranchNode(IExpressionNode node, string yName, Operator @operator)
        {
            var branchNode = node as BranchNode;
            Assert.That(branchNode, Is.Not.Null);
            Assert.That(branchNode.YName, Is.EqualTo(yName));
            Assert.That(branchNode.OperatorValue, Is.EqualTo(@operator));

            return branchNode;
        }

        private static void AssertCorrectLeafNode(IExpressionNode node, string leafValue)
        {
            var leafNode = node as ParameterLeafNode;
            Assert.That(leafNode, Is.Not.Null);
            Assert.That(leafNode.Value, Is.EqualTo(leafValue));
        }

        private static TriggerComplexType WrapTrigger(object obj)
        {
            return new TriggerComplexType {Item = obj};
        }

        private static TriggerComplexType[] WrapTriggers(params object[] objects)
        {
            return objects.Select(WrapTrigger).ToArray();
        }

        private static TriggerComplexType[] WrapTriggers(IEnumerable<object> objects)
        {
            return objects.Select(WrapTrigger).ToArray();
        }

        private class ConditionXmlBuilder
        {
            private readonly StandardTriggerComplexType standardTriggerXml;

            private ConditionXmlBuilder(StandardTriggerComplexType standardTriggerXml)
            {
                this.standardTriggerXml = standardTriggerXml;
            }

            public static ConditionXmlBuilder Start(StandardTriggerComplexType standardTriggerXml)
            {
                return new ConditionXmlBuilder(standardTriggerXml);
            }

            public ConditionXmlBuilder WithTrueOutput(object trueOutput)
            {
                standardTriggerXml.@true = (standardTriggerXml.@true ?? new TriggerComplexType[0]).Concat(new[]
                {
                    WrapTrigger(trueOutput)
                }).ToArray();
                return this;
            }

            public ConditionXmlBuilder WithFalseOutput(object falseOutput)
            {
                standardTriggerXml.@false = (standardTriggerXml.@false ?? new TriggerComplexType[0]).Concat(new[]
                {
                    WrapTrigger(falseOutput)
                }).ToArray();
                return this;
            }
        }
    }
}