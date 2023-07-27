using DeltaShell.NGHS.IO.DelftIniObjects;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations
{
    /// <summary>
    /// <see cref="IDelftIniPropertyBehaviour"/> defines the interface
    /// of a single operation on a single property.
    /// </summary>
    public interface IDelftIniPropertyBehaviour
    {
        /// <summary>
        /// Invokes this <see cref="IDelftIniPropertyBehaviour"/>
        /// on the specified <paramref name="property"/>.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="logHandler">An optional log handler.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="property"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// While not technically enforced, it is expected that each
        /// <see cref="IDelftIniPropertyBehaviour"/> acts upon
        /// one and only one property. If the provided property does not
        /// match the defined property, then nothing will occur.
        /// </remarks>
        void Invoke(DelftIniProperty property, ILogHandler logHandler);
    }
}