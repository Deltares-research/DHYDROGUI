using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// <see cref="ExpressionTreeAssembler"/> implements the method to assemble
    /// a collection of <see cref="ExpressionTree"/> from a collection of
    /// <see cref="ExpressionObject"/> and a control group name.
    /// </summary>
    public static class ExpressionTreeAssembler
    {
        /// <summary>
        /// Assembles the specified <see cref="ExpressionObject"/> into a
        /// collection of <see cref="ExpressionTree"/>.
        /// </summary>
        /// <param name="expressionObjects">The expression objects.</param>
        /// <param name="controlGroupName">Name of the control group.</param>
        /// <returns>
        /// The collection of <see cref="ExpressionTree"/> which could be
        /// constructed from the <paramref name="expressionObjects"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public static IEnumerable<ExpressionTree> Assemble(IEnumerable<ExpressionObject> expressionObjects,
                                                           string controlGroupName)
        {
            Ensure.NotNull(expressionObjects, nameof(expressionObjects));
            Ensure.NotNull(controlGroupName, nameof(controlGroupName));

            IList<ExpressionObject> expressionObjectList = expressionObjects.ToList();

            var nodeReferenceMapping = new Dictionary<string, NodeReference>();
            var createdBranchNodes = new Dictionary<string, BranchNode>();

            foreach (ExpressionObject obj in expressionObjectList)
            {
                createdBranchNodes[obj.Id] = CreateBranchNode(obj, expressionObjectList, nodeReferenceMapping);
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

        private static BranchNode CreateBranchNode(ExpressionObject expressionObject,
                                                   IList<ExpressionObject> expressionObjects,
                                                   IDictionary<string, NodeReference> nodeReferenceMapping)
        {
            Operator @operator = expressionObject.Operator;

            var branchNode = new BranchNode(@operator, expressionObject.Y);

            NodeReference firstNodeReference = branchNode.FirstNodeReference;
            if (TryCreateLeafNode(expressionObject.FirstReference,
                                  expressionObjects,
                                  out ILeafNode firstLeafNode))
            {
                firstNodeReference.Node = firstLeafNode;
            }
            else
            {
                nodeReferenceMapping[expressionObject.FirstReference.Value] = firstNodeReference;
            }

            NodeReference secondNodeReference = branchNode.SecondNodeReference;
            if (TryCreateLeafNode(expressionObject.SecondReference,
                                  expressionObjects,
                                  out ILeafNode secondLeafNode))
            {
                secondNodeReference.Node = secondLeafNode;
            }
            else
            {
                nodeReferenceMapping[expressionObject.SecondReference.Value] = secondNodeReference;
            }

            return branchNode;
        }

        private static bool TryCreateLeafNode(IExpressionReference reference,
                                              IEnumerable<ExpressionObject> expressionObjects,
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
                if (!expressionObjects.Any(e => e.Y == expressionYName))
                {
                    leafNode = new ParameterLeafNode(expressionYName);
                    return true;
                }
            }

            return false;
        }
    }
}