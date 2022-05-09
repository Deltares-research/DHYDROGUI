using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers
{
    public class RainfallRunoffModelOutputController
    {
        private IDictionary<string, ITimeBasedFunction> functionLookUp;
        private Queue<DateTime> outputWriteTimesQueue = new Queue<DateTime>();
        private RainfallRunoffModel model;
        
        public void Initialize(RainfallRunoffModel pModel, Action<EngineParameter, ITimeBasedFunction> fillFeatureCoveragesWithFeatures)
        {
            model = pModel;
            functionLookUp = model.OutputFunctions.ToDictionary(c => c.Components[0].Name, CreateWrapper);
            outputWriteTimesQueue = new Queue<DateTime>();
            GetTimesAtWhichToWriteOutput(model).ForEach(outputWriteTimesQueue.Enqueue);

            // fill features in feature coverages
            ForEachEngineParameter(fillFeatureCoveragesWithFeatures);
           
        }
        
        public void Cleanup()
        {
            functionLookUp = null;
        }

        private static IEnumerable<DateTime> GetTimesAtWhichToWriteOutput(RainfallRunoffModel model)
        {
            var times = new List<DateTime>();
            var stopTime = model.StopTime;
            var outputTimeStep = model.OutputTimeStep;

            var lastTime = model.StartTime;

            while (lastTime <= stopTime)
            {
                times.Add(lastTime);
                lastTime = lastTime.Add(outputTimeStep);
            }

            if (!times.Last().Equals(stopTime))
                times.Add(stopTime);

            return times;
        }
        

        private void ForEachEngineParameter(Action<EngineParameter, ITimeBasedFunction> action)
        {
            var engineParameters = model.OutputSettings.EngineParameters.Where(ep => ep.IsEnabled).ToList();

            foreach (var engineParameter in engineParameters)
            {
                if (!functionLookUp.ContainsKey(engineParameter.Name))
                {
                    //log.ErrorFormat("Output spatial data for {0} appears to be missing. Not implemented yet?", engineParameter.Name);
                }
                else
                {
                    var parameter = engineParameter;
                    action(parameter, functionLookUp[parameter.Name]);
                }
            }
        }

        private static ITimeBasedFunction CreateWrapper(IFunction function)
        {
            var coverage = function as IFeatureCoverage;
            if (coverage != null)
            {
                return new FeatureCoverageFiller(coverage);
            }
            return new TimeSeriesFiller((ITimeSeries)function);
        }
    }
}