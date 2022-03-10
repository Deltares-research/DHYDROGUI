using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    /// <summary>
    /// Data presentation class for weirView 
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class WeirViewData 
    {
        private IDictionary<string, CrestShape> crestShapeDictionary;
        private IDictionary<Type, IWeirFormula> weirFormulaDictionary;
        private IDictionary<string, Type> weirFormulaTypeDictionary;

        public WeirViewData(IEnumerable<IWeirFormula> supportedFormulas)
        {
            SetDictionaries(supportedFormulas);
        }

        public IDictionary<string, Type> GetWeirFormulaTypes()
        {
            return weirFormulaTypeDictionary;
        }

        public Type GetWeirFormulaType(string weirFormulaName)
        {
            return weirFormulaTypeDictionary[weirFormulaName];
        }

        public CrestShape GetCrestShape(string shapeName)
        {
            return crestShapeDictionary[shapeName];
        }
        
        public IWeirFormula GetWeirCurrentFormula(Type weirType)
        {
            return weirFormulaDictionary[weirType];
        }

        public IDictionary<string, CrestShape> GetCrestShapes()
        {
            return crestShapeDictionary;
        }

        private void SetDictionaries(IEnumerable<IWeirFormula> supportedFormulas)
        {
            weirFormulaDictionary = supportedFormulas != null
                                        ? supportedFormulas.ToDictionary(f => f.GetType(), f => f)
                                        : new Dictionary<Type, IWeirFormula>
                                            {
                                                {typeof (FreeFormWeirFormula), new FreeFormWeirFormula()},
                                                //{typeof (GatedWeirFormula), new GatedWeirFormula()},//not yet implemented in the kernel
                                                //{typeof (PierWeirFormula), PierWeirFormula.CreateDefault()},//not yet implemented in the kernel
                                                //{typeof (RiverWeirFormula), RiverWeirFormula.CreateDefault()},//not yet implemented in the kernel
                                                {typeof (SimpleWeirFormula), new SimpleWeirFormula()},
                                                {typeof (GeneralStructureWeirFormula), new GeneralStructureWeirFormula()}
                                            };
            
            // synchronize settings from formula to common properties: this is a hack due to design error

            weirFormulaTypeDictionary = weirFormulaDictionary.ToDictionary(kvp => kvp.Value.Name, kvp=>kvp.Key);
                

            crestShapeDictionary = new Dictionary<string, CrestShape>
                                       {
                                           {"Sharp crested", CrestShape.Sharp},
                                           {"Broad crested", CrestShape.Broad},
                                           {"Round crested", CrestShape.Round},
                                           {"Triangular crested", CrestShape.Triangular}
                                       };
        }

        public void UpdateDataWithWeir(IWeir weir)
        {
            // Update dictionary with WeirFormula information
            weirFormulaDictionary[weir.WeirFormula.GetType()] = weir.WeirFormula;
            weirFormulaTypeDictionary = weirFormulaDictionary.ToDictionary(kvp => kvp.Value.Name, kvp => kvp.Key);

        }

        
        /// <summary>
        /// Validates the properties against the formula.  
        /// </summary>
        /// <param name="weirFormula"></param>
        /// <param name="crestShape"></param>
        /// <returns></returns>
        public static WeirViewValidationResult ValidateData(IWeirFormula weirFormula, CrestShape crestShape)
        {
            const bool freeFormValid = false;
            const bool gatedValid = false;
            bool crestShapeValid = false;
            if (weirFormula is SimpleWeirFormula)
            {
                
                crestShapeValid = crestShape == CrestShape.Sharp;
            }
            if (weirFormula is RiverWeirFormula)
            {
                crestShapeValid = true;
            }

            if (weirFormula is GatedWeirFormula)
            {
                crestShapeValid = crestShape == CrestShape.Sharp;
            }
            if (weirFormula is PierWeirFormula)
            {
                crestShapeValid = true;
            }
            if (weirFormula is FreeFormWeirFormula)
            {
                crestShapeValid = true;
            }
            return new WeirViewValidationResult(freeFormValid, gatedValid, crestShapeValid);
        }

        public string GetWeirFormulaTypeName(IWeirFormula formula)
        {
            return weirFormulaTypeDictionary
                .Where(kvp => kvp.Value == formula.GetType())
                .Select(kvp => kvp.Key).FirstOrDefault();
        }

        public string GetCrestShapeName(CrestShape shape)
        {
            return crestShapeDictionary
                .Where(kvp => kvp.Value == shape)
                .Select(kvp => kvp.Key).FirstOrDefault();
        }
    }
}