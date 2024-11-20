using System.IO;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations.PostBehaviours
{
    /// <summary>
    /// <see cref="IIniPostOperationBehaviour"/> defines a
    /// single behaviour that should be executed by the <see cref="IIniFileOperator"/>
    /// after the <see cref="IIniPropertyBehaviour"/> have been executed.
    /// </summary>
    public interface IIniPostOperationBehaviour
    {
        /// <summary>
        /// Invokes this <see cref="IIniPostOperationBehaviour"/>
        /// with the data of the <see cref="IIniFileOperator.Invoke"/>.
        /// </summary>
        /// <param name="sourceFileStream">The source file stream.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="iniData">The parsed INI data.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter but the <paramref name="logHandler"/> is
        /// <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when no file name can be obtained from <paramref name="sourceFilePath"/>.
        /// </exception>
        void Invoke(Stream sourceFileStream, 
                    string sourceFilePath, 
                    IniData iniData,
                    ILogHandler logHandler);
        
    }
}