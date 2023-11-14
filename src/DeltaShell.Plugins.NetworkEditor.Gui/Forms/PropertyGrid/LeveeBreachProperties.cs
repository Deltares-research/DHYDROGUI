using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "LeveeBreachProperties_DisplayName")]
    public class LeveeBreachProperties : ObjectProperties<ILeveeBreach>
    {
        private bool useBreachLocationSnapping;
        private ILeveeBreach leveeBreach;

        public override object Data
        {
            [ExcludeFromCodeCoverage]
            get { return leveeBreach; }
            set
            {
                leveeBreach = (ILeveeBreach) value;
                if (leveeBreach?.BreachLocation != null && leveeBreach?.Geometry != null)
                    useBreachLocationSnapping = GeometryHelper.PointIsOnLineBetweenPreviousAndNext(leveeBreach.Geometry.Coordinates.First(), leveeBreach.BreachLocation.Coordinate, leveeBreach.Geometry.Coordinates.Last());
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        [ExcludeFromCodeCoverage]
        public string Name
        {
            get { return leveeBreach.Name; }
            set { leveeBreach.SetNameIfValid(value); }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Use Breach Location snapping")]
        [PropertyOrder(2)]
        public bool UseBreachLocationSnapping
        {
            get { return useBreachLocationSnapping; }
            set
            {
                useBreachLocationSnapping = value;
                BreachLocationX = leveeBreach.BreachLocationX;
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Breach Location X")]
        [PropertyOrder(3)]
        public double BreachLocationX
        {
            get { return leveeBreach.BreachLocationX; }
            set {
                if (useBreachLocationSnapping)
                {
                    var beginPoint = leveeBreach.Geometry.Coordinates.First();
                    var endPoint = leveeBreach.Geometry.Coordinates.Last();
                    var xDiff = endPoint.X - beginPoint.X;
                    var yDiff = endPoint.Y - beginPoint.Y;
                    //value is nieuwe X, nu nieuwe Y uitrekenen
                    var ratio = yDiff / xDiff;
                    var newYLocation = endPoint.Y - ((endPoint.X - value) * ratio);
                    leveeBreach.BreachLocationY = newYLocation;
                }
                leveeBreach.BreachLocationX = value;
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Breach Location Y")]
        [PropertyOrder(4)]
        public double BreachLocationY
        {
            get { return leveeBreach.BreachLocationY; }
            set
            {
                if (useBreachLocationSnapping)
                {
                    var beginPoint = leveeBreach.Geometry.Coordinates.First();
                    var endPoint = leveeBreach.Geometry.Coordinates.Last();
                    var xDiff = endPoint.X - beginPoint.X;
                    var yDiff = endPoint.Y - beginPoint.Y;
                    //value is nieuwe Y, nu nieuwe X uitrekenen
                    var ratio = yDiff / xDiff;
                    var newXLocation = endPoint.X - ((endPoint.Y - value) / ratio);
                    leveeBreach.BreachLocationX = newXLocation;
                }
                leveeBreach.BreachLocationY = value;
            }
        }
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee Breach Formula")]
        [DynamicVisible]
        [PropertyOrder(5)]
        [ExcludeFromCodeCoverage]
        public LeveeBreachGrowthFormula LeveeBreachFormula
        {
            get { return leveeBreach.LeveeBreachFormula; }
            set { leveeBreach.LeveeBreachFormula = value; }
        }
        
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Breach Growth Active")]
        [PropertyOrder(6)]
        public bool BreachGrowthActive
        {
            get {
                var activeLeveeBreachSettings = leveeBreach.GetActiveLeveeBreachSettings();
                return activeLeveeBreachSettings.BreachGrowthActive;
            }
            set
            {
                var activeLeveeBreachSettings = leveeBreach.GetActiveLeveeBreachSettings();
                activeLeveeBreachSettings.BreachGrowthActive = value;
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("StartTime Breach Growth")]
        [DynamicVisible]
        [PropertyOrder(7)]
        public DateTime StartTimeBreachGrowth
        {
            get {
                var activeLeveeBreachSettings = leveeBreach.GetActiveLeveeBreachSettings();
                return activeLeveeBreachSettings.StartTimeBreachGrowth;
            }
            set
            {
                var activeLeveeBreachSettings = leveeBreach.GetActiveLeveeBreachSettings();
                activeLeveeBreachSettings.StartTimeBreachGrowth = value;
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Use Waterlevelstream")]
        [PropertyOrder(8)]
        [ExcludeFromCodeCoverage]
        public bool WaterLevelFlowLocationsActive
        {
            get { return leveeBreach.WaterLevelFlowLocationsActive; }
            set { leveeBreach.WaterLevelFlowLocationsActive = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Waterlevelstream up x-location")]
        [DynamicVisible]
        [PropertyOrder(9)]
        [ExcludeFromCodeCoverage]
        public double WaterLevelUpstreamLocationX
        {
            get { return leveeBreach.WaterLevelUpstreamLocationX; }
            set { leveeBreach.WaterLevelUpstreamLocationX= value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Waterlevelstream up y-location")]
        [DynamicVisible]
        [PropertyOrder(10)]
        [ExcludeFromCodeCoverage]
        public double WaterLevelUpstreamLocationY
        {
            get { return leveeBreach.WaterLevelUpstreamLocationY; }
            set { leveeBreach.WaterLevelUpstreamLocationY = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Waterlevelstream down x-location")]
        [DynamicVisible]
        [PropertyOrder(11)]
        public double WaterLevelDownstreamLocationX
        {
            get { return leveeBreach.WaterLevelDownstreamLocationX; }
            set { leveeBreach.WaterLevelDownstreamLocationX = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Waterlevelstream down y-location")]
        [DynamicVisible]
        [PropertyOrder(12)]
        public double WaterLevelDownstreamLocationY
        {
            get { return leveeBreach.WaterLevelDownstreamLocationY; }
            set { leveeBreach.WaterLevelDownstreamLocationY = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee start x-location")]
        [PropertyOrder(13)]
        [ExcludeFromCodeCoverage]
        public double LeveeStartLocationX
        {
            get { return leveeBreach.Geometry.Coordinates[0].X; }
            set { leveeBreach.Geometry.Coordinates[0].X = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee start y-location")]
        [PropertyOrder(14)]
        [ExcludeFromCodeCoverage]
        public double LeveeStartLocationY
        {
            get { return leveeBreach.Geometry.Coordinates[0].Y; }
            set { leveeBreach.Geometry.Coordinates[0].Y = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee end x-location")]
        [PropertyOrder(15)]
        [ExcludeFromCodeCoverage]
        public double LeveeEndLocationX
        {
            get { return leveeBreach.Geometry.Coordinates[1].X; }
            set { leveeBreach.Geometry.Coordinates[1].X = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee end x-location")]
        [PropertyOrder(16)]
        [ExcludeFromCodeCoverage]
        public double LeveeEndLocationY
        {
            get { return leveeBreach.Geometry.Coordinates[1].Y; }
            set { leveeBreach.Geometry.Coordinates[1].Y = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee Geometry length")]
        [PropertyOrder(17)]
        [ExcludeFromCodeCoverage]
        public double LeveeGeometryLength
        {
            get { return leveeBreach.Geometry.Length; }
        }

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            
            if (propertyName == nameof(WaterLevelUpstreamLocationX) ||
                propertyName == nameof(WaterLevelUpstreamLocationY) ||
                propertyName == nameof(WaterLevelDownstreamLocationX) ||
                propertyName == nameof(WaterLevelDownstreamLocationY) )
            {
                return leveeBreach.WaterLevelFlowLocationsActive;
            }

            if (propertyName == nameof(LeveeBreachFormula) ||
                propertyName == nameof(StartTimeBreachGrowth))
            {
                return BreachGrowthActive;
            }

            return true;
        }
    }

}