using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public abstract class BoundaryConditionEditorController: IDisposable
    {
        virtual public BoundaryConditionEditor Editor { protected get; set; }

        public abstract void OnBoundaryConditionSelectionChanged(IBoundaryCondition boundaryCondition);

        public abstract IEnumerable<string> SupportedProcessNames { get; }

        public abstract IEnumerable<string> GetVariablesForProcess(string category);

        public abstract IEnumerable<string> GetAllowedVariablesFor(string category,
                                                                   BoundaryConditionSet boundaryConditions); 

        public virtual string GetVariableDescription(string variable, string category = null)
        {
            return variable ?? "";
        }

        public abstract IEnumerable<BoundaryConditionDataType> GetSupportedDataTypesForVariable(string variable);

        // Overriden by flow to keep the 'correct' ordering: first flow bc's, then salinity and temperature.
        public virtual void InsertBoundaryCondition(BoundaryConditionSet boundaryConditions,
            IBoundaryCondition boundaryCondition)
        {
            boundaryConditions.BoundaryConditions.Add(boundaryCondition);
        }

        public IEnumerable<BoundaryConditionDataType> AllSupportedDataTypes
        {
            get
            {
                return SupportedProcessNames.SelectMany(GetVariablesForProcess)
                                            .SelectMany(GetSupportedDataTypesForVariable)
                                            .Distinct();
            }
        }

        public virtual void Dispose(){}
    }
}