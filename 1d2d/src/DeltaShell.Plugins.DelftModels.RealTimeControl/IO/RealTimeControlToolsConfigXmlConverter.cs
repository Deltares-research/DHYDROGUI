using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using ExpressionObjectGroup = System.Collections.Generic.List<DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess.ExpressionObject>;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// Responsible for taking the objects that come from the tools config xml file
    /// and converting them into a set of <see cref="IRtcDataAccessObject{T}"/>.
    /// </summary>
    public static class RealTimeControlToolsConfigXmlConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="ruleElements"/>
        /// and <paramref name="triggerElements"/> to rtc data access objects.
        /// </summary>
        /// <param name="ruleElements"> The rule xml elements. </param>
        /// <param name="triggerElements"> The trigger xml elements. </param>
        /// <param name="logHandler"> The log handler. </param>
        /// <returns> A collection of <see cref="IRtcDataAccessObject{T}"/>. </returns>
        public static IEnumerable<IRtcDataAccessObject<RtcBaseObject>> ConvertToDataAccessObjects(RuleComplexType[] ruleElements,
                                                                                                  TriggerComplexType[] triggerElements,
                                                                                                  ILogHandler logHandler = null)
        {
            RuleComplexType[] signalElements = ruleElements.Where(IsSignal).ToArray();
            ruleElements = ruleElements.Except(signalElements).ToArray();

            return ConvertToRuleDataAccessObjects(ruleElements, logHandler)
                   .Concat<IRtcDataAccessObject<RtcBaseObject>>(ConvertToConditionDataAccessObjects(triggerElements, logHandler))
                   .Concat(ConvertToSignalDataAccessObjects(signalElements))
                   .Concat(ConvertToExpressionTrees(triggerElements))
                   .Where(o => o != null);
        }

        private static IEnumerable<RuleDataAccessObject> ConvertToRuleDataAccessObjects(IEnumerable<RuleComplexType> ruleElements,
                                                                                        ILogHandler logHandler)
        {
            return ruleElements.Select(r => RuleDataAccessObjectCreator.Create(r, logHandler));
        }

        private static IEnumerable<ConditionDataAccessObject> ConvertToConditionDataAccessObjects(IEnumerable<TriggerComplexType> triggerElements,
                                                                                                  ILogHandler logHandler = null)
        {
            return triggerElements.SelectMany(e => ConvertToConditionDataAccessObjects(e, logHandler)).RemoveDuplicateIds();
        }

        private static IEnumerable<ConditionDataAccessObject> ConvertToConditionDataAccessObjects(TriggerComplexType triggerXml,
                                                                                                  ILogHandler logHandler = null)
        {
            IList<ConditionDataAccessObject> dataAccessObjects = new List<ConditionDataAccessObject>();

            if (triggerXml.Item is StandardTriggerComplexType conditionElement)
            {
                dataAccessObjects.Add(ConditionDataAccessObjectCreator.Create(conditionElement, logHandler));

                TriggerComplexType[] conditionElementTrue = conditionElement.@true ?? new TriggerComplexType[0];
                TriggerComplexType[] conditionElementFalse = conditionElement.@false ?? new TriggerComplexType[0];

                IEnumerable<TriggerComplexType> outputItems = conditionElementTrue.Concat(conditionElementFalse);
                foreach (TriggerComplexType outputItem in outputItems)
                {
                    dataAccessObjects.AddRange(ConvertToConditionDataAccessObjects(outputItem, logHandler));
                }

                return dataAccessObjects;
            }

            return dataAccessObjects;
        }

        private static IEnumerable<SignalDataAccessObject> ConvertToSignalDataAccessObjects(IEnumerable<RuleComplexType> signalElements)
        {
            return signalElements.Select(SignalDataAccessObjectCreator.Create);
        }

        private static IEnumerable<ExpressionTree> ConvertToExpressionTrees(IEnumerable<TriggerComplexType> triggerElements)
        {
            IEnumerable<ExpressionObjectGroup> expressionGroups = GetExpressionGroupsRecursively(triggerElements);
            return AssembleExpressionTrees(expressionGroups).RemoveDuplicateIds();
        }

        private static IEnumerable<ExpressionObjectGroup> GetExpressionGroupsRecursively(IEnumerable<TriggerComplexType> triggerElements)
        {
            if (triggerElements == null)
            {
                return new List<ExpressionObjectGroup>();
            }

            var expressionGroups = new List<ExpressionObjectGroup>();
            var expressionGroup = new ExpressionObjectGroup();

            foreach (TriggerComplexType triggerElement in triggerElements)
            {
                switch (triggerElement.Item)
                {
                    case StandardTriggerComplexType conditionElement:
                        expressionGroups.AddRange(GetExpressionGroupsRecursively(conditionElement.@true));
                        expressionGroups.AddRange(GetExpressionGroupsRecursively(conditionElement.@false));
                        break;
                    case ExpressionComplexType expressionElement:
                        expressionGroup.Add(new ExpressionObject(expressionElement));
                        break;
                }
            }

            if (expressionGroup.Any())
            {
                expressionGroups.Add(expressionGroup);
            }

            return expressionGroups;
        }

        private static IEnumerable<ExpressionTree> AssembleExpressionTrees(IEnumerable<ExpressionObjectGroup> expressionGroups)
        {
            var expressionTrees = new List<ExpressionTree>();

            foreach (ExpressionObjectGroup expressionGroup in expressionGroups)
            {
                IEnumerable<IGrouping<string, ExpressionObject>> expressionsGroupedByControlGroup = expressionGroup.GroupBy(e => e.ControlGroupName);

                foreach (IGrouping<string, ExpressionObject> group in expressionsGroupedByControlGroup)
                {
                    ExpressionObject[] expressionObjects = group.ToArray();
                    string controlGroupName = group.Key;

                    IEnumerable<ExpressionTree> expressionTreesInControlGroup =
                        ExpressionTreeAssembler.Assemble(expressionObjects, controlGroupName);

                    expressionTrees.AddRange(expressionTreesInControlGroup);
                }
            }

            return expressionTrees;
        }

        private static bool IsSignal(RuleComplexType ruleElement)
        {
            object item = ruleElement.Item;
            if (item is LookupTableComplexType lookupTableElement)
            {
                string id = lookupTableElement.id;
                string tag = RealTimeControlXmlReaderHelper.GetTagFromElementId(id);

                if (tag == RtcXmlTag.LookupSignal)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<T> RemoveDuplicateIds<T>(this IEnumerable<T> source) where T : IRtcDataAccessObject<RtcBaseObject>
        {
            return source.GroupBy(o => o?.Id)
                         .Select(g => g.First());
        }
    }
}