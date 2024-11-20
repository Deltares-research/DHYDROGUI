namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// <see cref="IMduFileWriteConfig"/> collects a set of boolean values used to configure
    /// which parts should be written.
    /// </summary>
    public interface IMduFileWriteConfig
    {
        /// <summary>
        /// Get whether [the external forcings should be written].
        /// </summary>
        /// <value>
        /// <c> true </c> if [the external forcings should be written];
        /// otherwise, <c> false </c>.
        /// </value>
        bool WriteExtForcings { get; }

        /// <summary>
        /// Get whether [the features should be written].
        /// </summary>
        /// <value>
        /// <c> true </c> if [the features should be written];
        /// otherwise, <c> false </c>.
        /// </value>
        bool WriteFeatures { get; }

        /// <summary>
        /// Get whether [the Sediment and Morphology should be written].
        /// </summary>
        /// <value>
        /// <c> true </c> if [the Sediment and Morphology should be written];
        /// otherwise, <c> false </c>.
        /// </value>
        bool WriteMorphologySediment { get; }

        /// <summary>
        /// Get whether [the flow renumbering should be disabled].
        /// </summary>
        /// <value>
        /// <c> true </c> if [the flow renumbering should be disabled];
        /// otherwise, <c> false </c>.
        /// </value>
        bool DisableFlowNodeRenumbering { get; } 
        
        /// <summary>
        /// Get whether the restart start time should be written.
        /// </summary>
        bool WriteRestartStartTime { get; }
    }
}