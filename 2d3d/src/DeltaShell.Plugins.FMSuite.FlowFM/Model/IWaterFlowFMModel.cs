using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    /// <summary>
    /// <see cref="IWaterFlowFMModel"/> defines the public interface of the
    /// D-FlowFM plugin model.
    /// </summary>
    /// <seealso cref="ITimeDependentModel" />
    /// <seealso cref="IHasCoordinateSystem" />
    public interface IWaterFlowFMModel : ITimeDependentModel,
                                         IHasCoordinateSystem
    {
        /// <summary>
        /// Gets or sets the grid of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        UnstructuredGrid Grid { get; set; }

        /// <summary>
        /// Reloads the grid based on the internal state.
        /// </summary>
        /// <param name="writeNetFile">if set to <c>true</c> [write net file].</param>
        /// <param name="loadBathymetry">if set to <c>true</c> [load bathymetry].</param>
        void ReloadGrid(bool writeNetFile = true, bool loadBathymetry = false);

        /// <summary>
        /// Gets the path to the netCDF file describing the grid.
        /// </summary>
        string NetFilePath { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to [disable flow node renumbering].
        /// </summary>
        bool DisableFlowNodeRenumbering { get; set; }

        /// <summary>
        /// Gets the spatial data of this model.
        /// </summary>
        ISpatialData SpatialData { get; }

        /// <summary>
        /// Gets the model definition.
        /// </summary>
        WaterFlowFMModelDefinition ModelDefinition { get; }
    }
}