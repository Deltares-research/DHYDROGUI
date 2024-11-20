/*
 * Created by Ranorex
 * User: rodriqu_dd
 * Date: 08/06/2022
 * Time: 19:04
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
    /// Description of GetColumnIndex.
    /// </summary>
    [TestModule("00AF44A2-F9BF-461D-91E4-8C86A8991EAD", ModuleType.UserCode, 1)]
    public class GetColumnIndex : ITestModule
    {
        
        string _columnIndex = "";
        [TestVariable("bed2e53f-e17b-498c-9954-a97604af5efe")]
        public string columnIndex
        {
        	get { return _columnIndex; }
        	set { _columnIndex = value; }
        }
        
        
        string _columnName = "";
        [TestVariable("5e760d71-2a60-42c6-9b0d-3da2174834d8")]
        public string columnName
        {
        	get { return _columnName; }
        	set { _columnName = value; }
        }
        
    	
    	/// <summary>
        /// Constructs a new instance.
        /// </summary>
        public GetColumnIndex()
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
            var repos = DHYDRORepository.Instance;
            var table = repos.DSWindow.DocumentsPaneCentral.WaterQualityParametersDocumentView.Table;
            var headerRow = table.Header.Children.Select(it => it.As<Cell>()).ToList();
            var cellWithName = headerRow.Where(cl => cl.Element.GetAttributeValueText("AccessibleName")==columnName).FirstOrDefault();
            columnIndex = headerRow.IndexOf(cellWithName).ToString();
        }
    }
}
