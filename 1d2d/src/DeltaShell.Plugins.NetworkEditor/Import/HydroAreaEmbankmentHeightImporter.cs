using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class HydroAreaEmbankmentHeightImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroAreaEmbankmentHeightImporter));
        private const int SizeOfLocalNeighbourhood = 1;
        private const int MaxNumberOfUnsuccessfulMatches = 100;
        private const double PointTolerance = 5.0;
        private int _embankmentIndex = 0;
        private int _embankmentPointIndex = 0;

        public string Name { get { return "Embankment heights"; } }
        public string Description { get { return Name; } }
        public string Category { get { return "2D / 3D"; } }

        public Bitmap Image { get { return Properties.Resources.guide; } }

        public IEnumerable<Type> SupportedItemTypes { get { yield return typeof (HydroArea); } }

        public bool CanImportOn(object targetObject) { return true; }

        public bool CanImportOnRootLevel { get { return false; }}

        public string FileFilter { get { return "Shape file|*.shp"; }}

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get; private set; }

        public object ImportItem(string path, object target = null)
        {
            if (String.IsNullOrEmpty(path))
                throw new InvalidOperationException("No target path to import from.");
            if (target == null)
                throw new InvalidOperationException("No target found to import to.");

            HydroArea targetHydroArea = target as HydroArea;
            if (targetHydroArea == null)
                throw new InvalidOperationException("It is only possible to import a 2D shape file into a hydro area.");
            if (targetHydroArea.Embankments.Count < 1)
            {
                Log.Error("It is only possible to import embankment heights into a hydro area with embankments.");
                return null;
            }

            int totalNumberOfModelPoints = 0;
            foreach (Embankment embankment in targetHydroArea.Embankments)
            {
                totalNumberOfModelPoints += embankment.Geometry.NumPoints;
            }
            var shapefile = new ShapeFile(path);
            if (shapefile.Features.Count > totalNumberOfModelPoints)
            {
                Log.Error("Number of embankment heights is greater than the number of points in the hydro area.");
                return null;
            }

            ICoordinateTransformation coordinateTransformation = null;
            if (targetHydroArea.CoordinateSystem != null &&
                shapefile.CoordinateSystem != null &&
                targetHydroArea.CoordinateSystem.Name != shapefile.CoordinateSystem.Name)
            {
                coordinateTransformation = new OgrCoordinateSystemFactory().CreateTransformation(shapefile.CoordinateSystem, targetHydroArea.CoordinateSystem);
            }

            // The following assumptions have been made:
            // 1. It is reasonable to expect the height points to be 'in order' compared to the embankment points in the hydro model (though the order may be reversed)
            // 2. After 100 (or so) unsuccessful matches you may give up trying to fit a model

            int numFailures = 0;
            foreach (IFeature feature in shapefile.Features)
            {
                if (coordinateTransformation != null)
                {
                    feature.Geometry = GeometryTransform.TransformGeometry(feature.Geometry, coordinateTransformation.MathTransform);
                }
                if (MatchLocalArea(feature, targetHydroArea) ||
                    MatchAllEmbankmentPoints(feature, targetHydroArea))
                {
                    targetHydroArea.Embankments[_embankmentIndex].Geometry.Coordinates[_embankmentPointIndex].Z = Convert.ToDouble(feature.Attributes["POINT_Z"]);
                }
                else
                {
                    numFailures++;
                }
                if (numFailures > MaxNumberOfUnsuccessfulMatches)
                {
                    Log.Error("Number of unsuccessful imports exceeds limit (" + MaxNumberOfUnsuccessfulMatches + ").");
                    return null;
                }
            }
            if (numFailures == 0) Log.Info("Successfully imported all height points");
            else Log.Warn("Successfully imported " + (shapefile.Features.Count - numFailures) + " of " + shapefile.Features.Count + " height points");
            return target; 
        }

        private bool MatchLocalArea(IFeature feature, HydroArea targetHydroArea)
        {
            for (int i = 1; i <= SizeOfLocalNeighbourhood; i++)
            {
                if (_embankmentPointIndex + i < targetHydroArea.Embankments[_embankmentIndex].Geometry.Coordinates.Length)
                {
                    Coordinate embankmentPointCoordinate = targetHydroArea.Embankments[_embankmentIndex].Geometry.Coordinates[_embankmentPointIndex + i];
                    if (IsPointWithinRange(feature.Geometry.Coordinate.X, feature.Geometry.Coordinate.Y, embankmentPointCoordinate.X, embankmentPointCoordinate.Y))
                    {
                        _embankmentPointIndex += i;
                        return true;
                    }
                }
                if (_embankmentPointIndex - i >= 0)
                {
                    Coordinate embankmentPointCoordinate = targetHydroArea.Embankments[_embankmentIndex].Geometry.Coordinates[_embankmentPointIndex - i];
                    if (IsPointWithinRange(feature.Geometry.Coordinate.X, feature.Geometry.Coordinate.Y, embankmentPointCoordinate.X, embankmentPointCoordinate.Y))
                    {
                        _embankmentPointIndex -= i;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool MatchAllEmbankmentPoints(IFeature feature, HydroArea targetHydroArea)
        {
            for (int i = 0; i < targetHydroArea.Embankments.Count; i++)
                for (int j = 0; j < targetHydroArea.Embankments[i].Geometry.Coordinates.Length; j++)
                {
                    Coordinate embankmentPointCoordinate = targetHydroArea.Embankments[i].Geometry.Coordinates[j];
                    if (IsPointWithinRange(feature.Geometry.Coordinate.X, feature.Geometry.Coordinate.Y, embankmentPointCoordinate.X, embankmentPointCoordinate.Y))
                    {
                        _embankmentIndex = i;
                        _embankmentPointIndex = j;
                        return true;
                    }
                }
            return false;
        }

        private bool IsPointWithinRange(double heightPointX, double heightPointY, double embankmentPointX, double embankmentPointY)
        {
            return (Math.Pow((heightPointX - embankmentPointX), 2.0) + Math.Pow((heightPointY - embankmentPointY), 2.0) <= Math.Pow(PointTolerance, 2.0));
        }
    }
}
