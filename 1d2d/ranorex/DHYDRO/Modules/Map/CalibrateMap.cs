using System;
using System.Globalization;
using DHYDRO.Code;
using Ranorex;
using Ranorex.Core.Testing;
using WinForms = System.Windows.Forms;

namespace DHYDRO.Modules.Map
{
    /// <summary>
    ///     Description of CalibrateMap.
    /// </summary>
    [TestModule("0BC79973-0C13-4F47-8C3C-06DA550F6269", ModuleType.UserCode, 1)]
    public class CalibrateMap : ITestModule
    {
        private static readonly DHYDRO1D2DRepository Repo = DHYDRO1D2DRepository.Instance;

        /// <summary>
        ///     Performs the playback of actions in this module.
        ///     Calibrates the current map by clicking two points and reading the coordinates from the status bar
        ///     and updates the current transformation settings.
        /// </summary>
        /// <remarks>
        ///     You should not call this method directly, instead pass the module
        ///     instance to the <see cref="TestModuleRunner.Run(ITestModule)" /> method
        ///     that will in turn invoke this method.
        /// </remarks>
        void ITestModule.Run()
        {
            MapCalibration.Execute();
        }
    }
}