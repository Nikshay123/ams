namespace WebApp.Common.Constants
{
    public static class AppConstants
    {
        public const string ApplicationJson = "application/Json";

        public const string EmailApiEndPoint = "EmailApiEndPoint";
        public const string SmsApiEndPoint = "SmsApiEndPoint";
        public const string USCode = "+1";
        public const string DateFormat_YYYYMMDD = "yyyy-MM-dd";
        public const long MBInBytes = 1024 * 1024;
        public const long FileSize = 20 * MBInBytes;
        public const int FileLimit = 5;
        public const int DbUnqiueConstraintViolationErrorNumber = 2627;
    }
}