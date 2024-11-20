using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using log4net;

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

        private static readonly Dictionary<FmMeteoQuantity, string> FmMeteoQuantityKernelNames =
            new Dictionary<FmMeteoQuantity, string>()
            {
                {FmMeteoQuantity.Precipitation, "rainfall_rate"}
            };

        public static readonly Dictionary<FmMeteoLocationType, string> FmMeteoLocationKernelNames =
            new Dictionary<FmMeteoLocationType, string>()
            {
                {FmMeteoLocationType.Global, "global"},
                {FmMeteoLocationType.Feature, "feature"},
                {FmMeteoLocationType.Polygon, "polygon"},
                {FmMeteoLocationType.Grid, "grid"}
            };

        private bool PopulateBcBlockData(IFmMeteoField fmMeteoField, DateTime? referenceTime, BcBlockData bcBlockData)
        {

            if (ForcingTypeDefinitions == null) return true;
            var forcingTypeDefinition = ForcingTypeDefinitions["timeseries"];
            string location;
            if (!FmMeteoLocationKernelNames.TryGetValue(fmMeteoField.FmMeteoLocationType, out location))
            {
                location = FmMeteoLocationKernelNames[FmMeteoLocationType.Global];
            }
            bcBlockData.SupportPoint = location;

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
                bcBlockData.Quantities.Add(CreateBcQuantityDataForArgument(quantity, argument, referenceTime, TimeSpan.Zero));
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

        public bool InsertBoundaryData(IFmMeteoField fmMeteoField, List<BcBlockData> dataBlocks)
        {
            foreach (var bcBlockData in dataBlocks)
            {
                if (!InsertBoundaryData(fmMeteoField, bcBlockData))
                {
                    Log.WarnFormat("Can not add data from boundary block {0} to fmmeteodata {1}",
                        bcBlockData.SupportPoint, fmMeteoField.Name);
                    return false;
                }
            }
            return true;
        }

        private bool InsertBoundaryData(IFmMeteoField fmMeteoField, BcBlockData data)
        {
            if (data == null) return false;
            using (CultureUtils.SwitchToInvariantCulture())
            {
                // parse and validate forcingType
                ForcingTypeDefinition forcingTypeDefinition;
                if (!ForcingTypeDefinitions.TryGetValue(data.FunctionType, out forcingTypeDefinition))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at line {1}: function type {2} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.FunctionType);
                    return false;
                }
                InterpolationType timeInterpolationType;
                if (!TryParseTimeInterpolationType(data, out timeInterpolationType))
                {
                    Log.WarnFormat(
                        "File {0}, block starting at line {1}: time interpolation type {2} could not be parsed; omitting data block.",
                        data.FilePath, data.LineNumber, data.TimeInterpolationType);
                    return false;
                }
                // parse the Quantities in bc / bcm file
                var componentKeys = forcingTypeDefinition.ComponentDefinitions;
                var argVariables = new Dictionary<int, BcQuantityData>();
                var compVariables = new Dictionary<FmMeteoQuantity, BcQuantityData>();

                foreach (var quantity in data.Quantities)
                {
                    var quantityString = quantity.Quantity;

                    string fmMeteoQuantity = null;

                    // if it's an argument quantity, add it to the argVariables and continue
                    if (forcingTypeDefinition.ArgumentDefinitions.Contains(quantityString))
                    {
                        argVariables.Add(
                            forcingTypeDefinition.ArgumentDefinitions.ToList().IndexOf(quantityString),
                            quantity);

                        continue;
                    }

                    foreach (var componentKey in componentKeys)
                    {
                        if (String.IsNullOrEmpty(componentKey))
                        {
                            fmMeteoQuantity = quantityString.ToLower();
                            break;
                        }
                        if (quantityString.EndsWith(componentKey))
                        {
                            fmMeteoQuantity =
                                quantityString.ToLower().Substring(0, quantityString.Length - componentKey.Length).TrimEnd();
                            break;
                        }
                    }

                    // if we haven't match the quantity, give a warning and continue
                    if (fmMeteoQuantity == null)
                    {
                        Log.WarnFormat(
                            "File {0}, block starting at line {1}: quantity {2} could not be parsed; omitting data column.",
                            data.FilePath, data.LineNumber, quantity.Quantity);
                        continue;
                    }
                    
                    // add component variable
                    if (FmMeteoQuantityKernelNames.ContainsValue(fmMeteoQuantity))
                    {
                        var meteoQuantity = FmMeteoQuantityKernelNames.FirstOrDefault(pair => pair.Value == fmMeteoQuantity);
                        compVariables.Add(meteoQuantity.Key, quantity);
                    }
                    foreach (var arg in argVariables)
                    {
                        var variable = fmMeteoField.Data.Arguments[arg.Key];
                        variable.Values.Clear();
                        variable.SetValues(ParseValues(arg.Value, variable.ValueType, data.SupportPoint));
                        if (variable is IVariable<DateTime>)
                        {
                            variable.InterpolationType = timeInterpolationType;
                        }
                    }

                    foreach (var comp in compVariables)
                    {
                        var variable = fmMeteoField.Data.Components[0];
                        variable.SetValues(ParseValues(comp.Value, variable.ValueType, data.SupportPoint));
                    }
                }
            }
            return true;
        }
    }
}