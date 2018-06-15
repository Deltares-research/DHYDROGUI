using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Forms;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using Fluent;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Ribbon
{
    /// <summary>
    /// Interaction logic for Ribbon.xaml
    /// </summary>
    public partial class Ribbon : IRibbonCommandHandler
    {
        public Ribbon()
        {
            InitializeComponent();

            mapTab.Group = geospatialContextualGroup;

            ButtonReverseLine.ToolTip = new ScreenTip
            {
                Title = "Reverse line(s)",
                Text = "Reverses the selected poly-line features.",
                DisableReason = "Required to have exclusively 2D/3D oriented polyline features selected.",
                MaxWidth = 250,
            };
        }

        public object GetRibbonControl()
        {
            return RibbonControl;
        }

        public void ValidateItems()
        {
            ViewModel.RefreshButtons();
        }

        public bool IsContextualTabVisible(string tabGroupName, string tabName)
        {
            return true;
        }

        public IEnumerable<ICommand> Commands => Enumerable.Empty<ICommand>();
    }
}

 