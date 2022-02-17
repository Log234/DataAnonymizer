using System;
using System.Runtime.Serialization;

namespace DataAnonymizer.Exceptions
{
    internal class CsvException : Exception
    {
        public CsvException()
        {
        }

        public CsvException(string message) : base(message)
        {
        }

        public CsvException(string message, Exception? innerException) : base(message, innerException)
        {
        }

        protected CsvException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}