using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// Use this binding list if all functions are actually filtered versions of the same function 
    /// (and thus you do not want to process events multiple times): this binding list only processes 
    /// changes for first function. 
    /// </summary>
    public class SplitFunctionsBindingList : MultipleFunctionBindingList, DelftTools.Utils.Editing.IEditableObject
    {
        public SplitFunctionsBindingList(IEnumerable<IFunction> functions)
            : base(functions)
        {
        }

        protected override object AddNewCoreForFunction(IFunction function)
        {
            if (function == Function)
            {
                return base.AddNewCoreForFunction(function);
            }
            return null;
        }

        protected override void OnFunctionValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Reset && sender != Function)
            {
                return; //do no reset for anything but the first
            }

            base.OnFunctionValuesChanged(sender, e);
        }

        protected override void FireFunctionValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Reset && sender != Function)
            {
                return; //do no reset for anything but the first
            }

            base.FireFunctionValuesChanged(sender, e);
        }

        protected override void OnListChangedForFunction(ListChangedEventArgs e, IFunction function)
        {
            if (function == Function)
            {
                base.OnListChangedForFunction(e, function);
            }
        }
        
        #region IEditableObject

        public bool IsEditing
        {
            get { return Function?.IsEditing ?? false; }
        }

        public bool EditWasCancelled
        {
            get { return Function?.EditWasCancelled ?? false; }
        }

        public IEditAction CurrentEditAction
        {
            get { return Function?.CurrentEditAction; }
        }

        public void BeginEdit(string action)
        {
            Function?.BeginEdit(action);
        }

        public void BeginEdit(IEditAction action)
        {
            Function?.BeginEdit(action);
        }

        public void EndEdit()
        {
            Function?.EndEdit();
        }

        public void CancelEdit()
        {
            Function?.CancelEdit();
        }

        #endregion
    }
}