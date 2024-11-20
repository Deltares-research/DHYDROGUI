using System;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO.HydFileElement
{
    /// <summary>
    /// Base implementation of a key-value element in a hydrodynamics file (.hyd file).
    /// </summary>
    /// <typeparam name="T"> Type of the property value. </typeparam>
    public class KeyValueElement<T> : IHydFileElement
    {
        private readonly Action<HydFileData, T> dataSetAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueElement{T}"/> class.
        /// </summary>
        /// <param name="func">
        /// The function that applies <see cref="Value"/> to an instance
        /// of <see cref="HydFileData"/>. Cannot be null.
        /// </param>
        public KeyValueElement(Action<HydFileData, T> func)
        {
            dataSetAction = func;
        }

        public T Value { get; protected set; }

        public void SetDataTo(HydFileData hydFileData)
        {
            dataSetAction(hydFileData, Value);
        }

        public IHydFileElement ParseValue(string textToParse)
        {
            Value = HydFileStringValueParser.Parse<T>(textToParse);
            return this;
        }
    }
}