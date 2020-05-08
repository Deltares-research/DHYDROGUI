using System;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public interface IWaqPreProcessor : IDisposable
    {
        /// <summary>
        /// Set to indicate that the processor should stop executing
        /// </summary>
        bool TryToCancel { get; set; }

        /// <summary>
        /// Initialize WaqModelApi
        /// </summary>
        /// <param name="initSettings"> Initialization settings </param>
        /// <exception cref="NullReferenceException"> Thrown when <paramref name="initSettings"/> is null.</exception>
        /// <returns><c> true </c> if the WaqModelApi was initialized successfully; otherwise, <c> false </c>.</returns>
        bool InitializeWaq(WaqInitializationSettings initSettings);
    }
}