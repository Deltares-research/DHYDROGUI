using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Utils.Editing;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils
{
    /// <summary>
    /// Function type creator
    /// </summary>
    public class FunctionTypeCreator : IFunctionTypeCreator
    {
        private readonly string name;
        private readonly Func<IFunction, bool> checkFunction;
        private readonly Func<IFunction, IFunction> transformFunction;
        private readonly Func<IFunction, double> getDefaultValueFunction;
        private readonly Action<IFunction, double> setDefaultValueFunction;
        private readonly Func<IFunction, string> getUnitFunction;
        private readonly Action<IFunction, string> setUnitFunction;
        private readonly Func<IFunction, string> getUrlFunction;
        private readonly Action<IFunction, string> setUrlFunction;
        private readonly Func<IFunction, bool> isAllowedFunction;

        /// <summary>
        /// Creates a function type creator
        /// </summary>
        /// <param name="name"> The name of the function type creator </param>
        /// <param name="checkFunction"> The function to check the type of function arguments </param>
        /// <param name="transformFunction"> The function to transform function arguments </param>
        /// <param name="getDefaultValueFunction"> The function to get the default value of function arguments </param>
        /// <param name="setDefaultValueFunction"> The function to set the default value of function arguments </param>
        /// <param name="getUnitFunction"> The function to get the unit of function arguments </param>
        /// <param name="setUnitFunction"> The function to set the unit of function arguments </param>
        /// <param name="isAllowedFunction">
        /// The function to determine if the creator can be
        /// used for a given function or not.
        /// </param>
        public FunctionTypeCreator(string name,
                                   Func<IFunction, bool> checkFunction,
                                   Func<IFunction, IFunction> transformFunction,
                                   Func<IFunction, double> getDefaultValueFunction,
                                   Action<IFunction, double> setDefaultValueFunction,
                                   Func<IFunction, string> getUnitFunction,
                                   Action<IFunction, string> setUnitFunction,
                                   Func<IFunction, string> getUrlFunction,
                                   Action<IFunction, string> setUrlFunction,
                                   Func<IFunction, bool> isAllowedFunction)
        {
            this.name = name;
            this.checkFunction = checkFunction;
            this.transformFunction = transformFunction;
            this.getDefaultValueFunction = getDefaultValueFunction;
            this.setDefaultValueFunction = setDefaultValueFunction;
            this.getUnitFunction = getUnitFunction;
            this.setUnitFunction = setUnitFunction;
            this.getUrlFunction = getUrlFunction;
            this.setUrlFunction = setUrlFunction;
            this.isAllowedFunction = isAllowedFunction;
        }

        public string FunctionTypeName => name;

        /// <summary>
        /// Replaces the function in a collection for a new one created with a creator.
        /// </summary>
        /// <param name="functionCollection"> The function collection containing the function to be replaced. </param>
        /// <param name="function"> The function to be replaced. </param>
        /// <param name="creator">
        /// The creator handling the creation of a new <see cref="IFunction"/>
        /// based on <paramref name="function"/>.
        /// </param>
        /// <param name="dataOwner"> The data owner or <paramref name="function"/>. </param>
        /// <param name="previousType">
        /// Optional textual description of the original function type.
        /// this is only used for undo/redo messaging.
        /// </param>
        /// <returns> The newly created function that has been inserted into <paramref name="functionCollection"/>. </returns>
        public static IFunction ReplaceFunctionUsingCreator(IList<IFunction> functionCollection,
                                                            IFunction function, IFunctionTypeCreator creator,
                                                            IEditableObject dataOwner, string previousType = "")
        {
            int oldFunctionIndex = functionCollection.IndexOf(function);

            IFunction newfunction = creator.TransformToFunctionType(function);

            string editActionMessage = string.Format("Changing function type of {0} {1}to {2}", function, previousType,
                                                     creator.FunctionTypeName);
            dataOwner.BeginEdit(editActionMessage);

            functionCollection.RemoveAt(oldFunctionIndex);
            functionCollection.Insert(oldFunctionIndex, newfunction);

            dataOwner.EndEdit();

            return newfunction;
        }

        public bool IsThisFunctionType(IFunction function)
        {
            return checkFunction(function);
        }

        public IFunction TransformToFunctionType(IFunction function)
        {
            return transformFunction(function);
        }

        public double GetDefaultValueForFunction(IFunction function)
        {
            return !checkFunction(function) ? double.NaN : getDefaultValueFunction(function);
        }

        public void SetDefaultValueForFunction(IFunction function, double defaultValue)
        {
            if (checkFunction(function))
            {
                setDefaultValueFunction(function, defaultValue);
            }
        }

        public string GetUnitForFunction(IFunction function)
        {
            return !checkFunction(function) ? string.Empty : getUnitFunction(function);
        }

        public void SetUnitForFunction(IFunction function, string unit)
        {
            if (checkFunction(function))
            {
                setUnitFunction(function, unit);
            }
        }

        public string GetUrlForFunction(IFunction function)
        {
            return !checkFunction(function) ? string.Empty : getUrlFunction(function);
        }

        public void SetUrlForFunction(IFunction function, string url)
        {
            if (checkFunction(function))
            {
                setUrlFunction(function, url);
            }
        }

        public bool IsAllowed(IFunction function)
        {
            return isAllowedFunction(function);
        }
    }
}