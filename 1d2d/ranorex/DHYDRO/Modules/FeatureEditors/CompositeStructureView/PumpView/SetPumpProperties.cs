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

namespace DHYDRO.Modules.FeatureEditors.CompositeStructureView.PumpView
{
#pragma warning disable 0436 //(CS0436) The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly'. Using the type defined in 'assembly'.
    /// <summary>
    ///The SetPumpProperties recording.
    /// </summary>
    [TestModule("02e36185-a699-4162-9087-af5d8a9bd580", ModuleType.Recording, 1)]
    public partial class SetPumpProperties : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRO1D2DRepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRO1D2DRepository repo = global::DHYDRO.DHYDRO1D2DRepository.Instance;

        static SetPumpProperties instance = new SetPumpProperties();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public SetPumpProperties()
        {
            Capacity = "";
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static SetPumpProperties Instance
        {
            get { return instance; }
        }

#region Variables

        string _Capacity;

        /// <summary>
        /// Gets or sets the value of variable Capacity.
        /// </summary>
        [TestVariable("96ef603f-1b0a-4898-b455-6d8fe53c6c6e")]
        public string Capacity
        {
            get { return _Capacity; }
            set { _Capacity = value; }
        }

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

            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DSWindow.DocumentsPaneCentral.FeatureEditors.CompositeStructureView.PumpView.CapacityField' at Center.", repo.DSWindow.DocumentsPaneCentral.FeatureEditors.CompositeStructureView.PumpView.CapacityFieldInfo, new RecordItemIndex(0));
            repo.DSWindow.DocumentsPaneCentral.FeatureEditors.CompositeStructureView.PumpView.CapacityField.Click();
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Keyboard", "Key 'Ctrl+A' Press with focus on 'DSWindow.DocumentsPaneCentral.FeatureEditors.CompositeStructureView.PumpView.CapacityField'.", repo.DSWindow.DocumentsPaneCentral.FeatureEditors.CompositeStructureView.PumpView.CapacityFieldInfo, new RecordItemIndex(1));
            Keyboard.PrepareFocus(repo.DSWindow.DocumentsPaneCentral.FeatureEditors.CompositeStructureView.PumpView.CapacityField);
            Keyboard.Press(System.Windows.Forms.Keys.A | System.Windows.Forms.Keys.Control, 30, Keyboard.DefaultKeyPressTime, 1, true);
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Keyboard", "Key sequence from variable '$Capacity' with focus on 'DSWindow.DocumentsPaneCentral.FeatureEditors.CompositeStructureView.PumpView.CapacityField'.", repo.DSWindow.DocumentsPaneCentral.FeatureEditors.CompositeStructureView.PumpView.CapacityFieldInfo, new RecordItemIndex(2));
            repo.DSWindow.DocumentsPaneCentral.FeatureEditors.CompositeStructureView.PumpView.CapacityField.PressKeys(Capacity);
            Delay.Milliseconds(0);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
