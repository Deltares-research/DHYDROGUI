﻿///////////////////////////////////////////////////////////////////////////////
//
// This file was automatically generated by RANOREX.
// DO NOT MODIFY THIS FILE! It is regenerated by the designer.
// All your modifications will be lost!
// http://www.ranorex.com
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading;
using WinForms = System.Windows.Forms;

using Ranorex;
using Ranorex.Core;
using Ranorex.Core.Testing;
using Ranorex.Core.Repository;

namespace DHYDRO.Modules.Delays
{
#pragma warning disable 0436 //(CS0436) The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly'. Using the type defined in 'assembly'.
    /// <summary>
    ///The WaitForOpenProject recording.
    /// </summary>
    [TestModule("d2d35bb1-f562-4e75-8615-e94ac0c5ae40", ModuleType.Recording, 1)]
    public partial class WaitForOpenProject : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRORepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRORepository repo = global::DHYDRO.DHYDRORepository.Instance;

        static WaitForOpenProject instance = new WaitForOpenProject();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public WaitForOpenProject()
        {
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static WaitForOpenProject Instance
        {
            get { return instance; }
        }

#region Variables

#endregion

        /// <summary>
        /// Starts the replay of the static recording <see cref="Instance"/>.
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCode("Ranorex", global::Ranorex.Core.Constants.CodeGenVersion)]
        public static void Start()
        {
            TestModuleRunner.Run(Instance);
        }

        /// <summary>
        /// Performs the playback of actions in this recording.
        /// </summary>
        /// <remarks>You should not call this method directly, instead pass the module
        /// instance to the <see cref="TestModuleRunner.Run(ITestModule)"/> method
        /// that will in turn invoke this method.</remarks>
        [System.CodeDom.Compiler.GeneratedCode("Ranorex", global::Ranorex.Core.Constants.CodeGenVersion)]
        void ITestModule.Run()
        {
            Mouse.DefaultMoveTime = 300;
            Keyboard.DefaultKeyPressTime = 20;
            Delay.SpeedFactor = 1.00;

            Init();

            Report.Log(ReportLevel.Info, "Delay", "Waiting for 2ms.", new RecordItemIndex(0));
            Delay.Duration(2, false);
            
            try {
                Report.Log(ReportLevel.Info, "Wait", "(Optional Action)\r\nWaiting 30s to exist. Associated repository item: 'ProgressBarWindow.ProgressBarBar'", repo.ProgressBarWindow.ProgressBarBarInfo, new ActionTimeout(30000), new RecordItemIndex(1));
                repo.ProgressBarWindow.ProgressBarBarInfo.WaitForExists(30000);
            } catch(Exception ex) { Report.Log(ReportLevel.Warn, "Module", "(Optional Action) " + ex.Message, new RecordItemIndex(1)); }
            
            Report.Log(ReportLevel.Info, "Wait", "Waiting 2m to not exist. Associated repository item: 'ProgressBarWindow.ProgressBarBar'", repo.ProgressBarWindow.ProgressBarBarInfo, new ActionTimeout(120000), new RecordItemIndex(2));
            repo.ProgressBarWindow.ProgressBarBarInfo.WaitForNotExists(120000);
            
            Report.Log(ReportLevel.Info, "Delay", "Waiting for 1s.", new RecordItemIndex(3));
            Delay.Duration(1000, false);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
