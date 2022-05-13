﻿using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.GraphSync.Exceptions
{
    [Serializable]
    public class CommandValidationException : Exception
    {
        public CommandValidationException()
        {
        }

        public CommandValidationException(string? message)
            : base(message)
        {
        }

        public CommandValidationException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected CommandValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
