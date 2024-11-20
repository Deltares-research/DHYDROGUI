using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using NetTopologySuite.Extensions.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class LeveeBreachViewModel : INotifyPropertyChanged
    {
        private LeveeBreach leveeBreach;
        private bool useSnapping;
        private bool leveeBreachWaterLevelFlowLocationsActive;

        public LeveeBreach LeveeBreach
        {
            get { return leveeBreach; }
            set
            {
                if (leveeBreach != null)
                {
                    ((INotifyPropertyChanged)leveeBreach).PropertyChanged -= OnLeveeBreachPropertyChanged;
                }

                leveeBreach = value;

                if (leveeBreach == null) return;
                if (leveeBreach != null)
                {
                    ((INotifyPropertyChanged)leveeBreach).PropertyChanged += OnLeveeBreachPropertyChanged;
                }

                if (leveeBreach.BreachLocation != null && leveeBreach.Geometry != null)
                    UseSnapping = GeometryHelper.PointIsOnLineBetweenPreviousAndNext(
                        leveeBreach.Geometry.Coordinates.First(), leveeBreach.BreachLocation.Coordinate,
                        leveeBreach.Geometry.Coordinates.Last());
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedGrowthFormula));
                OnPropertyChanged(nameof(LeveeBreachSettings));
                OnPropertyChanged(nameof(UseActive));
                OnPropertyChanged(nameof(BreachLocationX));
                OnPropertyChanged(nameof(BreachLocationY));
                OnPropertyChanged(nameof(UseWaterLevelFlowLocation));
                OnPropertyChanged(nameof(WaterLevelUpstreamLocationX));
                OnPropertyChanged(nameof(WaterLevelUpstreamLocationY));
                OnPropertyChanged(nameof(WaterLevelDownstreamLocationX));
                OnPropertyChanged(nameof(WaterLevelDownstreamLocationY));

            }
        }

        private void OnLeveeBreachPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ILeveeBreach.BreachLocationX))
                OnPropertyChanged(nameof(BreachLocationX));

            if (e.PropertyName == nameof(ILeveeBreach.BreachLocationY))
                OnPropertyChanged(nameof(BreachLocationY));

            if (e.PropertyName == nameof(ILeveeBreach.WaterLevelUpstreamLocationX))
                OnPropertyChanged(nameof(WaterLevelUpstreamLocationX));

            if (e.PropertyName == nameof(ILeveeBreach.WaterLevelUpstreamLocationY))
                OnPropertyChanged(nameof(WaterLevelUpstreamLocationY));

            if (e.PropertyName == nameof(ILeveeBreach.WaterLevelDownstreamLocationX))
                OnPropertyChanged(nameof(WaterLevelDownstreamLocationX));

            if (e.PropertyName == nameof(ILeveeBreach.WaterLevelDownstreamLocationY))
                OnPropertyChanged(nameof(WaterLevelDownstreamLocationY));

            if (e.PropertyName == nameof(ILeveeBreach.WaterLevelFlowLocationsActive))
                OnPropertyChanged(nameof(UseWaterLevelFlowLocation));

            if (e.PropertyName == nameof(ILeveeBreach.LeveeBreachFormula))
                OnPropertyChanged(nameof(SelectedGrowthFormula));
        }

        public LeveeBreachGrowthFormula SelectedGrowthFormula
        {
            get { return LeveeBreach?.LeveeBreachFormula ?? LeveeBreachGrowthFormula.VerheijvdKnaap2002; }
            set
            {
                if (LeveeBreach == null) return;
                LeveeBreach.LeveeBreachFormula = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LeveeBreachSettings));
            }
        }

        [ExcludeFromCodeCoverage]
        public bool UseActive
        {
            get { return LeveeBreachSettings?.BreachGrowthActive ?? false; }
            set
            {
                if (LeveeBreachSettings == null) return;
                LeveeBreachSettings.BreachGrowthActive = value;
                OnPropertyChanged();
            }
        }
        public bool UseWaterLevelFlowLocation
        {
            get
            {
                leveeBreachWaterLevelFlowLocationsActive = LeveeBreach?.WaterLevelFlowLocationsActive ?? false;
                if (leveeBreachWaterLevelFlowLocationsActive)
                {
                    if (Equals(LeveeBreach.WaterLevelUpstreamLocationX, default(double)))
                    {
                        LeveeBreach.WaterLevelUpstreamLocationX = LeveeBreach.BreachLocationX;
                        OnPropertyChanged(nameof(WaterLevelUpstreamLocationX));
                    }

                    if (Equals(LeveeBreach.WaterLevelUpstreamLocationY, default(double)))
                    {
                        LeveeBreach.WaterLevelUpstreamLocationY = LeveeBreach.BreachLocationY;
                        OnPropertyChanged(nameof(WaterLevelUpstreamLocationY));
                    }

                    if (Equals(LeveeBreach.WaterLevelDownstreamLocationX, default(double)))
                    {
                        LeveeBreach.WaterLevelDownstreamLocationX = LeveeBreach.BreachLocationX;
                        OnPropertyChanged(nameof(WaterLevelDownstreamLocationX));
                    }

                    if (Equals(LeveeBreach.WaterLevelDownstreamLocationY, default(double)))
                    {
                        LeveeBreach.WaterLevelDownstreamLocationY = LeveeBreach.BreachLocationY;
                        OnPropertyChanged(nameof(WaterLevelDownstreamLocationY));
                    }
                }
                return leveeBreachWaterLevelFlowLocationsActive;
            }
            set
            {
                if (LeveeBreach == null) return;
                LeveeBreach.WaterLevelFlowLocationsActive = value;
                if (LeveeBreach.WaterLevelFlowLocationsActive)
                {
                    if (Equals(LeveeBreach.WaterLevelUpstreamLocationX, default(double)))
                        LeveeBreach.WaterLevelUpstreamLocationX = LeveeBreach.BreachLocationX;
                    if (Equals(LeveeBreach.WaterLevelUpstreamLocationY, default(double)))
                        LeveeBreach.WaterLevelUpstreamLocationY = LeveeBreach.BreachLocationY;
                    if (Equals(LeveeBreach.WaterLevelDownstreamLocationX, default(double)))
                        LeveeBreach.WaterLevelDownstreamLocationX = LeveeBreach.BreachLocationX;
                    if (Equals(LeveeBreach.WaterLevelDownstreamLocationY, default(double)))
                        LeveeBreach.WaterLevelDownstreamLocationY = LeveeBreach.BreachLocationY;
                }

                OnPropertyChanged(nameof(UseWaterLevelFlowLocation));
                OnPropertyChanged(nameof(WaterLevelUpstreamLocationX));
                OnPropertyChanged(nameof(WaterLevelUpstreamLocationY));
                OnPropertyChanged(nameof(WaterLevelDownstreamLocationX));
                OnPropertyChanged(nameof(WaterLevelDownstreamLocationY));
                OnPropertyChanged();
            }
        }

        [ExcludeFromCodeCoverage]
        public LeveeBreachSettings LeveeBreachSettings
        {
            get { return LeveeBreach?.GetActiveLeveeBreachSettings(); }
        }

        public double BreachLocationX
        {
            get { return LeveeBreach.BreachLocationX; }
            set
            {
                if (UseSnapping)
                {
                    var beginPoint = leveeBreach.Geometry.Coordinates.First();
                    var endPoint = leveeBreach.Geometry.Coordinates.Last();
                    var xDiff = endPoint.X - beginPoint.X;
                    var yDiff = endPoint.Y - beginPoint.Y;
                    //value is nieuwe X, nu nieuwe Y uitrekenen
                    var ratio = yDiff / xDiff;
                    var newYLocation = endPoint.Y - ((endPoint.X - value) * ratio);
                    LeveeBreach.BreachLocationY = newYLocation;
                }
                LeveeBreach.BreachLocationX = value;
                OnPropertyChanged(nameof(BreachLocationX));
                OnPropertyChanged(nameof(BreachLocationY));
                OnPropertyChanged();
            }
        }
        public double BreachLocationY
        {
            get { return LeveeBreach.BreachLocationY; }
            set
            {
                if (useSnapping)
                {
                    var beginPoint = leveeBreach.Geometry.Coordinates.First();
                    var endPoint = leveeBreach.Geometry.Coordinates.Last();
                    var xDiff = endPoint.X - beginPoint.X;
                    var yDiff = endPoint.Y - beginPoint.Y;
                    //value is nieuwe Y, nu nieuwe X uitrekenen
                    var ratio = yDiff / xDiff;
                    var newXLocation = endPoint.X - ((endPoint.Y - value) / ratio);
                    LeveeBreach.BreachLocationX = newXLocation;
                }
                LeveeBreach.BreachLocationY = value;
                OnPropertyChanged(nameof(BreachLocationX));
                OnPropertyChanged(nameof(BreachLocationY));
                OnPropertyChanged();
            }
        }

        [ExcludeFromCodeCoverage]
        public double WaterLevelUpstreamLocationX
        {
            get { return LeveeBreach.WaterLevelUpstreamLocationX; }
            set
            {
                LeveeBreach.WaterLevelUpstreamLocationX = value;
                OnPropertyChanged(nameof(WaterLevelUpstreamLocationX));
                OnPropertyChanged();

            }
        }
        [ExcludeFromCodeCoverage]
        public double WaterLevelUpstreamLocationY
        {
            get { return LeveeBreach.WaterLevelUpstreamLocationY; }
            set { LeveeBreach.WaterLevelUpstreamLocationY = value; 
                OnPropertyChanged(nameof(WaterLevelUpstreamLocationY));
                OnPropertyChanged();
            }
        }
        [ExcludeFromCodeCoverage]
        public double WaterLevelDownstreamLocationX
        {
            get { return LeveeBreach.WaterLevelDownstreamLocationX; }
            set
            {
                LeveeBreach.WaterLevelDownstreamLocationX = value;
                OnPropertyChanged(nameof(WaterLevelDownstreamLocationX));
                OnPropertyChanged();
            }
        }
        [ExcludeFromCodeCoverage]
        public double WaterLevelDownstreamLocationY
        {
            get { return LeveeBreach.WaterLevelDownstreamLocationY; }
            set
            {
                LeveeBreach.WaterLevelDownstreamLocationY = value;
                OnPropertyChanged(nameof(WaterLevelDownstreamLocationY));
                OnPropertyChanged();
            }
        }

        public bool UseSnapping
        {
            get { return useSnapping; }
            set
            {
                useSnapping = value;
                BreachLocationX = LeveeBreach.BreachLocationX;
                OnPropertyChanged(nameof(useSnapping));
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}