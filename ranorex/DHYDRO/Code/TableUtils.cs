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
	}
}
