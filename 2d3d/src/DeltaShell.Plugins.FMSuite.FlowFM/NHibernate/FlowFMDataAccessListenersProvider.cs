using System.Collections.Generic;
using DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.NHibernate
{
    /// <summary>
    /// Provides the needed <see cref="IDataAccessListener"/> for the <see cref="FlowFMApplicationPlugin"/>.
    /// </summary>
    /// <seealso cref="IDataAccessListenersProvider"/>
    public sealed class FlowFMDataAccessListenersProvider : IDataAccessListenersProvider
    {
        /// <inheritdoc/>
        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new WaterFlowFMDataAccessListener();
        }
    }
}