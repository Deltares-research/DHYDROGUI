using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Grids;
using SharpMap;
using SharpMap.CoordinateSystems;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers
{
    public class WaveGridFileImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaveGridFileImporter));

        private readonly Func<IEnumerable<WaveModel>> getModels;

        public WaveGridFileImporter(string category, Func<IEnumerable<WaveModel>> getModelsFunc)
        {
            Category = category;
            getModels = getModelsFunc;
        }

        public string Name => "Delft3D Grid";

        public string Category { get; private set; }
        public string Description => string.Empty;

        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(CurvilinearGrid);
            }
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => "Delft3D Grid (*.grd)|*.grd|All Files (*.*)|*.*";

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get; private set; }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            var targetGrid = target as CurvilinearGrid;
            if (targetGrid == null)
            {
                throw new NotSupportedException("Need a valid target to import the grid file into");
            }

            WaveModel model = getModels().First(m => WaveDomainHelper.GetAllDomains(m.OuterDomain)
                                                                     .Any(d => Equals(d.Grid, targetGrid)));
            IWaveDomainData domain =
                WaveDomainHelper.GetAllDomains(model.OuterDomain)
                                .First(d => Equals(d.Grid, targetGrid));

            model.BeginEdit("Importing grid");
            try
            {
                CurvilinearGrid grid = Delft3DGridFileReader.Read(path);
                grid.CoordinateSystem = grid.Attributes[CurvilinearGrid.CoordinateSystemKey] == "Spherical"
                                            ? new OgrCoordinateSystemFactory().CreateFromEPSG(4326)
                                            : null;

                IEnumerable<Coordinate> coordinates = grid.X.Values.Zip(grid.Y.Values, (x, y) => new Coordinate(x, y));
                if (Map.CoordinateSystemFactory == null)
                {
                    Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
                }

                if (model.CoordinateSystem != null &&
                    !CoordinateSystemValidator.CanAssignCoordinateSystem(coordinates, model.CoordinateSystem))
                {
                    Log.ErrorFormat(
                        "Grid coordinates are incompatible with current model coordinate system: {0}, canceling import.",
                        model.CoordinateSystem);
                    return null;
                }

                string uniqueFileName = model.ImportIntoModelDirectory(path);
                domain.GridFileName = uniqueFileName;
                WaveModel.LoadGrid(Path.GetDirectoryName(model.MdwFilePath), domain);
            }
            finally
            {
                model.EndEdit();
            }

            return target;
        }
    }
}