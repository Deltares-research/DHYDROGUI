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

namespace DHYDRO.Modules.Ribbon
{
#pragma warning disable 0436 //(CS0436) The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly'. Using the type defined in 'assembly'.
    /// <summary>
    ///The StartAddSewerConnection recording.
    /// </summary>
    [TestModule("e7cba844-ef0b-48c2-a694-26078a8ce483", ModuleType.Recording, 1)]
    public partial class StartAddSewerConnection : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRO1D2DRepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRO1D2DRepository repo = global::DHYDRO.DHYDRO1D2DRepository.Instance;

        static StartAddSewerConnection instance = new StartAddSewerConnection();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public StartAddSewerConnection()
        {
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static StartAddSewerConnection Instance
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

            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DSWindow.RibbonTabControl.Ribbon.Network_1D.ButtonAddNewSewerConnection' at Center.", repo.DSWindow.RibbonTabControl.Ribbon.Network_1D.ButtonAddNewSewerConnectionInfo, new RecordItemIndex(0));
            repo.DSWindow.RibbonTabControl.Ribbon.Network_1D.ButtonAddNewSewerConnection.Click();
            Delay.Milliseconds(0);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
