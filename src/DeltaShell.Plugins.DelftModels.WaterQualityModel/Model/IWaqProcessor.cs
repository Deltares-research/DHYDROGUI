using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataItemMetaData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public interface IWaqProcessor
    {
        /// <summary>
        /// Initializes the processor
        /// </summary>
        /// <param name="initializationSettings"> Settings used for initialization </param>
        void Initialize(WaqInitializationSettings initializationSettings);

        /// <summary>
        /// Run waq calculation process
        /// </summary>
        /// <param name="initializationSettings"> Settings needed to make the run </param>
        /// <param name="setProgress"> Method to set the progress </param>
        void Process(WaqInitializationSettings initializationSettings, Action<double> setProgress);

        /// <summary>
        /// Adds the output generated in <see cref="Process" /> function to the output of the waterQualityModel
        /// </summary>
        void AddOutput(string workDirectory, IList<WaterQualityObservationVariableOutput> observationVariableOutputs,
                       Action<ADataItemMetaData, string> addTextDocument, MonitoringOutputLevel monitoringOutputLevel);

        /// <summary>
        /// Set to indicate that the processor should stop executing
        /// </summary>
        bool TryToCancel { get; set; }
    }
}