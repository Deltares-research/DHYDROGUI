namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Concrete implementation of the <see cref="IMduFileWriteConfig"/> interface.
    /// </summary>
    /// <seealso cref="IMduFileWriteConfig"/>
    public class MduFileWriteConfig : IMduFileWriteConfig
    {
        /// <summary>
        /// Initializs a new instance of the <see cref="MduFileWriteConfig"/> class with default values.
        /// </summary>
        /// <remarks>
        /// The default values are:
        /// WriteExtForcings           = true
        /// WriteFeatures              = true
        /// WriteMorSed                = true
        /// DisableFlowNodeRenumbering = false
        /// WriteRestartStartTime      = false
        /// </remarks>
        public MduFileWriteConfig()
        {
            WriteExtForcings = true;
            WriteFeatures = true;
            WriteMorphologySediment = true;
            DisableFlowNodeRenumbering = false;
            WriteRestartStartTime = false;
        }

        public bool WriteExtForcings { get; set; }
        public bool WriteFeatures { get; set; }
        public bool WriteMorphologySediment { get; set; }
        public bool DisableFlowNodeRenumbering { get; set; }
        public bool WriteRestartStartTime { get; set; }
    }
}