using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.IO;
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
        public const string SecondaryFlowVariableName = "Secondary Flow";

        private IEventedList<string> sedimentFractionNames;
        private IEventedList<string> tracerNames;

        private const string DischargeVariableUnitDescription = "cubic meters per second";
        private const string DischargeVariableUnitSymbol = "m³/s";

        private const string SalinityVariableUnitDescription = "parts per trillion";
        private const string SalinityVariableUnitSymbol = "ppt";

        private const string TemperatureVariableUnitDescription = "degree celsius";
        private const string TemperatureVariableUnitSymbol = "°C";

        private const string SedimentFractionUnitDescription = "";
        private const string SedimentFractionUnitSymbol = "";

        private const string SecondaryFlowVariableUnitDescription = "meters per second";
        private const string SecondaryFlowVariableUnitSymbol = "m/s";

        private const string TracersUnitDescription = "kilograms per cubic meter";
        private const string TracerUnitSymbol = "kg/m³";

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
            if (e.PropertyName == TypeUtils.GetMemberName(() => Feature.Name))
            {
                Name = Feature.Name + " data";
                Function.Name = Name;
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
                sedimentFractionNames = value;
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
                tracerNames = value;
            }
        }

        public void SedimentFractionNamesCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var sedimentFractionName = e.Item as string;

            if (sedimentFractionName == null)
                return;

            switch (e.Action)
            {
                //Column Order: Time | Discharge | Salinity | Temperature | SedimentFractions | Secondary Flow | Tracers
                case NotifyCollectionChangeAction.Add:
                    var secondaryFlow =
                        Function.Components.FirstOrDefault(c => c.Name.Equals(SecondaryFlowVariableName));
                    var firstTracer = tracerNames.Any()
                        ? Function.Components.FirstOrDefault(c => c.Name.Equals(tracerNames.First()))
                        : null;

                    if (secondaryFlow != null)
                    {
                        Function.Components.Insert(Function.Components.IndexOf(secondaryFlow),
                            new Variable<double>(sedimentFractionName)
                            {
                                Unit = new Unit(SedimentFractionUnitDescription, SedimentFractionUnitSymbol)
                            });
                    }
                    else if (firstTracer != null)
                    {
                        Function.Components.Insert(Function.Components.IndexOf(firstTracer),
                            new Variable<double>(sedimentFractionName)
                            {
                                Unit = new Unit(SedimentFractionUnitDescription, SedimentFractionUnitSymbol)
                            });
                    }
                    else
                    {
                        Function.Components.Add(
                            new Variable<double>(sedimentFractionName)
                            {
                                Unit = new Unit(SedimentFractionUnitDescription, SedimentFractionUnitSymbol)
                            });
                    }
                    break;
                case NotifyCollectionChangeAction.Remove:
                    Function.RemoveComponentByName(sedimentFractionName);
                    break;
                case NotifyCollectionChangeAction.Replace:
                    throw new NotImplementedException("Renaming of sediment fraction is not yet supported");
                    break;
                case NotifyCollectionChangeAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void TracerNamesCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var tracerName = e.Item as string;
            if (tracerName == null) return;
            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    Function.Components.Add(new Variable<double>(tracerName)
                    {
                        Unit = new Unit(TracersUnitDescription, TracerUnitSymbol)
                    });
                    break;
                case NotifyCollectionChangeAction.Remove:
                    Function.RemoveComponentByName(tracerName);
                    break;
                case NotifyCollectionChangeAction.Replace:
                    throw new NotImplementedException("Renaming of tracer name is not yet supported");
                    break;
                case NotifyCollectionChangeAction.Reset:
                    break;
                default: throw new ArgumentOutOfRangeException();
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
            function.Components.Add(new Variable<double>(SecondaryFlowVariableName)
            {
                Unit = new Unit(SecondaryFlowVariableUnitDescription, SecondaryFlowVariableUnitSymbol)
            });
            return function;
        }
    }
}
