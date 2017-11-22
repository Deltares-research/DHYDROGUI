using System.Xml.Linq;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public class RealTimeControlDimrConfigModelCouplerProvider : IDimrConfigModelCouplerProvider
    {
        public IDimrConfigModelCoupler CreateCoupler(IModel source, IModel target, ICompositeActivity sourceCoupler,
            ICompositeActivity targetCoupler)
        {
            var sourceRtcModel = source as IRealTimeControlModel;
            var targetRtcModel = target as IRealTimeControlModel;
            if (sourceRtcModel != null || targetRtcModel != null)
            {
                var workingDirectory = sourceRtcModel == null
                    ? targetRtcModel.ExplicitWorkingDirectory
                    : sourceRtcModel.ExplicitWorkingDirectory;

                return new DimrConfigModelCoupler(source, target, sourceCoupler, targetCoupler)
                {
                    AddAdditionalCouplerInfo = (x, nameSpace) =>
                    {
                        var loggerNode = new XElement(nameSpace + "logger");
                        var workingDirNode = new XElement(nameSpace + "workingDir", workingDirectory);
                        var outputFileNode = new XElement(nameSpace + "outputFile", RealTimeControlModel.OutputFileName);
                        loggerNode.Add(workingDirNode, outputFileNode);
                        x.Add(loggerNode);
                        return x;
                    }
                };
            }
            return null;
        }
    }
}