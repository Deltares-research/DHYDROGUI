using System;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public sealed class WaterFlowModel1DGridDataValueConverter : IValueConverter
    {
        private readonly WaterFlowModel1D waterFlowModel1D;
        private readonly QuantityType quantityType;
        private readonly ElementSet elementSet;
        private readonly DataItemRole role;

        public WaterFlowModel1DGridDataValueConverter(WaterFlowModel1D waterFlowModel1D,
            QuantityType quantityType, ElementSet elementSet, DataItemRole role)
        {
            this.waterFlowModel1D = waterFlowModel1D;
            this.quantityType = quantityType;
            this.elementSet = elementSet;
            this.role = role;
        }

        public object OriginalValue { get; set; }
        object IValueConverter.ConvertedValue
        {
            get { return ConvertedValue; }
            set { ConvertedValue = (double[]) value; }
        }

        public Type OriginalValueType
        {
            get { return typeof (double[]); }
        }

        public Type ConvertedValueType { get { return OriginalValueType;  } }

        public double[] ConvertedValue
        {
            get
            {
                throw new Exception("This needs to be reimplemented again in DIMR!");  

                return null;//waterFlowModel1D.GetArrayFromModelApi(elementSet, quantityType);
            }

            set
            {
                throw new Exception("This needs to be reimplemented again in DIMR!");

                if (role == DataItemRole.Output)
                {
                    throw new Exception("Output data items can not be set ");  // return; //model won't understand
                }
                //waterFlowModel1D.SetArrayToModelApi(elementSet, quantityType, value);
            }
        }

        public long Id { get; set; }

        public Type GetEntityType()
        {
            return GetType();
        }

        public object DeepClone()
        {
            throw new NotImplementedException();
        }
    }
}