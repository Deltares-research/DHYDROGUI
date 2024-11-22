using System;

namespace DeltaShell.NGHS.IO.FileWriters
{
    public class FileWritingException : Exception
    {
        public FileWritingException()
        {
        }

        public FileWritingException(string message)
            : base(message)
        {
        }

        public FileWritingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}