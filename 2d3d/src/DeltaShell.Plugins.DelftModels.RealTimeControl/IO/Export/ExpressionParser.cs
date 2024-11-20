using System;
using System.Globalization;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using MathematicalExpressionParser.Core.ExpressionParser;
using MathematicalExpressionParser.Core.ExpressionTree;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Parses mathematical expression to a binary tree.
    /// </summary>
    public static class ExpressionParser
    {
        /// <summary>
        /// Parses the
        /// <param name="expressionStr"/>
        /// into a binary expression tree.
        /// </summary>
        /// <param name="expressionStr">The expression string.</param>
        /// <param name="rootNode">
        /// When this method returns, if the parsing was successful, the root node of the expression tree;
        /// otherwise <c>null</c>. This parameter is passed uninitialized.
        /// </param>
        /// <param name="errorMsg">
        /// When this method returns, if the parsing was unsuccessful, the error message;
        /// otherwise <c>null</c>. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the parsing operation was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The type of the parsing result was not recognized as either a Failure or a Success.
        /// </exception>
        public static bool TryParse(string expressionStr, out IExpressionNode rootNode, out string errorMsg)
        {
            rootNode = null;
            errorMsg = null;

            ParserResult result = Parser.parseExpression(expressionStr);

            if (result is ParserResult.Success success)
            {
                rootNode = Convert(success.Item);
                return true;
            }

            if (result is ParserResult.Failure failure)
            {
                errorMsg = failure.Item;
                return false;
            }

            throw new NotSupportedException($"The parsing of type {result.GetType()} was not recognized as either a Failure or a Success.");
        }

        private static IExpressionNode Convert(ExpressionType t)
        {
            switch (t)
            {
                case ExpressionType.AtomicExpression atom:
                    return ToNode(atom.Item);
                case ExpressionType.ComposedExpression comp:
                    return ToNode(comp.Item);
                default:
                    throw new NotSupportedException();
            }
        }

        private static IExpressionNode ToNode(ComposedExpressionType t)
        {
            return new BranchNode(ToOperator(t.@operator))
            {
                FirstNode = Convert(t.firstOperand),
                SecondNode = Convert(t.secondOperand)
            };
        }

        private static IExpressionNode ToNode(AtomicExpressionType t)
        {
            switch (t)
            {
                case AtomicExpressionType.Constant constant:
                    return new ConstantValueLeafNode(constant.Item.ToString(CultureInfo.InvariantCulture));
                case AtomicExpressionType.Parameter parameter:
                    return new ParameterLeafNode(parameter.Item);
                default:
                    throw new NotSupportedException();
            }
        }

        private static Operator ToOperator(ExpressionOperatorType t)
        {
            switch (t)
            {
                case ExpressionOperatorType.InfixFunction infixFunc
                    when infixFunc.Item == InfixFunctionType.Addition:
                    return Operator.Add;
                case ExpressionOperatorType.InfixFunction infixFunc
                    when infixFunc.Item == InfixFunctionType.Subtraction:
                    return Operator.Subtract;
                case ExpressionOperatorType.InfixFunction infixFunc
                    when infixFunc.Item == InfixFunctionType.Multiplication:
                    return Operator.Multiply;
                case ExpressionOperatorType.InfixFunction infixFunc
                    when infixFunc.Item == InfixFunctionType.Division:
                    return Operator.Divide;
                case ExpressionOperatorType.BinaryFunction binFunc
                    when binFunc.Item == BinaryFunctionType.Min:
                    return Operator.Min;
                case ExpressionOperatorType.BinaryFunction binFunc
                    when binFunc.Item == BinaryFunctionType.Max:
                    return Operator.Max;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}