using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils.Extensions;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.NGHS.IO.FileReaders.Boundary
{
    // Note: In this class we do not create new BoundaryConditions, these are created when adding a node to the network
    //       Instead we retrieve the BoundaryCondition (based on feature name) and update the properties
    //       The same is true of LateralSources
    public class BoundaryFileReader : IBoundaryFileReader
    {
        public static void ReadFile(string filename, IEnumerable<Model1DBoundaryNodeData> boundaryConditions)
        {
            if (!File.Exists(filename)) throw new FileReadingException(string.Format(Resources.Could_not_read_file_0_properly_it_doesnt_exist, filename));
            var iniSections = new BcReader(new FileSystem()).ReadBcFile(filename);
            if (!iniSections.Any()) throw new FileReadingException(string.Format(Resources.Could_not_read_file_0_properly_it_seems_empty, filename));

            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();

            foreach (var boundarySection in iniSections.Where(section => section.Section.Name.Equals( BoundaryRegion.BcBoundaryHeader, StringComparison.InvariantCultureIgnoreCase) || 
                                                                          section.Section.Name.Equals(BoundaryRegion.BcForcingHeader, StringComparison.InvariantCultureIgnoreCase)))
            {
                try
                {
                    var name = boundarySection.Section.ReadProperty<string>(BoundaryRegion.Name.Key);
                    if (name == FunctionAttributes.StandardFeatureNames.ModelWide)
                    {
                        ReadModelWideBoundaryCondition(boundarySection);
                        continue;
                    }

                    var model1DBoundaryNodeDatas = boundaryConditions as Model1DBoundaryNodeData[] ?? boundaryConditions.ToArray();
                    var waterFlowModel1DBoundaryNodeData = model1DBoundaryNodeDatas.FirstOrDefault(bc => bc.Feature.Name == name);
                    if (waterFlowModel1DBoundaryNodeData == null)
                    {
                        var manHoleName = boundarySection.Section.ReadProperty<string>("manHoleName", true);
                        if (manHoleName == null) continue;
                        waterFlowModel1DBoundaryNodeData = model1DBoundaryNodeDatas.FirstOrDefault(bc => bc.Feature.Name == manHoleName);
                        if (waterFlowModel1DBoundaryNodeData == null)
                            continue; 
                    }

                    if (waterFlowModel1DBoundaryNodeData.Node is Manhole manhole)
                    {
                        // name is compartment name not node name
                        var compartment = manhole.Compartments.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.InvariantCultureIgnoreCase));
                        OutletCompartment outlet = null;
                        
                        if (compartment == null)
                        {
                            outlet = manhole.Compartments.OfType<OutletCompartment>().FirstOrDefault();
                            if (outlet == null)
                            {
                                compartment = manhole.Compartments.FirstOrDefault();
                                if (compartment != null)
                                {
                                    manhole.UpdateCompartmentToOutletCompartment(compartment);
                                }
                                
                            }
                            outlet = manhole.Compartments.OfType<OutletCompartment>().FirstOrDefault();
                        }
                        else if (compartment is OutletCompartment outletCompartment)
                        {
                            outlet = outletCompartment;
                        }
                        else if (compartment is Compartment)
                        {
                            manhole.UpdateCompartmentToOutletCompartment(compartment);
                            outlet = manhole.Compartments.OfType<OutletCompartment>().FirstOrDefault(oc => oc.Name.Equals(compartment.Name, StringComparison.InvariantCultureIgnoreCase));
                        }

                        if (outlet != null) waterFlowModel1DBoundaryNodeData.OutletCompartment = outlet;
                    }
                    ReadBoundaryCondition(waterFlowModel1DBoundaryNodeData, boundarySection);
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
        
        /// <summary>
        /// Parses each lateral sources section from the specified file to a <see cref="ILateralSourceBcSection"/>.
        /// </summary>
        /// <param name="filePath">The full file path to the boundary conditions file.</param>
        /// <param name="logHandler"> Optional parameter; the log handler to report errors. </param>
        /// <returns> A collection of parsed <see cref="ILateralSourceBcSection"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="filePath"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown when the file at <paramref name="filePath"/> does not exist.
        /// </exception>
        public IEnumerable<ILateralSourceBcSection> ReadLateralSourcesFromBcFile(string filePath, ILogHandler logHandler = null)
        {
            Ensure.NotNullOrEmpty(filePath, nameof(filePath));
            EnsureFileExists(filePath);

            IEnumerable<BcIniSection> iniSections = new BcReader(new FileSystem()).ReadBcFile(filePath);
            foreach (BcIniSection section in iniSections)
            {
                if (!section.Section.Name.EqualsCaseInsensitive(BoundaryRegion.BcLateralHeader) &&
                    !section.Section.Name.EqualsCaseInsensitive(BoundaryRegion.BcForcingHeader))
                {
                    continue;
                }

                IEnumerable<IBcQuantityData> salinity = section.Table.Where(bcq => bcq.Quantity.Value.EqualsCaseInsensitive(BoundaryRegion.QuantityStrings.WaterSalinity));
                if (salinity.Any())
                {
                    continue;
                }

                yield return new LateralSourceBcSection(section, new BcSectionParser(logHandler));
            }
        }

        private static void EnsureFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }
        }

        private static void ReadModelWideBoundaryCondition(BcIniSection boundaryIniSection)
        {
            var functionType = boundaryIniSection.Section.ReadProperty<string>(BoundaryRegion.Function.Key);
            switch (functionType)
            {
                case BoundaryRegion.FunctionStrings.Constant:
                    break;
                case BoundaryRegion.FunctionStrings.QhTable:
                    break;
                case BoundaryRegion.FunctionStrings.TimeSeries:
                    
                    
                    var modelWideWindBoundaryConditionQuantities = boundaryIniSection.Table.Select(t => t.Quantity).Select(q=>q.Value);
                    foreach (var modelWideWindBoundaryConditionQuantity in modelWideWindBoundaryConditionQuantities)
                    {
                        switch (modelWideWindBoundaryConditionQuantity)
                        {
                            case BoundaryRegion.QuantityStrings.WindSpeed:
                            {
                                break;
                            }

                            case BoundaryRegion.QuantityStrings.WindDirection:
                            {
                                break;
                            }
                        }
                    }

                    break;
                default:
                    var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", boundaryIniSection.Section.Name,
                        BoundaryRegion.Function.Key, Environment.NewLine);
                    throw new BoundaryConditionReadingException(errorMessage);
            }
        }

        private static void ReadBoundaryCondition(Model1DBoundaryNodeData boundaryCondition, BcIniSection boundaryIniSection)
        {
            var saltBoundaryQuantity = boundaryIniSection.Table.Where(bcq => bcq.Quantity.Value == BoundaryRegion.QuantityStrings.WaterSalinity);
            if (saltBoundaryQuantity.Any()) return;

            var function = boundaryIniSection.Section.ReadProperty<string>(BoundaryRegion.Function.Key);
            if (function.ToLower().Equals(BoundaryRegion.FunctionStrings.Constant.ToLower()))
            {
                switch (boundaryIniSection.Table[0].Quantity.Value)
                {
                    case BoundaryRegion.QuantityStrings.WaterDischarge:
                        boundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowConstant;
                        boundaryCondition.Flow = ReadConstantValue(boundaryIniSection.Table[0], boundaryIniSection.Section.Name);
                        break;
                    case BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR:
                    case BoundaryRegion.QuantityStrings.WaterLevel:
                        boundaryCondition.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
                        boundaryCondition.WaterLevel = ReadConstantValue(boundaryIniSection.Table[0], boundaryIniSection.Section.Name);
                        var manhole = boundaryCondition.Node as Manhole;
                        if (manhole != null)
                        {
                            var outletCandidate = manhole.GetOutletCandidate();
                            if (outletCandidate != null)
                            {
                                var outlet = manhole.UpdateCompartmentToOutletCompartment(outletCandidate);
                                outlet.SurfaceWaterLevel = boundaryCondition.WaterLevel;
                            }
                            else
                            {
                                outletCandidate = manhole.Compartments.OfType<OutletCompartment>().SingleOrDefault(oc => string.Equals(oc.Name, manhole.Name, StringComparison.InvariantCultureIgnoreCase));
                                if (outletCandidate != null)
                                {
                                    ((OutletCompartment) outletCandidate).SurfaceWaterLevel = boundaryCondition.WaterLevel;
                                }
                            }
                        }
                        break;
                }
            }
            else if (function.ToLower().Equals(BoundaryRegion.FunctionStrings.QhTable.ToLower()))
            {
                boundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowWaterLevelTable;
                SetSectionValuesToFeatureData(boundaryCondition, boundaryIniSection, ConvertStringsToDoubles, ConvertStringsToDoubles);
            }
            else if (function.ToLower().Equals(BoundaryRegion.FunctionStrings.TimeSeries.ToLower()))
            {
                switch (boundaryIniSection.Table[1].Quantity.Value)
                {
                    case BoundaryRegion.QuantityStrings.WaterDischarge:
                        boundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;
                        break;
                    case BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR:
                    case BoundaryRegion.QuantityStrings.WaterLevel:
                        boundaryCondition.DataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries;
                        break;
                }
                SetSectionValuesToFeatureData(boundaryCondition, boundaryIniSection, GetDateTimesValues, ConvertStringsToDoubles);
            }
            else
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", boundaryIniSection.Section.Name, BoundaryRegion.Function.Key, Environment.NewLine);
                throw new BoundaryConditionReadingException(errorMessage);
            }
        }

        private static void SetSectionValuesToFeatureData<Targ>(IFeatureData featureData, BcIniSection iniSection, Func<IBcQuantityData, IEnumerable<Targ>> parseArgumentValues, Func<IBcQuantityData, IEnumerable<double>> parseFunctionValues)
        {
            var argumentValues = parseArgumentValues(iniSection.Table[0]);
            if (argumentValues == null)
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", iniSection.Section.Name,
                    iniSection.Table[0].Quantity, Environment.NewLine);
                throw new BoundaryConditionReadingException(errorMessage);
            }
            var functionValues = parseFunctionValues(iniSection.Table[1]);
            if (functionValues == null)
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", iniSection.Section.Name,
                    iniSection.Table[1].Quantity, Environment.NewLine);
                throw new BoundaryConditionReadingException(errorMessage);
            }
            
            var function = ((IFunction)featureData.Data);
            function.Clear();
            var orgSortValue = function.Arguments[0].IsAutoSorted;
            function.Arguments[0].IsAutoSorted = false;
            function.Arguments[0].SetValues(argumentValues);
            function.SetValues(functionValues);
            function.Arguments[0].IsAutoSorted = orgSortValue;
            function.Arguments[0].ExtrapolationType = ExtrapolationType.Linear;
            var periodic = iniSection.Section.ReadProperty<string>(BoundaryRegion.Periodic.Key, true);
            if (!string.IsNullOrEmpty(periodic) && periodic == "true")
                function.Arguments[0].ExtrapolationType = ExtrapolationType.Periodic;


        }

        private static double ReadConstantValue(IBcQuantityData quantityData, string sectionName)
        {
            double constantValue;
            if (!double.TryParse(quantityData.Values[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out constantValue))
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}.{2}", sectionName, quantityData.Quantity, Environment.NewLine);
                throw new LateralDischargeReadingException(errorMessage);
            }

            return constantValue;
        }

        private static IEnumerable<DateTime> GetDateTimesValues(IBcQuantityData quantityData)
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
       
        private static IEnumerable<double> ConvertStringsToDoubles(IBcQuantityData bcQuantityData)
        {
            var doubleCollection = new List<double>();
            foreach (var stringValue in bcQuantityData.Values)
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
