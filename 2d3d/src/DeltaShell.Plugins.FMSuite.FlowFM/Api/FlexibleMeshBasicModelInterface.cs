using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BasicModelInterface;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    public class FlexibleMeshBasicModelInterface : IBasicModelInterface
    {
        private string originalCurrentDirectory;

        public DateTime StartTime
        {
            get
            {
                double startTime;
                FlexibleMeshModelDll.get_start_time(out startTime);
                return GetRefDate().AddSeconds(startTime);
            }
        }

        public DateTime StopTime
        {
            get
            {
                double stopTime;
                FlexibleMeshModelDll.get_end_time(out stopTime);
                return GetRefDate().AddSeconds(stopTime);
            }
        }

        public DateTime CurrentTime
        {
            get
            {
                double t;
                FlexibleMeshModelDll.get_current_time(out t);
                return GetRefDate().AddSeconds(t);
            }
        }

        public TimeSpan TimeStep
        {
            get
            {
                double dt;
                FlexibleMeshModelDll.get_time_step(out dt);
                return new TimeSpan((long)(TimeSpan.TicksPerSecond * dt));
            }
        }

        public string[] VariableNames
        {
            get
            {
                int count = GetVariableCount();
                if (count == 0)
                {
                    return new string[0];
                }

                var names = new string[count];
                for (var i = 0; i < count; i++)
                {
                    names[i] = GetVariableName(i);
                }

                return names;
            }
        }

        public Logger Logger { get; set; }

        public string GetVariableLocation(string variableName)
        {
            var variableLocationBuilder = new StringBuilder(FlexibleMeshModelDll.MAXSTRLEN);
            FlexibleMeshModelDll.get_var_location(variableName, variableLocationBuilder);
            return variableLocationBuilder.ToString();
        }

        public int Initialize(string path)
        {
            originalCurrentDirectory = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(path) ?? ".");
            return FlexibleMeshModelDll.initialize(Path.GetFileName(path));
        }

        public int Update(double dt = -1)
        {
            double totalSeconds = TimeStep.TotalSeconds;
            return FlexibleMeshModelDll.update(totalSeconds);
        }

        public int Finish()
        {
            Directory.SetCurrentDirectory(originalCurrentDirectory);
            return 0;
        }

        public int[] GetShape(string variable)
        {
            // get rank
            FlexibleMeshModelDll.get_var_rank(variable, out int rank);

            // get shape
            var shape = new int[FlexibleMeshModelDll.MAXDIMS];
            FlexibleMeshModelDll.get_var_shape(variable, shape);
            return shape.Take(rank).ToArray();
        }

        public Array GetValues(string variable)
        {
            IntPtr ptr = IntPtr.Zero;
            FlexibleMeshModelDll.get_var(variable, ref ptr);

            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            // get rank
            FlexibleMeshModelDll.get_var_rank(variable, out int rank);

            // get shape
            var shape = new int[FlexibleMeshModelDll.MAXDIMS];
            FlexibleMeshModelDll.get_var_shape(variable, shape);
            shape = shape.Take(rank).ToArray();

            // get value type
            var typeNameBuilder = new StringBuilder(FlexibleMeshModelDll.MAXSTRLEN);
            FlexibleMeshModelDll.get_var_type(variable, typeNameBuilder);
            var typeName = typeNameBuilder.ToString();

            // copy to 1D array
            int totalLength = GetTotalLength(shape);

            Array values1D = ToArray1D(ptr, typeName, totalLength);

            if (rank == 1)
            {
                return values1D;
            }

            // convert to nD array
            Type valueType = ToType(typeName);
            var values = Array.CreateInstance(valueType, shape);
            Buffer.BlockCopy(values1D, 0, values, 0, values1D.Length * Marshal.SizeOf(valueType));

            return values;
        }

        public Array GetValues(string variable, int[] index)
        {
            throw new NotImplementedException();
        }

        public Array GetValues(string variable, int[] start, int[] count)
        {
            throw new NotImplementedException();
        }

        public void SetValues(string variable, Array values)
        {
            if (values is double[] valuesDouble1D)
            {
                FlexibleMeshModelDll.set_var(variable, valuesDouble1D);
            }
            else
            {
                if (values == null)
                {
                    throw new NotImplementedException("Could not cast null to array");
                }
                else
                {
                    throw new NotImplementedException("Could not cast values to array: " + values.GetType());
                }
            }
        }

        public void SetValues(string variable, int[] start, int[] count, Array values)
        {
            throw new NotImplementedException();
        }

        public void SetValues(string variable, int[] index, Array values)
        {
            throw new NotImplementedException();
        }

        private static DateTime GetRefDate()
        {
            var sb = new StringBuilder(FlexibleMeshModelDll.MAXSTRLEN);
            FlexibleMeshModelDll.get_string_attribute("refdat", sb);
            DateTime refDate = DateTime.ParseExact(sb.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            return refDate;
        }

        private int GetVariableCount()
        {
            var count = 0;
            FlexibleMeshModelDll.get_var_count(ref count);
            return count;
        }

        private string GetVariableName(int index)
        {
            var variableNameBuilder = new StringBuilder(FlexibleMeshModelDll.MAXSTRLEN);
            FlexibleMeshModelDll.get_var_name(index, variableNameBuilder);
            return variableNameBuilder.ToString();
        }

        private static Type ToType(string typeName)
        {
            switch (typeName)
            {
                case "double":
                    return typeof(double);

                case "int":
                    return typeof(int);

                case "float":
                    return typeof(float);
            }

            throw new NotSupportedException("Unsupported type: " + typeName);
        }

        private static Array ToArray1D(IntPtr ptr, string valueType, int totalLength)
        {
            if (valueType == "double")
            {
                var values = new double[totalLength];
                Marshal.Copy(ptr, values, 0, totalLength);
                return values;
            }

            if (valueType == "int")
            {
                var values = new int[totalLength];
                Marshal.Copy(ptr, values, 0, totalLength);
                return values;
            }

            if (valueType == "float")
            {
                var values = new float[totalLength];
                Marshal.Copy(ptr, values, 0, totalLength);
                return values;
            }

            throw new NotSupportedException("Unsupported type: " + valueType);
        }

        private static int GetTotalLength(int[] shape)
        {
            return shape.Aggregate(1, (current, t) => current * t);
        }
    }
}