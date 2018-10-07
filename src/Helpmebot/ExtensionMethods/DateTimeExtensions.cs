namespace Helpmebot.ExtensionMethods
{
    using System;

    public static class DateTimeExtensions
    {
        /// <summary>
        /// Calculates the duration since the provided date and now
        /// </summary>
        /// <param name="since">The date to calculate since</param>
        /// <param name="years">The number of complete years since the date provided</param>
        /// <param name="age">The timespan excluding complete years</param>
        public static void CalculateDuration(this DateTime since, out int years, out TimeSpan age)
        {
            var now = DateTime.Now;

            var backupVar = since;
            var calcVar = since;
            years = -1;
            while (calcVar < now)
            {
                years++;
                backupVar = calcVar;
                calcVar = calcVar.AddYears(1);
            }

            age = now - backupVar;
        }
    }
}