using System;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr.Properties;

namespace DeltaShell.Dimr
{
    /// <summary>
    /// Specific exception, which can be thrown if Dimr returns a non-zero integer.
    /// </summary>
    [Serializable]
    public class DimrErrorCodeException : Exception
    {
        public DimrErrorCodeException(ActivityStatus status, int errorCode)
           
        {
            ErrorCode = errorCode;
            Status = status;
        }

        /// <summary>
        /// Override this property, so that all exceptions of this type
        /// will have the same messages based on the status of the model
        /// and the error code. 
        /// </summary>
        public override string Message =>string.Format(Resources.DimrErrorCodeException_During__0__the_model_something_went_wrong_Error_Code__1__has_been_detected_Please_inspect_your_diagnostic_files, Status.ToString().ToLower(), ErrorCode);

        private int ErrorCode { get; }
        
        private ActivityStatus Status { get; }
    }
}
