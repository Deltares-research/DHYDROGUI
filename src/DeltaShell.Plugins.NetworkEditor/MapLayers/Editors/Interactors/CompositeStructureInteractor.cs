using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Converters.Geometries;
using SharpMap.Editors;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class CompositeStructureInteractor : FeatureInteractor, INetworkFeatureInteractor
    {
        private bool addingNew;
        private Bitmap ImageTracker { get; set; }
        private Bitmap SelectedImageTracker { get; set; }

        public CompositeStructureInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject)
            : base(layer, feature, vectorStyle, editableObject)
        {
        }

        protected override void CreateTrackers()
        {
            var compositeStructure = (ICompositeBranchStructure)SourceFeature;
            if (compositeStructure.Structures.Count <= 1)
            {
                // only when there are 2 or more structures the structureFeature is visible
                return;
            }
            ImageTracker = (VectorStyle != null) ? TrackerSymbolHelper.GenerateComposite(new Pen(Color.Lime),
                                                                 new SolidBrush(Color.Green),
                                                                 compositeStructure.Structures.Count * VectorStyle.Symbol.Width,
                                                                 VectorStyle.Symbol.Height/2,
                                                                 3,
                                                                 3) : null;

            SelectedImageTracker = (VectorStyle != null) ? TrackerSymbolHelper.GenerateComposite(new Pen(Color.DarkBlue),
                                                                         new SolidBrush(Color.LightSkyBlue),
                                                                         compositeStructure.Structures.Count * VectorStyle.Symbol.Width,
                                                                         VectorStyle.Symbol.Height/2,
                                                                         8,
                                                                         8) : null;

            var geometry = GeometryFactory.CreatePoint(CalculateCoordinate(SourceFeature.Geometry));
            Trackers.Add(new TrackerFeature(this, geometry, 0, SelectedImageTracker));
            Trackers[0].Selected = true;
        }
        
        private Coordinate CalculateCoordinate(IGeometry geometry)
        {
            if (Layer == null)
            {
                return (Coordinate)geometry.Coordinates[0].Clone();
            }

            if (Layer.CoordinateTransformation != null)
            {
                geometry = SharpMap.CoordinateSystems.Transformations.GeometryTransform.TransformGeometry(geometry, Layer.CoordinateTransformation.MathTransform);
            }

            var c1 = Layer.Map.ImageToWorld(new PointF(0, 0));

            // a bitmap is horizontal and vertical centered by the sharpmap renderer.
            // The y position should be:
            //
            //  x---xx---xx---xx---x   
            //  |   ||   ||   ||   |   -                                 = SourceFeature.Geometry.Coordinates[0].Y
            //  x---xx---xx---xx---x   |  = VectorStyle.Symbol.Height/2  |
            //  x------------------x   |                                 |
            //  |                  |   -  = VectorStyle.Symbol.Height/4  - = 3*VectorStyle.Symbol.Height/4
            //  x------------------x   
            int offset = 3 * VectorStyle.Symbol.Height / 4;
            var c2 = Layer.Map.ImageToWorld(new PointF(0, offset));

            return new Coordinate(geometry.Coordinates[0].X,geometry.Coordinates[0].Y - (c1.Y - c2.Y));
        }

        public override bool MoveTracker(TrackerFeature trackerFeature, double deltaX, double deltaY, SnapResult snapResult = null)
        {
            if (snapResult != null)
            {
                TargetFeature.Geometry = (IGeometry)trackerFeature.Geometry.Clone();

                var coordinate = CalculateCoordinate(trackerFeature.Geometry);
                Trackers[0].Geometry.Coordinates[0].X = coordinate.X;
                Trackers[0].Geometry.Coordinates[0].Y = coordinate.Y;
            }
            
            return base.MoveTracker(trackerFeature, deltaX, deltaY, snapResult);
        }

        public override void UpdateTracker(IGeometry geometry)
        {
            var coordinate = CalculateCoordinate(geometry);
            Trackers[0].Geometry.Coordinates[0].X = coordinate.X;
            Trackers[0].Geometry.Coordinates[0].Y = coordinate.Y;
        }
        public override void SetTrackerSelection(TrackerFeature trackerFeature, bool select)
        {
            if (trackerFeature.Selected == select) 
                return;
            trackerFeature.Selected = select;
            trackerFeature.Bitmap = select ? SelectedImageTracker : ImageTracker;
        }

        public override void Stop()
        {
            Stop(null);
        }

        public override void Stop(SnapResult snapResult)
        {
            if (!addingNew && snapResult == null) //cancel
            {
                TargetFeature = null;
                return;
            }

            var compositeBranchStructure = (ICompositeBranchStructure)SourceFeature;
            var branchFeature = (IBranchFeature)SourceFeature;
            var channel = (IChannel)branchFeature.Branch;
            var oldBranch = compositeBranchStructure.Branch;
            var structures = compositeBranchStructure.Structures.ToList();

            channel?.BranchFeatures.Remove(compositeBranchStructure);

            compositeBranchStructure.Branch = null;

            base.Stop();

            var tolerance = Layer == null ? Tolerance : MapHelper.ImageToWorld(Layer.Map, 1);
            if (Network != null)
            {
                NetworkHelper.AddBranchFeatureToNearestBranch(Network.Branches, compositeBranchStructure, tolerance);  
            }

            NetworkHelper.UpdateBranchFeatureChainageFromGeometry(compositeBranchStructure);

            foreach (var structure in structures)
            {
                structure.Geometry = (IGeometry)branchFeature.Geometry.Clone();
                if (oldBranch != compositeBranchStructure.Branch)
                {
                    channel?.BranchFeatures.Remove(structure);
                    structure.Branch = compositeBranchStructure.Branch;
                    compositeBranchStructure.Branch?.BranchFeatures.Add(structure);
                }
            }
        }

        public override void Add(IFeature feature)
        {
            addingNew = true;
            Start();
            Stop();
            addingNew = false;
        }
        public override void Delete()
        {
            var compositeBranchStructure = (ICompositeBranchStructure)SourceFeature;
            if (null == compositeBranchStructure.Branch)
            {
                // test for cascading removal by topology rule
                return;
            }
            compositeBranchStructure.Branch.BranchFeatures.Remove(compositeBranchStructure);
            compositeBranchStructure.Branch = null;
            foreach (var structure in compositeBranchStructure.Structures)
            {
                structure.Branch.BranchFeatures.Remove(structure);
                structure.Branch = null;
            }
            Layer.RenderRequired = true;
        }

        public override IEnumerable<IFeatureRelationInteractor> GetFeatureRelationInteractors(IFeature feature)
        {
            return HydroNetworkFeatureEditor.GetFeatureRelationInteractor(feature);
        }

        protected override bool AllowMoveCore()
        {
            return true;
        }

        protected override bool AllowDeletionCore()
        {
            return true;
        }

        public override bool AllowSingleClickAndMove()
        {
            return true;
        }

        public INetwork Network { get; set; }
    }
}