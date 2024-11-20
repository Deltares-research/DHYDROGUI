/*
 * Created by Ranorex
 * User: rodriqu_dd
 * Date: 28/10/2021
 * Time: 11:02
 * 
 * To change this template use Tools > Options > Coding > Edit standard headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading;
using WinForms = System.Windows.Forms;
using Ranorex_Automation_Helpers.UserCodeCollections;

using Ranorex;
using Ranorex.Core;
using Ranorex.Core.Testing;

namespace DHYDRO.Modules.Calculations
{
    /// <summary>
    /// Description of ConvertToCurrentCulture.
    /// </summary>
    [TestModule("FDF9ECBC-142C-4210-AB32-F9846D51AF27", ModuleType.UserCode, 1)]
    public class ConvertDoubleToCurrentCulture : ITestModule
    {
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public ConvertDoubleToCurrentCulture()
        {
            // Do not delete - a parameterless constructor is required!
        }
        
        
        string _stringToConvertToCurrentCulture = "";
        [TestVariable("814ae3f0-5a9d-4964-88c7-18b5716ee9a3")]
        public string stringToConvert
        {
            get { return _stringToConvertToCurrentCulture; }
            set { _stringToConvertToCurrentCulture = value; }
        }
        
        string _stringConverted = "";
        [TestVariable("ac1b2271-2ef8-44de-a3cd-04ec9493d520")]
        public string stringConverted
        {
            get { return _stringConverted; }
            set { _stringConverted = value; }
        }
        

        /// <summary>
        /// Performs the playback of actions in this module.
        /// </summary>
        /// <remarks>You should not call this method directly, instead pass the module
        /// instance to the <see cref="TestModuleRunner.Run(ITestModule)"/> method
        /// that will in turn invoke this method.</remarks>
        void ITestModule.Run()
        {
            stringConverted = stringToConvert.ToCurrentCulture();
        }
    }
}
