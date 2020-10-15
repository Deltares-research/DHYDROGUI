using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class ConditionDataAccessObjectCreatorTest
    {
        private static readonly Random random = new Random();

        [Test]
        public void Create_StandardTriggerXmlNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ConditionDataAccessObjectCreator.Create(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("standardTriggerXml"));
        }

        [TestCaseSource(nameof(GetCreateTestCases))]
        public void Create_ReturnCorrectResult(StandardTriggerComplexType standardTriggerXml,
                                               ConditionDataAccessObject expectedResult)
        {
            // Call
            ConditionDataAccessObject result = ConditionDataAccessObjectCreator.Create(standardTriggerXml);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult).Using(new ConditionDataAccessObjectComparer()),
                        "Expected the results to be equal to the expected results:");
        }

        private static IEnumerable<TestCaseData> GetCreateTestCases()
        {
            foreach (Operation operation in Enum.GetValues(typeof(Operation)))
            {
                yield return GetStandardConditionWithImplicitReferenceTestCase(operation);
                yield return GetStandardConditionWithExplicitReferenceTestCase(operation);
                yield return GetTimeConditionWithImplicitReferenceTestCase(operation);
                yield return GetTimeConditionWithExplicitReferenceTestCase(operation);
                yield return GetDirectionalConditionWithImplicitReferenceTestCase(operation);
                yield return GetDirectionalConditionWithExplicitReferenceTestCase(operation);
            }
        }

        private static TestCaseData GetStandardConditionWithImplicitReferenceTestCase(Operation operation)
        {
            const string reference = StandardCondition.ReferenceType.Implicit;
            const string inputReference = "input_ref";

            StandardTriggerComplexType standardTriggerXml = GetArgumentAndExpectedStandardConditionDataAccessObject(
                operation, reference, inputReference,
                out ConditionDataAccessObject expectedConditionDataAccessObject);

            return new TestCaseData(standardTriggerXml, expectedConditionDataAccessObject);
        }

        private static TestCaseData GetStandardConditionWithExplicitReferenceTestCase(Operation operation)
        {
            const string reference = StandardCondition.ReferenceType.Explicit;
            const string inputReference = "input_ref";

            StandardTriggerComplexType standardTriggerXml = GetArgumentAndExpectedStandardConditionDataAccessObject(
                operation, reference, inputReference,
                out ConditionDataAccessObject expectedConditionDataAccessObject);

            expectedConditionDataAccessObject.InputReferences.Add(inputReference);

            return new TestCaseData(standardTriggerXml, expectedConditionDataAccessObject);
        }

        private static TestCaseData GetTimeConditionWithImplicitReferenceTestCase(Operation operation)
        {
            string reference = StandardCondition.ReferenceType.Implicit;
            const string inputReference = "input_ref";

            StandardTriggerComplexType standardTriggerXml = GetArgumentAndExpectedTimeConditionDataAccessObject(
                operation, reference, inputReference,
                out ConditionDataAccessObject expectedConditionDataAccessObject);

            return new TestCaseData(standardTriggerXml, expectedConditionDataAccessObject);
        }

        private static TestCaseData GetTimeConditionWithExplicitReferenceTestCase(Operation operation)
        {
            string reference = StandardCondition.ReferenceType.Explicit;
            const string inputReference = "input_ref";

            StandardTriggerComplexType standardTriggerXml = GetArgumentAndExpectedTimeConditionDataAccessObject(
                operation, reference, inputReference,
                out ConditionDataAccessObject expectedConditionDataAccessObject);

            expectedConditionDataAccessObject.InputReferences.Add(inputReference);

            return new TestCaseData(standardTriggerXml, expectedConditionDataAccessObject);
        }

        private static TestCaseData GetDirectionalConditionWithImplicitReferenceTestCase(Operation operation)
        {
            const string reference = StandardCondition.ReferenceType.Implicit;
            const string inputReference = "input_ref";

            StandardTriggerComplexType standardTriggerXml = GetArgumentAndExpectedDirectionalConditionDataAccessObject(
                operation, reference, inputReference,
                out ConditionDataAccessObject expectedConditionDataAccessObject);

            return new TestCaseData(standardTriggerXml, expectedConditionDataAccessObject);
        }

        private static TestCaseData GetDirectionalConditionWithExplicitReferenceTestCase(Operation operation)
        {
            const string reference = StandardCondition.ReferenceType.Explicit;
            const string inputReference = "input_ref";

            StandardTriggerComplexType standardTriggerXml = GetArgumentAndExpectedDirectionalConditionDataAccessObject(
                operation, reference, inputReference,
                out ConditionDataAccessObject expectedConditionDataAccessObject);

            expectedConditionDataAccessObject.InputReferences.Add(inputReference);

            return new TestCaseData(standardTriggerXml, expectedConditionDataAccessObject);
        }

        private static StandardTriggerComplexType GetArgumentAndExpectedStandardConditionDataAccessObject(
            Operation operation, string reference, string inputRef,
            out ConditionDataAccessObject expectedConditionDataAccessObject)
        {
            const string controlGroupName = "control_group_name";
            const string conditionName = "condition_name";
            const string id = RtcXmlTag.StandardCondition + controlGroupName + "/" + conditionName;

            var value = (double) random.Next();

            const string trueRuleRef = "true_rule_ref";
            const string trueConditionRef = "true_cond_ref";
            const string trueExpressionRef = "true_expr_ref";

            const string falseRuleRef = "false_rule_ref";
            const string falseConditionRef = "false_cond_ref";
            const string falseExpressionRef = "false_expr_ref";

            TriggerComplexType[] trueOutputs =
            {
                GetStandardTriggerXml(trueConditionRef),
                GetRuleReferenceTriggerXml(trueRuleRef),
                GetExpressionXml(trueExpressionRef)
            };

            TriggerComplexType[] falseOutputs =
            {
                GetStandardTriggerXml(falseConditionRef),
                GetRuleReferenceTriggerXml(falseRuleRef),
                GetExpressionXml(falseExpressionRef)
            };

            StandardTriggerComplexType standardTriggerXml =
                CreateStandardConditionXml(id, value, operation, reference, inputRef, trueOutputs, falseOutputs);

            string[] trueOutputRefs =
            {
                trueConditionRef,
                trueRuleRef,
                trueExpressionRef
            };
            string[] falseOutputRefs =
            {
                falseConditionRef,
                falseRuleRef,
                falseExpressionRef
            };
            expectedConditionDataAccessObject = CreateStandardConditionDataAccessObject(
                id, conditionName,
                reference, operation, value,
                trueOutputRefs, falseOutputRefs);

            return standardTriggerXml;
        }

        private static StandardTriggerComplexType GetArgumentAndExpectedTimeConditionDataAccessObject(
            Operation operation, string reference, string inputRef,
            out ConditionDataAccessObject expectedConditionDataAccessObject)
        {
            const string controlGroupName = "control_group_name";
            const string conditionName = "condition_name";
            const string id = RtcXmlTag.TimeCondition + controlGroupName + "/" + conditionName;

            var value = (double) random.Next();

            const string trueRuleRef = "true_rule_ref";
            const string trueConditionRef = "true_cond_ref";
            const string trueExpressionRef = "true_expr_ref";

            const string falseRuleRef = "false_rule_ref";
            const string falseConditionRef = "false_cond_ref";
            const string falseExpressionRef = "false_expr_ref";

            TriggerComplexType[] trueOutputs =
            {
                GetStandardTriggerXml(trueConditionRef),
                GetRuleReferenceTriggerXml(trueRuleRef),
                GetExpressionXml(trueExpressionRef)
            };

            TriggerComplexType[] falseOutputs =
            {
                GetStandardTriggerXml(falseConditionRef),
                GetRuleReferenceTriggerXml(falseRuleRef),
                GetExpressionXml(falseExpressionRef)
            };

            StandardTriggerComplexType standardTriggerXml =
                CreateStandardConditionXml(id, value, operation, reference, inputRef, trueOutputs, falseOutputs);

            string[] trueOutputRefs =
            {
                trueConditionRef,
                trueRuleRef,
                trueExpressionRef
            };
            string[] falseOutputRefs =
            {
                falseConditionRef,
                falseRuleRef,
                falseExpressionRef
            };
            expectedConditionDataAccessObject = CreateTimeConditionDataAccessObject(
                id, conditionName,
                reference, operation, value,
                trueOutputRefs, falseOutputRefs);

            return standardTriggerXml;
        }

        private static StandardTriggerComplexType GetArgumentAndExpectedDirectionalConditionDataAccessObject(
            Operation operation, string reference, string inputRef,
            out ConditionDataAccessObject expectedConditionDataAccessObject)
        {
            const string controlGroupName = "control_group_name";
            const string conditionName = "condition_name";
            const string id = RtcXmlTag.DirectionalCondition + controlGroupName + "/" + conditionName;

            const string trueRuleRef = "true_rule_ref";
            const string trueConditionRef = "true_cond_ref";
            const string trueExpressionRef = "true_expr_ref";

            const string falseRuleRef = "false_rule_ref";
            const string falseConditionRef = "false_cond_ref";
            const string falseExpressionRef = "false_expr_ref";

            TriggerComplexType[] trueOutputs =
            {
                GetStandardTriggerXml(trueConditionRef),
                GetRuleReferenceTriggerXml(trueRuleRef),
                GetExpressionXml(trueExpressionRef)
            };

            TriggerComplexType[] falseOutputs =
            {
                GetStandardTriggerXml(falseConditionRef),
                GetRuleReferenceTriggerXml(falseRuleRef),
                GetExpressionXml(falseExpressionRef)
            };

            StandardTriggerComplexType standardTriggerXml = CreateDirectionalConditionXml(id, operation,
                                                                                          reference, inputRef,
                                                                                          trueOutputs, falseOutputs);

            string[] trueOutputRefs =
            {
                trueConditionRef,
                trueRuleRef,
                trueExpressionRef
            };
            string[] falseOutputRefs =
            {
                falseConditionRef,
                falseRuleRef,
                falseExpressionRef
            };
            expectedConditionDataAccessObject = CreateDirectionalConditionDataAccessObject(
                id, conditionName,
                reference, operation,
                trueOutputRefs, falseOutputRefs);

            return standardTriggerXml;
        }

        private static ConditionDataAccessObject CreateStandardConditionDataAccessObject(
            string id, string conditionName,
            string reference, Operation operation, double value,
            string[] trueRefs, string[] falseRefs)
        {
            var standardCondition = new StandardCondition
            {
                Name = conditionName,
                Reference = reference,
                Operation = operation,
                Value = value
            };
            var expectedConditionDataAccessObject = new ConditionDataAccessObject(id, standardCondition);
            expectedConditionDataAccessObject.TrueOutputReferences.AddRange(trueRefs);
            expectedConditionDataAccessObject.FalseOutputReferences.AddRange(falseRefs);

            return expectedConditionDataAccessObject;
        }

        private static ConditionDataAccessObject CreateTimeConditionDataAccessObject(
            string id, string conditionName,
            string reference, Operation operation, double value,
            string[] trueRefs, string[] falseRefs)
        {
            var timeCondition = new TimeCondition
            {
                Name = conditionName,
                Reference = reference,
                Operation = operation,
                Value = value
            };

            var expectedConditionDataAccessObject = new ConditionDataAccessObject(id, timeCondition);
            expectedConditionDataAccessObject.TrueOutputReferences.AddRange(trueRefs);
            expectedConditionDataAccessObject.FalseOutputReferences.AddRange(falseRefs);

            return expectedConditionDataAccessObject;
        }

        private static ConditionDataAccessObject CreateDirectionalConditionDataAccessObject(
            string id, string conditionName,
            string reference, Operation operation,
            string[] trueRefs, string[] falseRefs)
        {
            var standardCondition = new DirectionalCondition
            {
                Name = conditionName,
                Reference = reference,
                Operation = operation
            };
            var expectedConditionDataAccessObject = new ConditionDataAccessObject(id, standardCondition);
            expectedConditionDataAccessObject.TrueOutputReferences.AddRange(trueRefs);
            expectedConditionDataAccessObject.FalseOutputReferences.AddRange(falseRefs);

            return expectedConditionDataAccessObject;
        }

        private static StandardTriggerComplexType CreateDirectionalConditionXml(string id, Operation operation,
                                                                                string reference, string inputReference,
                                                                                TriggerComplexType[] trueTriggers, TriggerComplexType[] falseTriggers)
        {
            var condition = new RelationalConditionComplexType
            {
                Item = new RelationalConditionComplexTypeX1Series
                {
                    @ref = GetXmlReferenceType(reference),
                    Value = inputReference
                },
                relationalOperator = GetXmlOperation(operation),
                Item1 = new RelationalConditionComplexTypeX2Series {@ref = inputReferenceEnumStringType.EXPLICIT}
            };

            return new StandardTriggerComplexType
            {
                id = id,
                @true = trueTriggers,
                @false = falseTriggers,
                condition = condition
            };
        }

        private static StandardTriggerComplexType CreateStandardConditionXml(string id, double value, Operation operation,
                                                                             string reference, string inputReference,
                                                                             TriggerComplexType[] trueTriggers, TriggerComplexType[] falseTriggers)
        {
            var condition = new RelationalConditionComplexType
            {
                Item = new RelationalConditionComplexTypeX1Series
                {
                    @ref = GetXmlReferenceType(reference),
                    Value = inputReference
                },
                relationalOperator = GetXmlOperation(operation),
                Item1 = value.ToString()
            };

            return new StandardTriggerComplexType
            {
                id = id,
                @true = trueTriggers,
                @false = falseTriggers,
                condition = condition
            };
        }

        private static TriggerComplexType GetRuleReferenceTriggerXml(string id)
        {
            return new TriggerComplexType {Item = id};
        }

        private static TriggerComplexType GetStandardTriggerXml(string id)
        {
            return new TriggerComplexType {Item = new StandardTriggerComplexType {id = id}};
        }

        private static TriggerComplexType GetExpressionXml(string id)
        {
            return new TriggerComplexType {Item = new ExpressionComplexType {id = id}};
        }

        private static inputReferenceEnumStringType GetXmlReferenceType(string reference)
        {
            return reference == StandardCondition.ReferenceType.Explicit
                       ? inputReferenceEnumStringType.EXPLICIT
                       : inputReferenceEnumStringType.IMPLICIT;
        }

        private static relationalOperatorEnumStringType GetXmlOperation(Operation operation)
        {
            switch (operation)
            {
                case Operation.Equal:
                    return relationalOperatorEnumStringType.Equal;
                case Operation.Unequal:
                    return relationalOperatorEnumStringType.Unequal;
                case Operation.Less:
                    return relationalOperatorEnumStringType.Less;
                case Operation.LessEqual:
                    return relationalOperatorEnumStringType.LessEqual;
                case Operation.Greater:
                    return relationalOperatorEnumStringType.Greater;
                case Operation.GreaterEqual:
                    return relationalOperatorEnumStringType.GreaterEqual;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }
        }

        private class ConditionDataAccessObjectComparer : IEqualityComparer<ConditionDataAccessObject>
        {
            public bool Equals(ConditionDataAccessObject x, ConditionDataAccessObject y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                return Equals(x.ControlGroupName, y.ControlGroupName) &&
                       Equals(x.Id, y.Id) &&
                       x.InputReferences.SequenceEqual(y.InputReferences) &&
                       x.TrueOutputReferences.SequenceEqual(y.TrueOutputReferences) &&
                       x.FalseOutputReferences.SequenceEqual(y.FalseOutputReferences) &&
                       EqualsCondition(x.Object, y.Object);
            }

            public int GetHashCode(ConditionDataAccessObject obj)
            {
                return Tuple.Create(obj.Id, obj.ControlGroupName).GetHashCode() ^ obj.Object.Value.GetHashCode();
            }

            private static bool EqualsCondition(ConditionBase x, ConditionBase y)
            {
                var xStandardCondition = x as StandardCondition;
                var yStandardCondition = y as StandardCondition;

                if (xStandardCondition == null || yStandardCondition == null)
                {
                    return false;
                }

                return xStandardCondition.GetType() == yStandardCondition.GetType() &&
                       Equals(xStandardCondition.Name, yStandardCondition.Name) &&
                       Equals(xStandardCondition.Reference, yStandardCondition.Reference) &&
                       Equals(xStandardCondition.Operation, yStandardCondition.Operation) &&
                       Equals(xStandardCondition.Value, yStandardCondition.Value);
            }
        }
    }
}