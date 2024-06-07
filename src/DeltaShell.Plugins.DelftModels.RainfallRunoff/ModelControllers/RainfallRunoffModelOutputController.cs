using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers
{
    public class RainfallRunoffModelOutputController
    {
        private IDictionary<string, ITimeBasedFunction> functionLookUp;
        private RainfallRunoffModel model;
        
        public void Initialize(RainfallRunoffModel pModel, Action<EngineParameter, ITimeBasedFunction> fillFeatureCoveragesWithFeatures)
        {
            model = pModel;
            functionLookUp = model.OutputFunctions.ToDictionary(c => c.Components[0].Name, CreateWrapper);
            
            // fill features in feature coverages
            ForEachEngineParameter(fillFeatureCoveragesWithFeatures);
        }
        
        public void Cleanup()
        {
            functionLookUp = null;
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