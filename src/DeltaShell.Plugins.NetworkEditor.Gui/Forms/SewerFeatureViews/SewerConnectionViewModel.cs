using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class SewerConnectionViewModel : INotifyPropertyChanged
    {
        private ISewerConnection sewerConnection;
        private RoughnessSection pipeRoughnessSection;

        public RoughnessSection PipeRoughnessSection
        {
            get { return pipeRoughnessSection; }
            set
            {
                if (pipeRoughnessSection != null)
                {
                    ((INotifyPropertyChanged) pipeRoughnessSection).PropertyChanged -= OnPropertyChanged;
                }
                pipeRoughnessSection = value;

                if (pipeRoughnessSection != null)
                {
                    ((INotifyPropertyChanged)pipeRoughnessSection).PropertyChanged += OnPropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(PipeRoughnessType));
                OnPropertyChanged(nameof(PipeRoughnessValue));
            }
        }

        public RoughnessType PipeRoughnessType
        {
            get
            {
                if (PipeRoughnessSection == null)
                    return default(RoughnessType);

                var roughnessFunctionType = PipeRoughnessSection.GetRoughnessFunctionType(SewerConnection);
                return roughnessFunctionType == RoughnessFunction.Constant 
                    ? PipeRoughnessSection.EvaluateRoughnessType(new NetworkLocation(sewerConnection, 0)) 
                    : PipeRoughnessSection.GetDefaultRoughnessType();
            }
        }

        public double PipeRoughnessValue
        {
            get
            {
                if (PipeRoughnessSection == null)
                    return default(double);
                var variableValueFilter = new VariableValueFilter<INetworkLocation>(
                    PipeRoughnessSection.RoughnessNetworkCoverage.Locations, new NetworkLocation(sewerConnection,0));

                var roughnessValues = PipeRoughnessSection.RoughnessNetworkCoverage.GetValues<double>(variableValueFilter);
                var roughnessFunctionType = PipeRoughnessSection.GetRoughnessFunctionType(SewerConnection);

                return roughnessValues.Count == 0 || roughnessFunctionType != RoughnessFunction.Constant 
                    ? PipeRoughnessSection.GetDefaultRoughnessValue() 
                    : roughnessValues[0];
            }
        }

        public ISewerConnection SewerConnection
        {
            get { return sewerConnection; }
            set
            {
                if (sewerConnection != null)
                {
                    sewerConnection.PropertyChanged -= OnPropertyChanged;
                }

                sewerConnection = value;
                
                if (sewerConnection != null)
                {
                    sewerConnection.PropertyChanged += OnPropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(PipeSlope));
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPipe.LevelTarget) ||
                e.PropertyName == nameof(IPipe.LevelSource) ||
                e.PropertyName == nameof(IPipe.Length))
            {
                OnPropertyChanged(nameof(PipeSlope));
            }
        }

        public double PipeSlope
        {
            get { return sewerConnection?.Slope() ?? 0; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}