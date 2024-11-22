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
    ///The DelayXMiliseconds recording.
    /// </summary>
    [TestModule("64309eeb-f439-41a7-98a7-fc1d63e0015d", ModuleType.Recording, 1)]
    public partial class DelayXMiliseconds : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRORepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRORepository repo = global::DHYDRO.DHYDRORepository.Instance;

        static DelayXMiliseconds instance = new DelayXMiliseconds();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public DelayXMiliseconds()
        {
            delayInMiliseconds = "0";
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static DelayXMiliseconds Instance
        {
            get { return instance; }
        }

#region Variables

        string _delayInMiliseconds;

        /// <summary>
        /// Gets or sets the value of variable delayInMiliseconds.
        /// </summary>
        [TestVariable("9a7db6ae-dfb4-4e1f-869e-ebd323130abd")]
        public string delayInMiliseconds
        {
            get { return _delayInMiliseconds; }
            set { _delayInMiliseconds = value; }
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

            Report.Log(ReportLevel.Info, "Delay", "Waiting for time from variable $delayInMiliseconds.", new RecordItemIndex(0));
            Delay.Duration(Duration.Parse(delayInMiliseconds), false);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
