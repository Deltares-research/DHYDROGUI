using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class HydroAreaEmbankmentImporter : IFileImporter
    {
        public string Name
        {
            get { return "Embankments"; }
        }
        public string Description { get { return Name; } }
        public string Category { get { return "2D / 3D"; } }

        public Bitmap Image {
            get { return Properties.Resources.guide; } 
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof (HydroArea); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel { get { return false; }}
        
        public string FileFilter { get { return "Shape file|*.shp"; }}
        
        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }
        
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        
        public bool OpenViewAfterImport { get; private set; }
        
        public object ImportItem(string path, object target = null)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("No target path to import from.");
            }
            if (target == null)
            {
                throw new InvalidOperationException("No target found to import to.");
            }

            HydroArea targetHydroArea = target as HydroArea;
            if (targetHydroArea == null)
            {
                throw new InvalidOperationException("It is only possible to import a 2D embankments shape file into a hydro area.");
            }

            var shapefile = new ShapeFile(path);
            ICoordinateTransformation coordinateTransformation = null;
            if (targetHydroArea.CoordinateSystem != null && 
                shapefile.CoordinateSystem != null && 
                targetHydroArea.CoordinateSystem.Name != shapefile.CoordinateSystem.Name)
            {
                coordinateTransformation = new OgrCoordinateSystemFactory().CreateTransformation(shapefile.CoordinateSystem, targetHydroArea.CoordinateSystem);
            }

            IList features = shapefile.Features;
            foreach (var feature in features)
            {
                Embankment embankment = new Embankment()
                {
                    Geometry = ((IFeature) feature).Geometry
                };

                if (coordinateTransformation != null)
                {
                    embankment.Geometry = GeometryTransform.TransformGeometry(embankment.Geometry, coordinateTransformation.MathTransform);
                }
                embankment.Name = NamingHelper.GetUniqueName("Embankment{0:D2}", targetHydroArea.Embankments); 
                targetHydroArea.Embankments.Add(embankment);
            }

            return target; 
        }
    }
}
