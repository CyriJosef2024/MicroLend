using System;

namespace MicroLend.BLL.Exceptions
{
    /// <summary>
    /// Custom exception for Data Access Layer (DAL) errors.
    /// Wraps database-specific exceptions with user-friendly messages while
    /// maintaining detailed error information for debugging.
    /// </summary>
    public class DataAccessException : Exception
    {
        public DataAccessException() : base() { }

        public DataAccessException(string message) : base(message) { }

        public DataAccessException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}