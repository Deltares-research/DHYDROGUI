using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Utils.Aop;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// HACK: this class is very error-prone (hard casts to WFM1D, engine-specific enums), maybe we better change real structure parameter and in this way propagate it to API, 
    /// or define even in standard converter and handle it in WaterFlowModel1D
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class WaterFlowModelBranchFeatureValueConverter : ParameterValueConverter, IExplicitValueConverter
    {
        private QuantityType quantityType;
        private ElementSet elementSet;
        
        [Aggregation]
        public IModel Model { get; set; }

        public QuantityType QuantityType 
        { 
            get { return quantityType; }
            private set { quantityType = value; }
        }

        public ElementSet ElementSet
        {
            get { return elementSet; }
            private set { elementSet = value; }
        }

        public WaterFlowModelBranchFeatureValueConverter()
        {
            Location = new NetworkLocation();
        }

        public WaterFlowModelBranchFeatureValueConverter(IModel model, IFeature feature, string parameterName, 
            QuantityType quantityType, ElementSet elementSet, DataItemRole role, string unit)
        {
            Location = feature;
            Model = model;
            ParameterName = parameterName;
            QuantityType = quantityType;
            ElementSet = elementSet;
            Role = role;
            Unit = unit;
        }

        public override object DeepClone()
        {
            var clone = (WaterFlowModelBranchFeatureValueConverter)base.DeepClone();
            
            clone.ElementSet = ElementSet;
            clone.QuantityType = QuantityType;
            clone.Role = Role;
            clone.Unit = Unit;
            clone.ParameterName = ParameterName;
            clone.Model = Model;
            return clone;
        }

        public virtual DataItemRole Role { get; private set; }

        public int LocationModelId = -1; // cached model id (for performance reasons)

        [NoNotifyPropertyChange]
        public override double ConvertedValue
        {
            get
            {
                var model = Model as IDimrModel;

                if (model == null)
                {
                    return default(double);
                }
                
                var array = model.GetVar(Location.GetFeatureCategory(), Location.ToString(), ParameterName);
                var doubleArray = array as double[];
                return doubleArray != null ? doubleArray[0] : default(double);
            }
            set
            {
                var model = Model as WaterFlowModel1D;

                if (model == null)
                {
                    return;
                }

                if (Role == DataItemRole.Output)
                {
                    return; //model won't understand
                }
                
                model.SetVar(new[] {value}, Location.GetFeatureCategory(), Location.ToString(), ParameterName);
            }
        }

        public void Update(DateTime time, object value = null)
        {
            ConvertedValue = Convert.ToDouble(value);
        }

        private IDataItem cachedDataItem;

        public override string Name { get; set; }
    }
}