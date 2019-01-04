using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public class WaveBoundaryConditionEditorController : BoundaryConditionEditorController
    {
        public const string WaveQuantityName = "wave energy density";

        public override void OnBoundaryConditionSelectionChanged(IBoundaryCondition boundaryCondition)
        {
            var selectedPointIndex = Editor.SelectedSupportPointIndex;

            var view = (WaveBoundaryConditionDataView) Editor.BoundaryConditionDataView;
            if (view != null)
            {
                Editor.SelectedSupportPointChanged -= view.OnSelectedPointChanged;
                view.Data = null;
            }
            view = new WaveBoundaryConditionDataView
                {
                    Data = boundaryCondition, 
                    SelectedPointIndex = selectedPointIndex,
                    ImportIntoModelDirectory = ImportIntoModelDirectory,
                    Model = Model
                    
                };
            Editor.SelectedSupportPointChanged += view.OnSelectedPointChanged;
            Editor.BoundaryConditionDataView = view;
            Editor.ChildViews.Add(view);
        }

        public Func<string, string> ImportIntoModelDirectory { private get; set; }

        public override IEnumerable<string> SupportedProcessNames
        {
            get { yield return WaveBoundaryCondition.WaveProcessName; }
        }

        public WaveModel Model { get; set; }

        public override IEnumerable<string> GetVariablesForProcess(string category)
        {
            yield return WaveQuantityName;
        }

        public override IEnumerable<string> GetAllowedVariablesFor(string cartegory,
                                                                   BoundaryConditionSet boundaryConditions)
        {
            if (boundaryConditions != null &&
                boundaryConditions.BoundaryConditions.OfType<WaveBoundaryCondition>().Any())
            {
                yield break;
            }
            yield return WaveQuantityName;
        }

        public override IEnumerable<BoundaryConditionDataType> GetSupportedDataTypesForVariable(string variable)
        {
            yield return BoundaryConditionDataType.ParameterizedSpectrumConstant;
            yield return BoundaryConditionDataType.ParameterizedSpectrumTimeseries;
            yield return BoundaryConditionDataType.SpectrumFromFile;
        }
    }
}
