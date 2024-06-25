using System.Collections.Generic;
using DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.NHibernate
{
    /// <summary>
    /// Provides the needed <see cref="IDataAccessListener"/> for the <see cref="RainfallRunoffApplicationPlugin"/>.
    /// </summary>
    /// <seealso cref="IDataAccessListenersProvider"/>
    public sealed class RainfallRunoffDataAccessListenersProvider : IDataAccessListenersProvider
    {
        /// <inheritdoc/>
        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new RainfallRunoffDataAccessListener(new BasinGeometryShapeFileSerializer(), null);
        }
    }
}