using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Data access object for importing a <see cref="SignalBase"/> from the tools config xml file.
    /// </summary>
    /// <seealso cref="IRtcDataAccessObject{SignalBase}"/>
    public class SignalDataAccessObject : IRtcDataAccessObject<SignalBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalDataAccessObject"/> class.
        /// </summary>
        /// <param name="id"> The identifier that was read from the file. </param>
        /// <param name="signal"> The created signal. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="id"/> or <paramref name="signal"/> is <c>null</c>.
        /// </exception>
        public SignalDataAccessObject(string id, SignalBase signal)
        {
            Ensure.NotNull(id, nameof(id));
            Ensure.NotNull(signal, nameof(signal));

            Id = id;
            ControlGroupName = RealTimeControlXmlReaderHelper.GetControlGroupNameFromElementId(Id);
            Object = signal;
        }

        /// <summary>
        /// Gets the references to the signal inputs.
        /// </summary>
        /// <value>
        /// The input references.
        /// </value>
        public IList<string> InputReferences { get; } = new List<string>();

        public string Id { get; }

        public string ControlGroupName { get; }

        /// <summary>
        /// Gets the <see cref="SignalBase"/> that was created from the tools config file.
        /// </summary>
        /// <value>
        /// The created signal.
        /// </value>
        public SignalBase Object { get; }
    }
}