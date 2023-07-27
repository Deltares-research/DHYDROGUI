using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Guards;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Creates a <see cref="ConditionDataAccessObject"/> based of a <see cref="StandardTriggerComplexType"/>.
    /// </summary>
    public static class ConditionDataAccessObjectCreator
    {
        private static readonly IDictionary<string, Type> conditions = new Dictionary<string, Type>
        {
            {RtcXmlTag.StandardCondition, typeof(StandardCondition)},
            {RtcXmlTag.TimeCondition, typeof(TimeCondition)},
            {RtcXmlTag.DirectionalCondition, typeof(DirectionalCondition)}
        };

        /// <summary>
        /// Creates a <see cref="ConditionDataAccessObject"/> from the specified <paramref name="standardTriggerXml"/>.
        /// </summary>
        /// <param name="standardTriggerXml"> The standard trigger. </param>
        /// <param name="logHandler"> The log handler. </param>
        /// <returns>
        /// A <see cref="ConditionDataAccessObject"/> created from the specified <paramref name="standardTriggerXml"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="standardTriggerXml"/> is <c>null</c>.
        /// </exception>
        public static ConditionDataAccessObject Create(StandardTriggerComplexType standardTriggerXml,
                                                       ILogHandler logHandler = null)
        {
            Ensure.NotNull(standardTriggerXml, nameof(standardTriggerXml));

            StandardCondition condition = CreateCondition(standardTriggerXml, logHandler);
            if (condition == null)
            {
                return null;
            }

            var conditionDataAccessObject = new ConditionDataAccessObject(standardTriggerXml.id, condition);

            if (condition.Reference == StandardCondition.ReferenceType.Explicit &&
                standardTriggerXml.condition.Item is RelationalConditionComplexTypeX1Series seriesItem)
            {
                conditionDataAccessObject.InputReferences.Add(seriesItem.Value);
            }

            if (standardTriggerXml.@true != null)
            {
                IEnumerable<object> trueOutputs = standardTriggerXml.@true.Select(to => to.Item);
                conditionDataAccessObject.TrueOutputReferences.AddRange(GetReferences(trueOutputs));
            }

            if (standardTriggerXml.@false != null)
            {
                IEnumerable<object> falseOutputs = standardTriggerXml.@false.Select(to => to.Item);
                conditionDataAccessObject.FalseOutputReferences.AddRange(GetReferences(falseOutputs));
            }

            return conditionDataAccessObject;
        }

        private static IEnumerable<string> GetReferences(IEnumerable<object> outputItems)
        {
            foreach (object outputItem in outputItems)
            {
                switch (outputItem)
                {
                    case string ruleId:
                        yield return ruleId;
                        break;
                    case StandardTriggerComplexType conditionElement:
                        yield return conditionElement.id;
                        break;
                    case ExpressionComplexType expressionElement:
                        yield return expressionElement.id;
                        break;
                }
            }
        }

        private static StandardCondition CreateCondition(StandardTriggerComplexType standardConditionElement,
                                                         ILogHandler logHandler)
        {
            string id = standardConditionElement.id;
            string tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);
            if (tag == null)
            {
                logHandler?.ReportWarning($"Condition with id '{id}' does not contain a condition tag and will be skipped.");
                return null;
            }

            StandardCondition condition;
            if (conditions.TryGetValue(tag, out Type type))
            {
                condition = (StandardCondition) Activator.CreateInstance(type);
            }
            else
            {
                return null;
            }

            condition.Name = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(standardConditionElement.id);

            RelationalConditionComplexType conditionElement = standardConditionElement.condition;
            inputReferenceEnumStringType? referenceElementValue =
                (conditionElement.Item as RelationalConditionComplexTypeX1Series)?.@ref;
            string reference = referenceElementValue == inputReferenceEnumStringType.EXPLICIT
                                   ? StandardCondition.ReferenceType.Explicit
                                   : StandardCondition.ReferenceType.Implicit;

            relationalOperatorEnumStringType operatorElementValue = conditionElement.relationalOperator;
            Operation operation = GetOperationFromXmlObject(operatorElementValue);

            double value = conditionElement.Item1 is string valueElementValue
                               ? double.Parse(valueElementValue, CultureInfo.InvariantCulture)
                               : 0.0d;

            condition.Reference = reference;
            condition.Operation = operation;
            condition.Value = value;

            return condition;
        }

        private static Operation GetOperationFromXmlObject(relationalOperatorEnumStringType relationalOperator)
        {
            Operation operation;

            switch (relationalOperator)
            {
                case relationalOperatorEnumStringType.Equal:
                    operation = Operation.Equal;
                    break;
                case relationalOperatorEnumStringType.Greater:
                    operation = Operation.Greater;
                    break;
                case relationalOperatorEnumStringType.GreaterEqual:
                    operation = Operation.GreaterEqual;
                    break;
                case relationalOperatorEnumStringType.Less:
                    operation = Operation.Less;
                    break;
                case relationalOperatorEnumStringType.LessEqual:
                    operation = Operation.LessEqual;
                    break;
                case relationalOperatorEnumStringType.Unequal:
                    operation = Operation.Unequal;
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            return operation;
        }
    }
}