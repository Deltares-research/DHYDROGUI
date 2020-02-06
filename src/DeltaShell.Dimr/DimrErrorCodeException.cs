using System;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr.Properties;

namespace DeltaShell.Dimr
{
    public class DimrErrorCodeException : Exception
    {
        public DimrErrorCodeException(ActivityStatus status, int errorCode)
           
        {
            ErrorCode = errorCode;
            Status = status;
        }

        public override string Message =>string.Format(Resources.DimrErrorCodeException_During__0__the_model_run_something_went_wrong_Error_Code__1__sent_by_the_computational_core, Status.ToString().ToLower(), ErrorCode);

        private int ErrorCode { get; }
        
        private ActivityStatus Status { get; }
    }
}
