using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils
{
    [Entity(FireOnCollectionChange = false)]
    public class ControlledTestModel : TimeDependentModelBase
    {
        private readonly EventedList<IFeature> inputFeatures = new EventedList<IFeature>();
        private readonly EventedList<IFeature> outputFeatures = new EventedList<IFeature>();
        private IList<ExplicitValueConverterLookupItem> explicitValueConverterLookup;

        public ControlledTestModel()
        {
            InputFeatures.CollectionChanged += InputFeaturesCollectionChanged;
            OutputFeatures.CollectionChanged += OutputFeaturesCollectionChanged;

            InputFeatures.AddRange(new[]
            {
                new RtcTestFeature {Name = "input feature 1"},
                new RtcTestFeature {Name = "input feature 2"},
                new RtcTestFeature {Name = "input feature 3"}
            });

            OutputFeatures.AddRange(new[]
            {
                new RtcTestFeature {Name = "output feature 1"},
                new RtcTestFeature {Name = "output feature 2"},
                new RtcTestFeature {Name = "output feature 3"}
            });
        }

        public EventedList<IFeature> InputFeatures
        {
            get
            {
                return inputFeatures;
            }
        }

        public EventedList<IFeature> OutputFeatures
        {
            get
            {
                return outputFeatures;
            }
        }

        public override IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
        {
            switch (role)
            {
                case DataItemRole.Input:
                {
                    foreach (IFeature inputFeature in inputFeatures)
                    {
                        yield return inputFeature;
                    }

                    break;
                }
                case DataItemRole.Output:
                {
                    foreach (IFeature outputFeature in outputFeatures)
                    {
                        yield return outputFeature;
                    }

                    break;
                }
            }
        }

        public override IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            return DataItems.Where(di =>
            {
                var converter = di.ValueConverter as ControlledTestModelParameterValueConverter;

                return converter != null && Equals(converter.Location, location);
            });
        }

        protected override void OnInitialize()
        {
            explicitValueConverterLookup = ExplicitValueConverterAwareModelHelper.CreateExplicitValueConverterLookupItems(AllDataItems);

            ExplicitValueConverterAwareModelHelper.UpdateExplicitValueConverters(explicitValueConverterLookup, StartTime);
        }

        protected override void OnExecute()
        {
            CurrentTime += TimeStep;

            ExplicitValueConverterAwareModelHelper.UpdateExplicitValueConverters(explicitValueConverterLookup, CurrentTime);

            if (CurrentTime >= StopTime)
            {
                Status = ActivityStatus.Done;
            }
        }

        protected override void OnCleanup()
        {
            explicitValueConverterLookup.Clear();

            base.OnCleanup();
        }

        private void OutputFeaturesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BubbleCollectionChangedEvent(sender, e);
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddDataItem(DataItemRole.Output, (IFeature) removedOrAddedItem);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveDataItem((IFeature) removedOrAddedItem);
                    break;
            }
        }

        private void InputFeaturesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BubbleCollectionChangedEvent(sender, e);
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddDataItem(DataItemRole.Input, (IFeature) removedOrAddedItem);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveDataItem((IFeature) removedOrAddedItem);
                    break;
            }
        }

        private void RemoveDataItem(IFeature feature)
        {
            DataItems.Remove(GetDataItemByValue(feature));
        }

        private void AddDataItem(DataItemRole role, IFeature feature)
        {
            DataItems.Add(new DataItem
            {
                Role = role,
                ValueConverter = new ControlledTestModelParameterValueConverter(feature, "Value"),
                ValueType = typeof(double)
            });
        }
    }
}