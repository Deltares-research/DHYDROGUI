using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Link1d2d;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    /// <summary>
    /// Helper for doing UGrid related actions
    /// </summary>
    public static class UGridFileHelperExtensions
    {
        public static IEnumerable<string> ValidateMesh1DSourceLocationsOnlyExistOnce(this Disposable1DMeshGeometry mesh1d, IEnumerable<ILink1D2D> link1D2Ds)
        {
            int index = 0;
            var coordinatesDiscretizationPoints = mesh1d.NodesX
                                                        .Select((x, i) => new Coordinate(x, mesh1d.NodesY[i]))
                                                        .ToDictionary<Coordinate, int>(c => index++);

            foreach (ILink1D2D link in link1D2Ds)
            {
                var linkDiscretizationPointIndex = link.DiscretisationPointIndex;
                var linkDiscretizationPointCoordinate = coordinatesDiscretizationPoints[linkDiscretizationPointIndex];
                var otherDiscretizationPointCoordinatesAtSameLocationIndices = coordinatesDiscretizationPoints
                                                                               .Where(kvp => kvp.Value.Equals2D(linkDiscretizationPointCoordinate)
                                                                                             && kvp.Key != linkDiscretizationPointIndex)
                                                                               .Select(kvp => kvp.Key).ToArray();
                if (otherDiscretizationPointCoordinatesAtSameLocationIndices.Length > 0)
                {
                    var otherDiscretizationPointNames = otherDiscretizationPointCoordinatesAtSameLocationIndices.Select(j => mesh1d.NodeIds[j]);
                    string linksName = link.Name;
                    yield return string.Format(Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessage_part1, linksName, mesh1d.NodeIds[linkDiscretizationPointIndex], link.FaceIndex) +
                                 Environment.NewLine +
                                 string.Format(Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessage_part2, string.Join(", ", otherDiscretizationPointNames));
                }
            }
        }
    }
}