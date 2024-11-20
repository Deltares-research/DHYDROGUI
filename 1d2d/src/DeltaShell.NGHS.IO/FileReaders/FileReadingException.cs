using System;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public class FileReadingException : Exception
    {
        public FileReadingException(string message)
            : base(message)
        {
        }

        public FileReadingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}