using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    public class BoundaryDataMapSymbolsTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowAllSymbols()
        {
            var flowPanel = new FlowLayoutPanel();
            foreach (BoundaryConditionDataType forcing in new FlowBoundaryConditionEditorController().AllSupportedDataTypes)
            {
                foreach (FlowBoundaryQuantityType qt in Enum.GetValues(typeof(FlowBoundaryQuantityType)))
                {
                    Bitmap symbol = BoundaryDataMapSymbols.GetSymbol(qt, forcing);
                    var pb = new PictureBox {Image = symbol};
                    flowPanel.Controls.Add(pb);
                }
            }

            WindowsFormsTestHelper.ShowModal(flowPanel);
        }
    }
}