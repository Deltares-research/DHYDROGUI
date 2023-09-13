using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Ini;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations.PostBehaviours
{
    /// <summary>
    /// <see cref="WriteSectionsPostOperationBehaviour"/> defines the post-behaviour
    /// that writes the provided <see cref="IniData"/>
    /// </summary>
    /// <seealso cref="IIniPostOperationBehaviour" />
    public sealed class WriteSectionsPostOperationBehaviour : IniPostOperationBehaviour
    {
        private readonly IIniWriter iniWriter;
        private readonly string goalDirectory;

        /// <summary>
        /// Creates a new <see cref="WriteSectionsPostOperationBehaviour"/>.
        /// </summary>
        /// <param name="iniWriter">The ini writer.</param>
        /// <param name="goalDirectory">The goal directory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public WriteSectionsPostOperationBehaviour(IIniWriter iniWriter,
                                                     string goalDirectory)
        {
            Ensure.NotNull(iniWriter, nameof(iniWriter));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));

            this.iniWriter = iniWriter;
            this.goalDirectory = goalDirectory;
        } 

        public override void Invoke(Stream sourceFileStream, 
                                    string sourceFilePath, 
                                    IniData iniData, 
                                    ILogHandler logHandler)
        {
            base.Invoke(sourceFileStream, sourceFilePath, iniData, logHandler);

            string writePath = Path.Combine(goalDirectory, Path.GetFileName(sourceFilePath));
            iniWriter.WriteIniFile(iniData, writePath);
        }
    }
}