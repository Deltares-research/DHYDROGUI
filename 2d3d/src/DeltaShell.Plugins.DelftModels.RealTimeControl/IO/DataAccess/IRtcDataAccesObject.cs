using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Data access object for importing a <see cref="RtcBaseObject"/> from the tools config xml file.
    /// </summary>
    /// <typeparam name="T"> Type of the <see cref="RtcBaseObject"/> that is contained by this instance. </typeparam>
    public interface IRtcDataAccessObject<out T> where T : RtcBaseObject
    {
        /// <summary>
        /// Gets the identifier of the xml element that was read from file.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Gets the name of the control group this <see cref="RtcBaseObject"/> belongs to.
        /// </summary>
        /// <value>
        /// The name of the control group.
        /// </value>
        string ControlGroupName { get; }

        /// <summary>
        /// Gets the <see cref="RtcBaseObject"/> that was created from the tools config file.
        /// </summary>
        /// <value>
        /// The object.
        /// </value>
        T Object { get; }
    }
}