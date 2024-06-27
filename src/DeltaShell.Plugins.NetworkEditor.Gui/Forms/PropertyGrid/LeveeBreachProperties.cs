using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "LeveeBreachProperties_DisplayName")]
    public class LeveeBreachProperties : ObjectProperties<ILeveeBreach>
    {
        private NameValidator nameValidator = NameValidator.CreateDefault();
        
        private bool useBreachLocationSnapping;

        public override ILeveeBreach Data
        {
            [ExcludeFromCodeCoverage]
            get { return data; }
            set
            {
                data = value;
                if (data?.BreachLocation != null && data.Geometry != null)
                    useBreachLocationSnapping = GeometryHelper.PointIsOnLineBetweenPreviousAndNext(data.Geometry.Coordinates.First(), data.BreachLocation.Coordinate, data.Geometry.Coordinates.Last());
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        [ExcludeFromCodeCoverage]
        public string Name
        {
            get { return data.Name; }
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    data.Name = value;
                }
            }
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
                BreachLocationX = data.BreachLocationX;
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Breach Location X")]
        [PropertyOrder(3)]
        public double BreachLocationX
        {
            get { return data.BreachLocationX; }
            set {
                if (useBreachLocationSnapping)
                {
                    var beginPoint = data.Geometry.Coordinates.First();
                    var endPoint = data.Geometry.Coordinates.Last();
                    var xDiff = endPoint.X - beginPoint.X;
                    var yDiff = endPoint.Y - beginPoint.Y;
                    //value is nieuwe X, nu nieuwe Y uitrekenen
                    var ratio = yDiff / xDiff;
                    var newYLocation = endPoint.Y - ((endPoint.X - value) * ratio);
                    data.BreachLocationY = newYLocation;
                }
                data.BreachLocationX = value;
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Breach Location Y")]
        [PropertyOrder(4)]
        public double BreachLocationY
        {
            get { return data.BreachLocationY; }
            set
            {
                if (useBreachLocationSnapping)
                {
                    var beginPoint = data.Geometry.Coordinates.First();
                    var endPoint = data.Geometry.Coordinates.Last();
                    var xDiff = endPoint.X - beginPoint.X;
                    var yDiff = endPoint.Y - beginPoint.Y;
                    //value is nieuwe Y, nu nieuwe X uitrekenen
                    var ratio = yDiff / xDiff;
                    var newXLocation = endPoint.X - ((endPoint.Y - value) / ratio);
                    data.BreachLocationX = newXLocation;
                }
                data.BreachLocationY = value;
            }
        }
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee Breach Formula")]
        [DynamicVisible]
        [PropertyOrder(5)]
        [ExcludeFromCodeCoverage]
        public LeveeBreachGrowthFormula LeveeBreachFormula
        {
            get { return data.LeveeBreachFormula; }
            set { data.LeveeBreachFormula = value; }
        }
        
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Breach Growth Active")]
        [PropertyOrder(6)]
        public bool BreachGrowthActive
        {
            get {
                var activeLeveeBreachSettings = data.GetActiveLeveeBreachSettings();
                return activeLeveeBreachSettings.BreachGrowthActive;
            }
            set
            {
                var activeLeveeBreachSettings = data.GetActiveLeveeBreachSettings();
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
                var activeLeveeBreachSettings = data.GetActiveLeveeBreachSettings();
                return activeLeveeBreachSettings.StartTimeBreachGrowth;
            }
            set
            {
                var activeLeveeBreachSettings = data.GetActiveLeveeBreachSettings();
                activeLeveeBreachSettings.StartTimeBreachGrowth = value;
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Use Waterlevelstream")]
        [PropertyOrder(8)]
        [ExcludeFromCodeCoverage]
        public bool WaterLevelFlowLocationsActive
        {
            get { return data.WaterLevelFlowLocationsActive; }
            set { data.WaterLevelFlowLocationsActive = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Waterlevelstream up x-location")]
        [DynamicVisible]
        [PropertyOrder(9)]
        [ExcludeFromCodeCoverage]
        public double WaterLevelUpstreamLocationX
        {
            get { return data.WaterLevelUpstreamLocationX; }
            set { data.WaterLevelUpstreamLocationX= value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Waterlevelstream up y-location")]
        [DynamicVisible]
        [PropertyOrder(10)]
        [ExcludeFromCodeCoverage]
        public double WaterLevelUpstreamLocationY
        {
            get { return data.WaterLevelUpstreamLocationY; }
            set { data.WaterLevelUpstreamLocationY = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Waterlevelstream down x-location")]
        [DynamicVisible]
        [PropertyOrder(11)]
        public double WaterLevelDownstreamLocationX
        {
            get { return data.WaterLevelDownstreamLocationX; }
            set { data.WaterLevelDownstreamLocationX = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Waterlevelstream down y-location")]
        [DynamicVisible]
        [PropertyOrder(12)]
        public double WaterLevelDownstreamLocationY
        {
            get { return data.WaterLevelDownstreamLocationY; }
            set { data.WaterLevelDownstreamLocationY = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee start x-location")]
        [PropertyOrder(13)]
        [ExcludeFromCodeCoverage]
        public double LeveeStartLocationX
        {
            get { return data.Geometry.Coordinates[0].X; }
            set { data.Geometry.Coordinates[0].X = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee start y-location")]
        [PropertyOrder(14)]
        [ExcludeFromCodeCoverage]
        public double LeveeStartLocationY
        {
            get { return data.Geometry.Coordinates[0].Y; }
            set { data.Geometry.Coordinates[0].Y = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee end x-location")]
        [PropertyOrder(15)]
        [ExcludeFromCodeCoverage]
        public double LeveeEndLocationX
        {
            get { return data.Geometry.Coordinates[1].X; }
            set { data.Geometry.Coordinates[1].X = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee end x-location")]
        [PropertyOrder(16)]
        [ExcludeFromCodeCoverage]
        public double LeveeEndLocationY
        {
            get { return data.Geometry.Coordinates[1].Y; }
            set { data.Geometry.Coordinates[1].Y = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Levee Geometry length")]
        [PropertyOrder(17)]
        [ExcludeFromCodeCoverage]
        public double LeveeGeometryLength
        {
            get { return data.Geometry.Length; }
        }

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            
            if (propertyName == nameof(WaterLevelUpstreamLocationX) ||
                propertyName == nameof(WaterLevelUpstreamLocationY) ||
                propertyName == nameof(WaterLevelDownstreamLocationX) ||
                propertyName == nameof(WaterLevelDownstreamLocationY) )
            {
                return data.WaterLevelFlowLocationsActive;
            }

            if (propertyName == nameof(LeveeBreachFormula) ||
                propertyName == nameof(StartTimeBreachGrowth))
            {
                return BreachGrowthActive;
            }

            return true;
        }
        
        /// <summary>
        /// Get or set the <see cref="NameValidator"/> for this instance.
        /// Property is initialized with a default name validator. 
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public NameValidator NameValidator
        {
            get => nameValidator;
            set
            {
                Ensure.NotNull(value, nameof(value));
                nameValidator = value;
            }
        }
    }

}