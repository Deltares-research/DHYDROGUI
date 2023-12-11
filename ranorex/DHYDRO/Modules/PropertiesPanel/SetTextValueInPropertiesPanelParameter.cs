/*
 * Created by Ranorex
 * User: rodriqu_dd
 * Date: 08/06/2022
 * Time: 15:27
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
using System.Linq;

using Ranorex;
using Ranorex.Core;
using Ranorex.Core.Testing;

namespace DHYDRO.Modules.PropertiesPanel
{
    /// <summary>
    /// Description of SetTextValueInPropertiesPanelParameter.
    /// </summary>
    [TestModule("5890EBB4-9B00-42E5-ABE9-7E64A7C47983", ModuleType.UserCode, 1)]
    public class SetTextValueInPropertiesPanelParameter : ITestModule
    {
        
    	string _parameterName = "";
    	[TestVariable("7eb5ab80-5de9-4af8-8f26-91cc77fd21e4")]
    	public string parameterName
    	{
    		get { return _parameterName; }
    		set { _parameterName = value; }
    	}
    	
    	string _newValueForParameter = "";
    	[TestVariable("be1e3226-8ecd-4a71-baf3-c9077b613727")]
    	public string newValueForParameter
    	{
    		get { return _newValueForParameter; }
    		set { _newValueForParameter = value; }
    	}
    	
    	/// <summary>
        /// Constructs a new instance.
        /// </summary>
        public SetTextValueInPropertiesPanelParameter()
        {
            // Do not delete - a parameterless constructor is required!
        }

        /// <summary>
        /// Performs the playback of actions in this module.
        /// </summary>
        /// <remarks>You should not call this method directly, instead pass the module
        /// instance to the <see cref="TestModuleRunner.Run(ITestModule)"/> method
        /// that will in turn invoke this method.</remarks>
        void ITestModule.Run()
        {
            Mouse.DefaultMoveTime = 0;
            Keyboard.DefaultKeyPressTime = 0;
            Delay.SpeedFactor = 0.00;
            var repos = DHYDRORepository.Instance;
            Adapter propertiesPanelAdapter = repos.DSWindow.ListView.PropertiesPanel.Self;
            var allCells = propertiesPanelAdapter.Children.Where(ch=>ch.As<Cell>()!=null);
            var cellToChange = allCells.Where(cl => cl.Element.GetAttributeValueText("AccessibleName")==parameterName).FirstOrDefault();
            cellToChange.As<Cell>().Focus();
            cellToChange.As<Cell>().Select();
            cellToChange.Element.SetAttributeValue("AccessibleValue", newValueForParameter);
        }
    }
}
