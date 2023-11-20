using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Editors;
using SharpMap.Editors.Interactors;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class CrossSectionInteractor : FeatureInteractor, INetworkFeatureInteractor, IBranchMaintainableInteractor
    {
        public CrossSectionInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle,
                                      IEditableObject editableObject)
            : base(layer, feature, vectorStyle, editableObject)
        {
            var crossSection = (ICrossSection) SourceFeature;
            if ((crossSection != null) && (crossSection.Definition.GeometryBased))
            {
                LineStringInteractor = new LineStringInteractor(layer, feature, vectorStyle, editableObject);
            }
        }

        private LineStringInteractor LineStringInteractor { get; set; }

        public override IFallOffPolicy FallOffPolicy 
        { 
            get
            {
                var crossSection = (ICrossSection)SourceFeature;
                return crossSection.Definition.GeometryBased ? LineStringInteractor.FallOffPolicy : base.FallOffPolicy;
            }
            set
            {
                if (LineStringInteractor != null)
                {
                    LineStringInteractor.FallOffPolicy = value;
                }
                else
                {
                    base.FallOffPolicy = value;
                }
            } 
        }
        
        protected override void CreateTrackers()
        {
            if (SourceFeature == null)
            {
                return;
            }

            // If we delegate to LineStringInteractor, it will create trackers at construction.
            if (LineStringInteractor != null)
            {
                return;
            }

            Trackers.Clear();

            var imageTracker = TrackerSymbolHelper.GenerateSimple(new Pen(Color.Lime), new SolidBrush(Color.Green), 6, 6);

            var geometry = GetGeometry();

            if (Layer?.CoordinateTransformation != null)
            {
                geometry = GeometryTransform.TransformGeometry(geometry, Layer.CoordinateTransformation.MathTransform);
            }

            for (int i = 0; i < geometry.Coordinates.Length; i++)
            {
                var coordinate = geometry.Coordinates[i];
                var selectPoint = new NetTopologySuite.Geometries.Point(coordinate.X, coordinate.Y);
                
                Trackers.Add(new TrackerFeature(this, selectPoint, i, imageTracker));
            }
        }

        private IGeometry GetGeometry()
        {
            var csr= Layer?.CustomRenderers.OfType<CrossSectionRenderer>().FirstOrDefault();
            if (csr != null && csr.UseDefaultLength)
            {
                return csr.GetDefaultGeometry(SourceFeature as ICrossSection);
            }
            return SourceFeature.Geometry;
        }

        public override void Start()
        {
            if (LineStringInteractor != null)
            {
                LineStringInteractor.Start();
                TargetFeature = LineStringInteractor.TargetFeature;
            }
            else
            {
                base.Start();
            }
        }

        public override void Add(IFeature feature)
        {
            var crossSection = (ICrossSection)feature;
            
            if (crossSection.CrossSectionType == CrossSectionType.GeometryBased)
            {
                LineStringInteractor = new LineStringInteractor(Layer, feature, VectorStyle, EditableObject);
                CrossSectionHelper.AddDefaultZToGeometry((CrossSectionDefinitionXYZ)crossSection.Definition);
            }
            else
            {
                LineStringInteractor = null;
            }

            if (crossSection.CrossSectionType == CrossSectionType.ZW &&
                crossSection.Definition != null &&
                crossSection.Definition.Sections != null &&
                crossSection.Definition.Sections.Count == 0)
            {
                var hydroNetwork = Network as IHydroNetwork;
                if (hydroNetwork != null)
                {
                    var crossSectionSectionType = hydroNetwork.CrossSectionSectionTypes.FirstOrDefault(csst =>string.Equals(csst.Name, RoughnessDataSet.MainSectionTypeName, StringComparison.InvariantCultureIgnoreCase));
                    if (crossSectionSectionType != null)
                    {
                        crossSection.Definition.Sections.Add(
                            new CrossSectionSection
                            {
                                MinY = 0d,
                                MaxY = (Math.Abs(crossSection.Definition.Width)/2),
                                SectionType = crossSectionSectionType,
                            });
                    }
                }
            }
            Start();
            StopOrFlop(true);
        }

        public INetwork Network { get; set; }
        
        public override void Stop()
        {
            StopOrFlop(false);
        }

        public override void Stop(SnapResult snapResult)
        {
            StopOrFlop(false);
        }

        public void Stop(SnapResult snapResult, bool stayOnSameBranch)
        {
            StopOrFlop(false, stayOnSameBranch);
        }

        private void StopOrFlop(bool add, bool stayOnSameBranch=false)
        {
            var network = Network;
            var branchFeature = SourceFeature as IBranchFeature;
            if (branchFeature == null) return;

            var crossSection = (ICrossSection) SourceFeature;
            var channel = (IChannel) crossSection.Branch;
            if (null != channel)
            {
                network = channel.Network;
            }

            base.Stop();

            branchFeature.Chainage = ((IBranchFeature) TargetFeature).Chainage;
                
            // commit changes
            var crossSectionDefinition = crossSection.Definition;
            if (LineStringInteractor == null) //non geometry-based cross section
            {
                var tolerance = Layer == null ? Tolerance : MapHelper.ImageToWorld(Layer.Map, 1);
                var geometryBeforeAdd = TargetFeature.Geometry;

                var targetBranch = stayOnSameBranch
                    ? channel
                    : NetworkHelper.GetNearestBranch(network.Branches, geometryBeforeAdd,
                        tolerance) ?? channel;
                if (targetBranch == null)
                {
                    throw new ArgumentException("Could not find branch matching the given cross section geometry");
                }

                NetworkHelper.AddBranchFeatureToBranch(crossSection, targetBranch);  
                    
                if (add)
                {
                    crossSection.Chainage = BranchFeature.SnapChainage(crossSection.Branch.Length,
                        NetworkHelper
                            .GetBranchFeatureChainageFromGeometry(
                                crossSection.Branch,
                                geometryBeforeAdd));
                }

                var mapChainage = NetworkHelper.MapChainage(crossSection.Branch, crossSection.Chainage);

                if (crossSectionDefinition.Width > 0)
                {
                    // AddBranchFeatureToNearestBranch messes up the offset if BranchFeature is LineString
                    crossSection.Geometry = CrossSectionHelper.ComputeDefaultCrossSectionGeometry(crossSection.Branch.Geometry,
                        mapChainage,
                        crossSectionDefinition.Width, crossSectionDefinition.Thalweg,
                        crossSectionDefinition.GetProfile().First().X);
                }
            }
            else
            {
                var tolerance = MapHelper.ImageToWorld(Layer.Map, 1);
                NetworkHelper.AddBranchFeatureToNearestBranch(network.Branches, crossSection, tolerance);
                if (add)
                {
                    NetworkHelper.UpdateBranchFeatureChainageFromGeometry(crossSection);
                }
            }
                
            AddDefaultSection(crossSectionDefinition, network);
        }

        private static void AddDefaultSection(ICrossSectionDefinition crossSectionDefinition, INetwork network)
        {
            if (crossSectionDefinition is CrossSectionDefinitionZW)
            {
                return;//default is dangerous for zw because it might not be one of the magic 3. main, fp1,fp2
            }
            var minY = 0.0;
            var maxY = 0.0;

            var hydroNetwork = ((IHydroNetwork)network);

            if (hydroNetwork.CrossSectionSectionTypes.Count == 0)
            {
                return;
            }

            var defaultSectionType = hydroNetwork.CrossSectionSectionTypes[0];

            if (crossSectionDefinition.GetProfile() != null && crossSectionDefinition.GetProfile().Count() > 0)
            {
                minY = crossSectionDefinition.GetProfile().First().X;
                maxY = crossSectionDefinition.GetProfile().Last().X;
            }

            if (crossSectionDefinition.Sections.Count == 0)
            {
                // add default section

                crossSectionDefinition.Sections.Add(new CrossSectionSection
                                                        {
                                                            MinY = minY,
                                                            MaxY = maxY,
                                                            SectionType =
                                                                defaultSectionType
                                                        });
            }
        }

        public override void UpdateTracker(IGeometry geometry)
        {
            var crossSection = (ICrossSection)SourceFeature;
            if (crossSection.GeometryBased) 
                return;

            var hasTwoCoordinates = geometry.Coordinates.Length >= 2;

            if ((hasTwoCoordinates && Trackers.Count == 1) ||
                (!hasTwoCoordinates && Trackers.Count == 2))
            {
                CreateTrackers();
                return;
            }

            geometry = GetGeometry();

            Trackers[0].Geometry.Coordinates[0].X = geometry.Coordinates[0].X;
            Trackers[0].Geometry.Coordinates[0].Y = geometry.Coordinates[0].Y;

            // Hack GeometryHelper.UpdateEnvelopeInternal doesn't work for point based geometry
            Trackers[0].Geometry.EnvelopeInternal.Init(Trackers[0].Geometry.Coordinates[0]);

            if (!hasTwoCoordinates) return;

            Trackers[1].Geometry.Coordinates[0].X = geometry.Coordinates[1].X;
            Trackers[1].Geometry.Coordinates[0].Y = geometry.Coordinates[1].Y;
            Trackers[1].Geometry.EnvelopeInternal.Init(Trackers[1].Geometry.Coordinates[0]);
        }

        public override IList<TrackerFeature> Trackers
        {
            get
            {
                if (LineStringInteractor != null)
                {
                    return LineStringInteractor.Trackers;
                }
                return base.Trackers;
            }
        }

        public override bool MoveTracker(TrackerFeature trackerFeature, double deltaX, double deltaY,
                                         SnapResult snapResult = null)
        {
            if (LineStringInteractor != null)
            {
                return LineStringInteractor.MoveTracker(trackerFeature, deltaX, deltaY, snapResult);
            }

            var crossSection = TargetFeature as ICrossSection;

            if (crossSection == null)
            {
                return false;
            }

            var oldLocation = crossSection.Geometry.Coordinates[0];

            var newLocation = snapResult == null
                                  ? new Coordinate(oldLocation.X + deltaX, oldLocation.Y + deltaY, oldLocation.Z)
                                  : snapResult.Location;

            var newBranch = snapResult == null ? crossSection.Branch : (IBranch) snapResult.SnappedFeature;

            var chainage = GeometryHelper.Distance((ILineString)newBranch.Geometry, newLocation);

            crossSection.Chainage = BranchFeature.SnapChainage(newBranch.Length,
                                                               NetworkHelper.CalculationChainage(newBranch, chainage));

            if (!Equals(crossSection.Branch, newBranch))
            {
                crossSection.Branch = newBranch;
            }

            UpdateTracker(crossSection.Geometry);

            return true;
        }

        public override bool InsertTracker(Coordinate coordinate, SnapResult snapResult)
        {
            if (LineStringInteractor != null)
            {
                var prevCoordinate = Trackers[snapResult.SnapIndexPrevious].Geometry.Coordinates[0];
                var nextCoordinate = Trackers[snapResult.SnapIndexPrevious + 1].Geometry.Coordinates[0];
                coordinate.Z = InterpolateZValueForCoordinate(prevCoordinate, nextCoordinate, coordinate);
                return LineStringInteractor.InsertTracker(coordinate, snapResult);
            }
            return base.InsertTracker(coordinate, snapResult);
        }

        public override bool RemoveTracker(TrackerFeature trackerFeature)
        {
            if (LineStringInteractor != null)
            {
                return LineStringInteractor.RemoveTracker(trackerFeature);
            }
            return base.RemoveTracker(trackerFeature);
        }

        private static double InterpolateZValueForCoordinate(Coordinate prev, Coordinate next, Coordinate coordinate)
        {
            if ((Double.IsNaN(prev.Z)) || (Double.IsNaN(next.Z)))
            {
                return Double.NaN;
            }

            var distanceToPrev = Math.Sqrt(Math.Pow(coordinate.X - prev.X, 2) + Math.Pow(coordinate.Y - prev.Y, 2));
            var distanceToNext = Math.Sqrt(Math.Pow(next.X - coordinate.X, 2) + Math.Pow(next.Y - coordinate.Y, 2));
            var fraction = distanceToPrev / (distanceToPrev + distanceToNext);
            return prev.Z + (next.Z - prev.Z) * fraction;
        }

        public override void SetTrackerSelection(TrackerFeature trackerFeature, bool select)
        {
            if (LineStringInteractor != null)
            {
                LineStringInteractor.SetTrackerSelection(trackerFeature, select);
            }
            else
            {
                base.SetTrackerSelection(trackerFeature, select);
            }
        }

        public override Cursor GetCursor(TrackerFeature trackerFeature)
        {
            return LineStringInteractor != null ? LineStringInteractor.GetCursor(trackerFeature) : base.GetCursor(trackerFeature);
        }

        public override TrackerFeature GetTrackerAtCoordinate(Coordinate worldPos)
        {
            var trackerFeature = LineStringInteractor != null
                ? LineStringInteractor.GetTrackerAtCoordinate(worldPos)
                : base.GetTrackerAtCoordinate(worldPos);

            if (trackerFeature == null)
            {
                var size = MapHelper.ImageToWorld(Layer.Map, 6, 6);
                var boundingBox = MapHelper.GetEnvelope(worldPos, size.X, size.Y);

                if (SourceFeature.Geometry.EnvelopeInternal.Intersects(boundingBox))
                {
                    trackerFeature = new TrackerFeature(this, null, -1, null);
                }
            }

            if (trackerFeature != null)
            {
                // set feature interactor to this (CrossSectionInteractor) to get the right behavior
                // during moving, deleting etc.
                trackerFeature.FeatureInteractor = this;
            }
            return trackerFeature;
        }

        protected override bool AllowDeletionCore()
        {
            return true;
        }

        protected override bool AllowMoveCore()
        {
            return true;
        }

        public override bool AllowSingleClickAndMove()
        {
            var crossSection = (ICrossSection)SourceFeature;
            return !crossSection.GeometryBased;
        }
    }
}