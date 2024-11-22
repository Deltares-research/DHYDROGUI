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
    ///The AddNewLayer recording.
    /// </summary>
    [TestModule("39e020e2-f05b-42ba-9cda-5fadd8d92ea7", ModuleType.Recording, 1)]
    public partial class AddNewLayer : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRORepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRORepository repo = global::DHYDRO.DHYDRORepository.Instance;

        static AddNewLayer instance = new AddNewLayer();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public AddNewLayer()
        {
            FilePath = "";
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static AddNewLayer Instance
        {
            get { return instance; }
        }

#region Variables

        string _FilePath;

        /// <summary>
        /// Gets or sets the value of variable FilePath.
        /// </summary>
        [TestVariable("c39483a2-2b20-4e68-9565-abb1e03694c5")]
        public string FilePath
        {
            get { return _FilePath; }
            set { _FilePath = value; }
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

            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DSWindow.DocumentsPaneRight.MapLegendTree.AddNewLayerButton' at Center.", repo.DSWindow.DocumentsPaneRight.MapLegendTree.AddNewLayerButtonInfo, new RecordItemIndex(0));
            repo.DSWindow.DocumentsPaneRight.MapLegendTree.AddNewLayerButton.Click();
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Wait", "Waiting 5s to exist. Associated repository item: 'DialogSelectFile.FieldFilePath'", repo.DialogSelectFile.FieldFilePathInfo, new ActionTimeout(5000), new RecordItemIndex(1));
            repo.DialogSelectFile.FieldFilePathInfo.WaitForExists(5000);
            
            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DialogSelectFile.FieldFilePath' at Center.", repo.DialogSelectFile.FieldFilePathInfo, new RecordItemIndex(2));
            repo.DialogSelectFile.FieldFilePath.Click();
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Set value", "Setting attribute Text to '$FilePath' on item 'DialogSelectFile.FieldFilePath'.", repo.DialogSelectFile.FieldFilePathInfo, new RecordItemIndex(3));
            repo.DialogSelectFile.FieldFilePath.Element.SetAttributeValue("Text", FilePath);
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DialogSelectFile.ButtonOpen' at Center.", repo.DialogSelectFile.ButtonOpenInfo, new RecordItemIndex(4));
            repo.DialogSelectFile.ButtonOpen.Click();
            Delay.Milliseconds(0);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
