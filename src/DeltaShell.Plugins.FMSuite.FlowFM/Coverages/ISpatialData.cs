using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    /// <summary>
    /// Provides the interface for a spatial data manager, which holds the available spatial coverages in D-Flow FM.
    /// </summary>
    public interface ISpatialData
    {
        /// <summary>
        /// Gets or sets the bathymetry.
        /// </summary>
        UnstructuredGridCoverage Bathymetry { get; set; }

        /// <summary>
        /// Gets or sets the initial water level.
        /// </summary>
        UnstructuredGridCellCoverage InitialWaterLevel { get; set; }

        /// <summary>
        /// Gets or sets the initial salinity.
        /// </summary>
        UnstructuredGridCellCoverage InitialSalinity { get; set; }

        /// <summary>
        /// Gets or sets the initial temperature.
        /// </summary>
        UnstructuredGridCellCoverage InitialTemperature { get; set; }

        /// <summary>
        /// Gets or sets the roughness.
        /// </summary>
        UnstructuredGridFlowLinkCoverage Roughness { get; set; }

        /// <summary>
        /// Gets or sets the viscosity.
        /// </summary>
        UnstructuredGridFlowLinkCoverage Viscosity { get; set; }

        /// <summary>
        /// Gets or sets the diffusivity.
        /// </summary>
        UnstructuredGridFlowLinkCoverage Diffusivity { get; set; }

        /// <summary>
        /// Gets the collection of initial tracers.
        /// </summary>
        IEnumerable<UnstructuredGridCellCoverage> InitialTracers { get; }

        /// <summary>
        /// Gets the collection of initial fractions.
        /// </summary>
        IEnumerable<UnstructuredGridCellCoverage> InitialFractions { get; }

        /// <summary>
        /// Gets the collection of data items in which the coverages are held.
        /// </summary>
        /// <remarks>
        /// This collection should not be changed from outside.
        /// Instead, use the provided methods.
        /// </remarks>
        IEventedList<IDataItem> DataItems { get; }

        /// <summary>
        /// Adds a tracer to the tracers.
        /// </summary>
        /// <param name="coverage"> The coverage that defines the tracer. </param>
        /// <remarks>
        /// In the case a tracer with the same name already exists, the tracer will not be added.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="coverage"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the name of the <paramref name="coverage"/> is <c>null</c> or empty.
        /// </exception>
        void AddTracer(UnstructuredGridCellCoverage coverage);

        /// <summary>
        /// Removes the tracer with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name"> The name of the tracer. </param>
        void RemoveTracer(string name);

        /// <summary>
        /// Adds a sediment fraction.
        /// </summary>
        /// <param name="coverage">The coverage that defines the sediment fraction. </param>
        /// <remarks>
        /// In the case a sediment fraction with the same name already exists, the sediment fraction will not be added.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="coverage"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the name of the <paramref name="coverage"/> is <c>null</c> or empty.
        /// </exception>
        void AddFraction(UnstructuredGridCellCoverage coverage);

        /// <summary>
        /// Removes the sediment fraction with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name"> The name of the sediment fraction. </param>
        void RemoveFraction(string name);
    }
}