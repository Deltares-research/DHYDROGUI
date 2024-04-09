using System.IO;

namespace DHYDRO.Code
{
	/// <summary>
	/// Helper methods to create.
	/// </summary>
	public static class Create
    {
	    /// <summary>
	    /// Create the directory with the provided <paramref name="path" /> if it not exists.
	    /// </summary>
	    /// <param name="path"> The path of the directory to create. </param>
		public static void DirectoryIfNotExists(string path)
		{
			if (!Directory.Exists(path))
		    {
				Directory.CreateDirectory(path);
		    }
		}
		
	    /// <summary>
	    /// Create the parent directory of the provided <paramref name="path" />.
	    /// </summary>
	    /// <param name="path"> The path to the file or directory to create the parent directory of. </param>
	    public static void ParentDirectory(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
    }
}