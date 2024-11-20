using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Units;
using DelftTools.Utils.NetCdf;
using Deltares.Infrastructure.Logging;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    internal static class FouFileReader
    {
        private enum MeshType
        {
            Mesh1d,
            Mesh2d
        }

        internal const string NcVariableName = "nc_name";

        private const string noDataAttributeName = "_FillValue";
        private const string unitsAttributeName = "units";
        private const string longNameAttributeName = "long_name";
        private const string referenceDateAttribute = "reference_date_in_yyyymmdd";

        /// <summary>
        /// Read meta data from Fou file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Data object with meta data</returns>
        public static FouFileMetaData ReadMetaData(string path)
        {
            var grid = new UnstructuredGrid();
            var hydroNetwork = new HydroNetwork();
            var discretization = new Discretization { Network = hydroNetwork };
            var links = new List<ILink1D2D>();
            IConvertedUgridFileObjects convertedUgridFileObjects = new ConvertedUgridFileObjects
            {
                Discretization = discretization,
                Grid = grid,
                HydroNetwork = hydroNetwork,
                Links1D2D = links
            };
            var logHandler = new LogHandler(string.Format(Resources.ConstructFunctions_Reading_file_output_into_our_FM_model, "fm fou file", path));
            using (var ugridFile = new UGridFile(path))
            {
                ugridFile.ReadNetFileDataIntoModel(convertedUgridFileObjects, loadFlowLinksAndCells: true, recreateCells: false, forceCustomLengths: true, logHandler: logHandler);
            }
            logHandler.LogReport();

            string[] variables1D = GetMeshDependentVariables(path, MeshType.Mesh1d);
            string[] variables2D = GetMeshDependentVariables(path, MeshType.Mesh2d);

            List<INetworkLocation> networkLocations = discretization.Locations.Values.ToList();

            return new FouFileMetaData
            {
                Path = path,
                Grid = grid,
                Network = hydroNetwork,
                Mesh1dLocations = networkLocations,
                Variables1D = variables1D,
                Variables2D = variables2D,
            };
        }

        /// <summary>
        /// Creates coverages based on the 1d mesh
        /// </summary>
        /// <param name="metaData">Meta data of the fou file</param>
        /// <returns>Collection of network coverages based on the 1d mesh</returns>
        public static NetworkCoverage[] Create1dMeshCoverages(FouFileMetaData metaData)
        {
            if (!File.Exists(metaData.Path))
            {
                return Array.Empty<NetworkCoverage>();
            }

            using (DisposableObjectWrapper<NetCdfFile> wrapper = CreateNetCdfWrapper(metaData.Path))
            {
                NetCdfFile file = wrapper.WrapperObject;
                return metaData.Variables1D
                               .Select(v => GenerateCoverageForVariable(file, v, () =>
                                                                            new NetworkCoverage("", false)
                                                                            {
                                                                                Network = metaData.Network,
                                                                                SegmentGenerationMethod = SegmentGenerationMethod.None
                                                                            }))
                               .ToArray();
            }
        }

        /// <summary>
        /// Creates coverages based on the 2d mesh
        /// </summary>
        /// <param name="metaData">Meta data of the fou file</param>
        /// <returns>Collection of grid coverages based on the 2d mesh</returns>
        public static UnstructuredGridCellCoverage[] Create2dMeshCoverages(FouFileMetaData metaData)
        {
            if (!File.Exists(metaData.Path))
            {
                return Array.Empty<UnstructuredGridCellCoverage>();
            }

            using (DisposableObjectWrapper<NetCdfFile> wrapper = CreateNetCdfWrapper(metaData.Path))
            {
                NetCdfFile file = wrapper.WrapperObject;
                return metaData.Variables2D
                               .Select(v => GenerateCoverageForVariable(file, v, () => new UnstructuredGridCellCoverage(metaData.Grid, false)))
                               .ToArray();
            }
        }

        /// <summary>
        /// Reads the specified variable data from the specified fou file (path), filtered by specified indices
        /// </summary>
        /// <typeparam name="T">Type of the variable data</typeparam>
        /// <param name="metaData">Meta data of the fou file</param>
        /// <param name="variableName">Name of the variable to query</param>
        /// <param name="indices">Indices to filter on (null = no filtering)</param>
        /// <returns>MultiDimensionalArray with the requested values</returns>
        public static IMultiDimensionalArray<T> Read1dArrayValues<T>(FouFileMetaData metaData, string variableName, int[] indices)
        {
            using (DisposableObjectWrapper<NetCdfFile> wrapper = CreateNetCdfWrapper(metaData.Path))
            {
                NetCdfFile file = wrapper.WrapperObject;
                NetCdfVariable ncVariable = file.GetVariableByName(variableName);
                return GetMultiDimensionalArray<T>(file, ncVariable, indices);
            }
        }

        private static IMultiDimensionalArray<T> GetMultiDimensionalArray<T>(NetCdfFile file, NetCdfVariable ncVariable, int[] indices)
        {
            List<T> values;

            if (indices.Length == 1)
            {
                values = file.Read(ncVariable, new[]
                {
                    indices[0]
                }, new[]
                {
                    1
                }).OfType<T>().ToList();
            }
            else
            {
                Array array = file.Read(ncVariable);

                values = (indices.Any()
                              ? indices.Select(i => array.GetValue(i)).OfType<T>()
                              : array.OfType<T>()).ToList();
            }

            return new MultiDimensionalArray<T>(true, false, default(T), values, new[] { values.Count });
        }

        private static T GenerateCoverageForVariable<T>(NetCdfFile file, string variableName, Func<T> createCoverageFunc) where T : ICoverage
        {
            NetCdfVariable variable = file.GetVariableByName(variableName);
            Dictionary<string, object> attributes = file.GetAttributes(variable);

            attributes.TryGetValue(unitsAttributeName, out object unit);
            attributes.TryGetValue(noDataAttributeName, out object noDataValue);
            attributes.TryGetValue(longNameAttributeName, out object displayName);

            T coverage = createCoverageFunc.Invoke();

            coverage.Name = $"{displayName} ({variableName})";
            ((IFunction)coverage).Attributes = attributes.ToDictionary(a => a.Key, a => a.Value?.ToString());

            IVariable component = coverage.Components[0];
            component.NoDataValue = noDataValue;
            component.Unit = new Unit("", unit?.ToString());
            component.Attributes.Add(NcVariableName, variableName);

            return coverage;
        }

        private static string[] GetMeshDependentVariables(string path, MeshType meshType)
        {
            using (DisposableObjectWrapper<NetCdfFile> wrapper = CreateNetCdfWrapper(path))
            {
                NetCdfFile file = wrapper.WrapperObject;
                NetCdfDimension[] dimensions = file.GetAllDimensions()
                                                   .Where(d => string.Equals(file.GetDimensionName(d), GetDimensionName(meshType), StringComparison.InvariantCultureIgnoreCase))
                                                   .ToArray();

                return file.GetVariables().Where(v =>
                {
                    NetCdfDimension[] variableDimensions = file.GetDimensions(v).ToArray();
                    Dictionary<string, object> attributes = file.GetAttributes(v);

                    return variableDimensions.Length == 1
                           && dimensions.Contains(variableDimensions[0])
                           && IsFourierVariable(attributes);
                }).Select(file.GetVariableName).ToArray();
            }
        }

        private static bool IsFourierVariable(Dictionary<string, object> variableAttributes)
        {
            if (variableAttributes == null)
            {
                return false;
            }

            return variableAttributes.ContainsKey(referenceDateAttribute);
        }

        private static string GetDimensionName(MeshType meshType)
        {
            switch (meshType)
            {
                case MeshType.Mesh1d:
                    return "mesh1d_nNodes";
                case MeshType.Mesh2d:
                    return "mesh2d_nFaces";
                default:
                    throw new ArgumentOutOfRangeException(nameof(meshType), meshType, null);
            }
        }

        private static DisposableObjectWrapper<NetCdfFile> CreateNetCdfWrapper(string path)
        {
            return new DisposableObjectWrapper<NetCdfFile>(
                () => NetCdfFile.OpenExisting(path),
                cdfFile => cdfFile?.Close());
        }
    }
}