using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using log4net;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Importers
{
    public class WaveBoundaryFileImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaveBoundaryFileImporter));
        
        public string Name 
        {
            get { return "Wave Boundary Conditions (*.bcw)"; }
        }

        public string Category { get; private set; }
        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes 
        {
            get { yield return typeof (IList<WaveBoundaryCondition>); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel 
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "Wave Boundary Condition Files (*.bcw;*.sp2)|*.bcw;*.sp2"; }
        }

        public bool OpenViewAfterImport
        {
            get { return true; }
        }

        public object ImportItem(string path, object target = null)
        {
            var conditions = target as IList<WaveBoundaryCondition>;
            if (conditions == null)
                return null;

            var data = new BcwFile().Read(path);
            foreach (var boundaryData in data)
            {
                var name = boundaryData.Key;
                var functions = boundaryData.Value;

                var bc = conditions.FirstOrDefault(c => c.Name == name);
                if (bc == null)
                {
                    Log.WarnFormat("Could not import boundary condition; no boundary with name {0} found", name);
                    continue;
                }

                if (bc.DataType != BoundaryConditionDataType.ParametrizedSpectrumTimeseries)
                {
                    Log.WarnFormat("Could not import boundary condition; boundary {0} is not of type {1}", name,
                                   BoundaryConditionDataType.ParametrizedSpectrumTimeseries);
                    continue;
                }

                if (bc.DataPointIndices.Count != functions.Count)
                {
                    Log.WarnFormat(
                        "Could not import data onto boundary {0}; number of" +
                        " timeseries in file ({1}) did not match the number of support points ({2})",
                        bc.Name, functions.Count, bc.PointData.Count);
                    continue;
                }

                SetDataOnBoundaryCondition(bc, functions);
            }

            return target;
        }

        [InvokeRequired]
        private void SetDataOnBoundaryCondition(WaveBoundaryCondition bc, IList<IFunction> functions)
        {
            List<int> dpindices = bc.DataPointIndices.ToList();
            bc.ClearData();

            int functionIndex = 0;
            foreach (var index in dpindices)
            {
                bc.SetTimeseriesToSupportPoint(index, functions[functionIndex++]);
            }
        }

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
    }
}