using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using java.util;
using nl.wldelft.fews.pi;
using nl.wldelft.util.timeseries;
using TimeZone = java.util.TimeZone;

namespace Deltares.IO.FewsPI
{
    public class TimeEvent
    {
        public DateTime Time { get; set; }
        public double Value { get; set; }
    }

    public static class Java2DotNetHelper
    {
        private const long TicksPerMilli = 10000;
        private static readonly long epochOffSet = new DateTime(1970, 1, 1, 0, 0, 0).Ticks;

        public static DateTime DotNetDateTimeFromJavaMillies(long javaTimeInMillies)
        {
            return new DateTime(javaTimeInMillies * TicksPerMilli + epochOffSet);
        }

        public static TimeSpan DotNetTimeSpanFromJavaMillies(long javaTimeInMillies)
        {
            return new TimeSpan(javaTimeInMillies * TicksPerMilli + epochOffSet);
        }

        public static long JavaMilliesFromDotNetDateTime(DateTime dateTime)
        {
            return (dateTime.Ticks - epochOffSet) / TicksPerMilli;
        }

        public static long TicksToMillis(long ticks)
        {
            return (ticks/TicksPerMilli);
        }

        public static long MillisToTicks(long javaMillis)
        {
            return javaMillis*TicksPerMilli;
        }
    }

    internal static class TestDirResolver
    {
        public const string TestDirPlaceHolder = "%TEST_DIR%";

        public static java.io.File ResolveTestDirectory(this java.io.File file, string testDirectoryPath)
        {
            string path = file.getPath();
            if (!string.IsNullOrEmpty(path))
                path = path.Replace(TestDirPlaceHolder, testDirectoryPath);
            return new java.io.File(path);
        }
    }

    public class TimeSeriesWriter
    {
        private readonly TimeSeriesArray[] timeSeriesArrays;
        private readonly bool writeAsBinary;
        private readonly string filePath;
        private readonly TimeZone timeZone;

        public TimeSeriesWriter(TimeSeriesArrays fewsTimeSeriesArrays, bool writeAsBinary, string filePath, TimeZone timeZone)
        {
            timeSeriesArrays = new TimeSeriesArray[fewsTimeSeriesArrays.size()];
            for (int i = 0; i < fewsTimeSeriesArrays.size(); i++)
            {
                timeSeriesArrays[i] = fewsTimeSeriesArrays.get(i);
            }
            this.writeAsBinary = writeAsBinary;
            this.filePath = filePath;
            this.timeZone = timeZone;
        }

        public IEnumerable<TimeSeriesArray> TimeSeriesArrays
        {
            get { return timeSeriesArrays; }
        }

        public void Write()
        {
            TimeSeriesArrays fewsTimeSeriesArrays = new TimeSeriesArrays(timeSeriesArrays);
            PiTimeSeriesSerializer piTimeSeriesSerializer = new PiTimeSeriesSerializer();
            piTimeSeriesSerializer.setVersion(PiVersion.VERSION_1_4);
            if (writeAsBinary)
            {
                piTimeSeriesSerializer.setEventDestination(PiTimeSeriesSerializer.EventDestination.SEPARATE_BINARY_FILE);
            }
            fewsTimeSeriesArrays.write(new java.io.File(filePath), piTimeSeriesSerializer, timeZone);
        }
    }

    public class ProfilesFileWriteInfo
    {
        public ProfilesFileWriteInfo()
        {            
            Profiles = new List<TimeSeriesArray>();
        }

        public TimeZone TimeZone { get; set; }
        public List<TimeSeriesArray> Profiles { get; private set; }
        public string File { get; set; }
    }

    public class RunInfo
    {
        private const string TestFolderPlaceHolderId = "%TEST_DIR%";

        /// <summary>
        /// The key name for DeltaShell project file (deltaShellProjectFile) in the key value pair property list in the config
        /// </summary>
        const string DeltaShellProjectFileKey = "deltaShellProjectFile";

        /// <summary>
        /// The key name for piTimeSeriesAsBin in the key value pair property list in the config
        /// </summary>
        const string PiTimeSeriesAsBinKey = "piTimeSeriesAsBin";

        /// <summary>
        /// The key name for piTimeSeriesAsBin in the key value pair property list in the config
        /// </summary>
        const string ModelKey = "model";

        /// <summary>
        /// The key name for piTimeSeriesAsBin in the key value pair property list in the config
        /// </summary>
        const string SaveResultsInCopyOfProjectKey = "saveResultsInCopyOfProject";

