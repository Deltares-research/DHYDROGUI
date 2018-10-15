using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using log4net;
using NetTopologySuite.Extensions.Features;


namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class BcMeteoFileDataBuilder : BcFileFlowBoundaryDataBuilder
    {
        public BcBlockData CreateBlockData(IFmMeteoField fmMeteoField, DateTime? refDate)
        {
            var block = new BcBlockData();
            PopulateBcBlockData(fmMeteoField, refDate, block);
            return block;
        }
        private static readonly Dictionary<FmMeteoQuantity, string> FmMeteoQuantityKernelNames = new Dictionary<FmMeteoQuantity, string>()
        {
            { FmMeteoQuantity.Precipitation, "rainfall_rate" }
        };

        private static readonly Dictionary<FmMeteoLocationType, string> FmMeteoLocationKernelNames = new Dictionary<FmMeteoLocationType, string>()
        {
            { FmMeteoLocationType.Global, "global" }
        };

        private bool PopulateBcBlockData(IFmMeteoField fmMeteoField, DateTime? referenceTime, BcBlockData bcBlockData)
        {

            if (ForcingTypeDefinitions == null) return true;
            var forcingTypeDefinition = ForcingTypeDefinitions["timeseries"];
            if(!(fmMeteoField.FeatureData?.Feature is Feature2D))
                bcBlockData.SupportPoint = "global";
            
            var functionType = ForcingTypeDefinitions.First(kvp => kvp.Value == forcingTypeDefinition).Key;

            bcBlockData.FunctionType = functionType;
            
            var data = fmMeteoField.Data;

            var timeArgument = data.Arguments.FirstOrDefault() as IVariable<DateTime>;

            bcBlockData.TimeInterpolationType = timeArgument == null
                ? null
                : TimeInterpolationString(timeArgument.InterpolationType);
            
            var i = 0;
            foreach (var argument in data.Arguments)
            {
                var quantity = forcingTypeDefinition.ArgumentDefinitions[i++];
                bcBlockData.Quantities.Add(CreateBcQuantityDataForArgument(quantity, argument, referenceTime));
            }

            string quantityName;
            if (!FmMeteoQuantityKernelNames.TryGetValue(fmMeteoField.Quantity, out quantityName))
                quantityName = "unknown";

            foreach (var component in data.Components)
            {
                bcBlockData.Quantities.Add(new BcQuantityData
                {
                    Quantity = quantityName,
                    Unit = component.Unit.Symbol,
                    Values = PrintValues(component, null, null).ToList()
                });
            }
            return true;
        }

        
        private static readonly ILog Log = LogManager.GetLogger(typeof(BcMeteoFileDataBuilder));
        
    }
}