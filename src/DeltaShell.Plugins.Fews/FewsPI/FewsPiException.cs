using System;
using System.Runtime.Serialization;

namespace Deltares.IO.FewsPI
{
    [Serializable]
    public class FewsPiException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public FewsPiException()
        {
        }

        public FewsPiException(string message) : base(message)
        {
        }

        public FewsPiException(string message, Exception inner) : base(message, inner)
        {
        }

        protected FewsPiException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}