        /// <summary>
        /// The key name for letting flow start from 1D-points.xyz file
        /// </summary>
        const string Use1DPointsFileForRestartKey = "use1dPointsFileForRestart";

        /// <summary>
        /// The key name for exporting staggered grid points
        /// </summary>
        const string ExportStaggeredGridPointsKey = "exportStaggeredGridPoints";

        /// <summary>
        /// The key name for exporting staggered grid points
        /// </summary>
        const string DoCheckBackwardsCompatibilityKey = "checkBackwardsCompatibility";

        /// <summary>
        /// The key name for piTimeSeriesAsBin in the key value pair property list in the config
        /// </summary>
        const string ModelWorkingDirectoryKey = "workDir|";

        /// <summary>
        /// The key name for specifying that non-fews-adapter debug messages must be logged
        /// </summary>
        const string DoLogNonFewsDebugMessagesKey = "logNonFewsDebugMessages";

        private readonly string piRunFilePath;
        private readonly List<Exception> errors = new List<Exception>();
        private readonly PiRunFileReader reader;
        private IList<TimeSeriesWriter> outputTimeSeriesWriters;

        public RunInfo(string piRunFilePath)
        {
            this.piRunFilePath = piRunFilePath;
            reader = new PiRunFileReader(new java.io.File(piRunFilePath));
        }

        /// <summary>
        /// Validates the run info instance data.
        /// </summary>
        /// <returns>A list of exceptions when errors are found</returns>
        public IEnumerable<Exception> Validate()
        {
            errors.Clear();

            if (string.IsNullOrEmpty(ModelName) || ModelName.Trim() == "")
                errors.Add(new FewsPiException("The '" + ModelKey + "' key or value is missing"));

            IEnumerable<string> files = GetFiles(reader.getInputTimeSeriesFiles())
                .Concat(GetFiles(reader.getOutputTimeSeriesFiles()))
                .Concat(InputStateDescriptionFiles);

            if (string.IsNullOrEmpty(ProjectFile) || ProjectFile.Trim() == "")
            {
                errors.Add(new FewsPiException("The '" + DeltaShellProjectFileKey + "' key or value is missing"));
            }
            else
            {
                files = files.Concat(new[] {ProjectFile});
            }

            errors.AddRange((from file in files 
                    where !File.Exists(file) 
                    select new FileNotFoundException(string.Format("File '{0}' is not found", file)))
                .Cast<Exception>());

            return errors;
        }
        
        /// <summary>
        /// Gets input time series. Data is read/loaded from the files
        /// </summary>
        public IEnumerable<TimeSeries> InputTimeSeries
        {
            get
            {
                List inputTimeSeriesFiles = reader.getInputTimeSeriesFiles();

                List<TimeSeries> timeSeries = new List<TimeSeries>();
                for (int i = 0; i < inputTimeSeriesFiles.size(); i++)
                {
                    var inputTimeSeriesFile = (java.io.File)inputTimeSeriesFiles.get(i);
                    inputTimeSeriesFile = inputTimeSeriesFile.ResolveTestDirectory(GetTestDirectoryPath(piRunFilePath));

                    PiTimeSeriesReader timeSeriesReader = new PiTimeSeriesReader(inputTimeSeriesFile);
                    TimeSeriesArrays timeSeriesArraysInFile = timeSeriesReader.read();

                    for (int j = 0; j < timeSeriesArraysInFile.size(); j++)
                    {
                        timeSeries.Add(new TimeSeries(timeSeriesArraysInFile.get(j)));
                    }
                }

                return timeSeries;
            }
        }

        /// <summary>
        /// Gets a list of longitudinal profile files
        /// </summary>
        /// <remarks>
        /// If the %TEST_DIR% place holder is found it will be replaced with the data folder location (by default this string is empty)
        /// </remarks>
        internal IEnumerable<string> OutputLongitudinalProfilesFiles
        {
            get { return GetFiles(reader.getOutputLongitudinalProfilesFiles()); }
        }

