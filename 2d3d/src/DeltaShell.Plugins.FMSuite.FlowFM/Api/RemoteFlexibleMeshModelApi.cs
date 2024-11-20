using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BasicModelInterface;
using DelftTools.Utils.Remoting;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    public class RemoteFlexibleMeshModelApi : IFlexibleMeshModelApi
    {
        private bool disposed;
        private IFlexibleMeshModelApi remoteInstanceApi;

        public RemoteFlexibleMeshModelApi(IFlexibleMeshModelApi api)
        {
            remoteInstanceApi = api;
        }

        public DateTime StartTime => remoteInstanceApi.StartTime;

        public DateTime StopTime => remoteInstanceApi.StopTime;

        public DateTime CurrentTime => remoteInstanceApi.CurrentTime;

        public TimeSpan TimeStep => remoteInstanceApi.TimeStep;

        public string[] VariableNames => remoteInstanceApi.VariableNames;

        public Logger Logger
        {
            get => remoteInstanceApi.Logger;
            set => remoteInstanceApi.Logger = value;
        }

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

        public double InitializeComputationalTimeStep(double targetTimeRel, double dt)
        {
            try
            {
                return remoteInstanceApi.InitializeComputationalTimeStep(targetTimeRel, dt);
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

        public Type GetVariableType(string variable)
        {
            return remoteInstanceApi.GetVariableType(variable);
        }

        public string GetVariableLocation(string variable)
        {
            return remoteInstanceApi.GetVariableLocation(variable);
        }

        public bool GetSnappedFeature(string featureType, double[] xin, double[] yin, ref double[] xout,
                                      ref double[] yout,
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

        public void WriteNetGeometry(string filePath)
        {
            remoteInstanceApi.WriteNetGeometry(filePath);
        }

        public void WritePartitioning(string inputFileName, string outputFileName, string polFileName)
        {
            remoteInstanceApi.WritePartitioning(inputFileName, outputFileName, polFileName);
        }

        public void WritePartitioning(string inputFileName, string outputFileName, int numDomains, bool contiguous)
        {
            remoteInstanceApi.WritePartitioning(inputFileName, outputFileName, numDomains, contiguous);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private string WorkingDirectory { get; set; }

        private static void TryThrowWithKernelLoggedErrors(Exception innerException, string runDirectory)
        {
            string[] diaFiles = Directory.GetFiles(runDirectory, "*.dia", SearchOption.AllDirectories);

            if (diaFiles.Length <= 0)
            {
                throw new FileNotFoundException("Could not detect diagnostics file in " + runDirectory);
            }

            IEnumerable<string> errorMessages;
            string diaFilePath = diaFiles[0];

            try
            {
                using (var reader = new StreamReader(diaFilePath))
                {
                    Dictionary<DiaFileLogSeverity, IList<string>> messagesDictionary = DiaFileReader.GetAllMessages(reader);
                    errorMessages = messagesDictionary[DiaFileLogSeverity.Error].Concat(messagesDictionary[DiaFileLogSeverity.Fatal])
                                                                                .ToArray();
                }
            }
            catch (Exception e)
            {
                throw new FileFormatException(string.Format(Resources.RemoteFlexibleMeshModelApi_Unable_to_read_diagnostics_file__0____1_, diaFilePath,
                                                            e.Message));
            }

            if (!errorMessages.Any())
            {
                throw new InvalidOperationException(string.Format(
                                                        Resources.RemoteFlexibleMeshModelApi_No_errors_were_reported_in_the_diagnostics_file__0_,
                                                        diaFilePath));
            }

            throw new InvalidOperationException(string.Format(
                                                    Resources.RemoteFlexibleMeshModelApi_The_kernel_reported_the_following_error_s___0__1__0__Errors_extracted_from_diagnostics_file__2__,
                                                    Environment.NewLine,
                                                    string.Join(Environment.NewLine, errorMessages), diaFilePath),
                                                innerException);
        }

        private void DisposeInternal()
        {
            if (remoteInstanceApi != null)
            {
                RemoteInstanceContainer.RemoveInstance(remoteInstanceApi);
            }

            remoteInstanceApi = null;
            disposed = true;
            Thread.Sleep(100); // wait for process to truly exit
        }

        protected virtual void Dispose(bool disposing)
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

        ~RemoteFlexibleMeshModelApi()
        {
            Dispose(false);
        }
    }
}