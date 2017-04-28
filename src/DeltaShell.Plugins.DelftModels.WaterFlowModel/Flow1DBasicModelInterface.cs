using System;
using BasicModelInterface;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// TODO: We use this class only until the BMI package (i.e. the dynamic BasicModelInterfaceLibrary) is working properly...
    /// </summary>
    public class Flow1DBasicModelInterface : IBasicModelInterface
    {
        private readonly IModelApi modelApi;

        public Flow1DBasicModelInterface(IModelApi modelApi)
        {
            this.modelApi = modelApi;
        }

        public int Initialize(string path)
        {
            try
            {
                if (modelApi == null)
                {
                    Flow1DApiDll.initialize(path);
                }
                else
                {
                    modelApi.initialize(path);
                }
            }
            catch (Exception exception)
            {
                throw new Flow1DBasicModelInterfaceException("Could not initialize BMI Engine", exception);
            }
            return 0;
        }

        public int Update(double dt = -1)
        {
            throw new NotImplementedException();
        }

        public int Finish()
        {
            throw new NotImplementedException();
        }

        public int[] GetShape(string variable)
        {
            throw new NotImplementedException();
        }

        public Array GetValues(string variable)
        {
            Array array = new double[0];
            modelApi.get_var(variable, ref array);

            return array;
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
            var valuesDouble1D = values as double[];
            if (valuesDouble1D != null)
            {
                modelApi.set_var(variable, valuesDouble1D);
            }
            else
            {
                throw new NotImplementedException();
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

        public DateTime StartTime { get; private set; }
        public DateTime StopTime { get; private set; }
        public DateTime CurrentTime { get; private set; }
        public TimeSpan TimeStep { get; private set; }
        public string[] VariableNames { get; private set; }
        public Logger Logger { get; set; }
    }
    public class Flow1DBasicModelInterfaceException : Exception
    {
        public Flow1DBasicModelInterfaceException()
        {
        }

        public Flow1DBasicModelInterfaceException(string message)
            : base(message)
        {
        }

        public Flow1DBasicModelInterfaceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}