        public string OutputStateDescriptionFile
        {
            get
            {
                java.io.File outputStateDescriptionFile = reader.getOutputStateDescriptionFile();
                if (outputStateDescriptionFile != null)
                {
                    return ResolveTestFolderPlaceholder(outputStateDescriptionFile.getPath());
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the output time series from file.
        /// </summary>
        /// <remarks>
        /// These profiles are normally empty but the meta data (such as parameterId and locationId) is 
        /// used for quering the DeltaShell model
        /// </remarks>
        public IEnumerable<ProfilesFileWriteInfo> LongitudinalProfilesFileWriteInfo
        {
            get
            {
                foreach (var file in OutputLongitudinalProfilesFiles)
                {
                    PiProfilesReader piProfilesReader = new PiProfilesReader(file);
                    TimeSeriesArrays timeSeriesArrays = piProfilesReader.read();
                    var writeInfo = new ProfilesFileWriteInfo { File = file, TimeZone = piProfilesReader.getTimeZone() };
                    for (int i = 0; i < timeSeriesArrays.size(); i++ )
                    {
                        writeInfo.Profiles.Add(timeSeriesArrays.get(i));
                    }

                    yield return writeInfo;
                }
            }
        }
       
        /// <summary>
        /// Gets the input state description files
        /// </summary>
        /// <remarks>
        /// If the %TEST_DIR% place holder is found it will be replaced with the data folder location (by default this string is empty)
        /// </remarks>
        public IEnumerable<string> InputStateDescriptionFiles
        {
            get { return GetFiles(reader.getInputStateDescriptionFiles()); }
        }

        /// <summary>
        /// Gets the output time series from file.
        /// </summary>
        /// <remarks>
        /// These time series are normally empty but the meta data (such as parameterId and locationId) is 
        /// used for the time series queries in the DeltaShell model
        /// </remarks>
        public IEnumerable<TimeSeriesWriter> OutputTimeSeriesWriter
        {
            get
            {
                if (outputTimeSeriesWriters == null)
                {
                    outputTimeSeriesWriters = new List<TimeSeriesWriter>();

                    List fileList = reader.getOutputTimeSeriesFiles();

                    for (int i = 0; i < fileList.size(); i++)
                    {
                        var file = (java.io.File)fileList.get(i);
                        file = file.ResolveTestDirectory(GetTestDirectoryPath(piRunFilePath));

                        PiTimeSeriesParser piTimeSeriesParser = new PiTimeSeriesParser();
                        TimeSeriesArrays fewsTimeSeriesArrays = TimeSeriesUtils.read(file, piTimeSeriesParser);
                        var timeSeriesWriter = new TimeSeriesWriter(fewsTimeSeriesArrays, PiTimeSeriesAsBin, file.getAbsolutePath(), piTimeSeriesParser.getTimeZone());

                        outputTimeSeriesWriters.Add(timeSeriesWriter);
                    }
                }
                return outputTimeSeriesWriters;
            }
        }

        /// <summary>
        /// Gets the initial start date time from the run info 
        /// </summary>
        public DateTime StartDateTime
        {
            get { return Java2DotNetHelper.DotNetDateTimeFromJavaMillies(reader.getStartDateTime().getTime()); }
        }

        /// <summary>
        /// Gets the end date time from the run info 
        /// </summary>
        public DateTime EndDateTime
        {
            get { return Java2DotNetHelper.DotNetDateTimeFromJavaMillies(reader.getEndDateTime().getTime()); }
        }

        /// <summary>
        /// The output time step in millies, if specified in pi-run file (-1 otherwise)
        /// </summary>
        public int OutputTimeStepInJavaMillies
        {
            get { return -1; } // TODO 
        }

        /// <summary>
        /// Gets the output diagnostics file
        /// </summary>
        public string OutputDiagnosticsFile
        {
            get { return ResolveTestFolderPlaceholder(reader.getOutputDiagnosticFile().getPath()); }
        }

        /// <summary>
        /// Gets the name of the model to run in the DeltaShell project
        /// </summary>
        public string ModelName
        {
            get { return GetKeyValue(ModelKey); }
        }

        /// <summary>
        /// Gets the DeltaShell project file
        /// </summary>
        public string ProjectFile
        {
            get
            {      
                string projectFile = GetKeyValue(DeltaShellProjectFileKey);
                if (!string.IsNullOrEmpty(projectFile))
                {
                    return ResolveTestFolderPlaceholder(projectFile);
                }
                return projectFile;
            }
        }


        /// <summary>
        /// Gets the working directory for a model
        /// </summary>
        public string GetModelWorkingDirectory(string modelId)
        {
            foreach (string key in reader.getProperties().getKeys())
            {
                if (key.StartsWith(ModelWorkingDirectoryKey))
                {
                    string modelIdInKey = key.Substring(ModelWorkingDirectoryKey.Length);
                    if (modelIdInKey.ToLower().Equals(modelId.ToLower()))
                    {
                        return ResolveTestFolderPlaceholder(reader.getProperties().getString(key));
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Boolean indicating if the aggregation options for the output variables
        /// should be used as set in the DsProj file, or should be determined
        /// from the headers in the output time series (the latter is default).
        /// </summary>
        public bool UseAggrationOptionsAsSetInDsProj
        {
            get { return false; }  // TODO
        }

        /// <summary>
        /// Boolean indicating that non-fews-adapter debug messages must be logged
        /// </summary>
        public bool DoLogNonFewsDebugMessages
        {
            get
            {
                bool parsedBool;
                if (!bool.TryParse(GetKeyValue(DoLogNonFewsDebugMessagesKey), out parsedBool))
                {
                    return false;
                }
                return parsedBool;
            }
        }

        /// <summary>
        /// Gets the DeltaShell project file
        /// </summary>
        public string SavedProjectFile
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(ProjectFile), "CopyOf" + Path.GetFileName(ProjectFile));
            }
        }

        /// <summary>
        /// Gets a boolean value indicating that the time series data is in binary form
        /// </summary>
        public bool PiTimeSeriesAsBin
        {
            get
            {
                bool parsedBool;
                if (!bool.TryParse(GetKeyValue(PiTimeSeriesAsBinKey), out parsedBool))
                {
                    return false;
                }
                return parsedBool;
            }
        }

        /// <summary>
        /// Gets a boolean value indicating that the project should be saved after running
        /// </summary>
        public bool SaveResultsInCopyOfProject
        {
            get
            {
                bool parsedBool;
                if (!bool.TryParse(GetKeyValue(SaveResultsInCopyOfProjectKey), out parsedBool))
                {
                    return false;
                }
                return parsedBool;
            }
        }

        public bool Use1DPointsFileForRestart
        {
            get
            {
                bool parsedBool;
                if (!bool.TryParse(GetKeyValue(Use1DPointsFileForRestartKey), out parsedBool))
                {
                    return false;
                }
                return parsedBool;
            }
        }

        public bool ExportStaggeredGridPoints
        {
            get
            {
                bool parsedBool;
                if (!bool.TryParse(GetKeyValue(ExportStaggeredGridPointsKey), out parsedBool))
                {
                    return false;
                }
                return parsedBool;
            }
        }

        public string WorkingDirectory
        {
            get { return ResolveTestFolderPlaceholder(reader.getWorkDir().getPath()); }
        }

        public bool DoCheckBackwardsCompatibility
        {
            get
            {
                bool parsedBool;
                if (!bool.TryParse(GetKeyValue(DoCheckBackwardsCompatibilityKey), out parsedBool))
                {
                    return false;
                }
                return parsedBool;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Properties()
        {
            IDictionary<string, string > properties = new Dictionary<string, string>();
            foreach (var key in reader.getProperties().getKeys())
            {
                properties.Add(new KeyValuePair<string, string>(key, GetKeyValue(key)));
            }
            return properties;
        }

        private IEnumerable<string> GetFiles(List files)
        {
            for (int i = 0; i < files.size(); i++)
            {
                var file = (java.io.File) files.get(i);
                file = file.ResolveTestDirectory(GetTestDirectoryPath(piRunFilePath));
                yield return file.getAbsolutePath();
            }
        }

        private string ResolveTestFolderPlaceholder(string input)
        {
            if (input.Contains(TestFolderPlaceHolderId))
            {
                // Unit test. Replace the test string by the full path
                string testDirectoryPath = GetTestDirectoryPath(piRunFilePath);
                return input.Replace(TestFolderPlaceHolderId, testDirectoryPath);
            }
            return input;
        }

        private static string GetTestDirectoryPath(string piRunFilePath)
        {
            string inputFolder = Path.GetDirectoryName(piRunFilePath);
            if (!Path.GetFileName(inputFolder).ToLower().Equals("input"))
            {
                throw new Exception("invalid test setup");
            }
            return Path.GetDirectoryName(inputFolder);
        }

        private string GetKeyValue(string key)
        {
            foreach (string propertyKey in reader.getProperties().getKeys())
            {
                if(propertyKey.ToLower().Equals(key.ToLower()))
                {
                    return reader.getProperties().getString(propertyKey);
                }
            }
            return string.Empty;
        }
    }
}

