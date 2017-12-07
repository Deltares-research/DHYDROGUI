using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Reflection;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    /// <summary>
    /// SourceAndSink represents a timeseries on a polyline.
    /// </summary>
    [Entity]
    public class SourceAndSink : NetTopologySuite.Extensions.Features.Generic.FeatureData<IFunction, Feature2D>
    {
        public const string TimeVariableName = "Time";
        public const string DischargeVariableName = "Discharge";
        public const string SalinityVariableName = "Salinity";
        public const string TemperatureVariableName = "Temperature";

        private const string DischargeVariableUnitDescription = "cubic meters per second";
        private const string DischargeVariableUnitSymbol = "m³/s";

        private const string SalinityVariableUnitDescription = "parts per trillion";
        private const string SalinityVariableUnitSymbol = "ppt";

        private const string TemperatureVariableUnitDescription = "degree celsius";
        private const string TemperatureVariableUnitSymbol = "°C";

        public SourceAndSink()
        {
            Function = CreateData();
        }

        public bool IsPointSource
        {
            get { return Feature.Geometry.Coordinates.Count() == 1; }
        }

        public double Area { get; set; }

        public bool MomentumSource
        {
            get { return Area > 0; }
        }

        public bool CanIncludeMomentum
        {
            get { return !IsPointSource; }
        }

        public IFunction Function
        {
            get { return Data; }
            private set { Data = value; }
        }

        public override Feature2D Feature
        {
            get
            {
                return base.Feature;
            }
            set
            {
                if (Feature != null)
                {
                    ((INotifyPropertyChange)Feature).PropertyChanged -= FeaturePropertyChanged;
                }
                base.Feature = value;
                if (Feature != null)
                {
                    ((INotifyPropertyChange) Feature).PropertyChanged += FeaturePropertyChanged;
                }
                AfterFeatureSet();
            }
        }

        private void FeaturePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TypeUtils.GetMemberName(() => Feature.Name))
            {
                Name = Feature.Name + " data";
                Function.Name = Name;
            }
        }

        [EditAction]
        private void AfterFeatureSet()
        {
            Name = Feature.Name + " data";
            Function.Name = Name;
        }

        private static IFunction CreateData()
        {
            var function = new Function();
            function.Arguments.Add(new Variable<DateTime>(TimeVariableName));
            function.Components.Add(new Variable<double>(DischargeVariableName)
            {
                Unit = new Unit(DischargeVariableUnitDescription, DischargeVariableUnitSymbol)
            });
            function.Components.Add(new Variable<double>(SalinityVariableName)
            {
                Unit = new Unit(SalinityVariableUnitDescription, SalinityVariableUnitSymbol)
            });
            function.Components.Add(new Variable<double>(TemperatureVariableName)
            {
                Unit = new Unit(TemperatureVariableUnitDescription, TemperatureVariableUnitSymbol)
            });
            return function;
        }
    }
}
