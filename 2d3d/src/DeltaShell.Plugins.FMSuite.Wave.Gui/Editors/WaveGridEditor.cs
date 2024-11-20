using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using SharpMap.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public static class WaveGridEditor
    {
        public static void LaunchGridEditor(WaveModel model, IWaveDomainData editDomain = null)
        {
            if (model == null)
            {
                return;
            }

            string mdwDir = Path.GetDirectoryName(model.MdwFilePath);

            List<IWaveDomainData> domains = WaveDomainHelper
                                            .GetAllDomains(model.OuterDomain).Where(d => !d.Equals(editDomain)).ToList();
            if (editDomain != null)
            {
                domains.Insert(0, editDomain);
            }

            string[] gridPaths = domains.Select(d => Path.Combine(mdwDir, d.GridFileName)).ToArray();

            if (!gridPaths.Any())
            {
                return;
            }

            bool[] emptyFlags = domains.Select(d => d.Grid == null || d.Grid.IsEmpty).ToArray();
            RgfGridEditor.OpenGrids(gridPaths, emptyFlags,
                                    systemType: model.CoordinateSystem != null && model.CoordinateSystem.IsGeographic
                                                    ? CoordinateSystemType.Spherical
                                                    : CoordinateSystemType.Cartesian);

            // reload grid..
            try
            {
                CurvilinearGrid grid = Delft3DGridFileReader.Read(gridPaths[0]);
                IEnumerable<Coordinate> coordinates = grid.X.Values.Zip(grid.Y.Values, (x, y) => new Coordinate(x, y));
                if (model.CoordinateSystem != null &&
                    !CoordinateSystemValidator.CanAssignCoordinateSystem(coordinates, model.CoordinateSystem))
                {
                    throw new Exception("Grid coordinates are incompatible with current model coordinate system");
                }

                model.ReloadAllGrids();
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Failed to reload grid after RGFGrid edits: {0}", e.Message),
                                "Failed to reload grid.");
            }
        }
    }
}