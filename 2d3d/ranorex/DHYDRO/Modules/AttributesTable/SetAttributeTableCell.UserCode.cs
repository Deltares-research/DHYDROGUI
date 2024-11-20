using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WinForms = System.Windows.Forms;
using DHYDRO.Code;

using Ranorex;
using Ranorex.Core;
using Ranorex.Core.Repository;
using Ranorex.Core.Testing;

namespace DHYDRO.Modules.AttributesTable
{
    public partial class SetAttributeTableCell
    {
        /// <summary>
        /// This method gets called right after the recording has been started.
        /// It can be used to execute recording specific initialization code.
        /// </summary>
        private void Init()
        {
        }

        /// <summary>
        /// Fills the parameter value in the specified table cell.
        /// </summary>
        /// <param name="cellInfo">The repository item that represents the table cell.</param>
        public void FillInParameterValue(RepoItemInfo cellInfo)
        {
        	TableUtils.FillCellValue(cellInfo, ParameterValue);
        }
    }
}
