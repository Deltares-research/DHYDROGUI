using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;


namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    /// <summary>
    /// The <see cref="ShapeFileImporterFactory"/> provides the construction
    /// method to create new <see cref="ShapeFileImporter{TGeometry,TFeature2D}"/>.
    ///
    /// Furthermore, it provides a set of AfterCreateActions within the static
    /// nested class AfterFeatureCreateActions.
    /// </summary>
    public static class ShapeFileImporterFactory
    {
        /// <summary>
        /// Constructs a new <see cref="ShapeFileImporter{TGeometry,TFeature2D}"/>.
        /// </summary>
        /// <typeparam name="TGeometry">
        /// The type of the geometry of <typeparamref name="TFeature2D"/>.
        /// </typeparam>
        /// <typeparam name="TFeature2D"> The type of Feature2D constructed. </typeparam>
        /// <param name="afterFeatureCreateAction">
        /// An optional <see cref="Action"/> describing the read <see cref="IFeature"/>,
        /// the TFeature2D that is created upon import, and the set of target
        /// items. This function is executed after the creation of each element
        /// during the <see cref="ShapeFileImporter{TGeometry,TFeature2D}.OnImportItem"/>.
        /// It can be used to add additional data, or make modifications to the
        /// constructed feature.
        /// 
        /// <see cref="AfterFeatureCreateActions"/>
        /// describes a set of predefined AfterFeatureCreateActions
        /// </param>
        /// <returns>
        /// A new <see cref="ShapeFileImporter{TGeometry,TFeature2D}"/> with the
        /// provided <paramref name="afterFeatureCreateAction"/>.
        /// </returns>
        public static ShapeFileImporter<TGeometry, TFeature2D> Construct<TGeometry, TFeature2D>(
            Action<IFeature, TFeature2D, IEnumerable<TFeature2D>> afterFeatureCreateAction = null)
            where TGeometry : IGeometry
            where TFeature2D : IFeature
        {
            return new ShapeFileImporter<TGeometry, TFeature2D>(ShapeFileImporterHelper.Read<TGeometry>,
                                                                afterFeatureCreateAction);
        }


        /// <summary>
        /// A collection of AfterFeatureCreateActions used to compose ShapeFileImporters.
        /// </summary>
        public static class AfterFeatureCreateActions
        {
            /// <summary>
            /// Creates an action which executes <paramref name="actions"/> in
            /// order they are defined.
            /// </summary>
            /// <typeparam name="TFeature2D">
            /// The type of the Feature2D on which this action acts.
            /// </typeparam>
            /// <param name="actions"> The actions to be chained. </param>
            /// <returns>
            /// An action which executes <paramref name="actions"/> in order they are defined.
            /// </returns>
            public static Action<IFeature, TFeature2D, IEnumerable<TFeature2D>> Chain<TFeature2D>(
                params Action<IFeature, TFeature2D, IEnumerable<TFeature2D>>[] actions)
            {
                return (srcFeature, targetFeature, targets) => actions.ForEach(a => a?.Invoke(srcFeature,
                                                                                                    targetFeature, 
                                                                                                    targets));
            }

            /// <summary>
            /// An action to try and add the value of the "Name" attribute of
            /// <paramref name="srcFeature"/> to <paramref name="targetFeature"/>.Name.
            /// </summary>
            /// <typeparam name="TFeature2D">The type of the Feature2D.</typeparam>
            /// <param name="srcFeature">
            /// The source feature with which <paramref name="targetFeature"/> was constructed.
            /// </param>
            /// <param name="targetFeature">
            /// The feature constructed from <see cref="srcFeature"/>.
            /// </param>
            /// <param name="targets"> 
            /// The set of targets to which <paramref name="targetFeature"/> will be added.
            /// </param>
            /// <post-condition>
            /// IF "Name" IN srcFeature.Attributes THEN
            ///   (new) targetFeature.Name == srcFeature.Attributes["Name"]
            /// ELSE
            ///   (new) targetFeature.Name == "imported_feature"
            /// </post-condition>
            /// <remarks>
            /// If a feature already exists in targets with the same proposed name, it will
            /// be made unique by appending _{index}.
            /// </remarks>
            public static void TryAddName<TFeature2D>(IFeature srcFeature, 
                                                      TFeature2D targetFeature,
                                                      IEnumerable<TFeature2D> targets)
                where TFeature2D : IFeature, INameable
            {
                if (srcFeature.Attributes.ContainsKey("Name") && 
                    srcFeature.Attributes["Name"] is string name)
                {
                    targetFeature.Name = name;
                }
                else
                {
                    targetFeature.Name = "imported_feature";
                }

                // Ensure that we have an unique name.
                if (targets.Any(f => f.Name.Equals(targetFeature.Name)))
                {
                    targetFeature.Name = NamingHelper.GetUniqueName(targetFeature.Name + "_{0}", targets);
                }
            }

            /// <summary>
            /// An action to try and add the value of the "CrestWidth" attribute of
            /// <paramref name="srcFeature"/> to <paramref name="targetFeature"/>.CrestWidth.
            /// </summary>
            /// <typeparam name="TFeature2D"> The type of the Feature2D. </typeparam>
            /// <param name="srcFeature">
            /// The source feature with which <paramref name="targetFeature"/> was constructed.
            /// </param>
            /// <param name="targetFeature">
            /// The feature constructed from <see cref="srcFeature"/>.
            /// </param>
            /// <param name="_"> 
            /// The set of targets to which <paramref name="targetFeature"/> will be added.
            /// </param>
            /// <post-condition>
            /// IF "CrestWidth" IN srcFeature.Attributes THEN
            ///   (new) targetFeature.CrestWidth == srcFeature.Attributes["CrestWidth"]
            /// </post-condition>
            public static void TryAddCrestWidth<TFeature2D>(IFeature srcFeature, 
                                                            TFeature2D targetFeature,
                                                            IEnumerable<TFeature2D> _)
                where TFeature2D : Weir2D
            {
                if (srcFeature.Attributes.ContainsKey("CrestWidth") &&
                    srcFeature.Attributes["CrestWidth"] is double crestWidth)
                {
                    targetFeature.CrestWidth = crestWidth;
                }
            }

            /// <summary>
            /// An action to try and add the value of the "CrestLevel" attribute of
            /// <paramref name="srcFeature"/> to <paramref name="targetFeature"/>.CrestLevel.
            /// </summary>
            /// <typeparam name="TFeature2D"> The type of the Feature2D. </typeparam>
            /// <param name="srcFeature">
            /// The source feature with which <paramref name="targetFeature"/> was constructed.
            /// </param>
            /// <param name="targetFeature">
            /// The feature constructed from <see cref="srcFeature"/>.
            /// </param>
            /// <param name="_"> 
            /// The set of targets to which <paramref name="targetFeature"/> will be added.
            /// </param>
            /// <post-condition>
            /// IF "CrestLevel" IN srcFeature.Attributes THEN
            ///   (new) targetFeature.CrestLevel == srcFeature.Attributes["CrestLevel"]
            /// </post-condition>
            public static void TryAddCrestLevel<TFeature2D>(IFeature srcFeature, 
                                                            TFeature2D targetFeature,
                                                            IEnumerable<TFeature2D> _)
                where TFeature2D : Weir2D
            {
                if (srcFeature.Attributes.ContainsKey("CrestLevel") &&
                    srcFeature.Attributes["CrestLevel"] is double crestLevel)
                {
                    targetFeature.CrestLevel = crestLevel;
                }
            }

            /// <summary>
            /// An action to try and add the <see cref="IWeirFormula"/> described
            /// by the "FormulaName" attribute of <paramref name="srcFeature"/>
            /// to <paramref name="targetFeature"/>.WeirFormula.
            /// </summary>
            /// <typeparam name="TFeature2D"> The type of the Feature2D. </typeparam>
            /// <param name="srcFeature">
            /// The source feature with which <paramref name="targetFeature"/> was constructed.
            /// </param>
            /// <param name="targetFeature">
            /// The feature constructed from <see cref="srcFeature"/>.
            /// </param>
            /// <param name="_"> 
            /// The set of targets to which <paramref name="targetFeature"/> will be added.
            /// </param>
            /// <post-condition>
            /// IF "FormulaName" IN srcFeature.Attributes THEN
            ///   (new) targetFeature.WeirFormula.Name == srcFeature.Attributes["FormulaName"]
            /// </post-condition>
            public static void TryAddWeirFormula<TFeature2D>(IFeature srcFeature, 
                                                             TFeature2D targetFeature,
                                                             IEnumerable<TFeature2D> _)
                where TFeature2D : Weir2D
            {
                if (!srcFeature.Attributes.ContainsKey("FormulaName") ||
                    !(srcFeature.Attributes["FormulaName"] is string formulaName))
                {
                    return;
                }

                switch (formulaName)
                {
                    case "Simple weir (Weir)":
                        targetFeature.WeirFormula = new SimpleWeirFormula();
                        break;
                    case "Gated weir (Orifice)":
                        targetFeature.WeirFormula = new GatedWeirFormula(true);
                        break;
                    case "General structure":
                        targetFeature.WeirFormula = new GeneralStructureWeirFormula();
                        break;
                }
            }

            /// <summary>
            /// An action to try and add the value of the "Capacity" attribute of
            /// <paramref name="srcFeature"/> to <paramref name="targetFeature"/>.Capacity.
            /// </summary>
            /// <typeparam name="TFeature2D"> The type of the Feature2D. </typeparam>
            /// <param name="srcFeature">
            /// The source feature with which <paramref name="targetFeature"/> was constructed.
            /// </param>
            /// <param name="targetFeature">
            /// The feature constructed from <see cref="srcFeature"/>.
            /// </param>
            /// <param name="_"> 
            /// The set of targets to which <paramref name="targetFeature"/> will be added.
            /// </param>
            /// <post-condition>
            /// IF "Capacity" IN srcFeature.Attributes THEN
            ///   (new) targetFeature.Capacity == srcFeature.Attributes["Capacity"]
            /// </post-condition>
            public static void TryAddCapacity<TFeature2D>(IFeature srcFeature, 
                                                          TFeature2D targetFeature,
                                                          IEnumerable<TFeature2D> _)
                where TFeature2D : Pump2D
            {
                if (srcFeature.Attributes.ContainsKey("Capacity") &&
                    srcFeature.Attributes["Capacity"] is double capacity)
                {
                    targetFeature.Capacity = capacity;
                }
            }
        }
    }
}
