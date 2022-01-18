using System;
using DelftTools.Utils.NetCdf;
using DelftTools.Utils.Remoting;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.NetCdf;
using DeltaShell.NGHS.IO.Properties;
using log4net;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <summary>
    /// Remote variant of <see cref="UGridApi"/>.
    /// </summary>
    public sealed class RemoteUGridApi : IUGridApi
    {
        private IUGridApi api;
        private static readonly ILog log = LogManager.GetLogger(typeof(RemoteUGridApi));
        public RemoteUGridApi()
        {
            api = RemoteInstanceContainer.CreateInstance<IUGridApi, UGridApi>();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (RemoteInstanceContainer.IsProcessAlive(api))
            {
                try
                {
                    api?.Dispose();
                }
                catch
                {
                    // ignored because process will be killed (thus all resources will be released)
                }
            }
            
            RemoteInstanceContainer.RemoveInstance(api);
            api = null;
        }

        /// <inheritdoc/>
        public bool IsUGridFile()
        {
            return api.IsUGridFile();
        }

        /// <inheritdoc/>
        public void CreateFile(string filePath, FileMetaData fileMetaData)
        {
            api.CreateFile(filePath, fileMetaData);
        }

        /// <inheritdoc/>
        public void Open(string filePath, OpenMode mode = OpenMode.Reading)
        {
            ValidateFilePathWithAcceptableDeltaresVersion(filePath);
            api.Open(filePath, mode);
        }

        /// <summary>
        /// open netcdf manually, this functionality must be placed int the wrapper opening function using the ionc_adheresto_conventions_dll call when UNST-4908 is resolved
        /// format of the convention string could be:
        /// "CF-1.6 UGRID-1.0/Deltares-0.8"
        /// "CF-1.6 UGRID-1.0/Deltares-0.9"
        /// "CF-1.8 UGRID-1.0 Deltares-0.10"
        /// </summary>
        /// <param name="filePath"> The file path to the NetCDF file. </param>
        private static void ValidateFilePathWithAcceptableDeltaresVersion(string filePath)
        {
            NetCdfFile file = null;
            NetCdfConvention convention = null;

            try
            {
                file = NetCdfFile.OpenExisting(filePath);
                convention = file.GetConvention();
            }
            catch (Exception e)
            {
                log.WarnFormat(Resources.RemoteUGridApi_ProblemWhileReadingConvention, filePath, e.Message);
            }
            finally
            {
                file?.Close();
            }

            if (convention == null)
            {
                log.WarnFormat(Resources.RemoteUGridApi_MissingGlobalAttributeConventions, filePath);
                return;
            }

            var requiredConvention = new NetCdfConvention(deltares: new Version(0, 10));
            if (!convention.Satisfies(requiredConvention))
            {
                log.WarnFormat(Resources.RemoteUGridApi_RequiredNetCDFVersion, requiredConvention, convention, filePath);
            }
        }

        /// <inheritdoc/>
        public void Close()
        {
            api.Close();
        }

        /// <inheritdoc/>
        public double GetVersion()
        {
            return api.GetVersion();
        }

        /// <inheritdoc/>
        public int GetMeshCount()
        {
            return api.GetMeshCount();
        }

        /// <inheritdoc/>
        public int GetNumberOfMeshByType(UGridMeshType meshType)
        {
            return api.GetNumberOfMeshByType(meshType);
        }

        /// <inheritdoc/>
        public int[] GetMeshIdsByMeshType(UGridMeshType meshType)
        {
            return api.GetMeshIdsByMeshType(meshType);
        }

        /// <inheritdoc/>
        public int GetVarCount(int meshId, GridLocationType locationType)
        {
            return api.GetVarCount(meshId, locationType);
        }

        /// <inheritdoc/>
        public int[] GetVarIds(int meshId, GridLocationType locationType)
        {
            return api.GetVarIds(meshId, locationType);
        }

        /// <inheritdoc/>
        public double GetVariableNoDataValue(string variableName, int meshId, GridLocationType location)
        {
            return api.GetVariableNoDataValue(variableName, meshId, location);
        }

        /// <inheritdoc/>
        public double[] GetVariableValues(string variableName, int meshId, GridLocationType location)
        {
            return api.GetVariableValues(variableName, meshId, location);
        }

        /// <inheritdoc/>
        public void SetVariableValues(string variableName, string standardName, string longName, string unit, int meshId,
            GridLocationType location, double[] values, double noDataValue = -999)
        {
            api.SetVariableValues(variableName, standardName, longName, unit, meshId, location, values, noDataValue);
        }

        /// <inheritdoc/>
        public void ResetMeshVerticesCoordinates(int meshId, double[] xValues, double[] yValues)
        {
            api.ResetMeshVerticesCoordinates(meshId, xValues, yValues);
        }

        /// <inheritdoc/>
        public int GetCoordinateSystemCode()
        {
            return api.GetCoordinateSystemCode();
        }

        /// <inheritdoc/>
        public void SetCoordinateSystemCode(int epsgCode)
        {
            api.SetCoordinateSystemCode(epsgCode);
        }

        /// <inheritdoc/>
        public int[] GetNetworkIds()
        {
            return api.GetNetworkIds();
        }

        /// <inheritdoc/>
        public int GetNumberOfNetworks()
        {
            return api.GetNumberOfNetworks();
        }

        /// <inheritdoc/>
        public DisposableNetworkGeometry GetNetworkGeometry(int networkId)
        {
            return api.GetNetworkGeometry(networkId);
        }

        /// <inheritdoc/>
        public int WriteNetworkGeometry(DisposableNetworkGeometry geometry)
        {
            return api.WriteNetworkGeometry(geometry);
        }

        /// <inheritdoc/>
        public int GetNetworkIdFromMeshId(int meshId)
        {
            return api.GetNetworkIdFromMeshId(meshId);
        }

        /// <inheritdoc/>
        public Disposable1DMeshGeometry GetMesh1D(int meshId)
        {
            return api.GetMesh1D(meshId);
        }

        /// <inheritdoc/>
        public int WriteMesh1D(Disposable1DMeshGeometry mesh, int networkId)
        {
            return api.WriteMesh1D(mesh,networkId);
        }

        /// <inheritdoc/>
        public Disposable2DMeshGeometry GetMesh2D(int meshId)
        {
            return api.GetMesh2D(meshId);
        }

        /// <inheritdoc/>
        public int WriteMesh2D(Disposable2DMeshGeometry mesh)
        {
            return api.WriteMesh2D(mesh);
        }

        /// <inheritdoc/>
        public int GetLinksId()
        {
            return api.GetLinksId();
        }

        /// <inheritdoc/>
        public DisposableLinksGeometry GetLinks(int linksId)
        {
            return api.GetLinks(linksId);
        }

        /// <inheritdoc/>
        public int WriteLinks(DisposableLinksGeometry links)
        {
            return api.WriteLinks(links);
        }
    }
}