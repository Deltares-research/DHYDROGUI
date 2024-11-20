using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public class SourceSinkVariableInfo
    {
        public const string DischargeUnitDescription = "cubic meters per second";
        public const string DischargeUnitSymbol = "m³/s";

        public const string SalinityUnitDescription = "parts per trillion";
        public const string SalinityUnitSymbol = "ppt";

        public const string TemperatureUnitDescription = "degree celsius";
        public const string TemperatureUnitSymbol = "°C";

        public const string SedimentFractionUnitDescription = "";
        public const string SedimentFractionUnitSymbol = "";

        public const string SecondaryFlowUnitDescription = "meters per second";
        public const string SecondaryFlowUnitSymbol = "m/s";

        public const string TracersUnitDescription = "kilograms per cubic meter";
        public const string TracerUnitSymbol = "kg/m³";
    }
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
        public const string SecondaryFlowVariableName = "Secondary Flow";

        private IEventedList<string> sedimentFractionNames;
        private IEventedList<string> tracerNames;

        public SourceAndSink()
        {
            Function = CreateData();
            SedimentFractionNames = new EventedList<string>();
            TracerNames = new EventedList<string>();
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
            if (e.PropertyName == nameof(Feature.Name))
            {
                Name = Feature.Name + " data";
                if (Function != null) Function.Name = Name;
            }
        }

        public IEventedList<string> SedimentFractionNames
        {
            get{ return sedimentFractionNames; }
            set
            {
                if (sedimentFractionNames != null)
                {
                    SedimentFractionNames.CollectionChanged -= SedimentFractionNamesCollectionChanged;
                }
                sedimentFractionNames = value;
                if (sedimentFractionNames != null)
                {
                    SedimentFractionNames.CollectionChanged += SedimentFractionNamesCollectionChanged;
                }
            }
        }

        public IEventedList<string> TracerNames
        {
            get { return tracerNames; }
            set
            {
                if (tracerNames != null)
                {
                    TracerNames.CollectionChanged -= TracerNamesCollectionChanged;
                }
                tracerNames = value;
                if (tracerNames != null)
                {
                    TracerNames.CollectionChanged += TracerNamesCollectionChanged;
                }
            }
        }

        public void SedimentFractionNamesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var sedimentFractionName = e.GetRemovedOrAddedItem()as string;

            if (sedimentFractionName == null)
                return;
            
            switch (e.Action)
            {
                //Column Order: Time | Discharge | Salinity | Temperature | SedimentFractions | Secondary Flow | Tracers
                case NotifyCollectionChangedAction.Add:
                    AddSedimentFractionToComponents(sedimentFractionName);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Function.RemoveComponentByName(sedimentFractionName);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("Renaming of sediment fraction is not yet supported");
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddSedimentFractionToComponents(string name)
        {
            var secondaryFlow =
                Function.Components.FirstOrDefault(c => c.Name.Equals(SecondaryFlowVariableName));
            var firstTracer = tracerNames.Any()
                ? Function.Components.FirstOrDefault(c => c.Name.Equals(tracerNames.First()))
                : null;

            if (secondaryFlow != null)
            {
                Function.Components.Insert(Function.Components.IndexOf(secondaryFlow),
                    CreateSedimentFractionVariable(name));
            }
            else if (firstTracer != null)
            {
                Function.Components.Insert(Function.Components.IndexOf(firstTracer),
                    CreateSedimentFractionVariable(name));
            }
            else
            {
                Function.Components.Add(CreateSedimentFractionVariable(name));
            }
        }

        private IVariable CreateSedimentFractionVariable(string name)
        {
            return new Variable<double>(name)
            {
                Unit = new Unit(SourceSinkVariableInfo.SedimentFractionUnitDescription,
                    SourceSinkVariableInfo.SedimentFractionUnitSymbol)
            };
        }

        public void TracerNamesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var tracerName = e.GetRemovedOrAddedItem() as string;
            if (tracerName == null) return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Function.Components.Add(new Variable<double>(tracerName)
                    {
                        Unit = new Unit(SourceSinkVariableInfo.TracersUnitDescription, SourceSinkVariableInfo.TracerUnitSymbol)
                    });
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Function.RemoveComponentByName(tracerName);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var previousTracerName = e.OldItems[0] as string;
                    if (previousTracerName == null) break;

                    var matchingComponent = Function.Components.FirstOrDefault(c => c.Name == previousTracerName);
                    if (matchingComponent == null) break;

                    matchingComponent.Name = tracerName;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

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
                Unit = new Unit(SourceSinkVariableInfo.DischargeUnitDescription, SourceSinkVariableInfo.DischargeUnitSymbol)
            });
            function.Components.Add(new Variable<double>(SalinityVariableName)
            {
                Unit = new Unit(SourceSinkVariableInfo.SalinityUnitDescription, SourceSinkVariableInfo.SalinityUnitSymbol)
            });
            function.Components.Add(new Variable<double>(TemperatureVariableName)
            {
                Unit = new Unit(SourceSinkVariableInfo.TemperatureUnitDescription, SourceSinkVariableInfo.TemperatureUnitSymbol)
            });
            function.Components.Add(new Variable<double>(SecondaryFlowVariableName)
            {
                Unit = new Unit(SourceSinkVariableInfo.SecondaryFlowUnitDescription, SourceSinkVariableInfo.SecondaryFlowUnitSymbol)
            });
            return function;
        }
    }
}
