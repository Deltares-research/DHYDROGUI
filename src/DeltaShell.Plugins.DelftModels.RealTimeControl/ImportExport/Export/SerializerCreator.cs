using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public static class SerializerCreator
    {
        private static readonly Dictionary<Type, Type> serializers;
        static SerializerCreator()
        {
            serializers = new Dictionary<Type, Type>
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
        }

        public static T CreateSerializerType<T>(RtcBaseObject rtcObject)
        {
            var serializerType = serializers[rtcObject.GetType()];
            if (serializerType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(rtcObject));
            }

            var instance = Activator.CreateInstance(serializerType, rtcObject);

            return (T) instance;
        }

        public static RtcSerializerBase CreateSerializerType(RtcBaseObject rtcObject)
        {
            var serializerType = serializers[rtcObject.GetType()];
            if (serializerType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(rtcObject));
            }

            return (RtcSerializerBase) Activator.CreateInstance(serializerType, rtcObject);
        }
    }
}
