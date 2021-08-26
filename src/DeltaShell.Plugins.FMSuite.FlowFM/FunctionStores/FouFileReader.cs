using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Units;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    internal static class FouFileReader
    {
        private const string fourierAttributeToCheck = "frequency_degrees_per_hour";
        private const string noDataAttributeName = "_FillValue";
        private const string unitsAttributeName = "units";
        private const string longNameAttributeName = "long_name";

        internal const string ncVariableName = "nc_name";
        
        private enum MeshType
        {
            Mesh1d,
            Mesh2d
        }

        public static FouFileMetaData ReadMetaData(string path)
        {
            var grid = UGridFileHelper.ReadUnstructuredGrid(path, recreateCells:false);
            
            var hydroNetwork = new HydroNetwork();
            var discretization = new Discretization{Network = hydroNetwork};
            UGridFileHelper.ReadNetworkAndDiscretisation(path,discretization,hydroNetwork,null,null);

            var variables1D = GetMeshDependentVariables(path, MeshType.Mesh1d);
            var variables2D = GetMeshDependentVariables(path, MeshType.Mesh2d);

            var networkLocations = discretization.Locations.Values.ToList();

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

        public static NetworkCoverage[] Create1dMeshCoverages(FouFileMetaData metaData)
        {
            if (!File.Exists(metaData.Path))
                return Array.Empty<NetworkCoverage>();

            return DoWithNetCdfFile(metaData.Path, file =>
            {
                return metaData.Variables1D
                               .Select(v => GenerateCoverageForVariable(file, v, () =>
                                                                            new NetworkCoverage("", false)
                                                                            {
                                                                                Network = metaData.Network,
                                                                                SegmentGenerationMethod = SegmentGenerationMethod.None
                                                                            }))
                               .ToArray();
            });
        }

        public static UnstructuredGridCellCoverage[] Create2dMeshCoverages(FouFileMetaData metaData)
        {
            if (!File.Exists(metaData.Path))
                return Array.Empty<UnstructuredGridCellCoverage>();

            return DoWithNetCdfFile(metaData.Path, file =>
            {
                return metaData.Variables2D
                               .Select(v => GenerateCoverageForVariable(file, v, () => new UnstructuredGridCellCoverage(metaData.Grid, false)))
                               .ToArray();
            });
        }

        private static T GenerateCoverageForVariable<T>(NetCdfFile file, string variableName, Func<T> createCoverageFunc) where T : ICoverage
        {
            var variable = file.GetVariableByName(variableName);
            var attributes = file.GetAttributes(variable);

            attributes.TryGetValue(unitsAttributeName, out object unit);
            attributes.TryGetValue(noDataAttributeName, out object noDataValue);
            attributes.TryGetValue(longNameAttributeName, out object displayName);

            var coverage = createCoverageFunc.Invoke();

            coverage.Name = $"{displayName} ({variableName})";
            ((IFunction)coverage).Attributes = attributes.ToDictionary(a => a.Key, a => a.Value?.ToString());

            var component = coverage.Components[0];
            component.NoDataValue = noDataValue;
            component.Unit = new Unit("", unit?.ToString());
            component.Attributes.Add(ncVariableName, variableName);

            return coverage;
        }

        private static string[] GetMeshDependentVariables(string path, MeshType meshType)
        {
            return DoWithNetCdfFile(path, file =>
            {
                var dimensions = file.GetAllDimensions()
                                     .Where(d => string.Equals(file.GetDimensionName(d), GetDimensionName(meshType), StringComparison.InvariantCultureIgnoreCase))
                                     .ToArray();

                return file.GetVariables().Where(v =>
                {
                    var variableDimensions = file.GetDimensions(v).ToArray();
                    var attributes = file.GetAttributes(v);

                    return variableDimensions.Length == 1
                           && dimensions.Contains(variableDimensions[0])
                           && attributes.ContainsKey(fourierAttributeToCheck);
                }).Select(file.GetVariableName).ToArray();
            });
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

        public static T DoWithNetCdfFile<T>(string path, Func<NetCdfFile,T> fileAction)
        {
            var file = NetCdfFile.OpenExisting(path);
            try
            {
                return fileAction != null ? fileAction(file) : default(T);
            }
            finally
            {
                file?.Close();
            }
        }
    }
}