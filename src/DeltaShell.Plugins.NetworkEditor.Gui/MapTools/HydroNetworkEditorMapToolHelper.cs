using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Controls.Wpf.Dialogs;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public static class HydroNetworkEditorMapToolHelper
    {
        private static double CGDMinimumCellLength = 0.5;
        private static bool CGDGridAtStructure = true;
        private static double CGDStructureDistance = 10.0;
        private static bool CGDGridAtCrossSection = false;
        private static bool CGDGridAtLateral = false;
        private static bool CGDUseFixedLength = false;
        private static double CGDFixedLength = 100;

        public static bool RunCalculationGridWizard(IList<IChannel> selectedChannels, IDiscretization defaultDiscretization)
        {
            var calculationGridDialog = new ComputationalGridDialog
                                            {
                                                UpdateDiscretization = defaultDiscretization,
                                                MinimumCellLength = CGDMinimumCellLength,
                                                GridAtStructure = CGDGridAtStructure,
                                                StructureDistance = CGDStructureDistance,
                                                GridAtCrossSection = CGDGridAtCrossSection,
                                                GridAtLateralSource = CGDGridAtLateral,
                                                UseFixedLength = CGDUseFixedLength,
                                                FixedLength = CGDFixedLength,
                                                AllBranches = ((selectedChannels == null) || (selectedChannels.Count == 0)),
                                                AllowSelectionCheck = (selectedChannels != null)
                                            };

            if (DialogResult.OK != calculationGridDialog.ShowDialog())
            {
                return false;
            }

            UpdateDefaultCalculationParameters(calculationGridDialog);

            var discretization = calculationGridDialog.UpdateDiscretization;

            using (discretization.InEditMode("Generate grid"))
            {
                EventingHelper.DoWithoutEvents(() =>
                {
                    void GenerateGrid() => HydroNetworkHelper.GenerateDiscretization(discretization,
                                                                                     calculationGridDialog.OverwriteSegments, 
                                                                                     calculationGridDialog.Erase, 
                                                                                     calculationGridDialog.MinimumCellLength, 
                                                                                     calculationGridDialog.GridAtStructure, 
                                                                                     calculationGridDialog.StructureDistance, 
                                                                                     calculationGridDialog.GridAtCrossSection, 
                                                                                     calculationGridDialog.GridAtLateralSource, 
                                                                                     calculationGridDialog.UseFixedLength, 
                                                                                     calculationGridDialog.FixedLength, 
                                                                                     calculationGridDialog.AllBranches ? null : selectedChannels);

                    ProgressBarDialog.PerformTask("Generating Computational grid points", GenerateGrid, null);
                });
            }

            return true;
        }

        private static void UpdateDefaultCalculationParameters(ComputationalGridDialog calculationGridWizard)
        {
            CGDMinimumCellLength = calculationGridWizard.MinimumCellLength;
            CGDGridAtStructure = calculationGridWizard.GridAtStructure;
            CGDStructureDistance = calculationGridWizard.StructureDistance;
            CGDGridAtCrossSection = calculationGridWizard.GridAtCrossSection;
            CGDGridAtLateral = calculationGridWizard.GridAtLateralSource;
            CGDUseFixedLength = calculationGridWizard.UseFixedLength;
            CGDFixedLength = calculationGridWizard.FixedLength;
        }
    }
}