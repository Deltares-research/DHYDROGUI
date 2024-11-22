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

namespace DHYDRO.Modules.ModelSettings
{
#pragma warning disable 0436 //(CS0436) The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly'. Using the type defined in 'assembly'.
    /// <summary>
    ///The SetSettings recording.
    /// </summary>
    [TestModule("72b10c2c-e289-45df-827d-f85ac1a38b57", ModuleType.Recording, 1)]
    public partial class SetSettings : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRO1D2DRepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRO1D2DRepository repo = global::DHYDRO.DHYDRO1D2DRepository.Instance;

        static SetSettings instance = new SetSettings();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public SetSettings()
        {
            DataSource = "";
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static SetSettings Instance
        {
            get { return instance; }
        }

#region Variables

        string _DataSource;

        /// <summary>
        /// Gets or sets the value of variable DataSource.
        /// </summary>
        [TestVariable("135e4322-e4f9-49fa-bf7a-bc5804a2000a")]
        public string DataSource
        {
            get { return _DataSource; }
            set { _DataSource = value; }
        }

        /// <summary>
        /// Gets or sets the value of variable OutputGroupHeaderName.
        /// </summary>
        [TestVariable("8cb128c8-71ba-4571-91a9-fa8cfdc80c0c")]
        public string OutputGroupHeaderName
        {
            get { return repo.OutputGroupHeaderName; }
            set { repo.OutputGroupHeaderName = value; }
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

            Set(repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Output_Parameters.OutputGroup.SelfInfo, DataSource);
            Delay.Milliseconds(0);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
