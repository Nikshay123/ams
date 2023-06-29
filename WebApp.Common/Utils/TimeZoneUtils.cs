using System;
using TimeZoneConverter;

namespace WebApp.Common.Utils
{
    public class TimeZoneUtils
    {
        private const int DefaultMSTTimeZoneOffset = -360;

        public static DateTime GetDifference(DateTime date, int timeZone)
        {
            return date.AddMinutes(timeZone);
        }

        public static int GetTimezoneOffsetMinutes(string tzStandardName)
        {
            if (!string.IsNullOrEmpty(tzStandardName))
            {
                int offset;
                if (int.TryParse(tzStandardName, out offset))
                {
                    return offset;
                }

                try
                {
                    //Attempt to convert to specific target timezone to handle DST conversion
                    var tzi = TZConvert.GetTimeZoneInfo(tzStandardName);
                    var utcTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                    DateTime targetTime = TimeZoneInfo.ConvertTime(utcTime, tzi);

                    return (int)targetTime.Subtract(utcTime).TotalMinutes;
                }
                catch
                {
                    //ignore invalid tz name and just return default
                }
            }

            //use MST as default if not set
            return DefaultMSTTimeZoneOffset;
        }

        public static DateTime GetTimeZoneStartOfDay(int offset)
        {
            var utc = DateTime.UtcNow;
            var utcNoSec = new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, utc.Kind);
            var localTime = utcNoSec.AddMinutes(Convert.ToDouble(offset));
            var locatStartOfDay = utcNoSec.AddMinutes(-localTime.TimeOfDay.TotalMinutes);

            //ensure exactly on 1/2 hour increment
            if (locatStartOfDay.Minute < 15)
            {
                locatStartOfDay.AddMinutes(-localTime.Minute);
            }
            else if (locatStartOfDay.Minute < 45)
            {
                locatStartOfDay.AddMinutes(-localTime.Minute + 30);
            }
            else if (locatStartOfDay.Minute < 60)
            {
                locatStartOfDay.AddMinutes(-localTime.Minute + 60);
            }

            return locatStartOfDay;
        }

        public static DateTime GetStartOfDay(DateTime date, int offset)
        {
            return date.Date.AddMinutes(-offset);
        }

        public static int GetDateDifference(DateTime startDate, DateTime endDate)
        {
            TimeSpan dateDiff = startDate.Subtract(endDate);

            var totalDays = (int)dateDiff.TotalDays;

            return totalDays;
        }
    }
}