using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// <see cref="ExpressionTreeAssembler"/> implements the method to assemble
    /// a collection of <see cref="ExpressionTree"/> from a collection of
    /// <see cref="ExpressionObject"/> and a control group name.
    /// </summary>
    public class ExpressionTreeAssembler
    {
        private readonly IDictionary<string, NodeReference> nodeReferenceMapping = new Dictionary<string, NodeReference>();
        private readonly string controlGroupName;
        private readonly ExpressionObject[] expressionObjects;
        private readonly HashSet<string> expressionYNames;
        private readonly HashSet<string> objectInputReferences;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTreeAssembler"/> class.
        /// </summary>
        /// <param name="controlGroupName"> The control group name. </param>
        /// <param name="expressionObjects"> The expression objects for this control group. </param>
        /// <param name="rules"> All the rule data access objects for this control group. </param>
        /// <param name="conditions"> All the condition data access objects for this control group. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="expressionObjects"/>, <paramref name="rules"/> or <paramref name="conditions"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="controlGroupName"/> is <c>null</c> or empty.
        /// </exception>
        public ExpressionTreeAssembler(string controlGroupName,
                                       ExpressionObject[] expressionObjects,
                                       RuleDataAccessObject[] rules,
                                       ConditionDataAccessObject[] conditions)
        {
            Ensure.NotNullOrEmpty(controlGroupName, nameof(controlGroupName));
            Ensure.NotNull(rules, nameof(rules));
            Ensure.NotNull(conditions, nameof(conditions));
            Ensure.NotNull(expressionObjects, nameof(expressionObjects));

            this.controlGroupName = controlGroupName;
            this.expressionObjects = expressionObjects;

            expressionYNames = new HashSet<string>(expressionObjects.Select(e => e.Y));
            objectInputReferences = GetInputReferences(rules, conditions);
        }
        
        /// <summary>
        /// Assembles all the expression objects into a collection of <see cref="ExpressionTree"/>.
        /// Each <see cref="ExpressionTree"/> represents a mathematical expression.
        /// Each <see cref="ParameterLeafNode"/> in the tree corresponds with an <see cref="IInput"/>.
        /// </summary>
        /// <returns>
        /// The collection of <see cref="ExpressionTree"/> which could be
        /// constructed from the expression objects.
        /// </returns>
        public IEnumerable<ExpressionTree> Assemble()
        {
            var createdBranchNodes = new Dictionary<string, BranchNode>();

            foreach (ExpressionObject obj in expressionObjects)
            {
                createdBranchNodes[obj.Id] = CreateBranchNode(obj);
            }

            foreach (KeyValuePair<string, BranchNode> branchNodeKvp in createdBranchNodes)
            {
                BranchNode branchNode = branchNodeKvp.Value;
                if (nodeReferenceMapping.TryGetValue(branchNode.YName, out NodeReference nodeReference))
                {
                    nodeReference.Node = branchNode;
                }
                else
                {
                    string id = branchNodeKvp.Key;
                    var expression = new MathematicalExpression {Name = branchNode.YName};
                    yield return new ExpressionTree(branchNode, controlGroupName, id, expression);
                }
            }
        }

        private BranchNode CreateBranchNode(ExpressionObject expressionObject)
        {
            var branchNode = new BranchNode(expressionObject.Operator, expressionObject.Y);

            HashSet<string> expressionReferences = GetExpressionReferences(expressionObjects.Except(expressionObject));
            AddLeafNode(expressionReferences, expressionObject.FirstReference, branchNode.FirstNodeReference);
            AddLeafNode(expressionReferences, expressionObject.SecondReference, branchNode.SecondNodeReference);

            return branchNode;
        }

        private void AddLeafNode(HashSet<string> expressionReferences, IExpressionReference expressionReference, NodeReference nodeReference)
        {
            if (TryCreateLeafNode(expressionReference,
                                  expressionReferences,
                                  out ILeafNode leafNode))
            {
                nodeReference.Node = leafNode;
            }
            else
            {
                nodeReferenceMapping[expressionReference.Value] = nodeReference;
            }
        }

        private bool TryCreateLeafNode(IExpressionReference reference,
                                       ICollection<string> expressionReferences,
                                       out ILeafNode leafNode)
        {
            leafNode = null;
            if (reference is ParameterLeafReference)
            {
                leafNode = new ParameterLeafNode(reference.Value);
                return true;
            }
            else if (reference is ConstantLeafReference)
            {
                leafNode = new ConstantValueLeafNode(reference.Value);
                return true;
            }

            if (reference is ExpressionReference)
            {
                string expressionYName = reference.Value;
                
                if (ShouldNotBeASubExpression(expressionReferences, expressionYName)) 
                {
                    leafNode = new ParameterLeafNode(expressionYName);
                    return true;
                }
            }

            return false;
        }

        private bool ShouldNotBeASubExpression(ICollection<string> expressionReferences, string expressionYName)
        {
            // Can only create a sub expression (BranchNode) when:
            // - There is an expression object that represents the referenced Y; 
            // - There is no other expression that uses this Y as input.
            // - There is no rule or condition that uses this Y as input.
            // When any of these criteria are *not* met, a ParameterLeafNode should be created,
            // indicating that the referenced Y should not be a sub expression, but should be the root of a new Mathematical Expression.
            return NoExpressionWithYName(expressionYName) ||
                   ExpressionIsReferenceByMultipleExpressions(expressionReferences, expressionYName) ||
                   ExpressionIsReferenceByOtherObjects(expressionYName);
        }

        private bool ExpressionIsReferenceByOtherObjects(string expressionYName) => objectInputReferences.Contains(expressionYName);

        private static bool ExpressionIsReferenceByMultipleExpressions(ICollection<string> expressionReferences, string expressionYName)
            => expressionReferences.Contains(expressionYName);

        private bool NoExpressionWithYName(string expressionYName) => !expressionYNames.Contains(expressionYName);

        /// <summary>
        /// Gets all the expression references (referencing Y names) of all the expression objects.
        /// </summary>
        private static HashSet<string> GetExpressionReferences(IEnumerable<ExpressionObject> expressionObjects)
        {
            var expressionReferences = new HashSet<string>();

            foreach (ExpressionObject expressionObject in expressionObjects)
            {
                if (expressionObject.FirstReference is ExpressionReference expressionReference1)
                {
                    expressionReferences.Add(expressionReference1.Value);
                }

                if (expressionObject.SecondReference is ExpressionReference expressionReference2)
                {
                    expressionReferences.Add(expressionReference2.Value);
                }
            }

            return expressionReferences;
        }

        /// <summary>
        /// Gets all the input references for the rules and conditions.
        /// The references include inputs and mathematical expressions.
        /// </summary>
        private static HashSet<string> GetInputReferences(IEnumerable<RuleDataAccessObject> rules,
                                                          IEnumerable<ConditionDataAccessObject> conditions)
        {
            var objectInputReferences = new HashSet<string>();

            foreach (string inputReference in rules.SelectMany(r => r.InputReferences))
            {
                objectInputReferences.Add(inputReference);
            }

            foreach (string inputReference in conditions.SelectMany(r => r.InputReferences))
            {
                objectInputReferences.Add(inputReference);
            }

            return objectInputReferences;
        }
    }
}