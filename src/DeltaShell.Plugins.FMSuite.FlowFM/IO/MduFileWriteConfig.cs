using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// IMduFileWriteConfig collects a set of boolean values used to configure
    /// which parts should be written, when <see cref="MduFile.Write"/> and
    /// <see cref="MduFile.WriteProperties"/> are called.
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
    }

    /// <summary>
    /// Concrete implementation of the <see cref="IMduFileWriteConfig"/> interface.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.FMSuite.FlowFM.IO.IMduFileWriteConfig"/>
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
        /// </remarks>
        public MduFileWriteConfig()
        {
            WriteExtForcings = true;
            WriteFeatures = true;
            WriteMorphologySediment = true;
            DisableFlowNodeRenumbering = false;
        }

        public bool WriteExtForcings { get; set; }
        public bool WriteFeatures { get; set; }
        public bool WriteMorphologySediment { get; set; }
        public bool DisableFlowNodeRenumbering { get; set; }
    }
}