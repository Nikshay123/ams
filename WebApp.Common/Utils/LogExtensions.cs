using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace WebApp.Common.Utils
{
    public static class LogExtensions
    {
        #region Extension Methods

        #region Error

        public static void LogErrorExt(this ILogger logger, Exception exception, string methodName, Dictionary<string, string> methodParams, string details)
        {
            string msg = GetLogMessage(methodName, methodParams, details);
            logger.LogError(exception, msg);
        }

        public static void LogErrorExt(this ILogger logger, string methodName, Dictionary<string, string> methodParams, string details)
        {
            string msg = GetLogMessage(methodName, methodParams, details);
            logger.LogError(msg);
        }

        #endregion Error

        #region Information

        public static void LogInformationExt(this ILogger logger, Exception exception, string methodName, Dictionary<string, string> methodParams, string details)
        {
            string msg = GetLogMessage(methodName, methodParams, details);
            logger.LogInformation(exception, msg);
        }

        public static void LogInformationExt(this ILogger logger, string methodName, Dictionary<string, string> methodParams, string details)
        {
            string msg = GetLogMessage(methodName, methodParams, details);
            logger.LogInformation(msg);
        }

        #endregion Information

        #region Warning

        public static void LogWarningExt(this ILogger logger, Exception exception, string methodName, Dictionary<string, string> methodParams, string details)
        {
            string msg = GetLogMessage(methodName, methodParams, details);
            logger.LogWarning(exception, msg);
        }

        public static void LogWarningExt(this ILogger logger, string methodName, Dictionary<string, string> methodParams, string details)
        {
            string msg = GetLogMessage(methodName, methodParams, details);
            logger.LogWarning(msg);
        }

        #endregion Warning

        #endregion Extension Methods

        #region Public Methods

        public static string GetLogMessage(string methodName, Dictionary<string, string> methodParams, string details)
        {
            string methodParam = methodParams.ConvertToString();
            string msg = $"{methodName}({methodParam}): {details}";

            return msg;
        }

        public static string GetNullLog(this string paramName)
        {
            return $"{paramName} was null.";
        }

        public static string GetInvalidIntLog(this string paramName)
        {
            return $"{paramName} int was null or less than 0";
        }

        public static string GetInvalidDecimalLog(this string paramName)
        {
            return $"{paramName} decimal was null or less than 0";
        }

        public static string GetInvalidDateTimeLog(this string paramName)
        {
            return $"{paramName} date was out of range.";
        }

        public static string GetInvalidStringLog(this string paramName)
        {
            return $"{paramName} string was null or empty.";
        }

        public static string GetNullOrEmptyLog(this string paramName)
        {
            return $"{paramName} was null or empty.";
        }

        public static string GetAddFailureLog(this string paramName)
        {
            return $"Unable to add data for {paramName}.";
        }

        public static string GetUpdateFailureLog(this string paramName)
        {
            return $"Unable to update data for {paramName}.";
        }

        public static string GetDeleteFailureLog(this string paramName)
        {
            return $"Unable to delete data for {paramName}.";
        }

        public static string GetTransactionFailureLog(this string paramName)
        {
            return $"Transaction failure occurred for {paramName}.";
        }

        public static string GetInvalidEnumLog(this string enumKey, string enumVal)
        {
            return $"{enumVal} was null or not found for {enumKey}.";
        }

        public static string GetDuplicateStringLog(this string paramName)
        {
            return $"{paramName} already exists.";
        }

        public static string GetGuidNotFoundLog(this string paramName)
        {
            return $"Record not found for given {paramName}.";
        }

        #endregion Public Methods
    }
}