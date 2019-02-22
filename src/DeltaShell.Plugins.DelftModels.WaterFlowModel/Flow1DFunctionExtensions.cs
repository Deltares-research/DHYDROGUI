using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// Flow1DFunctionExtensions provides an interface to interact, and store interpolation and
    /// extrapolation as defined within the D-Flow1D Technical Reference Manual. This is used for
    /// the <see cref="WindFunction"/>, <see cref="PhysicalParameters.MeteoFunction"/>,
    /// <see cref="DataObjects.WaterFlowModel1DLateralSourceData"/>, and <see cref="DataObjects.WaterFlowModel1DBoundaryNodeData"/>.
    ///
    /// It provides methods to get and set the interpolation type, extrapolation type, and periodicity.
    /// If only these methods are used, and functions.arguments[0].InterpolationType|ExtrapolationType
    /// are not set directly, then the state is guaranteed to be correct.
    /// </summary>
    public static class Flow1DFunctionExtensions
    {
        /// <summary>The interpolation key used to store the interpolation in the Function.Attributes.</summary>
        private const string InterpolationKey = "Interpolation";
        /// <summary>The extrapolation key used to store the extrapolation in the Function.Attributes.</summary>
        private const string ExtrapolationKey = "Extrapolation";

        /// <summary>
        /// Get the type of the interpolation as defined in the Flow1D Technical Reference Manual.
        /// </summary>
        /// <param name="function">The function of which the interpolation type is retrieved.</param>
        /// <returns>The type of interpolation of the specified function.</returns>
        /// <remarks>
        /// If no interpolation key has been set yet, it will be derived from Argument[0].InterpolationType.
        /// </remarks>
        /// <remarks>
        /// Function is expected to have an Argument[0] set.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the Interpolation stored in the <see cref="IFunction.Attributes"/> cannot be parsed.
        /// </exception>
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
        /// <param name="function">The function on which the interpolation type is set.</param>
        /// <param name="value">The value to which the interpolation is set.</param>
        /// <remarks>
        /// Only executed if the function has at least one argument, and the first argument allows
        /// setting the interpolation type.
        /// </remarks>
        public static void SetInterpolationType(this IFunction function, Flow1DInterpolationType value)
        {
            if (!function.HasArguments() || !function.Arguments[0].AllowSetInterpolationType)
                return;

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

            function.SetAttribute(InterpolationKey, value);
        }

        /// <summary>
        /// Get the type of the Extrapolation as defined in the Flow1D Technical Reference Manual.
        /// </summary>
        /// <param name="function">The function of which the extrapolation type is retrieved.</param>
        /// <returns>The type of extrapolation of the specified function.</returns>
        /// <remarks>
        /// If no extrapolation key has been set yet, it will be derived from Argument[0].ExtrapolationType.
        /// </remarks>
        /// <remarks>
        /// Function is expected to have an Argument[0] set.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the Extrapolation stored in the <see cref="IFunction.Attributes"/> cannot be parsed.
        /// </exception>
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
        /// <param name="function">The function on which the extrapolation type is set.</param>
        /// <param name="value">The value to which the extrapolation is set.</param>
        /// <remarks>
        /// Only executed if the function has at least one argument, and the first argument allows
        /// setting the extrapolation type.
        /// </remarks>
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

            function.SetAttribute(ExtrapolationKey, value);
        }

        /// <summary>
        /// Determine whether this Function is periodic.
        /// </summary>
        /// <param name="function">The function.</param>
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
        /// <param name="newHasPeriodicity">The new periodicity of this function.</param>
        /// <remarks>
        /// Only executed if the function has at least one argument, and the first argument allows
        /// setting the extrapolation type.
        /// </remarks>
        public static void SetPeriodicity(this IFunction function, bool newHasPeriodicity)
        {
            if (!function.HasArguments() || !function.Arguments[0].AllowSetExtrapolationType)
                return;

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
        private static bool HasArguments(this IFunction function)
        {
            return function?.Arguments != null && function.Arguments.Count > 0;
        }

        #region AttributeHelpers        
        /// <summary>
        /// Get the attribute with the specified <paramref name="key"/> from the <paramref name="function"/> attributes.
        /// </summary>
        /// <param name="function">The function from which the attribute should be retrieved.</param>
        /// <param name="key">The key of the attribute to be retrieved.</param>
        /// <param name="hasBeenCalledBefore">
        /// if this function has been called before, used to determine whether the key should have been initialized.
        /// </param>
        /// <returns>The attribute string associated with the key in function.Attributes.</returns>
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
        /// <typeparam name="T">The type of value that should be added to the Attributes of <paramref name="function"/>.</typeparam>
        /// <param name="function">The function.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private static void SetAttribute<T>(this IFunction function, string key, T value) where T : IConvertible
        {
            function.Attributes[key] = value.ToString();
        }

        /// <summary>
        /// Synchronize the D-flow-1d interpolation and extrapolation schemes with the interpolation and
        /// extrapolation of this function.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <remarks>
        /// This should only be called when <see cref="GetInterpolationType"/> or <see cref="GetExtrapolationType"/> is called on
        /// a function which has no interpolation or extrapolation key defined yet.
        /// This situation only happens when an old project is opened, or a new function is created.
        /// </remarks>
        /// <exception cref="ArgumentNullException">If the function does not contain at least one argument.</exception>
        private static void SyncApproximationSchemes(this IFunction function)
        {
            if (!function.HasArguments())
                throw new ArgumentNullException(nameof(function.Arguments), 
                                                "Function have at least one argument.");

            var hasPeriodicity = function.Arguments[0].ExtrapolationType == ExtrapolationType.Periodic;

            switch (function.Arguments[0].InterpolationType)
            {
                case InterpolationType.Constant:
                    function.SetInterpolationType(Flow1DInterpolationType.BlockFrom);
                    function.SetExtrapolationType(Flow1DExtrapolationType.Constant);
                    break;
                case InterpolationType.Linear:
                case InterpolationType.None: // None should not be possible, as such we default to linear
                    function.SetInterpolationType(Flow1DInterpolationType.Linear);
                    function.SetExtrapolationType(Flow1DExtrapolationType.Linear);
                    break;
            }

            // Set periodicity after updating the Interpolation and Extrapolation.
            if (hasPeriodicity)
                function.SetPeriodicity(true);
        }
        #endregion
    }
}
