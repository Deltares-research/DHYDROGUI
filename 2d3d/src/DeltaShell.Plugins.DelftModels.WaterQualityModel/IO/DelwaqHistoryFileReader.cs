using System;
using System.Globalization;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public static class DelwaqHistoryFileReader
    {
        /// <summary>
        /// Reads from <paramref name="delwaqOutputFile"/> and returns a List of <see cref="DelwaqHisFileData"/>.
        /// </summary>
        /// <param name="delwaqOutputFile"> </param>
        /// <returns>
        /// Returns an empty List if file does not exist or is emtpy.
        /// Otherwise returns a list of <see cref="DelwaqHisFileData"/>.
        /// </returns>
        public static DelwaqHisFileData[] Read(string delwaqOutputFile)
        {
            BinaryReader reader = null;

            DelwaqHisFileData[] result;

            // Check whether the output file exits or not
            if (!File.Exists(delwaqOutputFile))
            {
                return new DelwaqHisFileData[0];
            }

            // Check whether the output file is empty or not
            if (new FileInfo(delwaqOutputFile).Length == 0)
            {
                return new DelwaqHisFileData[0];
            }

            try
            {
                reader = new BinaryReader(File.Open(delwaqOutputFile, FileMode.Open));

                reader.ReadChars(40 * 3); // Skip the first 3 headers (these contain meta-data that we do not need)

                // Read and parse the first time step string
                string timeString = new string(reader.ReadChars(40)).Substring(4, 19);
                DateTime firstTimeStep = DateTime.Parse(timeString, CultureInfo.InvariantCulture);

                // Read the number of output variables (substances and output parameters) and observation variables
                int numberOfOutputVariables = reader.ReadInt32();
                int numberOfObservationVariables = reader.ReadInt32();
                result = new DelwaqHisFileData[numberOfObservationVariables];
                // Read all output parameter names
                var outputVariables = new string[numberOfOutputVariables];
                for (var i = 0; i < numberOfOutputVariables; i++)
                {
                    outputVariables[i] = new string(reader.ReadChars(20)).Trim(' ');
                }

                // Read the observation variable names and add variable data objects for them
                if (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    for (var i = 0; i < numberOfObservationVariables; i++)
                    {
                        reader.ReadInt32(); // Skip the variable number
                        string observationVariable = new string(reader.ReadChars(20)).Trim(' ');
                        result[i] = new DelwaqHisFileData(observationVariable) { OutputVariables = outputVariables };
                    }
                }

                // Read the observation variable values for each output variable per time step
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    DateTime currentTimeStep = firstTimeStep.AddSeconds(reader.ReadInt32());

                    for (var i = 0; i < numberOfObservationVariables; i++)
                    {
                        for (var j = 0; j < numberOfOutputVariables; j++)
                        {
                            result[i].AddValueForTimeStep(currentTimeStep, reader.ReadSingle());
                        }
                    }
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return result;
        }
    }
}