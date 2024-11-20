using System;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Specifies an interface for reading hydro models.
    /// </summary>
    public interface IHydroModelReader
    {
        /// <summary>
        /// Reads a <see cref="HydroModel"/> (Integrated model) from the specified path.
        /// </summary>
        /// <param name="path">Path to the Dimr.xml</param>
        /// <param name="reportProgress">Optional callback for progress reporting.</param>
        /// <returns>Read <see cref="HydroModel"/></returns>
        HydroModel Read(string path, Action<string> reportProgress = null);
    }
}