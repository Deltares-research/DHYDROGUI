using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    public static class RunModelAcceptanceTestHelper
    {
        /// <summary>
        /// Check that all functions in all output function stores have values.
        /// </summary>
        /// <param name="fmModel">The model to check function stores of</param>
        public static void CheckFlowFMOutputFileStores(WaterFlowFMModel fmModel)
        {
            Assert.That(fmModel.OutputIsEmpty, Is.False);

            var fileStores = new List<IFunctionStore>
            {
                fmModel.Output1DFileStore,
                fmModel.OutputHisFileStore,
                fmModel.OutputFouFileStore,
                fmModel.OutputMapFileStore,
                fmModel.OutputClassMapFileStore
            };

            Assert.Multiple(() =>
                {
                    foreach (IFunctionStore store in fileStores)
                    {
                        if (store != null)
                        {
                            Assert.That(InvalidFunctionsInStore(store), Is.Empty,
                                        $"{store}@{fmModel}[{FunctionStorePath(store)}]");
                        }
                    }
                }
            );
        }

        /// <summary>
        /// Check that all functions in all output function stores of all RainfallRunoff and FlowFM models have values.
        /// </summary>
        /// <param name="hydroModel">The model to check function stores of</param>
        public static void CheckHydroModelOutputFileStores(HydroModel hydroModel)
        {
            foreach (RainfallRunoffModel rrModel in hydroModel.Models.OfType<RainfallRunoffModel>())
            {
                CheckRainfallRunoffOutputFileStores(rrModel);
            }

            foreach (WaterFlowFMModel fmModel in hydroModel.Models.OfType<WaterFlowFMModel>())
            {
                CheckFlowFMOutputFileStores(fmModel);
            }
        }

        /// <summary>
        /// A function is valid if its FunctionStore has a Path, and it has at least one Value.
        /// </summary>
        /// <param name="f">a function</param>
        /// <returns></returns>
        private static bool FunctionIsValid(IFunction f)
        {
            return FunctionStorePath(f.Store) != null && f.GetValues().Count != 0;
        }

        /// <summary>
        /// Returns the names of empty functions
        /// </summary>
        /// <param name="store"></param>
        /// <returns>a list of functions in the IFunctionStore without values</returns>
        private static IEnumerable<IFunction> InvalidFunctionsInStore(IFunctionStore store)
        {
            return store.Functions.Where(f => !FunctionIsValid(f));
        }

        /// <summary>
        /// Check that all functions in all output function stores have values.
        /// </summary>
        /// <param name="rrModel">The model to check function stores of</param>
        /// <remarks>
        /// Apparently for rainfall runoff models it is valid for output functions with a file store to have no
        /// values, so we check that at least one function has data.
        /// </remarks>
        private static void CheckRainfallRunoffOutputFileStores(RainfallRunoffModel rrModel)
        {
            Assert.That(rrModel.OutputIsEmpty, Is.False);
            Assert.That(rrModel.OutputFunctions.Where(FunctionIsValid).Any);
        }

        /// <summary>
        /// Get the path of the files that stores a function. The concrete classes implementing IFunctionStore do not have a
        /// shared interface that returns the Path of the file that stores the function. For concrete classes that implement
        /// IFileBased and IReadOnlyNetCdfFunctionStoreBase this function returns the Path defined on those interfaces.
        /// </summary>
        /// <param name="store">An IFunctionStore instance</param>
        /// <returns>the path to the file that stores the function, or null</returns>
        private static string FunctionStorePath(IFunctionStore store)
        {
            if (store is IFileBased)
            {
                return ((IFileBased)store).Path;
            }

            if (store is IReadOnlyNetCdfFunctionStoreBase)
            {
                return ((IReadOnlyNetCdfFunctionStoreBase)store).Path;
            }

            return null;
        }
    }
}