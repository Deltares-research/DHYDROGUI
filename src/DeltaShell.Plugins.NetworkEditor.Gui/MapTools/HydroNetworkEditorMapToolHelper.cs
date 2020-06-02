using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public static class HydroNetworkEditorMapToolHelper
    {
        // todo move to user settings
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
                AllBranches = selectedChannels == null || selectedChannels.Count == 0,
                AllowSelectionCheck = selectedChannels != null
            };

            if (DialogResult.OK != calculationGridDialog.ShowDialog())
            {
                return false;
            }

            UpdateDefaultCalculationParameters(calculationGridDialog);

            IDiscretization discretization = calculationGridDialog.UpdateDiscretization;

            var editable = discretization as IEditableObject;

            if (editable != null)
            {
                editable.BeginEdit("Generate grid");
            }

            HydroNetworkHelper.GenerateDiscretization(discretization,
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
            if (editable != null)
            {
                editable.EndEdit();
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