using System;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public interface IWaqProcessor
    {
        /// <summary>
        /// Run waq calculation process
        /// </summary>
        /// <param name="initializationSettings"> Settings needed to make the run </param>
        /// <param name="setProgress"> Method to set the progress </param>
        void Process(WaqInitializationSettings initializationSettings, Action<double> setProgress);

        /// <summary>
        /// Set to indicate that the processor should stop executing
        /// </summary>
        bool TryToCancel { get; set; }
    }
}