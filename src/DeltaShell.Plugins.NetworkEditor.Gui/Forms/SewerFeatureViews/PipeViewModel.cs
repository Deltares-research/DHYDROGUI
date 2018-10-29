using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    //[Entity]
    public class PipeViewModel : INotifyPropertyChanged
    {
        private IPipe pipe;
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

                var roughnessFunctionType = PipeRoughnessSection.GetRoughnessFunctionType(Pipe);
                return roughnessFunctionType == RoughnessFunction.Constant 
                    ? PipeRoughnessSection.EvaluateRoughnessType(new NetworkLocation(pipe, 0)) 
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
                    PipeRoughnessSection.RoughnessNetworkCoverage.Locations, new NetworkLocation(pipe,0));

                var roughnessValues = PipeRoughnessSection.RoughnessNetworkCoverage.GetValues<double>(variableValueFilter);
                var roughnessFunctionType = PipeRoughnessSection.GetRoughnessFunctionType(Pipe);

                return roughnessValues.Count == 0 || roughnessFunctionType != RoughnessFunction.Constant 
                    ? PipeRoughnessSection.GetDefaultRoughnessValue() 
                    : roughnessValues[0];
            }
        }

        public IPipe Pipe
        {
            get { return pipe; }
            set
            {
                if (pipe != null)
                {
                    ((INotifyPropertyChanged)pipe).PropertyChanged -= OnPropertyChanged;
                }
                pipe = value;
                
                if (pipe != null)
                {
                    ((INotifyPropertyChanged)pipe).PropertyChanged += OnPropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(PipeSlope));
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TypeUtils.GetMemberName<Pipe>(p => p.LevelTarget) ||
                e.PropertyName == TypeUtils.GetMemberName<Pipe>(p => p.LevelSource) ||
                e.PropertyName == TypeUtils.GetMemberName<Pipe>(p => p.Length))
            {
                OnPropertyChanged(nameof(PipeSlope));
            }
        }

        public double PipeSlope
        {
            get { return pipe?.Slope() ?? 0; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}