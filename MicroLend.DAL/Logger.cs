using System;
using System.IO;

namespace MicroLend.DAL
{
    /// <summary>
    /// Centralized logging utility that writes all exceptions to an external file for debugging and auditing purposes.
    /// Implements NFR3 (Reliability) requirement for maintaining audit trail of errors.
    /// </summary>
    public static class Logger
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static readonly string ErrorLogPath = Path.Combine(LogDirectory, "error_log.txt");
        private static readonly string AuditLogPath = Path.Combine(LogDirectory, "audit_log.txt");
        private static readonly string DebugLogPath = Path.Combine(LogDirectory, "debug_log.txt");
        private static readonly object LockObject = new object();

        static Logger()
        {
            // Create logs directory if it doesn't exist
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        /// <summary>
        /// Logs an error message with exception details to the error log file.
        /// </summary>
        /// <param name="message">The error message describing what happened.</param>
        /// <param name="ex">The exception that occurred.</param>
        public static void LogError(string message, Exception? ex = null)
        {
            try
            {
                lock (LockObject)
                {
                    using (var writer = new StreamWriter(ErrorLogPath, true))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}");
                        
                        if (ex != null)
                        {
                            writer.WriteLine($"Stack Trace: {ex.StackTrace}");
                            writer.WriteLine($"Exception Type: {ex.GetType().Name}");
                            writer.WriteLine($"Message: {ex.Message}");
                            
                            // Log inner exception if exists
                            if (ex.InnerException != null)
                            {
                                writer.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                            }
                        }
                        
                        writer.WriteLine(new string('-', 80));
                    }
                }
            }
            catch
            {
                // Silent fail - cannot log errors if logging itself fails
                // In production, this would write to Windows Event Log
            }
        }

        /// <summary>
        /// Logs a warning message to the error log file.
        /// </summary>
        /// <param name="message">The warning message.</param>
        public static void LogWarning(string message)
        {
            try
            {
                lock (LockObject)
                {
                    using (var writer = new StreamWriter(ErrorLogPath, true))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WARNING: {message}");
                        writer.WriteLine(new string('-', 80));
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }

        /// <summary>
        /// Logs an audit entry for critical operations (loan approvals, user changes).
        /// </summary>
        /// <param name="action">The action being performed (e.g., "LoanApproval", "UserUpdate").</param>
        /// <param name="user">The user performing the action.</param>
        /// <param name="details">Additional details about the operation.</param>
        public static void LogAudit(string action, string user, string details)
        {
            try
            {
                lock (LockObject)
                {
                    using (var writer = new StreamWriter(AuditLogPath, true))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] USER: {user} | ACTION: {action} | DETAILS: {details}");
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }

        /// <summary>
        /// Logs debug information (development only).
        /// </summary>
        /// <param name="message">The debug message.</param>
        public static void LogDebug(string message)
        {
            try
            {
                lock (LockObject)
                {
                    using (var writer = new StreamWriter(DebugLogPath, true))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DEBUG: {message}");
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }

        /// <summary>
        /// Logs an informational message to the debug log file.
        /// </summary>
        /// <param name="message">The informational message.</param>
        public static void LogInfo(string message)
        {
            try
            {
                lock (LockObject)
                {
                    using (var writer = new StreamWriter(DebugLogPath, true))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}");
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }
    }
}