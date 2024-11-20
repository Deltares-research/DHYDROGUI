using System;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public interface IWaqPreProcessor : IDisposable
    {
        /// <summary>
        /// Initialize WaqModelApi
        /// </summary>
        /// <param name="initSettings"> Initialization settings </param>
        /// <exception cref="NullReferenceException"> Thrown when <paramref name="initSettings"/> is null.</exception>
        void InitializeWaq(WaqInitializationSettings initSettings);
    }
}