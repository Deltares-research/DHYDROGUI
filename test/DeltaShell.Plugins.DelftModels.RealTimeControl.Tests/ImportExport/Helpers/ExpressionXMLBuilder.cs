using System;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport.Helpers
{
    /// <summary>
    /// Helper for building a <see cref="ExpressionXML"/>
    /// </summary>
    public class ExpressionXMLBuilder
    {
        private readonly ExpressionXML expressionXml;

        private ExpressionXMLBuilder(string id, Operator @operator, string yValue)
        {
            expressionXml = new ExpressionXML
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
        public static ExpressionXMLBuilder Create(string id, Operator @operator, string yValue)
        {
            return new ExpressionXMLBuilder(id, @operator, yValue);
        }

        /// <summary>
        /// Builds the first reference value with a constant value.
        /// </summary>
        /// <param name="value"> The constant value. </param>
        /// <returns> This expression xml builder. </returns>
        public ExpressionXMLBuilder WithConstantAsFirstReference(string value)
        {
            expressionXml.Item = value;
            return this;
        }

        /// <summary>
        /// Builds the first reference value with a reference to an input (input, expression)
        /// </summary>
        /// <param name="reference"> The referenced input name. </param>
        /// <returns> This expression xml builder. </returns>
        public ExpressionXMLBuilder WithInputAsFirstReference(string reference)
        {
            expressionXml.Item = new ExpressionXMLX1Series {Value = reference};
            return this;
        }

        /// <summary>
        /// Builds the second reference value with a constant value.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The built expression xml. </returns>
        public ExpressionXML AndConstantAsSecondReference(string value)
        {
            expressionXml.Item1 = value;
            return expressionXml;
        }

        /// <summary>
        /// Builds the first reference value with a reference to an input (input, expression)
        /// </summary>
        /// <param name="reference"> The referenced input name. </param>
        /// <returns> The built expression xml. </returns>
        public ExpressionXML AndInputAsSecondReference(string reference)
        {
            expressionXml.Item1 = new ExpressionXMLX2Series {Value = reference};
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