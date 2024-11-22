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

namespace DHYDRO.Modules.Map
{
#pragma warning disable 0436 //(CS0436) The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly'. Using the type defined in 'assembly'.
    /// <summary>
    ///The MakeOutputMapVariableVisible2D recording.
    /// </summary>
    [TestModule("ca3eeb8a-6166-4e23-be36-1c1d6ae8a4fa", ModuleType.Recording, 1)]
    public partial class MakeOutputMapVariableVisible2D : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRO1D2DRepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRO1D2DRepository repo = global::DHYDRO.DHYDRO1D2DRepository.Instance;

        static MakeOutputMapVariableVisible2D instance = new MakeOutputMapVariableVisible2D();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public MakeOutputMapVariableVisible2D()
        {
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static MakeOutputMapVariableVisible2D Instance
        {
            get { return instance; }
        }

#region Variables

        /// <summary>
        /// Gets or sets the value of variable variableName.
        /// </summary>
        [TestVariable("1cb541ff-ae3e-4222-ac48-5c0cd23f7914")]
        public string variableName
        {
            get { return repo.variableName; }
            set { repo.variableName = value; }
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

            Report.Log(ReportLevel.Info, "Set value", "Setting attribute Checked to 'True' on item 'DSWindow.DocumentsPaneRight.MapLegendTree.OutputMapTreeItem.OutputMapFileTreeItem_2D.OutputMapVariableTreeItem_2D.OutputMapVariableCheckBox_2D'.", repo.DSWindow.DocumentsPaneRight.MapLegendTree.OutputMapTreeItem.OutputMapFileTreeItem_2D.OutputMapVariableTreeItem_2D.OutputMapVariableCheckBox_2DInfo, new RecordItemIndex(0));
            repo.DSWindow.DocumentsPaneRight.MapLegendTree.OutputMapTreeItem.OutputMapFileTreeItem_2D.OutputMapVariableTreeItem_2D.OutputMapVariableCheckBox_2D.Element.SetAttributeValue("Checked", "True");
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Delay", "Waiting for 300ms.", new RecordItemIndex(1));
            Delay.Duration(300, false);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
