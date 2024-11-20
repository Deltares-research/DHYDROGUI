using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.Roughness
{
    public class RoughnessFunctionConvertor
    {
        /// <summary>
        /// convert internal format of function of
        ///  roughness = F(chainage, Q or H)
        /// to
        ///  [roughness, chainage1, chainage2, ..] = F(Q or H) 
        /// </summary>
        /// <param name="qFunction"></param>
        /// <param name="functionOfColumnTitle"></param>
        /// <returns></returns>
        public static IFunction ConvertFunctionOfToTableWithChainageColumns(IFunction qFunction, string functionOfColumnTitle)
        {
            var roughnessFunction = new Function();
            var q = new Variable<double>(functionOfColumnTitle);
            roughnessFunction.Arguments.Add(q);

            foreach (var chainage in qFunction.Arguments[0].Values)
            {
                var chainageColumn = new Variable<double>(chainage.ToString());
                roughnessFunction.Components.Add(chainageColumn);
            }
            foreach (var Q in qFunction.Arguments[1].Values)
            {
                var values = new List<double>();
                foreach (var chainage in qFunction.Arguments[0].Values)
                {
                    values.Add((double)qFunction[chainage, Q]);
                }
                roughnessFunction[Q] = values.ToArray();
            }
            return roughnessFunction;
        }

        /// <summary>
        /// reverse of ConvertFunctionOfToTableWithChainageColumns
        /// </summary>
        /// <param name="viewData"></param>
        /// <param name="data"></param>
        public static void ConvertTableWithChainageColumnsToFunctionOf(IFunction viewData, IFunction data)
        {
            // since we are using the clone, cancel does nothing
            var chainages = (IMultiDimensionalArray)data.Arguments[0].Values.Clone();
            //clear any previous Q
            data.Arguments[1].Clear();
            var qs = viewData.Arguments[0].Values;
            for (var i = 0; i < chainages.Count; i++)
            {
                var rougnesses = viewData.Components[i].Values;
                for (var j = 0; j < qs.Count; j++)
                {
                    data[chainages[i], qs[j]] = rougnesses[j];
                }
            }
        }
    }
}