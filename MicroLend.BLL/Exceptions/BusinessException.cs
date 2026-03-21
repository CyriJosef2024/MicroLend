using System;

namespace MicroLend.BLL.Exceptions
{
    /// <summary>
    /// Custom exception for Business Logic Layer (BLL) errors.
    /// Represents business rule violations and validation errors with
    /// user-friendly messages for display in the presentation layer.
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException() : base() { }

        public BusinessException(string message) : base(message) { }

        public BusinessException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}