using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public sealed class SewerConnectionViewModel : INotifyPropertyChanged, IDisposable
    {
        private ISewerConnection sewerConnection;
        private RoughnessSection pipeRoughnessSection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SewerConnectionViewModel"/> class.
        /// </summary>
        /// <param name="sewerConnection"> The sewer connection to visualize. </param>
        /// <param name="roughnessSection"> The roughness section in case of a pipe. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="sewerConnection"/> is <c>null</c> or
        /// when <paramref name="roughnessSection"/> is <c>null</c> in the case that the <paramref name="sewerConnection"/> is a <see cref="IPipe"/>.
        /// </exception>
        public SewerConnectionViewModel(ISewerConnection sewerConnection, RoughnessSection roughnessSection)
        {
            Ensure.NotNull(sewerConnection, nameof(sewerConnection));
            if (sewerConnection is IPipe)
            {
                Ensure.NotNull(roughnessSection, nameof(roughnessSection));
            }
            
            SewerConnection = sewerConnection;
            PipeRoughnessSection = roughnessSection;
        }
        public RoughnessSection PipeRoughnessSection
        {
            get { return pipeRoughnessSection; }
            private set
            {
                if (pipeRoughnessSection != null)
                {
                    ((INotifyPropertyChanged) pipeRoughnessSection).PropertyChanged -= OnDataPropertyChanged;
                }
                pipeRoughnessSection = value;

                if (pipeRoughnessSection != null)
                {
                    ((INotifyPropertyChanged)pipeRoughnessSection).PropertyChanged += OnDataPropertyChanged;
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
            private set
            {
                if (sewerConnection != null)
                {
                    sewerConnection.PropertyChanged -= OnDataPropertyChanged;
                }

                sewerConnection = value;
                
                if (sewerConnection != null)
                {
                    sewerConnection.PropertyChanged += OnDataPropertyChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(PipeSlope));
            }
        }

        private void OnDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPipe.LevelTarget) ||
                e.PropertyName == nameof(IPipe.LevelSource) ||
                e.PropertyName == nameof(IPipe.Length))
            {
                OnPropertyChanged(nameof(PipeSlope));
            }
            
            OnPropertyChanged(nameof(SourceNodeName));
            OnPropertyChanged(nameof(TargetNodeName));
            OnPropertyChanged(nameof(SourceCompartmentName));
            OnPropertyChanged(nameof(TargetCompartmentName));
        }

        public double PipeSlope
        {
            get { return sewerConnection?.Slope() ?? 0; }
        }

        /// <summary>
        /// Name of the source node.
        /// </summary>
        public string SourceNodeName => sewerConnection.Source.Name;
        
        /// <summary>
        /// Name of the target node.
        /// </summary>
        public string TargetNodeName => sewerConnection.Target.Name;

        /// <summary>
        /// Name of the source compartment.
        /// In the case that the source is a <see cref="IHydroNode"/> a static label is returned.
        /// </summary>
        public string SourceCompartmentName => sewerConnection.Source is IHydroNode
                                               ? Resources.Open_water_channel
                                               : sewerConnection.SourceCompartment.Name;

        /// <summary>
        /// Name of the target compartment.
        /// In the case that the target is a <see cref="IHydroNode"/> a static label is returned.
        /// </summary>
        public string TargetCompartmentName => sewerConnection.Target is IHydroNode
                                               ? Resources.Open_water_channel
                                               : sewerConnection.TargetCompartment.Name;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            sewerConnection.PropertyChanged -= OnDataPropertyChanged;
            if (pipeRoughnessSection != null)
            {
                ((INotifyPropertyChanged) pipeRoughnessSection).PropertyChanged -= OnDataPropertyChanged;
            }
        }
    }
}