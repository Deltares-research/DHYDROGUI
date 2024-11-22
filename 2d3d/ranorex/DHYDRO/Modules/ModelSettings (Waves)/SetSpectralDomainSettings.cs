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

namespace DHYDRO.Modules.ModelSettings__Waves_
{
#pragma warning disable 0436 //(CS0436) The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly'. Using the type defined in 'assembly'.
    /// <summary>
    ///The SetSpectralDomainSettings recording.
    /// </summary>
    [TestModule("041bf6cb-b1bf-4d92-a33d-143fb5e45abf", ModuleType.Recording, 1)]
    public partial class SetSpectralDomainSettings : ITestModule
    {
        /// <summary>
        /// Holds an instance of the global::DHYDRO.DHYDRORepository repository.
        /// </summary>
        public static global::DHYDRO.DHYDRORepository repo = global::DHYDRO.DHYDRORepository.Instance;

        static SetSpectralDomainSettings instance = new SetSpectralDomainSettings();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public SetSpectralDomainSettings()
        {
        }

        /// <summary>
        /// Gets a static instance of this recording.
        /// </summary>
        public static SetSpectralDomainSettings Instance
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

            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.DirectionalSpaceComboBox' at Center.", repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.DirectionalSpaceComboBoxInfo, new RecordItemIndex(0));
            repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.DirectionalSpaceComboBox.Click();
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'SettingsContextMenu.Circle' at Center.", repo.SettingsContextMenu.CircleInfo, new RecordItemIndex(1));
            repo.SettingsContextMenu.Circle.Click();
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfDirectionsField' at Center.", repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfDirectionsFieldInfo, new RecordItemIndex(2));
            repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfDirectionsField.Click();
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Keyboard", "Key 'Ctrl+A' Press.", new RecordItemIndex(3));
            Keyboard.Press(System.Windows.Forms.Keys.A | System.Windows.Forms.Keys.Control, 30, Keyboard.DefaultKeyPressTime, 1, true);
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Keyboard", "Key sequence '36' with focus on 'DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfDirectionsField'.", repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfDirectionsFieldInfo, new RecordItemIndex(4));
            repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfDirectionsField.PressKeys("36");
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfFrequenciesField' at Center.", repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfFrequenciesFieldInfo, new RecordItemIndex(5));
            repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfFrequenciesField.Click();
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Keyboard", "Key 'Ctrl+A' Press.", new RecordItemIndex(6));
            Keyboard.Press(System.Windows.Forms.Keys.A | System.Windows.Forms.Keys.Control, 30, Keyboard.DefaultKeyPressTime, 1, true);
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Keyboard", "Key sequence '24' with focus on 'DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfFrequenciesField'.", repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfFrequenciesFieldInfo, new RecordItemIndex(7));
            repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.NumberOfFrequenciesField.PressKeys("24");
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MinFrequenceField' at Center.", repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MinFrequenceFieldInfo, new RecordItemIndex(8));
            repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MinFrequenceField.Click();
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Keyboard", "Key 'Ctrl+A' Press.", new RecordItemIndex(9));
            Keyboard.Press(System.Windows.Forms.Keys.A | System.Windows.Forms.Keys.Control, 30, Keyboard.DefaultKeyPressTime, 1, true);
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Keyboard", "Key sequence '0.03' with focus on 'DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MinFrequenceField'.", repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MinFrequenceFieldInfo, new RecordItemIndex(10));
            repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MinFrequenceField.PressKeys("0.03");
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MaxFrequenceField' at Center.", repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MaxFrequenceFieldInfo, new RecordItemIndex(11));
            repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MaxFrequenceField.Click();
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Keyboard", "Key 'Ctrl+A' Press.", new RecordItemIndex(12));
            Keyboard.Press(System.Windows.Forms.Keys.A | System.Windows.Forms.Keys.Control, 30, Keyboard.DefaultKeyPressTime, 1, true);
            Delay.Milliseconds(0);
            
            Report.Log(ReportLevel.Info, "Keyboard", "Key sequence '1' with focus on 'DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MaxFrequenceField'.", repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MaxFrequenceFieldInfo, new RecordItemIndex(13));
            repo.DSWindow.DocumentsPaneCentral.ModelSettingsTabControl.Spectral_Domain.MaxFrequenceField.PressKeys("1");
            Delay.Milliseconds(0);
            
        }

#region Image Feature Data
#endregion
    }
#pragma warning restore 0436
}
