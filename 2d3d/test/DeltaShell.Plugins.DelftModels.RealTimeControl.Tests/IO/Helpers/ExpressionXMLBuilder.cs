using System;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Helpers
{
    /// <summary>
    /// Helper for building a <see cref="ExpressionComplexType"/>
    /// </summary>
    public class ExpressionComplexTypeBuilder
    {
        private readonly ExpressionComplexType expressionXml;

        private ExpressionComplexTypeBuilder(string id, Operator @operator, string yValue)
        {
            expressionXml = new ExpressionComplexType
            {
                id = id,
                mathematicalOperator = ConvertToXmlOperationType(@operator),
                Item2 = yValue
            };
        }

        /// <summary>
        /// Creates the specified identifier.
        /// </summary>
        /// <param name="id"> The id. </param>
        /// <param name="operator"> The operator. </param>
        /// <param name="yValue"> The y-value name. </param>
        /// <returns> A new expression xml builder. </returns>
        public static ExpressionComplexTypeBuilder Create(string id, Operator @operator, string yValue)
        {
            return new ExpressionComplexTypeBuilder(id, @operator, yValue);
        }

        /// <summary>
        /// Builds the first reference value with a constant value.
        /// </summary>
        /// <param name="value"> The constant value. </param>
        /// <returns> This expression xml builder. </returns>
        public ExpressionComplexTypeBuilder WithConstantAsFirstReference(string value)
        {
            expressionXml.Item = value;
            return this;
        }

        /// <summary>
        /// Builds the first reference value with a reference to an input (input, expression)
        /// </summary>
        /// <param name="reference"> The referenced input name. </param>
        /// <returns> This expression xml builder. </returns>
        public ExpressionComplexTypeBuilder WithInputAsFirstReference(string reference)
        {
            expressionXml.Item = new ExpressionComplexTypeX1Series {Value = reference};
            return this;
        }

        /// <summary>
        /// Builds the second reference value with a constant value.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The built expression xml. </returns>
        public ExpressionComplexType AndConstantAsSecondReference(string value)
        {
            expressionXml.Item1 = value;
            return expressionXml;
        }

        /// <summary>
        /// Builds the first reference value with a reference to an input (input, expression)
        /// </summary>
        /// <param name="reference"> The referenced input name. </param>
        /// <returns> The built expression xml. </returns>
        public ExpressionComplexType AndInputAsSecondReference(string reference)
        {
            expressionXml.Item1 = new ExpressionComplexTypeX2Series {Value = reference};
            return expressionXml;
        }

        private MathematicalOperatorEnumStringType ConvertToXmlOperationType(Operator @operator)
        {
            switch (@operator)
            {
                case Operator.Add:
                    return MathematicalOperatorEnumStringType.Item;
                case Operator.Subtract:
                    return MathematicalOperatorEnumStringType.Item1;
                case Operator.Multiply:
                    return MathematicalOperatorEnumStringType.Item2;
                case Operator.Divide:
                    return MathematicalOperatorEnumStringType.Item3;
                case Operator.Min:
                    return MathematicalOperatorEnumStringType.min;
                case Operator.Max:
                    return MathematicalOperatorEnumStringType.max;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null);
            }
        }
    }
}