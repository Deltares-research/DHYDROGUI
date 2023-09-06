using System.IO;
using DeltaShell.NGHS.IO.Ini;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours
{
    /// <summary>
    /// <see cref="IDelftIniPostOperationBehaviour"/> defines a
    /// single behaviour that should be executed by the <see cref="IDelftIniFileOperator"/>
    /// after the <see cref="IDelftIniPropertyBehaviour"/> have been executed.
    /// </summary>
    public interface IDelftIniPostOperationBehaviour
    {
        /// <summary>
        /// Invokes this <see cref="IDelftIniPostOperationBehaviour"/>
        /// with the data of the <see cref="IDelftIniFileOperator.Invoke"/>.
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