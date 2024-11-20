using System;
using System.Collections.Generic;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Forms;

namespace DeltaShell.Dimr.Gui
{
    /// <summary>
    /// Interaction logic for Ribbon.xaml
    /// </summary>
    public partial class Ribbon : IRibbonCommandHandler
    {
        public Ribbon()
        {
            InitializeComponent();
            tabDimr.Group = configContextualGroup;
        }

        public IEnumerable<ICommand> Commands
        {
            get
            {
                yield break;
            }
        }

        public bool IsContextualTabVisible(string tabGroupName, string tabName)
        {
            return tabGroupName == configContextualGroup.Name && tabName == tabDimr.Name && DimrGuiPlugin.Instance != null && DimrGuiPlugin.Instance.IsOnlyDimrModelSelected;
        }

        public void ValidateItems()
        {
            // There is nothing to validate, but is enforced through IRibbonCommandHandler.
        }

        public object GetRibbonControl()
        {
            return RibbonControl;
        }
    }
}