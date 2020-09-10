using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Provides a set of methods to create a serializer for a given rtc object.
    /// </summary>
    public static class SerializerCreator
    {
        private static readonly Dictionary<Type, Type> serializers = new Dictionary<Type, Type>
        {
            {typeof(TimeRule), typeof(TimeRuleSerializer)},
            {typeof(TimeCondition), typeof(TimeConditionSerializer)},
            {typeof(DirectionalCondition), typeof(DirectionalConditionSerializer)},
            {typeof(StandardCondition), typeof(StandardConditionSerializer)},
            {typeof(RelativeTimeRule), typeof(RelativeTimeRuleSerializer)},
            {typeof(PIDRule), typeof(PidRuleSerializer)},
            {typeof(LookupSignal), typeof(LookupSignalSerializer)},
            {typeof(IntervalRule), typeof(IntervalRuleSerializer)},
            {typeof(HydraulicRule), typeof(HydraulicRuleSerializer)},
            {typeof(FactorRule), typeof(FactorRuleSerializer)},
            {typeof(MathematicalExpression), typeof(MathematicalExpressionSerializer)},
            {typeof(Input), typeof(InputSerializer)}
        };

        /// <summary>
        /// Creates a serializer of the specified type with the specified <see cref="rtcObject"/>.
        /// </summary>
        /// <typeparam name="T"> The serializer type. </typeparam>
        /// <param name="rtcObject"> The RTC object to create a serializer for. </param>
        /// <returns> The serializer with the specified <see cref="rtcObject"/>. </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when there is no serializer available  for the
        /// type of the specified <paramref name="rtcObject"/>.
        /// </exception>
        public static T CreateSerializerType<T>(RtcBaseObject rtcObject)
        {
            Type serializerType = serializers[rtcObject.GetType()];
            if (serializerType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(rtcObject));
            }

            object instance = Activator.CreateInstance(serializerType, rtcObject);

            return (T) instance;
        }

        /// <summary>
        /// Creates a serializer for the specified <see cref="rtcObject"/>.
        /// </summary>
        /// <param name="rtcObject"> The RTC object to create a serializer for. </param>
        /// <returns> The serializer with the specified <see cref="rtcObject"/>. </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when there is no serializer available  for the
        /// type of the specified <paramref name="rtcObject"/>.
        /// </exception>
        public static RtcSerializerBase CreateSerializerType(RtcBaseObject rtcObject)
        {
            Type serializerType = serializers[rtcObject.GetType()];
            if (serializerType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(rtcObject));
            }

            return (RtcSerializerBase) Activator.CreateInstance(serializerType, rtcObject);
        }

        /// <summary>
        /// Creates a serializer of the specified type with the specified <see cref="rtcObject"/>.
        /// </summary>
        /// <typeparam name="T"> The serializer type. </typeparam>
        /// <param name="input"> The input to create a serializer for. </param>
        /// <returns> The serializer with the specified <see cref="input"/>. </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when there is no serializer available  for the
        /// type of the specified <paramref name="input"/>.
        /// </exception>
        /// <remarks>
        /// The specified <paramref name="input"/> must derive from <see cref="RtcBaseObject"/>.
        /// </remarks>
        public static T CreateSerializerType<T>(IInput input)
        {
            return CreateSerializerType<T>((RtcBaseObject) input);
        }
    }
}