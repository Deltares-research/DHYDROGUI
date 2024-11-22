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

namespace DHYDRO.Modules.IO
{
#pragma warning disable 0436 //(CS0436) The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly'. Using the type defined in 'assembly'.
    /// <summary>
    ///The SpecifyDirectoryToExportTo recording.
    /// </summary>
    [TestModule("f0954594-df29-426f-929a-855967176c56", ModuleType.Recording, 1)]
    public partial class SpecifyDirectoryToExportTo : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRORepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRORepository repo = global::DHYDRO.DHYDRORepository.Instance;

        static SpecifyDirectoryToExportTo instance = new SpecifyDirectoryToExportTo();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public SpecifyDirectoryToExportTo()
        {
            DirectoryName = "";
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static SpecifyDirectoryToExportTo Instance
        {
            get { return instance; }
        }

#region Variables

        string _DirectoryName;

        /// <summary>
        /// Gets or sets the value of variable DirectoryName.
        /// </summary>
        [TestVariable("62e22cf6-ae61-4f96-a7e1-7c5a7cadd8d2")]
        public string DirectoryName
        {
            get { return _DirectoryName; }
            set { _DirectoryName = value; }
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

            Report.Log(ReportLevel.Info, "Set value", "Setting attribute Text to '$DirectoryName' on item 'DialogSelectFolder.DirectoryPath'.", repo.DialogSelectFolder.DirectoryPathInfo, new RecordItemIndex(0));
            repo.DialogSelectFolder.DirectoryPath.Element.SetAttributeValue("Text", DirectoryName);
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DialogSelectFolder.SelectFolder' at Center.", repo.DialogSelectFolder.SelectFolderInfo, new RecordItemIndex(1));
            repo.DialogSelectFolder.SelectFolder.Click();
            Delay.Milliseconds(0);
            
            Code.UserCodeLibrary.ClickIfExists(repo.ConfirmSaveAsDialog.ConfirmYesButtonInfo, ValueConverter.ArgumentFromString<int>("waitPeriodInMilliSeconds", "3000"));
            Delay.Milliseconds(0);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
