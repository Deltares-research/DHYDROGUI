/*
 * Created by Ranorex
 * User: stolker
 * Date: 1-8-2024
 * Time: 11:23
 * 
 * To change this template use Tools > Options > Coding > Edit standard headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Linq;
using System.Threading;
using WinForms = System.Windows.Forms;

using Ranorex;
using Ranorex.Core;
using Ranorex.Core.Testing;

namespace DHYDRO.Modules.MapTree
{
    /// <summary>
    /// Description of UncheckNodeInMapTree.
    /// </summary>
    [TestModule("F1D5860C-6E6B-48AD-BBB3-14114128678A", ModuleType.UserCode, 1)]
    public class SetCheckStateInMapTree : ITestModule
    {
    	string _FullPathToTreeItem = "";
    	
    	[TestVariable("7fecbe0b-2b6e-4694-8d3f-248ec0da2a6d")]
    	public string FullPathToTreeItem
    	{
    		get { return _FullPathToTreeItem; }
    		set { _FullPathToTreeItem = value; }
    	}
    	
    	string _Checked = "false";
    	[TestVariable("b3e7e0b4-bf3a-4d2d-b0b1-bae39a3a79bd")]
    	public string Checked
    	{
    		get { return _Checked; }
    		set { _Checked = value; }
    	}
    	    	
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public SetCheckStateInMapTree()
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
            Mouse.DefaultMoveTime = 300;
            Keyboard.DefaultKeyPressTime = 100;
            Delay.SpeedFactor = 1.0;

            TreeItem selectedItem = SelectNodeInMapTree.SelectNode(FullPathToTreeItem);
            CheckBox checkBox = selectedItem.Find<CheckBox>("checkbox").Single();
            checkBox.Checked = bool.Parse(Checked);
        }
    }
}
