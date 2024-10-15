﻿// ReSharper disable once CheckNamespace
namespace System.Diagnostics
{
    /// <summary>
    /// Exception thrown when the program executes an instruction that was thought to be unreachable.
    /// </summary>
    public sealed class UnreachableException : Exception
    {
        private const string defaultMsg = "The program executed an instruction that was thought to be unreachable.";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnreachableException"/> class with the default error message.
        /// </summary>
        public UnreachableException() : base(defaultMsg) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnreachableException"/>
        /// class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public UnreachableException(string? message) : base(message ?? defaultMsg) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnreachableException"/>
        /// class with a specified error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UnreachableException(string? message, Exception? innerException) : base(message ?? defaultMsg, innerException) { }

    }
}
