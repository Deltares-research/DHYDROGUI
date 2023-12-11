using System;
using Ranorex;
using Ranorex.Core.Repository;

namespace DHYDRO.Code
{
	/// <summary>
	/// Provides data table editing functionality.
	/// </summary>
	public static class TableUtils
	{
		/// <summary>
		/// Sets the table cell value.
		/// </summary>
		/// <param name="cellInfo">The repository item that represents a table cell.</param>
		/// <param name="cellValue">The value to set to the table cell.</param>
        public static void FillCellValue(RepoItemInfo cellInfo, string cellValue)
        {
            Mouse.DefaultMoveTime = 0;
            Keyboard.DefaultKeyPressTime = 5;
            Delay.SpeedFactor = 0.00;
            var cellAdapter = cellInfo.FindAdapter<Cell>();
            cellAdapter.Click();
            cellAdapter.PressKeys(cellValue + "{Return}");
        }
        
		/// <summary>
		/// Sets the check state of the checkbox in the given cell.
		/// </summary>
		/// <param name="cellInfo">The repository item that represents a table cell containing a checkbox.</param>
		/// <param name="checkValue">The check state to set.</param>
		/// <exception cref="RanorexException">When the specified cell does not contain a checkbox.</exception>
        public static void SetChecked(RepoItemInfo cellInfo, bool checkValue)
        {
        	var isChecked = IsChecked(cellInfo);
        	
        	if (isChecked != checkValue)
        	{
        		var cellAdapter = cellInfo.FindAdapter<Cell>();
        		cellAdapter.Click();
        	}
        }
        
        /// <summary>
        /// Returns whether the checkbox in the given cell is checked.
        /// </summary>
        /// <param name="cellInfo">The repository item that represents a table cell containing a checkbox.</param>
        /// <returns><c>true</c> when the checkbox is checked; otherwise <c>false</c>.</returns>
        /// <exception cref="RanorexException">When the specified cell does not contain a checkbox.</exception>
        public static bool IsChecked(RepoItemInfo cellInfo)
        {
        	var cellAdapter = cellInfo.FindAdapter<Cell>();
	        var checkState = cellAdapter.GetAttributeValue<string>("AccessibleValue");
				
			if(checkState == "Checked")
			{
				return true;
			}
			if(checkState == "Unchecked")
			{
				return false;
			}
			
			throw new RanorexException($"Cell '{cellInfo.Name}' isn't a checkbox");
        }
	}
}
