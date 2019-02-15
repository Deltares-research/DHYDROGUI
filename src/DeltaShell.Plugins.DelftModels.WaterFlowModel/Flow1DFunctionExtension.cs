using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// Flow1D Interpolation options as defined within the D-Flow1D Technical Reference manual.
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum Flow1DInterpolationType
    {
        [Description("Linear")]
        Linear,
        [Description("Block-to (Constant)")]
        BlockTo,
        [Description("Block-from (Constant)")]
        BlockFrom,
    }

    /// <summary>
    /// Flow1D Extrapolation options as defined within the D-Flow1D Technical Reference Manual.
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum Flow1DExtrapolationType
    {
        [Description("Linear")]
        Linear,
        [Description("Constant")]
        Constant,
    }

    /// <summary>
    /// Flow1DFunctionExtension provides an interface to interact, and store interpolation and
    /// extrapolation as defined within D-Flow1D Technical Reference manual. This is used for
    /// the WindFunction, MeteoFunction, WaterFlowModel1DLateralSourceData, and WaterFlowModel1DBoundaryNodeData.
    ///
    /// It provides methods to get and set the interpolation type, extrapolation type, and periodicity.
    /// If only these methods are used, and functions.arguments[0].InterpolationType|ExtrapolationType
    /// are not set directly, then the state is guaranteed to be correct.
    /// </summary>
    public static class Flow1DFunctionExtension
    {
        /// <summary>The interpolation key used to store the interpolation in the Function.Attributes. </summary>
        private const string InterpolationKey = "Interpolation";
        /// <summary>The extrapolation key used to storte the extrapolation in the Function.Attributes. </summary>
        private const string ExtrapolationKey = "Extrapolation";

        /// <summary>
        /// Get the type of the interpolation as defined in the Flow1D Technical Reference manual.
        /// </summary>
        /// <param name="function"> The function of which the interpolation type is retrieved. </param>
        /// <returns> The type of interpolation of the specified function. </returns>
        /// <remarks>
        /// If no interpolation key has been set yet, it will be derived from Argument[0].InterpolationType.
        /// </remarks>
        /// <remarks>
        /// Function is expected to have an Argument[0] set.
        /// </remarks>
        public static Flow1DInterpolationType GetInterpolationType(this IFunction function)
        {
            if (Enum.TryParse<Flow1DInterpolationType>(function.GetAttribute(InterpolationKey),
                                                       out var interpolationType))
            {
                return interpolationType;
            }

            throw new InvalidOperationException($"The '{InterpolationKey}' in the Attributes dictionary of function {function.Name} has been corrupted.");
        }

        /// <summary>
        /// Set the type of the interpolation to <paramref name="value"/>.
        /// </summary>
        /// <param name="function"> The function on which the interpolation type is set. </param>
        /// <param name="value"> The value to which the interpolation is set. </param>
        /// <remarks> Only executed if function.HasArguments() && function.Arguments[0].AllowSetInterpolationType. </remarks>
        public static void SetInterpolationType(this IFunction function, Flow1DInterpolationType value)
        {
            if (!function.HasArguments() || !function.Arguments[0].AllowSetInterpolationType) return;

            switch (value)
            {
                case Flow1DInterpolationType.Linear:
                    function.Arguments[0].InterpolationType = InterpolationType.Linear;
                    break;
                case Flow1DInterpolationType.BlockFrom:
                case Flow1DInterpolationType.BlockTo:
                    function.Arguments[0].InterpolationType = InterpolationType.Constant;
                    break;
            }

            function.SetAttribute<Flow1DInterpolationType>(InterpolationKey, value);
        }

        /// <summary>
        /// Get the type of the Extrapolation as defined in the Flow1D Technical Reference manual.
        /// </summary>
        /// <param name="function"> The function of which the extrapolation type is retrieved. </param>
        /// <returns> The type of extrapolation of the specified function. </returns>
        /// <remarks>
        /// If no extrapolation key has been set yet, it will be derived from Argument[0].ExtrapolationType.
        /// </remarks>
        /// <remarks>
        /// Function is expected to have an Argument[0] set.
        /// </remarks>
        public static Flow1DExtrapolationType GetExtrapolationType(this IFunction function)
        {
            if (Enum.TryParse<Flow1DExtrapolationType>(function.GetAttribute(ExtrapolationKey),
                                                       out var extrapolationType))
            {
                return extrapolationType;
            }

            throw new InvalidOperationException($"The '{ExtrapolationKey}' in the Attributes dictionary of function {function.Name} has been corrupted.");
        }

        /// <summary>
        /// Set the type of the extrapolation to <paramref name="value"/>.
        /// </summary>
        /// <param name="function"> The function on which the extrapolation type is set. </param>
        /// <param name="value"> The value to which the extrapolation is set. </param>
        /// <remarks> Only executed if function.HasArguments() && function.Arguments[0].AllowSetExtrapolationType. </remarks>
        public static void SetExtrapolationType(this IFunction function, Flow1DExtrapolationType value)
        {
            if (!function.HasArguments() || !function.Arguments[0].AllowSetExtrapolationType) return;
            switch (value)
            {
                case Flow1DExtrapolationType.Linear:
                    function.Arguments[0].ExtrapolationType = ExtrapolationType.Linear;
                    break;
                case Flow1DExtrapolationType.Constant:
                    function.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
                    break;
            }

            function.SetAttribute<Flow1DExtrapolationType>(ExtrapolationKey,
                                                           value);
        }

        /// <summary>
        /// Determine whether this Function is periodic.
        /// </summary>
        /// <param name="function"> The function. </param>
        /// <returns><c>true</c> if the specified function has periodicity; otherwise, <c>false</c>.</returns>
        public static bool HasPeriodicity(this IFunction function)
        {
            return function.HasArguments() &&
                   function.Arguments[0].ExtrapolationType == ExtrapolationType.Periodic;
        }

        /// <summary>
        /// Set whether this Function is periodic.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="newHasPeriodicity">if set to <c>true</c> [new has periodicity].</param>
        public static void SetPeriodicity(this IFunction function, bool newHasPeriodicity)
        {
            if (!function.HasArguments() || !function.Arguments[0].AllowSetExtrapolationType) return;

            if (newHasPeriodicity)
            {
                function.Arguments[0].ExtrapolationType = ExtrapolationType.Periodic;
            }
            else
            {
                if (Enum.TryParse<Flow1DExtrapolationType>(function.GetAttribute(ExtrapolationKey),
                                                           out var extrapolationType))
                {
                    switch (extrapolationType)
                    {
                        case Flow1DExtrapolationType.Linear:
                            function.Arguments[0].ExtrapolationType = ExtrapolationType.Linear;
                            break;
                        case Flow1DExtrapolationType.Constant:
                            function.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Determine whether this Function has an argument defined.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns><c>true</c> if the specified function has arguments; otherwise, <c>false</c>.</returns>
        public static bool HasArguments(this IFunction function)
        {
            return function?.Arguments != null && function.Arguments.Count > 0;
        }


        #region AttributeHelpers        
        /// <summary>
        /// Get the attribute with the specified <paramref name="key"/> from the <paramref name="function"/> attributes.
        /// </summary>
        /// <param name="function">The function from which the attribute should be retrieved. </param>
        /// <param name="key"> The key of the attribute to be retrieved. </param>
        /// <param name="hasBeenCalledBefore">
        /// if this function has been called before, used to determine whether the key should have been initialized.
        /// </param>
        /// <returns> The attribute string associated with the key in function.Attributes. </returns>
        private static string GetAttribute(this IFunction function, string key, bool hasBeenCalledBefore = false)
        {
            try
            {
                return function.Attributes[key];
            }
            catch (KeyNotFoundException)
            {
                if (hasBeenCalledBefore) throw; // This should not occur

                function.SyncApproximationSchemes();
                return function.GetAttribute(key, true);
            }
        }

        /// <summary>
        /// Set the attribute associated with <paramref name="key"/> to <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T"> The type of value that should be added to the Attributes of <paramref name="function"/> </typeparam>
        /// <param name="function">The function.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private static void SetAttribute<T>(this IFunction function, string key, T value) where T : IConvertible
        {
            function.Attributes[key] = value.ToString();
        }

        /// <summary>
        /// Synchronize the D-flow-1d interpolation and extrapolation schemes with the interpolation and extrapolation of this function.
        /// </summary>
        /// <param name="function">The function.</param>
        private static void SyncApproximationSchemes(this IFunction function)
        {
            if (!function.HasArguments()) return;

            switch (function.Arguments[0].InterpolationType)
            {
                case InterpolationType.Constant:
                    function.SetAttribute<Flow1DInterpolationType>(InterpolationKey,
                                                                   Flow1DInterpolationType.BlockFrom);
                    break;
                case InterpolationType.Linear:
                case InterpolationType.None: // None should not be possible, as such we default to linear
                    function.SetAttribute<Flow1DInterpolationType>(InterpolationKey,
                                                                   Flow1DInterpolationType.Linear);
                    break;
            }

            switch (function.Arguments[0].ExtrapolationType)
            {
                case ExtrapolationType.Linear:
                    function.SetAttribute<Flow1DExtrapolationType>(ExtrapolationKey,
                                                                   Flow1DExtrapolationType.Linear);
                    break;
                case ExtrapolationType.Constant:
                case ExtrapolationType.None:     // None should not be possible, as such we default to linear
                case ExtrapolationType.Periodic: // Periodic implies periodic flag is set, as such we default to linear.
                    function.SetAttribute<Flow1DExtrapolationType>(ExtrapolationKey,
                                                                   Flow1DExtrapolationType.Constant);
                    break;
            }
        }
        #endregion
    }
}
