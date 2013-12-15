using System;

namespace Helpmebot.ExtensionMethods
{
    public static class DateExtensions
    {
        public static string ToInternetFormat(this DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}
