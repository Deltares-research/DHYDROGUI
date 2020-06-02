using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Importers
{
    public class WaveBoundaryFileImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaveBoundaryFileImporter));

        public string Name => "Wave Boundary Conditions (*.bcw)";

        public string Category { get; private set; }
        public string Description => string.Empty;
        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IList<WaveBoundaryCondition>);
            }
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => "Wave Boundary Condition Files (*.bcw;*.sp2)|*.bcw;*.sp2";

        public bool OpenViewAfterImport => true;

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            var conditions = target as IList<WaveBoundaryCondition>;
            if (conditions == null)
            {
                return null;
            }

            IDictionary<string, List<IFunction>> data = new BcwFile().Read(path);
            foreach (KeyValuePair<string, List<IFunction>> boundaryData in data)
            {
                string name = boundaryData.Key;
                List<IFunction> functions = boundaryData.Value;

                WaveBoundaryCondition bc = conditions.FirstOrDefault(c => c.Name == name);
                if (bc == null)
                {
                    Log.WarnFormat("Could not import boundary condition; no boundary with name {0} found", name);
                    continue;
                }

                if (bc.DataType != BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
                {
                    Log.WarnFormat("Could not import boundary condition; boundary {0} is not of type {1}", name,
                                   BoundaryConditionDataType.ParameterizedSpectrumTimeseries);
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

            var functionIndex = 0;
            foreach (int index in dpindices)
            {
                bc.SetTimeSeriesAtSupportPoint(index, functions[functionIndex++]);
            }
        }
    }
}