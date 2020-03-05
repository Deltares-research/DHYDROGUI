using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>
    /// Responsible for taking the objects that come from the tools config xml file
    /// and converting them into a set of <see cref="IRtcDataAccessObject{RtcBaseObject}"/>.
    /// </summary>
    public static class RealTimeControlToolsConfigXmlConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="ruleElements"/>
        /// and <paramref name="triggerElements"/> to rtc data access objects.
        /// </summary>
        /// <param name="ruleElements">The rule xml elements.</param>
        /// <param name="triggerElements">The trigger xml elements.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <returns></returns>
        public static IEnumerable<IRtcDataAccessObject<RtcBaseObject>> ConvertToDataAccessObjects(RuleXML[] ruleElements,
                                                                                                  TriggerXML[] triggerElements,
                                                                                                  ILogHandler logHandler = null)
        {
            RuleXML[] signalElements = ruleElements.Where(IsSignal).ToArray();
            ruleElements = ruleElements.Except(signalElements).ToArray();

            return ConvertToRuleDataAccessObjects(ruleElements, logHandler)
                   .Concat<IRtcDataAccessObject<RtcBaseObject>>(ConvertToConditionDataAccessObjects(triggerElements, logHandler))
                   .Concat(ConvertToSignalDataAccessObjects(signalElements))
                   .Concat(ConvertToExpressionTrees(triggerElements))
                   .Where(o => o != null);
        }

        private static IEnumerable<RuleDataAccessObject> ConvertToRuleDataAccessObjects(IEnumerable<RuleXML> ruleElements,
                                                                                        ILogHandler logHandler)
        {
            return ruleElements.Select(r => RuleDataAccessObjectCreator.Create(r, logHandler));
        }

        private static IEnumerable<ConditionDataAccessObject> ConvertToConditionDataAccessObjects(IEnumerable<TriggerXML> triggerElements,
                                                                                                  ILogHandler logHandler = null)
        {
            return triggerElements.SelectMany(e => ConvertToConditionDataAccessObjects(e, logHandler));
        }

        private static IEnumerable<ConditionDataAccessObject> ConvertToConditionDataAccessObjects(TriggerXML triggerXml,
                                                                                                  ILogHandler logHandler = null)
        {
            IList<ConditionDataAccessObject> dataAccessObjects = new List<ConditionDataAccessObject>();

            if (triggerXml.Item is StandardTriggerXML conditionElement)
            {
                dataAccessObjects.Add(ConditionDataAccessObjectCreator.Create(conditionElement, logHandler));

                IEnumerable<TriggerXML> outputItems = conditionElement.@true
                                                                      .Concat(conditionElement.@false);
                foreach (TriggerXML outputItem in outputItems)
                {
                    dataAccessObjects.AddRange(ConvertToConditionDataAccessObjects(outputItem, logHandler));
                }

                return dataAccessObjects;
            }

            return dataAccessObjects;
        }

        private static IEnumerable<SignalDataAccessObject> ConvertToSignalDataAccessObjects(IEnumerable<RuleXML> signalElements)
        {
            return signalElements.Select(e => SignalDataAccessObjectCreator.Create(e));
        }

        private static IEnumerable<ExpressionTree> ConvertToExpressionTrees(IEnumerable<TriggerXML> triggerElements)
        {
            IEnumerable<ExpressionObjectGroup> expressionGroups = GetExpressionGroupsRecursively(triggerElements);
            return AssembleExpressionTrees(expressionGroups);
        }

        private static IEnumerable<ExpressionObjectGroup> GetExpressionGroupsRecursively(IEnumerable<TriggerXML> triggerElements)
        {
            var expressionGroups = new List<ExpressionObjectGroup>();
            var expressionGroup = new ExpressionObjectGroup();

            foreach (TriggerXML triggerElement in triggerElements)
            {
                switch (triggerElement.Item)
                {
                    case StandardTriggerXML conditionElement:
                        expressionGroups.AddRange(GetExpressionGroupsRecursively(conditionElement.@true));
                        expressionGroups.AddRange(GetExpressionGroupsRecursively(conditionElement.@false));
                        break;
                    case ExpressionXML expressionElement:
                        expressionGroup.Add(new ExpressionObject(expressionElement));
                        break;
                }
            }

            if (expressionGroup.ExpressionObjects.Any())
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
                IEnumerable<IGrouping<string, ExpressionObject>> expressionsGroupedByControlGroup =
                    expressionGroup.ExpressionObjects.GroupBy(e => e.ControlGroupName);

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

        private static bool IsSignal(RuleXML ruleElement)
        {
            object item = ruleElement.Item;
            if (item is LookupTableXML lookupTableElement)
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
    }
}
