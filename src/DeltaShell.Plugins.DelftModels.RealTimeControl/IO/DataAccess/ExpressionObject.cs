using System;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Dimr.RtcXsd;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Serves as data access object for the object <see cref="ExpressionComplexType"/>
    /// that is retrieved from the rtc tools config xml file.
    /// </summary>
    public class ExpressionObject
    {
        /// <summary>
        /// Creates a new <see cref="ExpressionObject"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="firstReference">The first reference.</param>
        /// <param name="secondReference">The second reference.</param>
        /// <param name="yValue">The y value.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="id"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="operator"/> is not defined.
        /// </exception>
        public ExpressionObject(string id,
                                Operator @operator,
                                IExpressionReference firstReference,
                                IExpressionReference secondReference,
                                string yValue)
        {
            Ensure.NotNull(id, nameof(id));
            Ensure.IsDefined(@operator, nameof(@operator));

            Id = id;
            ControlGroupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(id);
            Operator = @operator;
            FirstReference = firstReference;
            SecondReference = secondReference;
            Y = yValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionObject"/> class.
        /// </summary>
        /// <param name="expressionXml">The expression type.</param>
        public ExpressionObject(ExpressionComplexType expressionXml) :
            this(expressionXml.id,
                 ConvertToOperator(expressionXml.mathematicalOperator),
                 ConvertToExpressionInput(expressionXml.Item),
                 ConvertToExpressionInput(expressionXml.Item1),
                 expressionXml.Item2) {}

        /// <summary>
        /// Gets the id of the object.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name of the corresponding control group.
        /// </summary>
        public string ControlGroupName { get; }

        /// <summary>
        /// Gets the reference to the first input parameter value.
        /// </summary>
        public IExpressionReference FirstReference { get; }

        /// <summary>
        /// Gets the operator of the expression.
        /// </summary>
        public Operator Operator { get; }

        /// <summary>
        /// Gets the reference to the second input parameter value.
        /// </summary>
        public IExpressionReference SecondReference { get; }

        /// <summary>
        /// Gets the y value name.
        /// </summary>
        public string Y { get; }

        private static IExpressionReference ConvertToExpressionInput(object item)
        {
            switch (item)
            {
                case string constantValue:
                    return new ConstantLeafReference(constantValue);
                case ExpressionComplexTypeX1Series parameter1:
                    return CreateExpressionReference(parameter1.Value);
                case ExpressionComplexTypeX2Series parameter2:
                    return CreateExpressionReference(parameter2.Value);
            }

            return null;
        }

        private static IExpressionReference CreateExpressionReference(string value)
        {
            if (value.StartsWith(RtcXmlTag.Input) || value.StartsWith(RtcXmlTag.DelayedInput))
            {
                return new ParameterLeafReference(value);
            }

            return new ExpressionReference(value);
        }

        private static Operator ConvertToOperator(MathematicalOperatorEnumStringType mathOperator)
        {
            switch (mathOperator)
            {
                case MathematicalOperatorEnumStringType.Item:
                    return Operator.Add;
                case MathematicalOperatorEnumStringType.Item1:
                    return Operator.Subtract;
                case MathematicalOperatorEnumStringType.Item2:
                    return Operator.Multiply;
                case MathematicalOperatorEnumStringType.Item3:
                    return Operator.Divide;
                case MathematicalOperatorEnumStringType.min:
                    return Operator.Min;
                case MathematicalOperatorEnumStringType.max:
                    return Operator.Max;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mathOperator), mathOperator, null);
            }
        }
    }
}