using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// RtcXmlTags used for writing and reading xml files.
    /// </summary>
    public static class RtcXmlTag
    {
        public const string DirectionalCondition = "[DirectionalCondition]";
        public const string FactorRule = "[FactorRule]";
        public const string HydraulicRule = "[HydraulicRule]";
        public const string IntervalRule = "[IntervalRule]";
        public const string LookupSignal = "[LookupSignal]";
        public const string PIDRule = "[PID]";
        public const string RelativeTimeRule = "[RelativeTimeRule]";
        public const string StandardCondition = "[StandardCondition]";
        public const string TimeCondition = "[TimeCondition]";
        public const string TimeRule = "[TimeRule]";

        public const string Input = "[Input]";
        public const string DelayedInput = Delayed + Input;
        public const string Output = "[Output]";

        public const string OutputAsInput = "[AsInputFor]";

        public const string Status = "[Status]";

        public const string SP = "[SP]"; // set point
        public const string IP = "[IP]"; // integral part
        public const string DP = "[DP]"; // differential part
        public const string Delayed = "[Delayed]";
        public const string Signal = "[Signal]";

        public static readonly IEnumerable<string> ComponentTags = new[]
        {
            DirectionalCondition,
            FactorRule,
            HydraulicRule,
            IntervalRule,
            LookupSignal,
            PIDRule,
            RelativeTimeRule,
            StandardCondition,
            TimeCondition,
            TimeRule
        };

        public static readonly IEnumerable<string> ConnectionPointTags = new[]
        {
            Input,
            Output
        };
    }
}