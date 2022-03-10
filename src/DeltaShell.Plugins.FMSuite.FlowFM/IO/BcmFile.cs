using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class BcmFile : BcFile
    {       
        public const string Extension = ".bcm";

        private const string BlockKey = "table-name";
        private const string ContentsKey = "contents";
        private const string LocationKey = "location";
        private const string TimeFunctionKey = "time-function";
        private const string ReferenceTimeKey = "reference-time";
        private const string TimeUnitKey = "time-unit";
        private const string InterpolationKey = "interpolation";
        private const string ParameterKey = "parameter";
        private const string RecordsInTableKey = "records-in-table";
        private const string UnitKey = "unit";
        private const string TimeUnit = "minutes";
        private int blocknr = 0;

        private readonly List<FlowBoundaryQuantityType> supportedProcesses = new List<FlowBoundaryQuantityType>()
        {
            FlowBoundaryQuantityType.MorphologyBedLevelPrescribed,
            FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed,
            FlowBoundaryQuantityType.MorphologyBedLoadTransport,
            FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint,
            FlowBoundaryQuantityType.MorphologyBedLevelFixed,
        };

        private readonly int columnWidth = RecordsInTableKey.Length + 1; /* Largest string length */

        static string[] SplitString(string str)
        {
            return str.Split('\'')
                .Select((element, index) => index % 2 == 0 // If even index
                    ? element.SplitOnEmptySpace() // Split the item
                    : new string[] { element }) // Keep the entire item
                .SelectMany(element => element).ToArray();
        }

        protected override List<string> SupportedProcesses
        {
            get
            {
                return supportedProcesses.Select(sp => FlowBoundaryCondition.GetProcessNameForQuantity(sp)).Distinct().ToList();
            }
        }

        private void WriteKyeValuePairParameterLine(string parameter, string unit)
        {
            var valueString = WriteBetweenCommas(parameter);
            if (unit != null)
            {
                valueString = valueString + " " + UnitKey + " " + WriteBetweenCommas(unit);
            }
            WriteKeyValuePairLine(ParameterKey, valueString);
        }

        private string WriteBetweenCommas(string value)
        {
            return "\'" + value + "\'";
        }

        private void WriteKeyValuePairLine(string keyString, string valueString)
        {
            WriteLine(keyString.PadRight(columnWidth) + valueString);
        }

        public override void Write(
            IEnumerable<KeyValuePair<IBoundaryCondition, BoundaryConditionSet>> boundaryConditions,
            string filePath, BcFileFlowBoundaryDataBuilder boundaryDataBuilder, DateTime? refDate = null)
        {
            blocknr = 0;
            base.Write(boundaryConditions, filePath, boundaryDataBuilder, refDate);
        }
        

        protected override void WriteBlock(BcBlockData block)
        {
            var bcmBlock = block as BcmBlockData;
            if (bcmBlock == null) return;

            var startDateTime = new DateTime();
            blocknr++;
            WriteKeyValuePairLine(BlockKey, WriteBetweenCommas(string.Format("Boundary Section : {0}", blocknr)));
            WriteKeyValuePairLine(LocationKey, WriteBetweenCommas(bcmBlock.Location));
            WriteKeyValuePairLine(ContentsKey, WriteBetweenCommas("Uniform"));
            WriteKeyValuePairLine(TimeFunctionKey, WriteBetweenCommas("non-equidistant"));

            if (bcmBlock.TimeInterpolationType != null)
            {
                WriteKeyValuePairLine(InterpolationKey, WriteBetweenCommas(bcmBlock.TimeInterpolationType));
            }

            if (bcmBlock.Quantities != null && bcmBlock.Quantities.Count > 0)
            {

                //Extract start time reference.
                var bcmBlockQuantity = bcmBlock.Quantities[0] as BcmQuantityData;
                if (bcmBlockQuantity == null) return;

                var strFullTime = bcmBlockQuantity.Values.FirstOrDefault();
                var timeReference = "";
                if (bcmBlockQuantity.ReferenceTime != null)
                {
                    timeReference = bcmBlockQuantity.ReferenceTime;
                    startDateTime = StringDateToDateTime(bcmBlockQuantity.ReferenceTime, "yyyyMMdd");
                    WriteKeyValuePairLine(ReferenceTimeKey, timeReference);
                    WriteKeyValuePairLine(TimeUnitKey, WriteBetweenCommas(TimeUnit));
                }
                if (strFullTime != null && strFullTime.Length == 8)
                {
                    timeReference = strFullTime.Substring(0,8); //yyyymmdd (we do not include the hhmmss when writing the time reference
                    WriteKeyValuePairLine(ReferenceTimeKey, timeReference);

                    WriteKeyValuePairLine(TimeUnitKey, WriteBetweenCommas(TimeUnit));
                }
                
            }

            foreach (var quantity in bcmBlock.Quantities)
            {
                WriteKyeValuePairParameterLine(quantity.Quantity, quantity.Unit);
            }

            var rowCount = bcmBlock.Quantities.Select(q => q.Values.Count).Min();
            if (rowCount == 0) return;
            if (bcmBlock.Quantities != null)
            {
                WriteKeyValuePairLine(RecordsInTableKey, rowCount.ToString());
            }

            var columnWidths =
                bcmBlock.Quantities.Select(q => q.Values.Take(rowCount).Select(s => s.Length).Max() + 1).ToList();

            for (var i = 0; i < rowCount; ++i)
            {
                var j = 0;
                WriteLine(string.Join(" ", bcmBlock.Quantities.Select(q => GetTimeStep(q.Values[i], j, startDateTime, TimeUnit).PadRight(columnWidths[j++]))).TrimEnd());
            }
        }

        private string GetTimeStep(string value, int i, DateTime startTime, string timeUnit)
        {
            if (i == 0)
            {
                var dateValue = StringDateToDateTime(value, "yyyyMMddHHmmss");
                var diff = (dateValue - startTime);
                switch (timeUnit)
                {
                    case "seconds":
                        return diff.TotalSeconds.ToString();
                    case "minutes":
                        return diff.TotalMinutes.ToString();
                    case "hours":
                        return diff.TotalHours.ToString();
                    case "days":
                        return diff.TotalDays.ToString();
                }
            }
            return value;
        }

        private DateTime StringDateToDateTime(string value, string format)
        {
            var date = new DateTime();
            try
            {
                date = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                log.Error("Could not load the reference time correctly, check the format. Using Now as a time reference instead.");
                date = DateTime.Now;
            }

            return date;
        }
        public override IEnumerable<BcBlockData> Read(string inputFile)
        {
                OpenInputFile(inputFile);
            try
            {
                var line = GetNextLine();
                while (line != null)
                {
                    if (line.StartsWith(BlockKey))
                    {
                        line = GetNextLine();
                    }

                    if (line.StartsWith(LocationKey))
                    {
                        var splitHeader = SplitString(line);
                        var boundaryName = splitHeader.Length == 2 ? splitHeader[1] : "BoundaryName";
                        var block = ReadDataBlock(out line, boundaryName);
                        if (block != null)
                        {
                            yield return block;
                        }
                    }
                    else
                    {
                        log.WarnFormat("Omitting line {0} not strarting with {1}", LineNumber, BlockKey);
                        line = GetNextLine();
                    }
                }
            }
            finally
            {
                CloseInputFile();
            }
        }

        private BcmBlockData ReadDataBlock(out string line, string blockName)
        {
            string contentsValue = null;
            string locationValue = blockName;
            string timeFunctionValue = "timeseries";      //time-function
            DateTime referenceTimeValue = new DateTime(); //with the timeUnitValue helps determine the time reference and time steps for each entry.
            string interpolationValue = null;             //timeInterpolationType
            string parameterValue = null;

            var parameterCount = 0;
            var quantityDataList = new List<BcQuantityData>();

            BcmQuantityData quantityData = null;

            line = GetNextLine();

            while (line != null)
            {
                var split = SplitString(line);
                if (split.Length < 2)
                {
                    break;
                }

                if (split[0] == ParameterKey)
                {
                    parameterValue = split[1];
                    quantityData = new BcmQuantityData
                    {
                        Quantity = parameterValue,
                        ReferenceTime = referenceTimeValue.ToString("yyyyMMdd")
                    };
                    if (split.Length == 4 && split[2] == "unit")
                    {
                        quantityData.Unit = split[3];
                    }

                    quantityDataList.Add(quantityData);
                }
                if (split[0] == ContentsKey)
                {
                    contentsValue = timeFunctionValue;
                }
                if (split[0] == InterpolationKey)
                {
                    interpolationValue = split[1];
                }
                if (split[0] == ReferenceTimeKey)
                {
                    referenceTimeValue = StringDateToDateTime(split[1], "yyyyMMdd");
                }
                if (split[0] == RecordsInTableKey && split.Length == 2)
                {
                    parameterCount = quantityDataList.Count;
                    int.TryParse(split[1], out var recordNumber);
                    //Read number of records
                    while (recordNumber > 0)
                    {
                        line = GetNextLine();
                        if (line == null) break;
                        recordNumber -= 1;

                        var columns = SplitString(line);
                        if (columns.Length < parameterCount)
                        {
                            log.WarnFormat("Omitting line {0} with less than {1} columns", LineNumber, parameterCount);
                        }
                        else if (columns.Length > parameterCount)
                        {
                            log.WarnFormat("Omitting line {0} with more than {1} columns", LineNumber, parameterCount);
                        }
                        else
                        {
                            for (var i = 0; i < parameterCount; ++i)
                            {
                                quantityDataList[i].Values.Add(columns[i]);
                            }
                        }
                    }
                }
                line = GetNextLine();
                if (line == null || line.StartsWith(BlockKey)) break;
            }
            
            if (blockName == null || !quantityDataList.Any()) //FunctionType cannot be null! but for now we are hardcoding it.
            {
                return null;
            }
            return
                new BcmBlockData 
                {
                    FilePath = InputFilePath,
                    SupportPoint = blockName,
                    FunctionType = contentsValue, //Forced, for the moment we did not receive the format of the bcm file and we do not know what to map this to.
                    TimeInterpolationType = interpolationValue,
                    Location = locationValue,
                    Quantities = quantityDataList
                };
        }

        
    }
}
