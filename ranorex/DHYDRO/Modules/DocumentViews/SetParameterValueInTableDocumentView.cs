/*
 * Created by Ranorex
 * User: rodriqu_dd
 * Date: 08/06/2022
 * Time: 17:41
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

namespace DHYDRO.Modules.DocumentViews
{
    /// <summary>
    /// Description of SetParameterValueInTableDocumentView.
    /// </summary>
    [TestModule("EB94C32F-9022-42C1-9EB5-EB50EB5FA3AE", ModuleType.UserCode, 1)]
    public class SetParameterValueInTableDocumentView : ITestModule
    {
        
        string _columnName = "";
        [TestVariable("8fba57a1-13a5-4c6b-a400-50c1fbc1a399")]
        public string columnName
        {
        	get { return _columnName; }
        	set { _columnName = value; }
        }
        
        
        string _parameterName = "";
        [TestVariable("59b1da7d-1bdd-4e45-90bf-86059cabb4ac")]
        public string parameterName
        {
        	get { return _parameterName; }
        	set { _parameterName = value; }
        }
        
        string _newValueParameter = "";
        [TestVariable("2afc5041-172b-41db-b4e5-cac883c53d47")]
        public string newValueParameter
        {
        	get { return _newValueParameter; }
        	set { _newValueParameter = value; }
        }
        
    	/// <summary>
        /// Constructs a new instance.
        /// </summary>
        public SetParameterValueInTableDocumentView()
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
            var table = repos.DSWindow.ListView.WinFormsAdapter.ParametersDocumentView.Table;
            var headerRow = table.Header.Children.Select(it => it.As<Cell>()).ToList();
            var cellWithName = headerRow.Where(cl => cl.Element.GetAttributeValueText("AccessibleName")==columnName).FirstOrDefault();
            var columnIndex = headerRow.IndexOf(cellWithName);
            var data = table.Data.Self;
            var rowToChange = data.Children.Where(rw => rw.Children[0].GetAttributeValue<string>("AccessibleValue") == parameterName).FirstOrDefault();
            var cellToChangeChildren = rowToChange.Children;
            var cellToChange = cellToChangeChildren[columnIndex];
            cellToChange.As<Cell>().Focus();
            cellToChange.As<Cell>().Select();
            cellToChange.DoubleClick();
            Keyboard.Press(newValueParameter + "{Return}");
        }
    }
}
