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

namespace DHYDRO.Modules.FeatureEditors.MultipleFunctionView
{
#pragma warning disable 0436 //(CS0436) The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly'. Using the type defined in 'assembly'.
    /// <summary>
    ///The SetMultipleFunctionViewTableCell recording.
    /// </summary>
    [TestModule("78a03ee6-745d-47fd-b2a9-aeeb39932b48", ModuleType.Recording, 1)]
    public partial class SetMultipleFunctionViewTableCell : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRO1D2DRepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRO1D2DRepository repo = global::DHYDRO.DHYDRO1D2DRepository.Instance;

        static SetMultipleFunctionViewTableCell instance = new SetMultipleFunctionViewTableCell();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public SetMultipleFunctionViewTableCell()
        {
            ParameterValue = "";
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static SetMultipleFunctionViewTableCell Instance
        {
            get { return instance; }
        }

#region Variables

        string _ParameterValue;

        /// <summary>
        /// Gets or sets the value of variable ParameterValue.
        /// </summary>
        [TestVariable("0002ceda-8f31-42af-ae2b-f50c52bafaa9")]
        public string ParameterValue
        {
            get { return _ParameterValue; }
            set { _ParameterValue = value; }
        }

        /// <summary>
        /// Gets or sets the value of variable rowNumber.
        /// </summary>
        [TestVariable("8e9934c7-a04c-4bd8-b138-e1535118001c")]
        public string rowNumber
        {
            get { return repo.rowNumber; }
            set { repo.rowNumber = value; }
        }

        /// <summary>
        /// Gets or sets the value of variable columnName.
        /// </summary>
        [TestVariable("802b7a40-50d7-425c-b1bc-585751b36262")]
        public string columnName
        {
            get { return repo.columnName; }
            set { repo.columnName = value; }
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

            FillInParameterValue(repo.DSWindow.DocumentsPaneCentral.MultipleFunctionView.Table.TableData.GenericTableCellInfo);
            Delay.Milliseconds(0);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
