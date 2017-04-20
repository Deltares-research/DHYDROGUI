using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Sobek.Readers.Readers
{
    /// <summary>
    /// His file (Sobek) reader
    /// </summary>
    public class HisFileReader: IDisposable
    {
        private HisFileHeader hisFileHeader;
        private BinaryReader binaryReader;
        private bool disposed;
        private FileStream fileStream;

        public HisFileReader(string path)
        {
            Open(path);
        }

        public HisFileHeader GetHisFileHeader
        {
            get { return hisFileHeader; }
        }

        public List<HisDataRow> ReadAllData()
        {
            return ReadAllData(null);
        }

        public List<HisDataRow> ReadAllData(string componentName)
        {
            var dataRows = new List<HisDataRow>();
            binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition;
            foreach (var timeStep in hisFileHeader.TimeSteps)
            {
                dataRows.AddRange(ReadTimeStep(timeStep, componentName));
            }
            return dataRows;
        }

        public List<HisDataRow> ReadTimeStep(DateTime timeStep, string componentName)
        {
            var rows = new List<HisDataRow>();
            int timeStepIndex = hisFileHeader.TimeSteps.IndexOf(timeStep);
            int position = hisFileHeader.StreamStartDataPosition + (timeStepIndex*hisFileHeader.StreamDataBlockSize);

            binaryReader.BaseStream.Position = position;

            binaryReader.ReadInt32(); //needed for position (= deltaT as integer)

            foreach (var location in hisFileHeader.Locations)
            {
                foreach (var component in hisFileHeader.Components)
                {
                    double value = Convert.ToDouble(binaryReader.ReadSingle());
                    if (componentName == null || componentName.ToLower() == component.ToLower())
                    {
                        var row = new HisDataRow
                                             {
                                                 LocationName = location,
                                                 Component = component,
                                                 TimeStep = timeStep,
                                                 Value = value
                                             };
                        rows.Add(row);
                    }
                }
            }

            return rows;
        }

        public List<HisDataRow> ReadLocation(string location, string componentName)
        {
            var rows = new List<HisDataRow>();
            int locationIndex = hisFileHeader.Locations.IndexOf(location);
            int componentIndex = hisFileHeader.Components.IndexOf(componentName);
            var bitPosition = (locationIndex * hisFileHeader.Components.Count + componentIndex) * sizeof(float);
            int step = 0;

            binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition;

            while (binaryReader.BaseStream.Position <= (binaryReader.BaseStream.Length - hisFileHeader.StreamDataBlockSize))
            {
                binaryReader.ReadInt32(); //(= deltaT as integer)
                var block = binaryReader.ReadBytes(hisFileHeader.StreamDataBlockSize - sizeof(int));

                var row = new HisDataRow
                {
                    LocationName = location,
                    Component = componentName,
                    TimeStep = hisFileHeader.TimeSteps[step],
                    Value = Convert.ToDouble(BitConverter.ToSingle(block, bitPosition))
                };
                rows.Add(row);
                step++;
            }

            return rows;
        }

        private HisFileHeader ReadHisHeader()
        {
            var hisFileHeader = new HisFileHeader();

            binaryReader.BaseStream.Position = 0;

            //read first 3 lines a 40 char: not needed
            binaryReader.ReadChars(120);

            //line with starttime and unit 
            var charArrayLine4 = binaryReader.ReadChars(40);
            var startTime = GetDateTimeLine4(charArrayLine4);

            //line with starttime and unit
            var timeStepUnitValue = GetTimeStepUnitValueLine4(charArrayLine4);

            //line with starttime and unit
            var timeStepUnit = GetTimeStepUnitLine4(charArrayLine4);

            //nComponents,nArguments
            var nComponents = binaryReader.ReadInt32();
            var nArguments = binaryReader.ReadInt32();

            //lst components
            hisFileHeader.Components = new List<string>(nComponents);
            for (var i = 0; i < nComponents; i++)
            {
                hisFileHeader.Components.Add(new string(binaryReader.ReadChars(20)).Trim());
            }

            //lst arguments (locations)
            hisFileHeader.Locations = new List<string>(nArguments);
            for (int i = 0; i < nArguments; i++)
            {
                binaryReader.ReadInt32(); // loc nummer: not needed
                hisFileHeader.Locations.Add(new string(binaryReader.ReadChars(20)).Trim());
            }

            //Timesteps (int = deltaTime)
            hisFileHeader.TimeSteps = new List<DateTime>();

            //deltaT as integer -> sizeof (int)
            int timeStepSize = sizeof (int) + (nArguments*nComponents*sizeof (float));

            hisFileHeader.StreamStartDataPosition = (int) binaryReader.BaseStream.Position;
            hisFileHeader.StreamDataBlockSize = timeStepSize;

            //set datetimes
            int blockSizeInBytes = timeStepSize - sizeof (int);
            while (binaryReader.BaseStream.Position <= binaryReader.BaseStream.Length - timeStepSize)
            {
                var timeStepValue = binaryReader.ReadInt32();
                hisFileHeader.TimeSteps.Add(startTime + GetTimeStepSpan(timeStepValue,timeStepUnitValue,timeStepUnit));
                binaryReader.ReadBytes(blockSizeInBytes);
            }

            return hisFileHeader;
        }

        private DateTime GetDateTimeLine4(char[] chars)
        {
            //T0: 1995.01.01 00:00:00  (scu=       1s)
            //0123456789012345678901234567890123456789
            int year = int.Parse(new string(new[] { chars[4], chars[5], chars[6], chars[7] }));
            int month = int.Parse(new string(new[] { chars[9], chars[10] }));
            int day = int.Parse(new string(new[] { chars[12], chars[13] }));
            int hours = int.Parse(new string(new[] { chars[15], chars[16] }));
            int minutes = int.Parse(new string(new[] { chars[18], chars[19] }));
            int seconds = int.Parse(new string(new[] { chars[21], chars[22] }));
            return new DateTime(year, month, day, hours, minutes, seconds);
        }

        private int GetTimeStepUnitValueLine4(char[] chars)
        {
            //T0: 1995.01.01 00:00:00  (scu=       1s)
            //0123456789012345678901234567890123456789
            return int.Parse(new string(new[] { chars[30],chars[31],chars[32],chars[33],chars[34],chars[35],chars[36], chars[37] }));
        }

        private string GetTimeStepUnitLine4(char[] chars)
        {
            //T0: 1995.01.01 00:00:00  (scu=       1s)
            //0123456789012345678901234567890123456789
            return new string(new[]{chars[38]});
        }

        private TimeSpan GetTimeStepSpan(int timeStepValue, int timeStepUnitValue, string timeStepUnit)
        {
           switch(timeStepUnit.ToLower())
           {
               case "s":
                   return new TimeSpan(0, 0, timeStepUnitValue * timeStepValue);
               case "m":
                   return new TimeSpan(0, timeStepUnitValue * timeStepValue, 0);
               case "h":
                   return new TimeSpan(timeStepUnitValue * timeStepValue, 0, 0);
               case "d":
                   return new TimeSpan(timeStepUnitValue * timeStepValue, 0, 0 ,0);
               default:
                   throw new ArgumentException(timeStepUnit + " is not supported as time unit argument");
           }
        }

        public void Open(string path)
        {
            Close();
            fileStream = File.Open(path, FileMode.Open);
            binaryReader = new BinaryReader(fileStream);
            hisFileHeader = ReadHisHeader();
        }

        public void Close()
        {
            if (fileStream != null)
            {
                fileStream.Close();
                binaryReader.Close();
                fileStream = null;
            }
        }

        /// <summary>
        /// See <see cref="System.IDisposable.Dispose"/> for more information.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Called when the object is being disposed or finalized.
        /// </summary>
        /// <param name="disposing">True when the object is being disposed (and therefore can
        /// access managed members); false when the object is being finalized without first
        /// having been disposed (and therefore can only touch unmanaged members).</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }

            disposed = true;
        }

        public struct HisFileHeader
        {
            public string Path;
            public List<DateTime> TimeSteps;
            public List<string> Locations;
            public List<string> Components;
            public int StreamStartDataPosition;
            public int StreamDataBlockSize;
        }

        public class HisDataRow
        {
            public string LocationName;
            public string Component;
            public DateTime TimeStep;
            public double Value;
            public INetworkLocation NetworkLocation; //for extern usage
        }

    }
}
