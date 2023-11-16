using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class CmpFileImporter: BoundaryDataImporterBase, IFileImporter
    {
        private static ILog Log = LogManager.GetLogger(typeof (CmpFileImporter));

        #region IFileImporter

        public string Name
        {
            get { return "Boundary data from .cmp file"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return "Boundary data"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.TextDocument; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(BoundaryCondition); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel { get { return false; } }

        public override string FileFilter { get { return "Harmonic series file|*.cmp"; } }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get { return true; } }

        public object ImportItem(string path, object target = null)
        {
            var boundaryCondition = target as IBoundaryCondition;

            if (boundaryCondition == null) return target;

            var fileReader = new CmpFile();
            try
            {
                var harmonicComponents = fileReader.Read(path);
                
                switch (boundaryCondition.DataType)
                {
                    case BoundaryConditionDataType.AstroComponents:
                        var astroComponents = harmonicComponents.Where(h => h.Name != null).ToList();
                        foreach (var data in SeriesToFill(boundaryCondition))
                        {
                            data.BeginEdit("Importing data to boundary condition");
                            FunctionHelper.SetValuesRaw(data.Arguments[0], astroComponents.Select(h => h.Name));
                            FunctionHelper.SetValuesRaw(data.Components[0], astroComponents.Select(h => h.Amplitude));
                            FunctionHelper.SetValuesRaw(data.Components[1], astroComponents.Select(h => h.Phase));
                            data.EndEdit();
                        }
                        break;
                    case BoundaryConditionDataType.AstroCorrection:
                        astroComponents = harmonicComponents.Where(h => h.Name != null).ToList();
                        foreach (var data in SeriesToFill(boundaryCondition))
                        {
                            data.BeginEdit("Importing data to boundary condition");
                            FunctionHelper.SetValuesRaw(data.Arguments[0], astroComponents.Select(h => h.Name));
                            FunctionHelper.SetValuesRaw(data.Components[0], astroComponents.Select(h => h.Amplitude));
                            FunctionHelper.SetValuesRaw(data.Components[2], astroComponents.Select(h => h.Phase));
                            data.EndEdit();
                        }
                        break;
                    case BoundaryConditionDataType.Harmonics:
                        foreach (var data in SeriesToFill(boundaryCondition))
                        {
                            data.BeginEdit("Importing data to boundary condition");
                            var orderedComponents = harmonicComponents.OrderBy(h => h.Frequency);
                            FunctionHelper.SetValuesRaw(data.Arguments[0], orderedComponents.Select(h => h.Frequency));
                            FunctionHelper.SetValuesRaw(data.Components[0], orderedComponents.Select(h => h.Amplitude));
                            FunctionHelper.SetValuesRaw(data.Components[1], orderedComponents.Select(h => h.Phase));
                            data.EndEdit();
                        }
                        break;
                    case BoundaryConditionDataType.HarmonicCorrection:
                        foreach (var data in SeriesToFill(boundaryCondition))
                        {
                            data.BeginEdit("Importing data to boundary condition");
                            var orderedComponents = harmonicComponents.OrderBy(h => h.Frequency);
                            FunctionHelper.SetValuesRaw(data.Arguments[0], orderedComponents.Select(h => h.Frequency));
                            FunctionHelper.SetValuesRaw(data.Components[0], orderedComponents.Select(h => h.Amplitude));
                            FunctionHelper.SetValuesRaw(data.Components[2], orderedComponents.Select(h => h.Phase));
                            data.EndEdit();
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Cmp-file import failed: {0}", e.Message);
            }
            return boundaryCondition;
        }

        #endregion

        public override IEnumerable<BoundaryConditionDataType> ForcingTypes
        {
            get
            {
                yield return BoundaryConditionDataType.AstroComponents;
                yield return BoundaryConditionDataType.AstroCorrection;
                yield return BoundaryConditionDataType.Harmonics;
                yield return BoundaryConditionDataType.HarmonicCorrection;
            }
        }

        public override void Import(string fileName, FlowBoundaryCondition boundaryCondition)
        {
            ImportItem(fileName, boundaryCondition);
        }

        public override bool CanImportOnBoundaryCondition(FlowBoundaryCondition boundaryCondition)
        {
            return ForcingTypes.Contains(boundaryCondition.DataType);
        }
    }
}
