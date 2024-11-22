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

namespace DHYDRO.Modules.Validation
{
#pragma warning disable 0436 //(CS0436) The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly'. Using the type defined in 'assembly'.
    /// <summary>
    ///The CheckValidationResult recording.
    /// </summary>
    [TestModule("8474cc58-38f2-4607-b7f7-f35d4429d1cf", ModuleType.Recording, 1)]
    public partial class CheckValidationResult : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRO1D2DRepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRO1D2DRepository repo = global::DHYDRO.DHYDRO1D2DRepository.Instance;

        static CheckValidationResult instance = new CheckValidationResult();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public CheckValidationResult()
        {
            ExpectedValidationResult = "";
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static CheckValidationResult Instance
        {
            get { return instance; }
        }

#region Variables

        string _ExpectedValidationResult;

        /// <summary>
        /// Gets or sets the value of variable ExpectedValidationResult.
        /// </summary>
        [TestVariable("46b29690-5506-49af-a98f-9fbb6d67f7b3")]
        public string ExpectedValidationResult
        {
            get { return _ExpectedValidationResult; }
            set { _ExpectedValidationResult = value; }
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

            CheckModelValidationResult(repo.DSWindow.DocumentsPaneCentral.ValidationView.SelfInfo);
            Delay.Milliseconds(0);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
