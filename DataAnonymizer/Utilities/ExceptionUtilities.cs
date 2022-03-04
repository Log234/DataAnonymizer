using System;
using System.Collections.Generic;

namespace DataAnonymizer.Utilities
{
    internal static class ExceptionUtilities
    {
        internal static string AggregateMessages(Exception exception, string separator = " -> ")
        {
            var messages = new List<string>();
            var currentException = exception;

            while (currentException is not null)
            {
                messages.Add(currentException.Message);
                currentException = currentException.InnerException;
            }

            return string.Join(separator, messages);
        }
    }
}