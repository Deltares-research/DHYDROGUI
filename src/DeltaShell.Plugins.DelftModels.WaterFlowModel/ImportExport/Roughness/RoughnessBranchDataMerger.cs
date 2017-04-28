using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class RoughnessBranchDataMerger
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RoughnessBranchDataMerger));

        public static void MergeIntoRoughnessSections(IList<RoughnessSection> roughnessSections, IEnumerable<RoughessCsvBranchData> branchData)
        {
            if (roughnessSections.Count == 0)
            {
                Log.Error("No roughness sections in model; can not continue import.");
                return;
            }
            var network = roughnessSections[0].RoughnessNetworkCoverage.Network;

            roughnessSections.ForEach(roughnessSection => roughnessSection.Clear());

            foreach (var roughessCsvBranchData in branchData)
            {
                var roughnessSection =
                    roughnessSections.Where(rs => rs.Name == roughessCsvBranchData.SectionType).FirstOrDefault();
                if (roughnessSection == null)
                {
                    Log.ErrorFormat("Unknown sectiontype '{0}'; record skipped.", roughessCsvBranchData.SectionType);
                    continue;
                }
                var branch = network.Branches.Where(b => b.Name == roughessCsvBranchData.BranchName).FirstOrDefault();
                if (branch == null)
                {
                    if (branch == null)
                    {
                        Log.ErrorFormat("Unknown branch '{0}'; skipped.", roughessCsvBranchData.BranchName);
                        continue;
                    }
                }
                IFunction roughnessFunction;

                if (roughessCsvBranchData.RoughnessFunctionOfQ != null)
                {
                    roughnessFunction = roughnessSection.AddQRoughnessFunctionToBranch(branch, roughessCsvBranchData.RoughnessFunctionOfQ);
                }
                else if (roughessCsvBranchData.RoughnessFunctionOfH != null)
                {
                    roughnessFunction = roughnessSection.AddHRoughnessFunctionToBranch(branch, roughessCsvBranchData.RoughnessFunctionOfH);
                }
                else
                {
                    roughnessFunction = roughessCsvBranchData.ConstantRoughness;
                }

                roughnessSection.UpdateCoverageForFunction(branch, roughnessFunction, roughessCsvBranchData.RoughnessType);
            }
        }

        public static Function DefineConstantFunction()
        {
            var chainage = new Variable<double>("Chainage");
            var roughness = new Variable<double>("Roughness");

            return new Function { Arguments = { chainage }, Components = { roughness } };
        }
    }
}