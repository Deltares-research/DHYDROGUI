using System;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    /// <summary>
    /// GwswElementExtensions can be for validating and checking types of a GwswAttribute
    /// </summary>
    public static class GwswElementExtensions
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswElementExtensions));
        private const string UniqueId = "UNIQUE_ID";

        /// <summary>
        /// Determines whether this instance is numerical.
        /// </summary>
        /// <param name="gwswAttribute">The GWSW attribute.</param>
        /// <returns>
        ///   <c>true</c> if the specified GWSW attribute is numerical; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumerical(this GwswAttribute gwswAttribute)
        {
            if (gwswAttribute.GwswAttributeType != null && gwswAttribute.GwswAttributeType.AttributeType != null)
                return gwswAttribute.GwswAttributeType.AttributeType.IsNumericalType();

            return false;
        }

        /// <summary>
        /// Determines whether [is type of] [the specified compare type].
        /// </summary>
        /// <param name="gwswAttribute">The GWSW attribute.</param>
        /// <param name="compareType">Type of the compare.</param>
        /// <returns>
        ///   <c>true</c> if [is type of] [the specified compare type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTypeOf(this GwswAttribute gwswAttribute, Type compareType)
        {
            if (gwswAttribute.GwswAttributeType != null && gwswAttribute.GwswAttributeType.AttributeType != null)
                return gwswAttribute.GwswAttributeType.AttributeType == compareType;

            return false;
        }

        /// <summary>
        /// Determines whether [is valid attribute].
        /// </summary>
        /// <param name="gwswAttribute">The GWSW attribute.</param>
        /// <returns>
        ///   <c>true</c> if [is valid attribute] [the specified GWSW attribute]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidAttribute(this GwswAttribute gwswAttribute)
        {
            if (gwswAttribute == null) return false;
            
            if (gwswAttribute.ValueAsString != null &&
                gwswAttribute.GwswAttributeType != null &&
                gwswAttribute.GwswAttributeType.AttributeType != null)
            {
                return true;
            }

            gwswAttribute.LogInvalidAttribute();
            return false;
        }

        /// <summary>
        /// Gets the element line.
        /// </summary>
        /// <param name="gwswElement">The GWSW element.</param>
        /// <returns></returns>
        public static int GetElementLine(this GwswElement gwswElement)
        {
            var line = 0;
            /* It should always have attributes, but just in case (mostly testing) we include this check. */
            if( gwswElement.GwswAttributeList.Any())
                return gwswElement.GwswAttributeList.Select(attr => attr?.LineNumber).First() ?? line;

            return line;
        }

        /// <summary>
        /// Gets the attribute from list.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns></returns>
        public static GwswAttribute GetAttributeFromList(this GwswElement element, string attributeName)
        {
            var attribute = element?.GwswAttributeList?.FirstOrDefault(attr => attr.GwswAttributeType.Key.Equals(attributeName));
            if (attribute != null)
                return attribute;

            var uniqueIdAttribute = element?.GwswAttributeList?.FirstOrDefault(attr => attr.GwswAttributeType.Key.Equals(UniqueId));
            Log.WarnFormat(Resources.GwswElementExtensions_GetAttributeFromList_Attribute__0__was_not_found_for_element__1__of_type__2__, attributeName, uniqueIdAttribute?.ValueAsString, element?.ElementTypeName);
            return null;
        }

        /// <summary>
        /// Gets the valid string value.
        /// </summary>
        /// <param name="gwswAttribute">The GWSW attribute.</param>
        /// <returns></returns>
        public static string GetValidStringValue(this GwswAttribute gwswAttribute)
        {
            if (gwswAttribute.IsValidAttribute() && gwswAttribute.IsTypeOf(typeof(string)))
            {
                return gwswAttribute.ValueAsString;
            }

            return null;
        }

        /// <summary>
        /// Tries the get value as double.
        /// </summary>
        /// <param name="gwswAttribute">The GWSW attribute.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGetValueAsDouble(this GwswAttribute gwswAttribute, out double value)
        {
            value = default(double);
            if (!gwswAttribute.IsValidAttribute() || gwswAttribute.ValueAsString == string.Empty) return false;
            if( !gwswAttribute.IsNumerical())
            {
                gwswAttribute.LogErrorParseType(typeof(double));
                return false;
            }

            try
            {
                value = Convert.ToDouble(gwswAttribute.ValueAsString, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                gwswAttribute.LogErrorParseType(typeof(double));
            }
            return false;
        }

        /// <summary>
        /// Tries the get value as integer.
        /// </summary>
        /// <param name="gwswAttribute">The GWSW attribute.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGetValueAsInt(this GwswAttribute gwswAttribute, out int value)
        {
            value = default(int);
            if (!gwswAttribute.IsValidAttribute() || gwswAttribute.ValueAsString == string.Empty) return false;
            if (!gwswAttribute.IsNumerical())
            {
                gwswAttribute.LogErrorParseType(typeof(int));
                return false;
            }

            try
            {
                value = Convert.ToInt32(gwswAttribute.ValueAsString, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                gwswAttribute.LogErrorParseType(typeof(int));
            }
            return false;
        }

        /// <summary>
        /// Gets the value from description.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gwswAttribute">The GWSW attribute.</param>
        /// <returns></returns>
        public static T GetValueFromDescription<T>(this GwswAttribute gwswAttribute)
        {
            var description = gwswAttribute.GetValidStringValue();
            try
            {
                return (T)typeof(T).GetEnumValueFromDescription(description);
            }
            catch (Exception)
            {
                Log.WarnFormat(Resources.SewerFeatureFactory_GetValueFromDescription_Type__0__is_not_recognized__please_check_the_syntax, description);
            }

            return default(T);
        }

        /// <summary>
        /// Logs the invalid attribute.
        /// </summary>
        /// <param name="gwswAttribute">The GWSW attribute.</param>
        private static void LogInvalidAttribute(this GwswAttribute gwswAttribute)
        {
            if (gwswAttribute.GwswAttributeType == null) return;

            var attributeType = gwswAttribute.GwswAttributeType;
            Log.ErrorFormat(Resources.GwswElementExtensions_LogInvalidAttribute_File__0___line__1___Column__2____3___contains_invalid_value___4___and_will_not_be_imported_
                , attributeType.FileName, gwswAttribute.LineNumber, attributeType.LocalKey, attributeType.Key, gwswAttribute.ValueAsString);
        }

        /// <summary>
        /// Logs the type of the error parse.
        /// </summary>
        /// <param name="gwswAttribute">The GWSW attribute.</param>
        /// <param name="toType">To type.</param>
        private static void LogErrorParseType(this GwswAttribute gwswAttribute, Type toType)
        {
            var attr = gwswAttribute.GwswAttributeType;
            Log.ErrorFormat(Resources.GwswElementExtensions_LogErrorParseType_File__0___line__1___element__2___It_was_not_possible_to_parse_attribute__3__from_type__4__to_type__5__
                , attr.FileName, gwswAttribute.LineNumber, attr.ElementName, attr.Name, gwswAttribute.ValueAsString, attr.AttributeType, toType);
        }
    }
}