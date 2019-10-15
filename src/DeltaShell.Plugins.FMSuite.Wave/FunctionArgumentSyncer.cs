using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave
{

    /// <summary>
    /// Syncing of first function argument on addition, removal, and replacement of values
    /// </summary>
    /// <typeparam name="T">The type of the first argument</typeparam>
    public class FunctionArgumentSyncer<T> : IDisposable
    {
        public IEventedList<IFunction> Functions { get; private set; }

        public FunctionArgumentSyncer()
        {
            Functions = new EventedList<IFunction>();
            Functions.CollectionChanged += FunctionsOnCollectionChanged;
        }

        private void FunctionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var arguments = ((IFunction) args.GetRemovedOrAddedItem()).Arguments;
            if (!arguments.Any()) return;
            if (arguments[0].ValueType != typeof(T)) return;

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add: 
                    ((IFunction)args.GetRemovedOrAddedItem()).Arguments[0].ValuesChanged += FunctionTimeValuesChanged;
                    OnFunctionAdded(((IFunction)args.GetRemovedOrAddedItem()));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ((IFunction)args.GetRemovedOrAddedItem()).Arguments[0].ValuesChanged -= FunctionTimeValuesChanged;
                    break;
                default:
                    return;
            }
        }

        private void FunctionTimeValuesChanged(object sender, FunctionValuesChangingEventArgs args)
        {
            var variable = sender as IVariable<T>;
            if (variable == null)
                return;

            switch (args.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    OnAddValues(args.Items.Cast<T>().ToList(), variable);
                    break;
                case NotifyCollectionChangeAction.Remove:
                    OnRemoveValues(args.Items.Cast<T>().ToList(), variable);
                    break;
                case NotifyCollectionChangeAction.Replace:
                    // use old index, the other functions will sort for themselves..
                    OnReplaceValues(args.Items.Cast<T>().ToList(), args.OldIndex, variable);
                    break;
                default:
                    throw new NotImplementedException("Function syncer does not support action: " +
                                                      args.Action);
            }
        }

        private void OnFunctionAdded(IFunction addedFunction)
        {
            if (Functions.Count == 1) return;

            var newValues = addedFunction.Arguments[0].GetValues<T>().ToList();
            var referenceValues = Functions.First().Arguments[0].GetValues<T>().ToList();

            // sync
            OnAddValues(newValues.Except(referenceValues).ToList(), addedFunction.Arguments[0]);

            addedFunction.Arguments[0].AddValues(referenceValues.Except(newValues).ToList());
        }

        private bool isRemoving;
        private void OnRemoveValues(IList<T> values, IVariable fromVariable)
        {
            if (!values.Any()) return;
            if (isRemoving) return;

            isRemoving = true;

            foreach (var function in Functions)
            {
                if (function.Arguments[0].Equals(fromVariable)) continue;
                function.RemoveValues(new VariableValueFilter<T>(function.Arguments[0], values));
            }

            isRemoving = false;
        }

        private bool isAdding;
        private void OnAddValues(IList<T> values, IVariable fromVariable)
        {
            if (!values.Any()) return;
            if (isAdding) return;

            isAdding = true;

            foreach (var function in Functions)
            {
                if (function.Arguments[0].Equals(fromVariable)) continue;
                function.Arguments[0].AddValues(values);
            }

            isAdding = false;
        }

        private bool isReplacing;
        private void OnReplaceValues(IList<T> values, int startIndex, IVariable fromVariable)
        {
            if (!values.Any()) return;
            if (isReplacing) return;

            isReplacing = true;

            foreach (var function in Functions)
            {
                if (function.Arguments[0].Equals(fromVariable)) continue;

                int index = 0;
                Enumerable.Range(startIndex, values.Count)
                          .ForEach(i => function.Arguments[0].Values[i] = values[index++]);
            }

            isReplacing = false;
        }

        public void Dispose()
        {
            Functions.CollectionChanged -= FunctionsOnCollectionChanged;
            Functions.ForEach(f => f.ValuesChanged -= FunctionTimeValuesChanged);
        }
    }
}