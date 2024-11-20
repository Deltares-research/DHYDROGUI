using System.Collections.Generic;
using DelftTools.Shell.Core.Dao;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.NHibernate
{
    /// <summary>
    /// Provides the needed <see cref="IDataAccessListener"/> for the <see cref="RealTimeControlApplicationPlugin"/>.
    /// </summary>
    /// <seealso cref="IDataAccessListenersProvider"/>
    public sealed class RealTimeControlDataAccessListenersProvider : IDataAccessListenersProvider
    {
        /// <inheritdoc/>
        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new RtcDataAccessListener(null);
        }
    }
}