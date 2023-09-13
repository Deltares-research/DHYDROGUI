using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations
{
    /// <summary>
    /// <see cref="IIniPropertyBehaviour"/> defines the interface
    /// of a single operation on a single property.
    /// </summary>
    public interface IIniPropertyBehaviour
    {
        /// <summary>
        /// Invokes this <see cref="IIniPropertyBehaviour"/>
        /// on the specified <paramref name="property"/>.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="logHandler">An optional log handler.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="property"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// While not technically enforced, it is expected that each
        /// <see cref="IIniPropertyBehaviour"/> acts upon
        /// one and only one property. If the provided property does not
        /// match the defined property, then nothing will occur.
        /// </remarks>
        void Invoke(IniProperty property, ILogHandler logHandler);
    }
}