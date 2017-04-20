using System;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public interface IWaqPreProcessor : IDisposable
    {
        /// <summary>
        /// Initialize WaqModelApi
        /// </summary>
        /// <param name="initSettings">Initialization settings</param>
        /// <param name="addTextDocumentAction">Action for adding text documents generated after preprocessing</param>
        /// <exception cref="NullReferenceException">Throws when <param name="initSettings"></param> is null</exception>
        /// <returns>Initialization completed successfully</returns>
        bool InitializeWaq(WaqInitializationSettings initSettings, Action<string, string> addTextDocumentAction);
    }
}