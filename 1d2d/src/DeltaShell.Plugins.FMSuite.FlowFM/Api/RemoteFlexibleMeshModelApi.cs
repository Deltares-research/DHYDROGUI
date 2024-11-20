using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BasicModelInterface;
using DelftTools.Utils.Remoting;
using DeltaShell.Dimr;
using DeltaShell.Plugins.FMSuite.Common.IO;
using ProtoBufRemote;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    public class RemoteFlexibleMeshModelApi : IFlexibleMeshModelApi
    {
        private bool disposed;
        private IFlexibleMeshModelApi remoteInstanceApi;

        public RemoteFlexibleMeshModelApi(bool showDebugConsole = false)
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
            // is not bit dependent, eg IntPtr and the like.
            if (!RemotingTypeConverters.RegisteredConverters.OfType<LoggerToProtoConverter>().Any())
            {
                lock (RemotingTypeConverters.RegisteredConverters)
                {
                    RemotingTypeConverters.RegisterTypeConverter(new LoggerToProtoConverter());
                }
            }
            remoteInstanceApi = RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>(showConsole: showDebugConsole);
            // for non-remote use: remoteInstanceApi = new FlexibleMeshModelApi();
        }

        private string WorkingDirectory { get; set; }

        public int Initialize(string path)
        {
            WorkingDirectory = Path.GetDirectoryName(path);
            try
            {
                return remoteInstanceApi.Initialize(path);
            }
            catch (Exception e)
            {
                TryThrowWithKernelLoggedErrors(e, WorkingDirectory);
                throw;
            }
        }

        public int Update(double dt = -1)
        {
            try
            {
                return remoteInstanceApi.Update(dt);
            }
            catch (Exception e)
            {
                TryThrowWithKernelLoggedErrors(e, WorkingDirectory);
                throw;
            }
        }

        public int Finish()
        {
            try
            {
                return remoteInstanceApi.Finish();
            }
            catch (Exception e)
            {
                TryThrowWithKernelLoggedErrors(e, WorkingDirectory);
                throw;
            }
        }

        public int[] GetShape(string variable)
        {
            return remoteInstanceApi.GetShape(variable);
        }

        public Array GetValues(string variable)
        {
            return remoteInstanceApi.GetValues(variable);
        }

        public Array GetValues(string variable, int[] index)
        {
            return remoteInstanceApi.GetValues(variable, index);
        }

        public Array GetValues(string variable, int[] start, int[] count)
        {
            return remoteInstanceApi.GetValues(variable, start, count);
        }

        public void SetValues(string variable, Array values)
        {
            var doubles = values as double[];
            if (doubles != null)
            {
                remoteInstanceApi.SetValuesDouble(variable, doubles);
                return;
            }
            var ints = values as int[];
            if (ints != null)
            {
                remoteInstanceApi.SetValuesInt(variable, ints);
                return;
            }
            remoteInstanceApi.SetValues(variable, values);
        }

        public void SetValues(string variable, int[] start, int[] count, Array values)
        {
            var doubles = values as double[];
            if (doubles != null)
            {
                remoteInstanceApi.SetValuesDouble(variable, start, count, doubles);
                return;
            }
            var ints = values as int[];
            if (ints != null)
            {
                remoteInstanceApi.SetValuesInt(variable, start, count, ints);
                return;
            }
            remoteInstanceApi.SetValues(variable, start, count, values);
        }

        public void SetValues(string variable, int[] index, Array values)
        {
            var doubles = values as double[];
            if (doubles != null)
            {
                remoteInstanceApi.SetValuesDouble(variable, index, doubles);
                return;
            }
            var ints = values as int[];
            if (ints != null)
            {
                remoteInstanceApi.SetValuesInt(variable, index, ints);
                return;
            }
            remoteInstanceApi.SetValues(variable, index, values);
        }

        private static void TryThrowWithKernelLoggedErrors(Exception innerException, string runDirectory)
        {
            var diaFiles = Directory.GetFiles(runDirectory, "*.dia");

            if (diaFiles.Length <= 0)
            {
                throw new FileNotFoundException("Could not detect diagnostics file in " + runDirectory);
            }

            List<string> errorMessages;
            var diaFile = diaFiles[0];

            try
            {
                errorMessages = DiaFileReader.CollectAllErrorMessages(diaFile);

                errorMessages.AddRange(File.ReadAllLines(diaFile).Where( line => line.Contains("FATAL")));
            }
            catch (Exception e)
            {
                throw new FileFormatException(string.Format("Unable to read diagnostics file {0}: {1}", diaFile,
                    e.Message));
            }

            if (!errorMessages.Any())
            {
                throw new InvalidOperationException(string.Format(
                    "No errors were reported in the diagnostics file {0}", diaFile));
            }

            throw new InvalidOperationException(string.Format(
                "The kernel reported the following error(s):{0}{1}{0}(Errors extracted from diagnostics file {2})",
                Environment.NewLine, string.Join(Environment.NewLine, errorMessages), diaFile), innerException);
        }

        public string GetVersionString()
        {
            return remoteInstanceApi.GetVersionString();
        }

        public bool InitializeUserTimeStep(double targetTimeRel)
        {
            try
            {
                return remoteInstanceApi.InitializeUserTimeStep(targetTimeRel);
            }
            catch (Exception e)
            {
                TryThrowWithKernelLoggedErrors(e, WorkingDirectory);
                throw;
            }
        }

        public bool FinalizeUserTimeStep()
        {
            try
            {
                return remoteInstanceApi.FinalizeUserTimeStep();
            }
            catch (Exception e)
            {
                TryThrowWithKernelLoggedErrors(e, WorkingDirectory);
                throw;
            }
        }

        public double InitializeComputationalTimeStep(double targetTimeRel, double timeStep)
        {
            try
            {
                return remoteInstanceApi.InitializeComputationalTimeStep(targetTimeRel, timeStep);
            }
            catch (Exception e)
            {
                TryThrowWithKernelLoggedErrors(e, WorkingDirectory);
                throw;
            }
        }

        public double RunComputationalTimeStep(double timeStep)
        {
            try
            {
                return remoteInstanceApi.RunComputationalTimeStep(timeStep);
            }
            catch (Exception e)
            {
                TryThrowWithKernelLoggedErrors(e, WorkingDirectory);
                throw;
            }
        }

        public bool FinalizeComputationalTimeStep()
        {
            try
            {
                return remoteInstanceApi.FinalizeComputationalTimeStep();
            }
            catch (Exception e)
            {
                TryThrowWithKernelLoggedErrors(e, WorkingDirectory);
                throw;
            }
        }

        public void Compute1d2dCoefficients()
        {
            try
            {
                remoteInstanceApi.Compute1d2dCoefficients();
            }
            catch (Exception e)
            {
                TryThrowWithKernelLoggedErrors(e, WorkingDirectory);
                throw;
            }
        }

        public Type GetVariableType(string variable)
        {
            return remoteInstanceApi.GetVariableType(variable);
        }

        public string GetVariableLocation(string variable)
        {
            return remoteInstanceApi.GetVariableLocation(variable);
        }

        public DateTime StartTime
        {
            get { return remoteInstanceApi.StartTime; }
        }

        public DateTime StopTime
        {
            get { return remoteInstanceApi.StopTime; }
        }

        public DateTime CurrentTime
        {
            get { return remoteInstanceApi.CurrentTime; }
        }

        public TimeSpan TimeStep
        {
            get { return remoteInstanceApi.TimeStep; }
        }

        public string[] VariableNames
        {
            get { return remoteInstanceApi.VariableNames; }
        }

        public Logger Logger
        {
            get { return remoteInstanceApi.Logger; }
            set { remoteInstanceApi.Logger = value; }
        }

        public bool GetSnappedFeature(string featureType, double[] xin, double[] yin, ref double[] xout, ref double[] yout,
            ref int[] featureIds)
        {
            return remoteInstanceApi.GetSnappedFeature(featureType, xin, yin, ref xout, ref yout, ref featureIds);
        }

        public void SetValuesDouble(string variable, double[] values)
        {
            remoteInstanceApi.SetValuesDouble(variable, values);
        }

        public void SetValuesDouble(string variable, int[] start, int[] count, double[] values)
        {
            remoteInstanceApi.SetValuesDouble(variable, start, count, values);
        }

        public void SetValuesDouble(string variable, int[] index, double[] values)
        {
            remoteInstanceApi.SetValuesDouble(variable, index, values);
        }

        public void SetValuesInt(string variable, int[] values)
        {
            remoteInstanceApi.SetValuesInt(variable, values);
        }

        public void SetValuesInt(string variable, int[] start, int[] count, int[] values)
        {
            remoteInstanceApi.SetValuesInt(variable, start, count, values);
        }

        public void SetValuesInt(string variable, int[] index, int[] values)
        {
            remoteInstanceApi.SetValuesInt(variable, index, values);
        }

        public double GetValue(string featureCategory, string featureName, string parameterName)
        {
            return remoteInstanceApi.GetValue(featureCategory, featureName, parameterName);
        }

        public void SetValue(string featureCategory, string featureName, string parameterName, double value)
        {
            remoteInstanceApi.SetValue(featureCategory, featureName, parameterName, value);
        }

        public void WriteNetGeometry(string fileName)
        {
            remoteInstanceApi.WriteNetGeometry(fileName);
        }

        public void WritePartitioning(string inputFileName, string outputFileName, string polFileName)
        {
            remoteInstanceApi.WritePartitioning(inputFileName, outputFileName, polFileName);
        }

        public void WritePartitioning(string inputFileName, string outputFileName, int numDomains, bool contiguous)
        {
            remoteInstanceApi.WritePartitioning(inputFileName, outputFileName, numDomains, contiguous);
        }

        ~RemoteFlexibleMeshModelApi()
        {
            // in case someone forgets to dispose..
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // Must always ensure this happens to prevent GC deadlock on project close!
            GC.SuppressFinalize(this);
            
        }

        private void DisposeInternal()
        {
            if (remoteInstanceApi != null)
                RemoteInstanceContainer.RemoveInstance(remoteInstanceApi);
            remoteInstanceApi = null;
            disposed = true;
            Thread.Sleep(100); // wait for process to truly exit
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    try
                    {
                        DisposeInternal();
                    }
                    catch (Exception e)
                    {
                        TryThrowWithKernelLoggedErrors(e, WorkingDirectory);
                        throw;
                    }
                }
                disposed = true;
            }
        }
    }
}