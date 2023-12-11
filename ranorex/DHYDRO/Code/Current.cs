using TS = Ranorex.Core.Testing.TestSuite;

namespace DHYDRO.Code
{
	/// <summary>
	/// This class contains global variables.
	/// </summary>
	public static class Current
    {
	    /// <summary>
	    /// The currently calibrated map transformation.
	    /// </summary>
	    public static Transformation MapTransformation = null;

	    /// <summary>
	    /// Gets the name of the currently running test case.
	    /// </summary>
	    public static string TestCaseName
        {
            get
            {
                var container = TS.CurrentTestContainer;
                while (!container.IsTestCase)
                {
	                container = container.ParentContainer;
                }

                return container.Name;
            }
        }

	    /// <summary>
	    /// Gets the input directory path used by the currently running Test Suite.
	    /// </summary>
	    public static string InputDirectory => GetParameter("InputDirectory");

	    /// <summary>
	    /// Gets the output directory path used by the currently running Test Suite.
	    /// </summary>
	    public static string OutputDirectory => GetParameter("OutputDirectory");

        private static string GetParameter(string key)
        {
            return TS.Current.Parameters[key];
        }
    }
}