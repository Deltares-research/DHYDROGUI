using System;
using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public class FileReadingException : Exception
    {
        public FileReadingException(string message)
            : base(message) {}

        public FileReadingException(string message, Exception inner)
            : base(message, inner) {}

        public static FileReadingException GetReportAsException(string subject, IEnumerable<string> errorMessages)
        {
            return new FileReadingException($"While reading {subject} the following errors occured :{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");
        }
    }
}