using System.Collections.Generic;
using DelftTools.Shell.Core.Dao;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate
{
    /// <summary>
    /// Provides the needed <see cref="IDataAccessListener"/> for the <see cref="WaterQualityModelApplicationPlugin"/>.
    /// </summary>
    /// <seealso cref="IDataAccessListenersProvider"/>
    public sealed class WaterQualityDataAccessListenersProvider : IDataAccessListenersProvider
    {
        /// <inheritdoc/>
        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new WaterQualityModelDataAccessListener(null);
        }
    }
}