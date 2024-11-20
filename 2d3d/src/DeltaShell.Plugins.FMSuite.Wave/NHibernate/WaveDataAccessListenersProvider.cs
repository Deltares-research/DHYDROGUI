using System.Collections.Generic;
using DelftTools.Shell.Core.Dao;

namespace DeltaShell.Plugins.FMSuite.Wave.NHibernate
{
    /// <summary>
    /// Provides the needed <see cref="IDataAccessListener"/> for the <see cref="WaveApplicationPlugin"/>.
    /// </summary>
    /// <seealso cref="IDataAccessListenersProvider"/>
    public sealed class WaveDataAccessListenersProvider : IDataAccessListenersProvider
    {
        /// <inheritdoc/>
        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new WaveDataAccessListener();
        }
    }
}