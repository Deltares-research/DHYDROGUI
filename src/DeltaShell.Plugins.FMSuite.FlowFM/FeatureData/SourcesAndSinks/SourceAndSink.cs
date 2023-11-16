using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Features.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks
{
    /// <summary>
    /// SourceAndSink represents a timeseries on a polyline.
    /// </summary>
    [Entity]
    public sealed class SourceAndSink : FeatureData<SourceAndSinkFunction, Feature2D>
    {
        public SourceAndSink() => Data = new SourceAndSinkFunction();

        public override Feature2D Feature
        {
            get => base.Feature;
            set
            {
                if (Feature != null)
                {
                    ((INotifyPropertyChange) Feature).PropertyChanged -= FeaturePropertyChanged;
                }

                base.Feature = value;
                if (Feature != null)
                {
                    ((INotifyPropertyChange) Feature).PropertyChanged += FeaturePropertyChanged;
                }

                AfterFeatureSet();
            }
        }

        /// <summary>
        /// Gets all the tracer names.
        /// </summary>
        public IEnumerable<string> TracerNames => GetNames<TracerVariable>();

        /// <summary>
        /// Gets all the sediment fraction names.
        /// </summary>
        public IEnumerable<string> SedimentFractionNames => GetNames<SedimentFractionVariable>();

        public bool IsPointSource => Feature.Geometry.Coordinates.Count() == 1;

        public double Area { get; set; }

        public bool MomentumSource => Area > 0;

        public bool CanIncludeMomentum => !IsPointSource;

        /// <summary>
        /// The function data on this <see cref="SourceAndSink"/>.
        /// </summary>
        public SourceAndSinkFunction Function => Data;

        private void FeaturePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Feature.Name))
            {
                Name = Feature.Name + " data";
                Function.Name = Name;
            }
        }

        private IEnumerable<string> GetNames<T>() where T : IVariable
        {
            return Function.Components.OfType<T>().Select(v => v.Name);
        }

        private void AfterFeatureSet()
        {
            Name = Feature.Name + " data";
            Function.Name = Name;
        }
    }
}