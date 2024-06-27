using System;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Contains the methods for performing conversions of an <see cref="Operator"/>.
    /// </summary>
    public static class OperatorConverter
    {
        /// <summary>
        /// Converts the specified <paramref name="operator"/> to a format string
        /// containing placeholders for two parameters.
        /// </summary>
        /// <param name="operator"> The operator. </param>
        /// <returns> The format string </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="operator"/> is not a defined value of <see cref="Operator"/>.
        /// </exception>
        public static string ToFormatString(this Operator @operator)
        {
            Ensure.IsDefined(@operator, nameof(@operator));

            switch (@operator)
            {
                case Operator.Add:
                    return "({0} + {1})";
                case Operator.Subtract:
                    return "({0} - {1})";
                case Operator.Multiply:
                    return "{0} * {1}";
                case Operator.Divide:
                    return "{0} / {1}";
                case Operator.Min:
                    return "min({0}, {1})";
                case Operator.Max:
                    return "max({0}, {1})";
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null);
            }
        }
    }
}