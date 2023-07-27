using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using DHYDRO.Common.Logging;
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

            RuleDataAccessObject[] rules = ConvertToRuleDataAccessObjects(ruleElements, logHandler).Where(r => r != null).ToArray();
            ConditionDataAccessObject[] conditions = ConvertToConditionDataAccessObjects(triggerElements, logHandler).Where(c => c != null).ToArray();
            SignalDataAccessObject[] signals = ConvertToSignalDataAccessObjects(signalElements).ToArray();
            ExpressionTree[] expressions = ConvertToExpressionTrees(triggerElements, rules, conditions).ToArray();

            return rules
                   .Concat<IRtcDataAccessObject<RtcBaseObject>>(conditions)
                   .Concat(signals)
                   .Concat(expressions)
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

        private static IEnumerable<ExpressionTree> ConvertToExpressionTrees(IEnumerable<TriggerComplexType> triggerElements, 
                                                                            RuleDataAccessObject[] rules, 
                                                                            ConditionDataAccessObject[] conditions)
        {
            IEnumerable<ExpressionObjectGroup> expressionGroups = GetExpressionGroupsRecursively(triggerElements);
            return AssembleExpressionTrees(expressionGroups, rules, conditions).RemoveDuplicateIds();
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

        private static IEnumerable<ExpressionTree> AssembleExpressionTrees(IEnumerable<ExpressionObjectGroup> expressionGroups, 
                                                                           RuleDataAccessObject[] rules, 
                                                                           ConditionDataAccessObject[] conditions)
        {
            var expressionTrees = new List<ExpressionTree>();

            Dictionary<string, IEnumerable<RuleDataAccessObject>> rulesPerControlGroup = rules.ToGroupedDictionary(r => r.ControlGroupName);
            Dictionary<string, IEnumerable<ConditionDataAccessObject>> conditionsPerControlGroup = conditions.ToGroupedDictionary(c => c.ControlGroupName);

            foreach (ExpressionObjectGroup expressionGroup in expressionGroups)
            {
                IEnumerable<IGrouping<string, ExpressionObject>> expressionsGroupedByControlGroup = expressionGroup.GroupBy(e => e.ControlGroupName);

                foreach (IGrouping<string, ExpressionObject> group in expressionsGroupedByControlGroup)
                {
                    ExpressionObject[] expressionObjects = group.ToArray();
                    string controlGroupName = group.Key;

                    RuleDataAccessObject[] rulesInControlGroup = GetCollection(rulesPerControlGroup, controlGroupName).ToArray();
                    ConditionDataAccessObject[] conditionsInControlGroup = GetCollection(conditionsPerControlGroup, controlGroupName).ToArray();

                    var assembler = new ExpressionTreeAssembler(controlGroupName, expressionObjects, rulesInControlGroup, conditionsInControlGroup);
                    IEnumerable<ExpressionTree> expressionTreesInControlGroup = assembler.Assemble();

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

        /// <summary>
        /// Groups the source by the provided <paramref name="keySelector"/> and
        /// creates a dictionary with the key and the corresponding grouped items.
        /// </summary>
        /// <param name="source"> The source elements. </param>
        /// <param name="keySelector"> The key selector. </param>
        /// <typeparam name="TKey"> The type of the key. </typeparam>
        /// <typeparam name="TValue"> The type of the elements. </typeparam>
        /// <returns>
        /// A dictionary with grouped items by their corresponding key.
        /// </returns>
        private static Dictionary<TKey, IEnumerable<TValue>> ToGroupedDictionary<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelector)
        {
            return source.GroupBy(keySelector).ToDictionary(r => r.Key, r => r.AsEnumerable());
        }

        /// <summary>
        /// Safely gets a collection from the <paramref name="dictionary"/>.
        /// </summary>
        /// <param name="dictionary"> The dictionary. </param>
        /// <param name="key"> THe key. </param>
        /// <typeparam name="TKey"> The type of the key. </typeparam>
        /// <typeparam name="TValue"> The type of the elements in the collection. </typeparam>
        /// <returns>
        /// The corresponding collection if the dictionary contains the given key; otherwise, an empty collection.
        /// </returns>
        private static IEnumerable<TValue> GetCollection<TKey, TValue>(this IDictionary<TKey, IEnumerable<TValue>> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out IEnumerable<TValue> items)
                       ? items
                       : Enumerable.Empty<TValue>();
        }
    }
}