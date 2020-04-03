using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// A mathematical expression that can have multiple inputs and a defined expression
    /// </summary>
    /// <seealso cref="RtcBaseObject" />
    /// <seealso cref="IInput" />
    [Entity]
    public class MathematicalExpression : RtcBaseObject, IInput
    {
        private IEventedList<IInput> inputs;
        private readonly Dictionary<char, string> inputMapping = new Dictionary<char, string>();

        /// <summary>
        /// Gets or sets the set point.
        /// </summary>
        public string SetPoint { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MathematicalExpression"/> class.
        /// </summary>
        public MathematicalExpression()
        {
            Inputs = new EventedList<IInput>();
            Expression = string.Empty;
        }

        /// <summary>
        /// Gets or sets the inputs of the mathematical expression.
        /// </summary>
        public IEventedList<IInput> Inputs
        {
            get => inputs;
            set
            {
                if (inputs == value)
                {
                    return;
                }

                if (inputs != null)
                {
                    inputs.CollectionChanged -= OnInputCollectionChanged;
                    inputs.ForEach(Unsubscribe);
                }

                inputs = value;

                if (inputs != null)
                {
                    inputs.CollectionChanged += OnInputCollectionChanged;
                    inputs.ForEach(Subscribe);
                }

                ResetMapping();
            }
        }

        /// <summary>
        /// Gets the input mapping in which capital letters are mapped
        /// in alphabetical order to an input name.
        /// </summary>
        /// <remarks>
        /// The mapping contains unique values.
        /// </remarks>
        public IReadOnlyDictionary<char, string> InputMapping => inputMapping;

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        public string Expression { get; set; }

        public override object Clone()
        {
            var mathematicalExpression = new MathematicalExpression();
            mathematicalExpression.CopyFrom(this);
            return mathematicalExpression;
        }

        public override void CopyFrom(object source)
        {
            var mathematicalExpression = source as MathematicalExpression;
            if (mathematicalExpression != null)
            {
                base.CopyFrom(source);

                Expression = mathematicalExpression.Expression;
            }
        }

        private void OnInputCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IInput[] newInputs = e.NewItems?.OfType<IInput>().ToArray();
            newInputs?.ForEach(Subscribe);
            newInputs?.ForEach(AddMappedInput);

            IInput[] oldInputs = e.OldItems?.OfType<IInput>().ToArray();
            oldInputs?.ForEach(Unsubscribe);
            oldInputs?.ForEach(RemoveMappedInput);
        }

        private void AddMappedInput(IInput input)
        {
            string inputName = input.Name;
            if (inputMapping.Values.Contains(inputName))
            {
                return;
            }

            int parameterCount = inputMapping.Count;
            char newParameter = ToChar(parameterCount);
            inputMapping[newParameter] = inputName;
        }

        private void RemoveMappedInput(IInput input)
        {
            string inputName = input.Name;
            if (Inputs.Any(i => i.Name == inputName))
            {
                return;
            }

            ResetMapping();
        }

        private void ResetMapping()
        {
            inputMapping.Clear();
            string[] inputNames = Inputs.Select(i => i.Name)
                                        .Distinct().ToArray();

            for (var i = 0; i < inputNames.Length; i++)
            {
                inputMapping[ToChar(i)] = inputNames[i];
            }
        }

        private void Subscribe(IInput input)
        {
            ((INotifyPropertyChange) input).PropertyChanged += OnInputPropertyChanged;
        }

        private void Unsubscribe(IInput input)
        {
            ((INotifyPropertyChange) input).PropertyChanged -= OnInputPropertyChanged;
        }

        private void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IInput.Name))
            {
                ResetMapping();
            }
        }

        private static char ToChar(int i)
        {
            return Convert.ToChar(i + 65);
        }
    }
}