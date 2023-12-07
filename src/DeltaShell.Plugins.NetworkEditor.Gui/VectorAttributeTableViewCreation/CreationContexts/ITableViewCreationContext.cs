using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/>.
    /// The creation context is dependent of the type that needs to be represented in the table view.
    /// </summary>
    /// <typeparam name="TFeature"> The type of feature. </typeparam>
    /// <typeparam name="THydroRegion"> The type of hydro region. </typeparam>
    /// <typeparam name="TFeatureRow"> The type of feature row object. </typeparam>
    public interface ITableViewCreationContext<in TFeature, out TFeatureRow, in THydroRegion> where TFeature : IFeature
                                                                                              where TFeatureRow : class, IFeatureRowObject
                                                                                              where THydroRegion : IHydroRegion
    {
        /// <summary>
        /// Get the description of the table view.
        /// </summary>
        string GetDescription();

        /// <summary>
        /// Determines whether or not the data is contained in the specified hydro region.
        /// </summary>
        /// <param name="region"> The hydro region. </param>
        /// <param name="data"> The data to check for in the hydro region. </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="region"/> contains the <paramref name="data"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="region"/> or <paramref name="data"/> is <c>null</c>.
        /// </exception>
        bool IsRegionData(THydroRegion region, IEnumerable<TFeature> data);

        /// <summary>
        /// Create a feature row object representing the data of the provided feature.
        /// </summary>
        /// <param name="feature"> The feature to represent. </param>
        /// <param name="allFeatures"> All features within the group of the feature. </param>
        /// <returns> A new instance of a <see cref="IFeatureRowObject"/>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="feature"/> or <paramref name="allFeatures"/> is <c>null</c>/
        /// </exception>
        TFeatureRow CreateFeatureRowObject(TFeature feature, IEnumerable<TFeature> allFeatures);

        /// <summary>
        /// Customize the attribute table view.
        /// </summary>
        /// <param name="view"> The attribute table view. </param>
        /// <param name="data"> The data that is represented in the table view. </param>
        /// <param name="guiContainer"> The GUI container container the running GUI instance. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="view"/>, <paramref name="data"/> or <paramref name="guiContainer"/> is <c>null</c>.
        /// </exception>
        void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<TFeature> data, GuiContainer guiContainer);
    }
}