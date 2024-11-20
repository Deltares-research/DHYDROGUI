using System;
using System.Collections.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public interface IHydroData : IUnique<long>, IDisposable
    {
        event EventHandler<EventArgs<string>> DataChanged;
        string FilePath { get; }

        UnstructuredGrid Grid { get; }

        DateTime ConversionStartTime { get; }
        DateTime ConversionStopTime { get; }
        TimeSpan ConversionTimeStep { get; }
        DateTime ConversionReferenceTime { get; }

        /// <summary>
        /// Determines whether the hydro dynamics data has data available for a specific
        /// function, process or substance.
        /// </summary>
        /// <param name="functionName"> Name of the function. </param>
        /// <returns> True if data is available, false otherwise. </returns>
        bool HasDataFor(string functionName);

        /// <summary>
        /// Gets the file path for a given function when available in the hydro data.
        /// </summary>
        /// <param name="functionName"> Name of the funcion. </param>
        /// <returns>
        /// The filepath for the given function if <see cref="HasDataFor"/> returns
        /// true for <paramref name="functionName"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// When <see cref="HasDataFor"/> returns
        /// false for <paramref name="functionName"/>.
        /// </exception>
        string GetFilePathFor(string functionName);

        /// <summary>
        /// Determines whether this hydro dynamical data has the same schematization (grid
        /// and layer definitions) as another hydro data.
        /// </summary>
        /// <param name="data"> The data. </param>
        /// <returns> True if the schematizations are the same, false otherwise. </returns>
        bool HasSameSchematization(IHydroData data);

        #region File references

        // the properties below go directly into the delwaq .inp file.

        /// <summary>
        /// The relative path (with respect to hydrolic data location) to the areas file.
        /// This is a binary file that contains the areas for waq.
        /// Normally, this file has the extension .are
        /// </summary>
        string AreasRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydrolic data location) to the volumes file.
        /// This is a binary file that contains the volumes for waq.
        /// Normally, this file has the extension .vol
        /// </summary>
        string VolumesRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydrolic data location) to the flows file.
        /// This is a binary file that contains the flows for waq.
        /// Normally, this file has the extension .flo
        /// </summary>
        string FlowsRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydrolic data location) to the pointers file.
        /// This is a binary file that contains the pointers for waq.
        /// Normally, this file has the extension .poi
        /// </summary>
        string PointersRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydrolic data location) to the lengths file.
        /// This is a binary file that contains the lengths for waq.
        /// Normally, this file has the extension .len
        /// </summary>
        string LengthsRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydrolic data location) to the salinity file.
        /// This is a binary file that contains the salinity for waq.
        /// Normally, this file has the extension .sal
        /// </summary>
        string SalinityRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydrolic data location) to the temperatures file.
        /// This is a binary file that contains the temperatures for waq.
        /// It is uncertain what the default extension is for this file.
        /// </summary>
        string TemperatureRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydrolic data location) to the vertical diffusion file.
        /// This is a binary file that contains the vertical diffusion for waq.
        /// Normally, this file has the extension .vdf
        /// </summary>
        string VerticalDiffusionRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydrolic data location) to the surfaces file.
        /// This is a binary file that contains the surfaces for waq.
        /// Normally, this file has the extension .srf
        /// </summary>
        string SurfacesRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydrolic data location) to the shear stresses file.
        /// This is a binary file that contains the shear stresses for waq.
        /// Normally, this file has the extension .tau
        /// </summary>
        string ShearStressesRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydraulic data location) to the grid file.
        /// This is a file that will be included as INCLUDE in the input file.
        /// The extension is .nc
        /// </summary>
        string GridRelativePath { get; }

        /// <summary>
        /// The relative path (with respect to hydraulic data location) to the attributes file.
        /// This is a file that will be included as INCLUDE in the input file.
        /// The extension is .atr
        /// </summary>
        string AttributesRelativePath { get; }

        /// <summary>
        /// Gets the velocities file path.
        /// </summary>
        /// <value>
        /// The velocities relative file path (with respect to hydraulic data location). Extension .dat.
        /// </value>
        string VelocitiesRelativePath { get; }

        /// <summary>
        /// Gets the widths file path.
        /// </summary>
        /// <value>
        /// The widths relative file path (with respect to hydraulic data location). Extension .dat.
        /// </value>
        string WidthsRelativePath { get; }

        /// <summary>
        /// Gets the chezy coefficients file path.
        /// </summary>
        /// <value>
        /// The chezy coefficients relative file path (with respect to hydraulic data location). Extension .dat.
        /// </value>
        string ChezyCoefficientsRelativePath { get; }

        #endregion File references

        #region Meta data

        // the data below in this region contain
        // meta data that goes directly into the delwaq .inp file.

        /// <summary>
        /// Gets the type of the hydro dynamic model.
        /// </summary>
        HydroDynamicModelType HydroDynamicModelType { get; }

        /// <summary>
        /// Gets the type of the layers applied in the hydro dynamic model.
        /// </summary>
        LayerType LayerType { get; }

        /// <summary>
        /// Gets the top-level Z coordinate allowed in the model.
        /// </summary>
        double ZTop { get; }

        /// <summary>
        /// Gets the bottom-level Z coordinate allowed in the model.
        /// </summary>
        double ZBot { get; }

        int NumberOfHorizontalExchanges { get; }

        int NumberOfVerticalExchanges { get; }

        int NumberOfHydrodynamicLayers { get; }

        int NumberOfDelwaqSegmentsPerHydrodynamicLayer { get; }

        int NumberOfWaqSegmentLayers { get; }

        IEventedList<WaterQualityBoundary> GetBoundaries();

        IDictionary<WaterQualityBoundary, int[]> GetBoundaryNodeIds();

        double[] HydrodynamicLayerThicknesses { get; }

        int[] NumberOfHydrodynamicLayersPerWaqSegmentLayer { get; }

        #endregion Meta data
    }
}