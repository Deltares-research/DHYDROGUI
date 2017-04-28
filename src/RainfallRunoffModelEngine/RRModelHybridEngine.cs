using System;
using System.IO;
using System.Text;

namespace RainfallRunoffModelEngine
{
    /// <summary>
    /// ModelEngine the RR model talks to. Wraps the Sobek RR rekenhart, communication happens through 
    /// both Sobek files and the OpenMI api (hence Hybrid).
    /// </summary>
    public class RRModelHybridEngine : IRRModelApi
    {

        public RRModelHybridEngine():this(null)
        {
        }

        public RRModelHybridEngine(IRRModelEngineDll engineDll = null)
        {
            if (engineDll == null)
            {
                engineDll = new RRModelEngineDll();
            }
            this.engineDll = engineDll;
        }

        private IRRModelEngineDll engineDll;
        
        private const string ComponentId = "RR";
        private const string SchematizationId = "Sobek_3b.fnm";
        
        public bool ModelInitialize()
        {
            //if this call causes the unit test to abort: look in the WorkingDirectory for sobek_3b.log (or Sobek_3bInit.log)
            var ret = engineDll.ModelInitialize(ComponentId, SchematizationId);
            ThrowOnSobekError("ModelInitialize", ret, true);
            return true;// success >= 0;
        }

        public int ModelGetCurrentTime()
        {
            throw new NotImplementedException();
        }

        public bool ModelPerformTimeStep()
        {
            var retVal = engineDll.ModelPerformTimeStep(ComponentId, SchematizationId);
            ThrowOnSobekError("ModelPerformTimeStep", retVal);
            return retVal >= 0;
        }

        public bool ModelFinalize()
        {
            //cleanup files?
            var retVal = engineDll.ModelFinalize(ComponentId, SchematizationId);
            ThrowOnSobekError("ModelFinalize", retVal);
            return retVal >= 0;
        }

        public void GetValues(QuantityType quantity, ElementSet elmSet, ref double[] values, int location)
        {
            int elementSet = (int)elmSet;
            int quant = (int)quantity;
            int numValues = values.Length;
            var retVal = engineDll.GetValuesByIntId(ComponentId, SchematizationId, ref quant, ref elementSet, ref numValues, values);
            ThrowOnSobekError("GetValues", retVal);
            //do more
        }

        public void GetValues(QuantityType quantity, ElementSet elmSet, ref double[] values)
        {
            GetValues(quantity, elmSet, ref values, -1);
        }

        public double[] GetValues(QuantityType quantity, ElementSet elmSet)
        {
            var pointsCount = GetSize(elmSet);
            if (pointsCount < 0)
                throw new Exception("GetSize of elementset " + elmSet + " returned " + pointsCount);
            if (pointsCount == 0)
                return new double[0];

            var values = new double[pointsCount];
            GetValues(quantity, elmSet, ref values);
            LogMessages();
            return values;
        }

        public double GetValue(QuantityType quantity, ElementSet elmSet, int location)
        {
            var values = new double[1];
            GetValues(quantity, elmSet, ref values, location);
            return values[0];
        }
        
        public int GetSize(ElementSet elmSet)
        {
            return engineDll.GetSize(ComponentId, SchematizationId,
                                                         RRModelEngineHelper.ElementSetToString(elmSet));
        }

        public bool SetValues(QuantityType quantity, ElementSet elmSet, double[] values)
        {
            var err = engineDll.SetValues(ComponentId, SchematizationId,
                                           RRModelEngineHelper.QuantityTypeToString(quantity),
                                           RRModelEngineHelper.ElementSetToString(elmSet), values.Length, values);
            ThrowOnSobekError("SetValues", err);
            return err >= 0;
        }

        public bool SetValue(QuantityType quantity, ElementSet elmSet, double value, int location)
        {
            //RRModelEngineDll.OES_SetValues()
            throw new NotImplementedException();
        }

        private void ThrowOnSobekError(string method, int sobekError, bool showLog=false)
        {
            if (sobekError < 0)
            {
                var exceptionMessage = String.Format("RR DLL, method {0}, error {1}: {2}", method, sobekError, GetError(sobekError));

                if (showLog)
                {
                    const string rrInitLog = "Sobek_3bInit.log";
                    if (File.Exists(rrInitLog))
                    {
                        exceptionMessage += "\nLog:\n" + File.ReadAllText(rrInitLog);
                    }

                    const string rrLog = "sobek_3b.log";
                    if (File.Exists(rrLog))
                    {
                        exceptionMessage += "\nLog:\n" + File.ReadAllText(rrLog);
                    }
                }

                throw new ArgumentException(exceptionMessage);
            }
        }

        public void LogMessages()
        {
        }

        public int MessageCount()
        {
            return 0;
        }

        public string GetError(int errorId)
        {
            var localErrorId = errorId;
            var errorDescription = new StringBuilder(512) {Length = 512};
            engineDll.GetError(ref localErrorId, errorDescription);
            var error = errorDescription.ToString();
            var indexOfEnd = error.IndexOf("   ");
            return indexOfEnd >= 0 ? error.Substring(0, indexOfEnd) : error;
        }

        public string GetMessage(int messageId, out ErrorLevel errorLevel)
        {
            throw new NotImplementedException();
        }
    }
}
