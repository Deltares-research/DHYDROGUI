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

namespace DHYDRO.Modules.FeatureEditors.MeteoView
{
#pragma warning disable 0436 //(CS0436) The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly'. Using the type defined in 'assembly'.
    /// <summary>
    ///The GenerateTimeSeries recording.
    /// </summary>
    [TestModule("47e168b3-bd6d-4724-ad8b-5346d5eeff55", ModuleType.Recording, 1)]
    public partial class GenerateTimeSeries : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRO1D2DRepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRO1D2DRepository repo = global::DHYDRO.DHYDRO1D2DRepository.Instance;

        static GenerateTimeSeries instance = new GenerateTimeSeries();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public GenerateTimeSeries()
        {
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static GenerateTimeSeries Instance
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

            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DSWindow.DocumentsPaneCentral.FeatureEditors.MeteoEditorView.GenerateTimeSeriesButton' at Center.", repo.DSWindow.DocumentsPaneCentral.FeatureEditors.MeteoEditorView.GenerateTimeSeriesButtonInfo, new RecordItemIndex(0));
            repo.DSWindow.DocumentsPaneCentral.FeatureEditors.MeteoEditorView.GenerateTimeSeriesButton.Click();
            Delay.Milliseconds(0);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
