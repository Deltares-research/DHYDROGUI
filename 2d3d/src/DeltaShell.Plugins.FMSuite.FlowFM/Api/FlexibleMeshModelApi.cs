using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BasicModelInterface;
using DelftTools.Utils.Interop;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    /// <summary>
    /// Do not use directly (mixing x86 and x64 dlls not possible, and unstruct.dll = 64bit).
    /// Instead use RemoteFlexibleMeshModelApi (and don't forget to dispose)
    /// </summary>
    public class FlexibleMeshModelApi : IFlexibleMeshModelApi
    {
        private readonly FlexibleMeshBasicModelInterface flexibleMeshBasicModelInterface; //BMI strategy

        public FlexibleMeshModelApi()
        {
            flexibleMeshBasicModelInterface = new FlexibleMeshBasicModelInterface();
        }

        public DateTime StartTime => flexibleMeshBasicModelInterface.StartTime;

        public DateTime StopTime => flexibleMeshBasicModelInterface.StopTime;

        public DateTime CurrentTime => flexibleMeshBasicModelInterface.CurrentTime;

        public TimeSpan TimeStep => flexibleMeshBasicModelInterface.TimeStep;

        public string[] VariableNames => flexibleMeshBasicModelInterface.VariableNames;

        public Logger Logger { get; set; }

        public string GetVersionString()
        {
            var versionAsStringBuilder = new StringBuilder("tst".PadRight(FlexibleMeshModelDll.MAXSTRLEN));
            FlexibleMeshModelDll.get_version_string(versionAsStringBuilder);
            return versionAsStringBuilder.ToString();
        }

        public int Initialize(string path)
        {
            return flexibleMeshBasicModelInterface.Initialize(path);
        }

        public int Update(double dt = -1)
        {
            return flexibleMeshBasicModelInterface.Update(dt);
        }

        public int Finish()
        {
            FlexibleMeshModelDll.finalize();
            return flexibleMeshBasicModelInterface.Finish();
        }

        public int[] GetShape(string variable)
        {
            return flexibleMeshBasicModelInterface.GetShape(variable);
        }

        public Array GetValues(string variable)
        {
            return flexibleMeshBasicModelInterface.GetValues(variable);
        }

        public Array GetValues(string variable, int[] index)
        {
            return flexibleMeshBasicModelInterface.GetValues(variable, index);
        }

        public Array GetValues(string variable, int[] start, int[] count)
        {
            return flexibleMeshBasicModelInterface.GetValues(variable, start, count);
        }

        public void SetValues(string variable, Array values)
        {
            flexibleMeshBasicModelInterface.SetValues(variable, values);
        }

        public void SetValues(string variable, int[] start, int[] count, Array values)
        {
            flexibleMeshBasicModelInterface.SetValues(variable, start, count, values);
        }

        public void SetValues(string variable, int[] index, Array values)
        {
            flexibleMeshBasicModelInterface.SetValues(variable, index, values);
        }

        public bool InitializeUserTimeStep(double targetTimeRel)
        {
            int result = FlexibleMeshModelDll.dfm_init_user_timestep(ref targetTimeRel);
            if (result != 0 && result != 88)
            {
                throw new Exception("Flow FM initialize user time step failed with error code " + result);
            }

            return true;
        }

        public bool FinalizeUserTimeStep()
        {
            int result = FlexibleMeshModelDll.dfm_finalize_user_timestep();
            if (result == 0)
            {
                return true;
            }

            throw new Exception("Flow FM finalize user time step failed with error code " + result);
        }

        /// <summary>
        /// Initializes the computational time step.
        /// </summary>
        /// <param name="targetTimeRel">The relative target time.</param>
        /// <param name="dt">The requested time step.</param>
        /// <returns>
        /// The initialized time step.
        /// </returns>
        /// <exception cref="Exception">Flow FM initialize computational time step failed with error code " + result</exception>
        public double InitializeComputationalTimeStep(double targetTimeRel, double dt)
        {
            double timeStep = dt;
            int result = FlexibleMeshModelDll.dfm_init_computational_timestep(ref targetTimeRel, ref timeStep);

            if (result != 0 && result != 88)
            {
                throw new Exception("Flow FM initialize computational time step failed with error code " + result);
            }

            return timeStep;
        }

        public double RunComputationalTimeStep(double timeStep)
        {
            double newTimeStep = timeStep;
            int result = FlexibleMeshModelDll.dfm_run_computational_timestep(ref newTimeStep);

            // Keep dflowfm (1d2d) computation going, when dflowfm core returns warning code 31 (time setback) 
            if (result != 0 && result != 31)
            {
                throw new Exception("Flow FM perform computational time step failed with error code " + result);
            }

            return newTimeStep;
        }

        public bool FinalizeComputationalTimeStep()
        {
            int result = FlexibleMeshModelDll.dfm_finalize_computational_timestep();
            if (result == 0)
            {
                return true;
            }

            throw new Exception("Flow FM finalize computational time step failed with error " + result);
        }

        public Type GetVariableType(string variable)
        {
            var sb = new StringBuilder(FlexibleMeshModelDll.MAXSTRLEN);
            FlexibleMeshModelDll.get_var_type(variable, sb);

            switch (sb.ToString())
            {
                case "double":
                    return typeof(double);
                case "int":
                    return typeof(int);
                default:
                    throw new NotSupportedException(string.Format("Unsupported type from bmi api: {0}", sb));
            }
        }

        public string GetVariableLocation(string variable)
        {
            return flexibleMeshBasicModelInterface.GetVariableLocation(variable);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // nothing to do here
        }

        public virtual bool GetSnappedFeature(string featureType, double[] xin, double[] yin, ref double[] xout,
                                              ref double[] yout, ref int[] featureIds)
        {
            int numInputs = xin.Length;

            using (var pinnedXin = new Pinned(xin))
            using (var pinnedYin = new Pinned(yin))
            {
                IntPtr ptrXin = pinnedXin.IntPtr;
                IntPtr ptrYin = pinnedYin.IntPtr;
                var numOutputs = 0;
                var ptrXout = new IntPtr();
                var ptrYout = new IntPtr();
                var ptrFeatureIds = new IntPtr();
                int errorCode = -1;

                FlexibleMeshModelDll.get_snapped_feature(featureType, ref numInputs, ref ptrXin, ref ptrYin,
                                                         ref numOutputs, ref ptrXout, ref ptrYout, ref ptrFeatureIds,
                                                         ref errorCode);

                xout = new double[numOutputs];
                yout = new double[numOutputs];
                featureIds = new int[numOutputs];

                if (numOutputs > 0)
                {
                    Marshal.Copy(ptrXout, xout, 0, numOutputs);
                    Marshal.Copy(ptrYout, yout, 0, numOutputs);
                    Marshal.Copy(ptrFeatureIds, featureIds, 0, numOutputs);
                }

                return errorCode == 0;
            }
        }

        /// <summary>
        /// Calls get_compound_field in the api
        /// </summary>
        public double GetValue(string featureCategory, string featureName, string parameterName)
        {
            var inputArr = new double[1];
            inputArr[0] = double.NaN;

            var pointer = new IntPtr();
            FlexibleMeshModelDll.get_compound_field(featureCategory, featureName, parameterName, ref pointer);

            if (pointer != IntPtr.Zero)
            {
                Marshal.Copy(pointer, inputArr, 0, 1);
            }

            return inputArr[0];
        }

        /// <summary>
        /// Calls set_compound_field in the api
        /// </summary>
        public void SetValue(string featureCategory, string featureName, string parameterName, double value)
        {
            using (var p = new Pinned(value))
            {
                IntPtr pointer = p.IntPtr;
                FlexibleMeshModelDll.set_compound_field(featureCategory, featureName, parameterName, pointer);
            }
        }

        public void WriteNetGeometry(string filePath)
        {
            FlexibleMeshModelDll.write_net_geom(filePath);
        }

        public void WritePartitioning(string inputFileName, string outputFileName, string polFileName)
        {
            if (File.Exists(polFileName))
            {
                FlexibleMeshModelDll.write_partition_pol(inputFileName, outputFileName, polFileName);
            }
            else
            {
                throw new FileNotFoundException("File " + polFileName + " could not be found");
            }
        }

        public void WritePartitioning(string inputFileName, string outputFileName, int numDomains, bool contiguous)
        {
            if (numDomains > 1)
            {
                int contInt = contiguous ? 1 : 0;
                FlexibleMeshModelDll.write_partition_metis(inputFileName, outputFileName, ref numDomains, ref contInt);
            }
            else
            {
                throw new ArgumentException("Cannot partition network into " + numDomains + " domains");
            }
        }

        public void SetValuesDouble(string variable, double[] values)
        {
            flexibleMeshBasicModelInterface.SetValues(variable, values);
        }

        public void SetValuesDouble(string variable, int[] start, int[] count, double[] values)
        {
            flexibleMeshBasicModelInterface.SetValues(variable, start, count, values);
        }

        public void SetValuesDouble(string variable, int[] index, double[] values)
        {
            flexibleMeshBasicModelInterface.SetValues(variable, index, values);
        }

        public void SetValuesInt(string variable, int[] values)
        {
            flexibleMeshBasicModelInterface.SetValues(variable, values);
        }

        public void SetValuesInt(string variable, int[] start, int[] count, int[] values)
        {
            flexibleMeshBasicModelInterface.SetValues(variable, start, count, values);
        }

        public void SetValuesInt(string variable, int[] index, int[] values)
        {
            flexibleMeshBasicModelInterface.SetValues(variable, index, values);
        }
    }
}