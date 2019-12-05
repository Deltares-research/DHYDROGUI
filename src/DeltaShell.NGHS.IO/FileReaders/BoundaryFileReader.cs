using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.NGHS.IO.FileReaders
{
    // TODO: currently this class cannot handle Salt entries in the BcFile

    // Note: In this class we do not create new BoundaryConditions, these are created when adding a node to the network
    //       Instead we retrieve the BoundaryCondition (based on feature name) and update the properties
    //       The same is true of LateralSources
    public static class BoundaryFileReader
    {
        public static void ReadFile(string filename, IEnumerable<Model1DBoundaryNodeData> boundaryConditions, IFunction wind = null)
        {
            if (!File.Exists(filename)) throw new FileReadingException(String.Format("Could not read file {0} properly, it doesn't exist.", filename));
            var categories = new DelftBcReader().ReadDelftBcFile(filename);
            if (categories.Count == 0) throw new FileReadingException(String.Format("Could not read file {0} properly, it seems empty", filename));

            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();

            foreach (var boundaryCategory in categories.Where(category => category.Name == BoundaryRegion.BcBoundaryHeader || category.Name == BoundaryRegion.BcForcingHeader ))
            {
                try
                {
                    var name = boundaryCategory.ReadProperty<string>(BoundaryRegion.Name.Key);
                    if (name == FunctionAttributes.StandardFeatureNames.ModelWide)
                    {
                        ReadModelWideBoundaryCondition(wind, boundaryCategory);
                        continue;
                    }

                    var model1DBoundaryNodeDatas = boundaryConditions as Model1DBoundaryNodeData[] ?? boundaryConditions.ToArray();
                    var waterFlowModel1DBoundaryNodeData = model1DBoundaryNodeDatas.FirstOrDefault(bc => bc.Feature.Name == name);
                    if (waterFlowModel1DBoundaryNodeData == null)
                        continue; //throw new BoundaryConditionReadingException(string.Format("Node ({0}) where the boundary condition should be put on is not available in the model",name));

                    ReadBoundaryCondition(waterFlowModel1DBoundaryNodeData, boundaryCategory);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read boundary condition", fileReadingException));
                }
            }
            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading boundary conditions an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }
        }
        public static void ReadFile(string filename, IEnumerable<Model1DLateralSourceData> lateralSourcesData)
        {
            if (!File.Exists(filename)) throw new FileReadingException(String.Format("Could not read file {0} properly, it doesn't exist.", filename));
            var categories = new DelftBcReader().ReadDelftBcFile(filename);
            if (categories.Count == 0) throw new FileReadingException(String.Format("Could not read file {0} properly, it seems empty", filename));

            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            var model1DLateralSourceDatas = lateralSourcesData as Model1DLateralSourceData[] ?? lateralSourcesData.ToArray();
            foreach (var lateralCategory in categories.Where(category => category.Name == BoundaryRegion.BcLateralHeader || category.Name == BoundaryRegion.BcForcingHeader))
            {
                try
                {
                    var name = lateralCategory.ReadProperty<string>(BoundaryRegion.Name.Key);
                    var waterFlowModel1DLateralSourceData = model1DLateralSourceDatas.FirstOrDefault<Model1DLateralSourceData>(ls => ls.Feature.Name == name);
                    if (waterFlowModel1DLateralSourceData == null)
                        continue;//throw new LateralDischargeReadingException(string.Format("Node ({0}) where the lateral discharge should be put on is not available in the model", name));

                    ReadLateralSource(waterFlowModel1DLateralSourceData, lateralCategory);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read lateral discharge", fileReadingException));
                }
            }

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading lateral discharges an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }
        }

        private static void ReadModelWideBoundaryCondition(IFunction Wind, IDelftBcCategory boundaryCategory)
        {
            var functionType = boundaryCategory.ReadProperty<string>(BoundaryRegion.Function.Key);
            switch (functionType)
            {
                case BoundaryRegion.FunctionStrings.Constant:
                    break;
                case BoundaryRegion.FunctionStrings.QhTable:
                    break;
                case BoundaryRegion.FunctionStrings.TimeSeries:
                    
                    
                    var modelWideWindBoundaryConditionQuantities = boundaryCategory.Table.Select(t => t.Quantity).Select(q=>q.Value);
                    foreach (var modelWideWindBoundaryConditionQuantity in modelWideWindBoundaryConditionQuantities)
                    {
                        switch (modelWideWindBoundaryConditionQuantity)
                        {
                            case BoundaryRegion.QuantityStrings.WindSpeed:
                            {
                                /*
                                InitializeFunctionArguments(boundaryCategory, Wind);
                                var functionValues = ConvertStringsToDoubles(boundaryCategory.Table[1]);
                                Wind.Velocity.SetValues(functionValues);
                                */
                                break;
                                
                            }

                            case BoundaryRegion.QuantityStrings.WindDirection:
                            {
                                /*
                                InitializeFunctionArguments(boundaryCategory, Wind);
                                var functionValues = ConvertStringsToDoubles(boundaryCategory.Table[1]);
                                Wind.Direction.SetValues(functionValues);
                                */
                                break;
                            }
                        }
                    }

                    break;
                default:
                    var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", boundaryCategory.Name,
                        BoundaryRegion.Function.Key, Environment.NewLine);
                    throw new BoundaryConditionReadingException(errorMessage);
            }
        }

        private static void InitializeFunctionArguments(IDelftBcCategory boundaryCategory, IFunction function)
        {
            var argumentValues = GetDateTimesValues(boundaryCategory.Table[0]);
            var dateTimes = argumentValues as IList<DateTime> ?? argumentValues.ToList();
            for (var index = 0; index < dateTimes.Count() && function.Arguments[0].Values.Count != 0; index++)
            {
                if (function.Arguments[0].Values[index].Equals(dateTimes[index])) continue;
                function.Clear();
                break;
            }
            if (function.Arguments[0].Values.Count == 0)
                function.Arguments[0].SetValues(dateTimes);
        }

        private static void ReadBoundaryCondition(Model1DBoundaryNodeData boundaryCondition, IDelftBcCategory boundaryCategory)
        {
            // TODO: the following 2 lines should be removed when we implement salt in the reader, currently we temporarily ignore Salt Boundaries
            var saltBoundaryQuantity = boundaryCategory.Table.Where(bcq => bcq.Quantity.Value == BoundaryRegion.QuantityStrings.WaterSalinity);
            if (saltBoundaryQuantity.Any()) return;

            var function = boundaryCategory.ReadProperty<string>(BoundaryRegion.Function.Key);
            switch (function)
            {
                case BoundaryRegion.FunctionStrings.Constant:
                    switch (boundaryCategory.Table[0].Quantity.Value)
                    {
                        case BoundaryRegion.QuantityStrings.WaterDischarge:
                            boundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowConstant;
                            boundaryCondition.Flow = ReadConstantValue(boundaryCategory.Table[0], boundaryCategory.Name);
                            break;
                        case BoundaryRegion.QuantityStrings.WaterLevel:
                            boundaryCondition.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
                            boundaryCondition.WaterLevel = ReadConstantValue(boundaryCategory.Table[0], boundaryCategory.Name);
                            break;
                    }
                    break;
                case BoundaryRegion.FunctionStrings.QhTable:
                    boundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowWaterLevelTable;
                    SetCategoryValuesToFeatureData(boundaryCondition, boundaryCategory, ConvertStringsToDoubles, ConvertStringsToDoubles);
                    break;
                case BoundaryRegion.FunctionStrings.TimeSeries:
                    switch (boundaryCategory.Table[1].Quantity.Value)
                    {
                        case BoundaryRegion.QuantityStrings.WaterDischarge:
                            boundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;
                            break;
                        case BoundaryRegion.QuantityStrings.WaterLevel:
                            boundaryCondition.DataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries;
                            break;
                    }
                    SetCategoryValuesToFeatureData(boundaryCondition, boundaryCategory, GetDateTimesValues, ConvertStringsToDoubles);
                    break;
                default:
                    var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", boundaryCategory.Name, BoundaryRegion.Function.Key, Environment.NewLine);
                    throw new BoundaryConditionReadingException(errorMessage);
            }
        }

        private static void ReadLateralSource(Model1DLateralSourceData lateralSource, IDelftBcCategory lateralSourceCategory)
        {
            // TODO: the following 2 lines should be removed when we implement salt in the reader, currently we temporarily ignore Salt Boundaries
            var saltBoundaryQuantity = lateralSourceCategory.Table.Where(bcq => bcq.Quantity.Value == BoundaryRegion.QuantityStrings.WaterSalinity);
            if (saltBoundaryQuantity.Any()) return;

            var function = lateralSourceCategory.ReadProperty<string>(BoundaryRegion.Function.Key);
            switch (function)
            {
                case BoundaryRegion.FunctionStrings.Constant:
                    lateralSource.DataType = Model1DLateralDataType.FlowConstant;
                    lateralSource.Flow = ReadConstantValue(lateralSourceCategory.Table[0], lateralSourceCategory.Name);
                    break;
                case BoundaryRegion.FunctionStrings.QhTable:
                    lateralSource.DataType = Model1DLateralDataType.FlowWaterLevelTable;
                    SetCategoryValuesToFeatureData(lateralSource, lateralSourceCategory, ConvertStringsToDoubles, ConvertStringsToDoubles);
                    break;
                case BoundaryRegion.FunctionStrings.TimeSeries:
                    lateralSource.DataType = Model1DLateralDataType.FlowTimeSeries;
                    SetCategoryValuesToFeatureData(lateralSource, lateralSourceCategory, GetDateTimesValues, ConvertStringsToDoubles);
                    break;
                default:
                    var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", lateralSourceCategory.Name, BoundaryRegion.Function.Key, Environment.NewLine);
                    throw new LateralDischargeReadingException(errorMessage);
            }
            
        }
       
        private static void SetCategoryValuesToFeatureData<Targ>(IFeatureData featureData, IDelftBcCategory category, Func<IDelftBcQuantityData, IEnumerable<Targ>> parseArgumentValues, Func<IDelftBcQuantityData, IEnumerable<double>> parseFunctionValues)
        {
            var argumentValues = parseArgumentValues(category.Table[0]);
            if (argumentValues == null)
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", category.Name,
                    category.Table[0].Quantity, Environment.NewLine);
                throw new BoundaryConditionReadingException(errorMessage);
            }
            var functionValues = parseFunctionValues(category.Table[1]);
            if (functionValues == null)
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", category.Name,
                    category.Table[1].Quantity, Environment.NewLine);
                throw new BoundaryConditionReadingException(errorMessage);
            }
            
            var function = ((IFunction)featureData.Data);
            function.Clear();
            function.Arguments[0].SetValues(argumentValues);
            function.SetValues(functionValues);
            function.Arguments[0].ExtrapolationType = ExtrapolationType.Linear;
            var periodic = category.ReadProperty<string>(BoundaryRegion.Periodic.Key, true);
            if (!string.IsNullOrEmpty(periodic) && periodic == "true")
                function.Arguments[0].ExtrapolationType = ExtrapolationType.Periodic;


        }

        private static double ReadConstantValue(IDelftBcQuantityData quantityData, string categoryName)
        {
            double constantValue;
            if (!double.TryParse(quantityData.Values[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out constantValue))
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", categoryName, quantityData.Quantity, Environment.NewLine);
                throw new LateralDischargeReadingException(errorMessage);
            }

            return constantValue;
        }

        private static IEnumerable<DateTime> GetDateTimesValues(IDelftBcQuantityData quantityData)
        {
            var dateTimeData = ConvertStringsToDoubles(quantityData);
            if (dateTimeData == null) return null;

            var unitString = quantityData.Unit.Value;
            if (unitString.Contains(BoundaryRegion.UnitStrings.TimeMinutes))
            {
                var referenceDateTime = DateTime.ParseExact(
                    quantityData.Unit.Value.Replace(BoundaryRegion.UnitStrings.TimeMinutes, "").Trim(),
                    BoundaryRegion.UnitStrings.TimeFormat, 
                    CultureInfo.InvariantCulture);
                return dateTimeData.Select(referenceDateTime.AddMinutes);
            }
            if (unitString.Contains(BoundaryRegion.UnitStrings.TimeSeconds))
            {
                var referenceDateTime = DateTime.ParseExact(
                    quantityData.Unit.Value.Replace(BoundaryRegion.UnitStrings.TimeSeconds, "").Trim(),
                    BoundaryRegion.UnitStrings.TimeFormat,
                    CultureInfo.InvariantCulture);
                return dateTimeData.Select(referenceDateTime.AddSeconds);
            }
            if (unitString.Contains(BoundaryRegion.UnitStrings.TimeHours))
            {
                var referenceDateTime = DateTime.ParseExact(
                    quantityData.Unit.Value.Replace(BoundaryRegion.UnitStrings.TimeHours, "").Trim(),
                    BoundaryRegion.UnitStrings.TimeFormat,
                    CultureInfo.InvariantCulture);
                return dateTimeData.Select(referenceDateTime.AddHours);
            }
            return null;
        }
       
        private static IEnumerable<double> ConvertStringsToDoubles(IDelftBcQuantityData delftBcQuantityData)
        {
            var doubleCollection = new List<double>();
            foreach (var stringValue in delftBcQuantityData.Values)
            {
                double doubleValue;
                if (!double.TryParse(stringValue, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out doubleValue)) return null;
                doubleCollection.Add(doubleValue);
            }
            return doubleCollection;
        }
        
        private class BoundaryConditionReadingException : FileReadingException
        {
            public BoundaryConditionReadingException(string message)
                : base(message)
            {
            }
        }
        
        private class LateralDischargeReadingException : FileReadingException
        {
            public LateralDischargeReadingException(string message)
                : base(message)
            {
            }
        }
    }
}
